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

namespace NekoViBE.Application.Features.EventProduct.Commands.UpdateEventProductList
{
    public class UpdateEventProductListCommandHandler : IRequestHandler<UpdateEventProductListCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateEventProductListCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public UpdateEventProductListCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdateEventProductListCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(UpdateEventProductListCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validate User
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || !userId.HasValue)
                {
                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                // 2. Validate Event tồn tại (Read-Only)
                // Dùng AsNoTracking() để tránh track vào context
                var eventExists = await _unitOfWork.Repository<Domain.Entities.Event>().GetQueryable()
                    .AnyAsync(x => x.Id == command.EventId && !x.IsDeleted, cancellationToken);

                if (!eventExists)
                    return Result.Failure($"Event with ID {command.EventId} not found", ErrorCodeEnum.NotFound);

                // ---------------------------------------------------------
                // 3. [FIX] CHUYỂN TOÀN BỘ LOGIC ĐỌC DỮ LIỆU LÊN TRƯỚC TRANSACTION
                // ---------------------------------------------------------

                // 3.1 Lấy danh sách EventProduct CŨ (Read-Only & AsNoTracking)
                var existingLinks = await _unitOfWork.Repository<Domain.Entities.EventProduct>().GetQueryable()
                    .Where(x => x.EventId == command.EventId)
                    .ToListAsync(cancellationToken);

                // 3.2 Lấy thông tin các Product có trong danh sách MỚI (để tính giá)
                var newProductIds = command.Products.Select(p => p.ProductId).Distinct().ToList();
                var productsDict = new Dictionary<Guid, Domain.Entities.Product>();

                if (newProductIds.Any())
                {
                    // Lấy Product ra và AsNoTracking để lát nữa UpdateRange hoạt động sạch sẽ
                    var products = await _unitOfWork.Repository<Domain.Entities.Product>().GetQueryable()
                        .Where(p => newProductIds.Contains(p.Id) && !p.IsDeleted)
                        .ToListAsync(cancellationToken);

                    productsDict = products.ToDictionary(p => p.Id, p => p);

                    if (products.Count != newProductIds.Count)
                    {
                        return Result.Failure("Some products in the list do not exist or are deleted", ErrorCodeEnum.NotFound);
                    }
                }
                // ---------------------------------------------------------

                // 4. Bắt đầu Transaction (Chỉ chứa thao tác GHI)
                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);

                    // A. Xóa toàn bộ liên kết CŨ (DeleteRange)
                    if (existingLinks.Any())
                    {
                        _unitOfWork.Repository<Domain.Entities.EventProduct>().DeleteRange(existingLinks);
                    }

                    // B. Tạo danh sách MỚI & Cập nhật giá Product
                    var newLinks = new List<Domain.Entities.EventProduct>();
                    var productsToUpdate = new List<Domain.Entities.Product>();

                    foreach (var item in command.Products)
                    {
                        if (productsDict.TryGetValue(item.ProductId, out var product))
                        {
                            // Tạo EventProduct
                            var newLink = new Domain.Entities.EventProduct
                            {
                                Id = Guid.NewGuid(),
                                EventId = command.EventId,
                                ProductId = item.ProductId,
                                IsFeatured = item.IsFeatured,
                                DiscountPercentage = item.DiscountPercentage,
                                CreatedBy = userId,
                                CreatedAt = DateTime.UtcNow,
                                Status = EntityStatusEnum.Active
                            };
                            newLinks.Add(newLink);

                            // Cập nhật giá Discount cho Product
                            // Vì 'product' lấy từ AsNoTracking, ta có thể sửa đổi thoải mái
                            decimal discountAmount = product.Price * (item.DiscountPercentage / 100m);
                            product.DiscountPrice = product.Price - discountAmount;

                            if (product.DiscountPrice < 0) product.DiscountPrice = 0;

                            product.UpdatedBy = userId;
                            product.UpdatedAt = DateTime.UtcNow;

                            productsToUpdate.Add(product);
                        }
                    }

                    // Lưu EventProducts
                    if (newLinks.Any())
                    {
                        await _unitOfWork.Repository<Domain.Entities.EventProduct>().AddRangeAsync(newLinks);
                    }

                    // Cập nhật Products (giá mới)
                    if (productsToUpdate.Any())
                    {
                        _unitOfWork.Repository<Domain.Entities.Product>().UpdateRange(productsToUpdate);
                    }

                    // C. Audit Log
                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = command.EventId,
                        EntityName = "EventProduct",
                        NewValue = JsonSerializer.Serialize(new { NewCount = newLinks.Count, DeletedCount = existingLinks.Count }),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Sync EventProducts for Event {command.EventId}: Deleted {existingLinks.Count}, Added {newLinks.Count}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    // D. Commit
                    await _unitOfWork.SaveChangesAsync(cancellationToken); // SaveChanges 1 lần
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch (Exception dbEx)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogError(dbEx, "Database error during EventProduct sync");
                    throw;
                }

                return Result.Success("Event products synchronized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing event products");
                return Result.Failure("Error processing request", ErrorCodeEnum.InternalError);
            }
        }
    }
}