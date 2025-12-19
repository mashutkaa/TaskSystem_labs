using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
    public class Priority
    {
        [Key]
        public int PriorityId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // Low, Medium, High

        public int Level { get; set; } // Числове значення для сортування (1, 2, 3...)

        public ICollection<Task> Tasks { get; set; }
    }
}