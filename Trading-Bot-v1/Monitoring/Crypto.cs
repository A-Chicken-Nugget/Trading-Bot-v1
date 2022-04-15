using System.Collections.Generic;
using Trading_Bot_v1.Controllers;

namespace Trading_Bot_v1.Monitoring {
    class Crypto : Monitor {
        public Crypto(string id, string stock_id, string symbol, string tickers) : base(id, stock_id, symbol, tickers) { }

        public override void GetMarketData() {
            Request.Create("GET", $"https://api.robinhood.com/marketdata/forex/quotes/{stock_id}/",
                new Dictionary<string, string>() {
                    {"accept", "*/*"},
                    {"sec-ch-ua", "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
                    {"authorization", $"Bearer {TradingBot.account.auth_token}"}
                }, null,
                (Dictionary<string, dynamic> response) => {
                    price = float.Parse(response["mark_price"]);
                    previous_close = float.Parse(response["open_price"]);
                    //price_history.Add(price);
                    day_percentage_change = (previous_close / (price - previous_close)) * 100;
                }
            );
        }
    }
}
