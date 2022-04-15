using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Trading_Bot_v1.Controllers {
    class Account {
        public string user_id;
        public string auth_token;
        public bool logged_in = false;

        public void Login(string username = null, string password = null) {
            logged_in = false;

            if (username == null && password == null) {
                Console.WriteLine("RSL >> Please enter your username: ");
                username = Console.ReadLine();

                Console.WriteLine("RSL >> Please enter your password");
                password = Console.ReadLine();
            }

            Request.Create("POST", "https://api.robinhood.com/oauth2/token/",
                new Dictionary<string, string>() {
                    {"accept", "*/*"},
                    {"accept-language", "en-US,en;q=0.9"},
                    {"sec-ch-ua", "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
                    {"content-type", "application/json"}
                },
                new Dictionary<string, dynamic>() {
                    {"device_token", "8ccbad04-9c7a-4ac3-aea7-13238057cbdd"},
                    {"client_id", "c82SH0WZOsabOXGP2sxqcj34FxkvfnWRZBKlBjFS"},
                    {"grant_type", "password"},
                    {"username", username},
                    {"password", password}
                },
                (Dictionary<string, dynamic> response) => {
                    if (response.ContainsKey("mfa_required")) {
                        Console.WriteLine("RSL ACCOUNT >> Enter 2FA SMS code: ");

                        string code = Console.ReadLine();

                        Request.Create("POST", "https://api.robinhood.com/oauth2/token/",
                            new Dictionary<string, string>() {
                                {"accept", "*/*"},
                                {"accept-language", "en-US,en;q=0.9"},
                                {"sec-ch-ua", "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
                                {"content-type", "application/json"}
                            },
                            new Dictionary<string, dynamic>() {
                                {"device_token", "8ccbad04-9c7a-4ac3-aea7-13238057cbdd"},
                                {"client_id", "c82SH0WZOsabOXGP2sxqcj34FxkvfnWRZBKlBjFS"},
                                {"grant_type", "password"},
                                {"username", username},
                                {"password", password},
                                {"mfa_code", code}
                            },
                            (Dictionary<string, dynamic> response) => {
                                if (response.ContainsKey("access_token")) {
                                    auth_token = response["access_token"];

                                    File.Delete("session.txt");
                                    using (FileStream fs = File.Create("session.txt")) {
                                        fs.Write(Encoding.UTF8.GetBytes(auth_token));
                                    }

                                    logged_in = true;

                                    Setup();
                                } else {
                                    Console.WriteLine("RSL ACCOUNT >> Invalid 2FA code provided");

                                    Login();
                                }
                            }
                        );
                    } else {
                        Console.WriteLine("RSL ACCOUNT >> Login failed. Please try again");

                        Login();
                    }
                }
            );
        }

        public void GetAccountInfo() {
            Request.Create("GET", "https://api.robinhood.com/user/",
                new Dictionary<string, string>() {
                    {"accept", "*/*"},
                    {"sec-ch-ua", "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
                    {"authorization", $"Bearer {auth_token}"}
                }, null,
                (Dictionary<string, dynamic> response) => {
                    user_id = response["id"];
                }
            );
        }

        public void Setup() {
            GetAccountInfo();
        }
    }
}
