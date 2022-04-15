using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Trading_Bot_v1.Controllers;
using Trading_Bot_v1.Monitoring;

namespace Trading_Bot_v1.Strategies {
    //"Recommend.Other|60",
    //"Recommend.All|60",
    //"Recommend.MA|60",
    //"RSI|60",
    //"RSI[1]|60",
    //"Stoch.K|60",
    //"Stoch.D|60",
    //"Stoch.K[1]|60",
    //"Stoch.D[1]|60",
    //"CCI20|60",
    //"CCI20[1]|60",
    //"ADX|60",
    //"ADX+DI|60",
    //"ADX-DI|60",
    //"ADX+DI[1]|60",
    //"ADX-DI[1]|60",
    //"AO|60",
    //"AO[1]|60",
    //"AO[2]|60",
    //"Mom|60",
    //"Mom[1]|60",
    //"MACD.macd|60",
    //"MACD.signal|60",
    //"Rec.Stoch.RSI|60",
    //"Stoch.RSI.K|60",
    //"Rec.WR|60",
    //"W.R|60",
    //"Rec.BBPower|60",
    //"BBPower|60",
    //"Rec.UO|60",
    //"UO|60",
    //"EMA10|60",
    //"close|60",
    //"SMA10|60",
    //"EMA20|60",
    //"SMA20|60",
    //"EMA30|60",
    //"SMA30|60",
    //"EMA50|60",
    //"SMA50|60",
    //"EMA100|60",
    //"SMA100|60",
    //"EMA200|60",
    //"SMA200|60",
    //"Rec.Ichimoku|60",
    //"Ichimoku.BLine|60",
    //"Rec.VWMA|60",
    //"VWMA|60",
    //"Rec.HullMA9|60",
    //"HullMA9|60",
    //"Pivot.M.Classic.S3|60",
    //"Pivot.M.Classic.S2|60",
    //"Pivot.M.Classic.S1|60",
    //"Pivot.M.Classic.Middle|60",
    //"Pivot.M.Classic.R1|60",
    //"Pivot.M.Classic.R2|60",
    //"Pivot.M.Classic.R3|60",
    //"Pivot.M.Fibonacci.S3|60",
    //"Pivot.M.Fibonacci.S2|60",
    //"Pivot.M.Fibonacci.S1|60",
    //"Pivot.M.Fibonacci.Middle|60",
    //"Pivot.M.Fibonacci.R1|60",
    //"Pivot.M.Fibonacci.R2|60",
    //"Pivot.M.Fibonacci.R3|60",
    //"Pivot.M.Camarilla.S3|60",
    //"Pivot.M.Camarilla.S2|60",
    //"Pivot.M.Camarilla.S1|60",
    //"Pivot.M.Camarilla.Middle|60",
    //"Pivot.M.Camarilla.R1|60",
    //"Pivot.M.Camarilla.R2|60",
    //"Pivot.M.Camarilla.R3|60",
    //"Pivot.M.Woodie.S3|60",
    //"Pivot.M.Woodie.S2|60",
    //"Pivot.M.Woodie.S1|60",
    //"Pivot.M.Woodie.Middle|60",
    //"Pivot.M.Woodie.R1|60",
    //"Pivot.M.Woodie.R2|60",
    //"Pivot.M.Woodie.R3|60",
    //"Pivot.M.Demark.S1|60",
    //"Pivot.M.Demark.Middle|60",
    //"Pivot.M.Demark.R1|60",
    //"BB.upper"
    //"BB.lower"
    //"BB.basis"

    /**
    
    Data format:

    {
        trades_on_enter = [
            {
                quantity = 1,
                exit_strategies = [
                    class_type = "",
                    data = [resursion]
                ]
            }
        ],
        trades_on_exit = [trade data],
        data = "value",
    }

    Have some keys be required for strategies to work. For example data dealing with the configuration for strategies

    **/

    class Indicator {
        public string requestString;
        public float value;

        public Indicator(string requestString) {
            this.requestString = requestString;
        }
    }

    abstract class Strategy {
        public string id;
        public string reason = "No reason provided.";
        public List<Indicator> indicators = new();
        public Dictionary<string, dynamic> data = new();

        public Trade trade;

        public Strategy(string id, Trade trade, List<Indicator> indicators, string data) {
            this.id = id;

            this.trade = trade;
            this.indicators = indicators;

            if (data != null) {
                this.data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(data);
            }
        }

        public void SetTrade(Trade trade) {
            this.trade = trade;
        }

        public int GetIndicatorValue(string requestString) {
            foreach (Indicator indicator in indicators) {
                if (indicator.requestString == requestString) {
                    return (int)Math.Floor(indicator.value);
                }
            }

            throw new Exception("Invalid indicator requestString given: " + requestString);
        }

        public dynamic GetDataValue(string key) {
            if (data.ContainsKey(key)) {
                return data[key];
            }
            return null;
        }

        public void SetDataValue(string key, dynamic value) {
            if (data.ContainsKey(key)) {
                data[key] = value;
            }
        }

        public void UpdateData() {
            Database.Query($@"
                UPDATE Trade_Strategies
                SET data = '{JsonConvert.SerializeObject(data)}'
                WHERE id = '{id}'
            ");
        }

        public abstract bool CheckForEntry();

        public abstract bool CheckForExit();

        public static List<Strategy> ListFromString(dynamic list) {
            List<Strategy> returnList = new();

            if (list != null) {
                foreach (dynamic strat in list) {
                    string type = strat["type"];

                    Console.WriteLine("Test: " + strat["data"]);

                    returnList.Add(
                        (Strategy)Activator.CreateInstance(
                            Type.GetType(type),

                            null,
                            null,
                            JsonConvert.SerializeObject(strat["data"])
                        )
                    );
                }
            }

            return returnList;
        }
    }
}
