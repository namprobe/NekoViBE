using FluentValidation;

namespace NekoViBE.Application.Features.Cart.Commands.AddToCart;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.Request.ProductId)
            .NotEmpty().WithMessage("ProductId is required.");

        RuleFor(x => x.Request.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}