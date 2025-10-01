using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.ProductTag
{
    public class ProductTagFilter : BasePaginationFilter
    {
        [JsonPropertyName("productId")]
        public Guid? ProductId { get; set; }

        [JsonPropertyName("tagId")]
        public Guid? TagId { get; set; }
    }
}
