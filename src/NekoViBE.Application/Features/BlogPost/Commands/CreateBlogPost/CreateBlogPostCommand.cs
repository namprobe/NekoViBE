// CreateBlogPostCommand.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.Models;

public record CreateBlogPostCommand(BlogPostRequest Request) : IRequest<Result>;