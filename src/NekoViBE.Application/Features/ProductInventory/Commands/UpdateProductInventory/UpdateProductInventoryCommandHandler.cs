using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
    public class UpdateProductInventoryCommandHandler : IRequestHandler<UpdateProductInventoryCommand, Result>
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
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to update product inventory");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repository = _unitOfWork.Repository<Domain.Entities.ProductInventory>();
                var productRepo = _unitOfWork.Repository<Domain.Entities.Product>();

                var existingInventory = await repository.GetFirstOrDefaultAsync(x => x.Id == command.Id);
                if (existingInventory == null)
                {
                    _logger.LogWarning("Product inventory with ID {Id} not found", command.Id);
                    return Result.Failure("Product inventory not found", ErrorCodeEnum.NotFound);
                }

                var oldProductId = existingInventory.ProductId;
                var oldQuantity = existingInventory.Quantity;
                var oldStatus = existingInventory.Status;
                var oldValue = JsonSerializer.Serialize(_mapper.Map<ProductInventoryRequest>(existingInventory));

                var newProductId = command.Request.ProductId;
                var newQuantity = command.Request.Quantity;

                

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // Validate new ProductId
                    var newProduct = await productRepo.GetFirstOrDefaultAsync(x => x.Id == newProductId);

                    // Update StockQuantity
                   
                        // Subtract old quantity from old product
                        var oldProduct = await productRepo.GetFirstOrDefaultAsync(x => x.Id == oldProductId);
                        
                            oldProduct.StockQuantity -= oldQuantity;
                            if (oldProduct.StockQuantity < 0)
                            {
                                _logger.LogWarning("Stock quantity for product {ProductId} would become negative", oldProductId);
                                return Result.Failure("Stock quantity cannot be negative", ErrorCodeEnum.InvalidOperation);
                            }
                            productRepo.Update(oldProduct);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                        

                        // Add new quantity to new product
                        newProduct.StockQuantity += newQuantity;
                        productRepo.Update(newProduct);
                    

                    // Update inventory
                    _mapper.Map(command.Request, existingInventory);
                    existingInventory.UpdatedBy = userId;
                    existingInventory.UpdatedAt = DateTime.UtcNow;
                    repository.Update(existingInventory);

                    // Log user action
                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = existingInventory.Id,
                        EntityName = "ProductInventory",
                        OldValue = oldValue,
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Updated product inventory for product ID: {newProductId} with quantity: {newQuantity}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    // Log status change if applicable
                    if (oldStatus != command.Request.Status)
                    {
                        var statusChangeAction = new UserAction
                        {
                            UserId = userId.Value,
                            Action = UserActionEnum.StatusChange,
                            EntityId = existingInventory.Id,
                            EntityName = "ProductInventory",
                            OldValue = oldStatus.ToString(),
                            NewValue = command.Request.Status.ToString(),
                            IPAddress = _currentUserService.IPAddress ?? "Unknown",
                            ActionDetail = $"Changed status of product inventory ID: {existingInventory.Id} from {oldStatus} to {command.Request.Status}",
                            CreatedAt = DateTime.UtcNow,
                            Status = EntityStatusEnum.Active
                        };
                        await _unitOfWork.Repository<UserAction>().AddAsync(statusChangeAction);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    scope.Complete();
                }

                return Result.Success("Product inventory updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product inventory with ID: {Id}", command.Id);
                return Result.Failure("Error updating product inventory", ErrorCodeEnum.InternalError);
            }
        }
    }
}
