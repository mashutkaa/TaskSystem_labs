using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DAL.EF;
using DAL.Repositories.Interfaces;

namespace DAL.Repositories.Impl
{
    public class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly TaskContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(TaskContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public T Get(int id)
        {
            return _dbSet.Find(id);
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).ToList();
        }

        public void Create(T item)
        {
            _dbSet.Add(item);
        }

        public void Update(T item)
        {
            _context.Entry(item).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            T item = _dbSet.Find(id);
            if (item != null)
                _dbSet.Remove(item);
        }
    }
}