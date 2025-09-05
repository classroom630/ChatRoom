using Microsoft.AspNetCore.Mvc;
using ChatRoom.Services;
using ChatRoom.Models;

namespace ChatRoom.Controllers
{
    public class ChatController : Controller
    {
        private readonly IUserService _userService;
        private readonly IChatService _chatService;

        public ChatController(IUserService userService, IChatService chatService)
        {
            _userService = userService;
            _chatService = chatService;
        }

        public IActionResult Index(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Username = username;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CheckUsername([FromBody] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Json(new { success = false, message = "Username cannot be empty" });
            }

            var isAvailable = await _userService.IsUsernameAvailableAsync(username);
            if (!isAvailable)
            {
                return Json(new { success = false, message = "Username is already taken" });
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Json(users);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(string userId1, string userId2)
        {
            var messages = await _chatService.GetMessagesAsync(userId1, userId2);
            return Json(messages);
        }
    }
}