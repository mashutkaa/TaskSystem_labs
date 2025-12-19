using System;
using System.Collections.Generic;
using System.Linq;
using BLL.DTO;
using BLL.Infrastructure;
using BLL.Interfaces;
using DAL.Entities;
using DAL.UnitOfWork;

using Task = DAL.Entities.Task;

namespace BLL.Services
{
    public class TaskService : ITaskService
    {
        private IUnitOfWork Database { get; set; }

        public TaskService(IUnitOfWork uow)
        {
            Database = uow;
        }

        public void CreateTask(TaskDTO taskDto, int creatorId)
        {
            // 1. ВАЛІДАЦІЯ (Бізнес-правила)
            if (string.IsNullOrWhiteSpace(taskDto.Title))
                throw new ValidationException("Назва завдання не може бути порожньою", "Title");

            if (taskDto.Deadline != null && taskDto.Deadline < DateTime.Now)
                throw new ValidationException("Дедлайн не може бути в минулому", "Deadline");

            // Перевірка, чи існує виконавець
            if (taskDto.AssigneeId != null)
            {
                var user = Database.Users.Get(taskDto.AssigneeId.Value);
                if (user == null)
                    throw new ValidationException("Виконавця з таким ID не знайдено", "AssigneeId");
            }

            // 2. МАПІНГ (DTO -> Entity)
            // Тепер тут не буде помилки, бо ми додали using Task = DAL.Entities.Task;
            var task = new Task
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                Deadline = taskDto.Deadline,
                CreatedAt = DateTime.Now,
                CreatorId = creatorId,
                AssigneeId = taskDto.AssigneeId,
                StatusId = 1, // 'New' за замовчуванням
                PriorityId = 2 // 'Medium' за замовчуванням
            };

            // 3. ЗБЕРЕЖЕННЯ
            Database.Tasks.Create(task);
            Database.Save();
        }

        public IEnumerable<TaskDTO> GetTasks(int? userId = null)
        {
            var entities = userId == null
                ? Database.Tasks.GetAll()
                : Database.Tasks.GetTasksByAssignee(userId.Value);

            return entities.Select(t => new TaskDTO
            {
                TaskId = t.TaskId,
                Title = t.Title,
                Description = t.Description,
                Deadline = t.Deadline,
                StatusName = t.Status?.Name,
                PriorityName = t.Priority?.Name,
                AssigneeId = t.AssigneeId,
                AssigneeName = t.Assignee?.FullName
            }).ToList();
        }

        public IEnumerable<TaskDTO> GetOverdueTasks()
        {
            var entities = Database.Tasks.GetOverdueTasks();

            return entities.Select(t => new TaskDTO
            {
                TaskId = t.TaskId,
                Title = t.Title,
                Deadline = t.Deadline,
                StatusName = t.Status?.Name,
                AssigneeName = t.Assignee?.FullName
            }).ToList();
        }

        public void Dispose()
        {
            Database.Dispose();
        }
    }
}