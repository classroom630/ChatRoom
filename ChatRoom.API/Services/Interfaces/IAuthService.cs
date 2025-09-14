using ChatRoom.Models.DTOs;

namespace ChatRoom.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
        Task<ApiResponse> RevokeTokenAsync(string refreshToken);
        Task<ApiResponse> LogoutAsync(string userId);
    }
}