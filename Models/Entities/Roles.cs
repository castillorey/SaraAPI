using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaraReportAPI.Models.Entities
{
    public partial class Roles
    {
        public Roles()
        {
            UserPermissions = new HashSet<UserPermissions>();
        }

        [Key]
        public int ID { get; set; }
        [Required]
        [StringLength(32)]
        public string Name { get; set; }

        [InverseProperty("Role")]
        public virtual ICollection<UserPermissions> UserPermissions { get; set; }
    }
}
