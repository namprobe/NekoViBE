using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Category.Commands.CreateCategory
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateCategoryCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public CreateCategoryCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateCategoryCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to create category");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var categoryRepo = _unitOfWork.Repository<Domain.Entities.Category>();
                if (command.Request.ParentCategoryId.HasValue &&
                    !await categoryRepo.AnyAsync(x => x.Id == command.Request.ParentCategoryId.Value))
                {
                    _logger.LogWarning("Parent category ID {ParentCategoryId} does not exist", command.Request.ParentCategoryId);
                    return Result.Failure("Parent category does not exist", ErrorCodeEnum.NotFound);
                }

                var entity = _mapper.Map<Domain.Entities.Category>(command.Request);
                entity.CreatedBy = userId;
                entity.CreatedAt = DateTime.UtcNow;
                entity.Status = EntityStatusEnum.Active;

                if (command.Request.ImageFile != null)
                {
                    var imagePath = await _fileService.UploadFileAsync(command.Request.ImageFile, "uploads", cancellationToken);
                    entity.ImagePath = imagePath;
                    _logger.LogInformation("ImagePath set to {ImagePath} for category {Name}", imagePath, entity.Name);
                }
                else
                {
                    _logger.LogWarning("No ImageFile provided for category {Name}", command.Request.Name);
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    await categoryRepo.AddAsync(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Create,
                        EntityId = entity.Id,
                        EntityName = "Category",
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Created category with name: {command.Request.Name}",
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

                return Result.Success("Category created successfully");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error uploading file for category");
                return Result.Failure("Error uploading file", ErrorCodeEnum.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return Result.Failure("Error creating category", ErrorCodeEnum.InternalError);
            }
        }
    }
}