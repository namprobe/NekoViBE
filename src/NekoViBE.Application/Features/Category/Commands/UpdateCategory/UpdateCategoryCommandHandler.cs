using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Category.Commands.UpdateCategory
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateCategoryCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public UpdateCategoryCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdateCategoryCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to update category");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.Category>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);

                if (entity == null)
                    return Result.Failure("Category not found", ErrorCodeEnum.NotFound);

                if (command.Request.ParentCategoryId.HasValue &&
                    !await repo.AnyAsync(x => x.Id == command.Request.ParentCategoryId.Value))
                {
                    _logger.LogWarning("Parent category ID {ParentCategoryId} does not exist", command.Request.ParentCategoryId);
                    return Result.Failure("Parent category does not exist", ErrorCodeEnum.NotFound);
                }

                var oldValue = JsonSerializer.Serialize(_mapper.Map<CategoryRequest>(entity));
                var oldStatus = entity.Status;
                var oldImagePath = entity.ImagePath;

                _mapper.Map(command.Request, entity);
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                if (command.Request.ImageFile != null)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(oldImagePath))
                    {
                        await _fileService.DeleteFileAsync(oldImagePath, cancellationToken);
                    }

                    // Upload new image
                    var imagePath = await _fileService.UploadFileAsync(command.Request.ImageFile, "uploads", cancellationToken);
                    entity.ImagePath = imagePath;
                    _logger.LogInformation("ImagePath updated to {ImagePath} for category {Name}", imagePath, entity.Name);
                } else
                {
                    await _fileService.DeleteFileAsync(oldImagePath, cancellationToken);
                    entity.ImagePath = null;
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    repo.Update(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = entity.Id,
                        EntityName = "Category",
                        OldValue = oldValue,
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Updated category with name: {command.Request.Name}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    if (oldStatus != command.Request.Status)
                    {
                        var statusChangeAction = new UserAction
                        {
                            UserId = userId.Value,
                            Action = UserActionEnum.StatusChange,
                            EntityId = entity.Id,
                            EntityName = "Category",
                            OldValue = oldStatus.ToString(),
                            NewValue = command.Request.Status.ToString(),
                            IPAddress = _currentUserService.IPAddress ?? "Unknown",
                            ActionDetail = $"Changed status of category '{entity.Name}' from {oldStatus} to {command.Request.Status}",
                            CreatedAt = DateTime.UtcNow,
                            Status = EntityStatusEnum.Active
                        };
                        await _unitOfWork.Repository<UserAction>().AddAsync(statusChangeAction);
                    }

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }

                return Result.Success("Category updated successfully");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error handling file for category");
                return Result.Failure("Error handling file", ErrorCodeEnum.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID: {Id}", command.Id);
                return Result.Failure("Error updating category", ErrorCodeEnum.InternalError);
            }
        }
    }
}