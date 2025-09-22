using FluentValidation;
using NekoViBE.Application.Common.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductImage.Commands.CreateProductImage
{
    public class CreateProductImageCommandValidator : AbstractValidator<CreateProductImageCommand>
    {
        public CreateProductImageCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull().WithMessage("Product image request is required");
                
            this.SetupProductImageRules(x => x.Request, isImageRequired: true);
        }
    }
}
