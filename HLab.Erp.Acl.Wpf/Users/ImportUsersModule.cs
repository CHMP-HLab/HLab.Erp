﻿using System.Windows.Input;
using HLab.Core.Annotations;
using HLab.DependencyInjection.Annotations;
using HLab.Erp.Core;
using HLab.Notify.PropertyChanged;

namespace HLab.Erp.Acl.Users
{
    using H = H<ImportUsersModule>;

    public class ImportUsersModule : NotifierBase, IBootloaderDependent
    {
        
        private readonly IErpServices _erp;

        [Import] public ImportUsersModule(IErpServices erp)
        {
            _erp = erp; 
            H.Initialize(this);
        }

        public string[] DependsOn => new []{"BootLoaderErpWpf"};

        public ICommand OpenCommand { get; } = H.Command(c => c.Action(
            e => e._erp.Docs.OpenDocumentAsync(typeof(ImportUsersViewModel))
        ).CanExecute(e => true));

        protected virtual string IconPath => "Icons/Entities/";

        public virtual void Load(IBootContext b)
        {
            if (_erp.Acl.Connection == null)
            {
                if(!_erp.Acl.Cancelled) b.Requeue();
                return;
            }
            if(!_erp.Acl.IsGranted(AclRights.ManageUser)) return;

            _erp.Menu.RegisterMenu("tools/ImportUsers", "{Import Users}",
                OpenCommand,
                "icons/tools/ImportUsers");
        }
    }
}
