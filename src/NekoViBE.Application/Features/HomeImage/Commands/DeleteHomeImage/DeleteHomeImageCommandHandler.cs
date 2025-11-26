// DeleteHomeImageCommandHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.HomeImage.Commands.DeleteHomeImage
{
    public class DeleteHomeImageCommandHandler : IRequestHandler<DeleteHomeImageCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteHomeImageCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public DeleteHomeImageCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteHomeImageCommandHandler> logger,
            ICurrentUserService currentUserService, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(DeleteHomeImageCommand command, CancellationToken ct)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);

                var repo = _unitOfWork.Repository<Domain.Entities.HomeImage>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);
                if (entity == null)
                    return Result.Failure("Home image not found", ErrorCodeEnum.NotFound);

                // Kiểm tra có user nào đang chọn làm ảnh home không
                var isInUse = await _unitOfWork.Repository<UserHomeImage>()
                    .AnyAsync(x => x.HomeImageId == command.Id);

                if (isInUse)
                    return Result.Failure("Cannot delete home image that is being used by users", ErrorCodeEnum.ResourceConflict);

                var oldImagePath = entity.ImagePath;

                entity.IsDeleted = true;
                entity.DeletedBy = userId;
                entity.DeletedAt = DateTime.UtcNow;

                await _unitOfWork.BeginTransactionAsync(ct);
                repo.Update(entity);

                if (!string.IsNullOrEmpty(oldImagePath))
                    await _fileService.DeleteFileAsync(oldImagePath, ct);

                var userAction = new UserAction
                {
                    UserId = userId.Value,
                    Action = UserActionEnum.Delete,
                    EntityId = entity.Id,
                    EntityName = "HomeImage",
                    OldValue = JsonSerializer.Serialize(new { entity.ImagePath }),
                    IPAddress = _currentUserService.IPAddress ?? "Unknown",
                    ActionDetail = "Deleted home image",
                    CreatedAt = DateTime.UtcNow,
                    Status = EntityStatusEnum.Active
                };
                await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                await _unitOfWork.CommitTransactionAsync(ct);
                return Result.Success("Home image deleted successfully");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error deleting file for home image {Id}", command.Id);
                return Result.Failure("Error deleting image file", ErrorCodeEnum.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting home image {Id}", command.Id);
                return Result.Failure("Error deleting home image", ErrorCodeEnum.InternalError);
            }
        }
    }
}