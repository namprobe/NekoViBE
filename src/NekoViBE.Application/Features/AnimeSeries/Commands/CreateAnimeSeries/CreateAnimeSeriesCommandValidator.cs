using FluentValidation;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.AnimeSeries.Commands.CreateAnimeSeries;

public class CreateAnimeSeriesCommandValidator : AbstractValidator<CreateAnimeSeriesCommand>
{
    public CreateAnimeSeriesCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Anime series request is required");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Request.ReleaseYear)
                .GreaterThan(1900)
                .LessThanOrEqualTo(DateTime.UtcNow.Year + 1);

            RuleFor(x => x.Request.ImageFile)
                .Must(file => file == null || file.Length <= 3 * 1024 * 1024) // Max 3MB
                .WithMessage("Image file size must not exceed 3MB")
                .Must(file => file == null || IsValidImageExtension(file.FileName))
                .WithMessage("Invalid image file format. Supported formats: .jpg, .png, .jpeg");
        });
    }

    private bool IsValidImageExtension(string fileName)
    {
        var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
        var extension = Path.GetExtension(fileName).ToLower();
        return allowedExtensions.Contains(extension);
    }
}