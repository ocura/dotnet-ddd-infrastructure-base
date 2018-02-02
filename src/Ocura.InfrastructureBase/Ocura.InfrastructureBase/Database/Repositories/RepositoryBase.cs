using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ocura.InfrastructureBase.Database.Interfaces;

namespace Ocura.InfrastructureBase.Database.Repositories
{
    /// <summary>
    ///     Repository base
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Ocura.InfrastructureBase.Database.Interfaces.IRepositoryBase{TEntity}" />
    public class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class
    {
        /// <summary>
        ///     The database context
        /// </summary>
        protected DbContext DbContext;

        /// <inheritdoc />
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            DbContext.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Adds the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Add(TEntity obj)
        {
            DbContext.Set<TEntity>().Add(obj);
            DbContext.SaveChanges();
        }

        /// <summary>
        ///     Adds the return identifier.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public int AddReturnId(TEntity obj)
        {
            DbContext.Set<TEntity>().Add(obj);
            DbContext.SaveChanges();

            var objType = obj.GetType();
            return Convert.ToInt32(objType.GetProperties()[0].GetValue(obj));
        }

        /// <summary>
        ///     Adds the range.
        /// </summary>
        /// <param name="objList">The object list.</param>
        public void AddRange(IEnumerable<TEntity> objList)
        {
            DbContext.Set<TEntity>().AddRange(objList);
            DbContext.SaveChanges();
        }

        /// <summary>
        ///     Updates the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Update(TEntity obj)
        {
            var entry = DbContext.Entry(obj);
            DbContext.Set<TEntity>().Attach(obj);
            entry.State = EntityState.Modified;
            DbContext.SaveChanges();
        }

        /// <summary>
        ///     Removes the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Remove(TEntity obj)
        {
            DbContext.Set<TEntity>().Remove(obj);
        }

        /// <summary>
        ///     Gets the by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public TEntity GetById(long id)
        {
            return DbContext.Set<TEntity>().Find(id);
        }

        /// <summary>
        ///     Gets the by.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> exp)
        {
            return DbContext.Set<TEntity>().Where(exp);
        }

        /// <summary>
        ///     Gets the cols all.
        /// </summary>
        /// <typeparam name="TColumns">The type of the columns.</typeparam>
        /// <param name="columns">The columns.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetColsAll<TColumns>(Expression<Func<TEntity, TColumns>> columns)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TColumns, TEntity>());
            var mapper = config.CreateMapper();
            IEnumerable<TColumns> data = DbContext.Set<TEntity>().Select(columns);
            return mapper.Map<IEnumerable<TColumns>, IEnumerable<TEntity>>(data);
        }

        /// <summary>
        ///     Gets the cols by.
        /// </summary>
        /// <typeparam name="TColumns">The type of the columns.</typeparam>
        /// <param name="exp">The exp.</param>
        /// <param name="columns">The columns.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetColsBy<TColumns>(Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, TColumns>> columns)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TColumns, TEntity>());
            var mapper = config.CreateMapper();
            IEnumerable<TColumns> data = DbContext.Set<TEntity>().Where(exp).Select(columns);
            return mapper.Map<IEnumerable<TColumns>, IEnumerable<TEntity>>(data);
        }

        /// <summary>
        ///     Gets the top cols by.
        /// </summary>
        /// <typeparam name="TColumns">The type of the columns.</typeparam>
        /// <param name="exp">The exp.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="topRowsNumber">The top rows number.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetTopColsBy<TColumns>(Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, TColumns>> columns, int topRowsNumber)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TColumns, TEntity>());
            var mapper = config.CreateMapper();
            IEnumerable<TColumns> data = DbContext.Set<TEntity>().Where(exp).Select(columns).Take(topRowsNumber);
            return mapper.Map<IEnumerable<TColumns>, IEnumerable<TEntity>>(data);
        }

        /// <summary>
        ///     Gets all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TEntity> GetAll()
        {
            return DbContext.Set<TEntity>().ToList();
        }

        /// <summary>
        ///     Executes the reader procedure.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> ExecuteReaderProcedure(string storedProcedure, params KeyValuePair<string, object>[] parameters)
        {
            return ExecuteReaderProcedure<TEntity>(storedProcedure, parameters);
        }

        /// <summary>
        ///     Executes the reader procedure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure">The stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IEnumerable<T> ExecuteReaderProcedure<T>(string storedProcedure, params KeyValuePair<string, object>[] parameters)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<List<dynamic>, IEnumerable<T>>());
            var mapper = config.CreateMapper();

            using (var cmd = DbContext.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = "dbo." + storedProcedure;
                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                    foreach (var parameter in parameters)
                    {
                        var sqlParameter = new SqlParameter
                        {
                            IsNullable = true,
                            ParameterName = parameter.Key,
                            Value = parameter.Value
                        };
                        cmd.Parameters.Add(sqlParameter);
                    }

                if (cmd.Connection.State != ConnectionState.Open)
                    cmd.Connection.Open();

                cmd.CommandTimeout = 60 * 10;

                var retObject = new List<ExpandoObject>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var dataRow = new ExpandoObject() as IDictionary<string, object>;
                        for (var iField = 0; iField < dataReader.FieldCount; iField++)
                            if (!dataReader.IsDBNull(iField))
                            {
                                var dataField = (IDictionary<string, object>) GenerateNestedObject(
                                    dataReader.GetName(iField),
                                    dataReader.IsDBNull(iField) ? null : dataReader[iField]);
                                dataRow = AddIDictionary((ExpandoObject) dataRow,
                                    (ExpandoObject) dataField);
                            }

                        retObject.Add((ExpandoObject) dataRow);
                    }
                }

                return retObject.Select(c => mapper.Map<T>(c));
            }
        }

        /// <summary>
        ///     Generates the nested object.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static dynamic GenerateNestedObject(string key, object value)
        {
            var obj = new ExpandoObject() as IDictionary<string, object>;
            if (key.Contains("."))
            {
                var indexOfDot = key.IndexOf('.');
                var newKey = key.Substring(0, indexOfDot);
                var childKey = key.Substring(indexOfDot + 1, key.Length - indexOfDot - 1);
                obj[newKey] = GenerateNestedObject(childKey, value);
            }
            else
            {
                obj[key] = value;
            }
            return obj;
        }

        /// <summary>
        ///     Adds the i dictionary.
        /// </summary>
        /// <param name="objA">The object a.</param>
        /// <param name="objB">The object b.</param>
        /// <returns></returns>
        private static ExpandoObject AddIDictionary(ExpandoObject objA, ExpandoObject objB)
        {
            var dicA = objA as IDictionary<string, object>;
            var dicB = objB as IDictionary<string, object>;

            foreach (var keyValuePair in dicB)
                if (dicA.Any(a => a.Key == keyValuePair.Key))
                    dicA[keyValuePair.Key] =
                        AddIDictionary((ExpandoObject) dicA[keyValuePair.Key], (ExpandoObject) keyValuePair.Value);
                else
                    dicA.Add(keyValuePair.Key, keyValuePair.Value);

            return (ExpandoObject) dicA;
        }
    }
}
