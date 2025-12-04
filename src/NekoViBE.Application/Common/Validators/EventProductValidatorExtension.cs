using FluentValidation;
using NekoViBE.Application.Common.DTOs.EventProduct;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.Validators
{
    public static class EventProductValidatorExtension
    {
        public static void SetupManageEventProductRules<T>(this AbstractValidator<T> validator, Expression<Func<T, EventProductRequest>> requestSelector)
        {
            var requestFunc = requestSelector.Compile();

            validator.RuleFor(x => requestFunc(x).EventId)
                .NotEmpty().WithMessage("Event ID is required");

            validator.RuleForEach(x => requestFunc(x).Products).ChildRules(products =>
            {
                products.RuleFor(p => p.ProductId)
                    .NotEmpty().WithMessage("Product ID is required");

                products.RuleFor(p => p.DiscountPercentage)
                    .InclusiveBetween(0, 100).WithMessage("Discount percentage must be between 0 and 100");
            });
        }
    }
}