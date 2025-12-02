using FluentValidation;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Domain.Enums;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.Validators;

public static class TagValidatorExtension
{
    // Xác thực tên Tag
    public static IRuleBuilderOptions<T, string> ValidTagName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Tag name is required")
            .Length(1, 50).WithMessage("Tag name must be between 1 and 50 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_]+$").WithMessage("Tag name can only contain letters, numbers, spaces, hyphens, and underscores");
    }

    // Thiết lập các quy tắc xác thực cho TagRequest
    public static void SetupTagRules<T>(this AbstractValidator<T> validator,
        Expression<Func<T, TagRequest>> requestSelector)
    {
        var requestFunc = requestSelector.Compile();

        validator.RuleFor(x => requestFunc(x).Name)
            .ValidTagName();

        validator.RuleFor(x => requestFunc(x).Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(requestFunc(x).Description));

        validator.RuleFor(x => requestFunc(x).Status)
            .IsInEnum().WithMessage("Invalid status value");
    }
}