using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSectorUz.Core.Exceptions.Services;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Orchestrations.AuthenticationUsers;
using ServerSectorUz.Core.Services.Orchestrations.AuthenticationUsers;

namespace ServerSectorUz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationOrchestrationService authenticationOrchestrationService;

    public AuthenticationController(IAuthenticationOrchestrationService authenticationOrchestrationService) =>
        this.authenticationOrchestrationService = authenticationOrchestrationService;

    [HttpPost("register")]
    [AllowAnonymous]
    public async ValueTask<ActionResult<AuthResponse>> RegisterAsync(RegisterUserRequest request)
    {
        try
        {
            AuthResponse response = await this.authenticationOrchestrationService.RegisterAsync(request);
            return Ok(response);
        }
        catch (AuthenticationValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (AuthenticationServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async ValueTask<ActionResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            AuthResponse response = await this.authenticationOrchestrationService.LoginAsync(request);
            return Ok(response);
        }
        catch (AuthenticationValidationException exception)
        {
            return Unauthorized(new { Error = exception.Message });
        }
        catch (AuthenticationServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async ValueTask<ActionResult<AuthResponse>> RefreshAsync(RefreshTokenRequest request)
    {
        try
        {
            AuthResponse response = await this.authenticationOrchestrationService.RefreshTokenAsync(request);
            return Ok(response);
        }
        catch (AuthenticationValidationException exception)
        {
            return Unauthorized(new { Error = exception.Message });
        }
        catch (Exception exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpGet("admin-only-probe")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminOnlyProbe() =>
        Ok(new { Message = "Role-based authorization is configured." });
}
