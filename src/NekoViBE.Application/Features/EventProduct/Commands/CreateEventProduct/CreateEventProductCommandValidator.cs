using FluentValidation;
using NekoViBE.Application.Common.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.EventProduct.Commands.CreateEventProduct
{
    public class CreateEventProductCommandValidator : AbstractValidator<CreateEventProductCommand>
    {
        public CreateEventProductCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull().WithMessage("Event product request is required");
        }
    }
}
