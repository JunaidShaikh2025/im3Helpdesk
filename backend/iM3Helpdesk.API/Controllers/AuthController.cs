using iM3Helpdesk.Application.DTOs.Auth;
using iM3Helpdesk.API.Services;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using iM3Helpdesk.Infrastructure.Services;
using System.Text;
using iM3Helpdesk.Application.Contracts.Services;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly ApplicationDbContext _context;
  private readonly IConfiguration _configuration;
  private readonly IEmailService _emailService;
  private readonly IOtpService _otpService;

  public AuthController(
      ApplicationDbContext context,
      IConfiguration configuration,
      IEmailService emailService,
      IOtpService otpService)
  {
    _context = context;
    _configuration = configuration;
    _emailService = emailService;
    _otpService = otpService;
  }

  [HttpPost("login")]
  [EnableRateLimiting("login")]
  public async Task<IActionResult> Login(
      [FromBody] LoginDto dto)
  {
    var user = await _context.Users
        .IgnoreQueryFilters()
        .Include(u => u.Organization)
        .FirstOrDefaultAsync(u => u.Email == dto.Email);

    if (user == null)
      return Unauthorized(
          new { message = "Invalid email or password" });


    // ✅ Deactivated by admin (LockedUntil = now + 100 years)
    if (user.LockedUntil.HasValue &&
        user.LockedUntil > DateTime.UtcNow.AddYears(50))
    {
      return Unauthorized(new
      {
        message = "Your account has been deactivated. Please contact your administrator."
      });
    }

    // ✅ Temporarily locked (brute force)
    if (user.LockedUntil.HasValue &&
        user.LockedUntil > DateTime.UtcNow)
    {
      var mins =
          (int)(user.LockedUntil.Value - DateTime.UtcNow)
          .TotalMinutes + 1;
      return Unauthorized(new
      {
        message = $"Account locked. Try again in {mins} minutes."
      });
    }

    // Wrong password
    if (!BCrypt.Net.BCrypt.Verify(
        dto.Password, user.PasswordHash))
    {
      user.FailedLoginAttempts++;
      if (user.FailedLoginAttempts >= 5)
      {
        user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
        user.FailedLoginAttempts = 0;
        await _context.SaveChangesAsync();
        return Unauthorized(new
        {
          message =
              "Account locked for 30 minutes due to " +
              "multiple failed attempts."
        });
      }
      await _context.SaveChangesAsync();
      return Unauthorized(new
      {
        message =
            $"Invalid email or password. " +
            $"{5 - user.FailedLoginAttempts} attempts remaining."
      });
    }

    if (!user.IsEmailVerified)
      return Unauthorized(
          new { message = "Please verify your email first" });

    if (user.OrganizationId.HasValue)
    {
      var org = await _context.Organizations
          .FindAsync(user.OrganizationId.Value);
      if (org != null && !org.IsActive)
        return Unauthorized(new
        {
          message =
              "Your organization has been deactivated. " +
              "Please contact support."
        });
    }

    // Reset failed attempts
    user.FailedLoginAttempts = 0;
    user.LockedUntil = null;

    // OTP is required when explicitly requested or when account-level 2FA is enabled.
    if (dto.LoginWithOtp || user.IsTwoFactorEnabled)
    {
      // OTP mode — sirf OTP bhejo, JWT mat do abhi
      await _context.SaveChangesAsync();
      await _otpService.SendOtpAsync(dto.Email);

      return Ok(new
      {
        requiresOtp = true,
        isTwoFactorEnabled = user.IsTwoFactorEnabled,
        message = "OTP sent to your email. Please verify."
      });
    }
    else
    {
      // Normal password mode — create session & set secure cookies
      var isFirstLogin = user.LastLoginAt == null;
      var (token, refreshToken) = await CreateUserSessionAsync(user);

      return Ok(new
      {
        requiresOtp = false,
        token,
        refreshToken,
        isFirstLogin,
        user = new
        {
          user.Id,
          user.FullName,
          user.Email,
          role = user.Role.ToString(),
          isTwoFactorEnabled = user.IsTwoFactorEnabled,
          organizationId = user.OrganizationId,
          organizationName = user.Organization?.Name
        }
      });
    }
  }

  private async Task<(string accessToken, string refreshToken)> CreateUserSessionAsync(User user)
  {
    var token = GenerateJwtToken(user);
    var refreshToken = GenerateRefreshToken();
    var familyId = Guid.NewGuid();

    var session = new UserSession
    {
      UserId = user.Id,
      TokenHash = HashRefreshToken(refreshToken),
      FamilyId = familyId,
      IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
      UserAgent = Request.Headers.UserAgent.ToString(),
      DeviceInfo = ResolveDeviceInfo(Request.Headers.UserAgent.ToString()),
      CreatedAt = DateTime.UtcNow,
      ExpiresAt = DateTime.UtcNow.AddDays(30),
      IsRevoked = false
    };

    _context.UserSessions.Add(session);
    user.RefreshToken = HashRefreshToken(refreshToken);
    user.RefreshTokenExpiresAt = session.ExpiresAt;
    user.LastLoginAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    SetAuthCookies(token, refreshToken);
    return (token, refreshToken);
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

    // Delete any legacy path cookies if present
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

  private static string ResolveDeviceInfo(string? userAgent)
  {
    if (string.IsNullOrWhiteSpace(userAgent)) return "Unknown";
    if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase)) return "Windows Desktop";
    if (userAgent.Contains("Macintosh", StringComparison.OrdinalIgnoreCase) || userAgent.Contains("Mac OS", StringComparison.OrdinalIgnoreCase)) return "macOS Desktop";
    if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase)) return "Android Device";
    if (userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) || userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase)) return "iOS Device";
    if (userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase)) return "Linux Desktop";
    return "Web Browser";
  }


  [HttpPost("verify-otp")]
  [EnableRateLimiting("login")]
  public async Task<IActionResult> VerifyOtp(
      [FromBody] VerifyOtpDto dto)
  {
    if (string.IsNullOrWhiteSpace(dto.Email) ||
        string.IsNullOrWhiteSpace(dto.Otp))
      return BadRequest(
          new { message = "Email and OTP are required" });

    var isValid = await _otpService
        .VerifyOtpAsync(dto.Email, dto.Otp);

    if (!isValid)
      return Unauthorized(new
      {
        message = "Invalid or expired OTP. Please try again."
      });

    var user = await _context.Users
        .IgnoreQueryFilters()
        .Include(u => u.Organization)
        .FirstOrDefaultAsync(u =>
            u.Email.ToLower() == dto.Email.ToLower());

    if (user == null)
      return Unauthorized(
          new { message = "User not found" });

    // ✅ Deactivated by admin check
    if (user.LockedUntil.HasValue &&
        user.LockedUntil > DateTime.UtcNow.AddYears(50))
    {
      return Unauthorized(new
      {
        message = "Your account has been deactivated. Please contact your administrator."
      });
    }

    // ✅ Temporarily locked check
    if (user.LockedUntil.HasValue &&
        user.LockedUntil > DateTime.UtcNow)
    {
      return Unauthorized(new
      {
        message = "Account is temporarily locked. Please try again later."
      });
    }

    // Issue JWT + refresh token session with family tracking
    var isFirstLogin = user.LastLoginAt == null;
    var (token, refreshToken) = await CreateUserSessionAsync(user);

    return Ok(new
    {
      token,
      refreshToken,
      isFirstLogin,
      user = new
      {
        user.Id,
        user.FullName,
        user.Email,
        role = user.Role.ToString(),
        isTwoFactorEnabled = user.IsTwoFactorEnabled,
        organizationId = user.OrganizationId,
        organizationName = user.Organization?.Name
      }
    });
  }


  [HttpPost("resend-otp")]
  [EnableRateLimiting("login")]
  public async Task<IActionResult> ResendOtp(
      [FromBody] ResendOtpDto dto)
  {
    if (string.IsNullOrWhiteSpace(dto.Email))
      return BadRequest(new { message = "Email required" });

    await _otpService.SendOtpAsync(dto.Email);

    return Ok(new
    {
      message = "New OTP sent to your email."
    });
  }


  [HttpPost("register")]
  public async Task<IActionResult> Register(
      [FromBody] RegisterDto dto)
  {
    if (dto.Password != dto.ConfirmPassword)
      return BadRequest(
          new { message = "Passwords do not match" });

    var existingUser = await _context.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Email == dto.Email);

    if (existingUser != null)
      return BadRequest(
          new { message = "Email already registered" });

    var organization = new Organization
    {
      Name = dto.CompanyName,
      Slug = dto.CompanyName.ToLower()
            .Replace(" ", "-").Replace(".", "")
            + "-" + Guid.NewGuid().ToString()[..6],
      TrialEndsAt = DateTime.UtcNow.AddDays(30),
      IsActive = true
    };
    _context.Organizations.Add(organization);

    var verificationToken = Guid.NewGuid().ToString();
    var user = new User
    {
      FullName = dto.FullName,
      Email = dto.Email,
      PhoneNumber = dto.PhoneNumber,
      PasswordHash =
          BCrypt.Net.BCrypt.HashPassword(dto.Password),
      Role = UserRole.CompanyAdmin,
      OrganizationId = organization.Id,
      IsEmailVerified = false,
      EmailVerificationToken = verificationToken
    };
    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    try
    {
      await _emailService.SendEmailVerificationAsync(
          user.Email, user.FullName,
          verificationToken, dto.CompanyName, organization.Id);
      await _emailService.SendWelcomeEmailAsync(
          user.Email, user.FullName, dto.CompanyName, organization.Id);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Email failed: {ex.Message}");
    }

    return Ok(new
    {
      message =
          "Account created! Please check your email to verify."
    });
  }


  [HttpPost("register-customer")]
  public async Task<IActionResult> RegisterCustomer(
      [FromBody] RegisterCustomerDto dto)
  {
    if (dto.Password != dto.ConfirmPassword)
      return BadRequest(
          new { message = "Passwords do not match" });

    var existingUser = await _context.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Email == dto.Email);

    if (existingUser != null)
      return BadRequest(
          new { message = "Email already registered" });

    var org = await _context.Organizations
        .FirstOrDefaultAsync(o =>
            o.Slug == dto.OrganizationSlug && o.IsActive);

    if (org == null)
      return BadRequest(new
      {
        message =
            "Invalid organization. Please check your invite link."
      });

    var verificationToken = Guid.NewGuid().ToString();
    var user = new User
    {
      FullName = dto.FullName,
      Email = dto.Email,
      PhoneNumber = dto.PhoneNumber ?? "",
      PasswordHash =
          BCrypt.Net.BCrypt.HashPassword(dto.Password),
      Role = UserRole.Customer,
      OrganizationId = org.Id,
      IsEmailVerified = false,
      EmailVerificationToken = verificationToken
    };
    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    try
    {
      await _emailService.SendEmailVerificationAsync(
          user.Email, user.FullName,
          verificationToken, org.Name, org.Id);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Email failed: {ex.Message}");
    }

    return Ok(new
    {
      message = "Account created! Please verify your email."
    });
  }


  [HttpPost("refresh")]
  public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto? dto = null)
  {
    var presentedRefreshToken = !string.IsNullOrWhiteSpace(dto?.RefreshToken)
        ? dto.RefreshToken
        : Request.Cookies["im3_refresh"];

    if (string.IsNullOrWhiteSpace(presentedRefreshToken))
    {
      ClearAuthCookies();
      return Unauthorized(new { message = "Invalid or expired refresh token" });
    }

    var presentedRefreshTokenHash = HashRefreshToken(presentedRefreshToken);

    var existingSession = await _context.UserSessions
        .Include(s => s.User)
        .ThenInclude(u => u!.Organization)
        .FirstOrDefaultAsync(s => s.TokenHash == presentedRefreshTokenHash);

    // 1. Session record not found in UserSessions (fallback check User table for backward compatibility)
    if (existingSession == null)
    {
      var legacyUser = await _context.Users
          .IgnoreQueryFilters()
          .Include(u => u.Organization)
          .FirstOrDefaultAsync(u =>
              u.RefreshToken == presentedRefreshTokenHash &&
              u.RefreshTokenExpiresAt > DateTime.UtcNow);

      if (legacyUser == null)
      {
        ClearAuthCookies();
        return Unauthorized(new { message = "Invalid or expired refresh token" });
      }

      // Upgrade legacy user session to family tracking
      var (legacyToken, legacyRefreshToken) = await CreateUserSessionAsync(legacyUser);
      return Ok(new
      {
        token = legacyToken,
        refreshToken = legacyRefreshToken,
        user = new
        {
          legacyUser.Id,
          legacyUser.FullName,
          legacyUser.Email,
          role = legacyUser.Role.ToString(),
          isTwoFactorEnabled = legacyUser.IsTwoFactorEnabled,
          organizationId = legacyUser.OrganizationId,
          organizationName = legacyUser.Organization?.Name
        }
      });
    }

    // 2. REUSE DETECTION (Token Theft Breach)
    // If a revoked token is ever reused, treat it as a breach and invalidate all tokens in that family!
    if (existingSession.IsRevoked)
    {
      var familySessions = await _context.UserSessions
          .Where(s => s.FamilyId == existingSession.FamilyId && !s.IsRevoked)
          .ToListAsync();

      foreach (var s in familySessions)
      {
        s.IsRevoked = true;
        s.RevokedAt = DateTime.UtcNow;
      }

      await _context.SaveChangesAsync();
      ClearAuthCookies();

      return StatusCode(StatusCodes.Status401Unauthorized, new
      {
        message = "Security breach detected: Replay of revoked refresh token. All active sessions in this family have been terminated."
      });
    }

    // 3. Token expired
    if (existingSession.ExpiresAt <= DateTime.UtcNow)
    {
      existingSession.IsRevoked = true;
      existingSession.RevokedAt = DateTime.UtcNow;
      await _context.SaveChangesAsync();
      ClearAuthCookies();
      return Unauthorized(new { message = "Refresh token has expired. Please log in again." });
    }

    var user = existingSession.User;
    if (user == null)
    {
      ClearAuthCookies();
      return Unauthorized(new { message = "User not found" });
    }

    if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
    {
      ClearAuthCookies();
      return Unauthorized(new { message = "Account is locked." });
    }

    // 4. Token Rotation: Invalidate current token and issue new token in SAME family
    var newRefreshToken = GenerateRefreshToken();
    var newRefreshTokenHash = HashRefreshToken(newRefreshToken);

    existingSession.IsRevoked = true;
    existingSession.RevokedAt = DateTime.UtcNow;
    existingSession.ReplacedByTokenHash = newRefreshTokenHash;

    var newSession = new UserSession
    {
      UserId = user.Id,
      TokenHash = newRefreshTokenHash,
      FamilyId = existingSession.FamilyId,
      IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
      UserAgent = Request.Headers.UserAgent.ToString(),
      DeviceInfo = existingSession.DeviceInfo,
      CreatedAt = DateTime.UtcNow,
      ExpiresAt = DateTime.UtcNow.AddDays(30),
      IsRevoked = false
    };

    _context.UserSessions.Add(newSession);
    user.RefreshToken = newRefreshTokenHash;
    user.RefreshTokenExpiresAt = newSession.ExpiresAt;
    await _context.SaveChangesAsync();

    var newToken = GenerateJwtToken(user);
    SetAuthCookies(newToken, newRefreshToken);

    return Ok(new
    {
      token = newToken,
      refreshToken = newRefreshToken,
      user = new
      {
        user.Id,
        user.FullName,
        user.Email,
        role = user.Role.ToString(),
        isTwoFactorEnabled = user.IsTwoFactorEnabled,
        organizationId = user.OrganizationId,
        organizationName = user.Organization?.Name
      }
    });
  }

  [HttpPost("logout")]
  public async Task<IActionResult> Logout()
  {
    var presentedRefreshToken = Request.Cookies["im3_refresh"];
    if (!string.IsNullOrWhiteSpace(presentedRefreshToken))
    {
      var tokenHash = HashRefreshToken(presentedRefreshToken);
      var session = await _context.UserSessions
          .FirstOrDefaultAsync(s => s.TokenHash == tokenHash);

      if (session != null)
      {
        session.IsRevoked = true;
        session.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
      }
    }

    ClearAuthCookies();
    return Ok(new { message = "Logged out successfully" });
  }

  [Authorize]
  [HttpGet("me")]
  public async Task<IActionResult> GetCurrentUser()
  {
    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      User.FindFirstValue(JwtRegisteredClaimNames.Sub);

    if (!Guid.TryParse(userIdClaim, out var userId))
      return Unauthorized();

    var user = await _context.Users
        .IgnoreQueryFilters()
        .Include(u => u.Organization)
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null)
      return Unauthorized();

    return Ok(new
    {
      user.Id,
      user.FullName,
      user.Email,
      role = user.Role.ToString(),
      isTwoFactorEnabled = user.IsTwoFactorEnabled,
      organizationId = user.OrganizationId,
      organizationName = user.Organization?.Name,
      user.PhotoUrl
    });
  }


  [HttpGet("verify-email")]
  public async Task<IActionResult> VerifyEmail(
      [FromQuery] string token)
  {
    var user = await _context.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u =>
            u.EmailVerificationToken == token);

    if (user == null)
      return BadRequest(
          new { message = "Invalid or expired token" });

    user.IsEmailVerified = true;
    user.EmailVerificationToken = null;
    await _context.SaveChangesAsync();
    return Ok(new { message = "Email verified successfully" });
  }


  [HttpPost("forgot-password")]
  public async Task<IActionResult> ForgotPassword(
      [FromBody] ForgotPasswordDto dto)
  {
    var user = await _context.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Email == dto.Email);

    if (user != null)
    {
      user.EmailVerificationToken =
          Guid.NewGuid().ToString();
      await _context.SaveChangesAsync();
      try
      {
        await _emailService.SendForgotPasswordAsync(
            user.Email, user.FullName,
            user.EmailVerificationToken,
            organizationId:user.OrganizationId);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Email failed: {ex.Message}");
      }
    }

    return Ok(new
    {
      message = "If email exists, reset link has been sent."
    });
  }

  [HttpPost("reset-password")]
  public async Task<IActionResult> ResetPassword(
    [FromBody] ResetPasswordDto dto)
  {
    if (string.IsNullOrWhiteSpace(dto.Token) ||
        string.IsNullOrWhiteSpace(dto.NewPassword))
      return BadRequest(
          new { message = "Token and new password are required." });

    var user = await _context.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u =>
            u.EmailVerificationToken == dto.Token);

    if (user == null)
      return BadRequest(new
      {
        message = "Invalid or expired reset link. Please request a new one."
      });

    // ✅ Password update karo aur token clear karo
    user.PasswordHash =
        BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
    user.EmailVerificationToken = null;

    // Security: active sessions invalidate karo
    user.RefreshToken = null;
    user.RefreshTokenExpiresAt = null;

    var activeSessions = await _context.UserSessions
        .Where(s => s.UserId == user.Id && !s.IsRevoked)
        .ToListAsync();
    foreach (var s in activeSessions)
    {
      s.IsRevoked = true;
      s.RevokedAt = DateTime.UtcNow;
    }

    await _context.SaveChangesAsync();

    return Ok(new { message = "Password reset successfully." });
  }

  private string GenerateJwtToken(User user)
  {
    var jwtSettings =
        _configuration.GetSection("JwtSettings");
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub,
          user.Id.ToString()),
      new Claim(JwtRegisteredClaimNames.Email,
          user.Email),
      new Claim(ClaimTypes.Role,
          user.Role.ToString()),
      new Claim("organizationId",
          user.OrganizationId?.ToString() ?? ""),
      new Claim("fullName", user.FullName),
      new Claim(JwtRegisteredClaimNames.Jti,
          Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken(
        issuer: jwtSettings["Issuer"],
        audience: jwtSettings["Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(15),
        signingCredentials: new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256));

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  private static string GenerateRefreshToken()
  {
    var bytes = RandomNumberGenerator.GetBytes(32);
    return Convert.ToBase64String(bytes);
  }

  private static string HashRefreshToken(string refreshToken)
  {
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
    return Convert.ToHexString(bytes);
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
