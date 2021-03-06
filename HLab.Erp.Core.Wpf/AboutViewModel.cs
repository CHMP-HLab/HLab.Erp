﻿using System;
using System.Collections.Generic;
using System.Text;
using HLab.Notify.PropertyChanged;

namespace HLab.Erp.Core
{
    public class AboutViewModel : NotifierBase
    {
        #region Constructors

        public AboutViewModel(IErpServices erp)
        {
            _erp = erp;
            H<AboutViewModel>.Initialize(this);
        }

        #endregion
        #region Properties

        public string Note
        {
            get => _note.Get();
            set => _note.Set(value);
        }
        private readonly IProperty<string> _note = H<AboutViewModel>.Property<string>();
        
        #endregion
        #region Data

        private IErpServices _erp;

        #endregion
    }
}
