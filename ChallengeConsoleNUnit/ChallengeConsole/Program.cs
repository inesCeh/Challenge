using System;
using System.Timers;
using System.Net.Http;
using System.Threading.Tasks;
using Hazelcast.Client;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Hazelcast.Core;
using System.Collections;

namespace ChallengeConsole
{
    public class Program
    {  
        private static System.Timers.Timer aTimer;
        static IMap<string, double> mapMetricsAggregatedData;
        static private ArrayList arlistCaloriesBurned;
        static private ArrayList arlistBodyWeights;
        static private ArrayList arlistSpeeds;
        static private ArrayList arlistPhysicalActivity;
        static private ArrayList arlistSteps;
        static private ArrayList arlistBodyMassIndexes;
        
        static async Task Main(string[] args)
        {
            SetupStoreage();
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

            DeleteStoreageContent(); 

            //Googlefit
            await ReadData("googlefit", "body_height", "challengeines@gmail.com", "2020-07-12", "2020-07-13");

            arlistBodyWeights = new ArrayList();
            await ReadData("googlefit", "body_weight", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            BodyWeightAgregation("challengeines@gmail.com", "googlefit", "body_weight", arlistBodyWeights);

            arlistCaloriesBurned = new ArrayList();
            await ReadData("googlefit", "calories_burned", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            CaloriesBurnedAgregation("challengeines@gmail.com", "googlefit", "calories_burned", arlistCaloriesBurned);


            await ReadData("googlefit", "heart_rate", "challengeines@gmail.com", "2020-07-12", "2020-07-13");

            arlistPhysicalActivity = new ArrayList();
            await ReadData("googlefit", "physical_activity", "challengeines@gmail.com", "2020-07-14", "2020-07-14");
            PhysicalActivityAgregation("challengeines@gmail.com", "googlefit", "physical_activity", arlistPhysicalActivity);

            arlistSpeeds = new ArrayList();
            await ReadData("googlefit", "speed", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            SpeedAgregation("challengeines@gmail.com", "googlefit", "speed", arlistSpeeds);

            arlistSteps = new ArrayList();
            await ReadData("googlefit", "step_count", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            StepCountAgregation("challengeines@gmail.com", "googlefit", "step_count", arlistSteps);

            //Fitbit
            arlistBodyMassIndexes = new ArrayList();
            await ReadData("fitbit", "body_mass_index", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            BodyMaxIndexAgregation("challengeines@gmail.com", "fitbit", "body_mass_index", arlistBodyMassIndexes);

            arlistBodyWeights = new ArrayList();
            await ReadData("fitbit", "body_weight", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            BodyWeightAgregation("challengeines@gmail.com", "fitbit", "body_weight", arlistBodyWeights);

            arlistPhysicalActivity = new ArrayList();
            await ReadData("fitbit", "physical_activity", "challengeines@gmail.com", "2020-07-12", "2020-07-13");
            PhysicalActivityAgregation("challengeines@gmail.com", "fitbit", "physical_activity", arlistPhysicalActivity);

            ReadFromStoreage();
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
                                //Console.WriteLine(data);  
                                //Console.WriteLine("Shim key : {0}", dataObj["shim"]);
                                Console.WriteLine("Shim key : {0}", shimKey);
                                Console.WriteLine("Endpoint : {0}", endpoint);
                                string body = $"{dataObj["body"]}";

                                JArray jsonArray = JArray.Parse(body) as JArray;
                                dynamic bodies = jsonArray;
                                foreach (var item in bodies)
                                {
                                    if (string.Equals(endpoint, "body_height")) {
                                        Console.WriteLine("HEIGHT : {0}", item.body.body_height.value.ToString());
                                        SaveData(userName, shimKey, endpoint, item.body.body_height.value.ToString());
                                    } else if(string.Equals(endpoint, "body_weight")) {
                                        arlistBodyWeights.Add(item.body.body_weight.value.ToString());
                                    } else if (string.Equals(endpoint, "calories_burned")) {
                                        arlistCaloriesBurned.Add(item.body.kcal_burned.value.ToString());
                                    } else if (string.Equals(endpoint, "heart_rate")) {
                                        Console.WriteLine("HEART_RATE : {0}", item.body.heart_rate.value.ToString());
                                        SaveData(userName, shimKey, endpoint, item.body.heart_rate.value.ToString());
                                    } else if (string.Equals(endpoint, "step_count")) {
                                        arlistSteps.Add(item.body.step_count.ToString()); 
                                    } else if (string.Equals(endpoint, "speed")) {
                                       arlistSpeeds.Add(item.body.speed.value.ToString());
                                    } else if (string.Equals(endpoint, "body_mass_index")) {
                                        arlistBodyMassIndexes.Add(item.body.body_mass_index.value.ToString());
                                    } else if (string.Equals(endpoint, "physical_activity")) {
                                        arlistPhysicalActivity.Add(item.body.activity_name.ToString());
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

        private static void BodyWeightAgregation(string userName, string shimKey, string endpoint, ArrayList arlistBodyWeights) {
            
            if(arlistBodyWeights.Count > 0) {
                string weight = arlistBodyWeights[arlistBodyWeights.Count-1].ToString();
                Console.WriteLine("WEIGHT : {0}", weight);
                SaveData(userName, shimKey, endpoint, weight);
            }
        }

         private static void CaloriesBurnedAgregation(string userName, string shimKey, string endpoint, ArrayList arlistCaloriesBurned){
            
            double sum = 0;
            foreach (string caloriesBurnedValue in arlistCaloriesBurned)
            {
                sum = sum + Double.Parse(caloriesBurnedValue);
            }
            Console.WriteLine("CALORIES_BURNED : {0}", sum.ToString());
            SaveData(userName, shimKey, endpoint, sum.ToString());
         }

        private static void PhysicalActivityAgregation(string userName, string shimKey, string endpoint, ArrayList physical_activity)
        {
            double sum = physical_activity.Count;
            Console.WriteLine("PHYSICAL_ACTIVITY : {0}", sum.ToString());
            SaveData(userName, shimKey, endpoint, sum.ToString());
        }

        private static void SpeedAgregation(string userName, string shimKey, string endpoint, ArrayList arlistSpeeds) {

            if(arlistBodyWeights.Count > 0) {
                double sum = 0;
                double avg = 0;
                foreach (string speedValue in arlistSpeeds)
                {
                    sum = sum + Double.Parse(speedValue);
                }
                avg = sum/arlistBodyWeights.Count;

                Console.WriteLine("SPEED : {0}", avg.ToString());
                SaveData(userName, shimKey, endpoint, avg.ToString());
            }
        }

        private static void StepCountAgregation(string userName, string shimKey, string endpoint, ArrayList arlistSteps) {
            
            int sum = 0;
            foreach (string stepCountValue in arlistSteps)
            {
                sum = sum + Int32.Parse(stepCountValue);
            }
            Console.WriteLine("STEP_COUNT : {0}", sum.ToString());
            SaveData(userName, shimKey, endpoint, sum.ToString());
        }

        public int Add(ArrayList arlist) {
            int sum = 0;
            foreach (string item in arlist)
            {
                sum = sum + Int32.Parse(item);
            }
            return sum;
        }

        private static void BodyMaxIndexAgregation(string userName, string shimKey, string endpoint, ArrayList arlistBodyMassIndexes) {

            if(arlistBodyMassIndexes.Count > 0) {
                string bodyMassIndex = arlistBodyWeights[arlistBodyMassIndexes.Count-1].ToString();
                Console.WriteLine("BODY_MASS_INDEX : {0}", bodyMassIndex);
                SaveData(userName, shimKey, endpoint, bodyMassIndex);
            }
        }
 
        private static void SetupStoreage() {   
            
            var client = HazelcastClient.NewHazelcastClient();
            mapMetricsAggregatedData = client.GetMap<string, double>("metrics-aggregated-data");
        }

        private static void SaveData(string userName, string shimKey, string endpoint, string value) {
 
            string key = userName + "*" + shimKey + "*" + endpoint;
            mapMetricsAggregatedData.Put(key, double.Parse(value));
                       
            //client.Shutdown();
        }

        private static void ReadFromStoreage() 
        {
            double value = 0;
            ICollection<string> keys = mapMetricsAggregatedData.KeySet();
            foreach (var key in keys) {
                value =  mapMetricsAggregatedData.Get(key);
                string temp = "Metric: " + "Key: " +  key + " Value: " + value;
                Console.WriteLine(temp);
            }
        }

        private static void DeleteStoreageContent() 
        {
            mapMetricsAggregatedData.Clear();
        }
    }
}
