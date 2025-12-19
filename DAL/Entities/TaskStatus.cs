using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
    public class TaskStatus
    {
        [Key]
        public int StatusId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // Наприклад: New, In Progress, Completed

        // Навігаційна властивість
        public ICollection<Task> Tasks { get; set; }
    }
}