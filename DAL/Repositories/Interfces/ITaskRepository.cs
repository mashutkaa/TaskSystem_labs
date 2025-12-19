using System.Collections.Generic;
using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    // ВИПРАВЛЕННЯ: IRepository<DAL.Entities.Task>
    public interface ITaskRepository : IRepository<DAL.Entities.Task>
    {
        IEnumerable<DAL.Entities.Task> GetTasksByAssignee(int userId);
        IEnumerable<DAL.Entities.Task> GetTasksByStatus(string statusName);
        IEnumerable<DAL.Entities.Task> GetOverdueTasks();
    }
}