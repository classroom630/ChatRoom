using Microsoft.EntityFrameworkCore;
using ChatRoom.API.Data;
using ChatRoom.API.Services.Interfaces;
using ChatRoom.Models.DTOs;
using ChatRoom.Models.Entities;

namespace ChatRoom.API.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TaskService> _logger;

        public TaskService(ApplicationDbContext context, ILogger<TaskService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<List<TaskDto>>> GetTasksAsync(string? userId = null, bool isAdminOrManager = false)
        {
            try
            {
                IQueryable<UserTask> query = _context.Tasks.Include(t => t.User);

                if (!isAdminOrManager && !string.IsNullOrEmpty(userId))
                {
                    // Regular users can only see their own tasks
                    query = query.Where(t => t.UserId == userId);
                }

                var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
                var taskDtos = tasks.Select(MapToTaskDto).ToList();

                return ApiResponse<List<TaskDto>>.SuccessResponse(taskDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks");
                return ApiResponse<List<TaskDto>>.ErrorResponse("An error occurred while retrieving tasks", 500);
            }
        }

        public async Task<ApiResponse<TaskDto>> GetTaskByIdAsync(int taskId, string requestingUserId, bool isAdminOrManager = false)
        {
            try
            {
                var task = await _context.Tasks.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == taskId);
                if (task == null)
                {
                    return ApiResponse<TaskDto>.ErrorResponse("Task not found", 404);
                }

                // Check permissions
                if (!isAdminOrManager && task.UserId != requestingUserId)
                {
                    return ApiResponse<TaskDto>.ErrorResponse("Access denied", 403);
                }

                var taskDto = MapToTaskDto(task);
                return ApiResponse<TaskDto>.SuccessResponse(taskDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task {TaskId}", taskId);
                return ApiResponse<TaskDto>.ErrorResponse("An error occurred while retrieving task", 500);
            }
        }

        public async Task<ApiResponse<TaskDto>> CreateTaskAsync(CreateTaskDto createTaskDto, string userId)
        {
            try
            {
                var task = new UserTask
                {
                    Title = createTaskDto.Title,
                    Description = createTaskDto.Description,
                    Priority = createTaskDto.Priority,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                // Load user for mapping
                await _context.Entry(task).Reference(t => t.User).LoadAsync();

                var taskDto = MapToTaskDto(task);
                return ApiResponse<TaskDto>.SuccessResponse(taskDto, "Task created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return ApiResponse<TaskDto>.ErrorResponse("An error occurred while creating task", 500);
            }
        }

        public async Task<ApiResponse<TaskDto>> UpdateTaskAsync(int taskId, UpdateTaskDto updateTaskDto, string requestingUserId, bool isAdminOrManager = false)
        {
            try
            {
                var task = await _context.Tasks.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == taskId);
                if (task == null)
                {
                    return ApiResponse<TaskDto>.ErrorResponse("Task not found", 404);
                }

                // Check permissions
                if (!isAdminOrManager && task.UserId != requestingUserId)
                {
                    return ApiResponse<TaskDto>.ErrorResponse("Access denied", 403);
                }

                task.Title = updateTaskDto.Title;
                task.Description = updateTaskDto.Description;
                task.Priority = updateTaskDto.Priority;
                task.IsCompleted = updateTaskDto.IsCompleted;
                task.UpdatedAt = DateTime.UtcNow;

                if (updateTaskDto.IsCompleted && task.CompletedAt == null)
                {
                    task.CompletedAt = DateTime.UtcNow;
                }
                else if (!updateTaskDto.IsCompleted)
                {
                    task.CompletedAt = null;
                }

                await _context.SaveChangesAsync();

                var taskDto = MapToTaskDto(task);
                return ApiResponse<TaskDto>.SuccessResponse(taskDto, "Task updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", taskId);
                return ApiResponse<TaskDto>.ErrorResponse("An error occurred while updating task", 500);
            }
        }

        public async Task<ApiResponse> DeleteTaskAsync(int taskId, string requestingUserId, bool isAdminOrManager = false)
        {
            try
            {
                var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
                if (task == null)
                {
                    return ApiResponse.ErrorResponse("Task not found", 404);
                }

                // Check permissions
                if (!isAdminOrManager && task.UserId != requestingUserId)
                {
                    return ApiResponse.ErrorResponse("Access denied", 403);
                }

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResponse("Task deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", taskId);
                return ApiResponse.ErrorResponse("An error occurred while deleting task", 500);
            }
        }

        public async Task<ApiResponse<TaskDto>> ToggleTaskCompletionAsync(int taskId, string requestingUserId, bool isAdminOrManager = false)
        {
            try
            {
                var task = await _context.Tasks.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == taskId);
                if (task == null)
                {
                    return ApiResponse<TaskDto>.ErrorResponse("Task not found", 404);
                }

                // Check permissions
                if (!isAdminOrManager && task.UserId != requestingUserId)
                {
                    return ApiResponse<TaskDto>.ErrorResponse("Access denied", 403);
                }

                task.IsCompleted = !task.IsCompleted;
                task.UpdatedAt = DateTime.UtcNow;

                if (task.IsCompleted)
                {
                    task.CompletedAt = DateTime.UtcNow;
                }
                else
                {
                    task.CompletedAt = null;
                }

                await _context.SaveChangesAsync();

                var taskDto = MapToTaskDto(task);
                return ApiResponse<TaskDto>.SuccessResponse(taskDto, "Task completion status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling task completion {TaskId}", taskId);
                return ApiResponse<TaskDto>.ErrorResponse("An error occurred while updating task", 500);
            }
        }

        public async Task<ApiResponse<List<TaskDto>>> GetTasksByUserAsync(string userId)
        {
            try
            {
                var tasks = await _context.Tasks
                    .Include(t => t.User)
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                var taskDtos = tasks.Select(MapToTaskDto).ToList();
                return ApiResponse<List<TaskDto>>.SuccessResponse(taskDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for user {UserId}", userId);
                return ApiResponse<List<TaskDto>>.ErrorResponse("An error occurred while retrieving user tasks", 500);
            }
        }

        private static TaskDto MapToTaskDto(UserTask task)
        {
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                Priority = task.Priority,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                CompletedAt = task.CompletedAt,
                UserId = task.UserId,
                UserName = $"{task.User.FirstName} {task.User.LastName}"
            };
        }
    }
}