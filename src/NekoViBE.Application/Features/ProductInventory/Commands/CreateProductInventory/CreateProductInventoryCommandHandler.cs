using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;
using System.Transactions; // giữ lại vì bạn yêu cầu dùng TransactionScope

namespace NekoViBE.Application.Features.ProductInventory.Commands.CreateProductInventory
{
    public class CreateProductInventoryCommandHandler : IRequestHandler<CreateProductInventoryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductInventoryCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public CreateProductInventoryCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateProductInventoryCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(CreateProductInventoryCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Unauthorized attempt to create product inventory");
                    return Result.Failure("User is not authenticated", ErrorCodeEnum.Unauthorized);
                }

                // Lấy Product + Include ProductInventories để tránh query 2 lần (và đảm bảo tính chính xác)
                var productRepo = _unitOfWork.Repository<Domain.Entities.Product>();
                var product = await productRepo.GetFirstOrDefaultAsync(
                    p => p.Id == command.Request.ProductId,
                    p => p.ProductInventories); // Include để tính tổng realtime nếu cần

                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", command.Request.ProductId);
                    return Result.Failure("Product not found", ErrorCodeEnum.NotFound);
                }

                // Tính tổng tồn kho hiện tại từ bảng ProductInventory (đảm bảo chính xác 100%)
                var currentTotalInventory = product.ProductInventories?.Sum(pi => pi.Quantity) ?? 0;
                var newTotal = currentTotalInventory + command.Request.Quantity;

                if (newTotal < 0)
                {
                    _logger.LogWarning("Attempt to set negative stock for product {ProductId}. Current: {Current}, Adding: {Adding}",
                        command.Request.ProductId, currentTotalInventory, command.Request.Quantity);
                    return Result.Failure("Stock quantity cannot be negative", ErrorCodeEnum.InvalidOperation);
                }

                // Map và tạo entity ProductInventory
                var inventoryEntity = _mapper.Map<Domain.Entities.ProductInventory>(command.Request);
                inventoryEntity.CreatedBy = userId;
                inventoryEntity.CreatedAt = DateTime.UtcNow;
                inventoryEntity.Status = EntityStatusEnum.Active;

                // Dùng TransactionScope như yêu cầu
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                // 1. Thêm bản ghi inventory mới
                await _unitOfWork.Repository<Domain.Entities.ProductInventory>().AddAsync(inventoryEntity);

                // 2. Cập nhật StockQuantity trên Product (chỉ cập nhật trường này, không cần load toàn bộ)
                product.StockQuantity = newTotal; // Đây chính là chỗ đồng bộ
                productRepo.Update(product); // Đánh dấu Product là Modified

                // 3. Ghi log hành động người dùng
                var userAction = new UserAction
                {
                    UserId = userId.Value,
                    Action = UserActionEnum.Create,
                    EntityId = inventoryEntity.Id,
                    EntityName = nameof(ProductInventory),
                    NewValue = JsonSerializer.Serialize(command.Request),
                    IPAddress = _currentUserService.IPAddress ?? "Unknown",
                    ActionDetail = $"Added {command.Request.Quantity} units to inventory of product ID: {command.Request.ProductId}. New total stock: {newTotal}",
                    CreatedAt = DateTime.UtcNow,
                    Status = EntityStatusEnum.Active
                };

                await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                // Lưu tất cả thay đổi trong 1 lần
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                scope.Complete();

                _logger.LogInformation("Successfully added {Quantity} to product {ProductId} inventory. New stock: {Stock}",
                    command.Request.Quantity, command.Request.ProductId, newTotal);

                return Result.Success("Product inventory created and stock updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product inventory for product ID: {ProductId}", command.Request.ProductId);
                return Result.Failure("An error occurred while creating product inventory", ErrorCodeEnum.InternalError);
            }
        }
    }
}