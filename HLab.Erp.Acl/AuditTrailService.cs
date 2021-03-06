﻿using HLab.Core.Annotations;
using HLab.DependencyInjection.Annotations;
using HLab.Erp.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace HLab.Erp.Acl
{

    public interface IAuditTrailProvider
    {
        bool Audit(string action,AclRight rightNeeded, string log, object entity, string caption, string iconPath, bool sign, bool motivate);
    }

    
}
