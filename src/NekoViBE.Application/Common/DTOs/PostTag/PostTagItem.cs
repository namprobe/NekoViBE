using NekoViBE.Application.Common.DTOs.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.PostTag
{
    public class PostTagItem : BaseResponse
    {
        public Guid Id { get; set; }
        public Guid TagId { get; set; }
        // Danh sách các Tag liên quan
        [JsonPropertyName("tags")]
        public List<TagItem> Tags { get; set; } = new();
    }
}
