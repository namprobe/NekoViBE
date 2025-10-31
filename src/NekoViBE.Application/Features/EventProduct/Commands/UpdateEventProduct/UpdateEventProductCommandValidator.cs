using FluentValidation;
using NekoViBE.Application.Common.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.EventProduct.Commands.UpdateEventProduct
{
    public class UpdateEventProductCommandValidator : AbstractValidator<UpdateEventProductCommand>
    {
        public UpdateEventProductCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Event product ID is required");

            RuleFor(x => x.Request)
                .NotNull().WithMessage("Event product request is required");

                this.SetupEventProductRules(x => x.Request);
        }
    }
}
