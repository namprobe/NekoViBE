using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.EventProduct.Commands.UpdateEventProduct
{
    // Command nhận EventId để biết cập nhật cho Event nào
    

    public class UpdateEventProductCommandHandler : IRequestHandler<UpdateEventProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateEventProductCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public UpdateEventProductCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdateEventProductCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(UpdateEventProductCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var request = command.Request;

                // Ensure ID matches
                if (command.EventId != request.EventId)
                    return Result.Failure("Event ID mismatch", ErrorCodeEnum.InvalidInput);

                // 1. Validate User
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);

                // 2. Kiểm tra Event tồn tại (Read-Only)
                var eventExists = await _unitOfWork.Repository<Domain.Entities.Event>().GetQueryable()
                    .AnyAsync(x => x.Id == request.EventId && !x.IsDeleted, cancellationToken);

                if (!eventExists)
                    return Result.Failure("Event not found", ErrorCodeEnum.NotFound);

                // 3. Lấy danh sách EventProduct CŨ (Read-Only & AsNoTracking)
                // Dùng AsNoTracking để GenericRepository.DeleteRange xử lý attach/detach sạch sẽ
                var existingItems = await _unitOfWork.Repository<Domain.Entities.EventProduct>().GetQueryable()
                    .AsNoTracking()
                    .Where(x => x.EventId == request.EventId)
                    .ToListAsync(cancellationToken);

                // 4. Chuẩn bị danh sách EventProduct MỚI
                var newEntities = new List<Domain.Entities.EventProduct>();
                if (request.Products != null && request.Products.Any())
                {
                    foreach (var item in request.Products)
                    {
                        newEntities.Add(new Domain.Entities.EventProduct
                        {
                            Id = Guid.NewGuid(),
                            EventId = request.EventId,
                            ProductId = item.ProductId,
                            IsFeatured = item.IsFeatured,
                            DiscountPercentage = item.DiscountPercentage,
                            CreatedBy = userId,
                            CreatedAt = DateTime.UtcNow,
                            Status = EntityStatusEnum.Active
                        });
                    }
                }

                // 5. Transaction (Write Only)
                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);

                    // A. Xóa tất cả cái cũ (DeleteRange)
                    if (existingItems.Any())
                    {
                        _unitOfWork.Repository<Domain.Entities.EventProduct>().DeleteRange(existingItems);
                    }

                    // B. Thêm danh sách mới (AddRange)
                    if (newEntities.Any())
                    {
                        await _unitOfWork.Repository<Domain.Entities.EventProduct>().AddRangeAsync(newEntities);
                    }

                    // C. Audit Log
                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = request.EventId,
                        EntityName = "EventProduct",
                        NewValue = JsonSerializer.Serialize(request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Updated products for Event {request.EventId}. Deleted {existingItems.Count}, Added {newEntities.Count}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    // D. Commit
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "DB Error in UpdateEventProduct");
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }

                return Result.Success("Event products updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event products");
                return Result.Failure("Error updating event products", ErrorCodeEnum.InternalError);
            }
        }
    }
}