﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using HLab.DependencyInjection.Annotations;
using HLab.Erp.Core.EntityLists;
using HLab.Erp.Data;
using HLab.Erp.Data.Observables;
using HLab.Notify.PropertyChanged;

namespace HLab.Erp.Core.ListFilters
{
    using H = H<FilterTextViewModel>;

    public class FilterTextViewModel : FilterViewModel
    {
        public FilterTextViewModel() => H.Initialize(this);

        public string Value {
            get => _value.Get();
            set => _value.Set(value);
        }
        private readonly IProperty<string> _value = H.Property<string>();

        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        private static readonly MethodInfo ToLowerMethod = typeof(string).GetMethod("ToLower", new Type[]{ });

        public Expression<Func<T,bool>> Match<T>(Expression<Func<T, string>> getter)
        {
            if (!Enabled || string.IsNullOrWhiteSpace(Value)) 
                return null; 

            var entity = getter.Parameters[0];
            var value = Expression.Constant(Value.ToLower(),typeof(string));

            var ex1 = Expression.Call(getter.Body,ToLowerMethod);
            var ex = Expression.Call(ex1,ContainsMethod,value);
            //var ex = Expression.Call(getter.Body,ContainsMethod,value);

            return Expression.Lambda<Func<T, bool>>(ex,entity);
        }

        public FilterTextViewModel Link<T>(ObservableQuery<T> q, Expression<Func<T, string>> getter)
            where T : class, IEntity
        {
            //var entity = getter.Parameters[0];
            q.AddFilter(Title,()=> Match(getter));
            Update = ()=> q.UpdateAsync();
            return this;
        }
        public FilterTextViewModel PostLink<T>(ObservableQuery<T> q, Func<T, string> getter)
            where T : class, IEntity
        {
            //var entity = getter.Parameters[0];
            q.AddPostFilter(Title,s => Value==null || getter(s).Contains(Value));
            Update = ()=> q.UpdateAsync();
            return this;
        }

        public Action Update
        {
            get => _update.Get();
            set => _update.Set(value);
        }
        private readonly IProperty<Action> _update = H.Property<Action>();

        private IProperty<bool> _updateTrigger = H.Property<bool>(c => c
            .On(e => e.Value)
            .On(e => e.Enabled)
            .On(e => e.Update)
            .NotNull(e => e.Update)
            .Do((e,p) => e.Update.Invoke())
        );
    }
}