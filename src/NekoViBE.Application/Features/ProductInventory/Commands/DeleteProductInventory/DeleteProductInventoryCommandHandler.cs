using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductInventory.Commands.DeleteProductInventory
{
    public class DeleteProductInventoryCommandHandler : IRequestHandler<DeleteProductInventoryCommand, Result>
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
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to delete product inventory");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.ProductInventory>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);
                if (entity == null)
                {
                    _logger.LogWarning("Product inventory with ID {Id} not found", command.Id);
                    return Result.Failure("Product inventory not found", ErrorCodeEnum.NotFound);
                }

                var productRepo = _unitOfWork.Repository<Domain.Entities.Product>();
                var product = await productRepo.GetFirstOrDefaultAsync(x => x.Id == entity.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", entity.ProductId);
                    return Result.Failure("Product not found", ErrorCodeEnum.NotFound);
                }

                product.StockQuantity -= entity.Quantity;
                entity.IsDeleted = true;
                entity.DeletedBy = userId;
                entity.DeletedAt = DateTime.UtcNow;
                entity.Status = EntityStatusEnum.Inactive;

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    repo.Update(entity);
                    productRepo.Update(product);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Delete,
                        EntityId = entity.Id,
                        EntityName = "ProductInventory",
                        OldValue = JsonSerializer.Serialize(new { entity.ProductId, entity.Quantity }),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Deleted product inventory with ID: {entity.Id} for product ID: {entity.ProductId}",
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

                return Result.Success("Product inventory deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product inventory with ID: {Id}", command.Id);
                return Result.Failure("Error deleting product inventory", ErrorCodeEnum.InternalError);
            }
        }
    }
}
