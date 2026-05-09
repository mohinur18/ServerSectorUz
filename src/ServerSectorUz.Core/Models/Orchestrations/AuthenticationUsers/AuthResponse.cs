namespace ServerSectorUz.Core.Models.Orchestrations.AuthenticationUsers;

public class AuthResponse
{
    public Guid UserId { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset AccessTokenExpiresOn { get; set; }
    public DateTimeOffset RefreshTokenExpiresOn { get; set; }
}
