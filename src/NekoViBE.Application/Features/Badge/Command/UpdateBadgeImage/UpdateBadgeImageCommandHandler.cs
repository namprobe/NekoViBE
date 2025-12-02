using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.Badge.Command.UpdateBadgeImage
{
    public class UpdateBadgeImageCommandHandler : IRequestHandler<UpdateBadgeImageCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateBadgeImageCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public UpdateBadgeImageCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdateBadgeImageCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(UpdateBadgeImageCommand command, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting badge image update for ID: {Id}", command.Id);

                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to update badge image");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.Badge>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);

                if (entity == null)
                {
                    _logger.LogWarning("Badge not found for ID: {Id}", command.Id);
                    return Result.Failure("Badge not found", ErrorCodeEnum.NotFound);
                }

                var oldImagePath = entity.IconPath;

                // Delete old image if exists
                if (!string.IsNullOrEmpty(oldImagePath))
                {
                    await _fileService.DeleteFileAsync(oldImagePath, cancellationToken);
                    _logger.LogInformation("Deleted old badge image: {OldImagePath}", oldImagePath);
                }

                // Upload new image
                var imagePath = await _fileService.UploadFileAsync(command.Request.IconPath, "uploads/badge", cancellationToken);
                entity.IconPath = imagePath;
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("New badge image uploaded to: {ImagePath} for badge {Name}", imagePath, entity.Name);

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    repo.Update(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = entity.Id,
                        EntityName = "Badge",
                        OldValue = JsonSerializer.Serialize(new { IconPath = oldImagePath }),
                        NewValue = JsonSerializer.Serialize(new { IconPath = imagePath }),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Updated badge image for badge: {entity.Name}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    _logger.LogInformation("Badge image updated successfully for badge ID: {Id}, Name: {Name}", entity.Id, entity.Name);
                    return Result.Success("Badge image updated successfully");
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogError(ex, "Error occurred during badge image update transaction for badge ID: {Id}", command.Id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating badge image for badge ID: {Id}", command.Id);
                return Result.Failure("An error occurred while updating the badge image", ErrorCodeEnum.InternalError);
            }
        }
    }
}
