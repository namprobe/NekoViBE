// CreateBlogPostCommandHandler.cs
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

namespace NekoViBE.Application.Features.BlogPost.Commands.CreateBlogPost;

public class CreateBlogPostCommandHandler : IRequestHandler<CreateBlogPostCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBlogPostCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileService _fileService;

    public CreateBlogPostCommandHandler(IUnitOfWork unitOfWork, IMapper mapper,
        ILogger<CreateBlogPostCommandHandler> logger, ICurrentUserService currentUserService, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _fileService = fileService;
    }

    public async Task<Result> Handle(CreateBlogPostCommand command, CancellationToken ct)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId == null)
                return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);

            var entity = _mapper.Map<Domain.Entities.BlogPost>(command.Request);
            entity.AuthorId = userId;
            entity.CreatedBy = userId;
            entity.CreatedAt = DateTime.UtcNow;
            entity.PublishDate = command.Request.PublishDate ?? DateTime.UtcNow;

            string? imagePath = null;
            if (command.Request.FeaturedImageFile != null)
            {
                imagePath = await _fileService.UploadFileAsync(command.Request.FeaturedImageFile, "blog", ct);
                entity.FeaturedImagePath = imagePath;
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);

            await _unitOfWork.Repository<Domain.Entities.BlogPost>().AddAsync(entity);

            // Xử lý Tags
            foreach (var tagName in command.Request.TagNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var normalizedName = tagName.Trim();
                var tag = await _unitOfWork.Repository<Tag>()
                    .GetFirstOrDefaultAsync(t => t.Name.ToLower() == normalizedName.ToLower());

                if (tag == null)
                {
                    tag = new Tag { Name = normalizedName, CreatedBy = userId, CreatedAt = DateTime.UtcNow };
                    await _unitOfWork.Repository<Tag>().AddAsync(tag);
                    await _unitOfWork.SaveChangesAsync(ct); // Để lấy Id
                }

                var postTag = new PostTag
                {
                    PostId = entity.Id,
                    TagId = tag.Id,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<PostTag>().AddAsync(postTag);
            }

            var userAction = new UserAction
            {
                UserId = userId.Value,
                Action = UserActionEnum.Create,
                EntityId = entity.Id,
                EntityName = "BlogPost",
                NewValue = JsonSerializer.Serialize(command.Request),
                IPAddress = _currentUserService.IPAddress ?? "Unknown",
                ActionDetail = $"Created blog post: {entity.Title}",
                CreatedAt = DateTime.UtcNow,
                Status = EntityStatusEnum.Active
            };
            await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

            await _unitOfWork.CommitTransactionAsync(ct);
            return Result.Success("Blog post created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blog post");
            return Result.Failure("Error creating blog post", ErrorCodeEnum.InternalError);
        }
    }
}