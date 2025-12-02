using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.ProductInventory;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;
using System.Transactions;

namespace NekoViBE.Application.Features.ProductInventory.Commands.UpdateProductInventory
{
    public class UpdateProductInventoryCommandHandler
        : IRequestHandler<UpdateProductInventoryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateProductInventoryCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProductInventoryCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdateProductInventoryCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(UpdateProductInventoryCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);

                var inventoryRepo = _unitOfWork.Repository<Domain.Entities.ProductInventory>();
                var productRepo = _unitOfWork.Repository<Domain.Entities.Product>();

                var existingInventory = await inventoryRepo.GetFirstOrDefaultAsync(x => x.Id == command.Id);
                if (existingInventory == null)
                    return Result.Failure("Product inventory not found", ErrorCodeEnum.NotFound);

                var oldProductId = existingInventory.ProductId;
                var oldQuantity = existingInventory.Quantity;
                var oldStatus = existingInventory.Status;

                var newProductId = command.Request.ProductId;
                var newQuantity = command.Request.Quantity;

                var oldValueJson = JsonSerializer.Serialize(_mapper.Map<ProductInventoryRequest>(existingInventory));

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var oldProduct = await productRepo.GetFirstOrDefaultAsync(x => x.Id == oldProductId);
                    var newProduct = await productRepo.GetFirstOrDefaultAsync(x => x.Id == newProductId);

                    if (oldProduct == null || newProduct == null)
                        return Result.Failure("Product not found", ErrorCodeEnum.NotFound);

                    // CASE 1: ProductId KHÔNG đổi → chỉ cập nhật số lượng
                    if (oldProductId == newProductId)
                    {
                        int diff = newQuantity - oldQuantity;
                        oldProduct.StockQuantity += diff;

                        if (oldProduct.StockQuantity < 0)
                            return Result.Failure("Stock quantity cannot be negative", ErrorCodeEnum.InvalidOperation);

                        productRepo.Update(oldProduct);
                    }
                    else
                    {
                        // CASE 2: ProductId ĐỔI → chuyển stock
                        oldProduct.StockQuantity -= oldQuantity;
                        if (oldProduct.StockQuantity < 0)
                            return Result.Failure("Stock quantity cannot be negative", ErrorCodeEnum.InvalidOperation);

                        newProduct.StockQuantity += newQuantity;

                        productRepo.Update(oldProduct);
                        productRepo.Update(newProduct);
                    }

                    // Update ProductInventory
                    _mapper.Map(command.Request, existingInventory);
                    existingInventory.UpdatedBy = userId;
                    existingInventory.UpdatedAt = DateTime.UtcNow;
                    inventoryRepo.Update(existingInventory);

                    // Log
                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = existingInventory.Id,
                        EntityName = "ProductInventory",
                        OldValue = oldValueJson,
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Updated product inventory {existingInventory.Id}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };

                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    // Log status
                    if (oldStatus != command.Request.Status)
                    {
                        await _unitOfWork.Repository<UserAction>().AddAsync(new UserAction
                        {
                            UserId = userId.Value,
                            Action = UserActionEnum.StatusChange,
                            EntityId = existingInventory.Id,
                            EntityName = "ProductInventory",
                            OldValue = oldStatus.ToString(),
                            NewValue = command.Request.Status.ToString(),
                            IPAddress = _currentUserService.IPAddress ?? "Unknown",
                            ActionDetail =
                                $"Changed status of product inventory {existingInventory.Id} from {oldStatus} to {command.Request.Status}",
                            CreatedAt = DateTime.UtcNow,
                            Status = EntityStatusEnum.Active
                        });
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    scope.Complete();
                }

                return Result.Success("Product inventory updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product inventory {Id}", command.Id);
                return Result.Failure("Error updating product inventory", ErrorCodeEnum.InternalError);
            }
        }
    }
}
