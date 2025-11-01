// File: Application/Features/BlogPost/Queries/GetBlogPost/GetBlogPostQueryHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.BlogPost.Queries.GetBlogPost
{
    public class GetBlogPostQueryHandler : IRequestHandler<GetBlogPostQuery, Result<BlogPostResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetBlogPostQueryHandler> _logger;
        private readonly IFileService _fileService;

        public GetBlogPostQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetBlogPostQueryHandler> logger,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<Result<BlogPostResponse>> Handle(GetBlogPostQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _unitOfWork.Repository<Domain.Entities.BlogPost>().GetFirstOrDefaultAsync(
    x => x.Id == query.Id,
    x => x.PostCategory,
    x => x.Author,
    x => x.PostTags
);


                if (entity == null)
                    return Result<BlogPostResponse>.Failure("Blog post not found", ErrorCodeEnum.NotFound);

                foreach (var pt in entity.PostTags)
                {
                    pt.Tag = await _unitOfWork.Repository<Domain.Entities.Tag>()
                        .GetFirstOrDefaultAsync(t => t.Id == pt.TagId);
                }

                // Map cơ bản
                var response = _mapper.Map<BlogPostResponse>(entity);

                // Gán URL ảnh
                response.FeaturedImage = _fileService.GetFileUrl(entity.FeaturedImagePath);

                // Gán danh sách Tag
                response.PostTags = entity.PostTags
                    .GroupBy(pt => pt.Id)
                    .Select(g => new Application.Common.DTOs.PostTag.PostTagItem
                    {
                        Id = g.Key,
                        Tags = g
                            .Where(pt => pt.Tag != null)
                            .Select(pt => new TagItem
                            {
                                Id = pt.Tag.Id.ToString(),
                                Name = pt.Tag.Name
                            })
                            .ToList()
                    })
                    .ToList();

                return Result<BlogPostResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blog post {Id}", query.Id);
                return Result<BlogPostResponse>.Failure("Error retrieving blog post", ErrorCodeEnum.InternalError);
            }
        }
    }
}
