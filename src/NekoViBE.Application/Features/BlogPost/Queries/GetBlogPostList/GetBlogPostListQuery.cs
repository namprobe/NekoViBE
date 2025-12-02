// File: Application/Features/BlogPost/Queries/GetBlogPostList/GetBlogPostListQuery.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.BlogPost.Queries.GetBlogPostList
{
    public record GetBlogPostListQuery(BlogPostFilter Filter) : IRequest<PaginationResult<BlogPostItem>>;
}