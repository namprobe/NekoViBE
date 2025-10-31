using FluentValidation;
using NekoViBE.Application.Common.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductInventory.Commands.UpdateProductInventory
{
    public class UpdateProductInventoryCommandValidator : AbstractValidator<UpdateProductInventoryCommand>
    {
        public UpdateProductInventoryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Product inventory ID is required");

            this.SetupProductInventoryRules(x => x.Request);

            RuleFor(x => x.Request)
                .NotNull().WithMessage("Product inventory request is required");
        }
    }
}
