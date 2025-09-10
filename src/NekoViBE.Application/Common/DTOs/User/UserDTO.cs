using NekoViBE.Application.Common.DTOs.Role;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.User
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime JoiningAt { get; set; }
        public string? AvatarPath { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public EntityStatusEnum Status { get; set; }
        public string StatusName => Status.ToString();
        public List<RoleInfoDTO> Roles { get; set; } = new(); // Changed to include role info
    }
}
