using NekoViBE.Application.Common.DTOs.Role;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.User
{
    public class UserItem : BaseResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("avatarPath")]
        public string? AvatarPath { get; set; }

        [JsonPropertyName("status")]
        public EntityStatusEnum Status { get; set; }

        [JsonPropertyName("lastLoginAt")]
        public DateTime? LastLoginAt { get; set; }

        [JsonPropertyName("joiningAt")]
        public DateTime JoiningAt { get; set; }

        [JsonPropertyName("roles")]
        public List<RoleInfoDTO> Roles { get; set; } = new List<RoleInfoDTO>();

        [JsonPropertyName("userType")]
        public string UserType { get; set; } = "User"; // Customer, Staff, Admin
    }
}
