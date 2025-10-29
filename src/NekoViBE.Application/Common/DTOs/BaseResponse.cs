using System.Text.Json.Serialization;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs;

public abstract class BaseResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
    [JsonPropertyName("createdBy")]
    public Guid? CreatedBy { get; set; }
    [JsonPropertyName("updatedBy")]
    public Guid? UpdatedBy { get; set; }
    [JsonPropertyName("status")]
    public EntityStatusEnum Status { get; set; }
    [JsonPropertyName("statusName")]
    public string StatusName => Status.ToString();

}