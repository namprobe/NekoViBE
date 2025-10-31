using FluentValidation;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductTag.Commands.CreateProductTag
{
    public class CreateProductTagValidator : AbstractValidator<CreateProductTagCommand>
    {
        public CreateProductTagValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Request)
                .NotNull().WithMessage("ProductTag request is required");

            When(x => x.Request != null, () =>
            {
                this.SetupProductTagRules(x => x.Request);

                // Kiểm tra tính duy nhất của cặp ProductId và TagId
                RuleFor(x => x.Request)
                    .MustAsync(async (request, cancellation) =>
                        !await unitOfWork.Repository<Domain.Entities.ProductTag>().AnyAsync(x => x.ProductId == request.ProductId && x.TagId == request.TagId))
                    .WithMessage("ProductTag relationship already exists");
            });
        }
    }
}
