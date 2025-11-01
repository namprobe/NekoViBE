// File: Application/Features/BlogPost/Commands/UpdateBlogPost/UpdateBlogPostCommand.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.BlogPost.Commands.UpdateBlogPost
{
    public record UpdateBlogPostCommand(Guid Id, BlogPostRequest Request) : IRequest<Result<BlogPostResponse>>;
}