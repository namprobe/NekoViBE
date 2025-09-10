using MediatR;
using NekoViBE.Application.Common.DTOs.Role;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Auth.Commands.Role
{
    public record CreateRoleCommand : IRequest<Result<RoleDTO>>
    {
        [Required]
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public EntityStatusEnum Status { get; set; } = EntityStatusEnum.Active;
    }

    public class CreateRoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NormalizedName { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public EntityStatusEnum Status { get; set; }
    public string StatusName => Status.ToString();
}
}
