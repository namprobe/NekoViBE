using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.Auth;

public class UpdateProfileRequest
{
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string PhoneNumber { get; set; } = string.Empty;
    
    public GenderEnum Gender { get; set; }
    
    public DateTime DateOfBirth { get; set; }
    
    public string? Bio { get; set; }
    
    public IFormFile? Avatar { get; set; }
}