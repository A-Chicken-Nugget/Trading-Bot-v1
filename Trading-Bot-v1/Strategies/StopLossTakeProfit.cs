using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trading_Bot_v1.Monitoring;

namespace Trading_Bot_v1.Strategies {
    enum ValueType {
        PERCENTAGE = 0,
        PRICE = 1,
        DOLLAR_AMOUNT = 2
    }

    class StopLossTakeProfit : Strategy {
        public static List<Indicator> required_indicators = new();
        public static List<string> required_data = new() {
            "stopLoss_value_type",
            "stopLoss_value",
            "takeProfit_value_type",
            "takeProfit_value"
        };

        public StopLossTakeProfit(string id = null, Trade trade = null, string data = null) : base(id, trade, required_indicators, data) { }

        public override bool CheckForEntry() {
            throw new Exception("Strategy method not meant for entry has been checked.");
        }

        public override bool CheckForExit() {
            //
            // Take profit
            //
            ValueType takeProfit_value_type = (ValueType)int.Parse(GetDataValue("takeProfit_value_type"));
            float takeProfit_value = float.Parse(GetDataValue("takeProfit_value"));

            switch (takeProfit_value_type) {
                case ValueType.DOLLAR_AMOUNT:
                    break;

                case ValueType.PERCENTAGE:
                    break;

                case ValueType.PRICE:
                    if (trade.entered_price >= takeProfit_value) {
                        return TakeProfit();
                    }

                    break;

                default:
                    throw new Exception("Invalid take profit value type given.");
            }

            //
            // Stop loss
            //
            ValueType stopLoss_value_type = (ValueType)int.Parse(GetDataValue("stopLoss_value_type"));
            float stopLoss_value = float.Parse(GetDataValue("stopLoss_value"));

            switch (stopLoss_value_type) {
                case ValueType.DOLLAR_AMOUNT:
                    break;

                case ValueType.PERCENTAGE:
                    break;

                case ValueType.PRICE:
                    if (trade.entered_price <= stopLoss_value) {
                        return StopLoss();
                    }

                    break;

                default:
                    throw new Exception("Invalid take profit value type given.");
            }

            return false;
        }

        public bool TakeProfit() {
            // If the strategy creator wants to adjust the take profit level if take profit is hit
            if (GetDataValue("takeProfit_increase_value_type") != null && GetDataValue("takeProfit_increase_value") != null) {
                ValueType takeProfitIncrease_value_type = (ValueType)int.Parse(GetDataValue("takeProfit_increase_value_type"));
                float takeProfitIncrease_value = float.Parse(GetDataValue("takeProfit_increase_value"));

                switch (takeProfitIncrease_value_type) {
                    case ValueType.DOLLAR_AMOUNT:
                        break;

                    case ValueType.PERCENTAGE:
                        break;

                    case ValueType.PRICE:
                        SetDataValue("takeProfit_value", trade.monitor.price + takeProfitIncrease_value);
                        UpdateData();

                        break;

                    default:
                        throw new Exception("Invalid take profit value type given. (2)");
                }
            } else {
                trade.ExitTrade("Hit take profit.");

                return true;
            }

            return false;
        }

        public bool StopLoss() {
            trade.ExitTrade("Hit stop loss.");

            return true;
        }
    }
}
