using FluentValidation;

namespace NekoViBE.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is not valid");
        RuleFor(x => x.Request.Password).NotEmpty().WithMessage("Password is required");
            //.MinimumLength(8).WithMessage("Password must be at least 8 characters long");
            //.Must(BeAValidPassword).WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character");
        
    }

    // private static bool BeAValidPassword(string password)
    // {
    //     if (string.IsNullOrEmpty(password))
    //         return false;

    //     // Check for at least one uppercase letter
    //     var hasUpper = password.Any(char.IsUpper);
        
    //     // Check for at least one lowercase letter
    //     var hasLower = password.Any(char.IsLower);
        
    //     // Check for at least one digit
    //     var hasDigit = password.Any(char.IsDigit);
        
    //     // Check for at least one special character
    //     var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

    //     return hasUpper && hasLower && hasDigit && hasSpecial;
    // }
}