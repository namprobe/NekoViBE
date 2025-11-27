using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.HomeImage.Commands.UpdateHomeImage
{
    public class UpdateHomeImageCommandHandler : IRequestHandler<UpdateHomeImageCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateHomeImageCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public UpdateHomeImageCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdateHomeImageCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(UpdateHomeImageCommand command, CancellationToken ct)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);

                var repo = _unitOfWork.Repository<Domain.Entities.HomeImage>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);
                if (entity == null)
                    return Result.Failure("Home image not found", ErrorCodeEnum.NotFound);

                // Kiểm tra AnimeSeriesId hợp lệ (nếu có)
                if (!string.IsNullOrEmpty(command.Request.AnimeSeriesId))
                {
                    if (!Guid.TryParse(command.Request.AnimeSeriesId, out var animeId))
                    {
                        return Result.Failure("AnimeSeriesId must be a valid GUID", ErrorCodeEnum.ValidationFailed);
                    }

                    var exists = await _unitOfWork.Repository<Domain.Entities.AnimeSeries>()
                        .AnyAsync(x => x.Id == animeId);

                    if (!exists)
                    {
                        return Result.Failure("Anime series not found", ErrorCodeEnum.NotFound);
                    }
                }

                var oldImagePath = entity.ImagePath;
                var oldValue = JsonSerializer.Serialize(new { entity.Name, entity.ImagePath, entity.AnimeSeriesId });

                // Chỉ upload nếu có file mới
                if (command.Request.ImageFile != null)
                {
                    var newPath = await _fileService.UploadFileAsync(command.Request.ImageFile, "home-images", ct);
                    entity.ImagePath = newPath;

                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(oldImagePath))
                        await _fileService.DeleteFileAsync(oldImagePath, ct);
                }
                else if (!string.IsNullOrEmpty(command.Request.ExistingImagePath))
                {
                    // Người dùng không đổi ảnh → giữ nguyên
                    entity.ImagePath = oldImagePath;
                }
                // Nếu cả 2 đều null → lỗi (không được phép xóa ảnh)

                entity.Name = command.Request.Name.Trim();
                entity.AnimeSeriesId = string.IsNullOrEmpty(command.Request.AnimeSeriesId)
                    ? null
                    : Guid.Parse(command.Request.AnimeSeriesId);
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.BeginTransactionAsync(ct);
                repo.Update(entity);

                var userAction = new UserAction
                {
                    UserId = userId.Value,
                    Action = UserActionEnum.Update,
                    EntityId = entity.Id,
                    EntityName = "HomeImage",
                    OldValue = oldValue,
                    NewValue = JsonSerializer.Serialize(new { entity.Name, entity.ImagePath, entity.AnimeSeriesId }),
                    IPAddress = _currentUserService.IPAddress ?? "Unknown",
                    ActionDetail = "Updated home image",
                    CreatedAt = DateTime.UtcNow,
                    Status = EntityStatusEnum.Active
                };
                await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                await _unitOfWork.CommitTransactionAsync(ct);
                return Result.Success("Home image updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating home image {Id}", command.Id);
                return Result.Failure("Error updating home image", ErrorCodeEnum.InternalError);
            }
        }
    }
}