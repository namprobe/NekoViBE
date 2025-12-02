// File: Application/Features/BlogPost/Queries/GetBlogPost/GetBlogPostQuery.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.BlogPost.Queries.GetBlogPost
{
    public record GetBlogPostQuery(Guid Id) : IRequest<Result<BlogPostResponse>>;
}