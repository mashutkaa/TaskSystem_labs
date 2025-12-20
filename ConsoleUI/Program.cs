using BLL.DTO;
using BLL.Infrastructure;
using BLL.Interfaces;
using BLL.Services;
using DAL.EF;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace ConsoleUI
{
    class Program
    {
        // Змінна для поточного користувача (спрощена імітація сесії)
        static int currentUserId;
        static string currentUserRole;

        static void Main(string[] args)
        {
            // Налаштування БД
            string connectionString = "server=localhost;database=task_management_system;user=root;password=1111;";
            var optionsBuilder = new DbContextOptionsBuilder<TaskContext>();

            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));

            using (var context = new TaskContext(optionsBuilder.Options))
            {
                // Створюємо базу, якщо її немає
                context.Database.EnsureCreated();

                // Створюємо ланцюжок залежностей: Context -> UnitOfWork -> Service
                IUnitOfWork uow = new UnitOfWork(context);
                ITaskService taskService = new TaskService(uow);

                Console.OutputEncoding = System.Text.Encoding.UTF8;

                // 1. АВТОРИЗАЦІЯ
                currentUserId = 2;
                currentUserRole = "Manager";

                Console.WriteLine($"Вхід виконано: {currentUserRole} (ID: {currentUserId})");

                // --- МЕНЮ ---
                while (true)
                {
                    Console.WriteLine("\n=== TASK MANAGER (LAYERED ARCHITECTURE) ===");
                    Console.WriteLine("1. Показати всі завдання");
                    Console.WriteLine("2. Показати мої завдання");
                    Console.WriteLine("3. Створити завдання (через BLL)");
                    Console.WriteLine("4. Прострочені завдання");
                    Console.WriteLine("0. Вихід");
                    Console.Write("Вибір: ");

                    string choice = Console.ReadLine();
                    try
                    {
                        switch (choice)
                        {
                            case "1":
                                ShowTasks(taskService.GetTasks(null));
                                break;
                            case "2":
                                ShowTasks(taskService.GetTasks(4)); // Припустимо, ми дивимось за Петра (ID 4)
                                break;
                            case "3":
                                CreateTaskUI(taskService);
                                break;
                            case "4":
                                ShowTasks(taskService.GetOverdueTasks());
                                break;
                            case "0":
                                return;
                        }
                    }

                    catch (BLL.Infrastructure.ValidationException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[ВАЛІДАЦІЯ]: {ex.Message} (Поле: {ex.Property})");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[ПОМИЛКА]: {ex.Message}");
                        Console.ResetColor();
                    }
                }
            }
        }

        static void ShowTasks(System.Collections.Generic.IEnumerable<TaskDTO> tasks)
        {
            if (!tasks.Any())
            {
                Console.WriteLine("Список порожній.");
                return;
            }

            Console.WriteLine("{0,-5} | {1,-30} | {2,-15} | {3,-15}", "ID", "Назва", "Виконавець", "Статус");
            Console.WriteLine(new string('-', 70));
            foreach (var t in tasks)
            {
                Console.WriteLine("{0,-5} | {1,-30} | {2,-15} | {3,-15}",
                    t.TaskId,
                    t.Title.Length > 29 ? t.Title.Substring(0, 26) + "..." : t.Title,
                    t.AssigneeName ?? "---",
                    t.StatusName);
            }
        }

        static void CreateTaskUI(ITaskService service)
        {
            Console.WriteLine("\n--- Створення завдання ---");

            Console.Write("Назва: ");
            string title = Console.ReadLine();

            Console.Write("Опис: ");
            string desc = Console.ReadLine();

            Console.Write("Днів до дедлайну (введіть мінус, щоб перевірити валідацію): ");
            int.TryParse(Console.ReadLine(), out int days);
            DateTime deadline = DateTime.Now.AddDays(days);

            Console.Write("ID виконавця (наприклад, 4): ");
            int.TryParse(Console.ReadLine(), out int assigneeId);

            var taskDto = new TaskDTO
            {
                Title = title,
                Description = desc,
                Deadline = deadline,
                AssigneeId = assigneeId > 0 ? assigneeId : null
            };

            // Тут полетить ValidationException, якщо дані некоректні
            service.CreateTask(taskDto, currentUserId);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Завдання успішно створено!");
            Console.ResetColor();
        }
    }
}