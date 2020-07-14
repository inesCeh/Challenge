using System;
using System.Timers;
using System.Net.Http;
using System.Threading.Tasks;
using Hazelcast.Client;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Hazelcast.Core;

namespace ChallengeConsole
{
    class Program
    {  
        static IMap<string, HazelcastJsonValue> mapJsons;    
        private static System.Timers.Timer aTimer;
        static async Task Main(string[] args)
        {
            CreateDatabase();
            await InitiateAuthorization();
        }
        private static void SetTimer()
        {
            aTimer = new System.Timers.Timer(30000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
                          e.SignalTime);

            //Googlefit
            await ReadData("googlefit", "body_height", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            await ReadData("googlefit", "body_weight", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            await ReadData("googlefit", "calories_burned", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            await ReadData("googlefit", "heart_rate", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            await ReadData("googlefit", "physical_activity", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            await ReadData("googlefit", "speed", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            await ReadData("googlefit", "step_count", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            //Fitbit
            await ReadData("fitbit", "body_mass_index", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            await ReadData("fitbit", "body_weight", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            await ReadData("fitbit", "physical_activity", "challengeines@gmail.com", "2020-07-12", "2020-07-13");

            ReadFromDatabase();
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
                                SaveJsonData(shimKey, endpoint, data);
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

       private static void CreateDatabase() {
           
            var client = HazelcastClient.NewHazelcastClient();
            mapJsons = client.GetMap<string, HazelcastJsonValue>("jsonData");
       }

        private static void SaveJsonData(string metricsSource, string metricsType, string jsonData) {

            string key = metricsSource + metricsType;
            mapJsons.Put(key, new HazelcastJsonValue(jsonData));
            //client.Shutdown();
        }

        private static void ReadFromDatabase() 
        {
            ICollection<HazelcastJsonValue> jsons = mapJsons.Values();
            foreach (var json in jsons)
            {
                Console.WriteLine();   
                Console.WriteLine();   
                Console.WriteLine("******************************************************************");
                Console.WriteLine(json.ToString());
            }
        }
    }
}
