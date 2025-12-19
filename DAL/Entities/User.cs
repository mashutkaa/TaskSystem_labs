using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } // Admin, Manager, Employee

        // Навігаційні властивості
        public ICollection<Task> CreatedTasks { get; set; } // Завдання, які створив менеджер
        public ICollection<Task> AssignedTasks { get; set; } // Завдання, призначені співробітнику
    }
}