using FluentValidation;
using NekoViBE.Application.Common.DTOs.ProductTag;
using NekoViBE.Domain.Enums;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.Validators
{
    public static class ProductTagValidatorExtension
    {
        public static IRuleBuilderOptions<T, Guid> ValidProductTagProductId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Product ID is required");
        }

        public static IRuleBuilderOptions<T, Guid> ValidTagId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Tag ID is required");
        }

        public static void SetupProductTagRules<T>(this AbstractValidator<T> validator,
            Expression<Func<T, ProductTagRequest>> requestSelector)
        {
            var requestFunc = requestSelector.Compile();

            validator.RuleFor(x => requestFunc(x).ProductId)
                .ValidProductTagProductId(); // Sử dụng phương thức đã đổi tên

            validator.RuleFor(x => requestFunc(x).TagId)
                .ValidTagId();

            validator.RuleFor(x => requestFunc(x).Status)
                .IsInEnum().WithMessage("Invalid status value");
        }
    }
}
