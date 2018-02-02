using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Ocura.InfrastructureBase.Database.Interfaces
{
    public interface IRepositoryBase<TEntity> : IDisposable where TEntity : class
    {
        void Add(TEntity obj);
        int AddReturnId(TEntity obj);
        void AddRange(IEnumerable<TEntity> objList);
        void Update(TEntity obj);
        void Remove(TEntity obj);
        TEntity GetById(long id);
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> exp);
        IEnumerable<TEntity> GetColsAll<TColumns>(Expression<Func<TEntity, TColumns>> columns);
        IEnumerable<TEntity> GetColsBy<TColumns>(Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, TColumns>> columns);
        IEnumerable<TEntity> GetTopColsBy<TColumns>(Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, TColumns>> columns, int topRowsNumber);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> ExecuteReaderProcedure(string storedProcedure, params KeyValuePair<string, object>[] parameters);
        IEnumerable<T> ExecuteReaderProcedure<T>(string storedProcedure, params KeyValuePair<string, object>[] parameters);
    }
}
