﻿using HLab.Erp.Data;
using HLab.Notify.PropertyChanged;
using NPoco;

namespace HLab.Erp.Acl
{
    using H = HD<AclGranted>;

    public class AclGranted : Entity
    {
        public AclGranted() => H.Initialize(this);

        public bool Deny
        {
            get => _aclGranted.Get();
            set => _aclGranted.Set(value);
        }

        readonly IProperty<bool> _aclGranted = H.Property<bool>();

        public int? RightId
        {
            get => _right.Id.Get();
            set => _right.Id.Set(value);
        }
        [Ignore]
        public AclRight Right
        {
            get => _right.Get();
            set => _right.Set(value);
        }

        readonly IForeign<AclRight> _right = H.Foreign<AclRight>();


        public int? ToNodeId
        {
            get => _toNode.Id.Get();
            set => _toNode.Id.Set(value);
        }

        [Ignore]
         public AclNode ToNode
         {
            get => _toNode.Get();
            set => _toNode.Set(value);
         }

         readonly IForeign<AclNode> _toNode = H.Foreign<AclNode>();

        public int? OnNodeId
        {
            get => _onNode.Id.Get();
            set => _onNode.Id.Set(value);
        }

        [Ignore]
         public AclNode OnNode
         {
            get => _onNode.Get();
            set => _onNode.Set(value);
         }

         readonly IForeign<AclNode> _onNode = H.Foreign<AclNode>();
    }
}
