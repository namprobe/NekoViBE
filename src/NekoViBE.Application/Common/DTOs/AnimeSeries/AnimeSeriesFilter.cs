using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.AnimeSeries
{
    public class AnimeSeriesFilter : BasePaginationFilter
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("releaseYear")]
        public int? ReleaseYear { get; set; }
    }
}
