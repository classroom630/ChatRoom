using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ChatRoom.API.Services.Interfaces;
using ChatRoom.Models.DTOs;
using ChatRoom.Models.Entities;

namespace ChatRoom.API.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Roles = roles.ToList(),
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt
                    });
                }

                return ApiResponse<List<UserDto>>.SuccessResponse(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return ApiResponse<List<UserDto>>.ErrorResponse("An error occurred while retrieving users", 500);
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("User not found", 404);
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                };

                return ApiResponse<UserDto>.SuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", userId);
                return ApiResponse<UserDto>.ErrorResponse("An error occurred while retrieving user", 500);
            }
        }

        public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(createUserDto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("Email is already registered", 400);
                }

                var user = new ApplicationUser
                {
                    UserName = createUserDto.Email,
                    Email = createUserDto.Email,
                    FirstName = createUserDto.FirstName,
                    LastName = createUserDto.LastName,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, createUserDto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<UserDto>.ErrorResponse("Failed to create user", 400, errors);
                }

                // Assign role
                await _userManager.AddToRoleAsync(user, createUserDto.Role);

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email!, user.FirstName, user.LastName);

                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                };

                return ApiResponse<UserDto>.SuccessResponse(userDto, "User created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return ApiResponse<UserDto>.ErrorResponse("An error occurred while creating user", 500);
            }
        }

        public async Task<ApiResponse<UserDto>> UpdateUserAsync(string userId, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("User not found", 404);
                }

                // Check if email is already taken by another user
                var existingUser = await _userManager.FindByEmailAsync(updateUserDto.Email);
                if (existingUser != null && existingUser.Id != userId)
                {
                    return ApiResponse<UserDto>.ErrorResponse("Email is already taken by another user", 400);
                }

                user.Email = updateUserDto.Email;
                user.UserName = updateUserDto.Email;
                user.FirstName = updateUserDto.FirstName;
                user.LastName = updateUserDto.LastName;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<UserDto>.ErrorResponse("Failed to update user", 400, errors);
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                };

                return ApiResponse<UserDto>.SuccessResponse(userDto, "User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return ApiResponse<UserDto>.ErrorResponse("An error occurred while updating user", 500);
            }
        }

        public async Task<ApiResponse> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse.ErrorResponse("User not found", 404);
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResponse("Failed to delete user", 400, errors);
                }

                return ApiResponse.SuccessResponse("User deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return ApiResponse.ErrorResponse("An error occurred while deleting user", 500);
            }
        }

        public async Task<ApiResponse> AssignRoleAsync(AssignRoleDto assignRoleDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(assignRoleDto.UserId);
                if (user == null)
                {
                    return ApiResponse.ErrorResponse("User not found", 404);
                }

                var roleExists = await _roleManager.RoleExistsAsync(assignRoleDto.Role);
                if (!roleExists)
                {
                    return ApiResponse.ErrorResponse("Role does not exist", 400);
                }

                // Remove all existing roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }

                // Add new role
                var result = await _userManager.AddToRoleAsync(user, assignRoleDto.Role);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResponse("Failed to assign role", 400, errors);
                }

                return ApiResponse.SuccessResponse("Role assigned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user {UserId}", assignRoleDto.UserId);
                return ApiResponse.ErrorResponse("An error occurred while assigning role", 500);
            }
        }

        public async Task<ApiResponse<List<string>>> GetUserRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<List<string>>.ErrorResponse("User not found", 404);
                }

                var roles = await _userManager.GetRolesAsync(user);
                return ApiResponse<List<string>>.SuccessResponse(roles.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles for user {UserId}", userId);
                return ApiResponse<List<string>>.ErrorResponse("An error occurred while retrieving user roles", 500);
            }
        }

        public async Task<ApiResponse<List<string>>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return ApiResponse<List<string>>.SuccessResponse(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all roles");
                return ApiResponse<List<string>>.ErrorResponse("An error occurred while retrieving roles", 500);
            }
        }
    }
}