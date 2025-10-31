using FluentValidation;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.ProductTag.Commands.UpdateProductTag;

public class UpdateProductTagValidator : AbstractValidator<UpdateProductTagCommand>
{
    public UpdateProductTagValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ProductTag ID is required");

        RuleFor(x => x.Request)
            .NotNull().WithMessage("ProductTag request is required");

        When(x => x.Request != null, () =>
        {
            this.SetupProductTagRules(x => x.Request);

            // Kiểm tra tính duy nhất của cặp ProductId và TagId
            RuleFor(x => x.Request)
                .MustAsync(async (command, request, cancellation) =>
                    !await unitOfWork.Repository<Domain.Entities.ProductTag>().AnyAsync(pt =>
                        pt.ProductId == request.ProductId &&
                        pt.TagId == request.TagId &&
                        pt.Id != command.Id))
                .WithMessage("ProductTag relationship already exists");
        });
    }
}