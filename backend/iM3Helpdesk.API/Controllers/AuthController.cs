using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly IAuthService _authService;
  private readonly IOtpService _otpService;
  private readonly ILogger<AuthController> _logger;

  public AuthController(
      IAuthService authService,
      IOtpService otpService,
      ILogger<AuthController> logger)
  {
    _authService = authService;
    _otpService = otpService;
    _logger = logger;
  }

  [HttpPost("login")]
  [EnableRateLimiting("login")]
  public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct = default)
  {
    var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
    var ua = Request.Headers.UserAgent.ToString();

    var result = await _authService.LoginAsync(dto, ip, ua, ct);
    if (!result.IsSuccess)
    {
      return StatusCode(result.StatusCode, new { message = result.ErrorMessage });
    }

    if (result.Data!.RequiresOtp)
    {
      return Ok(new
      {
        requiresOtp = true,
        isTwoFactorEnabled = result.Data.IsTwoFactorEnabled,
        message = result.Data.Message
      });
    }

    SetAuthCookies(result.Data.Token!, result.Data.RefreshToken!);
    return Ok(new
    {
      requiresOtp = false,
      token = result.Data.Token,
      refreshToken = result.Data.RefreshToken,
      isFirstLogin = result.Data.IsFirstLogin,
      user = new
      {
        id = result.Data.User!.Id,
        fullName = result.Data.User.FullName,
        email = result.Data.User.Email,
        role = result.Data.User.Role,
        isTwoFactorEnabled = result.Data.User.IsTwoFactorEnabled,
        organizationId = result.Data.User.OrganizationId,
        organizationName = result.Data.User.OrganizationName
      }
    });
  }

  [HttpPost("verify-otp")]
  [EnableRateLimiting("login")]
  public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto, CancellationToken ct = default)
  {
    var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
    var ua = Request.Headers.UserAgent.ToString();

    var result = await _authService.VerifyOtpAsync(dto.Email, dto.Otp, ip, ua, ct);
    if (!result.IsSuccess)
    {
      return StatusCode(result.StatusCode, new { message = result.ErrorMessage });
    }

    SetAuthCookies(result.Data!.Token!, result.Data.RefreshToken!);
    return Ok(new
    {
      token = result.Data.Token,
      refreshToken = result.Data.RefreshToken,
      isFirstLogin = result.Data.IsFirstLogin,
      user = new
      {
        id = result.Data.User!.Id,
        fullName = result.Data.User.FullName,
        email = result.Data.User.Email,
        role = result.Data.User.Role,
        isTwoFactorEnabled = result.Data.User.IsTwoFactorEnabled,
        organizationId = result.Data.User.OrganizationId,
        organizationName = result.Data.User.OrganizationName
      }
    });
  }

  [HttpPost("resend-otp")]
  [EnableRateLimiting("login")]
  public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto dto)
  {
    if (string.IsNullOrWhiteSpace(dto.Email))
      return BadRequest(new { message = "Email required" });

    await _otpService.SendOtpAsync(dto.Email);
    return Ok(new { message = "New OTP sent to your email." });
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct = default)
  {
    var result = await _authService.RegisterSuperAdminAsync(dto, ct);
    return result.IsSuccess 
      ? Ok(new { message = result.Data }) 
      : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
  }

  [HttpPost("register-customer")]
  public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDto dto, CancellationToken ct = default)
  {
    var result = await _authService.RegisterCustomerAsync(dto, ct);
    return result.IsSuccess 
      ? Ok(new { message = result.Data }) 
      : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
  }

  [HttpPost("refresh")]
  public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto? dto = null, CancellationToken ct = default)
  {
    var presentedRefreshToken = !string.IsNullOrWhiteSpace(dto?.RefreshToken)
        ? dto.RefreshToken
        : Request.Cookies["im3_refresh"];

    if (string.IsNullOrWhiteSpace(presentedRefreshToken))
    {
      ClearAuthCookies();
      return Unauthorized(new { message = "Invalid or expired refresh token" });
    }

    var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
    var ua = Request.Headers.UserAgent.ToString();

    var result = await _authService.RefreshTokenAsync(presentedRefreshToken, ip, ua, ct);
    if (!result.IsSuccess)
    {
      ClearAuthCookies();
      return StatusCode(result.StatusCode, new { message = result.ErrorMessage });
    }

    SetAuthCookies(result.Data!.Token, result.Data.RefreshToken);
    return Ok(new
    {
      token = result.Data.Token,
      refreshToken = result.Data.RefreshToken,
      user = new
      {
        id = result.Data.User!.Id,
        fullName = result.Data.User.FullName,
        email = result.Data.User.Email,
        role = result.Data.User.Role,
        isTwoFactorEnabled = result.Data.User.IsTwoFactorEnabled,
        organizationId = result.Data.User.OrganizationId,
        organizationName = result.Data.User.OrganizationName
      }
    });
  }

  [HttpPost("logout")]
  public async Task<IActionResult> Logout(CancellationToken ct = default)
  {
    var presentedRefreshToken = Request.Cookies["im3_refresh"];
    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    Guid.TryParse(userIdClaim, out var userId);

    await _authService.LogoutAsync(presentedRefreshToken, userId == Guid.Empty ? null : userId, ct);
    ClearAuthCookies();
    return Ok(new { message = "Logged out successfully" });
  }

  [Authorize]
  [HttpGet("me")]
  public async Task<IActionResult> GetCurrentUser(CancellationToken ct = default)
  {
    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    if (!Guid.TryParse(userIdClaim, out var userId))
      return Unauthorized();

    var result = await _authService.GetCurrentUserAsync(userId, ct);
    if (!result.IsSuccess)
      return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

    return Ok(new
    {
      id = result.Data!.Id,
      fullName = result.Data.FullName,
      email = result.Data.Email,
      role = result.Data.Role,
      isTwoFactorEnabled = result.Data.IsTwoFactorEnabled,
      organizationId = result.Data.OrganizationId,
      organizationName = result.Data.OrganizationName,
      photoUrl = result.Data.PhotoUrl
    });
  }

  [HttpGet("verify-email")]
  public async Task<IActionResult> VerifyEmail([FromQuery] string token, CancellationToken ct = default)
  {
    var result = await _authService.VerifyEmailAsync(token, ct);
    return result.IsSuccess 
      ? Ok(new { message = result.Data }) 
      : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
  }

  [HttpPost("forgot-password")]
  public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken ct = default)
  {
    var result = await _authService.ForgotPasswordAsync(dto.Email, ct);
    return result.IsSuccess 
      ? Ok(new { message = result.Data }) 
      : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
  }

  [HttpPost("reset-password")]
  public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken ct = default)
  {
    var result = await _authService.ResetPasswordAsync(dto, ct);
    return result.IsSuccess 
      ? Ok(new { message = result.Data }) 
      : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
  }

  private void SetAuthCookies(string accessToken, string refreshToken)
  {
    var secureCookie = Request.IsHttps;
    var accessCookieOptions = new CookieOptions
    {
      HttpOnly = true,
      Secure = secureCookie,
      SameSite = SameSiteMode.Lax,
      Expires = DateTimeOffset.UtcNow.AddMinutes(15),
      Path = "/"
    };

    var refreshCookieOptions = new CookieOptions
    {
      HttpOnly = true,
      Secure = secureCookie,
      SameSite = SameSiteMode.Lax,
      Expires = DateTimeOffset.UtcNow.AddDays(30),
      Path = "/"
    };

    Response.Cookies.Append("im3_access", accessToken, accessCookieOptions);
    Response.Cookies.Append("im3_refresh", refreshToken, refreshCookieOptions);
  }

  private void ClearAuthCookies()
  {
    var secureCookie = Request.IsHttps;
    var accessCookieOptions = new CookieOptions
    {
      HttpOnly = true,
      Secure = secureCookie,
      SameSite = SameSiteMode.Lax,
      Path = "/"
    };

    var refreshCookieOptions = new CookieOptions
    {
      HttpOnly = true,
      Secure = secureCookie,
      SameSite = SameSiteMode.Lax,
      Path = "/"
    };

    Response.Cookies.Delete("im3_access", accessCookieOptions);
    Response.Cookies.Delete("im3_refresh", refreshCookieOptions);

    Response.Cookies.Delete("im3_refresh", new CookieOptions
    {
      HttpOnly = true,
      Secure = secureCookie,
      SameSite = SameSiteMode.Lax,
      Path = "/api/Auth/refresh"
    });
    Response.Cookies.Delete("im3_refresh", new CookieOptions
    {
      HttpOnly = true,
      Secure = secureCookie,
      SameSite = SameSiteMode.Lax,
      Path = "/api/auth"
    });
  }
}

public class RefreshTokenDto
{
  public string RefreshToken { get; set; } = string.Empty;
}

public class VerifyOtpDto
{
  public string Email { get; set; } = string.Empty;
  public string Otp { get; set; } = string.Empty;
}

public class ResendOtpDto
{
  public string Email { get; set; } = string.Empty;
}
