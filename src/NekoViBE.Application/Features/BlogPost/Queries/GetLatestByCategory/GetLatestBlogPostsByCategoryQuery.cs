using MediatR;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.BlogPost.Queries.GetLatestByCategory
{
    public class GetLatestBlogPostsByCategoryQuery : IRequest<Result<List<BlogPostItem>>>
    {
    }
}