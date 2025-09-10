using FluentValidation;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.PaymentMethod.Commands.CreatePaymentMethod;

/// <summary>
/// Validator for CreatePaymentMethodCommand
/// Uses common validation rules for easy reusability
/// </summary>
public class CreatePaymentMethodValidator : AbstractValidator<CreatePaymentMethodCommand>
{
    public CreatePaymentMethodValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Payment method request is required");

        // One-line setup for all PaymentMethod validation with flexible icon size
        When(x => x.Request != null, () =>
        {
            this.SetupPaymentMethodRules(x => x.Request, iconMaxSizeInMB: 3.0); // 3MB for payment method icons
        });
    }
}
