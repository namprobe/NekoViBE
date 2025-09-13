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
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Product.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateProductCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProductCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdateProductCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
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

                // Check if CategoryId exists
                var categoryRepo = _unitOfWork.Repository<Domain.Entities.Category>();
                if (!await categoryRepo.AnyAsync(x => x.Id == command.Request.CategoryId))
                    return Result.Failure("Category does not exist", ErrorCodeEnum.NotFound);

                var oldValue = JsonSerializer.Serialize(_mapper.Map<ProductRequest>(entity));
                var oldStatus = entity.Status;

                // Ensure PreOrderReleaseDate is null if IsPreOrder is false
                if (!command.Request.IsPreOrder)
                {
                    command.Request.PreOrderReleaseDate = null;
                    _logger.LogInformation("IsPreOrder is false, setting PreOrderReleaseDate to null for product {Name}", command.Request.Name);
                }

                _mapper.Map(command.Request, entity);
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                // Handle image update
                var oldImage = entity.ProductImages.FirstOrDefault(x => x.IsPrimary);
                if (command.Request.ImageFile != null)
                {
                    _logger.LogInformation("Received ImageFile: {FileName}, Size: {FileSize}",
                        command.Request.ImageFile.FileName, command.Request.ImageFile.Length);
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(command.Request.ImageFile.FileName)}";
                    var filePath = Path.Combine("wwwroot/images/products", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await command.Request.ImageFile.CopyToAsync(stream, cancellationToken);
                    }

                    // Delete old image file if exists
                    if (oldImage != null && !string.IsNullOrEmpty(oldImage.ImagePath))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), oldImage.ImagePath.TrimStart('/'));
                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                            _logger.LogInformation("Deleted old image file: {OldFilePath}", oldFilePath);
                        }
                    }

                    // Update or add new image
                    if (oldImage != null)
                    {
                        oldImage.ImagePath = $"/images/products/{fileName}";
                        oldImage.UpdatedBy = userId;
                        oldImage.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        entity.ProductImages.Add(new ProductImage
                        {
                            ImagePath = $"/images/products/{fileName}",
                            IsPrimary = true,
                            DisplayOrder = 1,
                            CreatedBy = userId,
                            CreatedAt = DateTime.UtcNow,
                            Status = EntityStatusEnum.Active
                        });
                    }
                    _logger.LogInformation("ImagePath updated to {ImagePath} for product {Name}", $"/images/products/{fileName}", entity.Name);
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
                        EntityName = "Product",
                        OldValue = oldValue,
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Updated product with name: {command.Request.Name}",
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

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }

                return Result.Success("Product updated successfully");
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx, "Error saving image file for product");
                return Result.Failure("Error saving image file", ErrorCodeEnum.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {Id}", command.Id);
                return Result.Failure("Error updating product", ErrorCodeEnum.InternalError);
            }
        }
    }
}