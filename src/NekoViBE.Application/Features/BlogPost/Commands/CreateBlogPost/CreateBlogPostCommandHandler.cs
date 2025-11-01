// File: CreateBlogPostCommandHandler.cs
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

namespace NekoViBE.Application.Features.BlogPost.Commands.CreateBlogPost
{
    public class CreateBlogPostCommandHandler : IRequestHandler<CreateBlogPostCommand, Result<BlogPostResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateBlogPostCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public CreateBlogPostCommandHandler(IUnitOfWork uow, IMapper mapper, ILogger<CreateBlogPostCommandHandler> logger,
            ICurrentUserService userService, IFileService fileService)
        {
            _unitOfWork = uow;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = userService;
            _fileService = fileService;
        }

        public async Task<Result<BlogPostResponse>> Handle(CreateBlogPostCommand cmd, CancellationToken ct)
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId == null) return Result<BlogPostResponse>.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);

            var entity = _mapper.Map<Domain.Entities.BlogPost>(cmd.Request);
            entity.AuthorId = userId;
            entity.CreatedBy = userId;
            entity.CreatedAt = DateTime.UtcNow;
            entity.PublishDate = cmd.Request.PublishDate ?? DateTime.UtcNow;

            // Upload image
            if (cmd.Request.FeaturedImageFile != null)
            {
                entity.FeaturedImagePath = await _fileService.UploadFileAsync(cmd.Request.FeaturedImageFile, "blog", ct);
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(ct);

                await _unitOfWork.Repository<Domain.Entities.BlogPost>().AddAsync(entity);

                // Add Tags
                foreach (var tagId in cmd.Request.TagIds)
                {
                    var postTag = new PostTag { PostId = entity.Id, TagId = tagId };
                    await _unitOfWork.Repository<PostTag>().AddAsync(postTag);
                }

                // Audit
                var action = new UserAction
                {
                    UserId = userId.Value,
                    Action = UserActionEnum.Create,
                    EntityId = entity.Id,
                    EntityName = "BlogPost",
                    NewValue = JsonSerializer.Serialize(cmd.Request),
                    ActionDetail = $"Created blog: {cmd.Request.Title}",
                    IPAddress = _currentUserService.IPAddress ?? "Unknown",
                    CreatedAt = DateTime.UtcNow,
                    Status = EntityStatusEnum.Active
                };
                await _unitOfWork.Repository<UserAction>().AddAsync(action);

                await _unitOfWork.CommitTransactionAsync(ct);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }

            entity.FeaturedImagePath = _fileService.GetFileUrl(entity.FeaturedImagePath);
            var response = _mapper.Map<BlogPostResponse>(entity);
            return Result<BlogPostResponse>.Success(response);
        }
    }
}