﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLab.DependencyInjection.Annotations;
using HLab.Erp.Base.Data;
using HLab.Erp.Core.ViewModels;
using HLab.Mvvm.Annotations;

namespace HLab.Erp.Base.Wpf
{
    public class ListCustomerViewModel : EntityListViewModel<ListCustomerViewModel,Customer>, IMvvmContextProvider
    {
        [Import]
        private ILocalizationService _localization;

        public void ConfigureMvvmContext(IMvvmContext ctx)
        {
        }

        public string Title => "Customers";

        public ListCustomerViewModel()
        {
            Columns
                .Column("^Name", s => s.Name)
                .Column("^Country", s => s.Country)
                .Column("^Email", s => s.Email)
                .Column("^Address", s => s.Address);

            //Filters.Add(new EntityFilterViewModel<Customer,Country>().Configure(
            //    "Country",
            //    "Pays",
            //    c => c.Country,List
            //    ));
            List.Update();
        }
    }
}
