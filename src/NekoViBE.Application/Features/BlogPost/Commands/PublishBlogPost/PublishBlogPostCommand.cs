using MediatR;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.BlogPost.Commands.PublishBlogPost
{
    public record PublishBlogPostCommand(Guid Id, bool IsPublished) : IRequest<Result<BlogPostResponse>>;
}
