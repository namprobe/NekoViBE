// NekoViBE.Application.Features.PostCategory.Commands.CreatePostCategory/CreatePostCategoryCommandValidator.cs
using FluentValidation;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Validators;

namespace NekoViBE.Application.Features.PostCategory.Commands.CreatePostCategory
{
    public class CreatePostCategoryCommandValidator : AbstractValidator<CreatePostCategoryCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreatePostCategoryCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.Request)
                .NotNull().WithMessage("Request is required");

            When(x => x.Request != null, () =>
            {
                this.SetupPostCategoryRules(
                    requestSelector: x => x.Request,
                    unitOfWork: _unitOfWork,
                    excludeId: null // Create → không loại trừ ID nào
                );
            });
        }
    }
}