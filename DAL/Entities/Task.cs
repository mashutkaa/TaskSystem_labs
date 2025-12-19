using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities
{
    public class Task
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? Deadline { get; set; }

        // Зовнішні ключі та зв'язки

        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public TaskStatus Status { get; set; }

        public int PriorityId { get; set; }
        [ForeignKey("PriorityId")]
        public Priority Priority { get; set; }

        public int CreatorId { get; set; }
        [ForeignKey("CreatorId")]
        public User Creator { get; set; }

        public int? AssigneeId { get; set; }
        [ForeignKey("AssigneeId")]
        public User Assignee { get; set; }
    }
}