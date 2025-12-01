using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.UserHomeImage.Commands.SaveUserHomeImages
{
    public class SaveUserHomeImagesCommandHandler : IRequestHandler<SaveUserHomeImagesCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SaveUserHomeImagesCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public SaveUserHomeImagesCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SaveUserHomeImagesCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(SaveUserHomeImagesCommand command, CancellationToken ct)
        {
            try
            {
                // 1. Validate User
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || !userId.HasValue)
                {
                    _logger.LogWarning("Unauthorized attempt to save UserHomeImages");
                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                // 2. Validate HomeImageIds tồn tại
                // SỬA LỖI: Dùng vòng lặp check từng cái thay vì CountAsync(Contains) để tránh lỗi SQL "WITH"
                var homeImageIds = command.Requests.Select(r => r.HomeImageId).Distinct().ToList();

                foreach (var id in homeImageIds)
                {
                    // Query đơn giản: SELECT TOP 1 1 FROM HomeImage WHERE Id = ...
                    var exists = await _unitOfWork.Repository<Domain.Entities.HomeImage>()
                        .AnyAsync(x => x.Id == id && !x.IsDeleted);

                    if (!exists)
                    {
                        return Result.Failure($"HomeImage with ID {id} not found", ErrorCodeEnum.NotFound);
                    }
                }

                // 3. Bắt đầu xử lý Transaction
                await _unitOfWork.BeginTransactionAsync(ct);

                // Lấy danh sách hiện tại
                var currentImages = await _unitOfWork.Repository<Domain.Entities.UserHomeImage>()
                    .GetQueryable()
                    .Where(x => x.UserId == userId.Value && !x.IsDeleted)
                    .ToListAsync(ct);

                // --- CHIẾN LƯỢC: XÓA CŨ - TẠO MỚI (RESET) ---

                // Bước A: Xóa toàn bộ ảnh hiện có của user
                if (currentImages.Any())
                {
                    foreach (var img in currentImages)
                    {
                        var deleteAction = new UserAction
                        {
                            UserId = userId.Value,
                            Action = UserActionEnum.Delete,
                            EntityId = img.Id,
                            EntityName = nameof(UserHomeImage),
                            OldValue = JsonSerializer.Serialize(new { img.HomeImageId, img.Position }),
                            IPAddress = _currentUserService.IPAddress ?? "Unknown",
                            ActionDetail = $"Reset: Removed home image from position {img.Position}",
                            CreatedAt = DateTime.UtcNow,
                            Status = EntityStatusEnum.Active
                        };
                        await _unitOfWork.Repository<UserAction>().AddAsync(deleteAction);
                    }
                    _unitOfWork.Repository<Domain.Entities.UserHomeImage>().DeleteRange(currentImages);
                }

                // Bước B: Tạo mới lại toàn bộ theo Request
                foreach (var request in command.Requests.OrderBy(r => r.Position))
                {
                    var newImage = new Domain.Entities.UserHomeImage
                    {
                        UserId = userId.Value,
                        HomeImageId = request.HomeImageId,
                        Position = request.Position,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };

                    await _unitOfWork.Repository<Domain.Entities.UserHomeImage>().AddAsync(newImage);

                    var createAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Create,
                        EntityId = newImage.Id,
                        EntityName = nameof(UserHomeImage),
                        NewValue = JsonSerializer.Serialize(new { newImage.UserId, newImage.HomeImageId, newImage.Position }),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Reset: Set home image (HomeImageId: {request.HomeImageId}) to position {request.Position}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(createAction);
                }

                // 4. Commit transaction
                await _unitOfWork.CommitTransactionAsync(ct);

                return Result.Success("User home images saved successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                _logger.LogError(ex, "Error saving UserHomeImages");
                return Result.Failure("Error saving home images", ErrorCodeEnum.InternalError);
            }
        }
    }
}