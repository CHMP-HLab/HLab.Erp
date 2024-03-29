﻿using HLab.Erp.Base.Data;
using HLab.Mvvm;
using HLab.Mvvm.Application;
using HLab.Notify.PropertyChanged;

namespace HLab.Erp.Base.Wpf.Entities.Countries
{
    using H = H<CountryListableViewModel>;
    public class CountryListableViewModel : ViewModel<Country>, IListableModel
    {
        public CountryListableViewModel() => H.Initialize(this);

        public string Caption => _caption.Get();

        readonly IProperty<string> _caption = H.Property<string>(c => c
            .Bind(e => e.Model.Name));

        public string IconPath => _iconPath.Get();

        readonly IProperty<string> _iconPath = H.Property<string>(c => c
            .Bind(e => e.Model.IconPath));
    }
}
