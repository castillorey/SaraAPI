using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaraReportAPI.Models.Entities.Views
{
    public partial class RosterMeta4
    {
        [Key]
        public int EmployeeNumber { get; set; }
        [StringLength(50)]
        public string NetworkLogin { get; set; }
        [StringLength(150)]
        public string FullName { get; set; }
        [Required]
        [StringLength(80)]
        public string Position { get; set; }
        public bool Status { get; set; }
        public int EmployeeNumberSup { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? DateOff { get; set; }
        [StringLength(50)]
        public string Email { get; set; }
        public long? CC { get; set; }
        public virtual RosterMeta4 EmployeeSup { get; set; }
        public virtual Users User { get; set; }
        public virtual ICollection<RosterMeta4> Employees { get; set; }
    }
}
