using NekoViBE.Application.Common.DTOs.Role;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Auth
{
    public class RoleResponse
    {
        [JsonPropertyName("roles")]
        public List<RoleDTO> Roles { get; set; } = new();
    }
}
