using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ChatRoom.API.Data;
using ChatRoom.API.Services.Interfaces;
using ChatRoom.Models.DTOs;
using ChatRoom.Models.Entities;

namespace ChatRoom.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            IEmailService emailService,
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _emailService = emailService;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password", 401);
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!result.Succeeded)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password", 401);
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Generate tokens
                var accessToken = await _jwtService.GenerateAccessTokenAsync(user);
                var refreshToken = await _jwtService.GenerateRefreshTokenAsync();

                // Store refresh token
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7"))
                };

                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync();

                var userDto = await MapToUserDto(user);
                var authResponse = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")),
                    User = userDto
                };

                return ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email {Email}", loginDto.Email);
                return ApiResponse<AuthResponseDto>.ErrorResponse("An error occurred during login", 500);
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Email is already registered", 400);
                }

                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Failed to create user", 400, errors);
                }

                // Assign role
                await _userManager.AddToRoleAsync(user, registerDto.Role);

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email!, user.FirstName, user.LastName);

                // Generate tokens
                var accessToken = await _jwtService.GenerateAccessTokenAsync(user);
                var refreshToken = await _jwtService.GenerateRefreshTokenAsync();

                // Store refresh token
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7"))
                };

                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync();

                var userDto = await MapToUserDto(user);
                var authResponse = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")),
                    User = userDto
                };

                return ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Registration successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email {Email}", registerDto.Email);
                return ApiResponse<AuthResponseDto>.ErrorResponse("An error occurred during registration", 500);
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var storedToken = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(x => x.Token == refreshToken);

                if (storedToken == null || !storedToken.IsActive)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid refresh token", 401);
                }

                // Revoke old token
                storedToken.RevokedAt = DateTime.UtcNow;

                // Generate new tokens
                var newAccessToken = await _jwtService.GenerateAccessTokenAsync(storedToken.User);
                var newRefreshToken = await _jwtService.GenerateRefreshTokenAsync();

                // Store new refresh token
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var newRefreshTokenEntity = new RefreshToken
                {
                    Token = newRefreshToken,
                    UserId = storedToken.UserId,
                    ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7"))
                };

                _context.RefreshTokens.Add(newRefreshTokenEntity);
                await _context.SaveChangesAsync();

                var userDto = await MapToUserDto(storedToken.User);
                var authResponse = new AuthResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")),
                    User = userDto
                };

                return ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return ApiResponse<AuthResponseDto>.ErrorResponse("An error occurred during token refresh", 500);
            }
        }

        public async Task<ApiResponse> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                await _jwtService.RevokeRefreshTokenAsync(refreshToken);
                return ApiResponse.SuccessResponse("Token revoked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return ApiResponse.ErrorResponse("An error occurred while revoking token", 500);
            }
        }

        public async Task<ApiResponse> LogoutAsync(string userId)
        {
            try
            {
                await _jwtService.RevokeAllRefreshTokensAsync(userId);
                return ApiResponse.SuccessResponse("Logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user {UserId}", userId);
                return ApiResponse.ErrorResponse("An error occurred during logout", 500);
            }
        }

        private async Task<UserDto> MapToUserDto(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}