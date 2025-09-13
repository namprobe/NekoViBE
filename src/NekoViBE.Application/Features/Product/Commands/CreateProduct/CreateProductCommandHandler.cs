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

namespace NekoViBE.Application.Features.Product.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public CreateProductCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateProductCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
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

                if (command.Request.ImageFile != null)
                {
                    _logger.LogInformation("Received ImageFile: {FileName}, Size: {FileSize}",
                        command.Request.ImageFile.FileName, command.Request.ImageFile.Length);
                    var fileName = $"{Guid.NewGuid()}_{command.Request.ImageFile.FileName}";
                    var filePath = Path.Combine("wwwroot/images/products", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await command.Request.ImageFile.CopyToAsync(stream, cancellationToken);
                    }
                    entity.ProductImages.Add(new ProductImage
                    {
                        ImagePath = $"/images/products/{fileName}",
                        IsPrimary = true,
                        DisplayOrder = 1,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    });
                    _logger.LogInformation("ImagePath set to {ImagePath} for product {Name}", $"/images/products/{fileName}", entity.Name);
                }
                else
                {
                    _logger.LogWarning("No ImageFile provided for product {Name}", command.Request.Name);
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    await _unitOfWork.Repository<Domain.Entities.Product>().AddAsync(entity);

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

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }

                return Result.Success("Product created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product with name: {Name}", command.Request.Name);
                return Result.Failure("Error creating product", ErrorCodeEnum.InternalError);
            }
        }
    }
}