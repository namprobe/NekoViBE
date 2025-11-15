// File: Application/Features/BlogPost/Commands/UpdateBlogPost/UpdateBlogPostCommandValidator.cs
using FluentValidation;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.BlogPost.Commands.UpdateBlogPost
{
    public class UpdateBlogPostCommandValidator : AbstractValidator<UpdateBlogPostCommand>
    {
        public UpdateBlogPostCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request).NotNull();
            When(x => x.Request != null, () =>
            {
                this.SetupBlogPostRules(x => x.Request);
            });
        }
    }
}