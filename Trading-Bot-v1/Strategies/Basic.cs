using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trading_Bot_v1.Monitoring;

namespace Trading_Bot_v1.Strategies {
    class Basic : Strategy {
        public static List<Indicator> required_indicators = new() {
            new Indicator("RSI|1"),
            new Indicator("RSI|5"),
            new Indicator("CCI20|1"),
            new Indicator("CCI20|5"),
            new Indicator("BB.upper|1"),
            new Indicator("BB.upper|5"),
            new Indicator("BB.lower|1"),
            new Indicator("BB.lower|5")
        };

        public Basic(string id = null, Trade trade = null, string data = null) : base(id, trade, required_indicators, data) {}

        public override bool CheckForEntry() {
            int rsi_1m = GetIndicatorValue("RSI|1");
            int rsi_5m = GetIndicatorValue("RSI|5");
            int cci_1m = GetIndicatorValue("CCI20|1");
            int cci_5m = GetIndicatorValue("CCI20|5");
            int bbLower_1m = GetIndicatorValue("BB.lower|1");

            if (rsi_5m < 30 && cci_5m < -100) {
                if (rsi_1m < 30 || cci_1m < -100 || trade.entered_price < bbLower_1m) {
                    return true;
                }
            }
            return false;
        }

        public override bool CheckForExit() {
            int rsi_1m = GetIndicatorValue("RSI|1");
            int rsi_5m = GetIndicatorValue("RSI|5");
            int cci_1m = GetIndicatorValue("CCI20|1");
            int cci_5m = GetIndicatorValue("CCI20|5");
            int bbUpper_1m = GetIndicatorValue("BB.upper|1");

            if (rsi_5m > 70 && cci_5m > 100) {
                if (rsi_1m > 70 || cci_1m > 100 || trade.entered_price > bbUpper_1m) {
                    return true;
                }
            }
            return false;
        }
    }
}
