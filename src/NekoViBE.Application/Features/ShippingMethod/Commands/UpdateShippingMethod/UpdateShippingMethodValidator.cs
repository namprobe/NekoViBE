using FluentValidation;
using NekoViBE.Application.Common.DTOs;

namespace NekoViBE.Application.Features.ShippingMethod.Commands.UpdateShippingMethod;

public class UpdateShippingMethodValidator : AbstractValidator<ShippingMethodRequest>
{
    public UpdateShippingMethodValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
            
        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Cost must be greater than or equal to 0");
            
        RuleFor(x => x.EstimatedDays)
            .GreaterThan(0).WithMessage("Estimated days must be greater than 0")
            .When(x => x.EstimatedDays.HasValue);
            
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
