using FluentValidation;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.ProductInventory.Commands.CreateProductInventory
{
    public class CreateProductInventoryCommandValidator : AbstractValidator<CreateProductInventoryCommand>
    {
        public CreateProductInventoryCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull().WithMessage("Product inventory request is required");

                this.SetupProductInventoryRules(x => x.Request);
        }
    }
}