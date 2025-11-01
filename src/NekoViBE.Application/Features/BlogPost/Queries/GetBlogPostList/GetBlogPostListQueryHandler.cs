// Application/Features/BlogPost/Queries/GetBlogPostList/GetBlogPostListQueryHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.BlogPost.Queries.GetBlogPostList
{
    public class GetBlogPostListQueryHandler : IRequestHandler<GetBlogPostListQuery, PaginationResult<BlogPostItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetBlogPostListQueryHandler> _logger;
        private readonly IFileService _fileService;

        public GetBlogPostListQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetBlogPostListQueryHandler> logger,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<PaginationResult<BlogPostItem>> Handle(GetBlogPostListQuery request, CancellationToken cancellationToken)
        {
            var predicate = request.Filter.BuildPredicate();
            var orderBy = request.Filter.BuildOrderBy();
            var isAscending = request.Filter.IsAscending ?? false;

            var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.BlogPost>().GetPagedAsync(
                pageNumber: request.Filter.Page,
                pageSize: request.Filter.PageSize,
                predicate: predicate,
                orderBy: orderBy,
                isAscending: isAscending,
                includes: new Expression<Func<Domain.Entities.BlogPost, object>>[]
                {
                    x => x.PostCategory,
                    x => x.Author,
                    x => x.PostTags
                });

            var blogPostItems = _mapper.Map<List<BlogPostItem>>(items);

            // Gán URL ảnh nổi bật
            foreach (var (dto, entity) in blogPostItems.Zip(items))
            {
                dto.FeaturedImage = _fileService.GetFileUrl(entity.FeaturedImagePath);
            }

            // Log nếu có bài viết chưa có ảnh nổi bật
            var postsWithoutImage = items
                .Where(x => string.IsNullOrEmpty(x.FeaturedImagePath))
                .Select(x => x.Title)
                .ToList();

            if (postsWithoutImage.Any())
            {
                _logger.LogWarning("Blog posts without featured image: {PostTitles}", string.Join(", ", postsWithoutImage));
            }

            return PaginationResult<BlogPostItem>.Success(
                blogPostItems,
                request.Filter.Page,
                request.Filter.PageSize,
                totalCount);
        }
    }
}