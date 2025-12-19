using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DAL.EF;
using DAL.Entities;
using DAL.UnitOfWork;

// Щоб не плутати системні Task і наші, зробимо псевдонім
using MyTask = DAL.Entities.Task;

namespace ConsoleUI
{
    class Program
    {
        // Змінна для збереження поточного користувача
        static User currentUser = null;

        static void Main(string[] args)
        {
            string connectionString = "server=localhost;database=task_management_system;user=root;password=1111;";

            var optionsBuilder = new DbContextOptionsBuilder<TaskContext>();
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));

            using (var context = new TaskContext(optionsBuilder.Options))
            {
                context.Database.EnsureCreated(); // Гарантуємо, що БД є

                using (var uow = new UnitOfWork(context))
                {
                    Console.OutputEncoding = System.Text.Encoding.UTF8;

                    // 1. ЕТАП ЛОГІНУ
                    if (!Login(uow)) return;

                    // 2. ЕТАП МЕНЮ
                    bool exit = false;
                    while (!exit)
                    {
                        Console.Clear();
                        Console.WriteLine($"=== Система завдань | Користувач: {currentUser.FullName} ({currentUser.Role}) ===");
                        Console.WriteLine("1. Переглянути всі завдання (Перевірка)");
                        Console.WriteLine("2. Створити нове завдання");
                        Console.WriteLine("3. Мої завдання (фільтр по мені)");
                        Console.WriteLine("4. Прострочені завдання");
                        Console.WriteLine("0. Вихід");
                        Console.Write("\nВаш вибір: ");

                        string choice = Console.ReadLine();

                        switch (choice)
                        {
                            case "1":
                                ShowAllTasks(uow);
                                break;
                            case "2":
                                CreateTask(uow);
                                break;
                            case "3":
                                ShowMyTasks(uow);
                                break;
                            case "4":
                                ShowOverdueTasks(uow);
                                break;
                            case "0":
                                exit = true;
                                break;
                            default:
                                Console.WriteLine("Невірний вибір.");
                                break;
                        }

                        if (!exit)
                        {
                            Console.WriteLine("\nНатисніть Enter, щоб продовжити...");
                            Console.ReadLine();
                        }
                    }
                }
            }
        }

        // --- ЛОГІКА АВТОРИЗАЦІЇ ---
        static bool Login(UnitOfWork uow)
        {
            Console.Clear();
            Console.WriteLine("--- ВХІД У СИСТЕМУ ---");

            Console.Write("Email (default: manager@company.com): ");
            string email = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(email)) email = "manager@company.com";

            Console.Write("Password (default: manager_pass_hash): ");
            string password = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(password)) password = "manager_pass_hash";

            currentUser = uow.Users.Find(u => u.Email == email && u.PasswordHash == password).FirstOrDefault();

            if (currentUser == null)
            {
                ColorMessage("Помилка: Невірні дані.", ConsoleColor.Red);
                return false;
            }
            return true;
        }

        // --- 1. ПЕРЕГЛЯД УСІХ (Щоб побачити, що додався запис) ---
        static void ShowAllTasks(UnitOfWork uow)
        {
            Console.WriteLine("\n--- СПИСОК ВСІХ ЗАВДАНЬ У БД ---");
            var tasks = uow.Tasks.GetAll();

            if (!tasks.Any())
            {
                Console.WriteLine("Список порожній.");
                return;
            }

            // Заголовок таблиці
            Console.WriteLine("{0,-5} | {1,-30} | {2,-20} | {3,-15}", "ID", "Назва", "Виконавець", "Статус");
            Console.WriteLine(new string('-', 80));

            foreach (var t in tasks)
            {
                string assignee = t.Assignee != null ? t.Assignee.FullName : "---";
                Console.WriteLine("{0,-5} | {1,-30} | {2,-20} | {3,-15}",
                    t.TaskId,
                    Truncate(t.Title, 29),
                    Truncate(assignee, 19),
                    t.Status?.Name);
            }
        }

        // --- 2. СТВОРЕННЯ ЗАВДАННЯ ---
        static void CreateTask(UnitOfWork uow)
        {
            if (currentUser.Role != "Manager" && currentUser.Role != "Admin")
            {
                ColorMessage("У вас немає прав створювати завдання!", ConsoleColor.Yellow);
                return;
            }

            Console.WriteLine("\n--- ДОДАВАННЯ НОВОГО ЗАВДАННЯ ---");

            Console.Write("Назва: ");
            string title = Console.ReadLine();

            Console.Write("Опис: ");
            string desc = Console.ReadLine();

            Console.Write("Днів до дедлайну: ");
            if (!int.TryParse(Console.ReadLine(), out int days)) days = 3;

            // Вибір виконавця
            Console.WriteLine("\n--- Оберіть виконавця ---");
            var employees = uow.Users.Find(u => u.Role == "Employee");
            foreach (var emp in employees)
            {
                Console.WriteLine($"ID: {emp.UserId} - {emp.FullName}");
            }
            Console.Write("ID виконавця: ");
            int.TryParse(Console.ReadLine(), out int assigneeId);

            // Створення об'єкта
            var newTask = new MyTask
            {
                Title = title,
                Description = desc,
                Deadline = DateTime.Now.AddDays(days),
                CreatedAt = DateTime.Now,
                CreatorId = currentUser.UserId,
                AssigneeId = assigneeId > 0 ? assigneeId : (int?)null, // Якщо 0, то ніхто
                StatusId = 1, // New
                PriorityId = 2 // Medium (для спрощення ставимо дефолт)
            };

            try
            {
                uow.Tasks.Create(newTask);
                uow.Save(); // ФІЗИЧНИЙ ЗАПИС У БД
                ColorMessage("Успішно додано! Перевірте список (пункт 1).", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                ColorMessage($"Помилка збереження: {ex.Message}", ConsoleColor.Red);
                if (ex.InnerException != null) Console.WriteLine(ex.InnerException.Message);
            }
        }

        // --- 3. МОЇ ЗАВДАННЯ ---
        static void ShowMyTasks(UnitOfWork uow)
        {
            Console.WriteLine($"\n--- ЗАВДАННЯ ДЛЯ {currentUser.FullName} ---");
            var myTasks = uow.Tasks.GetTasksByAssignee(currentUser.UserId);

            foreach (var t in myTasks)
            {
                Console.WriteLine($"- {t.Title} (Пріоритет: {t.Priority?.Name})");
            }
        }

        // --- 4. ПРОСТРОЧЕНІ ---
        static void ShowOverdueTasks(UnitOfWork uow)
        {
            Console.WriteLine("\n--- УВАГА! ПРОСТРОЧЕНІ ---");
            var overdue = uow.Tasks.GetOverdueTasks();
            foreach (var t in overdue)
            {
                ColorMessage($"[!] {t.Title} (Дедлайн був: {t.Deadline})", ConsoleColor.Red);
            }
        }

        // Допоміжні методи
        static void ColorMessage(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        static string Truncate(string value, int maxLen)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Length <= maxLen ? value : value.Substring(0, maxLen - 3) + "...";
        }
    }
}