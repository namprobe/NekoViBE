using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Product
{
    public class ProductFilter : BasePaginationFilter
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("categoryId")]
        public Guid? CategoryId { get; set; }

        [JsonPropertyName("animeSeriesId")]
        public Guid? AnimeSeriesId { get; set; }

        [JsonPropertyName("hasImage")]
        public bool? HasImage { get; set; }
    }
}
