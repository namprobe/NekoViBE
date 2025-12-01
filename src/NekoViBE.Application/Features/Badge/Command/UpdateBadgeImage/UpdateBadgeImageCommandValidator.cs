using FluentValidation;

namespace NekoViBE.Application.Features.Badge.Command.UpdateBadgeImage
{
    public class UpdateBadgeImageCommandValidator : AbstractValidator<UpdateBadgeImageCommand>
    {
        public UpdateBadgeImageCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Badge ID is required");

            RuleFor(x => x.Request.IconPath)
                .NotNull()
                .WithMessage("Icon image is required")
                .Must(file => file != null && file.Length > 0)
                .WithMessage("Icon image file must not be empty")
                .Must(file => file == null || file.Length <= 5 * 1024 * 1024)
                .WithMessage("Icon image file size must not exceed 5MB")
                .Must(file =>
                {
                    if (file == null) return true;
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    return allowedExtensions.Contains(extension);
                })
                .WithMessage("Icon image must be a valid image file (jpg, jpeg, png, gif, webp)");
        }
    }
}
