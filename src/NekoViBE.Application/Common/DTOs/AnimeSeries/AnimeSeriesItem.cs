using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.AnimeSeries
{
    public class AnimeSeriesItem : BaseResponse
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("releaseYear")]
        public int ReleaseYear { get; set; }

        [JsonPropertyName("imagePath")]
        public string? ImagePath { get; set; }
    }
}
