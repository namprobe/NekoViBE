using FluentValidation;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.ProductReview.Commands.UpdateProductReview
{
    public class UpdateProductReviewCommandValidator : AbstractValidator<UpdateProductReviewCommand>
    {
        public UpdateProductReviewCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Product review ID is required");

            RuleFor(x => x.Request)
                .NotNull().WithMessage("Product review request is required");

            this.SetupProductReviewRules(x => x.Request);
        }
    }
}