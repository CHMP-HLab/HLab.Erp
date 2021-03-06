﻿//#define MULTITHREAD

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using HLab.Core.DebugTools;
using HLab.DependencyInjection.Annotations;
using HLab.Notify.Annotations;
using HLab.Notify.Collections;
using HLab.Notify.PropertyChanged;

////using System.Data.Entity;

namespace HLab.Erp.Data.Observables
{
    //public interface IObservableQuery
    //{
    //    IEntity SelectedEntity { get; set; }
    //    void Update();
    //}
    //public interface IObservableQuery<T> : ITriggerable, IObservableQuery, IList<T>
    //    where T : class, IEntity
    //{
    //    IObservableQuery<T> AddFilter(Expression<Func<T, bool>> expression, int order = 0, string name = null);
    //    IObservableQuery<T> SetSource(Func<IEnumerable<T>> src);
    //    IObservableQuery<T> SetSource(Func<IQueryable<T>, IQueryable<T>> src);
    //    void Update(bool force = true);
    //}

    public static class ObservableQueryExtensions
    {
        public static ObservableQuery<T> AddFilter<T>(this ObservableQuery<T> oq, string name, Expression<Func<T, bool>> expression, int order = 0)
        where T : class, IEntity
        {
            oq.AddFilter(expression, order, name);
            return oq;
        }

        public static ObservableQuery<T> FluentUpdate<T>(this ObservableQuery<T> oq, bool force = true)
            where T : class, IEntity
        {
            oq.Update(force);
            return oq;
        }
    }

    [Export(typeof(ObservableQuery<>))]
    public class ObservableQuery<T> : ObservableCollectionNotifier<T>, ITriggerable//, IObservableQuery<T>
        where T : class, IEntity
    {
        [Import]
        public ObservableQuery(IDataService db)
        {
            _db = db;
            H.Initialize(this,OnPropertyChanged);
        }

        protected new class H : NotifyHelper<ObservableQuery<T>> { }

        private readonly IDataService _db;


        //public ModelCommand CreateCommand => this.GetCommand(Create, ()=>true);
        //public ModelCommand DeleteCommand => this.GetCommand(Delete, ()=>false);

        private IEnumerable<T> _source = null;

        public class CreateHelper : IDisposable
        {
            public ObservableQuery<T> List = null;
            public T Entity = null;
            public bool Done = true;

            public void Dispose()
            {
//                Context?.Dispose();
            }
        }
        private class Filter
        {
            public string Name { get; set; }
            public Expression<Func<T, bool>> Expression { get; set; } = null;
            public Func<IQueryable<T>, IQueryable<T>> Func { get; set; } = null;
            public int Order { get; set; }
        }
        private class PostFilter
        {
            public string Name { get; set; }
            public Func<T, bool> Expression { get; set; } = null;
            public Func<IEnumerable<T>, IEnumerable<T>> Func { get; set; }
            public int Order { get; set; }
        }

        private readonly object _lockFilters = new object();
        private readonly List<Filter> _filters = new List<Filter>();
        private readonly List<PostFilter> _postFilters = new List<PostFilter>();


        //public new IEntity Selected
        //{
        //    get => base.Selected;
        //    set => base.Selected = (T)value;
        //}
        //private IProperty<IEntity> _selectedEntity = H.Property<IEntity>( c => c
        //    .On(e => e.Selected)
        //    .Set(e => e.Selected)
        //    );
        public ObservableQuery<T> SetSource(Func<IEnumerable<T>> src)
        {
            SourceEnumerable = src;
            return this;
        }

        public ObservableQuery<T> SetSource(Func<IQueryable<T>, IQueryable<T>> src)
        {
            SourceQuery = src;
            return this;
        }

        public Func<IQueryable<T>, IQueryable<T>> SourceQuery
        {
            get => _sourceQuery.Get();
            set => _sourceQuery.Set(value);
        }
        private readonly IProperty<Func<IQueryable<T>, IQueryable<T>>> _sourceQuery = H.Property<Func<IQueryable<T>, IQueryable<T>>>(c => c
           .Set(e =>(Func<IQueryable<T>, IQueryable<T>>)(q => q))
        );

        public Func<IEnumerable<T>> SourceEnumerable
        {
            get => _sourceEnumerable.Get();
            set => _sourceEnumerable.Set(value);
        }
        private readonly IProperty<Func<IEnumerable<T>>> _sourceEnumerable = H.Property<Func<IEnumerable<T>>>();


        //private Func<DbContext, IQueryable<T>> Query()
        //{
        //    Expression<Func<DbContext, IQueryable<T>>> s = ctx => ctx.Set<T>();

        //        lock (_lockFilters)
        //            foreach (Filter filter in _filters.OrderBy(f => f.Order))
        //            {
        //                var s2 = s.Compile();

        //                if(filter.Func!=null)
        //                    s =  ctx => filter.Func(s2(ctx));
        //                else
        //                    s = ctx => s2(ctx).Where(filter.Expression);
        //            }

        //    return EF.CompileQuery<DbContext, IQueryable<T>>(s);
        //}
        private IQueryable<T> Query(IQueryable<T> s)
        {
            if (s == null) return null;
            
            lock (_lockFilters)
                foreach (Filter filter in _filters.OrderBy(f => f.Order))
                {

                    s = filter.Func != null ? filter.Func(s) : s.Where(filter.Expression);
                }
            return s;
        }


        private IEnumerable<T> PostQuery(IEnumerable<T> q)
        {
            if (q == null) return null;
            lock (_lockFilters)
                foreach (var filter in _postFilters.OrderBy(f => f.Order))
                {
                    q = filter.Func != null ? filter.Func(q) : q.Where(filter.Expression);
                }
            return q;
        }
        private List<T> PostQuery(bool force = true)
        {
            if (_source == null || force)
            {
                if (SourceEnumerable != null)
                {
                    _source = SourceEnumerable?.Invoke();
                }
                else
                {
                    //using (var context = DbService.D.Get())
                    {
                        // NullException : often occur when data is not nullable but sql is
                        //Query().ToList();

                        _source = _db.FetchQuery<T>(Query);
                    }               
                }                
            }
            var list = PostQuery(_source).ToList();

            return list;
        }


        public ObservableQuery<T> AddFilter(Expression<Func<T, bool>> expression, int order = 0, string name = null)
        {
            lock (_lockFilters)
            {
                if (name != null) RemoveFilter(name);
                _filters.Add(new Filter
                {
                    Name = name,
                    Expression = expression,
                    Order = order,
                });
                return this;
            }
        }

        public ObservableQuery<T> AddFilterFunc(Func<IQueryable<T>, IQueryable<T>> func, int order = 1, string name = null)
        {
            lock (_lockFilters)
            {
                if (name != null) RemoveFilter(name);
                _filters.Add(new Filter
                {
                    Name = name,
                    Func = func,
                    Order = order,
                });
                return this;
            }
        }
        public ObservableQuery<T> AddPostFilter(string name, Func<T, bool> expression, int order = 0)
        {
            return AddPostFilter(expression, order, name);
        }

        public ObservableQuery<T> AddPostFilter(Func<T, bool> expression, int order = 0, string name = null)
        {
            lock (_lockFilters)
            {
                if (name != null) RemoveFilter(name);
                _postFilters.Add(new PostFilter
                {
                    Name = name,
                    Expression = expression,
                    Order = order,
                });
                return this;
            }
            //return AddPostFilter(s => s.Where(expression), order, name);
        }

        public ObservableQuery<T> AddPostFilter(Func<IEnumerable<T>, IEnumerable<T>> func, int order = 0, string name = null)
        {
            lock (_lockFilters)
            {
                if (name != null) RemovePostFilter(name);
                _postFilters.Add(new PostFilter
                {
                    Name = name,
                    Func = func,
                    Order = order,
                });
                return this;
            }
        }
        public ObservableQuery<T> RemoveFilter(string name)
        {
            lock (_lockFilters)
            {
                foreach (Filter f in _filters.Where(f => f.Name == name).ToList())
                {
                    _filters.Remove(f);
                }
                return this;
            }
        }

        public ObservableQuery<T> RemovePostFilter(string name)
        {
            lock (_lockFilters)
            {
                foreach (var f in _postFilters.Where(f => f.Name == name).ToList())
                {
                    _postFilters.Remove(f);
                }
                return this;
            }
        }

        //public Func<TThis, T, DbContext, bool> OnCreate;
        protected readonly ConcurrentDictionary<string, Action<CreateHelper>> OnCreate = new ConcurrentDictionary<string, Action<CreateHelper>>();
        protected readonly ConcurrentDictionary<string, Action<CreateHelper>> OnCreated = new ConcurrentDictionary<string, Action<CreateHelper>>();

        protected readonly ConcurrentDictionary<string, Action<CreateHelper>> OnDelete = new ConcurrentDictionary<string, Action<CreateHelper>>();
        protected readonly ConcurrentDictionary<string, Action<CreateHelper>> OnDeleted = new ConcurrentDictionary<string, Action<CreateHelper>>();
        protected void Create()
        {
            //TODO
            //var entity = new T();

            //using (var helper = new CreateHelper { List = this, Entity = entity })
            //    if (Exec(OnCreate, helper))
            //    {
            //        //TODO
            //        //helper.Context.Insert(entity);

            //        Selected = entity;
            //        FluentUpdate();
            //        Exec(OnCreated, helper);
            //    }
        }
        protected void Delete()
        {
            var entity = Selected;
            if (entity == null) return;

            if (Exec(OnDelete, new CreateHelper
            {
                List = this,
                Entity = entity,
            }))
            {
                _db.Delete(Selected);
                Update();

                Exec(OnDeleted, new CreateHelper
                {
                    List = this,
                    Entity = entity,
                });
            }           
        }

        private ObservableQuery<T> AddCreator(IDictionary<string, Action<CreateHelper>> dict, Action<CreateHelper> func,
            string name = "")
        {
            lock (_lockFilters)
            {
                if (name != null && dict.ContainsKey(name)) dict.Remove(name);
                dict.Add(name ?? "", func);
                return this;
            }
        }

        protected bool Exec(ConcurrentDictionary<string, Action<CreateHelper>> dict, CreateHelper h)
        {
            foreach (var action in dict.Values)
            {
                action(h);
                // if one action fail, we don't want to create new entity.
                if (!h.Done) return false;
            }
            return true;
        }

        public ObservableQuery<T> AddOnCreate(Action<CreateHelper> func, string name = "")
        {
            return AddCreator(OnCreate, func, name);
        }
        public ObservableQuery<T> AddOnCreated(Action<CreateHelper> func, string name = "")
        {
            return AddCreator(OnCreated, func, name);
        }
        public ObservableQuery<T> AddOnDelete(Action<CreateHelper> func, string name = "")
        {
            return AddCreator(OnDelete, func, name);
        }
        public ObservableQuery<T> AddOnDeleted(Action<CreateHelper> func, string name = "")
        {
            return AddCreator(OnDeleted, func, name);
        }

//        public ModelCommand GetEntityInteractifCommand => this.GetCommand(GetEntityInteractif,()=>true);

        public void GetEntityInteractif()
        {
            //Selected = GetEntityInteractif(Selected);
        }

        //public T GetEntityInteractif(T item)
        //{
        //    var view = new EntitySelectorWindow { DataContext = this };

        //    if (item != null) Select(item);

        //    if (view.ShowDialog() ?? false)
        //    {
        //        var selectedItem = Selected?.Entity;
        //        //GetDbContext.Entry(selectedItem).State = EntityState.Detached;
        //        return selectedItem;
        //    }
        //    return item;
        //}





        private readonly object _lockUpdate = new object();

        public ObservableQuery<T> Do(Action<ObservableQuery<T>> action)
        {
            action(this);
            return this;
        }

        private Thread _updateThread = null;

        public ObservableQuery<T> FluentUpdateAsync(bool force = true)
        {
            var oldThread = _updateThread;

            _updateThread = new Thread(()=>
            {
                lock (_lockUpdate)
                {
                    if(oldThread?.IsAlive??false)  _updateNeeded = true;
                }

                oldThread?.Join();

                Update(force);
            });
            _updateThread.Start();
            return this;
        }

        private readonly object _lockUpdateNeeded = new object();
        private volatile bool _updateNeeded = false;


        private volatile bool _updating = false;

        private bool _initialized = false;

        public ObservableQuery<T> Init()
        {
            if (!_initialized)
            {
                Update();
            }
            return this;
        }


        public void Update() => Update(true);

        public void Update(bool force)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            lock (_lockUpdate)
            {
                _updateNeeded = false;
            }


            var changed = false;


            {
                List<T> list = PostQuery(force)/*.ToList()*/;

                Console.WriteLine("Query : " + stopwatch.ElapsedMilliseconds);

                if (list == null) return;

                var count = list.Count;

                for(int n = 0; n < list.Count; n++ )
                {
                    var item = list[n];
                    var id = item.Id;

                    if (_updateNeeded)
                    {
                        lock (_lockUpdate)
                        {
                            _updating = false;
                            _updateNeeded = false;
                        }
                        return;
                    }
                    // while list is consistant
                    if (n < Count && Equals(id,this[n].Id)) continue;

                    //next item exists elswhere in collection
                    var n2 = n + 1;
                    while(n2<Count)
                    {
                        var i = this[n2];
                        if(Equals(i.Id,id))
                        {
                            RemoveAt(n2);
                            base.Insert(n, i);
                            continue;
                        }
                        n2++;
                    }

                    base.Insert(n, item);
                    changed = true;
                }
                Console.WriteLine("Update : " + stopwatch.ElapsedMilliseconds);

                //remove remaining items
                while (count < Count)
                {
                        RemoveAt(count);
                }

                Console.WriteLine("Cleanup : " + stopwatch.ElapsedMilliseconds);
                Debug.Assert(Count==count);
                
            }
/*
            foreach (var i in this)
            {
                Debug.Assert(this.Count(e => Equals(e.Id, i.Id)) == 1);
            }
            */
            if (!changed)
            {
            }

            _initialized = true;
            _updating = false;
        }

        //take care of modified entity that do not match filters anymore
        private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = sender as T;
            if (!Match(vm)) Remove(vm);
        }

        private bool Match(T vm)
        {
            if (vm != null)
            {
                return _filters.Where(filter => filter.Expression != null).All(filter => filter.Expression.Compile()(vm)) 
                    && _postFilters.Where(filter => filter.Expression != null).All(filter => filter.Expression(vm));
            }
            return false;
        }

        public void OnTriggered()
        {
            Update();
        }

        public override void Add(T item)
        {
            throw new NotImplementedException("Observable Query is readOnly");
        }
        public override void Insert(int index, T item)
        {
            throw new NotImplementedException("Observable Query is readOnly");
        }
    }
}
