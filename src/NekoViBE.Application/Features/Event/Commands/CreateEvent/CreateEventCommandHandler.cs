using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Event.Commands.CreateEvent
{
    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateEventCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public CreateEventCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateEventCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(CreateEventCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to create event");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var titleExists = await _unitOfWork.Repository<Domain.Entities.Event>()
                    .AnyAsync(x => x.Name == command.Request.Name && !x.IsDeleted &&
                        x.StartDate <= command.Request.EndDate && command.Request.StartDate <= x.EndDate);

                if (titleExists)
                    return Result.Failure("Event with this name already exists", ErrorCodeEnum.ResourceConflict);

                var entity = _mapper.Map<Domain.Entities.Event>(command.Request);
                entity.CreatedBy = userId;
                entity.CreatedAt = DateTime.UtcNow;
                entity.Status = command.Request.Status;

                if (command.Request.ImageFile != null)
                {
                    var imagePath = await _fileService.UploadFileAsync(command.Request.ImageFile, "uploads/events", cancellationToken);
                    entity.ImagePath = imagePath;
                    _logger.LogInformation("ImagePath set to {ImagePath} for event {Name}", imagePath, entity.Name);
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    await _unitOfWork.Repository<Domain.Entities.Event>().AddAsync(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Create,
                        EntityId = entity.Id,
                        EntityName = "Event",
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Created event with name: {command.Request.Name}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }

                return Result.Success("Event created successfully");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error uploading file for event");
                return Result.Failure("Error uploading file", ErrorCodeEnum.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                return Result.Failure("Error creating event", ErrorCodeEnum.InternalError);
            }
        }
    }
}
