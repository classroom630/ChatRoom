using ChatRoom.Models;
using System.Collections.Concurrent;

namespace ChatRoom.Services
{
    public interface IChatService
    {
        Task<ChatMessage> SendMessageAsync(string fromUserId, string toUserId, string message);
        Task<List<ChatMessage>> GetMessagesAsync(string userId1, string userId2);
        Task<List<ChatMessage>> GetRecentMessagesAsync(string userId, int count = 50);
    }

    public class ChatService : IChatService
    {
        private readonly ConcurrentBag<ChatMessage> _messages = new();
        private readonly IUserService _userService;

        public ChatService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<ChatMessage> SendMessageAsync(string fromUserId, string toUserId, string message)
        {
            var fromUser = await _userService.GetUserByIdAsync(fromUserId);
            var toUser = await _userService.GetUserByIdAsync(toUserId);

            var chatMessage = new ChatMessage
            {
                FromUserId = fromUserId,
                ToUserId = toUserId,
                FromUsername = fromUser?.Username ?? "Unknown",
                ToUsername = toUser?.Username ?? "Unknown",
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            _messages.Add(chatMessage);
            return chatMessage;
        }

        public Task<List<ChatMessage>> GetMessagesAsync(string userId1, string userId2)
        {
            var messages = _messages
                .Where(m => (m.FromUserId == userId1 && m.ToUserId == userId2) ||
                           (m.FromUserId == userId2 && m.ToUserId == userId1))
                .OrderBy(m => m.Timestamp)
                .ToList();

            return Task.FromResult(messages);
        }

        public Task<List<ChatMessage>> GetRecentMessagesAsync(string userId, int count = 50)
        {
            var messages = _messages
                .Where(m => m.FromUserId == userId || m.ToUserId == userId)
                .OrderByDescending(m => m.Timestamp)
                .Take(count)
                .OrderBy(m => m.Timestamp)
                .ToList();

            return Task.FromResult(messages);
        }
    }
}