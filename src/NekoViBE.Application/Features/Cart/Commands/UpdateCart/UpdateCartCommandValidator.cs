using FluentValidation;

namespace NekoViBE.Application.Features.Cart.Commands.UpdateCart;

public class UpdateCartCommandValidator : AbstractValidator<UpdateCartCommand>
{
    public UpdateCartCommandValidator()
    {
        RuleFor(x => x.CartItemId)
            .NotEmpty().WithMessage("CartItemId is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}