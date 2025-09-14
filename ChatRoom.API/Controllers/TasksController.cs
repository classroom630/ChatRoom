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
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            var isManager = User.IsInRole("Manager");
            var isAdminOrManager = isAdmin || isManager;

            var result = await _taskService.GetTasksAsync(currentUserId, isAdminOrManager);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var isAdmin = User.IsInRole("Admin");
            var isManager = User.IsInRole("Manager");
            var isAdminOrManager = isAdmin || isManager;

            var result = await _taskService.GetTaskByIdAsync(id, currentUserId, isAdminOrManager);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "User,Manager,Admin")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<TaskDto>.ErrorResponse("Validation failed", 400, errors));
            }

            var result = await _taskService.CreateTaskAsync(createTaskDto, currentUserId);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return CreatedAtAction(nameof(GetTaskById), new { id = result.Data!.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var isAdmin = User.IsInRole("Admin");
            var isManager = User.IsInRole("Manager");
            var isAdminOrManager = isAdmin || isManager;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<TaskDto>.ErrorResponse("Validation failed", 400, errors));
            }

            var result = await _taskService.UpdateTaskAsync(id, updateTaskDto, currentUserId, isAdminOrManager);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var isAdmin = User.IsInRole("Admin");
            var isManager = User.IsInRole("Manager");
            var isAdminOrManager = isAdmin || isManager;

            var result = await _taskService.DeleteTaskAsync(id, currentUserId, isAdminOrManager);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> ToggleTaskCompletion(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var isAdmin = User.IsInRole("Admin");
            var isManager = User.IsInRole("Manager");
            var isAdminOrManager = isAdmin || isManager;

            var result = await _taskService.ToggleTaskCompletionAsync(id, currentUserId, isAdminOrManager);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetTasksByUser(string userId)
        {
            var result = await _taskService.GetTasksByUserAsync(userId);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }
    }
}