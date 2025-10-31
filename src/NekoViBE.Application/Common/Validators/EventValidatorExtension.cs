using FluentValidation;
using NekoViBE.Application.Common.DTOs.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Validators
{
    public static class EventValidatorExtension
    {
        public static IRuleBuilderOptions<T, string> ValidEventName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Event name is required");
        }

        public static void SetupEventRules<T>(this AbstractValidator<T> validator, Expression<Func<T, EventRequest>> requestSelector)
        {
            var requestFunc = requestSelector.Compile();

            validator.RuleFor(x => requestFunc(x).Name)
                .ValidEventName();

            validator.RuleFor(x => requestFunc(x).Status)
                .ValidAnimeSeriesStatus(); // Reuse existing status validator

            validator.RuleFor(x => requestFunc(x).StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Start date must not be earlier than the current date")
                .LessThanOrEqualTo(x => requestFunc(x).EndDate).WithMessage("Start date must be before or equal to end date");

            validator.RuleFor(x => requestFunc(x).EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThanOrEqualTo(x => requestFunc(x).StartDate).WithMessage("End date must be after or equal to start date");

            validator.RuleFor(x => requestFunc(x).Location).NotEmpty().WithMessage("Event's location is required");

            validator.RuleFor(x => requestFunc(x).ImageFile)
                .Must(file => file == null || file.Length <= 10 * 1024 * 1024).WithMessage("Image file size must not exceed 10MB")
                .Must(file => file == null || IsValidImageExtension(file.FileName)).WithMessage("Invalid image file format. Supported formats: .jpg, .png, .jpeg")
                .When(x => requestFunc(x).ImageFile != null);
        }

        private static bool IsValidImageExtension(string fileName)
        {
            var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
            var extension = Path.GetExtension(fileName).ToLower();
            return allowedExtensions.Contains(extension);
        }
    }
}
