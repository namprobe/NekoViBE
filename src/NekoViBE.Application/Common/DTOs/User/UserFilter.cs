using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.User
{
    public class UserFilter : BasePaginationFilter
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("status")]
        public EntityStatusEnum? Status { get; set; }

        [JsonPropertyName("roleId")]
        public Guid? RoleId { get; set; }

        [JsonPropertyName("userType")]
        public string? UserType { get; set; } // "Customer", "Staff", "Admin"

        [JsonPropertyName("hasAvatar")]
        public bool? HasAvatar { get; set; }
    }
}
