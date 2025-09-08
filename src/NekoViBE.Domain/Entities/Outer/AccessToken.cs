using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities.Outer;

/// <summary>
/// AccessToken entity - nhiều access token thuộc về 1 refresh token
/// </summary>
public class AccessToken : BaseEntity
{
    /// <summary>
    /// Foreign key to RefreshToken
    /// </summary>
    public Guid RefreshTokenId { get; set; }
    
    /// <summary>
    /// Access token value
    /// </summary>
    public string TokenValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Token type (Bearer, etc.)
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
    
    /// <summary>
    /// Access token expires at
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Scope of the access token
    /// </summary>
    public string Scope { get; set; } = string.Empty;
    /// <summary>
    /// Last used timestamp
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
    
    /// <summary>
    /// Navigation property to refresh token
    /// </summary>
    public virtual RefreshToken RefreshToken { get; set; } = null!;
}
