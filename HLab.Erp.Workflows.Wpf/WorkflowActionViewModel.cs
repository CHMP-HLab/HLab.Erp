﻿using System.Windows.Input;
using HLab.Mvvm;
using HLab.Notify.PropertyChanged;

namespace HLab.Erp.Workflows
{
    using H = H<WorkflowActionViewModel>;

    internal class WorkflowActionViewModel : ViewModel<WorkflowAction>
    {
        public WorkflowActionViewModel() => H.Initialize(this);

        public string IconPath => _iconPath.Get();

        readonly IProperty<string> _iconPath = H.Property<string>(c => c
            .Bind(e => e.Model.IconPath)
        );

        public ICommand Command { get; } = H.Command(c => c
            .CanExecute(vm => vm.Model.Check() == WorkflowConditionResult.Passed)
            .Action(e=> e.Model.Action())
        );
    }
}
