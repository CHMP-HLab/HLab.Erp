﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HLab.Erp.Base.Wpf.Entities
{
    /// <summary>
    /// Logique d'interaction pour DetailView.xaml
    /// </summary>
    public partial class DetailView : UserControl
    {
        public DetailView()
        {
            InitializeComponent();
        }
        private void Populate(Type modelType)
        {
            foreach (var property in modelType.GetProperties())
            {
                if (property.PropertyType == typeof(string))
                {

                }
            }
        }
    }

}
