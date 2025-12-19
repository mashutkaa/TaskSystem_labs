using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;

// 1. Проєкти BLL
using BLL.Services;
using BLL.DTO;
using BLL.Infrastructure;

// 2. Проєкти DAL
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DAL.UnitOfWork;

// 3. ВИРІШЕННЯ КОНФЛІКТІВ ІМЕН (ALIASES)
// Оскільки імена класів збігаються зі стандартними системними, ми вказуємо явно:
using Task = DAL.Entities.Task;             // Щоб не плутати з System.Threading.Tasks.Task
using TaskStatus = DAL.Entities.TaskStatus; // Щоб не плутати з System.Threading.Tasks.TaskStatus

namespace BLL.Tests
{
    public class TaskServiceTests
    {
        // Моки
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<ITaskRepository> _mockTaskRepo;
        private readonly Mock<IRepository<User>> _mockUserRepo;

        private readonly TaskService _service;

        public TaskServiceTests()
        {
            // Ініціалізуємо моки
            _mockUow = new Mock<IUnitOfWork>();
            _mockTaskRepo = new Mock<ITaskRepository>();
            _mockUserRepo = new Mock<IRepository<User>>();

            // Налаштовуємо UnitOfWork
            _mockUow.Setup(u => u.Tasks).Returns(_mockTaskRepo.Object);
            _mockUow.Setup(u => u.Users).Returns(_mockUserRepo.Object);

            // Створюємо сервіс
            _service = new TaskService(_mockUow.Object);
        }

        #region CreateTask Tests

        [Fact]
        public void CreateTask_ShouldThrowException_WhenTitleIsEmpty()
        {
            // Arrange
            var taskDto = new TaskDTO { Title = "", Description = "Test" };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => _service.CreateTask(taskDto, 1));
            Assert.Equal("Title", ex.Property);
        }

        [Fact]
        public void CreateTask_ShouldThrowException_WhenDeadlineIsInPast()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Title = "Valid Title",
                Deadline = DateTime.Now.AddDays(-1) // Вчора
            };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => _service.CreateTask(taskDto, 1));
            Assert.Equal("Deadline", ex.Property);
        }

        [Fact]
        public void CreateTask_ShouldThrowException_WhenAssigneeNotFound()
        {
            // Arrange
            int nonExistentUserId = 999;
            var taskDto = new TaskDTO { Title = "Valid Title", AssigneeId = nonExistentUserId };

            // Налаштовуємо мок
            _mockUserRepo.Setup(repo => repo.Get(nonExistentUserId)).Returns((User)null);

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => _service.CreateTask(taskDto, 1));
            Assert.Equal("AssigneeId", ex.Property);
        }

        [Fact]
        public void CreateTask_ShouldCallRepository_WhenDataIsValid()
        {
            // Arrange
            int creatorId = 1;
            int assigneeId = 2;
            var taskDto = new TaskDTO
            {
                Title = "Good Task",
                AssigneeId = assigneeId,
                Deadline = DateTime.Now.AddDays(1)
            };

            // Налаштовуємо мок
            _mockUserRepo.Setup(repo => repo.Get(assigneeId)).Returns(new User { UserId = assigneeId });

            // Act
            _service.CreateTask(taskDto, creatorId);

            // Assert
            _mockTaskRepo.Verify(repo => repo.Create(It.Is<Task>(t =>
                t.Title == taskDto.Title &&
                t.CreatorId == creatorId
            )), Times.Once);

            _mockUow.Verify(u => u.Save(), Times.Once);
        }

        #endregion

        #region GetTasks Tests

        [Fact]
        public void GetTasks_ShouldReturnAllTasks_WhenUserIdIsNull()
        {
            // Arrange
            // Тепер TaskStatus тут - це саме твій клас із DAL, а не системний enum
            var tasksFromDb = new List<Task>
            {
                new Task { TaskId = 1, Title = "T1", Status = new TaskStatus { Name = "New" } },
                new Task { TaskId = 2, Title = "T2", Status = new TaskStatus { Name = "In Progress" } }
            };

            _mockTaskRepo.Setup(repo => repo.GetAll()).Returns(tasksFromDb);

            // Act
            var result = _service.GetTasks(null);

            // Assert
            Assert.Equal(2, result.Count());
            _mockTaskRepo.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public void GetTasks_ShouldReturnFilteredTasks_WhenUserIdIsProvided()
        {
            // Arrange
            int userId = 5;
            var userTasks = new List<Task>
            {
                new Task { TaskId = 3, Title = "User Task", AssigneeId = userId }
            };

            _mockTaskRepo.Setup(repo => repo.GetTasksByAssignee(userId)).Returns(userTasks);

            // Act
            var result = _service.GetTasks(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("User Task", result.First().Title);
            _mockTaskRepo.Verify(repo => repo.GetTasksByAssignee(userId), Times.Once);
        }

        #endregion

        #region GetOverdueTasks Tests

        [Fact]
        public void GetOverdueTasks_ShouldReturnOnlyOverdueTasks()
        {
            // Arrange
            var overdueTasks = new List<Task>
            {
                new Task { TaskId = 10, Title = "Late Task", Deadline = DateTime.Now.AddDays(-5) }
            };

            _mockTaskRepo.Setup(repo => repo.GetOverdueTasks()).Returns(overdueTasks);

            // Act
            var result = _service.GetOverdueTasks();

            // Assert
            Assert.Single(result);
            Assert.Equal("Late Task", result.First().Title);
        }

        #endregion
    }
}