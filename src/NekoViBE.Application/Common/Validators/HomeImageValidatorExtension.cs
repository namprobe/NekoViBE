using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace NekoViBE.Application.Common.Validators
{
    public static class HomeImageValidatorExtension
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public static IRuleBuilderOptions<T, IFormFile> ValidHomeImageFile<T>(
            this IRuleBuilder<T, IFormFile> ruleBuilder)
        {
            return ruleBuilder
                .NotNull().WithMessage("Image file is required")
                .Must(file => file.Length <= 5 * 1024 * 1024)
                    .WithMessage("Image file size must not exceed 5MB")
                .Must(file => AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                    .WithMessage("Only .jpg, .jpeg, .png, .webp files are allowed");
        }

        // Trong HomeImageValidatorExtension.cs (hoặc tạo mới)
        public static IRuleBuilderOptions<T, string> ValidHomeImageName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Name is required")
                .Length(2, 100).WithMessage("Name must be between 2 and 100 characters");
        }
    }
}