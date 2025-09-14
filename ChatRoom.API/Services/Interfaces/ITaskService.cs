using ChatRoom.Models.DTOs;

namespace ChatRoom.API.Services.Interfaces
{
    public interface ITaskService
    {
        Task<ApiResponse<List<TaskDto>>> GetTasksAsync(string? userId = null, bool isAdminOrManager = false);
        Task<ApiResponse<TaskDto>> GetTaskByIdAsync(int taskId, string requestingUserId, bool isAdminOrManager = false);
        Task<ApiResponse<TaskDto>> CreateTaskAsync(CreateTaskDto createTaskDto, string userId);
        Task<ApiResponse<TaskDto>> UpdateTaskAsync(int taskId, UpdateTaskDto updateTaskDto, string requestingUserId, bool isAdminOrManager = false);
        Task<ApiResponse> DeleteTaskAsync(int taskId, string requestingUserId, bool isAdminOrManager = false);
        Task<ApiResponse<TaskDto>> ToggleTaskCompletionAsync(int taskId, string requestingUserId, bool isAdminOrManager = false);
        Task<ApiResponse<List<TaskDto>>> GetTasksByUserAsync(string userId);
    }
}