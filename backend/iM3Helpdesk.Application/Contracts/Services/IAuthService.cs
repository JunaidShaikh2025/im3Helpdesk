using iM3Helpdesk.Application.Common;
using iM3Helpdesk.Application.DTOs.Auth;

namespace iM3Helpdesk.Application.Contracts.Services;

public interface IAuthService
{
    Task<ServiceResult<LoginResultDto>> LoginAsync(
        LoginDto dto, string? ipAddress, string? userAgent, CancellationToken ct = default);

    Task<ServiceResult<LoginResultDto>> VerifyOtpAsync(
        string email, string otp, string? ipAddress, string? userAgent, CancellationToken ct = default);

    Task<ServiceResult<string>> RegisterSuperAdminAsync(
        RegisterDto dto, CancellationToken ct = default);

    Task<ServiceResult<string>> RegisterCustomerAsync(
        RegisterCustomerDto dto, CancellationToken ct = default);

    Task<ServiceResult<string>> VerifyEmailAsync(
        string token, CancellationToken ct = default);

    Task<ServiceResult<string>> ResendVerificationAsync(
        string email, CancellationToken ct = default);

    Task<ServiceResult<RefreshTokenResultDto>> RefreshTokenAsync(
        string? incomingRefreshToken, string? ipAddress, string? userAgent, CancellationToken ct = default);

    Task<ServiceResult> LogoutAsync(
        string? refreshToken, Guid? userId, CancellationToken ct = default);

    Task<ServiceResult<string>> ForgotPasswordAsync(
        string email, CancellationToken ct = default);

    Task<ServiceResult<string>> ResetPasswordAsync(
        ResetPasswordDto dto, CancellationToken ct = default);

    Task<ServiceResult<UserSummaryDto>> GetCurrentUserAsync(
        Guid userId, CancellationToken ct = default);
}
