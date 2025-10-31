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

namespace NekoViBE.Application.Features.ProductImage.Commands.DeleteProductImage
{
    public class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteProductImageCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileStorageService;

        public DeleteProductImageCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteProductImageCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileStorageService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileStorageService = fileStorageService;
        }

        public async Task<Result> Handle(DeleteProductImageCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to delete product image with ID {Id}", command.Id);
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.ProductImage>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted);
                if (entity == null)
                {
                    _logger.LogWarning("Product image with ID {Id} not found or is deleted", command.Id);
                    return Result.Failure("Product image not found", ErrorCodeEnum.NotFound);
                }

                var oldValue = JsonSerializer.Serialize(new { entity.ProductId, entity.ImagePath, entity.IsPrimary, entity.DisplayOrder, entity.Status });

                // Soft delete
                entity.IsDeleted = true;
                entity.Status = EntityStatusEnum.Inactive;
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                // Delete physical file
                if (!string.IsNullOrEmpty(entity.ImagePath))
                {
                    await _fileStorageService.DeleteFileAsync(entity.ImagePath, cancellationToken);
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    repo.Update(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Delete,
                        EntityId = entity.Id,
                        EntityName = "ProductImage",
                        OldValue = oldValue,
                        NewValue = null,
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Deleted product image ID: {entity.Id} for product ID: {entity.ProductId}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogError(ex, "Database error deleting product image with ID: {Id}", command.Id);
                    return Result.Failure($"Database error deleting product image: {ex.Message}", ErrorCodeEnum.DatabaseError);
                }

                return Result.Success("Product image deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product image with ID: {Id}", command.Id);
                return Result.Failure($"Error deleting product image: {ex.Message}", ErrorCodeEnum.InternalError);
            }
        }
    }
}
