using System.ComponentModel.DataAnnotations;
using ChatRoom.Models.DTOs;
using ChatRoom.Models.Entities;

namespace ChatRoom.Models.ViewModels
{
    public class CreateTaskViewModel
    {
        [Required]
        [Display(Name = "Task Title")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [MaxLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Priority")]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    }

    public class EditTaskViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Task Title")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [MaxLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Priority")]
        public TaskPriority Priority { get; set; }

        [Display(Name = "Completed")]
        public bool IsCompleted { get; set; }

        public string UserId { get; set; } = string.Empty;
    }

    public class TaskListViewModel
    {
        public List<TaskDto> Tasks { get; set; } = new List<TaskDto>();
        public string Filter { get; set; } = "all"; // all, completed, pending
        public TaskPriority? PriorityFilter { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}