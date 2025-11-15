// NekoViBE.Application.Features.PostCategory.Commands.UpdatePostCategory/UpdatePostCategoryCommandValidator.cs
using FluentValidation;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Validators;
using Microsoft.EntityFrameworkCore;

namespace NekoViBE.Application.Features.PostCategory.Commands.UpdatePostCategory;

public class UpdatePostCategoryCommandValidator : AbstractValidator<UpdatePostCategoryCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePostCategoryCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required");

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Post category request is required");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.Name)
                .ValidPostCategoryName()
                .MustAsync(async (command, name, cancellation) =>
                {
                    var excludeId = command.Id;

                    return !await _unitOfWork.Repository<Domain.Entities.PostCategory>()
                        .GetQueryable()
                        .AnyAsync(pc =>
                            !pc.IsDeleted &&
                            EF.Functions.Like(pc.Name, name.Trim()) && // Dùng LIKE để không cần ToLower
                            pc.Id != excludeId,
                            cancellation);
                })
                .WithMessage("Category name already exists");

            RuleFor(x => x.Request.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Description));
        });
    }
}