using FluentValidation;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Tag.Commands.UpdateTag
{
    public class UpdateTagValidator : AbstractValidator<UpdateTagCommand>
    {
        public UpdateTagValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Tag ID is required");

            RuleFor(x => x.Request)
                .NotNull().WithMessage("Tag request is required");

            When(x => x.Request != null, () =>
            {
                this.SetupTagRules(x => x.Request);

                // Kiểm tra tính duy nhất của tên Tag
                RuleFor(x => x.Request.Name)
                    .MustAsync(async (command, name, cancellation) =>
                        !await unitOfWork.Repository<Domain.Entities.Tag>().AnyAsync(x => x.Name == name && x.Id != command.Id))
                    .WithMessage("Tag name must be unique");
            });
        }
    }
}
