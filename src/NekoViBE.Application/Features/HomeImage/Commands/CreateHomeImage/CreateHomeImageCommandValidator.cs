using FluentValidation;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.HomeImage.Commands.CreateHomeImage
{
    public class CreateHomeImageCommandValidator : AbstractValidator<CreateHomeImageCommand>
    {
        public CreateHomeImageCommandValidator()
        {
            RuleFor(x => x.Request.ImageFile)
                .ValidHomeImageFile();

            RuleFor(x => x.Request.AnimeSeriesId)
                .Must(id => !id.HasValue || id.Value != Guid.Empty)
                .When(x => x.Request.AnimeSeriesId.HasValue)
                .WithMessage("AnimeSeriesId must be a valid GUID");
        }
    }
}