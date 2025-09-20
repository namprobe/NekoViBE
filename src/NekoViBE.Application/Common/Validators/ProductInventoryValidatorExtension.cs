using FluentValidation;
using NekoViBE.Application.Common.DTOs.ProductInventory;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Validators
{
    public static class ProductInventoryValidatorExtension
    {
        public static IRuleBuilderOptions<T, Guid> ValidProductId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Product ID is required");
        }

        public static IRuleBuilderOptions<T, int> ValidQuantity<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder
                .GreaterThanOrEqualTo(1).WithMessage("Quantity must be greater than 0");
        }

        public static IRuleBuilderOptions<T, EntityStatusEnum> ValidProductInventoryStatus<T>(this IRuleBuilder<T, EntityStatusEnum> ruleBuilder)
        {
            return ruleBuilder
                .IsInEnum().WithMessage("Invalid status value");
        }

        public static void SetupProductInventoryRules<T>(this AbstractValidator<T> validator,
            Expression<Func<T, ProductInventoryRequest>> requestSelector)
        {
            var requestFunc = requestSelector.Compile();

            validator.RuleFor(x => requestFunc(x).ProductId)
                .ValidProductId();

            validator.RuleFor(x => requestFunc(x).Quantity)
                .ValidQuantity();

            validator.RuleFor(x => requestFunc(x).Status)
                .ValidProductInventoryStatus();
        }
    }
}
