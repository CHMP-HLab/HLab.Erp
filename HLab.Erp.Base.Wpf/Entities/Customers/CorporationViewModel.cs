﻿using System.ComponentModel;
using HLab.DependencyInjection.Annotations;
using HLab.Erp.Acl;
using HLab.Erp.Base.Data;
using HLab.Erp.Core;
using HLab.Erp.Data;
using HLab.Notify.PropertyChanged;

namespace HLab.Erp.Base.Wpf.Entities.Customers
{
    public abstract class CorporationViewModel<T> : EntityViewModel<T>, ICorporationViewModel
    where T : class, IEntity<int>, INotifyPropertyChanged, ICorporation, IListableModel
    {
        [Import]
        public IErpServices Erp { get; }

        protected CorporationViewModel() => H<CorporationViewModel<T>>.Initialize(this);

        public override string Title => _title.Get();
        private readonly IProperty<string> _title = H<CorporationViewModel<T>>.Property<string>(c => c
            .OneWayBind(e => e.Model.Caption)
        );

        ICorporation ICorporationViewModel.Model => Model;
    }
}
