using FluentValidation;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Domain.Enums;
using System;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.Validators;

public static class AnimeSeriesValidatorExtension
{
    /// <summary>
    /// Validates anime series title with business rules
    /// Required | 2-150 chars
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidAnimeSeriesTitle<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Anime series title is required")
            .Length(2, 150).WithMessage("Anime series title must be between 2 and 150 characters");
    }

    /// <summary>
    /// Validates entity status (specific for AnimeSeries to avoid ambiguity)
    /// </summary>
    public static IRuleBuilderOptions<T, EntityStatusEnum> ValidAnimeSeriesStatus<T>(this IRuleBuilder<T, EntityStatusEnum> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum().WithMessage("Invalid status value");
    }

    /// <summary>
    /// Sets up AnimeSeriesRequest validation with business rules
    /// </summary>
    public static void SetupAnimeSeriesRules<T>(this AbstractValidator<T> validator,
        Expression<Func<T, AnimeSeriesRequest>> requestSelector)
    {
        var requestFunc = requestSelector.Compile();

        // Title
        validator.RuleFor(x => requestFunc(x).Title)
            .ValidAnimeSeriesTitle();

        // Status
        validator.RuleFor(x => requestFunc(x).Status)
            .ValidAnimeSeriesStatus();

        // Description (optional)
        validator.RuleFor(x => requestFunc(x).Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(requestFunc(x).Description));

        // ReleaseYear (optional rule: 1900 -> current year + 1)
        validator.RuleFor(x => requestFunc(x).ReleaseYear)
            .GreaterThanOrEqualTo(1900).WithMessage("Release year must be >= 1900")
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 1).WithMessage("Release year cannot be in far future");
    }
}
