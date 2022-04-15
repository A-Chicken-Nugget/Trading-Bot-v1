using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trading_Bot_v1.Monitoring;
using Trading_Bot_v1.Strategies;
using Trading_Bot_v1.Monitoring;

namespace Trading_Bot_v1.Custom {
    static class ListExtensions {
        //
        // List<Monitor>
        //

        public static bool ContainsId(this List<Monitor> monitors, string id) {
            foreach (Monitor monitor in monitors) {
                if (monitor.id == id) {
                    return true;
                }
            }
            return false;
        }

        public static Monitor GetById(this List<Monitor> monitors, string id) {
            foreach (Monitor monitor in monitors) {
                if (monitor.id == id) {
                    return monitor;
                }
            }

            return null;
        }

        //
        // List<Trade>
        //

        public static bool ContainsId(this List<Trade> trades, string id) {
            foreach (Trade trade in trades) {
                if (trade.id == id) {
                    return true;
                }
            }
            return false;
        }

        //
        // List<Strategy>
        //

        public static bool ContainsId(this List<Strategy> strategies, string id) {
            foreach (Strategy strategy in strategies) {
                if (strategy.id == id) {
                    return true;
                }
            }
            return false;
        }
    }
}
