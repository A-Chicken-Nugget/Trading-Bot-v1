using System;
using System.Collections.Generic;
using Trading_Bot_v1.Controllers;
using Trading_Bot_v1.Custom;
using Trading_Bot_v1.Strategies;

namespace Trading_Bot_v1.Monitoring {
    enum TradeStatus {
        WAITING = 0,
        IN_PROGRESS = 1,
        COMPLETED = 2
    }

    class Trade {
        public string id;
        public TradeStatus status;
        public bool is_auto;
        public float quantity;
        public float entered_price;
        public long entered_date;

        public List<Strategy> entry_strategies = new();
        public List<Strategy> exit_strategies = new();
        public Monitor monitor;

        public Trade(TradeStatus status, bool is_auto, float quantity, float entered_price, long entered_date, Monitor monitor, List<Strategy> entry_strategies, List<Strategy> exit_strategies) {
            id = Guid.NewGuid().ToString();
            this.status = status;
            this.is_auto = is_auto;
            this.quantity = quantity;
            this.entered_price = entered_price;
            this.entered_date = entered_date;

            this.entry_strategies = entry_strategies ?? new();
            this.exit_strategies = exit_strategies;
            this.monitor = monitor;

            string insertStrategyString = "";

            Console.WriteLine($"///////////////{id}");

            foreach (Strategy strategy in this.entry_strategies) {
                strategy.id = Guid.NewGuid().ToString();
                strategy.trade = this;

                Console.WriteLine(strategy.ToString());

                insertStrategyString += $@"
                    INSERT INTO Trade_Strategies (id, trade_id, type, class_type, data)
                    VALUES ('{strategy.id}', '{id}', 0, '{strategy.GetType()}', '');
                ";
            }
            foreach (Strategy strategy in this.exit_strategies) {
                strategy.id = Guid.NewGuid().ToString();
                strategy.trade = this;

                insertStrategyString += $@"
                    INSERT INTO Trade_Strategies (id, trade_id, type, class_type, data)
                    VALUES ('{strategy.id}', '{id}', 1, '{strategy.GetType()}', '');
                ";
            }

            if (status == TradeStatus.WAITING) {
                Database.Query($@"
                    INSERT INTO Trades (id, stock_id, is_auto, quantity)
                    VALUES ('{id}', '{monitor.id}', 1, {quantity});
                    {insertStrategyString}
                ");
            } else if (status == TradeStatus.IN_PROGRESS) {
                Database.Query($@"
                    INSERT INTO Trades (id, stock_id, status, is_auto, quantity, entered_price, entered_date)
                    VALUES ('{id}', '{monitor.id}', 1, 1, {quantity}, {entered_price}, {entered_date});
                    {insertStrategyString}
                ");
            }
        }

        public Trade(string id, TradeStatus status, bool is_auto, float quantity, float entered_price, long entered_date, Monitor monitor) {
            this.id = id;
            this.status = status;
            this.is_auto = is_auto;
            this.quantity = quantity;
            this.entered_price = entered_price;
            this.entered_date = entered_date;

            this.monitor = monitor;

            RefreshStrategies();
        }

        /// <summary>
        /// Queries database for trade strategies. Adds/updates appropriately
        /// </summary>
        public void RefreshStrategies() {
            Database.Query($@"
                SELECT *
                FROM Trade_Strategies
                WHERE trade_id = '{id}'
            ", (Dictionary<int, Dictionary<string, string>> strategies) => {
                if (strategies.Count > 0) {
                    List<string> strategiesInDatabase = new();

                    // Add new/update existing strategies
                    foreach (KeyValuePair<int, Dictionary<string, string>> strategyTuple in strategies) {
                        Dictionary<string, string> strategy = strategyTuple.Value;

                        strategiesInDatabase.Add(strategy["id"]);

                        // TODO: pass in custom data to strategy classes
                        if (strategy["type"] == "0") {
                            if (entry_strategies.ContainsId(strategy["id"])) {
                                // check for data updates
                            } else {
                                entry_strategies.Add(
                                    (Strategy)Activator.CreateInstance(
                                        Type.GetType(strategy["class_type"]),

                                        strategy["id"],
                                        this,
                                        strategy["data"] != "" ? strategy["data"] : null
                                    )
                                );
                            }
                        } else if (strategy["type"] == "1") {
                            if (exit_strategies.ContainsId(strategy["id"])) {
                                // check for data updates
                            } else {
                                exit_strategies.Add(
                                    (Strategy)Activator.CreateInstance(
                                        Type.GetType(strategy["class_type"]),

                                        strategy["id"],
                                        this,
                                        strategy["data"] != "" ? strategy["data"] : null
                                    )
                                );
                            }
                        } else {
                            throw new Exception($"Invalid strategy type for strategy {strategy["id"]}");
                        }
                    }

                    // Remove strategies not in database
                    List<Strategy> totalStrategies = new();
                    string removeString = "";

                    totalStrategies.AddRange(entry_strategies);
                    totalStrategies.AddRange(exit_strategies);

                    foreach (Strategy strategy in totalStrategies) {
                        if (!strategiesInDatabase.Contains(strategy.id)) {
                            removeString += $"{strategy.id},";
                        }
                    }

                    if (removeString != "") {
                        removeString.TrimEnd(',');

                        Database.Query($@"
                            DELETE
                            FROM Trade_Strategies
                            WHERE id IN ({removeString})
                        ");
                    }
                } else {
                    throw new Exception($"Trade {id} doesn't have any strategies!");
                }
            });
        }

        /// <summary>
        /// Checks the trade strategies for an exit
        /// </summary>
        public void CheckStrategies() {
            if (status == TradeStatus.WAITING) {
                foreach (Strategy strategy in entry_strategies) {
                    if (strategy.CheckForEntry()) {

                        status = TradeStatus.IN_PROGRESS;
                        entered_price = monitor.price;
                        entered_date = DateTimeOffset.Now.ToUnixTimeSeconds();

                        Database.Query($@"
                            UPDATE Trades
                            SET status = 1, entered_price = {entered_price}, entered_date = {entered_date}
                            WHERE id = '{id}'
                        ");

                        Console.WriteLine($"Entered trade with {monitor.symbol}");

                        break;
                    }
                }
            } else if (status == TradeStatus.IN_PROGRESS) {
                foreach (Strategy strategy in exit_strategies) {
                    if (strategy.CheckForExit()) {
                        ExitTrade(strategy.reason);

                        break;
                    }
                }
            }
        }

        public void ExitTrade(string reason) {
            if (status == TradeStatus.IN_PROGRESS) {
                status = TradeStatus.COMPLETED;

                //Console.WriteLine($"Exited trade with {monitor.symbol}");

                Database.Query($@"
                    UPDATE Trades
                    SET status = 3, exited_price = {monitor.price}, exited_date = {DateTimeOffset.Now.ToUnixTimeSeconds()}, exited_description = '{reason}'
                    WHERE id = '{id}'
                ", (Dictionary<int, Dictionary<string, string>> results) => {
                    monitor.trades.Remove(this);
                });
            } else {
                throw new Exception($"Attempted to exit trade when trade is not in progress. Id: {id}");
            }
        }
    }
}