using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.HomeImage
{
    public class HomeImageItem : BaseResponse
    {
        [JsonPropertyName("imagePath")]
        public string ImagePath { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("animeSeriesId")]
        public Guid? AnimeSeriesId { get; set; }

        [JsonPropertyName("animeSeriesName")]
        public string? AnimeSeriesName { get; set; }
    }
}
