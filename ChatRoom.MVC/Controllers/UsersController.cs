using Microsoft.AspNetCore.Mvc;
using ChatRoom.Models.ViewModels;
using ChatRoom.Models.DTOs;
using ChatRoom.MVC.Services;
using ChatRoom.MVC.Extensions;

namespace ChatRoom.MVC.Controllers
{
    [Route("Users")]
    public class UsersController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IApiService apiService, ILogger<UsersController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var currentUser = HttpContext.Session.GetObject<UserDto>("CurrentUser");

            if (string.IsNullOrEmpty(token) || currentUser == null || !currentUser.Roles.Contains("Admin"))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _apiService.GetUsersAsync(token);
                if (result.Success && result.Data != null)
                {
                    var model = new UserListViewModel
                    {
                        Users = result.Data
                    };
                    return View(model);
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message ?? "Failed to load users";
                    return View(new UserListViewModel());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["ErrorMessage"] = "Error loading users.";
                return View(new UserListViewModel());
            }
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var currentUser = HttpContext.Session.GetObject<UserDto>("CurrentUser");

            if (string.IsNullOrEmpty(token) || currentUser == null || !currentUser.Roles.Contains("Admin"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var currentUser = HttpContext.Session.GetObject<UserDto>("CurrentUser");

            if (string.IsNullOrEmpty(token) || currentUser == null || !currentUser.Roles.Contains("Admin"))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var createUserDto = new CreateUserDto
                {
                    Email = model.Email,
                    Password = model.Password,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = model.Role
                };

                var result = await _apiService.CreateUserAsync(createUserDto, token);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "User created successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    if (result.Errors?.Any() == true)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", result.Message ?? "Failed to create user");
                    }
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                ModelState.AddModelError("", "An error occurred while creating the user.");
                return View(model);
            }
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var currentUser = HttpContext.Session.GetObject<UserDto>("CurrentUser");

            if (string.IsNullOrEmpty(token) || currentUser == null || !currentUser.Roles.Contains("Admin"))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var userResult = await _apiService.GetUserByIdAsync(id, token);
                var rolesResult = await _apiService.GetRolesAsync(token);

                if (userResult.Success && userResult.Data != null)
                {
                    var model = new EditUserViewModel
                    {
                        Id = userResult.Data.Id,
                        Email = userResult.Data.Email,
                        FirstName = userResult.Data.FirstName,
                        LastName = userResult.Data.LastName,
                        Role = userResult.Data.Roles.FirstOrDefault() ?? "User",
                        AvailableRoles = rolesResult.Success && rolesResult.Data != null 
                                          ? rolesResult.Data 
                                          : new List<string> { "Admin", "Manager", "User" }
                    };
                    return View(model);
                }
                else
                {
                    TempData["ErrorMessage"] = userResult.Message ?? "User not found";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user for edit");
                TempData["ErrorMessage"] = "Error loading user.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var currentUser = HttpContext.Session.GetObject<UserDto>("CurrentUser");

            if (string.IsNullOrEmpty(token) || currentUser == null || !currentUser.Roles.Contains("Admin"))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                // Reload available roles
                try
                {
                    var rolesResult = await _apiService.GetRolesAsync(token);
                    model.AvailableRoles = rolesResult.Success && rolesResult.Data != null 
                                          ? rolesResult.Data 
                                          : new List<string> { "Admin", "Manager", "User" };
                }
                catch
                {
                    model.AvailableRoles = new List<string> { "Admin", "Manager", "User" };
                }
                return View(model);
            }

            try
            {
                var updateUserDto = new UpdateUserDto
                {
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var updateResult = await _apiService.UpdateUserAsync(id, updateUserDto, token);

                if (updateResult.Success)
                {
                    // Update role if necessary
                    var assignRoleDto = new AssignRoleDto
                    {
                        UserId = id,
                        Role = model.Role
                    };

                    var roleResult = await _apiService.AssignRoleAsync(assignRoleDto, token);

                    if (roleResult.Success)
                    {
                        TempData["SuccessMessage"] = "User updated successfully!";
                    }
                    else
                    {
                        TempData["WarningMessage"] = "User updated but role assignment failed.";
                    }

                    return RedirectToAction("Index");
                }
                else
                {
                    if (updateResult.Errors?.Any() == true)
                    {
                        foreach (var error in updateResult.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", updateResult.Message ?? "Failed to update user");
                    }

                    // Reload available roles
                    try
                    {
                        var rolesResult = await _apiService.GetRolesAsync(token);
                        model.AvailableRoles = rolesResult.Success && rolesResult.Data != null 
                                              ? rolesResult.Data 
                                              : new List<string> { "Admin", "Manager", "User" };
                    }
                    catch
                    {
                        model.AvailableRoles = new List<string> { "Admin", "Manager", "User" };
                    }

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                ModelState.AddModelError("", "An error occurred while updating the user.");

                // Reload available roles
                try
                {
                    var rolesResult = await _apiService.GetRolesAsync(token);
                    model.AvailableRoles = rolesResult.Success && rolesResult.Data != null 
                                          ? rolesResult.Data 
                                          : new List<string> { "Admin", "Manager", "User" };
                }
                catch
                {
                    model.AvailableRoles = new List<string> { "Admin", "Manager", "User" };
                }

                return View(model);
            }
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var currentUser = HttpContext.Session.GetObject<UserDto>("CurrentUser");

            if (string.IsNullOrEmpty(token) || currentUser == null || !currentUser.Roles.Contains("Admin"))
            {
                return RedirectToAction("Login", "Account");
            }

            // Prevent self-deletion
            if (id == currentUser.Id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction("Index");
            }

            try
            {
                var result = await _apiService.DeleteUserAsync(id, token);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "User deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message ?? "Failed to delete user";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                TempData["ErrorMessage"] = "An error occurred while deleting the user.";
            }

            return RedirectToAction("Index");
        }
    }
}