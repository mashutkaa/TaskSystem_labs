using System;

namespace BLL.DTO
{
    public class TaskDTO
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? Deadline { get; set; }

        // передаємо назви, а не ID, щоб UI не мучився
        public string StatusName { get; set; }
        public string PriorityName { get; set; }

        public int? AssigneeId { get; set; }
        public string AssigneeName { get; set; }
    }
}