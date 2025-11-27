using FluentValidation;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.HomeImage.Commands.UpdateHomeImage
{
    public class UpdateHomeImageCommandValidator : AbstractValidator<UpdateHomeImageCommand>
    {
        public UpdateHomeImageCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request.Name).ValidHomeImageName();

            // Chỉ validate file nếu có upload
            RuleFor(x => x.Request.ImageFile)
                .ValidHomeImageFile()
                .When(x => x.Request.ImageFile != null);

            // AnimeSeriesId phải hợp lệ nếu có
            RuleFor(x => x.Request.AnimeSeriesId)
                .Must(id => string.IsNullOrEmpty(id) || Guid.TryParse(id, out _))
                .WithMessage("AnimeSeriesId must be a valid GUID or empty");
        }
    }
}