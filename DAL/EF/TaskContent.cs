using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL.EF
{
    public class TaskContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        // ВИПРАВЛЕННЯ: Пишемо Entities.Task, щоб не плутати з системним
        public DbSet<DAL.Entities.Task> Tasks { get; set; }

        public DbSet<DAL.Entities.TaskStatus> TaskStatuses { get; set; }
        public DbSet<Priority> Priorities { get; set; }

        public TaskContext(DbContextOptions<TaskContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User -> Created Tasks
            modelBuilder.Entity<DAL.Entities.Task>()
                .HasOne(t => t.Creator)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Assigned Tasks
            modelBuilder.Entity<DAL.Entities.Task>()
                .HasOne(t => t.Assignee)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DAL.Entities.TaskStatus>().HasData(
                new DAL.Entities.TaskStatus { StatusId = 1, Name = "New" },
                new DAL.Entities.TaskStatus { StatusId = 2, Name = "In Progress" },
                new DAL.Entities.TaskStatus { StatusId = 3, Name = "Completed" },
                new DAL.Entities.TaskStatus { StatusId = 4, Name = "Overdue" }
            );

            modelBuilder.Entity<Priority>().HasData(
                new Priority { PriorityId = 1, Name = "Low", Level = 1 },
                new Priority { PriorityId = 2, Name = "Medium", Level = 2 },
                new Priority { PriorityId = 3, Name = "High", Level = 3 },
                new Priority { PriorityId = 4, Name = "Critical", Level = 4 }
            );
        }
    }
}