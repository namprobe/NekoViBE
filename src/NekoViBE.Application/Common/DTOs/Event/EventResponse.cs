using NekoViBE.Application.Common.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Event
{
    public class EventResponse : EventItem
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("products")]
        public List<ProductItem>? Products { get; set; } = new List<ProductItem>();
    }
}
