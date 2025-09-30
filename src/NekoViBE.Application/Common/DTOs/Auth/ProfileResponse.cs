using System.Text.Json.Serialization;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.Auth.Queries.GetProfile;

public class ProfileResponse
{
    
    public string Email { get; set; } = null!;

    
    public string FirstName { get; set; } = null!;
    
    public string LastName { get; set; } = null!;

    
    public string? PhoneNumber { get; set; }
   
    public string? Gender { get; set; }
   
    public DateTime? DateOfBirth { get; set; }
   
    public string? Bio { get; set; }
    
    public string? AvatarPath { get; set; }

    // Staff specific fields
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Position { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? HireDate { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Salary { get; set; }
}
