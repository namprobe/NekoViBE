using NekoViBE.Application.Common.DTOs.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Product
{
    public class ProductItem : BaseResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("categoryId")]
        public Guid CategoryId { get; set; }

        [JsonPropertyName("category")]
        public CategoryItem? Category { get; set; } // Thêm thuộc tính Category

        [JsonPropertyName("animeSeriesId")]
        public Guid? AnimeSeriesId { get; set; }
        [JsonPropertyName("stockQuantity")]
        public int StockQuantity { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }
}
