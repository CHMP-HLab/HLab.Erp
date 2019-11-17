﻿using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using HLab.Core.Annotations;
using HLab.DependencyInjection.Annotations;
using HLab.Erp.Core.ListFilters;
using HLab.Erp.Core.Tools.Details;
using HLab.Erp.Data;
using HLab.Erp.Data.Observables;
using HLab.Mvvm;
using HLab.Notify.PropertyChanged;

namespace HLab.Erp.Core.ViewModels.EntityLists
{
    public abstract class EntityListViewModel<TClass> : ViewModel<TClass>, IListViewModel
    where TClass : EntityListViewModel<TClass>
    {
        public virtual void PopulateDataGrid(DataGrid grid)
        {
            throw new NotImplementedException();
        }

        public abstract void SetOpenAction(Action<object> action);
    }

    public abstract class EntityListViewModel<TClass,T> : EntityListViewModel<TClass>, IListViewModel<T>
    where TClass : EntityListViewModel<TClass, T>
        where T : class, IEntity, new()
    {
        public new T Model
        {
            get => (T)base.Model;
            set => base.Model = value;
        }

        public override Type ModelType => typeof(T);

        [Import]
        private IDocumentService _docs;
        [Import]
        private IMessageBus _msg;

        [Import]
        public ObservableQuery<T> List { get; }
        public ColumnsProvider<T> Columns { get; }
        public ObservableCollection<IFilterViewModel> Filters { get; } = new ObservableCollection<IFilterViewModel>();

        public ObservableCollection<dynamic> ListViewModel { get; } = new ObservableCollection<dynamic>();
        private readonly ConcurrentDictionary<T,dynamic> _cache = new ConcurrentDictionary<T, dynamic>();
        protected EntityListViewModel()
        {
            Columns = new ColumnsProvider<T>(List);
            List_CollectionChanged(null,new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add,List,0));
            List.CollectionChanged += List_CollectionChanged;
            H.Initialize((TClass)this,OnPropertyChanged);

            OpenAction = target => _docs.OpenDocument(target);
        }

        public ICommand OpenCommand { get; } = H.Command(c => c
            .CanExecute(e=>e.Selected!=null)
            .Action(e => e.OpenAction.Invoke(e.Selected))
            .On(e => e.Selected).CheckCanExecute()
        );

        protected Action<T> OpenAction;
        public override void SetOpenAction(Action<object> action)
        {
            OpenAction = t => action(t);
        }


        public ICommand AddCommand { get; } = H.Command(c => c
//            .CanExecute(e=>e.Selected!=null)
            .Action(async e => await e.OnAddCommand(e.Selected))
//            .On(e => e.Selected).CheckCanExecute()
        );

        protected virtual async Task OnAddCommand(T target)
        {
            var entity = (T)Activator.CreateInstance(typeof(T));
            if(entity is IEntity<int> e) e.Id=-1;
            _docs.OpenDocument(entity);
        }

        private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newIndex = e.NewStartingIndex;
                    foreach (var n in e.NewItems.OfType<T>())
                    {
                        ObjectMapper<T> h = _cache.GetOrAdd(n,o => new ObjectMapper<T>(o, Columns));
                        ListViewModel.Insert(newIndex++,h);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var n in e.OldItems.OfType<T>())
                    {
                        //TODO should keep deleted elements in cache for some times
                        if (_cache.TryRemove(n, out var h))
                            ListViewModel.Remove(h);
                        else
                        {
                            var hh = ListViewModel.FirstOrDefault(x =>
                            {
                                if (x is ObjectMapper<T> om)
                                {
                                    if (om.Model.Id == n.Id) return true;
                                }

                                return false;
                            });

                            if (hh != null)
                            {
                                ListViewModel.Remove(hh);
                            }
                            else
                            {
                                var test = ListViewModel[e.OldStartingIndex];
                            }
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public GridView View => _view.Get();
        private readonly IProperty<GridView> _view = H.Property<GridView>(c => c
            .Set(e => e.Columns.GetView()));
        public ListCollectionView ListCollectionView => _listCollectionView.Get();
        private readonly IProperty<ListCollectionView> _listCollectionView = H.Property<ListCollectionView>(c => c
            .Set(setter: e =>
            {
                var lcv = new ListCollectionView(e.ListViewModel);
                lcv.GroupDescriptions.Add(new PropertyGroupDescription("FileId"));
                return lcv;
            }));

        public override void PopulateDataGrid(DataGrid grid)
        {

            grid.SourceUpdated += delegate(object sender, DataTransferEventArgs args)
            {
                ICollectionView cv = CollectionViewSource.GetDefaultView(grid.ItemsSource);
                if (cv != null)
                {
                    cv.GroupDescriptions.Clear();
                    cv.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
                }
            };


            foreach (var col in View.Columns)
            {
                grid.Columns.Add(new DataGridTemplateColumn
                {
                    Header = (col.Header as GridViewColumnHeader)?.Content,
                    CellTemplate = col.CellTemplate
                });
            }
        }

        public T Selected
        {
            get => _selected.Get();
            set { if(_selected.Set(value)) _msg.Publish(new DetailMessage(value)); }
        }

        private readonly IProperty<T> _selected = H.Property<T>();

        public dynamic SelectedViewModel
        {
            get => _selectedViewModel.Get();
            set => Selected = value?.Model;
        }

        private readonly IProperty<dynamic> _selectedViewModel = H.Property<dynamic>(c => c
            .On(e => e.Selected)
            .Set(e => e.Selected==null?null:e._cache.GetOrAdd(e.Selected, o => new ObjectMapper<T>(o, e.Columns))));
    }
}