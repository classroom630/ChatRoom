using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ChatRoom.API.Services.Interfaces;
using ChatRoom.Models.DTOs;

namespace ChatRoom.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userService.GetAllUsersAsync();
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Users can only view their own profile, admins can view any profile
            if (!isAdmin && currentUserId != id)
            {
                return Forbid();
            }

            var result = await _userService.GetUserByIdAsync(id);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<UserDto>.ErrorResponse("Validation failed", 400, errors));
            }

            var result = await _userService.CreateUserAsync(createUserDto);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return CreatedAtAction(nameof(GetUserById), new { id = result.Data!.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Users can only update their own profile, admins can update any profile
            if (!isAdmin && currentUserId != id)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<UserDto>.ErrorResponse("Validation failed", 400, errors));
            }

            var result = await _userService.UpdateUserAsync(id, updateUserDto);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto assignRoleDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.ErrorResponse("Validation failed", 400, errors));
            }

            var result = await _userService.AssignRoleAsync(assignRoleDto);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpGet("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserRoles(string id)
        {
            var result = await _userService.GetUserRolesAsync(id);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpGet("roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllRoles()
        {
            var result = await _userService.GetAllRolesAsync();
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }
    }
}