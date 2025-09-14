using Microsoft.AspNetCore.Mvc;
using ChatRoom.Models.ViewModels;
using ChatRoom.Models.DTOs;
using ChatRoom.MVC.Services;
using ChatRoom.MVC.Extensions;

namespace ChatRoom.MVC.Controllers
{
    [Route("Dashboard")]
    public class DashboardController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IApiService apiService, ILogger<DashboardController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var currentUser = HttpContext.Session.GetObject<UserDto>("CurrentUser");

            if (string.IsNullOrEmpty(token) || currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var dashboardModel = new DashboardViewModel
                {
                    CurrentUser = currentUser
                };

                // Get tasks
                var tasksResult = await _apiService.GetTasksAsync(token);
                if (tasksResult.Success && tasksResult.Data != null)
                {
                    dashboardModel.Tasks = tasksResult.Data;
                }

                // Get users if admin or manager
                if (currentUser.Roles.Contains("Admin") || currentUser.Roles.Contains("Manager"))
                {
                    var usersResult = await _apiService.GetUsersAsync(token);
                    if (usersResult.Success && usersResult.Data != null)
                    {
                        dashboardModel.Users = usersResult.Data;
                    }
                }

                // Calculate stats
                dashboardModel.Stats = new DashboardStats
                {
                    TotalTasks = dashboardModel.Tasks.Count,
                    CompletedTasks = dashboardModel.Tasks.Count(t => t.IsCompleted),
                    PendingTasks = dashboardModel.Tasks.Count(t => !t.IsCompleted),
                    TotalUsers = dashboardModel.Users.Count,
                    RecentTasks = dashboardModel.Tasks.Count(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                };

                // Return appropriate view based on role
                if (currentUser.Roles.Contains("Admin"))
                {
                    var adminModel = new AdminDashboardViewModel
                    {
                        CurrentUser = dashboardModel.CurrentUser,
                        Tasks = dashboardModel.Tasks,
                        Users = dashboardModel.Users,
                        Stats = dashboardModel.Stats,
                        AvailableRoles = new List<string> { "Admin", "Manager", "User" }
                    };
                    return View("AdminDashboard", adminModel);
                }
                else if (currentUser.Roles.Contains("Manager"))
                {
                    return View("ManagerDashboard", dashboardModel);
                }
                else
                {
                    return View("UserDashboard", dashboardModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["ErrorMessage"] = "Error loading dashboard data.";
                return View("UserDashboard", new DashboardViewModel { CurrentUser = currentUser });
            }
        }
    }
}