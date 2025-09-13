using Microsoft.AspNetCore.Http;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Product
{
    public class ProductRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("discountPrice")]
        public decimal? DiscountPrice { get; set; }

        [JsonPropertyName("stockQuantity")]
        public int StockQuantity { get; set; }

        [JsonPropertyName("categoryId")]
        public Guid CategoryId { get; set; }

        [JsonPropertyName("animeSeriesId")]
        public Guid? AnimeSeriesId { get; set; }

        [JsonPropertyName("isPreOrder")]
        public bool IsPreOrder { get; set; }

        [JsonPropertyName("preOrderReleaseDate")]
        public DateTime? PreOrderReleaseDate { get; set; }

        [JsonPropertyName("status")]
        public EntityStatusEnum Status { get; set; }

        [JsonPropertyName("imageFile")]
        public IFormFile? ImageFile { get; set; }
    }
}
