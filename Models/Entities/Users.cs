using SaraReportAPI.Models.Entities.Views;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaraReportAPI.Models.Entities {
    public partial class Users
    {
        [Key]
        public int ID { get; set; }
        public int EmployeeNumber { get; set; }
        [Column(TypeName = "datetime2(3)")]
        public DateTime? DateFirstLogin { get; set; }
        [Column(TypeName = "datetime2(3)")]
        public DateTime? DateLastLogin { get; set; }
        public virtual RosterMeta4 RosterMeta4 { get; set; }
    }
}
