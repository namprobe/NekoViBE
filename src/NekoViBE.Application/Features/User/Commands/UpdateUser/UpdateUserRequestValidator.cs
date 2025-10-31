using FluentValidation;
using NekoViBE.Application.Common.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.User.Commands.UpdateUser
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
                .Matches("^[a-zA-Z]+$").WithMessage("First name can only contain letters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
                .Matches("^[a-zA-Z]+$").WithMessage("Last name can only contain letters");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.RoleIds)
                .NotEmpty().WithMessage("At least one role is required");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status value");
        }
    }
}
