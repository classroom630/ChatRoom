using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ChatRoom.API.Data;
using ChatRoom.API.Services.Interfaces;
using ChatRoom.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatRoom.API.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<JwtService> _logger;

        public JwtService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "DefaultKey");
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? ""),
                new(ClaimTypes.Email, user.Email ?? ""),
                new("firstName", user.FirstName),
                new("lastName", user.LastName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "DefaultKey");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId)
        {
            try
            {
                var storedToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(x => x.Token == refreshToken && x.UserId == userId);

                return storedToken != null && storedToken.IsActive;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating refresh token");
                return false;
            }
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var token = await _context.RefreshTokens
                    .FirstOrDefaultAsync(x => x.Token == refreshToken);

                if (token != null)
                {
                    token.RevokedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
            }
        }

        public async Task RevokeAllRefreshTokensAsync(string userId)
        {
            try
            {
                var tokens = await _context.RefreshTokens
                    .Where(x => x.UserId == userId && x.RevokedAt == null)
                    .ToListAsync();

                foreach (var token in tokens)
                {
                    token.RevokedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all refresh tokens for user {UserId}", userId);
            }
        }
    }
}