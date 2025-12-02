using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.UserHomeImage
{
    public class UserHomeImageRequest
    {
        [JsonPropertyName("homeImageId")]
        public Guid HomeImageId { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; } // 1, 2 hoặc 3
    }
}
