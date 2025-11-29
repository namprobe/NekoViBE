// Application/Features/UserHomeImage/Commands/SaveUserHomeImages/SaveUserHomeImagesCommandValidator.cs
using FluentValidation;
using NekoViBE.Application.Common.DTOs.UserHomeImage;

namespace NekoViBE.Application.Features.UserHomeImage.Commands.SaveUserHomeImages
{
    public class SaveUserHomeImagesCommandValidator : AbstractValidator<SaveUserHomeImagesCommand>
    {
        public SaveUserHomeImagesCommandValidator()
        {
            RuleFor(x => x.Requests)
                .NotNull().WithMessage("Requests are required")
                .Must(x => x.Count <= 3).WithMessage("Maximum 3 images allowed");

            RuleForEach(x => x.Requests).ChildRules(request =>
            {
                request.RuleFor(x => x.HomeImageId)
                    .NotEmpty().WithMessage("HomeImageId is required");
                request.RuleFor(x => x.Position)
                    .InclusiveBetween(1, 3).WithMessage("Position must be 1, 2, or 3");
            });

            RuleFor(x => x.Requests)
                .Must(requests => requests.Select(r => r.Position).Distinct().Count() == requests.Count)
                .WithMessage("Positions must be unique")
                .Must(requests => requests.Select(r => r.HomeImageId).Distinct().Count() == requests.Count)
                .WithMessage("HomeImageIds must be unique");
        }
    }
}