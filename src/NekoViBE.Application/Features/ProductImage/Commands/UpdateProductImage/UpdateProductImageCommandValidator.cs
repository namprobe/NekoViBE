using FluentValidation;
using NekoViBE.Application.Common.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductImage.Commands.UpdateProductImage
{
    public class UpdateProductImageCommandValidator : AbstractValidator<UpdateProductImageCommand>
    {
        public UpdateProductImageCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Product image ID is required");

            RuleFor(x => x.Request)
                .NotNull().WithMessage("Product image request is required");
                this.SetupProductImageRules(x => x.Request, isImageRequired: false);
        }
    }
}
