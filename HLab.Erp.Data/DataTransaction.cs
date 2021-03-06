﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NPoco;

namespace HLab.Erp.Data
{
    public class DataTransaction : IDataTransaction
    {
        private readonly DataService _service;
        private readonly ITransaction _transaction;
        internal IDatabase Database;
        private Action _rollback = default(Action);

        public DataTransaction(DataService service, IDatabase database)
        {
            _service = service;
            Database = database;
            _transaction = database.GetTransaction();
        }

        public T Add<T>(Action<T> setter, Action<T> added = null) where T : class, IEntity
        {
            var t = (T)Activator.CreateInstance(typeof(T));  //_entityFactory(typeof(T));
            //if(t is IEntity<int> tt) tt.Id=-1;

            setter?.Invoke(t);

            object e = null;
            if (typeof(T).GetCustomAttributes<SoftIncrementAttribut>().FirstOrDefault() is SoftIncrementAttribut a)
            {
                if (t is IEntity<int> ti)
                {
                    var ids = Database.Query<T>().OrderByDescending(d => ((IEntity<int>)d).Id).FirstOrDefault();

                    var id = ((IEntity<int>)ids)?.Id ?? 0;

                    id++;

                    ti.Id = id;
                }
            }

            e = Database.Insert(t);

            if (e != null)
            {
                t.IsLoaded = true;
                added?.Invoke(t);
            }

            return _service.GetCache<T>().GetOrAddAsync(t).Result;
        }

        public async Task<T> AddAsync<T>(Action<T> setter, Action<T> added = null) where T : class, IEntity
        {
            var t = (T)Activator.CreateInstance(typeof(T));  //_entityFactory(typeof(T));
            if(t is IEntity<int> tt) tt.Id=-1;

            setter?.Invoke(t);

            object e = null;
            if (typeof(T).GetCustomAttributes<SoftIncrementAttribut>().FirstOrDefault() is SoftIncrementAttribut a)
            {
                if (t is IEntity<int> ti)
                {
                    var ids = await Database.QueryAsync<T>().OrderByDescending(d => ((IEntity<int>) d).Id)
                        .FirstOrDefault().ConfigureAwait(false);

                    var id = ((IEntity<int>) ids)?.Id ?? 0;

                    id++;

                    ti.Id = id;

                }
            }

            e = await Database.InsertAsync(t).ConfigureAwait(false);

            if (e != null)
            {
                t.IsLoaded = true;
                added?.Invoke(t);
            }

            return await _service.GetCache<T>().GetOrAddAsync(t).ConfigureAwait(false);
        }

        public void Update<T>(T value, IEnumerable<string> columns) where T : class, IEntity
        {
            Database.Update(value, columns);
        }

        public async Task<bool> UpdateAsync<T>(T value, IEnumerable<string> columns) where T : class, IEntity
        {
            var n = await Database.UpdateAsync(value, columns);
            return n > 0;
        }

        public void Save<T>(T value) where T : class, IEntity
        {
            Database.Save(value);
        }

        public Task SaveAsync<T>(T value) where T : class, IEntity
        {
            return Task.Run(() => Database.Save(value));
        }
        public int Delete<T>(T entity, Action<T> deleted = null)
            where T : class, IEntity
        {
            var result = Database.Delete<T>(entity);

            if(result>0)
                _service.GetCache<T>().ForgetAsync(entity);

            if (result > 0) deleted?.Invoke((T)entity);

            return result;
        }
        public async Task<int> DeleteAsync<T>(T entity, Action<T> deleted = null)
            where T : class, IEntity
        {
            var result = await Database.DeleteAsync(entity);

            if (result > 0) 
                await _service.GetCache<T>().ForgetAsync(entity);

            if (result > 0) 
                deleted?.Invoke((T)entity);
            
            return result;
        }

        public void Done()
        {
            _transaction.Complete();
        }

        public void Dispose()
        {
            _transaction.Dispose();
        }
    }
}