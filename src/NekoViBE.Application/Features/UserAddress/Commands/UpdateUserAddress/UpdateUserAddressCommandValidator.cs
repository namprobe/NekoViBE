using FluentValidation;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.UserAddress.Commands.UpdateUserAddress;

public class UpdateUserAddressCommandValidator : AbstractValidator<UpdateUserAddressCommand>
{
    public UpdateUserAddressCommandValidator()
    {
        // ID validation
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Address ID is required");
        
        // Use extension method with requireAll = true for update validation
        // This ensures all fields that are provided cannot be null/empty
        this.SetupUserAddressRules(x => x.Request, requireAll: true);
    }
}

