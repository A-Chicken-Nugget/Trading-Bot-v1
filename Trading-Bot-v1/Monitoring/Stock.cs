using System;
using System.Collections.Generic;
using Trading_Bot_v1.Controllers;

namespace Trading_Bot_v1.Monitoring {
    class Stock : Monitor {
        public Stock(string id, string stock_id, string symbol, string ticker) : base(id, stock_id, symbol, ticker) { }

        public override void GetMarketData() {
            Request.Create("GET", $"https://api.robinhood.com/marketdata/quotes/?bounds=trading&include_inactive=true&instruments=https://api.robinhood.com/instruments/{stock_id}/",
                new Dictionary<string, string>() {
                    {"accept", "*/*"},
                    {"sec-ch-ua", "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
                    {"authorization", $"Bearer {TradingBot.account.auth_token}"}
                }, null,
                (Dictionary<string, dynamic> response) => {
                    dynamic stock = response["results"][0];

                    price = (float)Math.Round((double)stock["last_trade_price"], 2);
                    previous_close = (float)stock["adjusted_previous_close"];
                    //price_history.Add(price);
                    day_percentage_change = (previous_close / (price - previous_close)) * 100;
                }
            );
        }
    }
}
