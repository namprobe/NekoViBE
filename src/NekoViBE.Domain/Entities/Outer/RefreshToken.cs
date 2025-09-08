using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities.Outer;

/// <summary>
/// RefreshToken entity - 1 refresh token có thể có nhiều access token
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// Provider name (Google, Microsoft, etc.)
    /// </summary>
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// Refresh token value
    /// </summary>
    public string TokenValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Refresh token expires at
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Scope of the refresh token
    /// </summary>
    public string Scope { get; set; } = string.Empty;
    /// <summary>
    /// Last used timestamp
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
    
    /// <summary>
    /// Navigation property to access tokens
    /// </summary>
    public virtual ICollection<AccessToken> AccessTokens { get; set; } = new List<AccessToken>();
}
