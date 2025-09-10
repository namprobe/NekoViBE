using FluentValidation;

namespace NekoViBE.Application.Features.AnimeSeries.Commands.CreateAnimeSeries;

public class CreateAnimeSeriesCommandValidator : AbstractValidator<CreateAnimeSeriesCommand>
{
    public CreateAnimeSeriesCommandValidator()
    {
        RuleFor(x => x.Request.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Request.ReleaseYear)
            .GreaterThan(1900)
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 1);
    }
}
