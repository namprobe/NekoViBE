using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Tag
{
    public class TagResponse : TagItem
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
