﻿using System.Windows.Controls;
using HLab.Mvvm.Annotations;

namespace HLab.Erp.Workflows
{
    /// <summary>
    /// Logique d'interaction pour WorkFlowState.xaml
    /// </summary>
    public partial class WorkFlowStateView : UserControl, IView<WorkflowViewModel>
    {
        public WorkFlowStateView()
        {
            InitializeComponent();
        }

    }
}
