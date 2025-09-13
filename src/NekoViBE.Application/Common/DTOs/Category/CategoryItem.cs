using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Category
{
    public class CategoryItem : BaseResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("parentCategoryId")]
        public Guid? ParentCategoryId { get; set; }

        [JsonPropertyName("imagePath")]
        public string? ImagePath { get; set; }
    }
}
