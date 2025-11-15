// NekoViBE.Application.Features.PostCategory.Commands.UpdatePostCategory/UpdatePostCategoryCommandHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.PostCategory.Commands.UpdatePostCategory;

public class UpdatePostCategoryCommandHandler : IRequestHandler<UpdatePostCategoryCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdatePostCategoryCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePostCategoryCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdatePostCategoryCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdatePostCategoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId == null)
            {
                return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }

            var repo = _unitOfWork.Repository<Domain.Entities.PostCategory>();
            var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);

            if (entity == null)
                return Result.Failure("Post category not found", ErrorCodeEnum.NotFound);

            var oldValue = JsonSerializer.Serialize(_mapper.Map<PostCategoryRequest>(entity));
            var oldStatus = entity.Status;

            _mapper.Map(command.Request, entity);
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
                    EntityName = "PostCategory",
                    OldValue = oldValue,
                    NewValue = JsonSerializer.Serialize(command.Request),
                    IPAddress = _currentUserService.IPAddress ?? "Unknown",
                    ActionDetail = $"Updated post category: {command.Request.Name}",
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
                        EntityName = "PostCategory",
                        OldValue = oldStatus.ToString(),
                        NewValue = command.Request.Status.ToString(),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Changed status of post category '{entity.Name}' from {oldStatus} to {command.Request.Status}",
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

            return Result.Success("Post category updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post category with ID: {Id}", command.Id);
            return Result.Failure("Error updating post category", ErrorCodeEnum.InternalError);
        }
    }
}