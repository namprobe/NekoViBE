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

namespace NekoViBE.Application.Features.EventProduct.Commands.CreateEventProduct
{
    public class CreateEventProductCommandHandler : IRequestHandler<CreateEventProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateEventProductCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public CreateEventProductCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<CreateEventProductCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(CreateEventProductCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validate User
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);

                var request = command.Request;

                // 2. Validate Event tồn tại (Read-Only)
                var eventExists = await _unitOfWork.Repository<Domain.Entities.Event>().GetQueryable()
                    .AnyAsync(x => x.Id == request.EventId && !x.IsDeleted, cancellationToken);

                if (!eventExists)
                    return Result.Failure("Event not found", ErrorCodeEnum.NotFound);

                // 3. Lấy danh sách Product ID để kiểm tra tồn tại
                if (request.Products == null || !request.Products.Any())
                    return Result.Failure("Product list cannot be empty", ErrorCodeEnum.InvalidInput);

                var productIds = request.Products.Select(x => x.ProductId).Distinct().ToList();

                // Kiểm tra xem tất cả Product có tồn tại không
                var existingProductsCount = await _unitOfWork.Repository<Domain.Entities.Product>().GetQueryable()
                    .CountAsync(x => productIds.Contains(x.Id) && !x.IsDeleted, cancellationToken);

                if (existingProductsCount != productIds.Count)
                    return Result.Failure("One or more products not found or deleted", ErrorCodeEnum.NotFound);

                // 4. Kiểm tra trùng lặp trong Event (Optional: Nếu muốn chặn add trùng)
                // Ở đây ta có thể bỏ qua và chỉ add những cái chưa có, hoặc báo lỗi.
                // Để đơn giản và hiệu suất, ta sẽ lọc những cái đã có ra.
                var existingEventProducts = await _unitOfWork.Repository<Domain.Entities.EventProduct>().GetQueryable()
                    .Where(x => x.EventId == request.EventId && productIds.Contains(x.ProductId))
                    .Select(x => x.ProductId)
                    .ToListAsync(cancellationToken);

                // 5. Chuẩn bị Entities
                var newEntities = new List<Domain.Entities.EventProduct>();
                foreach (var item in request.Products)
                {
                    // Chỉ thêm nếu chưa tồn tại trong Event đó
                    if (!existingEventProducts.Contains(item.ProductId))
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

                if (!newEntities.Any())
                    return Result.Success("No new products to add (all existed).");

                // 6. Transaction (Write Only)
                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);

                    await _unitOfWork.Repository<Domain.Entities.EventProduct>().AddRangeAsync(newEntities);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Create,
                        EntityId = request.EventId, // Gắn vào Event ID vì đây là bulk action
                        EntityName = "EventProduct",
                        NewValue = JsonSerializer.Serialize(request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Added {newEntities.Count} products to Event {request.EventId}",
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

                return Result.Success($"Successfully added {newEntities.Count} products to event.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event products");
                return Result.Failure("Error creating event products", ErrorCodeEnum.InternalError);
            }
        }
    }
}