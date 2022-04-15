using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Trading_Bot_v1.Controllers {
    static class Request {
        public static void Create(string type, string url, Dictionary<string, string> pheaders, Dictionary<string, dynamic> body, Action<Dictionary<string, dynamic>> action) {
            WebRequest info_request = WebRequest.Create(url);
            info_request.Method = type;

            // Add headers to request
            if (pheaders != null) {
                WebHeaderCollection headers = new WebHeaderCollection();

                foreach (KeyValuePair<string, string> header in pheaders) {
                    headers.Add(header.Key, header.Value);
                }
                info_request.Headers = headers;
            }

            // Add body data to the request
            if (body != null) {
                using (StreamWriter streamWriter = new StreamWriter(info_request.GetRequestStream())) {
                    streamWriter.Write(JsonConvert.SerializeObject(body));
                }
            }

            // Submit request and grab response
            try {
                using (StreamReader stream = new StreamReader(info_request.GetResponse().GetResponseStream())) {
                    Dictionary<string, dynamic> response = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(stream.ReadToEnd());

                    if (!response.ContainsKey("detail")) {
                        action(response);
                    } else {
                        Console.WriteLine("RSL ERROR >> " + response["detail"]);
                    }
                }
            } catch (Exception ex) {
                if (ex.ToString().Contains("Unauthorized")) {
                    if (TradingBot.account.logged_in) {
                        Console.WriteLine("RSL >> Session expired, please relogin");
                    } else {
                        Console.WriteLine("RSL ACCOUNT >> Login failed. Please try again");
                    }

                    TradingBot.account.Login();
                } else {
                    Console.WriteLine("RSL ERROR >> " + ex.ToString());
                }
            }
        }
    }
}
