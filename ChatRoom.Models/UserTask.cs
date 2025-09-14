using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatRoom.Models.Entities
{
    public class UserTask
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsCompleted { get; set; } = false;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Foreign key
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;
    }

    public enum TaskPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Urgent = 4
    }
}