using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaraReportAPI.Models.Entities
{
    public partial class UserPermissions
    {
        [Key]
        public int ID { get; set; }
        public int? RoleID { get; set; }
        public int? AccountID { get; set; }
        public int UserGroupsOrNameID { get; set; }

        [ForeignKey(nameof(RoleID))]
        [InverseProperty(nameof(Roles.UserPermissions))]
        public virtual Roles Role { get; set; }
        [ForeignKey(nameof(UserGroupsOrNameID))]
        [InverseProperty(nameof(UserGroupsOrNames.UserPermissions))]
        public virtual UserGroupsOrNames UserGroupsOrName { get; set; }
    }
}
