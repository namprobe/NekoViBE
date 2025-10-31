using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Badge;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;


namespace NekoViBE.Application.Features.Badge.Command.UpdateBadge
{
    public class UpdateBadgeCommandHandler : IRequestHandler<UpdateBadgeCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateBadgeCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public UpdateBadgeCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdateBadgeCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(UpdateBadgeCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to update badge");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.Badge>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);

                if (entity == null)
                    return Result.Failure("Badge not found", ErrorCodeEnum.NotFound);


                var oldValue = JsonSerializer.Serialize(_mapper.Map<UpdateBadgeRequest>(entity));
                var oldStatus = entity.Status;
                var oldImagePath = entity.IconPath;

                _mapper.Map(command.Request, entity);
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                if (command.Request.IconPath != null)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(oldImagePath))
                    {
                        await _fileService.DeleteFileAsync(oldImagePath, cancellationToken);
                    }

                    // Upload new image
                    var imagePath = await _fileService.UploadFileAsync(command.Request.IconPath, "uploads/badge", cancellationToken);
                    entity.IconPath = imagePath;
                    _logger.LogInformation("ImagePath updated to {ImagePath} for badge {Name}", imagePath, entity.Name);
                }
                else
                {
                    await _fileService.DeleteFileAsync(oldImagePath, cancellationToken);
                    entity.IconPath = null;
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    repo.Update(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = entity.Id,
                        EntityName = "Badge",
                        OldValue = oldValue,
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Updated badge with name: {command.Request.Name}",
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
                            EntityName = "Badge",
                            OldValue = oldStatus.ToString(),
                            NewValue = command.Request.Status.ToString(),
                            IPAddress = _currentUserService.IPAddress ?? "Unknown",
                            ActionDetail = $"Changed status of badge '{entity.Name}' from {oldStatus} to {command.Request.Status}",
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

                return Result.Success("Badge updated successfully");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error handling file for badge");
                return Result.Failure("Error handling file", ErrorCodeEnum.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating badge with ID: {Id}", command.Id);
                return Result.Failure("Error updating badge", ErrorCodeEnum.InternalError);
            }
        }
    }
    //public class UpdateBadgeCommandHandler : IRequestHandler<UpdateBadgeCommand, Result<BadgeDto>>
    //{
    //    private readonly ILogger<UpdateBadgeCommandHandler> _logger;
    //    private readonly IUnitOfWork _unitOfWork;
    //    private readonly IMapper _mapper;
    //    private readonly ICurrentUserService _currentUserService;

    //    public UpdateBadgeCommandHandler(
    //        ILogger<UpdateBadgeCommandHandler> logger,
    //        IUnitOfWork unitOfWork,
    //        IMapper mapper,
    //        ICurrentUserService currentUserService)
    //    {
    //        _logger = logger;
    //        _unitOfWork = unitOfWork;
    //        _mapper = mapper;
    //        _currentUserService = currentUserService;
    //    }

    //    public async Task<Result<BadgeDto>> Handle(UpdateBadgeCommand request, CancellationToken cancellationToken)
    //    {
    //        try
    //        {
    //            var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
    //            if (!isValid)
    //            {
    //                return Result<BadgeDto>.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
    //            }

    //            var badge = await _unitOfWork.Repository<Domain.Entities.Badge>().GetByIdAsync(request.Id);
    //            if (badge == null)
    //            {
    //                return Result<BadgeDto>.Failure("Badge not found", ErrorCodeEnum.NotFound);
    //            }

    //            _mapper.Map(request, badge);
    //            badge.UpdatedAt = DateTime.UtcNow;
    //            badge.UpdatedBy = currentUserId;

    //            _unitOfWork.Repository<Domain.Entities.Badge>().Update(badge);
    //            await _unitOfWork.SaveChangesAsync(cancellationToken);

    //            var badgeDto = _mapper.Map<BadgeDto>(badge);
    //            return Result<BadgeDto>.Success(badgeDto, "Badge updated successfully");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error updating badge {BadgeId}", request.Id);
    //            return Result<BadgeDto>.Failure("Error updating badge", ErrorCodeEnum.InternalError);
    //        }
    //    }
    //}

}
