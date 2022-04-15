use trading_bot;

CREATE TABLE Stocks (
	id VARCHAR(36) NOT NULL,
    type TEXT NOT NULL,
    stock_id TEXT NOT NULL,
    symbol VARCHAR(4) NOT NULL,
    ticker VARCHAR(25) NULL,
    
    PRIMARY KEY (id)
);

CREATE TABLE Trades (
	id VARCHAR(36) NOT NULL,
    stock_id VARCHAR(36) NOT NULL,
    status INT NOT NULL DEFAULT 0,
    is_auto BOOL NOT NULL,
    quantity FLOAT NOT NULL,
    entered_price FLOAT NULL,
    entered_date LONG NULL,
    exited_price FLOAT NULL,
    exited_date LONG NULL,
    exited_description TEXT NULL,
    
    PRIMARY KEY (id)
);

CREATE TABLE Trade_Strategies (
	id VARCHAR(36) NOT NULL,
    trade_id VARCHAR(36) NOT NULL,
    type INT NOT NULL,
    class_type TEXT NOT NULL,
    data TEXT NOT NULL,
    
    PRIMARY KEY (id)
);

CREATE TABLE Monitor_Strategies (
	id VARCHAR(36) NOT NULL,
    monitor_id VARCHAR(36) NOT NULL,
    class_type TEXT NOT NULL,
    data TEXT NOT NULL
);

# Stocks
-- INSERT INTO Stocks (id, type, stock_id, symbol, ticker)
-- VALUES ("588d9200-4846-4ddd-b36b-5309b67685bf", "Trading_Bot_v1.Monitoring.Stock", "91f7ea28-e413-4ca4-b9fa-91f5822f8b8d", "TQQQ", "NASDAQ:TQQQ");

-- INSERT INTO Monitor_Strategies (id, monitor_id, class_type, data)
-- VALUES ("a53a91f0-8326-4ac0-9ba2-7fa6801c6df1", "588d9200-4846-4ddd-b36b-5309b67685bf", "Trading_Bot_v1.Strategies.Test", "{\u0022test\u0022:\u0022test1\u0022,\u0022exit_strategies\u0022:[{\u0022type\u0022:\u0022Trading_Bot_v1.Strategies.Test\u0022,\u0022data\u0022:{\u0022test\u0022:\u0022test2\u0022}},{\u0022type\u0022:\u0022Trading_Bot_v1.Strategies.Test\u0022,\u0022data\u0022:{\u0022test\u0022:\u0022test3\u0022}}]}");

# Crypto
INSERT INTO Stocks (id, type, stock_id, symbol, ticker)
VALUES ("cac33083-63c0-4f20-b684-5e5bb05c4595", "Trading_Bot_v1.Monitoring.Crypto", "3d961844-d360-45fc-989b-f6fca761d511", "BTC", "BITSTAMP:BTCUSD");
INSERT INTO Monitor_Strategies (id, monitor_id, class_type, data)
-- VALUES ("a53a91f0-8326-4ac0-9ba2-7fa6801c6df1", "cac33083-63c0-4f20-b684-5e5bb05c4595", "Trading_Bot_v1.Strategies.Test", "{\"test\":\"test1\",\"quantity\":\"1\",\"exit_strategies\":[{\"type\":\"Trading_Bot_v1.Strategies.Test\",\"data\":{\"test\":\"test2\",\"quantity\":\"1.5\"}},{\"type\":\"Trading_Bot_v1.Strategies.Test\",\"data\":{\"test\":\"test3\",\"quantity\":\"2\"}}]}");
VALUES ("a53a91f0-8326-4ac0-9ba2-7fa6801c6df1", "cac33083-63c0-4f20-b684-5e5bb05c4595", "Trading_Bot_v1.Strategies.Test", "{\"quantity\":\"1\",\"exit_strategies\":[{\"type\":\"Trading_Bot_v1.Strategies.StopLossTakeProfit\",\"data\":{\"stopLoss_value_type\":\"1\",\"stopLoss_value\":\"100\",\"takeProfit_value_type\":\"1\",\"takeProfit_value\":\"100\"}}]}");

-- INSERT INTO Stocks (id, type, stock_id, symbol, ticker)
-- VALUES ("c4f5624a-cd89-4236-8611-6db2e5e48714", "Trading_Bot_v1.Monitoring.Crypto", "76637d50-c702-4ed1-bcb5-5b0732a81f48", "ETH", "BITSTAMP:ETHUSD");
-- INSERT INTO Stocks (id, type, stock_id, symbol, ticker)
-- VALUES ("a870c308-987f-4215-b47b-f38c241155d0", "Trading_Bot_v1.Monitoring.Crypto", "76637d50-c702-4ed1-bcb5-5b0732a81f48", "DOGE", "BITFINEX:DOGEUSD");