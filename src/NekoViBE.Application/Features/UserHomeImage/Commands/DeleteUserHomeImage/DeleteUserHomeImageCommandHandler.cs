// Handler
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.UserHomeImage.Commands.DeleteUserHomeImage
{
    public class DeleteUserHomeImageCommandHandler : IRequestHandler<DeleteUserHomeImageCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteUserHomeImageCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public DeleteUserHomeImageCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteUserHomeImageCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(DeleteUserHomeImageCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);

                var repo = _unitOfWork.Repository<Domain.Entities.UserHomeImage>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted);

                if (entity == null)
                    return Result.Failure("Record not found", ErrorCodeEnum.NotFound);

                if (entity.UserId != userId.Value)
                    return Result.Failure("You can only delete your own home images", ErrorCodeEnum.Forbidden);

                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = userId;
                entity.Status = EntityStatusEnum.Inactive;

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                repo.Update(entity);

                var userAction = new UserAction
                {
                    UserId = userId.Value,
                    Action = UserActionEnum.Delete,
                    EntityId = entity.Id,
                    EntityName = "UserHomeImage",
                    OldValue = JsonSerializer.Serialize(new { entity.HomeImageId, entity.Position }),
                    IPAddress = _currentUserService.IPAddress ?? "Unknown",
                    ActionDetail = $"Removed home image from position {entity.Position}",
                    CreatedAt = DateTime.UtcNow,
                    Status = EntityStatusEnum.Active
                };
                await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return Result.Success("Home image removed successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error deleting UserHomeImage {Id}", command.Id);
                return Result.Failure("Error deleting home image", ErrorCodeEnum.InternalError);
            }
        }
    }
}