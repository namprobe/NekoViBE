// File: Application/Features/BlogPost/Commands/CreateBlogPost/CreateBlogPostCommand.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.BlogPost.Commands.CreateBlogPost
{
    public record CreateBlogPostCommand(BlogPostRequest Request) : IRequest<Result<BlogPostResponse>>;
}