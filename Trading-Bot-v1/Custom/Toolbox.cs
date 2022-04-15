using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trading_Bot_v1.Custom {
    class Toolbox {
        public static string EscapeJSONQuotes(string str) {
            return str.Replace("\"", "\\\"");
        }
    }
}
