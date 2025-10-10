using AutoMapper;
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

namespace NekoViBE.Application.Features.ProductImage.Commands.UpdateProductImage
{
    public class UpdateProductImageCommandHandler : IRequestHandler<UpdateProductImageCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateProductImageCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public UpdateProductImageCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdateProductImageCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(UpdateProductImageCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to update product image with ID {Id}", command.Id);
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.ProductImage>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted);
                if (entity == null)
                {
                    _logger.LogWarning("Product image with ID {Id} not found or is deleted", command.Id);
                    return Result.Failure("Product image not found", ErrorCodeEnum.NotFound);
                }

                var productRepo = _unitOfWork.Repository<Domain.Entities.Product>();
                var newProduct = await productRepo.GetFirstOrDefaultAsync(x => x.Id == command.Request.ProductId && !x.IsDeleted);
                if (newProduct == null)
                {
                    _logger.LogWarning("New product with ID {ProductId} not found or is deleted", command.Request.ProductId);
                    return Result.Failure("New product not found", ErrorCodeEnum.NotFound);
                }

                // Check for existing DisplayOrder (excluding current image)

                var existingDisplayOrder = await _unitOfWork.Repository<Domain.Entities.ProductImage>()

                    .GetFirstOrDefaultAsync(x => x.ProductId == command.Request.ProductId && x.Id != command.Id && !x.IsDeleted && x.DisplayOrder == command.Request.DisplayOrder);

                if (existingDisplayOrder != null)

                {

                    _logger.LogWarning("Display order {DisplayOrder} already exists for product ID {ProductId}", command.Request.DisplayOrder, command.Request.ProductId);

                    return Result.Failure("Display order is already used for this product", ErrorCodeEnum.InvalidOperation);

                }

                var oldValue = JsonSerializer.Serialize(new { entity.ProductId, entity.ImagePath, entity.IsPrimary, entity.DisplayOrder, entity.Status });
                var oldProductId = entity.ProductId;
                var oldStatus = entity.Status;
                var oldIsPrimary = entity.IsPrimary;

                string? newImagePath = entity.ImagePath;
                if (command.Request.Image != null)
                {
                    // Delete old image
                    if (!string.IsNullOrEmpty(entity.ImagePath))
                    {
                        await _fileService.DeleteFileAsync(entity.ImagePath, cancellationToken);
                    }
                    // Upload new image
                    newImagePath = await _fileService.UploadFileAsync(command.Request.Image, "uploads", cancellationToken);
                    if (string.IsNullOrEmpty(newImagePath))
                    {
                        _logger.LogWarning("Failed to save new image for product image ID {Id}", command.Id);
                        return Result.Failure("Failed to save new image", ErrorCodeEnum.FileUploadFailed);
                    }
                }

                // Update entity fields
                entity.ProductId = command.Request.ProductId;
                entity.ImagePath = newImagePath;
                entity.IsPrimary = command.Request.IsPrimary;
                entity.DisplayOrder = command.Request.DisplayOrder;
                entity.Status = command.Request.Status;
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                // If IsPrimary is true, set other images for this product to IsPrimary = false
                if (command.Request.IsPrimary)
                {
                    var existingImages = await _unitOfWork.Repository<Domain.Entities.ProductImage>()
                        .FindAsync(x => x.ProductId == command.Request.ProductId && !x.IsDeleted && x.IsPrimary);

                    foreach (var img in existingImages)
                    {
                        img.IsPrimary = false;
                        // gán DisplayOrder cũ = DisplayOrder người dùng nhập vào
                        img.DisplayOrder = command.Request.DisplayOrder;
                        _unitOfWork.Repository<Domain.Entities.ProductImage>().Update(img);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }

                    // ảnh mới sẽ luôn có DisplayOrder = 1
                    entity.DisplayOrder = 1;
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    repo.Update(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = entity.Id,
                        EntityName = "ProductImage",
                        OldValue = oldValue,
                        NewValue = JsonSerializer.Serialize(new { command.Request.ProductId, ImagePath = newImagePath, command.Request.IsPrimary, command.Request.DisplayOrder, command.Request.Status }),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Updated product image ID: {entity.Id} for product ID: {command.Request.ProductId} with path: {newImagePath}" + (oldProductId != command.Request.ProductId ? $" (ProductId changed from {oldProductId})" : ""),
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    if (oldStatus != command.Request.Status)
                    {
                        var statusChangeAction = new UserAction
                        {
                            UserId = userId.Value,
                            Action = UserActionEnum.StatusChange,
                            EntityId = entity.Id,
                            EntityName = "ProductImage",
                            OldValue = oldStatus.ToString(),
                            NewValue = command.Request.Status.ToString(),
                            IPAddress = _currentUserService.IPAddress ?? "Unknown",
                            ActionDetail = $"Changed status of product image '{entity.Id}' from {oldStatus} to {command.Request.Status}",
                            CreatedAt = DateTime.UtcNow,
                            Status = EntityStatusEnum.Active
                        };
                        await _unitOfWork.Repository<UserAction>().AddAsync(statusChangeAction);
                    }

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogError(ex, "Database error updating product image with ID: {Id}, ProductId: {ProductId}, ImagePath: {ImagePath}", command.Id, command.Request.ProductId, newImagePath);
                    return Result.Failure($"Database error updating product image: {ex.Message}", ErrorCodeEnum.DatabaseError);
                }

                _logger.LogInformation("Successfully updated product image {Id}: ProductId={ProductId}, ImagePath={ImagePath}", entity.Id, entity.ProductId, entity.ImagePath);
                return Result.Success("Product image updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product image with ID: {Id}, ProductId: {ProductId}", command.Id, command.Request.ProductId);
                return Result.Failure($"Error updating product image: {ex.Message}", ErrorCodeEnum.InternalError);
            }
        }
    }
}
