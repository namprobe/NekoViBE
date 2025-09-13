using FluentValidation;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Validators
{
    public static class CategoryValidatorExtension
    {
        /// <summary>
        /// Validates category name with business rules
        /// Required | 2-150 chars
        /// </summary>
        public static IRuleBuilderOptions<T, string> ValidCategoryName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Category name is required")
                .Length(2, 150).WithMessage("Category name must be between 2 and 150 characters");
        }

        /// <summary>
        /// Validates entity status (specific for Category to avoid ambiguity)
        /// </summary>
        public static IRuleBuilderOptions<T, EntityStatusEnum> ValidCategoryStatus<T>(this IRuleBuilder<T, EntityStatusEnum> ruleBuilder)
        {
            return ruleBuilder
                .IsInEnum().WithMessage("Invalid status value");
        }

        /// <summary>
        /// Validates image file with business rules
        /// Optional | Max 3MB | Supported formats: .jpg, .png, .jpeg
        /// </summary>
        public static IRuleBuilderOptions<T, Microsoft.AspNetCore.Http.IFormFile> ValidCategoryImageFile<T>(this IRuleBuilder<T, Microsoft.AspNetCore.Http.IFormFile> ruleBuilder)
        {
            return ruleBuilder
                .Must(file => file == null || file.Length <= 3 * 1024 * 1024)
                .WithMessage("Image file size must not exceed 3MB")
                .Must(file => file == null || IsValidImageExtension(file.FileName))
                .WithMessage("Invalid image file format. Supported formats: .jpg, .png, .jpeg");
        }

        /// <summary>
        /// Sets up CategoryRequest validation with business rules
        /// </summary>
        public static void SetupCategoryRules<T>(this AbstractValidator<T> validator,
            Expression<Func<T, CategoryRequest>> requestSelector)
        {
            var requestFunc = requestSelector.Compile();

            // Name
            validator.RuleFor(x => requestFunc(x).Name)
                .ValidCategoryName();

            // Status
            validator.RuleFor(x => requestFunc(x).Status)
                .ValidCategoryStatus();

            // Description (optional)
            validator.RuleFor(x => requestFunc(x).Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
                .When(x => !string.IsNullOrWhiteSpace(requestFunc(x).Description));

            // ParentCategoryId (optional)
            validator.RuleFor(x => requestFunc(x).ParentCategoryId)
                .NotEmpty().When(x => requestFunc(x).ParentCategoryId.HasValue)
                .WithMessage("Parent category ID must be valid");

            // ImageFile (optional)
            validator.RuleFor(x => requestFunc(x).ImageFile)
                .ValidCategoryImageFile();
        }

        private static bool IsValidImageExtension(string fileName)
        {
            var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
            var extension = Path.GetExtension(fileName).ToLower();
            return allowedExtensions.Contains(extension);
        }
    }
}
