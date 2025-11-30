using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.UserHomeImage
{
    public class UserHomeImageFilter : BasePaginationFilter
    {
        [JsonPropertyName("userId")]
        public Guid? UserId { get; set; }
    }
}
