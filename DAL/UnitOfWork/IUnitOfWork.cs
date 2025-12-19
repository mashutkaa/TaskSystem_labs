using System;
using DAL.Repositories.Interfaces;

namespace DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        ITaskRepository Tasks { get; }
        IRepository<Entities.User> Users { get; }
        // Можна додати репозиторії для Status/Priority якщо треба

        void Save();
    }
}