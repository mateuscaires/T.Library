using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using T.Interfaces;
using System.Text;
using System.Data;
using T.Common;
using System.Linq.Expressions;
using T.Entities;

namespace T.Infra.Data
{
    public abstract class RepositoryBase<TMainAdoBase, TEntity> : IDisposable, IRepositoryBase<TEntity> where TMainAdoBase : AdoBase, new() where TEntity : class, new()
    {
        private TEntity _instance;

        private TMainAdoBase _ado;

        public string SqlCommandText { get; set; }

        public Encoding Encoding1252 { get { return Encoding.GetEncoding(1252); } }

        protected virtual TMainAdoBase ADO { get { return _ado; } }
        
        public List<TEntity> Items { get; set; }

        public TEntity Instance { get { return _instance; } set { _instance = value; } }

        public RepositoryBase()
        {
            _ado = new TMainAdoBase();
        }

        public virtual void Add(TEntity obj)
        {
            _ado.Insert(obj);
        }

        public virtual TEntity GetById(long id)
        {
            return ADO.GetByKey<TEntity, long>(id);
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            return ADO.GetAll<TEntity>();
        }

        public virtual void Update(TEntity obj)
        {
            Update(obj);
        }

        public virtual void Remove(TEntity obj)
        {
            ADO.Delete(obj);
        }

        #region ADO

        public virtual void ExecuteCommand(StringBuilder sb)
        {
            ADO.ExecuteCommand(sb);
        }

        public virtual void ExecuteCommand(string command)
        {
            ADO.ExecuteCommand(command);
        }

        public virtual DataTable GetTableData(string command)
        {
            return ADO.GetTableData(command);
        }

        public virtual DataTable GetTableData(StringBuilder sb)
        {
            return ADO.GetTableData(sb);
        }

        public virtual T Insert<T>(T instance) where T : class
        {
            return ADO.Insert(instance);
        }

        public virtual T Update<T>(T instance) where T : class
        {
            if (typeof(T) == typeof(TEntity))
            {
                Update(instance as TEntity);
                return instance;
            }
            else
                return ADO.Update(instance);
        }

        public virtual void ExecuteMassInsert<T>(IEnumerable<T> items) where T : class
        {
            ADO.ExecuteMassInsert(items);
        }

        public virtual void ExecuteMassInsert(DataTable table, string tableName)
        {
            ADO.ExecuteMassInsert(table, tableName);
        }

        public virtual T GetLast<T>() where T : class
        {
            return ADO.GetLast<T>();
        }

        public virtual void LogEx(Exception ex)
        {
            ADO.LogEx(ex);
        }

        public virtual T Save<T>(T instance, int id) where T : class, new()
        {
            if (instance.HasValue())
            {
                T obj = default(T);

                if (id > 0)
                {
                    obj = GetById<T>(id);

                    if (obj.HasValue())
                        return Update(obj);
                    else
                        return Insert(instance);
                }
                else
                    return Insert(instance);
            }

            return instance;
        }

        public virtual T GetById<T>(int id) where T : class, new()
        {
            return ADO.GetByKey<T, int>(id);
        }

        public virtual T GetByKey<T>(int id) where T : class, new()
        {
            return ADO.GetByKey<T, int>(id);
        }
        
        public virtual List<T> Select<T>(StringBuilder query)
        {
            return ADO.Select<T>(query.ToString());
        }

        public virtual T SelectSingle<T>(StringBuilder query)
        {
            return SelectSingle<T>(query.ToString());
        }

        public List<T> GetAll<T>() where T : class, new()
        {
            return ADO.GetAll<T>();
        }

        public List<T> Select<T>() where T : class, new()
        {
            return ADO.GetAll<T>();
        }

        public virtual List<T> Select<T>(string query)
        {
            return ADO.Select<T>(query);
        }

        public virtual T SelectSingle<T>(string query)
        {
            return ADO.SelectSingle<T>(query);
        }

        #endregion

        public virtual void Dispose()
        {
            
        }
    }
    
    public class RepositoryBase<TMainContext, TMainAdoBase, TEntity> : IDisposable, IRepositoryBase<TEntity> where TMainContext : MainContext<TMainAdoBase>, new() where TMainAdoBase : AdoBase, new() where TEntity : class, new()
    {
        private TEntity _instance;

        public string SqlCommandText { get; set; }
        
        public Encoding Encoding1252 { get { return Encoding.GetEncoding(1252); } }

        protected TMainAdoBase ADO { get { return Db.ADO; } }

        protected TMainContext Db;

        public List<TEntity> Items { get; set; }

        public TEntity Instance { get { return _instance; } set { _instance = value; } }
        
        public RepositoryBase()
        {
            Db = new TMainContext();
        }

        public virtual void Add(TEntity obj)
        {
            try
            {
                ADO.Insert(obj);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public virtual TEntity GetById(long id)
        {
            try
            {
                return Db.Set<TEntity>().Find(id);
            }
            catch
            {
                return ADO.GetByKey<TEntity, long>(id);
            }
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            try
            {
                return Db.Set<TEntity>().ToList();
            }
            catch
            {
                return ADO.Select<TEntity>();
            }
        }

        public virtual void Update(TEntity obj)
        {
            try
            {
                Db.Entry(obj).State = EntityState.Modified;
                Db.SaveChanges();
            }
            catch
            {
                ADO.Update(obj);
            }
        }

        public virtual void Remove(TEntity obj)
        {
            Db.Set<TEntity>().Remove(obj);
            Db.SaveChanges();
        }

        public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter)
        {
            return Db.Set<TEntity>().Where(filter.Compile());
        }

        public TEntity GetItem(Expression<Func<TEntity, bool>> filter)
        {
            return Db.Set<TEntity>().Where(filter.Compile()).FirstOrDefault();
        }

        #region ADO

        public virtual void ExecuteCommand(StringBuilder sb)
        {
            ADO.ExecuteCommand(sb);
        }

        public virtual void ExecuteCommand(string command)
        {
            ADO.ExecuteCommand(command);
        }

        public virtual DataTable GetTableData(string command)
        {
            return ADO.GetTableData(command);
        }

        public virtual DataTable GetTableData(StringBuilder sb)
        {
            return ADO.GetTableData(sb);
        }

        public virtual T Insert<T>(T instance) where T : class
        {
            return ADO.Insert(instance);
        }

        public virtual T Update<T>(T instance) where T : class
        {
            if (typeof(T) == typeof(TEntity))
            {
                Update(instance as TEntity);
                return instance;
            }
            else
               return ADO.Update(instance);
        }

        public virtual void ExecuteMassInsert<T>(IEnumerable<T> items) where T : class
        {
            ADO.ExecuteMassInsert(items);
        }

        public virtual void ExecuteMassInsert(DataTable table, string tableName)
        {
            ADO.ExecuteMassInsert(table, tableName);
        }
        
        public virtual T GetLast<T>() where T : class
        {
            return ADO.GetLast<T>();
        }

        public virtual void LogEx(Exception ex)
        {
            ADO.LogEx(ex);
        }

        /*
        public virtual void Save<T>(T instance) where T : class, new()
        {
            string pk = instance.GetPrimaryKey();

            ADO.Save(instance);            
        }
        */

        public virtual T Save<T>(T instance, int id) where T : class, new()
        {
            if (instance.HasValue())
            {
                T obj = default(T);

                if (id > 0)
                {
                    obj = GetById<T>(id);

                    if (obj.HasValue())
                        return Update(obj);
                    else
                        return Insert(instance);
                }
                else
                    return Insert(instance);
            }

            return instance;
        }

        public virtual T GetById<T>(int id) where T : class, new ()
        {
            return ADO.GetByKey<T, int>(id);            
        }

        public virtual T GetByKey<T>(int id) where T : class, new()
        {
            return ADO.GetByKey<T, int>(id);
        }
        
        public virtual List<T> Select<T>(StringBuilder query)
        {
            return ADO.Select<T>(query.ToString());
        }

        public virtual T SelectSingle<T>(StringBuilder query)
        {
            return SelectSingle<T>(query.ToString());
        }

        public List<T> GetAll<T>() where T : class, new()
        {
            return ADO.GetAll<T>();
        }

        public List<T> Select<T>() where T : class, new()
        {
            return ADO.GetAll<T>();
        }

        public virtual List<T> Select<T>(string query)
        {
            return ADO.Select<T>(query);
        }

        public virtual T SelectSingle<T>(string query)
        {
            return ADO.SelectSingle<T>(query);
        }

        public virtual T SelectSingle<T>(Expression<Func<T, bool>> expression)
        {
            return ADO.SelectSingle<T>(expression);
        }

        public virtual List<T> Select<T>(Expression<Func<T, bool>> expression)
        {
            return ADO.Select<T>(expression);
        }
        
        /*
        public List<T> Select<T>(Func<T, bool> expression)
        {
            return ADO.Select(expression);
        }
        */
        #endregion

        public void Dispose()
        {
            Db.Dispose();
        }
    }
}
