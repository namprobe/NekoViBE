using Microsoft.AspNetCore.Http;

namespace NekoViBE.Application.Common.DTOs.Badge
{
    public class UpdateBadgeImageRequest
    {
        public IFormFile IconPath { get; set; } = null!;
    }
}
