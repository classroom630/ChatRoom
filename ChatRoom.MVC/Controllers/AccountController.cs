using Microsoft.AspNetCore.Mvc;
using ChatRoom.Models.ViewModels;
using ChatRoom.Models.DTOs;
using ChatRoom.MVC.Services;
using ChatRoom.MVC.Extensions;

namespace ChatRoom.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IApiService apiService, ILogger<AccountController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Check if already logged in
            var token = HttpContext.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var loginDto = new LoginDto
                {
                    Email = model.Email,
                    Password = model.Password
                };

                var result = await _apiService.LoginAsync(loginDto);

                if (result.Success && result.Data != null)
                {
                    // Store authentication data in session
                    HttpContext.Session.SetString("AccessToken", result.Data.AccessToken);
                    HttpContext.Session.SetString("RefreshToken", result.Data.RefreshToken);
                    HttpContext.Session.SetObject("CurrentUser", result.Data.User);

                    // Redirect based on return URL or to dashboard
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    ModelState.AddModelError("", result.Message ?? "Login failed");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            // Check if already logged in
            var token = HttpContext.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var registerDto = new RegisterDto
                {
                    Email = model.Email,
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = "User" // Default role for registration
                };

                var result = await _apiService.RegisterAsync(registerDto);

                if (result.Success && result.Data != null)
                {
                    // Store authentication data in session
                    HttpContext.Session.SetString("AccessToken", result.Data.AccessToken);
                    HttpContext.Session.SetString("RefreshToken", result.Data.RefreshToken);
                    HttpContext.Session.SetObject("CurrentUser", result.Data.User);

                    TempData["SuccessMessage"] = "Registration successful! Welcome to ChatRoom.";
                    return RedirectToAction("Index", "Dashboard");
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
                        ModelState.AddModelError("", result.Message ?? "Registration failed");
                    }
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            // Clear session
            HttpContext.Session.Clear();

            TempData["InfoMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }
    }
}