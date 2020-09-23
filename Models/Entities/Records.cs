using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaraReportAPI.Models.Entities
{
    public partial class Records
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [StringLength(64)]
        public string Name { get; set; }
        [Required]
        [StringLength(124)]
        public string Email { get; set; }
        [Required]
        [StringLength(32)]
        public string Phone { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime CreationDate { get; set; }
    }
}
