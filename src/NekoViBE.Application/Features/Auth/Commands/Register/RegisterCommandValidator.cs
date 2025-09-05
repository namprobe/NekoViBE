using FluentValidation;
namespace NekoViBE.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Request.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.Request.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Request.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.Request.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

        RuleFor(x => x.Request.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

        RuleFor(x => x.Request.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^[0-9]{10,11}$").WithMessage("Invalid phone number format");

        RuleFor(x => x.Request.Gender)
            .IsInEnum().When(x => x.Request.Gender.HasValue)
            .WithMessage("Invalid gender value");
        
        // Avatar validation - only when avatar is not null
        RuleFor(x => x.Request.Avatar)
            .Must(BeValidImageFile)
            .WithMessage($"Avatar must be an image file ({string.Join(", ", _allowedImageExtensions)}) and less than {MaxFileSizeBytes / (1024 * 1024)}MB")
            .When(x => x.Request.Avatar != null);
    }

    private bool BeValidImageFile(Microsoft.AspNetCore.Http.IFormFile? file)
    {
        if (file == null) return true; // Allow null files

        // Check file size
        if (file.Length > MaxFileSizeBytes) return false;

        // Check file extension
        var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedImageExtensions.Contains(extension)) return false;

        // Check content type
        var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedContentTypes.Contains(file.ContentType?.ToLowerInvariant())) return false;

        return true;
    }
}