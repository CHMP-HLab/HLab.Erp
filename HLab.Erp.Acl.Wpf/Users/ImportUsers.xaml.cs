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
using HLab.Erp.Acl.Users;
using HLab.Erp.Core;
using HLab.Mvvm.Annotations;
using HLab.Mvvm.Application;

namespace HLab.Erp.Acl
{
    /// <summary>
    /// Logique d'interaction pour ImportUsers.xaml
    /// </summary>
    public partial class ImportUsers : UserControl, IView<ImportUsersViewModel>, IViewClassDocument
    {
        public ImportUsers()
        {
            InitializeComponent();
        }
    }
}
