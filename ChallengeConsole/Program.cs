using System;
using System.Timers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Data.SQLite;
using Hazelcast.Client;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Diagnostics;



namespace ChallengeConsole
{
    class Program
    {       
        private static System.Timers.Timer aTimer;
        static async Task Main(string[] args)
        {
            await InitiateAuthorization();
        }
        private static void SetTimer()
        {
            aTimer = new System.Timers.Timer(15000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
                          e.SignalTime);
            await ReadData("googlefit", "body_weight", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            await ReadData("googlefit", "body_height", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
        }

        private static async Task InitiateAuthorization() {
            
            Console.WriteLine("Initiate authorization...");

            string baseURL = $"http://localhost:8083/authorize/googlefit?username=challengeines@gmail.com";
            //string baseURL = $"http://localhost:8083/authorize/fitbit?username=challengeines@gmail.com";
            try { 
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage res = await client.GetAsync(baseURL))
                    {
                        using (HttpContent content = res.Content)
                        {
                            string data = await content.ReadAsStringAsync();

                            if (data != null)
                            {
                                var dataObj = JObject.Parse(data);

                                String autorizationUrl = $"{dataObj["authorizationUrl"]}";
                                Console.WriteLine("authorizationUrl : {0}" ,autorizationUrl);
                                
                                //TO DO: Display autoratization screen
                                //Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", autorizationUrl);
                            
                            }
                            else
                            {
                                Console.WriteLine("Data is null!");
                            }
                        }
                    }
                }
            } catch(Exception exception) {
                Console.WriteLine(exception);
            }

            SetTimer();

            Console.WriteLine("\nPress the Enter key to exit the application...\n");
            Console.WriteLine("The application started at {0:HH:mm:ss.fff}", DateTime.Now);
            Console.ReadLine();
            aTimer.Stop();
            aTimer.Dispose();
      
            Console.WriteLine("Terminating the application...");
        }

        private static async Task ReadData(string shimKey, string endpoint, string userName, string startDate, string endDate) {

            Console.WriteLine("Read data...");

            string baseURL = $"http://localhost:8083/data/{shimKey}/{endpoint}?username={userName}&dateStart={startDate}&&dtaEnd={endDate}&normalize=true";
            try { 
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage res = await client.GetAsync(baseURL))
                    {
                        using (HttpContent content = res.Content)
                        {
                            string data = await content.ReadAsStringAsync();
                            if (data != null)
                            {
                                var dataObj = JObject.Parse(data);
                                Console.WriteLine(data);  
                                Console.WriteLine("Shim key : {0}", dataObj["shim"]);
                                string body = $"{dataObj["body"]}";
                                JArray jsonArray = JArray.Parse(body) as JArray;
                                dynamic bodies = jsonArray;
                                foreach (var item in bodies)
                                {
                                    if(string.Equals(endpoint, "body_weight")) {
                                        Console.WriteLine("WEIGHT : {0}", item.body.body_weight.value.ToString());
                                    } else if (string.Equals(endpoint, "body_height")) {
                                        Console.WriteLine("HEIGHT : {0}", item.body.body_height.value.ToString());
                                    } else {
                                        Console.WriteLine("OTHER");
                                    }
                                }

                            }
                            else
                            {
                                Console.WriteLine("Data is null!");
                            }
                        }
                    }
                }
            } catch(Exception exception) {
                Console.WriteLine(exception);
            }
        }
    }
}
