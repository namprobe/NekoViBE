using FluentValidation;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.ProductReview.Commands.CreateProductReview
{
    public class CreateProductReviewCommandValidator : AbstractValidator<CreateProductReviewCommand>
    {
        public CreateProductReviewCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull().WithMessage("Product review request is required");

            this.SetupProductReviewRules(x => x.Request);
        }
    }
}