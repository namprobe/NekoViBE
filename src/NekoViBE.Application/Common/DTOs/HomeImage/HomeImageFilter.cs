using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.HomeImage
{
    public class HomeImageFilter : BasePaginationFilter
    {
        [JsonPropertyName("search")]
        public string? Search { get; set; }

        [JsonPropertyName("animeSeriesId")]
        public Guid? AnimeSeriesId { get; set; }

        [JsonPropertyName("hasAnimeSeries")]
        public bool? HasAnimeSeries { get; set; }
    }
}
