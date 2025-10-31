using FluentValidation;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.Category.Commands.CreateCategory
{
    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull().WithMessage("Category request is required");
                
            this.SetupCategoryRules(x => x.Request);
        }
    }
}
