// NekoViBE.Application.Common.Validators/PostCategoryValidatorExtension.cs
using FluentValidation;
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.Validators
{
    public static class PostCategoryValidatorExtension
    {
        public static IRuleBuilderOptions<T, string> ValidPostCategoryName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Category name is required")
                .Length(2, 100).WithMessage("Name must be between 2 and 100 characters");
        }

        /// <summary>
        /// Kiểm tra Name không trùng với category khác (ngoại trừ chính nó nếu là update)
        /// </summary>
        public static IRuleBuilderOptions<T, string> MustHaveUniqueName<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            IUnitOfWork unitOfWork,
            Guid? excludeId = null) // excludeId: ID của category đang update
        {
            return ruleBuilder
                .MustAsync(async (name, cancellation) =>
                {
                    var exists = await unitOfWork.Repository<PostCategory>()
                        .AnyAsync(x =>
                            !x.IsDeleted &&
                            x.Name.Trim().ToLower() == name.Trim().ToLower() &&
                            (!excludeId.HasValue || x.Id != excludeId.Value));

                    return !exists;
                })
                .WithMessage("Category name already exists");
        }

        public static void SetupPostCategoryRules<T>(this AbstractValidator<T> validator,
            Expression<Func<T, PostCategoryRequest>> requestSelector,
            IUnitOfWork? unitOfWork = null,
            Guid? excludeId = null)
        {
            var req = requestSelector.Compile();

            validator.RuleFor(x => req(x).Name)
                .ValidPostCategoryName();

            // Chỉ thêm kiểm tra trùng tên nếu có unitOfWork
            if (unitOfWork != null)
            {
                validator.RuleFor(x => req(x).Name)
                    .MustHaveUniqueName(unitOfWork, excludeId);
            }

            validator.RuleFor(x => req(x).Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrWhiteSpace(req(x).Description));
        }
    }
}