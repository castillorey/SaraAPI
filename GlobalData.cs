using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaraReportAPI {
    public class GlobalData {
        public string UseConnection { get; set; }
        public bool EnableVervose { get; set; }

        public Dictionary<string, string> Jwt { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> ActiveDirectoryInfo { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> EmailInfo { get; set; } = new Dictionary<string, string>();

        private static readonly object padlock = new object();

        GlobalData() { }

        private static GlobalData current = null;
        public static GlobalData Current {
            get {
                if (current == null) {
                    lock (padlock) {
                        if (current == null) {
                            current = new GlobalData();
                        }
                    }
                }
                return current;
            }
        }
    }
}
