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
    }
}

