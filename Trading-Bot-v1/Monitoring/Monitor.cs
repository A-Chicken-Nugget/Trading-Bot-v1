using System;
using System.Collections.Generic;
using Trading_Bot_v1.Controllers;
using Trading_Bot_v1.Strategies;
using Trading_Bot_v1.Monitoring;
using Trading_Bot_v1.Custom;
using Newtonsoft.Json;

namespace Trading_Bot_v1.Monitoring {
    abstract class Monitor {
        public string id;
        public string stock_id;
        public string symbol;
        public string ticker;

        public float price;
        public float day_percentage_change;
        public float previous_close;

        public List<Trade> trades = new();
        public List<Strategy> entry_strategies = new();

        public int last_refresh_strategies = 0;
        public Dictionary<string, long> trade_history = new();

        public Monitor(string id, string stock_id, string symbol, string ticker) {
            this.id = id;
            this.stock_id = stock_id;
            this.symbol = symbol;
            this.ticker = ticker;
        }

        public abstract void GetMarketData();

        /// <summary>
        /// Checks monitor for trade entries
        /// </summary>
        /// TODO: Allow customizability of the amount of trades to do. Ex.) based on quantity, frequency, etc
        public void CheckStrategies() {
            foreach (Strategy strategy in entry_strategies) {
                bool can_trade = true;

                if (trade_history.ContainsKey(strategy.id) && (DateTimeOffset.Now.ToUnixTimeSeconds() - trade_history[strategy.id] < 300)) {
                    can_trade = false;
                }

                if (can_trade) {
                    if (strategy.CheckForEntry()) {
                        string quantity = strategy.GetDataValue("quantity");
                        List<Strategy> exit_strategies = Strategy.ListFromString(strategy.GetDataValue("exit_strategies"));

                        if (quantity != null) {
                            trade_history[strategy.id] = DateTimeOffset.Now.ToUnixTimeSeconds();

                            EnterTrade(float.Parse(quantity), null, exit_strategies ?? entry_strategies);
                        } else {
                            throw new Exception($"Unable to enter trade with monitor {id}, strategy {strategy.id} isn't setup properly!");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks monitor trade strategies for trade exits
        /// </summary>
        public void CheckTradeStrategies() {
            foreach (Trade trade in (List<Trade>)new(trades)) {
                trade.CheckStrategies();
            }
        }

        /// <summary>
        /// Creates a trade to be monitored
        /// </summary>
        /// <param name="quantity">The amount of stock to purchase</param>
        /// <param name="entry_strategies">The strategies used to enter the trade</param>
        /// <param name="exit_strategies">The strategies used to exit the trade</param>
        public void EnterTrade(float quantity, List<Strategy> entry_strategies, List<Strategy> exit_strategies) {
            trades.Add(
                new Trade(
                    TradeStatus.IN_PROGRESS,
                    true,
                    quantity,
                    price,
                    DateTimeOffset.Now.ToUnixTimeSeconds(),
                    this,
                    entry_strategies,
                    exit_strategies
                )
            );
        }

        /// <summary>
        /// Queries database for monitor strategies. Adds/updates appropriately
        /// </summary>
        public void RefreshStrategies() {
            Database.Query($@"
                SELECT *
                FROM Monitor_Strategies
                WHERE monitor_id = '{id}'
            ", (Dictionary<int, Dictionary<string, string>> strategies) => {
                if (strategies.Count > 0) {
                    List<string> strategiesInDatabase = new();

                    // Add new/update existing strategies
                    foreach (KeyValuePair<int, Dictionary<string, string>> strategyTuple in strategies) {
                        Dictionary<string, string> strategy = strategyTuple.Value;

                        strategiesInDatabase.Add(strategy["id"]);

                        if (entry_strategies.ContainsId(strategy["id"])) {
                            // check for data updates
                        } else {
                            entry_strategies.Add(
                                (Strategy)Activator.CreateInstance(
                                    Type.GetType(strategy["class_type"]),

                                    strategy["id"],
                                    null,
                                    strategy["data"] != "" ? strategy["data"] : null
                                )
                            );
                        }
                    }

                    // Remove strategies not in database
                    List<Strategy> totalStrategies = new(entry_strategies);
                    string removeString = "";

                    foreach (Strategy strategy in totalStrategies) {
                        if (!strategiesInDatabase.Contains(strategy.id)) {
                            removeString += $"{strategy.id},";
                        }
                    }

                    if (removeString != "") {
                        Database.Query($@"
                            DELETE
                            FROM Monitor_Strategies
                            WHERE id IN ({removeString.TrimEnd(',')})
                        ");
                    }
                } else {
                    throw new Exception($"Monitor {id} doesn't have any strategies!");
                }
            });
        }

        /// <summary>
        /// Requests the indicator data for the trade indicators
        /// </summary>
        public void GetIndicatorData() {
            List<string> requestStrings = new();
            List<Indicator> indicators = new();

            foreach (Trade trade in trades) {
                List<Strategy> totalStrategies = new();

                totalStrategies.AddRange(entry_strategies);
                totalStrategies.AddRange(trade.entry_strategies);
                totalStrategies.AddRange(trade.exit_strategies);

                foreach (Strategy strategy in totalStrategies) {
                    foreach (Indicator indicator in strategy.indicators) {
                        if (!requestStrings.Contains(indicator.requestString)) {
                            requestStrings.Add(indicator.requestString);
                        }
                        indicators.Add(indicator);
                    }
                }
            }

            if (requestStrings.Count > 0) {
                Request.Create("POST", "https://scanner.tradingview.com/crypto/scan",
                    new Dictionary<string, string>() {
                        {"accept", "*/*"},
                        {"content-type", "application/x-www-form-urlencoded"},
                        {"sec-ch-ua", "\" Not;A Brand\";v=\"99\", \"Google Chrome\";v=\"97\", \"Chromium\";v=\"97\""}
                    },
                    new Dictionary<string, dynamic>() {
                        {"symbols",
                            new Dictionary<string, dynamic>() {
                                {"tickers",
                                    new List<string>() {
                                        ticker
                                    }
                                },
                                {"query",
                                    new Dictionary<string, List<string>>() {
                                        {"types", new List<string>() {}}
                                    }
                                }
                            }
                        },
                        {"columns",
                            requestStrings
                        }
                    },
                    (Dictionary<string, dynamic> response) => {
                        for (int i = 0; i < requestStrings.Count; i++) {
                            for (int j = 0; j < indicators.Count; j++) {
                                Indicator indicator = indicators[j];

                                if (indicator.requestString == requestStrings[i]) {
                                    indicator.value = response["data"][0]["d"][i];
                                }
                            }
                        }
                    }
                );
            }
        }

        /// <summary>
        /// Updates data used by strategies and checks them
        /// </summary>
        public void Run() {
            if (DateTimeOffset.Now.ToUnixTimeSeconds() - last_refresh_strategies > 10) {
                RefreshStrategies();

                foreach (Trade trade in trades) {
                    trade.RefreshStrategies();
                }
            }

            GetMarketData();
            GetIndicatorData();

            CheckStrategies();
            CheckTradeStrategies();
        }
    }
}
