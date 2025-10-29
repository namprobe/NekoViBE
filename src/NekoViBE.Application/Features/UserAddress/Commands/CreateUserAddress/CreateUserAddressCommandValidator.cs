using FluentValidation;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.UserAddress.Commands.CreateUserAddress;

public class CreateUserAddressCommandValidator : AbstractValidator<CreateUserAddressCommand>
{
    public CreateUserAddressCommandValidator()
    {
        // Use extension method for complete validation setup
        this.SetupUserAddressRules(x => x.Request, requireAll: false);
    }
}