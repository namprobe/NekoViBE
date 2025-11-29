using FluentValidation;

namespace NekoViBE.Application.Features.Order.Commands.PlaceOrder;

public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Place order request is required");

        When(x => x.Request is not null && x.Request.ProductId.HasValue, () =>
        {
            RuleFor(x => x.Request!.Quantity)
                .NotNull()
                .WithMessage("Quantity is required when purchasing a specific product");
        });

        When(x => x.Request is not null && x.Request.IsOneClick, () =>
        {
            RuleFor(x => x.Request!.GuestEmail)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Guest email is required for one click orders")
                .EmailAddress()
                .WithMessage("Guest email is not valid");

            RuleFor(x => x.Request!.GuestFirstName)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Guest first name is required for one click orders");

            RuleFor(x => x.Request!.GuestLastName)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Guest last name is required for one click orders");

            RuleFor(x => x.Request!.GuestPhone)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Guest phone is required for one click orders");

            RuleFor(x => x.Request!.OneClickAddress)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Guest address is required for one click orders");
        });

        When(x => x.Request is not null && x.Request.ShippingMethodId.HasValue, () =>
        {
            RuleFor(x => x.Request!.UserAddressId)
                .NotNull()
                .WithMessage("User address is required when shipping method is selected");
            
            RuleFor(x => x.Request!.ShippingAmount)
                .NotNull()
                .WithMessage("Shipping amount is required when shipping method is selected")
                .GreaterThanOrEqualTo(0)
                .WithMessage("Shipping amount cannot be negative");
        });
    }
}

