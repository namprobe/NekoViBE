using AutoMapper;
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
using System.Transactions;

namespace NekoViBE.Application.Features.ProductInventory.Commands.CreateProductInventory
{
    public class CreateProductInventoryCommandHandler : IRequestHandler<CreateProductInventoryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductInventoryCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public CreateProductInventoryCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateProductInventoryCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(CreateProductInventoryCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to create product inventory");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var productRepo = _unitOfWork.Repository<Domain.Entities.Product>();
                var product = await productRepo.GetFirstOrDefaultAsync(x => x.Id == command.Request.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", command.Request.ProductId);
                    return Result.Failure("Product not found", ErrorCodeEnum.NotFound);
                }

                var entity = _mapper.Map<Domain.Entities.ProductInventory>(command.Request);
                entity.CreatedBy = userId;
                entity.CreatedAt = DateTime.UtcNow;
                entity.Status = EntityStatusEnum.Active;

                product.StockQuantity += command.Request.Quantity;
                if (product.StockQuantity < 0)
                {
                    _logger.LogWarning("Stock quantity for product {ProductId} would become negative", command.Request.ProductId);
                    return Result.Failure("Stock quantity cannot be negative", ErrorCodeEnum.InvalidOperation);
                }

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _unitOfWork.Repository<Domain.Entities.ProductInventory>().AddAsync(entity);
                    productRepo.Update(product);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Create,
                        EntityId = entity.Id,
                        EntityName = "ProductInventory",
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Created product inventory for product ID: {command.Request.ProductId} with quantity: {command.Request.Quantity}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    scope.Complete();
                }

                return Result.Success("Product inventory created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product inventory for product ID: {ProductId}", command.Request.ProductId);
                return Result.Failure("Error creating product inventory", ErrorCodeEnum.InternalError);
            }
        }
    }
}