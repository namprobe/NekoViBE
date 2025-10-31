using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.ProductReview.Commands.CreateProductReview
{
    public class CreateProductReviewCommandHandler : IRequestHandler<CreateProductReviewCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductReviewCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public CreateProductReviewCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateProductReviewCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(CreateProductReviewCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to create product review");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                // Check if product exists
                var productExists = await _unitOfWork.Repository<Domain.Entities.Product>().AnyAsync(x => x.Id == command.Request.ProductId && !x.IsDeleted);
                if (!productExists)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", command.Request.ProductId);
                    return Result.Failure("Product not found", ErrorCodeEnum.NotFound);
                }


                var entity = _mapper.Map<Domain.Entities.ProductReview>(command.Request);
                entity.UserId = userId.Value;
                entity.CreatedBy = userId;
                entity.CreatedAt = DateTime.UtcNow;
                entity.Status = command.Request.Status;

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    await _unitOfWork.Repository<Domain.Entities.ProductReview>().AddAsync(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Create,
                        EntityId = entity.Id,
                        EntityName = "ProductReview",
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Created product review for product ID: {command.Request.ProductId}",
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

                return Result.Success("Product review created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product review");
                return Result.Failure("Error creating product review", ErrorCodeEnum.InternalError);
            }
        }
    }
}
