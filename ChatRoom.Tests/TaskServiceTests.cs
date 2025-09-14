using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ChatRoom.API.Services;
using ChatRoom.API.Data;
using ChatRoom.Models.Entities;
using ChatRoom.Models.DTOs;

namespace ChatRoom.Tests
{
    public class TaskServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<TaskService>> _loggerMock;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<TaskService>>();
            _taskService = new TaskService(_context, _loggerMock.Object);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var user = new ApplicationUser
            {
                Id = "user1",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            var task = new UserTask
            {
                Id = 1,
                Title = "Test Task",
                Description = "Test Description",
                Priority = TaskPriority.Medium,
                UserId = "user1",
                User = user,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.Tasks.Add(task);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetTasksAsync_UserRole_ReturnsUserTasksOnly()
        {
            // Arrange
            var userId = "user1";
            var isAdminOrManager = false;

            // Act
            var result = await _taskService.GetTasksAsync(userId, isAdminOrManager);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal("Test Task", result.Data.First().Title);
        }

        [Fact]
        public async Task GetTasksAsync_AdminRole_ReturnsAllTasks()
        {
            // Arrange
            var userId = "admin1";
            var isAdminOrManager = true;

            // Act
            var result = await _taskService.GetTasksAsync(userId, isAdminOrManager);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data); // We only have one task in test data
        }

        [Fact]
        public async Task CreateTaskAsync_ValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var createTaskDto = new CreateTaskDto
            {
                Title = "New Task",
                Description = "New Description",
                Priority = TaskPriority.High
            };
            var userId = "user1";

            // Act
            var result = await _taskService.CreateTaskAsync(createTaskDto, userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("New Task", result.Data.Title);
            Assert.Equal("New Description", result.Data.Description);
            Assert.Equal(TaskPriority.High, result.Data.Priority);
            Assert.Equal(userId, result.Data.UserId);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ValidTaskAndOwner_ReturnsTask()
        {
            // Arrange
            var taskId = 1;
            var requestingUserId = "user1";
            var isAdminOrManager = false;

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId, requestingUserId, isAdminOrManager);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Test Task", result.Data.Title);
        }

        [Fact]
        public async Task GetTaskByIdAsync_InvalidTaskId_ReturnsNotFound()
        {
            // Arrange
            var taskId = 999;
            var requestingUserId = "user1";
            var isAdminOrManager = false;

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId, requestingUserId, isAdminOrManager);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Task not found", result.Message);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetTaskByIdAsync_UnauthorizedUser_ReturnsAccessDenied()
        {
            // Arrange
            var taskId = 1;
            var requestingUserId = "unauthorized_user";
            var isAdminOrManager = false;

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId, requestingUserId, isAdminOrManager);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Access denied", result.Message);
            Assert.Equal(403, result.StatusCode);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}