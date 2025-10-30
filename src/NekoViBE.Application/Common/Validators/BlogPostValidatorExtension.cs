// NekoViBE.Application.Common.Validators/BlogPostValidatorExtension.cs
using FluentValidation;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Domain.Enums;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.Validators
{
    public static class BlogPostValidatorExtension
    {
        public static IRuleBuilderOptions<T, string> ValidBlogPostTitle<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Title is required")
                .Length(5, 200).WithMessage("Title must be 5-200 characters");
        }

        public static IRuleBuilderOptions<T, string> ValidBlogPostContent<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Content is required")
                .MinimumLength(50).WithMessage("Content must be at least 50 characters");
        }

        public static void SetupBlogPostRules<T>(this AbstractValidator<T> validator,
            Expression<Func<T, BlogPostRequest>> requestSelector)
        {
            var req = requestSelector.Compile();

            validator.RuleFor(x => req(x).Title).ValidBlogPostTitle();
            validator.RuleFor(x => req(x).Content).ValidBlogPostContent();
            validator.RuleFor(x => req(x).PostCategoryId).NotEmpty().WithMessage("Category is required");
            validator.RuleFor(x => req(x).TagNames).NotEmpty().WithMessage("At least one tag is required");
            validator.RuleFor(x => req(x).FeaturedImageFile)
                .Must(file => file == null || file.Length <= 5 * 1024 * 1024)
                .WithMessage("Image must not exceed 5MB")
                .Must(file => file == null || new[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(file.FileName).ToLower()))
                .WithMessage("Only .jpg, .jpeg, .png allowed");
        }
    }
}