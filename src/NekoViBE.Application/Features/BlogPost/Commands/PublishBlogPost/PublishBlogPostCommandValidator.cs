using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.BlogPost.Commands.PublishBlogPost
{
    public class PublishBlogPostCommandValidator : AbstractValidator<PublishBlogPostCommand>
    {
        public PublishBlogPostCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Blog post ID is required");
        }
    }
}
