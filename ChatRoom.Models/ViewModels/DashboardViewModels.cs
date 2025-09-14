using ChatRoom.Models.DTOs;

namespace ChatRoom.Models.ViewModels
{
    public class DashboardViewModel
    {
        public UserDto CurrentUser { get; set; } = null!;
        public List<TaskDto> Tasks { get; set; } = new List<TaskDto>();
        public List<UserDto> Users { get; set; } = new List<UserDto>(); // For Admin/Manager
        public DashboardStats Stats { get; set; } = new DashboardStats();
    }

    public class DashboardStats
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int TotalUsers { get; set; } // For Admin/Manager
        public int RecentTasks { get; set; }
    }

    public class AdminDashboardViewModel : DashboardViewModel
    {
        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
}