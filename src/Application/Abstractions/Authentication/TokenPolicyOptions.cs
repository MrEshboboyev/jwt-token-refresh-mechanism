namespace Application.Abstractions.Authentication;

public class TokenPolicyOptions
{
    public const string SectionName = "TokenPolicy";
    
    /// <summary>
    /// Gets or sets the refresh token lifetime in days
    /// </summary>
    public int RefreshTokenLifetimeDays { get; set; } = 7;
    
    /// <summary>
    /// Gets or sets the access token lifetime in hours
    /// </summary>
    public int AccessTokenLifetimeHours { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets whether to enable sliding window expiration for refresh tokens
    /// </summary>
    public bool EnableSlidingWindowExpiration { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the sliding window period in days
    /// Refresh tokens will be extended if used within this period before expiration
    /// </summary>
    public int SlidingWindowPeriodDays { get; set; } = 3;
    
    /// <summary>
    /// Gets or sets the maximum lifetime of a refresh token in days
    /// Even with sliding window, tokens won't be extended beyond this limit
    /// </summary>
    public int MaxRefreshTokenLifetimeDays { get; set; } = 30;
    
    /// <summary>
    /// Gets or sets whether to enable refresh token reuse detection
    /// </summary>
    public bool EnableRefreshTokenReuseDetection { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to enable concurrent login detection
    /// </summary>
    public bool EnableConcurrentLoginDetection { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the maximum number of concurrent sessions per user
    /// </summary>
    public int MaxConcurrentSessionsPerUser { get; set; } = 5;
}
