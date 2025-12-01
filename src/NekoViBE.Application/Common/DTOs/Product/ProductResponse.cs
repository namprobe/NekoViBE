using NekoViBE.Application.Common.DTOs.Event;
using NekoViBE.Application.Common.DTOs.ProductImage;
using NekoViBE.Application.Common.DTOs.ProductReview;
using NekoViBE.Application.Common.DTOs.ProductTag;
using NekoViBE.Application.Common.DTOs.Tag;
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

        [JsonPropertyName("stockQuantity")]
        public int StockQuantity { get; set; }

        [JsonPropertyName("isPreOrder")]
        public bool IsPreOrder { get; set; }

        [JsonPropertyName("preOrderReleaseDate")]
        public DateTime? PreOrderReleaseDate { get; set; }

        // Thêm danh sách liên quan
        [JsonPropertyName("images")]
        public List<ProductImageResponse>? Images { get; set; } = new List<ProductImageResponse>();

        [JsonPropertyName("productTags")]
        public List<ProductTagItem>? ProductTags { get; set; } = new List<ProductTagItem>();

        [JsonPropertyName("reviews")]
        public List<ProductReviewItem>? Reviews { get; set; } = new List<ProductReviewItem>();

        [JsonPropertyName("events")]
        public List<EventItem>? Events { get; set; } = new List<EventItem>();

        // Thêm trường tính toán
        [JsonPropertyName("totalSales")]
        public int TotalSales { get; set; } = 0; // Tổng Quantity từ OrderItems

        [JsonPropertyName("averageRating")]
        public double AverageRating { get; set; } = 0.0; // Trung bình Rating từ Reviews
    }
}
