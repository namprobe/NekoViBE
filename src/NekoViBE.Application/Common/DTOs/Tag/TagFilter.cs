using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Tag
{
    public class TagFilter : BasePaginationFilter
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
