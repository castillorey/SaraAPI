using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaraReportAPI.Models.Data.Session {
    public class SMUserCredentials {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }

        public SMUserCredentials CleanData() {
            try {
                SMUserCredentials userCredentials = new SMUserCredentials() {
                    Username = TrimAllNonPrintableCharacters(Username).Trim(),
                    Password = TrimAllNonPrintableCharacters(Password).Trim(),
                    Domain = TrimAllNonPrintableCharacters(Domain).Trim()
                };
                if (string.IsNullOrWhiteSpace(userCredentials.Username) ||
                    string.IsNullOrWhiteSpace(userCredentials.Password) ||
                    string.IsNullOrWhiteSpace(userCredentials.Domain)) {
                    return null;
                }
                return userCredentials;
            } catch (Exception) {
                return null;
            }
        }

        protected string TrimAllNonPrintableCharacters(string text, bool trim = true) {
            try {
                string val = System.Text.RegularExpressions.Regex.Replace(text, @"[\p{Cc}-[\r\n]]+", string.Empty);
                return trim ? val.Trim() : val;
            } catch (Exception) {
                return string.Empty;
            }
        }
    }
}
