using NekoViBE.Application.Common.DTOs.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.ProductTag
{
    public class ProductTagItem : BaseResponse
    {
        [JsonPropertyName("productId")]
        public Guid ProductId { get; set; }

        [JsonPropertyName("tagId")]
        public Guid TagId { get; set; }
        [JsonPropertyName("tag")]
        public TagItem Tag { get; set; } = null!;
    }
}
