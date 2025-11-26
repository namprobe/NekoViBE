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

namespace NekoViBE.Application.Features.HomeImage.Commands.CreateHomeImage
{
    public class CreateHomeImageCommandHandler : IRequestHandler<CreateHomeImageCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateHomeImageCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public CreateHomeImageCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateHomeImageCommandHandler> logger,
            ICurrentUserService currentUserService, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(CreateHomeImageCommand command, CancellationToken ct)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);

                if (command.Request.AnimeSeriesId.HasValue)
                {
                    var exists = await _unitOfWork.Repository<Domain.Entities.AnimeSeries>().AnyAsync(x => x.Id == command.Request.AnimeSeriesId.Value);
                    if (!exists)
                        return Result.Failure("Anime series not found", ErrorCodeEnum.NotFound);
                }

                var imagePath = await _fileService.UploadFileAsync(command.Request.ImageFile, "home-images", ct);

                var entity = new Domain.Entities.HomeImage
                {
                    ImagePath = imagePath,
                    AnimeSeriesId = command.Request.AnimeSeriesId,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.BeginTransactionAsync(ct);
                await _unitOfWork.Repository<Domain.Entities.HomeImage>().AddAsync(entity);

                var userAction = new UserAction
                {
                    UserId = userId.Value,
                    Action = UserActionEnum.Create,
                    EntityId = entity.Id,
                    EntityName = "HomeImage",
                    NewValue = JsonSerializer.Serialize(new { entity.ImagePath, entity.AnimeSeriesId }),
                    IPAddress = _currentUserService.IPAddress ?? "Unknown",
                    ActionDetail = "Created new home image",
                    CreatedAt = DateTime.UtcNow,
                    Status = EntityStatusEnum.Active
                };
                await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                await _unitOfWork.CommitTransactionAsync(ct);
                return Result.Success("Home image created successfully");
            }
            catch (Exception ex) when (ex is IOException or not null)
            {
                _logger.LogError(ex, "Error creating home image");
                return Result.Failure("Error uploading image", ErrorCodeEnum.InternalError);
            }
        }
    }
}
