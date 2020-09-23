using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaraReportAPI.Models.Entities
{
    public partial class Reports
    {
        [Key]
        public int ID { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime EndDate { get; set; }
        public int RowsNumer { get; set; }
    }
}
