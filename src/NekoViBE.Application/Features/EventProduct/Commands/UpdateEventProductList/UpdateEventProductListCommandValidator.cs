// Application/Features/EventProduct/Commands/UpdateEventProductList/UpdateEventProductListCommandValidator.cs
using FluentValidation;

namespace NekoViBE.Application.Features.EventProduct.Commands.UpdateEventProductList
{
    public class UpdateEventProductListCommandValidator : AbstractValidator<UpdateEventProductListCommand>
    {
        public UpdateEventProductListCommandValidator()
        {
            RuleFor(x => x.EventId).NotEmpty().WithMessage("EventId is required");

            RuleFor(x => x.Products).NotNull().WithMessage("Product list cannot be null");

            RuleForEach(x => x.Products).ChildRules(product =>
            {
                product.RuleFor(p => p.ProductId).NotEmpty().WithMessage("ProductId is required");
                product.RuleFor(p => p.DiscountPercentage)
                    .InclusiveBetween(0, 100).WithMessage("Discount percentage must be between 0 and 100");
            });
        }
    }
}