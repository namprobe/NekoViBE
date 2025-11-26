// UpdateHomeImageCommandValidator.cs
using FluentValidation;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.HomeImage.Commands.UpdateHomeImage
{
    public class UpdateHomeImageCommandValidator : AbstractValidator<UpdateHomeImageCommand>
    {
        public UpdateHomeImageCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request.ImageFile)
                .ValidHomeImageFile()
                .When(x => x.Request.ImageFile != null);

            RuleFor(x => x.Request.AnimeSeriesId)
                .Must(id => !id.HasValue || id.Value != Guid.Empty)
                .When(x => x.Request.AnimeSeriesId.HasValue);
        }
    }
}