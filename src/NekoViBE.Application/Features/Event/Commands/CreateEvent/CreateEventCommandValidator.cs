using FluentValidation;
using NekoViBE.Application.Common.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Event.Commands.CreateEvent
{
    public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
    {
        public CreateEventCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull().WithMessage("Event request is required");
                
                this.SetupEventRules(x => x.Request);

            //RuleFor(x => x.Request.StartDate)
            //    .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            //    .WithMessage("Start date must not be earlier than the current date");
        }
    }
}
