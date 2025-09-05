namespace ChatRoom.Models
{
    public class CallRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FromUserId { get; set; } = string.Empty;
        public string ToUserId { get; set; } = string.Empty;
        public string FromUsername { get; set; } = string.Empty;
        public string ToUsername { get; set; } = string.Empty;
        public CallType Type { get; set; }
        public CallStatus Status { get; set; } = CallStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum CallType
    {
        Audio,
        Video
    }

    public enum CallStatus
    {
        Pending,
        Accepted,
        Rejected,
        Ended
    }
}