using FluentValidation;

namespace NekoViBE.Application.Features.AnimeSeries.Commands.UpdateAnimeSeries;

public class UpdateAnimeSeriesCommandValidator : AbstractValidator<UpdateAnimeSeriesCommand>
{
    public UpdateAnimeSeriesCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Request.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Request.ReleaseYear)
            .GreaterThan(1900)
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 1);
    }
}
