using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DAL.EF;
using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace DAL.Repositories.Impl
{
    // ВИПРАВЛЕННЯ: BaseRepository<DAL.Entities.Task>
    public class TaskRepository : BaseRepository<DAL.Entities.Task>, ITaskRepository
    {
        public TaskRepository(TaskContext context) : base(context)
        {
        }

        public new IEnumerable<DAL.Entities.Task> GetAll()
        {
            return _context.Tasks
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Assignee)
                .Include(t => t.Creator)
                .ToList();
        }

        public IEnumerable<DAL.Entities.Task> GetTasksByAssignee(int userId)
        {
            return _context.Tasks
                .Include(t => t.Status)
                .Include(t => t.Priority) // Додав ще пріоритет для краси
                .Where(t => t.AssigneeId == userId)
                .ToList();
        }

        public IEnumerable<DAL.Entities.Task> GetTasksByStatus(string statusName)
        {
            return _context.Tasks
                .Include(t => t.Status)
                .Where(t => t.Status.Name == statusName)
                .ToList();
        }

        public IEnumerable<DAL.Entities.Task> GetOverdueTasks()
        {
            return _context.Tasks
               .Include(t => t.Status)
               .Where(t => t.Deadline < DateTime.Now && t.Status.Name != "Completed")
               .ToList();
        }
    }
}