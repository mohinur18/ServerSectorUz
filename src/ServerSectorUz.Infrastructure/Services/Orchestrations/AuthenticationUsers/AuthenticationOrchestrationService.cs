using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServerSectorUz.Core.Brokers.DateTimes;
using ServerSectorUz.Core.Brokers.Loggings;
using ServerSectorUz.Core.Brokers.Securities;
using ServerSectorUz.Core.Brokers.Storages;
using ServerSectorUz.Core.Brokers.Tokens;
using ServerSectorUz.Core.Exceptions.Dependencies;
using ServerSectorUz.Core.Exceptions.Services;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Configurations;
using ServerSectorUz.Core.Models.Foundations.AuthenticationUsers;
using ServerSectorUz.Core.Models.Orchestrations.AuthenticationUsers;
using ServerSectorUz.Core.Services.Foundations.AuthenticationUsers;
using ServerSectorUz.Core.Services.Orchestrations.AuthenticationUsers;

namespace ServerSectorUz.Infrastructure.Services.Orchestrations.AuthenticationUsers;

public class AuthenticationOrchestrationService : IAuthenticationOrchestrationService
{
    private readonly IUserService userService;
    private readonly IUserCredentialService userCredentialService;
    private readonly IUserSessionService userSessionService;
    private readonly IPasswordBroker passwordBroker;
    private readonly ITokenBroker tokenBroker;
    private readonly IStorageBroker storageBroker;
    private readonly IDateTimeBroker dateTimeBroker;
    private readonly ILoggingBroker loggingBroker;
    private readonly JwtOptions jwtOptions;

    public AuthenticationOrchestrationService(
        IUserService userService,
        IUserCredentialService userCredentialService,
        IUserSessionService userSessionService,
        IPasswordBroker passwordBroker,
        ITokenBroker tokenBroker,
        IStorageBroker storageBroker,
        IDateTimeBroker dateTimeBroker,
        ILoggingBroker loggingBroker,
        IOptions<JwtOptions> jwtOptions)
    {
        this.userService = userService;
        this.userCredentialService = userCredentialService;
        this.userSessionService = userSessionService;
        this.passwordBroker = passwordBroker;
        this.tokenBroker = tokenBroker;
        this.storageBroker = storageBroker;
        this.dateTimeBroker = dateTimeBroker;
        this.loggingBroker = loggingBroker;
        this.jwtOptions = jwtOptions.Value;
    }

    public async ValueTask<AuthResponse> RegisterAsync(RegisterUserRequest request)
    {
        try
        {
            ValidateRegisterRequest(request);

            var user = await this.userService.AddUserAsync(new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            });

            (string passwordHash, string passwordSalt) = this.passwordBroker.HashPassword(request.Password);

            await this.userCredentialService.AddUserCredentialAsync(new UserCredential
            {
                UserId = user.Id,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                MustChangePassword = false
            });

            IEnumerable<string> roles = await RetrieveUserRolesAsync(user.Id);
            return await IssueTokenPairAsync(user, roles);
        }
        catch (AuthenticationValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new AuthenticationServiceException("Failed to register user.", exception);
        }
    }

    public async ValueTask<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            ValidateLoginRequest(request);

            User? maybeUser = await this.userService.RetrieveUserByEmailAsync(request.Email);

            if (maybeUser is null || !maybeUser.IsActive)
            {
                throw new AuthenticationValidationException("Invalid email or password.");
            }

            UserCredential? maybeCredential =
                await this.userCredentialService.RetrieveUserCredentialByUserIdAsync(maybeUser.Id);

            if (maybeCredential is null)
            {
                throw new AuthenticationValidationException("Invalid email or password.");
            }

            bool isValidPassword = this.passwordBroker.VerifyPassword(
                request.Password,
                maybeCredential.PasswordHash,
                maybeCredential.PasswordSalt);

            if (!isValidPassword)
            {
                throw new AuthenticationValidationException("Invalid email or password.");
            }

            IEnumerable<string> roles = await RetrieveUserRolesAsync(maybeUser.Id);
            return await IssueTokenPairAsync(maybeUser, roles);
        }
        catch (AuthenticationValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new AuthenticationServiceException("Failed to login user.", exception);
        }
    }

    public async ValueTask<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            ValidateRefreshTokenRequest(request);

            User? maybeUser = await this.userService.RetrieveUserByIdAsync(request.UserId);

            if (maybeUser is null || !maybeUser.IsActive)
            {
                throw new AuthenticationValidationException("Invalid refresh token request.");
            }

            UserSession? activeSession =
                await this.userSessionService.RetrieveActiveUserSessionByUserIdAsync(request.UserId);

            if (activeSession is null)
            {
                throw new AuthenticationValidationException("No active session found.");
            }

            string incomingTokenHash = HashRefreshToken(request.RefreshToken);

            if (!string.Equals(activeSession.RefreshTokenHash, incomingTokenHash, StringComparison.Ordinal))
            {
                throw new AuthenticationValidationException("Refresh token is invalid.");
            }

            IEnumerable<string> roles = await RetrieveUserRolesAsync(maybeUser.Id);
            return await IssueTokenPairAsync(maybeUser, roles, activeSession);
        }
        catch (AuthenticationValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new AuthenticationDependencyException("Failed to refresh token.", exception);
        }
    }

    private async ValueTask<AuthResponse> IssueTokenPairAsync(
        User user,
        IEnumerable<string> roles,
        UserSession? existingSession = null)
    {
        DateTimeOffset now = this.dateTimeBroker.GetCurrentDateTimeOffset();
        DateTimeOffset accessTokenExpiresOn = now.AddMinutes(this.jwtOptions.AccessTokenMinutes);
        DateTimeOffset refreshTokenExpiresOn = now.AddDays(this.jwtOptions.RefreshTokenDays);

        string accessToken = this.tokenBroker.GenerateAccessToken(user, roles, accessTokenExpiresOn);
        string refreshToken = this.tokenBroker.GenerateRefreshToken();
        string refreshTokenHash = HashRefreshToken(refreshToken);

        if (existingSession is null)
        {
            await this.userSessionService.AddUserSessionAsync(new UserSession
            {
                UserId = user.Id,
                RefreshTokenHash = refreshTokenHash,
                ExpiresDate = refreshTokenExpiresOn,
                RevokedDate = null
            });
        }
        else
        {
            existingSession.RefreshTokenHash = refreshTokenHash;
            existingSession.ExpiresDate = refreshTokenExpiresOn;
            existingSession.RevokedDate = null;
            existingSession.IsActive = true;

            await this.userSessionService.ModifyUserSessionAsync(existingSession);
        }

        return new AuthResponse
        {
            UserId = user.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresOn = accessTokenExpiresOn,
            RefreshTokenExpiresOn = refreshTokenExpiresOn
        };
    }

    private async ValueTask<IEnumerable<string>> RetrieveUserRolesAsync(Guid userId)
    {
        return await (
            from userRole in this.storageBroker.UserRoles
            join role in this.storageBroker.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == userId && role.IsActive
            select role.Name)
            .Distinct()
            .ToListAsync();
    }

    private static string HashRefreshToken(string refreshToken)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(bytes);
    }

    private static void ValidateRegisterRequest(RegisterUserRequest request)
    {
        if (request is null)
        {
            throw new AuthenticationValidationException("Register request is required.");
        }

        ValidateString(request.FirstName, nameof(request.FirstName));
        ValidateString(request.LastName, nameof(request.LastName));
        ValidateString(request.Email, nameof(request.Email));
        ValidateString(request.Password, nameof(request.Password));

        if (request.Password.Length < 8)
        {
            throw new AuthenticationValidationException("Password must be at least 8 characters.");
        }
    }

    private static void ValidateLoginRequest(LoginRequest request)
    {
        if (request is null)
        {
            throw new AuthenticationValidationException("Login request is required.");
        }

        ValidateString(request.Email, nameof(request.Email));
        ValidateString(request.Password, nameof(request.Password));
    }

    private static void ValidateRefreshTokenRequest(RefreshTokenRequest request)
    {
        if (request is null)
        {
            throw new AuthenticationValidationException("Refresh token request is required.");
        }

        if (request.UserId == Guid.Empty)
        {
            throw new AuthenticationValidationException("UserId is invalid.");
        }

        ValidateString(request.RefreshToken, nameof(request.RefreshToken));
    }

    private static void ValidateString(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new AuthenticationValidationException($"{parameterName} is required.");
        }
    }
}
