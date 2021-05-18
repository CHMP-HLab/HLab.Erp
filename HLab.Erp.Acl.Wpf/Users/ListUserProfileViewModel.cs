﻿using Grace.DependencyInjection.Attributes;
using HLab.Erp.Core;
using HLab.Erp.Core.EntityLists;
using HLab.Erp.Core.ListFilterConfigurators;
using System;

namespace HLab.Erp.Acl.Users
{
    public class ProfilesPerUserListViewModel : EntityListViewModel<UserProfile>
    {
        public ProfilesPerUserListViewModel(User user) : base(c => c
            .StaticFilter(e => e.UserId == user.Id)
            .Column()
            .Header("{Name}")
            .Content(s => s.Profile.Name)
        )
        {
            OpenAction = target => Erp.Docs.OpenDocumentAsync(target.Profile);
        }

        //public UserProfileListViewModel(Profile profile) : base(c => c
        //    .StaticFilter(e => e.ProfileId == profile.Id)
        //    .Column()
        //    .Header("{Name}")
        //    .Content(s => s.User.Caption)
        //)
        //{
        //    OpenAction = target => Erp.Docs.OpenDocumentAsync(target.User);
        //}
    }
    public class UsersPerProfileListViewModel : EntityListViewModel<UserProfile>
    {
        public UsersPerProfileListViewModel(Profile profile) : base(c => c
            .StaticFilter(e => e.ProfileId == profile.Id)
            .Column()
            .Header("{Name}")
            .Content(s => s.User.Caption)
        )
        {
            OpenAction = target => Erp.Docs.OpenDocumentAsync(target.User);
        }

        protected override bool CanExecuteAdd(Action<string> errorAction) => Erp.Acl.IsGranted(errorAction, AclRights.ManageProfiles);
        protected override bool CanExecuteDelete(UserProfile profile, Action<string> errorAction) => Erp.Acl.IsGranted(errorAction, AclRights.ManageProfiles);
    }
}