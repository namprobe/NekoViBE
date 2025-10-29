using FluentValidation;
using NekoViBE.Application.Common.DTOs.ProductReview;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.Validators
{
    public static class ProductReviewsValidatorExtension
    {
        public static IRuleBuilderOptions<T, int> ValidProductReviewRating<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");
        }

        public static void SetupProductReviewRules<T>(this AbstractValidator<T> validator, Expression<Func<T, ProductReviewRequest>> requestSelector)
        {
            var requestFunc = requestSelector.Compile();

            validator.RuleFor(x => requestFunc(x).ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            validator.RuleFor(x => requestFunc(x).Rating)
                .ValidProductReviewRating();

            validator.RuleFor(x => requestFunc(x).Title)
                .MaximumLength(100).WithMessage("Title must not exceed 100 characters")
                .When(x => requestFunc(x).Title != null);

            validator.RuleFor(x => requestFunc(x).Comment)
                .MaximumLength(500).WithMessage("Comment must not exceed 500 characters")
                .When(x => requestFunc(x).Comment != null);

            validator.RuleFor(x => requestFunc(x).Status)
                .ValidAnimeSeriesStatus();
        }
    }
}