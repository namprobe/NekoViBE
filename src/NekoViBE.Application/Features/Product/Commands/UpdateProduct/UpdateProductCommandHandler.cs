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
                }

                _mapper.Map(command.Request, entity);
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                var productImageRepo = _unitOfWork.Repository<Domain.Entities.ProductImage>();
                var productTagRepo = _unitOfWork.Repository<Domain.Entities.ProductTag>();

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // ----------- Update Images -----------
                    if (command.Request.ImageFiles != null && command.Request.ImageFiles.Any())
                    {
                        var existingImages = await productImageRepo.FindAsync(x => x.ProductId == entity.Id && !x.IsDeleted);
                        foreach (var img in existingImages)
                        {
                            if (!string.IsNullOrEmpty(img.ImagePath))
                            {
                                await _fileService.DeleteFileAsync(img.ImagePath, cancellationToken);
                            }
                            img.IsDeleted = true;
                            img.DeletedBy = userId;
                            img.DeletedAt = DateTime.UtcNow;
                            img.Status = EntityStatusEnum.Inactive;
                            productImageRepo.Update(img);
                        }

                        for (int i = 0; i < command.Request.ImageFiles.Count; i++)
                        {
                            var file = command.Request.ImageFiles[i];
                            var imagePath = await _fileService.UploadFileAsync(file, "uploads/products", cancellationToken);

                            var newImage = new Domain.Entities.ProductImage
                            {
                                ProductId = entity.Id,
                                ImagePath = imagePath,
                                IsPrimary = (i == 0),
                                DisplayOrder = i + 1,
                                CreatedBy = userId,
                                CreatedAt = DateTime.UtcNow,
                                Status = EntityStatusEnum.Active
                            };

                            await productImageRepo.AddAsync(newImage);
                        }
                    }

                    // ----------- Update Tags -----------
                    if (command.Request.TagIds != null)
                    {
                        var existingTags = await productTagRepo.FindAsync(x => x.ProductId == entity.Id && !x.IsDeleted);
                        foreach (var tag in existingTags)
                        {
                            tag.IsDeleted = true;
                            tag.DeletedBy = userId;
                            tag.DeletedAt = DateTime.UtcNow;
                            tag.Status = EntityStatusEnum.Inactive;
                            productTagRepo.Update(tag);
                        }

                        foreach (var tagId in command.Request.TagIds)
                        {
                            var newTag = new Domain.Entities.ProductTag
                            {
                                ProductId = entity.Id,
                                TagId = tagId,
                                CreatedBy = userId,
                                CreatedAt = DateTime.UtcNow,
                                Status = EntityStatusEnum.Active
                            };
                            await productTagRepo.AddAsync(newTag);
                        }
                    }

                    // ----------- Update Product -----------
                    repo.Update(entity);

                    // ----------- Log UserAction -----------
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

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
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
