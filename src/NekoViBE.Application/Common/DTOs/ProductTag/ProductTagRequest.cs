using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.ProductTag
{
    public class ProductTagRequest
    {
        [JsonPropertyName("productId")]
        public Guid ProductId { get; set; }

        [JsonPropertyName("tagId")]
        public Guid TagId { get; set; }

        [JsonPropertyName("status")]
        public EntityStatusEnum Status { get; set; }
    }
}
