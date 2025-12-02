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
            var request = command.Request;

            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId == null)
                return Result.Failure("Invalid user", ErrorCodeEnum.Unauthorized);


            if (!request.IsPreOrder)
                request.PreOrderReleaseDate = null;

            var entity = _mapper.Map<Domain.Entities.Product>(request);

            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                // Thêm product
                await _unitOfWork.Repository<Domain.Entities.Product>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                //  Validate ảnh
                if (request.ImageFiles == null || !request.ImageFiles.Any())
                    return Result.Failure("At least one product image is required", ErrorCodeEnum.FileNotFound);

                // Upload tất cả ảnh
                for (int i = 0; i < request.ImageFiles.Count; i++)
                {
                    var file = request.ImageFiles[i];
                    if (file == null || file.Length == 0) continue;

                    var filePath = await _fileService.UploadFileAsync(file, "uploads", cancellationToken);


                    var productImage = new Domain.Entities.ProductImage
                    {
                        ProductId = entity.Id,
                        ImagePath = filePath,
                        IsPrimary = (i == 0),  // ảnh đầu tiên là Primary
                        DisplayOrder = i + 1,
                        CreatedBy = userId
                    };

                    await _unitOfWork.Repository<Domain.Entities.ProductImage>().AddAsync(productImage);

                }

                // Thêm ProductTag nếu có
                if (request.TagIds != null && request.TagIds.Any())
                {
                    foreach (var tagId in request.TagIds)
                    {
                        var productTag = new Domain.Entities.ProductTag
                        {
                            ProductId = entity.Id,
                            TagId = tagId,
                            CreatedBy = userId.Value
                        };
                        await _unitOfWork.Repository<Domain.Entities.ProductTag>().AddAsync(productTag);

                    }
                }

                // Ghi lại UserAction
                var userAction = new UserAction { 
                    UserId = userId.Value, 
                    Action = UserActionEnum.Create, 
                    EntityId = entity.Id, 
                    EntityName = "Product", 
                    NewValue = JsonSerializer.Serialize(command.Request), 
                    IPAddress = _currentUserService.IPAddress ?? "Unknown", 
                    ActionDetail = $"Created product with name: {command.Request.Name}", 
                    CreatedAt = DateTime.UtcNow, Status = EntityStatusEnum.Active }; 

                await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                transaction.Complete();

                return Result.Success("Product created successfully");
            }
            catch (IOException ex)
            {
                return Result.Failure($"Image upload failed:", ErrorCodeEnum.FileUploadFailed);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Product creation failed:", ErrorCodeEnum.StorageError);
            }
        }

    }
}