using Microsoft.AspNetCore.SignalR;
using ChatRoom.Services;
using ChatRoom.Models;

namespace ChatRoom.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IUserService _userService;
        private readonly IChatService _chatService;

        public ChatHub(IUserService userService, IChatService chatService)
        {
            _userService = userService;
            _chatService = chatService;
        }

        public async Task JoinChat(string username)
        {
            var isAvailable = await _userService.IsUsernameAvailableAsync(username);
            if (!isAvailable)
            {
                await Clients.Caller.SendAsync("UsernameUnavailable", "Username is already taken");
                return;
            }

            var user = await _userService.AddUserAsync(username, Context.ConnectionId);
            if (user != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "ChatRoom");
                await Clients.Caller.SendAsync("JoinedChat", user);
                
                var allUsers = await _userService.GetAllUsersAsync();
                await Clients.Group("ChatRoom").SendAsync("UserListUpdated", allUsers);
                await Clients.Others.SendAsync("UserJoined", user);
            }
        }

        public async Task SendMessage(string toUserId, string message)
        {
            var fromUser = await _userService.GetUserByConnectionIdAsync(Context.ConnectionId);
            if (fromUser == null) return;

            var toUser = await _userService.GetUserByIdAsync(toUserId);
            if (toUser == null) return;

            var chatMessage = await _chatService.SendMessageAsync(fromUser.Id, toUserId, message);
            
            // Send to both sender and receiver
            await Clients.Client(fromUser.ConnectionId).SendAsync("MessageReceived", chatMessage);
            await Clients.Client(toUser.ConnectionId).SendAsync("MessageReceived", chatMessage);
        }

        public async Task StartCall(string toUserId, string callType)
        {
            var fromUser = await _userService.GetUserByConnectionIdAsync(Context.ConnectionId);
            if (fromUser == null) return;

            var toUser = await _userService.GetUserByIdAsync(toUserId);
            if (toUser == null) return;

            var callRequest = new CallRequest
            {
                FromUserId = fromUser.Id,
                ToUserId = toUserId,
                FromUsername = fromUser.Username,
                ToUsername = toUser.Username,
                Type = callType == "video" ? CallType.Video : CallType.Audio
            };

            await Clients.Client(toUser.ConnectionId).SendAsync("IncomingCall", callRequest);
        }

        public async Task AcceptCall(string callId, string fromUserId)
        {
            var toUser = await _userService.GetUserByConnectionIdAsync(Context.ConnectionId);
            if (toUser == null) return;

            var fromUser = await _userService.GetUserByIdAsync(fromUserId);
            if (fromUser == null) return;

            await Clients.Client(fromUser.ConnectionId).SendAsync("CallAccepted", callId, toUser);
            await Clients.Client(toUser.ConnectionId).SendAsync("CallStarted", callId, fromUser);
        }

        public async Task RejectCall(string callId, string fromUserId)
        {
            var fromUser = await _userService.GetUserByIdAsync(fromUserId);
            if (fromUser == null) return;

            await Clients.Client(fromUser.ConnectionId).SendAsync("CallRejected", callId);
        }

        public async Task EndCall(string callId, string otherUserId)
        {
            var otherUser = await _userService.GetUserByIdAsync(otherUserId);
            if (otherUser == null) return;

            await Clients.Client(otherUser.ConnectionId).SendAsync("CallEnded", callId);
        }

        // WebRTC signaling
        public async Task SendOffer(string toUserId, object offer)
        {
            var toUser = await _userService.GetUserByIdAsync(toUserId);
            if (toUser == null) return;

            await Clients.Client(toUser.ConnectionId).SendAsync("ReceiveOffer", offer);
        }

        public async Task SendAnswer(string toUserId, object answer)
        {
            var toUser = await _userService.GetUserByIdAsync(toUserId);
            if (toUser == null) return;

            await Clients.Client(toUser.ConnectionId).SendAsync("ReceiveAnswer", answer);
        }

        public async Task SendIceCandidate(string toUserId, object candidate)
        {
            var toUser = await _userService.GetUserByIdAsync(toUserId);
            if (toUser == null) return;

            await Clients.Client(toUser.ConnectionId).SendAsync("ReceiveIceCandidate", candidate);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = await _userService.GetUserByConnectionIdAsync(Context.ConnectionId);
            if (user != null)
            {
                await _userService.RemoveUserAsync(user.Id);
                var allUsers = await _userService.GetAllUsersAsync();
                await Clients.Group("ChatRoom").SendAsync("UserListUpdated", allUsers);
                await Clients.Others.SendAsync("UserLeft", user);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}