using System;
using System.Timers;
using System.Net.Http;
using System.Threading.Tasks;
using Hazelcast.Client;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Hazelcast.Core;
using System.Collections;
using System.Diagnostics;
using System.Net.Mail;

namespace ChallengeConsole
{
    public class Program
    {  
        public static Dictionary<string, string> dictionary;
        private static System.Timers.Timer aTimer;
        public static IMap<string, double> mapMetricsAggregatedData;
        private static ArrayList arlistCaloriesBurned;
        public static ArrayList arlistBodyWeights;
        private static ArrayList arlistSpeeds;
        private static ArrayList arlistPhysicalActivity;
        private static ArrayList arlistSteps;
        private static ArrayList arlistBodyMassIndexes;
        
        public static bool IsValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        static async Task Main(string[] args)
        {
            dictionary = new Dictionary<string, string>();
            dictionary.Add("googlefit", "");
            dictionary.Add("fitbit", "");

            Dictionary<string, string> dictionaryCopy = new Dictionary<string, string>(dictionary);

            foreach(KeyValuePair<string, string> item in dictionaryCopy) {
                
                string shimKey = item.Key;
                Console.WriteLine(shimKey);
                string userName = "";
                do {
                    Console.Write("Enter your username (email address) for " + shimKey + ": ");
                    userName = Console.ReadLine();
                    if (!string.IsNullOrEmpty(userName)) {
                        if(IsValid(userName)){

                            dictionary[shimKey] = userName;
                        } else {
                            Console.WriteLine("Email address is not valid, please try again.");
                        }
                    } else {
                      Console.WriteLine("Empty input, please try again.");
                    }
                } while (string.IsNullOrEmpty(userName) || !IsValid(userName)); 
            }

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

            string userNameGooglefit = dictionary["googlefit"];
            //Googlefit
            await ReadData("googlefit", userNameGooglefit, "body_height", "2020-07-12", "2020-07-13");

            arlistBodyWeights = new ArrayList();
            await ReadData("googlefit", userNameGooglefit, "body_weight", "2020-07-12", "2020-07-13");
            BodyWeightAgregation(userNameGooglefit, "googlefit", "body_weight", arlistBodyWeights);

            arlistCaloriesBurned = new ArrayList();
            await ReadData("googlefit", userNameGooglefit, "calories_burned", "2020-07-12", "2020-07-13");
            CaloriesBurnedAgregation(userNameGooglefit, "googlefit", "calories_burned", arlistCaloriesBurned);


            await ReadData("googlefit", userNameGooglefit, "heart_rate", "2020-07-12", "2020-07-13");

            arlistPhysicalActivity = new ArrayList();
            await ReadData("googlefit", userNameGooglefit, "physical_activity", "2020-07-14", "2020-07-14");
            PhysicalActivityAgregation(userNameGooglefit, "googlefit", "physical_activity", arlistPhysicalActivity);

            arlistSpeeds = new ArrayList();
            await ReadData("googlefit", userNameGooglefit, "speed", "2020-07-12", "2020-07-13");
            SpeedAgregation(userNameGooglefit, "googlefit", "speed", arlistSpeeds);

            arlistSteps = new ArrayList();
            await ReadData("googlefit", userNameGooglefit, "step_count", "2020-07-12", "2020-07-13");
            StepCountAgregation(userNameGooglefit, "googlefit", "step_count", arlistSteps);

            string userNameFitbit = dictionary["fitbit"];
            //Fitbit
            arlistBodyMassIndexes = new ArrayList();
            await ReadData("fitbit", userNameFitbit, "body_mass_index", "2020-07-12", "2020-07-13");
            BodyMaxIndexAgregation(userNameFitbit, "fitbit", "body_mass_index", arlistBodyMassIndexes);

            arlistBodyWeights = new ArrayList();
            await ReadData("fitbit", userNameFitbit, "body_weight", "2020-07-12", "2020-07-13");
            BodyWeightAgregation(userNameFitbit, "fitbit", "body_weight", arlistBodyWeights);

            arlistPhysicalActivity = new ArrayList();
            await ReadData("fitbit", userNameFitbit, "physical_activity", "2020-07-12", "2020-07-13");
            PhysicalActivityAgregation(userNameFitbit, "fitbit", "physical_activity", arlistPhysicalActivity);

            ReadFromStoreage();
        }

        public static async Task InitiateAuthorization() {
            
            Console.WriteLine("Initiate authorization...");

            foreach(KeyValuePair<string, string> item in dictionary) {
                string shimKey = item.Key;
                string userName = item.Value;
                string baseURL = $"http://localhost:8083/authorize/{shimKey}?username={userName}";
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
                                    Console.WriteLine("Data:");
                                    Console.WriteLine(dataObj);

                                    String isAuthorized = $"{dataObj["isAuthorized"]}";
                                    Console.WriteLine("{0} isAuthorized : {1}" , shimKey, isAuthorized);
                                    if(!string.IsNullOrEmpty(isAuthorized)) {
                                        if(isAuthorized.Equals("False")) {
                                            String autorizationUrl = $"{dataObj["authorizationUrl"]}";
                                            Console.WriteLine("authorizationUrl : {0}" ,autorizationUrl);

                                            if(!string.IsNullOrEmpty(autorizationUrl)) {
                                                Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", autorizationUrl);
                                            }
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

            SetTimer();
        
            Console.WriteLine("\nPress the Enter key to exit the application...\n");
            Console.WriteLine("The application started at {0:HH:mm:ss.fff}", DateTime.Now);
            Console.ReadLine();
            aTimer.Stop();
            aTimer.Dispose();
      
            Console.WriteLine("Terminating the application...");
        }

        public static async Task ReadData(string shimKey, string userName, string endpoint, string startDate, string endDate) {

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
            
            double sum = AddDouble(arlistCaloriesBurned);
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
            double max = MaxValue(arlistSpeeds);
            Console.WriteLine("SPEED : {0}", max.ToString());
            SaveData(userName, shimKey, endpoint, max.ToString());
        }

        public static void StepCountAgregation(string userName, string shimKey, string endpoint, ArrayList arlistSteps) {
            
            int sum = Add(arlistSteps);
            Console.WriteLine("STEP_COUNT : {0}", sum.ToString());
            SaveData(userName, shimKey, endpoint, sum.ToString());
        }

        
        public static double MaxValue(ArrayList arlist) {
            
            double max = 0.0;
            if(arlist.Count > 0) {

                max = double.MinValue;
                foreach (string item in arlist)
                {
                    if (Double.Parse(item) > max)
                    {
                        max =Double.Parse(item);
                    }
                 }
            }
            return max;
        }

        public static int Add(ArrayList arlist) {
           
            int sum = 0;
            foreach (string item in arlist)
            {
                sum = sum + Int32.Parse(item);
            }
            return sum;
        }

        public static double AddDouble(ArrayList arlist) {
           
            double sum = 0;
            foreach (string item in arlist)
            {
                sum = sum + Convert.ToDouble(item);
            }
            return sum;
        }

        public static bool IsArrayListEmpty(ArrayList arlist) {
            
            if(arlist.Count > 0) {
                return false;
            } 

            return true;
        }

        private static void BodyMaxIndexAgregation(string userName, string shimKey, string endpoint, ArrayList arlistBodyMassIndexes) {

            if(!IsArrayListEmpty(arlistBodyMassIndexes)) {
                string bodyMassIndex = arlistBodyMassIndexes[arlistBodyMassIndexes.Count-1].ToString();
                Console.WriteLine("BODY_MASS_INDEX : {0}", bodyMassIndex);
                SaveData(userName, shimKey, endpoint, bodyMassIndex);
            }
        }
 
        public static void SetupStoreage() {   
            
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

        public static void DeleteStoreageContent() 
        {
            mapMetricsAggregatedData.Clear();
        }
    }
}
