using FluentValidation;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.Request.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");
        RuleFor(x => x.Request.ConfirmPassword).ValidConfirmPassword(x => x.Request.ConfirmPassword);
        RuleFor(x => x.Request.NewPassword).ValidPassword(8);
    }
}