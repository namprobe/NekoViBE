// File: Application/Features/BlogPost/Commands/UpdateBlogPost/UpdateBlogPostCommandHandler.cs
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

namespace NekoViBE.Application.Features.BlogPost.Commands.UpdateBlogPost
{
    public class UpdateBlogPostCommandHandler : IRequestHandler<UpdateBlogPostCommand, Result<BlogPostResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateBlogPostCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public UpdateBlogPostCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdateBlogPostCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result<BlogPostResponse>> Handle(UpdateBlogPostCommand command, CancellationToken cancellationToken)
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

                var oldImagePath = entity.FeaturedImagePath;
                var oldValue = JsonSerializer.Serialize(_mapper.Map<BlogPostRequest>(entity));

                _mapper.Map(command.Request, entity);
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                if (command.Request.FeaturedImageFile != null)
                {
                    // Có ảnh mới → xóa ảnh cũ
                    if (!string.IsNullOrEmpty(oldImagePath))
                        await _fileService.DeleteFileAsync(oldImagePath, cancellationToken);

                    var newPath = await _fileService.UploadFileAsync(command.Request.FeaturedImageFile, "blog", cancellationToken);
                    entity.FeaturedImagePath = newPath;
                }
                else if (command.Request.RemoveFeaturedImage)
                {
                    // Yêu cầu xóa ảnh → xóa file + set null
                    if (!string.IsNullOrEmpty(oldImagePath))
                        await _fileService.DeleteFileAsync(oldImagePath, cancellationToken);

                    entity.FeaturedImagePath = null;
                }
                else
                {
                    // Không làm gì → giữ nguyên ảnh cũ
                    entity.FeaturedImagePath = oldImagePath;
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);

                    repo.Update(entity);

                    // Xóa PostTag cũ
                    var oldTags = await _unitOfWork.Repository<PostTag>()
    .FindAsync(x => x.PostId == entity.Id); // ← DÙNG FindAsync
                    _unitOfWork.Repository<PostTag>().DeleteRange(oldTags);

                    // Thêm PostTag mới
                    foreach (var tagId in command.Request.TagIds)
                    {
                        var postTag = new PostTag { PostId = entity.Id, TagId = tagId };
                        await _unitOfWork.Repository<PostTag>().AddAsync(postTag);
                    }

                    // Audit
                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = entity.Id,
                        EntityName = "BlogPost",
                        OldValue = oldValue,
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Updated blog post: {command.Request.Title}",
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

                entity.FeaturedImagePath = _fileService.GetFileUrl(entity.FeaturedImagePath);
                var response = _mapper.Map<BlogPostResponse>(entity);
                return Result<BlogPostResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog post {Id}", command.Id);
                return Result<BlogPostResponse>.Failure("Failed to update blog post", ErrorCodeEnum.InternalError);
            }
        }
    }
}