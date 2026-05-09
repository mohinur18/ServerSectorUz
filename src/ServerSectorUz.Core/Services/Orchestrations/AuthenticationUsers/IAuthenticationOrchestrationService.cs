using ServerSectorUz.Core.Models.Orchestrations.AuthenticationUsers;

namespace ServerSectorUz.Core.Services.Orchestrations.AuthenticationUsers;

public interface IAuthenticationOrchestrationService
{
    ValueTask<AuthResponse> RegisterAsync(RegisterUserRequest request);
    ValueTask<AuthResponse> LoginAsync(LoginRequest request);
    ValueTask<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
}
