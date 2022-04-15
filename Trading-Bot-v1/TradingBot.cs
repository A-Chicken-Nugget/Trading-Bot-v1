using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using Trading_Bot_v1.Controllers;
using Trading_Bot_v1.Custom;
using Trading_Bot_v1.Monitoring;

namespace Trading_Bot_v1 {
    class TradingBot {
        public static Account account = new();
        public static List<Monitor> monitors = new() {
            //new Crypto("cac33083-63c0-4f20-b684-5e5bb05c4595", "3d961844-d360-45fc-989b-f6fca761d511", "BTC", "BITSTAMP:BTCUSD"),
            //new Crypto("c4f5624a-cd89-4236-8611-6db2e5e48714", "76637d50-c702-4ed1-bcb5-5b0732a81f48", "ETH", "BITSTAMP:ETHUSD"),
            //new Crypto("a870c308-987f-4215-b47b-f38c241155d0", "1ef78e1b-049b-4f12-90e5-555dcf2fe204", "DOGE", "BITFINEX:DOGEUSD"),
            //new Crypto("b953c308-987f-4215-b47b-f38c241155d0", "383280b1-ff53-43fc-9c84-f01afd0989cd", "LTC", "COINBASE:LTCUSD")
        };
        public static long last_monitor_update = 0;
        public static long last_gui_update = 0;
        public static long last_update = 0;
        public static int update_count = 0;
        public static int fps = 0;

        /// <summary>
        /// Queries database to check for monitor/trade updates
        /// </summary>
        public static void UpdateMonitors() {
            Database.Query($@"
                SELECT *
                FROM Stocks
            ", (Dictionary<int, Dictionary<string, string>> stocks) => {
                if (stocks.Count > 0) {
                    //Fetch stocks to monitor
                    foreach (KeyValuePair<int, Dictionary<string, string>> stockTuple in stocks) {
                        Dictionary<string, string> stock = stockTuple.Value;

                        if (!monitors.ContainsId(stock["id"])) {
                            monitors.Add(
                                (Monitor)Activator.CreateInstance(
                                    Type.GetType(stock["type"]),

                                    stock["id"],
                                    stock["stock_id"],
                                    stock["symbol"],
                                    stock["ticker"]
                                )
                            );
                        }
                    }

                    Database.Query($@"
                        SELECT *
                        FROM Trades
                        WHERE status IN (0,1)
                    ", (Dictionary<int, Dictionary<string, string>> trades) => {
                        if (trades.Count > 0) {
                            foreach (KeyValuePair<int, Dictionary<string, string>> tradeTuple in trades) {
                                Dictionary<string, string> trade = tradeTuple.Value;
                                Monitor monitor = monitors.GetById(trade["stock_id"]);

                                if (monitor != null) {
                                    if (monitor.trades.ContainsId(trade["id"])) {
                                        // check for updates in trade data
                                    } else {
                                        monitor.trades.Add(
                                            new Trade(
                                                trade["id"],
                                                (TradeStatus)int.Parse(trade["status"]),
                                                bool.Parse(trade["is_auto"]),
                                                float.Parse(trade["quantity"]),
                                                (trade["entered_price"] == "" ? 0 : float.Parse(trade["entered_price"])),
                                                (trade["entered_date"] == "" ? 0 : long.Parse(trade["entered_date"])),
                                                monitor
                                            )
                                        );
                                    }
                                } else {
                                    // trade monitor doesn't exist so close trade
                                }
                            }
                        }
                    });
                }
            });
        }

        static void Main(string[] args) {
            // Have trade class handle the strategy data on trade enter/exit

            //Console.WriteLine(Toolbox.EscapeJSONQuotes(JsonConvert.SerializeObject(
            //    new Dictionary<string, dynamic>() {
            //        {"quantity", "1"},
            //        {"exit_strategies", new List<Dictionary<string, dynamic>>() {
            //            new Dictionary<string, dynamic>() {
            //                {"type", "Trading_Bot_v1.Strategies.StopLossTakeProfit"},
            //                {"data", new Dictionary<string, string>() {
            //                    {"stopLoss_value_type", "1"},
            //                    {"stopLoss_value", "100"},
            //                    {"takeProfit_value_type", "1"},
            //                    {"takeProfit_value", "100"},
            //                }}
            //            }
            //        }}
            //    }
            //)));
            //Dictionary<string, dynamic> test = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>("{"test":"test1","exit_strategies":[{"type":"Trading_Bot_v1.Strategies.Test","data":{"test":"test2"}},{"type":"Trading_Bot_v1.Strategies.Test","data":{"test":"test3"}}]}");

            Console.Title = "Trading Bot v1.0";
            Console.ForegroundColor = ConsoleColor.Green;

            // Manage authentication token for requests
            try {
                using (StreamReader sr = File.OpenText("session.txt")) {
                    account.auth_token = sr.ReadLine();

                    sr.Close();

                    account.Setup();
                }
            } catch (Exception ex) {
                account.Login();
            }

            Console.CursorVisible = false;

            while (true) {
                update_count++;

                if (DateTimeOffset.Now.ToUnixTimeSeconds() - last_monitor_update > 10) {
                    UpdateMonitors();

                    last_monitor_update = DateTimeOffset.Now.ToUnixTimeSeconds();
                }

                foreach (Monitor monitor in monitors) {
                    monitor.Run();
                }

                if (DateTimeOffset.Now.ToUnixTimeSeconds() - last_gui_update > 3) {
                    //Console.SetCursorPosition(0, 0);
                    //Console.Clear();
                    //Console.WriteLine(GUI.Display());

                    last_gui_update = DateTimeOffset.Now.ToUnixTimeSeconds();
                }

                if (DateTimeOffset.Now.ToUnixTimeSeconds() - last_update >= 1) {
                    fps = update_count;
                    update_count = 0;
                    last_update = DateTimeOffset.Now.ToUnixTimeSeconds();
                }
            }
        }
    }
}
