// SaveEventProductsCommandValidator.cs
using FluentValidation;

namespace NekoViBE.Application.Features.Event.Commands.SaveEventProducts
{
    public class SaveEventProductsCommandValidator : AbstractValidator<SaveEventProductsCommand>
    {
        public SaveEventProductsCommandValidator()
        {
            RuleFor(x => x.EventId).NotEmpty();
            RuleFor(x => x.Requests)
                .NotNull()
                .Must(r => r.Count <= 20).WithMessage("Maximum 20 products per event");

            RuleForEach(x => x.Requests).ChildRules(req =>
            {
                req.RuleFor(x => x.ProductId).NotEmpty();
                req.RuleFor(x => x.DiscountPercentage)
                    .InclusiveBetween(0, 100)
                    .WithMessage("DiscountPercentage must be between 0 and 100");
            });

            RuleFor(x => x.Requests)
                .Must(r => r.Select(x => x.ProductId).Distinct().Count() == r.Count)
                .WithMessage("Duplicate ProductId is not allowed");
        }
    }
}