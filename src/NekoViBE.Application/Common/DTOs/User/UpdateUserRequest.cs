using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.User
{

    public class UpdateUserRequest
    {
  
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? AvatarPath { get; set; }

        [Required]
        public List<Guid> RoleIds { get; set; } = new();

        [Required]
        public EntityStatusEnum Status { get; set; }
    }
}
