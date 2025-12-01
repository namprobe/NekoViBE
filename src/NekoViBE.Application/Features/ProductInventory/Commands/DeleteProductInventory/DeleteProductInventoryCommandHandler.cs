using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;
using System.Transactions;

namespace NekoViBE.Application.Features.ProductInventory.Commands.DeleteProductInventory
{
    public class DeleteProductInventoryCommandHandler
        : IRequestHandler<DeleteProductInventoryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteProductInventoryCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public DeleteProductInventoryCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteProductInventoryCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(DeleteProductInventoryCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);

                var repo = _unitOfWork.Repository<NekoViBE.Domain.Entities.ProductInventory>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);

                if (entity == null)
                    return Result.Failure("Product inventory not found", ErrorCodeEnum.NotFound);

                var productRepo = _unitOfWork.Repository<Domain.Entities.Product>();
                var product = await productRepo.GetFirstOrDefaultAsync(x => x.Id == entity.ProductId);

                if (product == null)
                    return Result.Failure("Product not found", ErrorCodeEnum.NotFound);

                // Check âm stock
                if (product.StockQuantity - entity.Quantity < 0)
                    return Result.Failure("Stock quantity cannot be negative", ErrorCodeEnum.InvalidOperation);

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // 1) Cập nhật stock
                    product.StockQuantity -= entity.Quantity;
                    productRepo.Update(product);

                    // 2) Soft-delete ProductInventory
                    entity.IsDeleted = true;
                    entity.DeletedBy = userId;
                    entity.DeletedAt = DateTime.UtcNow;
                    entity.Status = EntityStatusEnum.Inactive;
                    repo.Update(entity);

                    // 3) Log hành động
                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Delete,
                        EntityId = entity.Id,
                        EntityName = "ProductInventory",
                        OldValue = JsonSerializer.Serialize(new
                        {
                            entity.ProductId,
                            entity.Quantity
                        }),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Deleted product inventory ID {entity.Id}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    scope.Complete();
                }

                return Result.Success("Product inventory deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product inventory {Id}", command.Id);
                return Result.Failure("Error deleting product inventory", ErrorCodeEnum.InternalError);
            }
        }
    }
}
