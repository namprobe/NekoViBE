using FluentValidation;
using NekoViBE.Application.Common.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Product.Commands.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull().WithMessage("Product request is required");

            this.SetupProductRules(x => x.Request);
        }
    }
}
