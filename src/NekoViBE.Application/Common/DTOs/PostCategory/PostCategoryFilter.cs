using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.PostCategory
{
    public class PostCategoryFilter : BasePaginationFilter
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
