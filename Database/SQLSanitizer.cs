using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GigaChad_Corp_Usermanager.MySQL
{
    internal class SQLSanitizer
    {
        public static string SanitizeInput(string input) {
            if (string.IsNullOrEmpty(input)) {
                return input;
            }

            string sanitized = input;

            // Basic removal of semicolon, single-quote, and comment markers
            string[] dangerousChars = { ";", "--", "/*", "*/", "'", "\"" };

            foreach (var ch in dangerousChars) {
                sanitized = sanitized.Replace(ch, "");
            }

            // Remove dangerous sql commands from the query string
            sanitized = Regex.Replace(sanitized, @"\b(exec|select|insert|delete|update|drop|alter)\b", "", RegexOptions.IgnoreCase);

            return sanitized;
        }
    }
}
