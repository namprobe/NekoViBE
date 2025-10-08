using NekoViBE.Application.Common.DTOs.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.ProductTag
{
    public class ProductTagResponse : ProductTagItem
    {
        [JsonPropertyName("tag")]
        public new TagResponse Tag { get; set; } = null!;
    }
}
