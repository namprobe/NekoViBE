using NekoViBE.Application.Common.DTOs.HomeImage;
using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs.UserHomeImage
{
    public class UserHomeImageItem : BaseResponse
    {
        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("homeImage")]
        public HomeImageItem HomeImage { get; set; } = null!;
    }

}
