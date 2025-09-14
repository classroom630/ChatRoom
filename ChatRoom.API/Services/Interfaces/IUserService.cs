using ChatRoom.Models.DTOs;

namespace ChatRoom.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<List<UserDto>>> GetAllUsersAsync();
        Task<ApiResponse<UserDto>> GetUserByIdAsync(string userId);
        Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto);
        Task<ApiResponse<UserDto>> UpdateUserAsync(string userId, UpdateUserDto updateUserDto);
        Task<ApiResponse> DeleteUserAsync(string userId);
        Task<ApiResponse> AssignRoleAsync(AssignRoleDto assignRoleDto);
        Task<ApiResponse<List<string>>> GetUserRolesAsync(string userId);
        Task<ApiResponse<List<string>>> GetAllRolesAsync();
    }
}