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

namespace NekoViBE.Application.Features.EventProduct.Commands.DeleteEventProduct
{
    public class DeleteEventProductCommandHandler : IRequestHandler<DeleteEventProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteEventProductCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public DeleteEventProductCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteEventProductCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(DeleteEventProductCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to delete event product");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.EventProduct>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted);
                if (entity == null)
                    return Result.Failure("Event product not found", ErrorCodeEnum.NotFound);

                entity.IsDeleted = true;
                entity.DeletedBy = userId;
                entity.DeletedAt = DateTime.UtcNow;
                entity.Status = EntityStatusEnum.Inactive;

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    repo.Update(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Delete,
                        EntityId = entity.Id,
                        EntityName = "EventProduct",
                        OldValue = JsonSerializer.Serialize(new { entity.EventId, entity.ProductId, entity.IsFeatured, entity.DiscountPercentage }),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Deleted event product for EventId: {entity.EventId}, ProductId: {entity.ProductId}",
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

                return Result.Success("Event product deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event product with ID: {Id}", command.Id);
                return Result.Failure("Error deleting event product", ErrorCodeEnum.InternalError);
            }
        }
    }
}
