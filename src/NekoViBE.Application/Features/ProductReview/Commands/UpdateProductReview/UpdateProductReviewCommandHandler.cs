using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.ProductReview;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.ProductReview.Commands.UpdateProductReview
{
    public class UpdateProductReviewCommandHandler : IRequestHandler<UpdateProductReviewCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateProductReviewCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProductReviewCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdateProductReviewCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(UpdateProductReviewCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to update product review");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.ProductReview>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted);
                if (entity == null || entity.UserId != userId)
                {
                    _logger.LogWarning("Comment not found", command.Id);
                    return Result.Failure("Comment not found", ErrorCodeEnum.NotFound);
                }


                // Check if product exists
                var productExists = await _unitOfWork.Repository<Domain.Entities.Product>().AnyAsync(x => x.Id == command.Request.ProductId && !x.IsDeleted);
                if (!productExists)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", command.Request.ProductId);
                    return Result.Failure("Product not found", ErrorCodeEnum.NotFound);
                }

                var oldValue = JsonSerializer.Serialize(_mapper.Map<ProductReviewRequest>(entity));
                var oldStatus = entity.Status;

                _mapper.Map(command.Request, entity);
                entity.UserId = userId.Value; // Ensure UserId remains unchanged
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    repo.Update(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = entity.Id,
                        EntityName = "ProductReview",
                        OldValue = oldValue,
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Updated product review for product ID: {command.Request.ProductId}",
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
                            EntityName = "ProductReview",
                            OldValue = oldStatus.ToString(),
                            NewValue = command.Request.Status.ToString(),
                            IPAddress = _currentUserService.IPAddress ?? "Unknown",
                            ActionDetail = $"Changed status of product review from {oldStatus} to {command.Request.Status}",
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

                return Result.Success("Product review updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product review with ID: {Id}", command.Id);
                return Result.Failure("Error updating product review", ErrorCodeEnum.InternalError);
            }
        }
    }
}
