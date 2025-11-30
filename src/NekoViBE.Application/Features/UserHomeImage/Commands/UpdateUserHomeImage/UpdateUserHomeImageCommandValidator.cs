// Validator
using FluentValidation;

namespace NekoViBE.Application.Features.UserHomeImage.Commands.UpdateUserHomeImage
{
    public class UpdateUserHomeImageCommandValidator : AbstractValidator<UpdateUserHomeImageCommand>
    {
        public UpdateUserHomeImageCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required");

            RuleFor(x => x.Request)
                .NotNull().WithMessage("Request is required");

            RuleFor(x => x.Request.HomeImageId)
                .NotEmpty().WithMessage("HomeImageId is required");

            RuleFor(x => x.Request.Position)
                .InclusiveBetween(1, 3).WithMessage("Position must be 1, 2 or 3");
        }
    }
}