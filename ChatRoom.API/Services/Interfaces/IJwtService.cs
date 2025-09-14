using ChatRoom.Models.Entities;
using System.Security.Claims;

namespace ChatRoom.API.Services.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateAccessTokenAsync(ApplicationUser user);
        Task<string> GenerateRefreshTokenAsync();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId);
        Task RevokeRefreshTokenAsync(string refreshToken);
        Task RevokeAllRefreshTokensAsync(string userId);
    }
}