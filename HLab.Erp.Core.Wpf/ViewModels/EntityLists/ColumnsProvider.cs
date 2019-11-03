﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using HLab.Erp.Data;
using HLab.Erp.Data.Observables;
using HLab.Mvvm;
using HLab.Mvvm.Lang;

namespace HLab.Erp.Core.ViewModels
{
    public class ColumnsProvider<T> where T:class, IEntity
    {
        private readonly Dictionary<string,Column<T>> _dict = new Dictionary<string, Column<T>>();
        private ObservableQuery<T> _list;

        public ColumnsProvider(ObservableQuery<T> list)
        {
            _list = list;
        }

        public ColumnsProvider<T> Column(string caption, Func<T,Task<object>> f,Expression<Func<T,object>> orderBy,string id=null)
        {
            var c = new Column<T>(caption,t => new AsyncView{Getter = async () => await f(t)} ,orderBy, id, false);
            _dict.Add(c.Id,c);
            return this;
        }
        public ColumnsProvider<T> Column(string caption, Expression<Func<T,object>> f,string id=null)
        {
            var c = new Column<T>(caption,f.Compile(),f, id, false);
            _dict.Add(c.Id,c);
            return this;
        }
        public ColumnsProvider<T> Column(string caption,Func<T,object> getter, Expression<Func<T,object>> orderBy,string id=null)
        {
            var c = new Column<T>(caption,getter,orderBy, id, false);
            _dict.Add(c.Id,c);
            return this;
        }
        //public ColumnsProvider<T> Hidden(string id, Func<T,Task<object>> f)
        //{
        //    var c = new Column<T>("", f, id, true);
        //    _dict.Add(c.Id,c);
        //    return this;
        //}
        public ColumnsProvider<T> Hidden(string id, Func<T,object> f)
        {
            var c = new Column<T>("", f,null, id, true);
            _dict.Add(c.Id,c);
            return this;
        }

        public object GetValue(T obj, string name)
        {
            if (_dict.ContainsKey(name))
            {
                return _dict[name].Get(obj);
            }
            else return null;
        }

        public GridView GetView()
        {
            var gv = new GridView();

            foreach (var column in _dict.Values)
            {
                if (column.Hidden) continue;

                object content;
                if (column.Caption is string s)
                    content = new Localize {Id = s};
                else
                {
                    content = column.Caption;
                }

                var header = new GridViewColumnHeader {Content = content};
                header.Click += (a, b) =>
                {
                    //TODO : click is never called and so sorting does not work 
                    _list.OrderBy = column.OrderBy;
                    _list.Update();
                };
 
                var c = new GridViewColumn
                {
                    Header = header,
                    //DisplayMemberBinding = new Binding(column.Id),
                    CellTemplate = CreateColumnTemplate(column.Id)
                };

                gv.Columns.Add(c);
            }

            return gv;
        }
        public DataTemplate CreateColumnTemplate(string property)
        {
            //    StringReader stringReader = new StringReader(
            //        @"<DataTemplate 
            //xmlns:mvvm=""clr-namespace:HLab.Mvvm;assembly=HLab.Mvvm""
            //xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""> 
            //    <mvvm:ViewLocator Model=""{Binding " + property + @"}""/> 
            //</DataTemplate>");


            StringReader stringReader = new StringReader(
                @"<DataTemplate 
        xmlns:mvvm=""clr-namespace:HLab.Mvvm;assembly=HLab.Mvvm""
        xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""> 
            <ContentControl Content=""{Binding " + property + @"}""/> 
        </DataTemplate>");


            XmlReader xmlReader = XmlReader.Create(stringReader);
            return XamlReader.Load(xmlReader) as DataTemplate;
        }
    }
}