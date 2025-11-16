using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Product.Queries.GetProductList; // Thêm namespace này
using NekoViBE.Application.Common.DTOs.Product; // Thêm namespace này

namespace NekoViBE.Application.Features.BlogPost.Queries.GetBlogPost
{
    public class GetBlogPostQueryHandler : IRequestHandler<GetBlogPostQuery, Result<BlogPostResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetBlogPostQueryHandler> _logger;
        private readonly IFileService _fileService;
        private readonly IMediator _mediator; // Thêm Mediator để gọi GetProductListQuery

        public GetBlogPostQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetBlogPostQueryHandler> logger,
            IFileService fileService,
            IMediator mediator) // Inject Mediator
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
            _mediator = mediator;
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
                response.AuthorAvatar = _fileService.GetFileUrl(entity.Author?.AvatarPath);

                // Gán danh sách Tag
                response.PostTags = entity.PostTags
                    .GroupBy(pt => pt.Id)
                    .Select(g => new Application.Common.DTOs.PostTag.PostTagItem
                    {
                        Id = g.Key,
                        TagId = g.First().TagId,
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

                // LẤY DANH SÁCH SẢN PHẨM DỰA TRÊN TAGIDS
                var tagIds = entity.PostTags.Select(pt => pt.TagId).ToList();
                if (tagIds.Any())
                {
                    var productFilter = new ProductFilter
                    {
                        TagIds = tagIds,
                        Page = 1,
                        PageSize = 10, // Có thể cấu hình số lượng sản phẩm tối đa
                        Status = Domain.Enums.EntityStatusEnum.Active // Chỉ lấy sản phẩm active
                    };

                    var productQuery = new GetProductListQuery(productFilter);
                    var productResult = await _mediator.Send(productQuery, cancellationToken);

                    if (productResult.IsSuccess)
                    {
                        response.Products = (List<ProductItem>?)productResult.Items; // Gán danh sách sản phẩm
                    }
                    else
                    {
                        _logger.LogWarning("Failed to retrieve products for blog post {Id}: {Errors}",
                            query.Id, string.Join(", ", productResult.Errors ?? Enumerable.Empty<string>()));
                        response.Products = new List<ProductItem>(); // Gán rỗng nếu lỗi
                    }
                }
                else
                {
                    response.Products = new List<ProductItem>(); // Không có tag, trả về rỗng
                }

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