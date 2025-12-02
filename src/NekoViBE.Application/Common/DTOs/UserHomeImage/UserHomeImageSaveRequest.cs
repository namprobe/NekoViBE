// Application/Common/DTOs/UserHomeImage/UserHomeImageSaveRequest.cs
using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs.UserHomeImage
{
    public class UserHomeImageSaveRequest
    {
        [JsonPropertyName("homeImageId")]
        public Guid HomeImageId { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; } // 1, 2 hoặc 3
    }
}