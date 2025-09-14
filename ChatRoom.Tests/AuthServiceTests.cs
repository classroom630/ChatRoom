using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ChatRoom.API.Services;
using ChatRoom.API.Services.Interfaces;
using ChatRoom.API.Data;
using ChatRoom.Models.Entities;
using ChatRoom.Models.DTOs;

namespace ChatRoom.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // Setup mocks
            _userManagerMock = MockUserManager<ApplicationUser>();
            _signInManagerMock = MockSignInManager(_userManagerMock.Object);
            _jwtServiceMock = new Mock<IJwtService>();
            _emailServiceMock = new Mock<IEmailService>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            // Setup configuration mock
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x["ExpiryMinutes"]).Returns("60");
            jwtSection.Setup(x => x["RefreshTokenExpiryDays"]).Returns("7");
            _configurationMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSection.Object);

            _authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _jwtServiceMock.Object,
                _emailServiceMock.Object,
                _context,
                _configurationMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsSuccessResponse()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "Password123!" };
            var user = new ApplicationUser { Id = "1", Email = "test@example.com", UserName = "test@example.com" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, false))
                .ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);
            _jwtServiceMock.Setup(x => x.GenerateAccessTokenAsync(user))
                .ReturnsAsync("access_token");
            _jwtServiceMock.Setup(x => x.GenerateRefreshTokenAsync())
                .ReturnsAsync("refresh_token");
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("access_token", result.Data.AccessToken);
            Assert.Equal("refresh_token", result.Data.RefreshToken);
        }

        [Fact]
        public async Task LoginAsync_InvalidCredentials_ReturnsErrorResponse()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "wrong_password" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid email or password", result.Message);
        }

        [Fact]
        public async Task RegisterAsync_ValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "newuser@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "John",
                LastName = "Doe",
                Role = "User"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((ApplicationUser)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), registerDto.Role))
                .ReturnsAsync(IdentityResult.Success);
            _emailServiceMock.Setup(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _jwtServiceMock.Setup(x => x.GenerateAccessTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("access_token");
            _jwtServiceMock.Setup(x => x.GenerateRefreshTokenAsync())
                .ReturnsAsync("refresh_token");
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Registration successful", result.Message);
        }

        private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var userManager = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            return userManager;
        }

        private static Mock<SignInManager<TUser>> MockSignInManager<TUser>(UserManager<TUser> userManager) where TUser : class
        {
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<TUser>>();
            var signInManager = new Mock<SignInManager<TUser>>(userManager, contextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);
            return signInManager;
        }
    }
}