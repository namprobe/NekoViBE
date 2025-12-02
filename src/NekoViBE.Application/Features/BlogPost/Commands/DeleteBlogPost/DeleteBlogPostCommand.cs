// File: Application/Features/BlogPost/Commands/DeleteBlogPost/DeleteBlogPostCommand.cs
using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.BlogPost.Commands.DeleteBlogPost
{
    public record DeleteBlogPostCommand(Guid Id) : IRequest<Result>;
}