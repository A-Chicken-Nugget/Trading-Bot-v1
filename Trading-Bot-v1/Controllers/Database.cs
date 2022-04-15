using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Trading_Bot_v1.Controllers {
    class Database {
        public static void Query(string qs, Action<Dictionary<int, Dictionary<string, string>>> callback = null) {
            MySqlConnection connection = new MySqlConnection("server=localhost;userid=root;password=cheeseit123;database=trading_bot");
            connection.Open();

            MySqlCommand command = new MySqlCommand(qs, connection);
            MySqlDataReader resultsReader = command.ExecuteReader();

            Dictionary<int, Dictionary<string, string>> results = new Dictionary<int, Dictionary<string, string>>();

            if (resultsReader.HasRows) {
                int i = 0;

                while (resultsReader.Read()) {
                    results.Add(i, new Dictionary<string, string>());

                    for (int j = 0; j < resultsReader.FieldCount; j++) {
                        if (results[i].ContainsKey(resultsReader.GetName(j))) {
                            results[i][resultsReader.GetName(j)] = resultsReader.GetValue(j).ToString();
                        } else {
                            results[i].Add(resultsReader.GetName(j), resultsReader.GetValue(j).ToString());
                        }
                    }

                    i++;
                }
            }

            if (callback != null) {
                callback(results);
            }

            connection.Close();
            command.Dispose();
            resultsReader.Dispose();
        }
    }
}
