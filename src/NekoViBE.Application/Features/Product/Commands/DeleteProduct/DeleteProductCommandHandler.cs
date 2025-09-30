using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Product.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteProductCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public DeleteProductCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteProductCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to delete product");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.Product>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);

                if (entity == null)
                    return Result.Failure("Product not found", ErrorCodeEnum.NotFound);

                // Check for dependencies
                var hasOrders = await _unitOfWork.Repository<Domain.Entities.OrderItem>().AnyAsync(x => x.ProductId == command.Id && !x.IsDeleted);
                var hasWishlistItems = await _unitOfWork.Repository<WishlistItem>().AnyAsync(x => x.ProductId == command.Id && !x.IsDeleted);
               
                if (hasOrders || hasWishlistItems)
                    return Result.Failure("Cannot delete product with associated orders, or wishlist items", ErrorCodeEnum.ResourceConflict);

                // Delete associated images
                foreach (var image in entity.ProductImages)
                {
                    if (!string.IsNullOrEmpty(image.ImagePath))
                    {
                        await _fileService.DeleteFileAsync(image.ImagePath, cancellationToken);
                    }
                }

                entity.IsDeleted = true;
                entity.DeletedBy = userId;
                entity.DeletedAt = DateTime.UtcNow;
                entity.Status = EntityStatusEnum.Inactive;

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    repo.Update(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Delete,
                        EntityId = entity.Id,
                        EntityName = "Product",
                        OldValue = JsonSerializer.Serialize(entity),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Deleted product with name: {entity.Name}",
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

                return Result.Success("Product deleted successfully");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error deleting file for product with ID: {Id}", command.Id);
                return Result.Failure("Error deleting file", ErrorCodeEnum.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {Id}", command.Id);
                return Result.Failure("Error deleting product", ErrorCodeEnum.InternalError);
            }
        }
    }
}