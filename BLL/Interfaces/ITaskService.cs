using System.Collections.Generic;
using BLL.DTO;

namespace BLL.Interfaces
{
    public interface ITaskService
    {
        void CreateTask(TaskDTO taskDto, int creatorId);
        IEnumerable<TaskDTO> GetTasks(int? userId = null); // Якщо null - то всі, якщо є ID - то фільтруємо
        IEnumerable<TaskDTO> GetOverdueTasks();
        void Dispose();
    }
}