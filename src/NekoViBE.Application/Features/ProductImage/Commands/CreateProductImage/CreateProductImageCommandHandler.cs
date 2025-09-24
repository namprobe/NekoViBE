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

namespace NekoViBE.Application.Features.ProductImage.Commands.CreateProductImage
{
    public class CreateProductImageCommandHandler : IRequestHandler<CreateProductImageCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductImageCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public CreateProductImageCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateProductImageCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(CreateProductImageCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to create product image");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var productRepo = _unitOfWork.Repository<Domain.Entities.Product>();
                var product = await productRepo.GetFirstOrDefaultAsync(x => x.Id == command.Request.ProductId && !x.IsDeleted);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found or is deleted", command.Request.ProductId);
                    return Result.Failure("Product not found", ErrorCodeEnum.NotFound);
                }

                // Check for existing DisplayOrder

                var existingDisplayOrder = await _unitOfWork.Repository<Domain.Entities.ProductImage>()

                    .GetFirstOrDefaultAsync(x => x.ProductId == command.Request.ProductId && !x.IsDeleted && x.DisplayOrder == command.Request.DisplayOrder);

                if (existingDisplayOrder != null)

                {

                    _logger.LogWarning("Display order {DisplayOrder} already exists for product ID {ProductId}", command.Request.DisplayOrder, command.Request.ProductId);

                    return Result.Failure("Display order is already used for this product", ErrorCodeEnum.InvalidOperation);

                }

                // Handle file upload
                var imagePath = await _fileService.UploadFileAsync(command.Request.Image, "images/products", cancellationToken);
                if (string.IsNullOrEmpty(imagePath))
                {
                    _logger.LogWarning("Failed to save image for product ID {ProductId}", command.Request.ProductId);
                    return Result.Failure("Failed to save image", ErrorCodeEnum.FileUploadFailed);
                }

                var entity = new Domain.Entities.ProductImage
                {
                    ProductId = command.Request.ProductId,
                    ImagePath = imagePath,
                    IsPrimary = command.Request.IsPrimary,
                    DisplayOrder = command.Request.DisplayOrder,
                    Status = command.Request.Status,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

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
                    await _unitOfWork.Repository<Domain.Entities.ProductImage>().AddAsync(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Create,
                        EntityId = entity.Id,
                        EntityName = "ProductImage",
                        NewValue = JsonSerializer.Serialize(new { command.Request.ProductId, entity.ImagePath, command.Request.IsPrimary, command.Request.DisplayOrder, command.Request.Status }),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Created product image for product ID: {command.Request.ProductId} with path: {imagePath}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogError(ex, "Database error creating product image for product ID: {ProductId}, ImagePath: {ImagePath}", command.Request.ProductId, imagePath);
                    return Result.Failure($"Database error creating product image: {ex.Message}", ErrorCodeEnum.DatabaseError);
                }

                return Result.Success("Product image created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product image for product ID: {ProductId}", command.Request.ProductId);
                return Result.Failure($"Error creating product image: {ex.Message}", ErrorCodeEnum.InternalError);
            }
        }
    }
}
