using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.HomeImage
{
    public class UpdateHomeImageDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("imageFile")]
        public IFormFile? ImageFile { get; set; } // Optional

        [JsonPropertyName("existingImagePath")]
        public string? ExistingImagePath { get; set; } // Nhận từ FE

        [JsonPropertyName("animeSeriesId")]
        public string? AnimeSeriesId { get; set; }
    }
}
