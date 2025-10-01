using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Product
{
    public class ProductResponse : ProductItem
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("discountPrice")]
        public decimal? DiscountPrice { get; set; }

        [JsonPropertyName("stockQuantity")]
        public int StockQuantity { get; set; }

        [JsonPropertyName("isPreOrder")]
        public bool IsPreOrder { get; set; }

        [JsonPropertyName("preOrderReleaseDate")]
        public DateTime? PreOrderReleaseDate { get; set; }
    }
}
