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

        [JsonPropertyName("priceRange")]
        public string? PriceRange { get; set; }  // "under-500k", "500k-1m", "1m-2m", "over-2m"

        [JsonPropertyName("sortType")]
        public string? SortType { get; set; }    // "price-asc", "price-desc", "name-asc", "name-desc"

        [JsonPropertyName("tagIds")]
        public List<Guid>? TagIds { get; set; }

        [JsonPropertyName("stockStatus")]
        public string? StockStatus { get; set; }
    }
}
