using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.EventProduct
{
    public class EventProductFilter : BasePaginationFilter
    {
        [JsonPropertyName("eventId")]
        public Guid? EventId { get; set; }

        [JsonPropertyName("productId")]
        public Guid? ProductId { get; set; }

        [JsonPropertyName("isFeatured")]
        public bool? IsFeatured { get; set; }
    }
}
