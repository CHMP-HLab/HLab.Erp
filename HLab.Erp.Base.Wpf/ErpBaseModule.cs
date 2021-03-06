﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HLab.Core.Annotations;
using HLab.DependencyInjection.Annotations;
using HLab.Erp.Base.Wpf.Entities.Countries;
using HLab.Erp.Base.Wpf.Entities.Customers;
using HLab.Erp.Base.Wpf.Entities.Icons;
using HLab.Erp.Core;
using HLab.Erp.Data;
using HLab.Mvvm.Annotations;
using HLab.Notify.PropertyChanged;

namespace HLab.Erp.Base.Wpf
{
    using H = H<ErpBaseModule>;

    public class ErpBaseModule : NotifierBase, IBootloader //postboot
    {
        
        private readonly IErpServices _erp;

        [Import]public ErpBaseModule(IErpServices erp)
        {
            _erp = erp;
            H.Initialize(this);
        }

        public ICommand CountryCommand { get; } = H.Command(c => c.Action(
                e => e._erp.Docs.OpenDocumentAsync(typeof(ListCountryViewModel))
            ));
        public ICommand IconCommand { get; } = H.Command(c => c.Action(
                e => e._erp.Docs.OpenDocumentAsync(typeof(IconsListViewModel))
            ));
        public void Load(IBootContext b)
        {
                _erp.Menu.RegisterMenu("param/country", "{Country}",
                    CountryCommand,
                    "Icons/Entities/Country");

                _erp.Menu.RegisterMenu("param/icons", "{Icons}",
                    IconCommand,
                    "Icons/Entities/Icon");

        }
    }
}
