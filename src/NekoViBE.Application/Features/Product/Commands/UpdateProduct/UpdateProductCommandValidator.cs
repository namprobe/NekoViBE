using FluentValidation;
using NekoViBE.Application.Common.Validators;
using System;

namespace NekoViBE.Application.Features.Product.Commands.UpdateProduct
{
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Product ID is required");

            RuleFor(x => x.Request)
                .NotNull().WithMessage("Product request is required");

            this.SetupProductRulesUpdate(x => x.Request);
        }
    }
}