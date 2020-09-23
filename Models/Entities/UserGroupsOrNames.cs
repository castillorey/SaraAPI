using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaraReportAPI.Models.Entities
{
    public partial class UserGroupsOrNames
    {
        public UserGroupsOrNames()
        {
            UserPermissions = new HashSet<UserPermissions>();
        }

        [Key]
        public int ID { get; set; }
        [StringLength(32)]
        public string UserGroup { get; set; }
        [StringLength(32)]
        public string UserName { get; set; }

        [InverseProperty("UserGroupsOrName")]
        public virtual ICollection<UserPermissions> UserPermissions { get; set; }
    }
}
