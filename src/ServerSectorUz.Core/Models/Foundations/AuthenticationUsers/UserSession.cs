using ServerSectorUz.Core.Models.Foundations;

namespace ServerSectorUz.Core.Models.Foundations.AuthenticationUsers;

public class UserSession : BaseEntity
{
    public Guid UserId { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresDate { get; set; }
    public DateTimeOffset? RevokedDate { get; set; }
}
