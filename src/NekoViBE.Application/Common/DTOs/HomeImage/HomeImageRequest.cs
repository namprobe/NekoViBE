using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.HomeImage
{
    public class HomeImageRequest
    {
        [JsonPropertyName("animeSeriesId")] public Guid? AnimeSeriesId { get; set; }
        [JsonPropertyName("imageFile")] public IFormFile ImageFile { get; set; } = null!; // Required
    }
}
