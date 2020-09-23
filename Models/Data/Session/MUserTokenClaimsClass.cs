using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaraReportAPI.Models.Data.Session {
    public class MUserTokenClaimsClass {
        public string FullName { get; set; }
        public int UserID { get; set; }
        public int EmployeeNumber { get; set; }
        public int? EmployeeNumberSup { get; set; }
        public string NetworkLogin { get; set; }
        public string ClientIP { get; set; }
        public List<string> Roles { get; set; }
        public double TokenV { get; set; }
    }
}
