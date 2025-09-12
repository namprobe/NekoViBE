using Microsoft.AspNetCore.Http;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Category
{
    public class CategoryRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("parentCategoryId")]
        public Guid? ParentCategoryId { get; set; }

        [JsonPropertyName("status")]
        public EntityStatusEnum Status { get; set; }

        [JsonPropertyName("imageFile")]
        public IFormFile? ImageFile { get; set; }
    }
}
