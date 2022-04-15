using System.Text;
using Trading_Bot_v1.Monitoring;

namespace Trading_Bot_v1.Controllers {
    class GUI {
        public static string Display() {
            int number_trades = 0;

            foreach (Monitor monitor in TradingBot.monitors) {
                number_trades += monitor.trades.Count;
            }

            string gui = $@"

___________                  .___.__                 __________        __   
\__    ___/___________     __| _/|__| ____    ____   \______   \ _____/  |_ 
  |    |  \_  __ \__  \   / __ | |  |/    \  / ___\   |    |  _//  _ \   __\
  |    |   |  | \// __ \_/ /_/ | |  |   |  \/ /_/  >  |    |   (  <_> )  |  
  |____|   |__|  (____  /\____ | |__|___|  /\___  /   |______  /\____/|__|  
                      \/      \/         \//_____/           \/       v1.0

FPS: {TradingBot.fps}
Currently trading: {number_trades}
";
            return gui;
        }
    }
}
