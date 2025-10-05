using FluentValidation;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Domain.Enums;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.Validators
{
    public static class ProductValidatorExtension
    {
        public static IRuleBuilderOptions<T, string> ValidProductName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Product name is required")
                .Length(2, 150).WithMessage("Product name must be between 2 and 150 characters");
        }

        public static IRuleBuilderOptions<T, EntityStatusEnum> ValidProductStatus<T>(this IRuleBuilder<T, EntityStatusEnum> ruleBuilder)
        {
            return ruleBuilder
                .IsInEnum().WithMessage("Invalid status value");
        }

        public static IRuleBuilderOptions<T, Microsoft.AspNetCore.Http.IFormFile> ValidProductImageFile<T>(this IRuleBuilder<T, Microsoft.AspNetCore.Http.IFormFile> ruleBuilder)
        {
            return ruleBuilder
                .Must(file => file == null || file.Length <= 10 * 1024 * 1024)
                .WithMessage("Image file size must not exceed 10MB")
                .Must(file => file == null || IsValidImageExtension(file.FileName))
                .WithMessage("Invalid image file format. Supported formats: .jpg, .png, .jpeg");
        }

        public static void SetupProductRules<T>(this AbstractValidator<T> validator,
            Expression<Func<T, ProductRequest>> requestSelector)
        {
            var requestFunc = requestSelector.Compile();

            validator.RuleFor(x => requestFunc(x).Name)
                .ValidProductName();

            validator.RuleFor(x => requestFunc(x).Status)
                .ValidProductStatus();

            validator.RuleFor(x => requestFunc(x).Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
                .When(x => !string.IsNullOrWhiteSpace(requestFunc(x).Description));

            validator.RuleFor(x => requestFunc(x).Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            validator.RuleFor(x => requestFunc(x).DiscountPrice)
                .LessThanOrEqualTo(x => requestFunc(x).Price).When(x => requestFunc(x).DiscountPrice.HasValue)
                .WithMessage("Discount price must not exceed original price");

            validator.RuleFor(x => requestFunc(x).StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be non-negative");

            validator.RuleFor(x => requestFunc(x).CategoryId)
                .NotEmpty().WithMessage("Category ID is required");

            validator.RuleFor(x => requestFunc(x).AnimeSeriesId)
                .NotEmpty().When(x => requestFunc(x).AnimeSeriesId.HasValue)
                .WithMessage("Anime series ID must be valid");

            // Quy tắc cho IsPreOrder và PreOrderReleaseDate
            validator.RuleFor(x => requestFunc(x).PreOrderReleaseDate)
                .NotNull().When(x => requestFunc(x).IsPreOrder)
                .WithMessage("Pre-order release date is required when IsPreOrder is true")
                .GreaterThan(DateTime.UtcNow).When(x => requestFunc(x).IsPreOrder)
                .WithMessage("Pre-order release date must be in the future");

            validator.RuleFor(x => requestFunc(x).PreOrderReleaseDate)
                .Null().When(x => !requestFunc(x).IsPreOrder)
                .WithMessage("Pre-order release date must be null when IsPreOrder is false");

            validator.RuleFor(x => requestFunc(x).ImageFiles)
            .Must(files => files == null || files.All(f => f.Length <= 10 * 1024 * 1024))
            .WithMessage("Each image must not exceed 10MB")
            .Must(files => files == null || files.All(f => new[] { ".jpg", ".jpeg", ".png" }
                .Contains(Path.GetExtension(f.FileName).ToLower())))
            .WithMessage("Only .jpg, .jpeg, .png formats are allowed");

        }

        private static bool IsValidImageExtension(string fileName)
        {
            var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
            var extension = Path.GetExtension(fileName).ToLower();
            return allowedExtensions.Contains(extension);
        }
    }
}