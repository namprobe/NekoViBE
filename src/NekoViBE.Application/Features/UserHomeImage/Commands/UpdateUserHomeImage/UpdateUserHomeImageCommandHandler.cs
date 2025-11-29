// Application/Features/UserHomeImage/Commands/UpdateUserHomeImage/UpdateUserHomeImageCommandHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.UserHomeImage.Commands.UpdateUserHomeImage
{
    public class UpdateUserHomeImageCommandHandler : IRequestHandler<UpdateUserHomeImageCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateUserHomeImageCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public UpdateUserHomeImageCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdateUserHomeImageCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(UpdateUserHomeImageCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Unauthorized attempt to update user home image");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.UserHomeImage>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted);

                if (entity == null)
                {
                    _logger.LogWarning("UserHomeImage with ID {Id} not found", command.Id);
                    return Result.Failure("Record not found", ErrorCodeEnum.NotFound);
                }

                if (entity.UserId != userId.Value)
                {
                    _logger.LogWarning("User {UserId} attempted to update another user's home image", userId.Value);
                    return Result.Failure("You can only update your own home images", ErrorCodeEnum.Forbidden);
                }

                // Kiểm tra HomeImage mới có tồn tại không
                var homeImageExists = await _unitOfWork.Repository<Domain.Entities.HomeImage>()
                    .AnyAsync(x => x.Id == command.Request.HomeImageId && !x.IsDeleted);
                if (!homeImageExists)
                    return Result.Failure("Home image not found", ErrorCodeEnum.NotFound);

                // Kiểm tra Position mới có bị trùng không (ngoại trừ chính nó)
                if (command.Request.Position < 1 || command.Request.Position > 3)
                    return Result.Failure("Position must be 1, 2 or 3", ErrorCodeEnum.InvalidInput);

                var positionTaken = await repo.AnyAsync(x =>
                    x.UserId == userId.Value &&
                    x.Position == command.Request.Position &&
                    x.Id != command.Id &&
                    !x.IsDeleted);

                if (positionTaken)
                    return Result.Failure($"Position {command.Request.Position} is already taken", ErrorCodeEnum.InvalidInput);

                var oldValue = JsonSerializer.Serialize(new
                {
                    entity.HomeImageId,
                    entity.Position
                });

                // Map dữ liệu mới
                entity.HomeImageId = command.Request.HomeImageId;
                entity.Position = command.Request.Position;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = userId;

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                repo.Update(entity);

                var userAction = new UserAction
                {
                    UserId = userId.Value,
                    Action = UserActionEnum.Update,
                    EntityId = entity.Id,
                    EntityName = "UserHomeImage",
                    OldValue = oldValue,
                    NewValue = JsonSerializer.Serialize(new
                    {
                        command.Request.HomeImageId,
                        command.Request.Position
                    }),
                    IPAddress = _currentUserService.IPAddress ?? "Unknown",
                    ActionDetail = $"Updated home image position {entity.Position}",
                    CreatedAt = DateTime.UtcNow,
                    Status = EntityStatusEnum.Active
                };
                await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return Result.Success("Home image updated successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error updating UserHomeImage ID {Id}", command.Id);
                return Result.Failure("Error updating home image", ErrorCodeEnum.InternalError);
            }
        }
    }
}