using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Category
{
    public class CategoryFilter : BasePaginationFilter
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("parentCategoryId")]
        public Guid? ParentCategoryId { get; set; }

        [JsonPropertyName("hasImage")]
        public bool? HasImage { get; set; }
    }
}
