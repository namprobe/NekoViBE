// Application/Features/UserHomeImage/Commands/CreateUserHomeImage/CreateUserHomeImageCommandValidator.cs
using FluentValidation;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.UserHomeImage.Commands.CreateUserHomeImage
{
    public class CreateUserHomeImageCommandValidator : AbstractValidator<CreateUserHomeImageCommand>
    {
        public CreateUserHomeImageCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull().WithMessage("Request is required");

            RuleFor(x => x.Request.HomeImageId)
                .NotEmpty().WithMessage("HomeImageId is required");

            RuleFor(x => x.Request.Position)
                .InclusiveBetween(1, 3).WithMessage("Position must be 1, 2 or 3");

            // Dùng extension nếu bạn muốn tách riêng
            // this.SetupUserHomeImageRules(x => x.Request);
        }
    }
}