using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LobotJR.Data
{
    public class SqliteRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly SqliteContext context;
        private readonly DbSet<TEntity> dbSet;

        public SqliteRepository(SqliteContext context)
        {
            this.context = context;
            dbSet = context.Set<TEntity>();
        }

        public void Commit()
        {
            context.SaveChanges();
        }

        public TEntity Create(TEntity entry)
        {
            return dbSet.Add(entry);
        }

        public TEntity Delete(TEntity entry)
        {
            return dbSet.Remove(entry);
        }

        public TEntity DeleteById(int id)
        {
            var toRemove = dbSet.Find(id);
            if (toRemove != null)
            {
                return dbSet.Remove(toRemove);
            }
            return null;
        }

        public IEnumerable<TEntity> Read()
        {
            return dbSet;
        }

        public IEnumerable<TEntity> Read(Func<TEntity, bool> filter)
        {
            return dbSet.Where(filter);
        }

        public TEntity Read(TEntity entry)
        {
            return dbSet.Where(x => x.Equals(entry)).FirstOrDefault();
        }

        public TEntity ReadById(int id)
        {
            return dbSet.Find(id);
        }

        public TEntity Update(TEntity entry)
        {
            var output = dbSet.Attach(entry);
            context.Entry(entry).State = EntityState.Modified;
            return output;
        }
    }
}
