using ChatRoom.Models;
using System.Collections.Concurrent;

namespace ChatRoom.Services
{
    public interface IUserService
    {
        Task<User?> AddUserAsync(string username, string connectionId);
        Task<User?> GetUserByIdAsync(string userId);
        Task<User?> GetUserByConnectionIdAsync(string connectionId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<List<User>> GetAllUsersAsync();
        Task<bool> RemoveUserAsync(string userId);
        Task<bool> RemoveUserByConnectionIdAsync(string connectionId);
        Task<bool> UpdateUserConnectionAsync(string userId, string connectionId);
        Task<bool> IsUsernameAvailableAsync(string username);
    }

    public class UserService : IUserService
    {
        private readonly ConcurrentDictionary<string, User> _users = new();

        public Task<User?> AddUserAsync(string username, string connectionId)
        {
            var userId = Guid.NewGuid().ToString();
            var user = new User
            {
                Id = userId,
                Username = username,
                ConnectionId = connectionId,
                JoinedAt = DateTime.UtcNow,
                IsOnline = true
            };

            return Task.FromResult(_users.TryAdd(userId, user) ? user : null);
        }

        public Task<User?> GetUserByIdAsync(string userId)
        {
            _users.TryGetValue(userId, out var user);
            return Task.FromResult(user);
        }

        public Task<User?> GetUserByConnectionIdAsync(string connectionId)
        {
            var user = _users.Values.FirstOrDefault(u => u.ConnectionId == connectionId);
            return Task.FromResult(user);
        }

        public Task<User?> GetUserByUsernameAsync(string username)
        {
            var user = _users.Values.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }

        public Task<List<User>> GetAllUsersAsync()
        {
            return Task.FromResult(_users.Values.Where(u => u.IsOnline).ToList());
        }

        public Task<bool> RemoveUserAsync(string userId)
        {
            return Task.FromResult(_users.TryRemove(userId, out _));
        }

        public Task<bool> RemoveUserByConnectionIdAsync(string connectionId)
        {
            var user = _users.Values.FirstOrDefault(u => u.ConnectionId == connectionId);
            if (user != null)
            {
                return Task.FromResult(_users.TryRemove(user.Id, out _));
            }
            return Task.FromResult(false);
        }

        public Task<bool> UpdateUserConnectionAsync(string userId, string connectionId)
        {
            if (_users.TryGetValue(userId, out var user))
            {
                user.ConnectionId = connectionId;
                user.IsOnline = true;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> IsUsernameAvailableAsync(string username)
        {
            var isAvailable = !_users.Values.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.IsOnline);
            return Task.FromResult(isAvailable);
        }
    }
}