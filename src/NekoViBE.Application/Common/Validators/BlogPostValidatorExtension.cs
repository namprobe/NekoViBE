// File: Application/Common/Validators/BlogPostValidatorExtension.cs
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
                .Length(2, 200).WithMessage("Title must be 2-200 characters");
        }

        public static IRuleBuilderOptions<T, string> ValidBlogPostContent<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Content is required")
                .MaximumLength(10000).WithMessage("Content too long");
        }

        public static void SetupBlogPostRules<T>(this AbstractValidator<T> validator,
            Expression<Func<T, BlogPostRequest>> requestSelector)
        {
            var req = requestSelector.Compile();

            validator.RuleFor(x => req(x).Title).ValidBlogPostTitle();
            validator.RuleFor(x => req(x).Content).ValidBlogPostContent();
            validator.RuleFor(x => req(x).Status).IsInEnum();

            validator.RuleFor(x => req(x).FeaturedImageFile)
                .Must(f => f == null || f.Length <= 10 * 1024 * 1024)
                .WithMessage("Image <= 10MB")
                .Must(f => f == null || new[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(f.FileName).ToLower()))
                .WithMessage("Only .jpg, .jpeg, .png");
        }
    }
}