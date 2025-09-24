using FluentValidation;
using Microsoft.AspNetCore.Http;
using NekoViBE.Application.Common.DTOs.ProductImage;
using NekoViBE.Domain.Enums;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.Validators
{
    public static class ProductImageValidatorExtension
    {

        public static IRuleBuilderOptions<T, IFormFile> ValidImageFile<T>(this IRuleBuilder<T, IFormFile> ruleBuilder)
        {
            return ruleBuilder
                .NotNull().WithMessage("Image file is required")
                .Must(file => file != null && new[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(file.FileName).ToLower()))
                .WithMessage("Image must be a .jpg, .jpeg, or .png file")
                .Must(file => file != null && file.Length <= 10 * 1024 * 1024) // 10MB limit
                .WithMessage("Image size must not exceed 10MB");
        }

        public static IRuleBuilderOptions<T, int> ValidDisplayOrder<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");
        }

        public static IRuleBuilderOptions<T, EntityStatusEnum> ValidProductImageStatus<T>(this IRuleBuilder<T, EntityStatusEnum> ruleBuilder)
        {
            return ruleBuilder
                .IsInEnum().WithMessage("Invalid status value");
        }

        public static void SetupProductImageRules<T>(this AbstractValidator<T> validator,
            Expression<Func<T, ProductImageRequest>> requestSelector, bool isImageRequired = true)
        {
            var requestFunc = requestSelector.Compile();

            validator.RuleFor(x => requestFunc(x).ProductId)
                .ValidProductId();

            if (isImageRequired)

            {

                validator.RuleFor(x => requestFunc(x).Image)

                    .ValidImageFile();

            }

            else

            {

                validator.RuleFor(x => requestFunc(x).Image)

                    .Must(file => file == null || new[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(file.FileName).ToLower()))

                    .WithMessage("Image must be a .jpg, .jpeg, or .png file")

                    .When(x => requestFunc(x).Image != null)

                    .Must(file => file == null || file.Length <= 10 * 1024 * 1024)

                    .WithMessage("Image size must not exceed 10MB")

                    .When(x => requestFunc(x).Image != null);

            }

            validator.RuleFor(x => requestFunc(x).DisplayOrder)
                .ValidDisplayOrder();

            validator.RuleFor(x => requestFunc(x).Status)
                .ValidProductImageStatus();
        }
    }
}
