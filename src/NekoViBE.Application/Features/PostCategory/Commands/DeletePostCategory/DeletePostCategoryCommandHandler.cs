// NekoViBE.Application.Features.PostCategory.Commands.DeletePostCategory/DeletePostCategoryCommandHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.PostCategory.Commands.DeletePostCategory;

public class DeletePostCategoryCommandHandler : IRequestHandler<DeletePostCategoryCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePostCategoryCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public DeletePostCategoryCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeletePostCategoryCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeletePostCategoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId == null)
            {
                _logger.LogWarning("Invalid or unauthenticated user attempting to delete post category");
                return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }

            var repo = _unitOfWork.Repository<Domain.Entities.PostCategory>();
            var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);

            if (entity == null)
                return Result.Failure("Post category not found", ErrorCodeEnum.NotFound);

            // Check for dependencies
            var hasPosts = await _unitOfWork.Repository<Domain.Entities.BlogPost>().AnyAsync(x => x.PostCategoryId == command.Id);
            if (hasPosts)
                return Result.Failure("Cannot delete category with associated blog posts", ErrorCodeEnum.ResourceConflict);

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
                    EntityName = "PostCategory",
                    OldValue = JsonSerializer.Serialize(new { entity.Name, entity.Description }),
                    IPAddress = _currentUserService.IPAddress ?? "Unknown",
                    ActionDetail = $"Deleted post category: {entity.Name}",
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

            return Result.Success("Post category deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post category with ID: {Id}", command.Id);
            return Result.Failure("Error deleting post category", ErrorCodeEnum.InternalError);
        }
    }
}