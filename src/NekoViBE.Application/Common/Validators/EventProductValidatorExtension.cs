using FluentValidation;
using NekoViBE.Application.Common.DTOs.EventProduct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Validators
{
    public static class EventProductValidatorExtension
    {
        public static void SetupEventProductRules<T>(this AbstractValidator<T> validator, Expression<Func<T, EventProductRequest>> requestSelector)
        {
            var requestFunc = requestSelector.Compile();

            validator.RuleFor(x => requestFunc(x).EventId)
                .NotEmpty().WithMessage("Event ID is required");

            validator.RuleFor(x => requestFunc(x).ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            validator.RuleFor(x => requestFunc(x).DiscountPercentage)
                .InclusiveBetween(0, 100).WithMessage("Discount percentage must be between 0 and 100");
        }
    }
}
