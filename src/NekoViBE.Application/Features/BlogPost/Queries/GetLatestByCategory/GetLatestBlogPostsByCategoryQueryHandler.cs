using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using System.Linq.Expressions;

namespace NekoViBE.Application.Features.BlogPost.Queries.GetLatestByCategory
{
    public class GetLatestBlogPostsByCategoryQueryHandler
        : IRequestHandler<GetLatestBlogPostsByCategoryQuery, Result<List<BlogPostItem>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetLatestBlogPostsByCategoryQueryHandler> _logger;
        private readonly IFileService _fileService;

        public GetLatestBlogPostsByCategoryQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetLatestBlogPostsByCategoryQueryHandler> logger,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<Result<List<BlogPostItem>>> Handle(GetLatestBlogPostsByCategoryQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // DÙNG FindAsync để có predicate
                var allPublishedPosts = await _unitOfWork.Repository<Domain.Entities.BlogPost>()
                    .FindAsync(
                        predicate: bp => bp.IsPublished && bp.Status == EntityStatusEnum.Active,
                        includes: new Expression<Func<Domain.Entities.BlogPost, object>>[]
                        {
                            x => x.PostCategory,
                            x => x.Author
                        });

                // Group theo PostCategoryId và lấy bài mới nhất (CreatedAt)
                var latestPosts = allPublishedPosts
                    .GroupBy(bp => bp.PostCategoryId)
                    .Select(g => g.OrderByDescending(bp => bp.CreatedAt).FirstOrDefault())
                    .Where(bp => bp != null)
                    .ToList();

                // Map sang DTO
                var response = _mapper.Map<List<BlogPostItem>>(latestPosts);

                // Gán URL ảnh
                foreach (var (dto, entity) in response.Zip(latestPosts, (d, e) => (d, e)))
                {
                    dto.FeaturedImage = _fileService.GetFileUrl(entity.FeaturedImagePath);
                    dto.AuthorAvatar = _fileService.GetFileUrl(entity.Author?.AvatarPath);
                }

                return Result<List<BlogPostItem>>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest blog posts by category");
                return Result<List<BlogPostItem>>.Failure("Failed to retrieve latest posts", ErrorCodeEnum.InternalError);
            }
        }
    }
}