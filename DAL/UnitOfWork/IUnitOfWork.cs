using System;
using DAL.Repositories.Interfaces;

namespace DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        ITaskRepository Tasks { get; }
        IRepository<Entities.User> Users { get; }

        void Save();
    }
}