// File: Application/Features/BlogPost/Commands/DeleteBlogPost/DeleteBlogPostCommandHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.BlogPost.Commands.DeleteBlogPost
{
    public class DeleteBlogPostCommandHandler : IRequestHandler<DeleteBlogPostCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteBlogPostCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public DeleteBlogPostCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteBlogPostCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(DeleteBlogPostCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);

                var repo = _unitOfWork.Repository<Domain.Entities.BlogPost>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);

                if (entity == null)
                    return Result.Failure("Blog post not found", ErrorCodeEnum.NotFound);

                if (!string.IsNullOrEmpty(entity.FeaturedImagePath))
                    await _fileService.DeleteFileAsync(entity.FeaturedImagePath, cancellationToken);

                entity.IsDeleted = true;
                entity.DeletedBy = userId;
                entity.DeletedAt = DateTime.UtcNow;
                entity.Status = EntityStatusEnum.Inactive;

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    repo.Update(entity);

                    var oldTags = await _unitOfWork.Repository<PostTag>()
    .FindAsync(x => x.PostId == command.Id);
                    _unitOfWork.Repository<PostTag>().DeleteRange(oldTags);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Delete,
                        EntityId = entity.Id,
                        EntityName = "BlogPost",
                        OldValue = JsonSerializer.Serialize(new { entity.Title, entity.Content }),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Deleted blog post: {entity.Title}",
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

                return Result.Success("Blog post deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog post {Id}", command.Id);
                return Result.Failure("Failed to delete blog post", ErrorCodeEnum.InternalError);
            }
        }
    }
}