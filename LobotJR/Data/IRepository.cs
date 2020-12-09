using System;
using System.Collections.Generic;

namespace LobotJR.Data
{
    public interface IRepository<TEntity>
    {
        TEntity Create(TEntity entry);
        IEnumerable<TEntity> Read();
        IEnumerable<TEntity> Read(Func<TEntity, bool> filter);
        TEntity Read(TEntity entry);
        TEntity ReadById(int id);
        TEntity Update(TEntity entry);
        TEntity Delete(TEntity entry);
        TEntity DeleteById(int id);
        void Commit();
    }
}
