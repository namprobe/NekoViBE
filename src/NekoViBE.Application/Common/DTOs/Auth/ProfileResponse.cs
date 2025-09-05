using System.Text.Json.Serialization;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.Auth.Queries.GetProfile;

public class ProfileResponse
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = null!;
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = null!;

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }
    [JsonPropertyName("gender")]
    public string? Gender { get; set; }
    [JsonPropertyName("dateOfBirth")]
    public DateTime? DateOfBirth { get; set; }
    [JsonPropertyName("bio")]
    public string? Bio { get; set; }
    [JsonPropertyName("avatarPath")]
    public string? AvatarPath { get; set; }

    // Staff specific fields
    [JsonPropertyName("position")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Position { get; set; }
    [JsonPropertyName("hireDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? HireDate { get; set; }
    [JsonPropertyName("salary")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Salary { get; set; }
}
