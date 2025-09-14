using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace NekoViBE.Application.Features.Product.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateProductCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public UpdateProductCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdateProductCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to update product");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.Product>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);

                if (entity == null)
                    return Result.Failure("Product not found", ErrorCodeEnum.NotFound);

                var oldValue = JsonSerializer.Serialize(_mapper.Map<ProductRequest>(entity));
                var oldStatus = entity.Status;

                if (!command.Request.IsPreOrder)
                {
                    command.Request.PreOrderReleaseDate = null;
                    _logger.LogInformation("IsPreOrder is false, setting PreOrderReleaseDate to null for product {Name}", command.Request.Name);
                }

                _mapper.Map(command.Request, entity);
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                var productImageRepo = _unitOfWork.Repository<ProductImage>();
                var primaryImage = await productImageRepo.GetFirstOrDefaultAsync(x => x.ProductId == entity.Id && x.IsPrimary && !x.IsDeleted);

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    if (command.Request.ImageFile != null)
                    {
                        // Upload new image
                        var imagePath = await _fileService.UploadFileAsync(command.Request.ImageFile, "images/products", cancellationToken);

                        if (primaryImage != null)
                        {
                            // Delete old image file if exists
                            if (!string.IsNullOrEmpty(primaryImage.ImagePath))
                            {
                                _logger.LogInformation("Deleting old primary image at {ImagePath} for product {Name}", primaryImage.ImagePath, entity.Name);
                                await _fileService.DeleteFileAsync(primaryImage.ImagePath, cancellationToken);
                            }

                            // Update existing primary image
                            primaryImage.ImagePath = imagePath;
                            primaryImage.UpdatedBy = userId;
                            primaryImage.UpdatedAt = DateTime.UtcNow;
                            productImageRepo.Update(primaryImage);
                            _logger.LogInformation("Updated primary image to {ImagePath} for product {Name}", imagePath, entity.Name);
                        }
                        else
                        {
                            // Create new primary image
                            var newImage = new ProductImage
                            {
                                ProductId = entity.Id,
                                ImagePath = imagePath,
                                IsPrimary = true,
                                DisplayOrder = 1,
                                CreatedBy = userId,
                                CreatedAt = DateTime.UtcNow,
                                Status = EntityStatusEnum.Active
                            };
                            await productImageRepo.AddAsync(newImage);
                            entity.ProductImages.Add(newImage);
                            _logger.LogInformation("Created new primary image with path {ImagePath} for product {Name}", imagePath, entity.Name);
                        }
                    }
                    else
                    {
                        // If no ImageFile provided, soft delete the primary image
                        if (primaryImage != null)
                        {
                            if (!string.IsNullOrEmpty(primaryImage.ImagePath))
                            {
                                _logger.LogInformation("Deleting primary image file at {ImagePath} for product {Name}", primaryImage.ImagePath, entity.Name);
                                await _fileService.DeleteFileAsync(primaryImage.ImagePath, cancellationToken);
                            }
                            primaryImage.IsDeleted = true;
                            primaryImage.DeletedBy = userId;
                            primaryImage.DeletedAt = DateTime.UtcNow;
                            primaryImage.Status = EntityStatusEnum.Inactive;
                            productImageRepo.Update(primaryImage);
                            _logger.LogInformation("Soft deleted primary image for product {Name}", entity.Name);
                        }
                        else
                        {
                            _logger.LogInformation("No primary image found to delete for product {Name}", entity.Name);
                        }
                    }

                    // Update product
                    repo.Update(entity);

                    // Ghi lại UserAction
                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = entity.Id,
                        EntityName = "Product",
                        OldValue = oldValue,
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Updated product with name: {command.Request.Name}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    // Ghi lại UserAction cho thay đổi trạng thái
                    if (oldStatus != command.Request.Status)
                    {
                        var statusChangeAction = new UserAction
                        {
                            UserId = userId.Value,
                            Action = UserActionEnum.StatusChange,
                            EntityId = entity.Id,
                            EntityName = "Product",
                            OldValue = oldStatus.ToString(),
                            NewValue = command.Request.Status.ToString(),
                            IPAddress = _currentUserService.IPAddress ?? "Unknown",
                            ActionDetail = $"Changed status of product '{entity.Name}' from {oldStatus} to {command.Request.Status}",
                            CreatedAt = DateTime.UtcNow,
                            Status = EntityStatusEnum.Active
                        };
                        await _unitOfWork.Repository<UserAction>().AddAsync(statusChangeAction);
                    }

                    // Lưu tất cả thay đổi trong UnitOfWork
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Hoàn tất giao dịch
                    scope.Complete();
                }

                return Result.Success("Product updated successfully");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error handling file for product");
                return Result.Failure("Error handling file", ErrorCodeEnum.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {Id}", command.Id);
                return Result.Failure("Error updating product", ErrorCodeEnum.InternalError);
            }
        }
    }
}