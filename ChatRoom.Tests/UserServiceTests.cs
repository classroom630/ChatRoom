using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ChatRoom.API.Services;
using ChatRoom.API.Services.Interfaces;
using ChatRoom.Models.Entities;
using ChatRoom.Models.DTOs;

namespace ChatRoom.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userManagerMock = MockUserManager<ApplicationUser>();
            _roleManagerMock = MockRoleManager();
            _emailServiceMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<UserService>>();

            _userService = new UserService(
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _emailServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetUserByIdAsync_ValidUserId_ReturnsUser()
        {
            // Arrange
            var userId = "user1";
            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(userId, result.Data.Id);
            Assert.Equal("test@example.com", result.Data.Email);
            Assert.Equal("Test", result.Data.FirstName);
            Assert.Equal("User", result.Data.LastName);
            Assert.Contains("User", result.Data.Roles);
        }

        [Fact]
        public async Task GetUserByIdAsync_InvalidUserId_ReturnsNotFound()
        {
            // Arrange
            var userId = "invalid_user";

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task CreateUserAsync_ValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                Email = "newuser@example.com",
                Password = "Password123!",
                FirstName = "New",
                LastName = "User",
                Role = "User"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(createUserDto.Email))
                .ReturnsAsync((ApplicationUser)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), createUserDto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), createUserDto.Role))
                .ReturnsAsync(IdentityResult.Success);
            _emailServiceMock.Setup(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _userService.CreateUserAsync(createUserDto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(createUserDto.Email, result.Data.Email);
            Assert.Equal(createUserDto.FirstName, result.Data.FirstName);
            Assert.Equal(createUserDto.LastName, result.Data.LastName);
            Assert.Equal("User created successfully", result.Message);
        }

        [Fact]
        public async Task CreateUserAsync_ExistingEmail_ReturnsErrorResponse()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                Email = "existing@example.com",
                Password = "Password123!",
                FirstName = "Existing",
                LastName = "User",
                Role = "User"
            };

            var existingUser = new ApplicationUser { Email = createUserDto.Email };
            _userManagerMock.Setup(x => x.FindByEmailAsync(createUserDto.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _userService.CreateUserAsync(createUserDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Email is already registered", result.Message);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task AssignRoleAsync_ValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var assignRoleDto = new AssignRoleDto
            {
                UserId = "user1",
                Role = "Manager"
            };

            var user = new ApplicationUser { Id = assignRoleDto.UserId };
            _userManagerMock.Setup(x => x.FindByIdAsync(assignRoleDto.UserId))
                .ReturnsAsync(user);
            _roleManagerMock.Setup(x => x.RoleExistsAsync(assignRoleDto.Role))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });
            _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(user, assignRoleDto.Role))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.AssignRoleAsync(assignRoleDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Role assigned successfully", result.Message);
        }

        private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var userManager = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            return userManager;
        }

        private static Mock<RoleManager<IdentityRole>> MockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            var roleManager = new Mock<RoleManager<IdentityRole>>(store.Object, null, null, null, null);
            return roleManager;
        }
    }
}