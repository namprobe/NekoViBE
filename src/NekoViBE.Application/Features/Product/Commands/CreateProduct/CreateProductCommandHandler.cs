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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace NekoViBE.Application.Features.Product.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public CreateProductCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateProductCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(CreateProductCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to create product");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                // Đảm bảo PreOrderReleaseDate = null khi IsPreOrder = false
                if (!command.Request.IsPreOrder)
                {
                    command.Request.PreOrderReleaseDate = null;
                    _logger.LogInformation("IsPreOrder is false, setting PreOrderReleaseDate to null for product {Name}", command.Request.Name);
                }

                var entity = _mapper.Map<Domain.Entities.Product>(command.Request);
                entity.CreatedBy = userId;
                entity.CreatedAt = DateTime.UtcNow;
                entity.Status = EntityStatusEnum.Active;

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var productImageRepo = _unitOfWork.Repository<ProductImage>();

                    if (command.Request.ImageFile != null)
                    {
                        var imagePath = await _fileService.UploadFileAsync(command.Request.ImageFile, "uploads/products", cancellationToken);
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
                        entity.ProductImages.Add(newImage);
                        await productImageRepo.AddAsync(newImage);
                        _logger.LogInformation("Created new primary image with path {ImagePath} for product {Name}", imagePath, entity.Name);
                    }
                    else
                    {
                        _logger.LogWarning("No ImageFile provided for product {Name}", command.Request.Name);
                    }

                    // Thêm sản phẩm
                    await _unitOfWork.Repository<Domain.Entities.Product>().AddAsync(entity);

                    // Ghi lại UserAction
                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Create,
                        EntityId = entity.Id,
                        EntityName = "Product",
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Created product with name: {command.Request.Name}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    // Lưu tất cả thay đổi
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Hoàn tất giao dịch
                    scope.Complete();
                }

                return Result.Success("Product created successfully");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error uploading file for product");
                return Result.Failure("Error uploading file", ErrorCodeEnum.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product with name: {Name}", command.Request.Name);
                return Result.Failure("Error creating product", ErrorCodeEnum.InternalError);
            }
        }
    }
}