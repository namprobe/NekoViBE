// File: Application/Features/BlogPost/Commands/PublishBlogPost/PublishBlogPostCommandHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.BlogPost.Commands.PublishBlogPost
{
    public class PublishBlogPostCommandHandler : IRequestHandler<PublishBlogPostCommand, Result<BlogPostResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PublishBlogPostCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public PublishBlogPostCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PublishBlogPostCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result<BlogPostResponse>> Handle(PublishBlogPostCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                    return Result<BlogPostResponse>.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);

                var repo = _unitOfWork.Repository<Domain.Entities.BlogPost>();
                var entity = await repo.GetFirstOrDefaultAsync(
                    x => x.Id == command.Id,
                    x => x.PostTags,
                    x => x.PostCategory,
                    x => x.Author
                );

                if (entity == null)
                    return Result<BlogPostResponse>.Failure("Blog post not found", ErrorCodeEnum.NotFound);
                // Kiểm tra trạng thái hiện tại
                if (entity.IsPublished == command.IsPublished)
                {
                    var status = command.IsPublished ? "published" : "unpublished";
                    return Result<BlogPostResponse>.Failure($"Blog post is already {status}", ErrorCodeEnum.Conflict);
                }

                // Lưu giá trị cũ để audit
                var oldValue = JsonSerializer.Serialize(new
                {
                    entity.IsPublished,
                    entity.PublishDate
                });

                // Cập nhật
                entity.IsPublished = command.IsPublished;

                if (command.IsPublished)
                {
                    entity.PublishDate = DateTime.UtcNow; // Chỉ cập nhật khi publish
                }
                // Nếu unpublish → giữ nguyên PublishDate

                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);

                    repo.Update(entity);

                    // Audit log
                    var actionDetail = command.IsPublished
                        ? $"Published blog post: {entity.Title}"
                        : $"Unpublished blog post: {entity.Title}";

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = entity.Id,
                        EntityName = "BlogPost",
                        OldValue = oldValue,
                        NewValue = JsonSerializer.Serialize(new
                        {
                            entity.IsPublished,
                            PublishDate = entity.PublishDate
                        }),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = actionDetail,
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

                // Lấy URL ảnh
                entity.FeaturedImagePath = _fileService.GetFileUrl(entity.FeaturedImagePath);
                var response = _mapper.Map<BlogPostResponse>(entity);

                return Result<BlogPostResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing/unpublishing blog post {Id}", command.Id);
                return Result<BlogPostResponse>.Failure("Failed to update publish status", ErrorCodeEnum.InternalError);
            }
        }
    }
}