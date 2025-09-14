using ChatRoom.Models.DTOs;

namespace ChatRoom.MVC.Services
{
    public interface IApiService
    {
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponse<List<UserDto>>> GetUsersAsync(string token);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(string userId, string token);
        Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto, string token);
        Task<ApiResponse<UserDto>> UpdateUserAsync(string userId, UpdateUserDto updateUserDto, string token);
        Task<ApiResponse> DeleteUserAsync(string userId, string token);
        Task<ApiResponse> AssignRoleAsync(AssignRoleDto assignRoleDto, string token);
        Task<ApiResponse<List<string>>> GetRolesAsync(string token);
        Task<ApiResponse<List<TaskDto>>> GetTasksAsync(string token);
        Task<ApiResponse<TaskDto>> GetTaskByIdAsync(int taskId, string token);
        Task<ApiResponse<TaskDto>> CreateTaskAsync(CreateTaskDto createTaskDto, string token);
        Task<ApiResponse<TaskDto>> UpdateTaskAsync(int taskId, UpdateTaskDto updateTaskDto, string token);
        Task<ApiResponse> DeleteTaskAsync(int taskId, string token);
        Task<ApiResponse<TaskDto>> ToggleTaskCompletionAsync(int taskId, string token);
        Task<ApiResponse<List<TaskDto>>> GetTasksByUserAsync(string userId, string token);
    }
}