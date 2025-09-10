using MediatR;
using NekoViBE.Application.Common.DTOs.User;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.User.Commands.CreateUser
{
    public record CreateUserCommand : IRequest<Result<UserDTO>>
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public List<Guid> RoleIds { get; set; } = new(); // Changed from role names to role IDs

        public string? AvatarPath { get; set; }

        public EntityStatusEnum Status { get; set; } = EntityStatusEnum.Active;
    }
}
