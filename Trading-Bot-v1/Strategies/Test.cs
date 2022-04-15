using System;
using System.Collections.Generic;
using Trading_Bot_v1.Monitoring;

namespace Trading_Bot_v1.Strategies {
    class Test : Strategy {
        public static List<Indicator> required_indicators = new() {
            new Indicator("BB.upper|1"),
            new Indicator("BB.lower|1")
        };

        public Test(string id = null, Trade trade = null, string data = null) : base(id, trade, required_indicators, data) {}

        public override bool CheckForEntry() {
            int bbUpper = GetIndicatorValue("BB.upper|1");

            return true;
            //if (trade.monitor.price >= bbUpper) {
            //    return true;
            //}

            //return false;
        }

        public override bool CheckForExit() {
            int bbLower = GetIndicatorValue("BB.lower|1");

            Console.WriteLine("Test2: " + GetDataValue("test"));

            if (trade.monitor.price <= bbLower) {
                return true;
            }

            return false;
        }
    }
}
