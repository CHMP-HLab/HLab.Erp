﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HLab.Base.Fluent;
using HLab.Erp.Data;
using HLab.Erp.Data.Observables;

namespace HLab.Erp.Core
{
    public interface IFiltersFluentConfigurator<T>
        where T : class, IEntity
    {
        ObservableQuery<T> List { get; }
    }

    public interface IFiltersFluentConfigurator<T,out TFilter> : IFluentConfigurator<TFilter>, IFiltersFluentConfigurator<T>
        where TFilter : IFilterViewModel, new()
        where T : class, IEntity
    {
    }


    public class FiltersFluentConfigurator<T,TFilter> : FluentConfigurator<TFilter>, IFiltersFluentConfigurator<T>//IFiltersFluentConfigurator<T, TFilter>
        where TFilter : IFilterViewModel, new()
        where T : class, IEntity
    {
        public ObservableQuery<T> List { get; }
        public FiltersFluentConfigurator(ObservableQuery<T> list, TFilter target) : base(target)
        {
            List = list;
        }
    }
    /// <summary>
    /// 
    /// 
    /// </summary>
    public interface IEntityListViewModel
    {
        void Populate(object grid);

        void SetOpenAction(Action<object> action);
        void SetSelectAction(Action<object> action);
        ReadOnlyObservableCollection<IFilterViewModel> Filters { get; }
        ICommand AddCommand { get; }
        dynamic SelectedViewModel { get; set; }
        IEnumerable<int> SelectedIds { get; set; }
        void RefreshColumn(string column);
        void RefreshColumn(string column, int id);

        T GetFilter<T>() where T : IFilterViewModel;
        void Start();
    }

    public interface IEntityListViewModel<T> : IEntityListViewModel
        where T : class, IEntity
    {
        ObservableQuery<T> List { get; }
        IColumnsProvider<T> Columns { get; }

        IEntityListViewModel<T> AddFilter<TFilter>(Action<FiltersFluentConfigurator<T,TFilter>> configure)
            where TFilter : IFilterViewModel, new();

        void AddFilter(IFilterViewModel filter);
    }

}