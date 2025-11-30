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
                // Kiểm tra user hợp lệ
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || !userId.HasValue)
                {
                    _logger.LogWarning("Unauthorized attempt to save UserHomeImages");
                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                // Kiểm tra HomeImageIds tồn tại
                var homeImageIds = command.Requests.Select(r => r.HomeImageId).ToList();

                ////////////////////////////////////////////////////
                foreach (var id in homeImageIds)
                {
                    var exists = await _unitOfWork.Repository<Domain.Entities.HomeImage>()
                        .AnyAsync(x => x.Id == id && !x.IsDeleted);

                    if (!exists)
                    {
                        return Result.Failure("One or more HomeImages not found", ErrorCodeEnum.NotFound);
                    }
                }
                ////////////////////////////////////////////////////


                // Lấy danh sách UserHomeImage hiện tại của user
                var currentImages = await _unitOfWork.Repository<Domain.Entities.UserHomeImage>()
    .GetQueryable()
    .Where(x => x.UserId == userId.Value && !x.IsDeleted)
    .AsTracking()
    .ToListAsync(ct);


                await _unitOfWork.BeginTransactionAsync(ct);

                // Xóa các ảnh không còn trong danh sách mới
                var newHomeImageIds = command.Requests.Select(r => r.HomeImageId).ToList();
                var imagesToDelete = currentImages.Where(img => !newHomeImageIds.Contains(img.HomeImageId)).ToList();
                foreach (var img in imagesToDelete)
                {
                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Delete,
                        EntityId = img.Id,
                        EntityName = nameof(UserHomeImage),
                        OldValue = JsonSerializer.Serialize(new { img.HomeImageId, img.Position }),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Removed home image from position {img.Position}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    _unitOfWork.Repository<Domain.Entities.UserHomeImage>().Delete(img);
                }

                // Cập nhật hoặc tạo mới
                foreach (var request in command.Requests.OrderBy(r => r.Position))
                {
                    var existingImage = currentImages.FirstOrDefault(img => img.HomeImageId == request.HomeImageId && !img.IsDeleted);
                    if (existingImage != null)
                    {
                        // Cập nhật vị trí nếu thay đổi
                        if (existingImage.Position != request.Position)
                        {
                            var oldValue = JsonSerializer.Serialize(new { existingImage.HomeImageId, existingImage.Position });
                            existingImage.Position = request.Position;
                            existingImage.UpdatedAt = DateTime.UtcNow;
                            existingImage.UpdatedBy = userId;
                            _unitOfWork.Repository<Domain.Entities.UserHomeImage>().Update(existingImage);

                            var userAction = new UserAction
                            {
                                UserId = userId.Value,
                                Action = UserActionEnum.Update,
                                EntityId = existingImage.Id,
                                EntityName = nameof(UserHomeImage),
                                OldValue = oldValue,
                                NewValue = JsonSerializer.Serialize(new { existingImage.HomeImageId, existingImage.Position }),
                                IPAddress = _currentUserService.IPAddress ?? "Unknown",
                                ActionDetail = $"Updated home image position to {request.Position}",
                                CreatedAt = DateTime.UtcNow,
                                Status = EntityStatusEnum.Active
                            };
                            await _unitOfWork.Repository<UserAction>().AddAsync(userAction);
                        }
                    }
                    else
                    {
                        // Tạo mới
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

                        var userAction = new UserAction
                        {
                            UserId = userId.Value,
                            Action = UserActionEnum.Create,
                            EntityId = newImage.Id,
                            EntityName = nameof(UserHomeImage),
                            NewValue = JsonSerializer.Serialize(new { newImage.UserId, newImage.HomeImageId, newImage.Position }),
                            IPAddress = _currentUserService.IPAddress ?? "Unknown",
                            ActionDetail = $"Added home image (HomeImageId: {request.HomeImageId}) to position {request.Position}",
                            CreatedAt = DateTime.UtcNow,
                            Status = EntityStatusEnum.Active
                        };
                        await _unitOfWork.Repository<UserAction>().AddAsync(userAction);
                    }
                }

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