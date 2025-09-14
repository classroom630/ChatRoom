using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ChatRoom.API.Services.Interfaces;
using ChatRoom.Models.DTOs;

namespace ChatRoom.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Validation failed", 400, errors));
            }

            var result = await _authService.RegisterAsync(registerDto);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Validation failed", 400, errors));
            }

            var result = await _authService.LoginAsync(loginDto);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Invalid refresh token", 400));
            }

            var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var result = await _authService.RevokeTokenAsync(refreshTokenDto.RefreshToken);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid user", 400));
            }

            var result = await _authService.LogoutAsync(userId);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }
    }
}