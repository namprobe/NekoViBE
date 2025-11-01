// File: Application/Features/BlogPost/Commands/CreateBlogPost/CreateBlogPostCommandValidator.cs
using FluentValidation;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.BlogPost.Commands.CreateBlogPost
{
    public class CreateBlogPostCommandValidator : AbstractValidator<CreateBlogPostCommand>
    {
        public CreateBlogPostCommandValidator()
        {
            RuleFor(x => x.Request).NotNull().WithMessage("Request is required");
            When(x => x.Request != null, () =>
            {
                this.SetupBlogPostRules(x => x.Request);
            });
        }
    }
}