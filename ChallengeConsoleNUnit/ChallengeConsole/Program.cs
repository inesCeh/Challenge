﻿using System;
using System.Timers;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Mail;
using Hazelcast.Client;
using Hazelcast.Core;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ChallengeConsole
{
    public class Program
    {  
        public static Dictionary<string, string> dictionary;
        public static System.Timers.Timer aTimer;
        public static IMap<string, double> mapMetricsAggregatedData;
        private static ArrayList arlistCaloriesBurned;
        public static ArrayList arlistBodyWeights;
        private static ArrayList arlistSpeeds;
        private static ArrayList arlistHeartRates;
        private static ArrayList arlistPhysicalActivity;
        private static ArrayList arlistSteps;
        private static ArrayList arlistBodyMassIndexes;

        /// <summary>
        /// Checks if email address is valid.
        /// </summary>
        /// <param name="emailaddress"></param>
        /// <returns>True, if email address is valid and false otherwise</returns>
        public static bool IsValid(string emailAddress)
        {
            try {
                MailAddress addres = new MailAddress(emailAddress);
                return addres.Address == emailAddress;
            } catch {
                return false;
            }
        }
        
        /// <summary>
        /// Create dictionary with shimKeys in userNames
        /// </summary>
        public static void CreateDictionary() {
            
            dictionary = new Dictionary<string, string>();
            dictionary.Add("googlefit", "");
            dictionary.Add("fitbit", "");
        }

        static async Task Main(string[] args)
        {
            CreateDictionary();

            Dictionary<string, string> dictionaryCopy = new Dictionary<string, string>(dictionary);

            foreach(KeyValuePair<string, string> item in dictionaryCopy) {
                
                string shimKey = item.Key;
                Console.WriteLine(shimKey);
                string userName = "";
                //Until a user enters a valid email address (userName)
                do {
                    Console.Write("Enter your username (email address) for " + shimKey + ": ");
                    userName = Console.ReadLine();
                    //If userName is not null or empty string 
                    if (!string.IsNullOrEmpty(userName)) {
                        //If email address is valid, we add it to dictionary
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

        public static void SetTimer()
        {
            // 30 seconds
            aTimer = new System.Timers.Timer(30000);
            // 1 hour
            //aTimer = new System.Timers.Timer(3600000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
                          e.SignalTime);

            DeleteStoreageContent(); 

            // Get user name for Google Fit
            string userNameGooglefit = dictionary["googlefit"];

            await ReadData("googlefit", userNameGooglefit, "body_height", "2020-07-12", "2020-07-13");

            arlistBodyWeights = new ArrayList();
            await ReadData("googlefit", userNameGooglefit, "body_weight", "2020-07-12", "2020-07-13");
            BodyWeightAgregation(userNameGooglefit, "googlefit", "body_weight", arlistBodyWeights);

            arlistCaloriesBurned = new ArrayList();
            await ReadData("googlefit", userNameGooglefit, "calories_burned", "2020-07-12", "2020-07-13");
            CaloriesBurnedAgregation(userNameGooglefit, "googlefit", "calories_burned", arlistCaloriesBurned);

            arlistHeartRates = new ArrayList();
            await ReadData("googlefit", userNameGooglefit, "heart_rate", "2020-07-12", "2020-07-13");
            HeartRateAgregation(userNameGooglefit, "googlefit", "heart_rate", arlistHeartRates);

            arlistPhysicalActivity = new ArrayList();
            await ReadData("googlefit", userNameGooglefit, "physical_activity", "2020-07-14", "2020-07-14");
            PhysicalActivityAgregation(userNameGooglefit, "googlefit", "physical_activity", arlistPhysicalActivity);

            arlistSpeeds = new ArrayList();
            await ReadData("googlefit", userNameGooglefit, "speed", "2020-07-12", "2020-07-13");
            SpeedAgregation(userNameGooglefit, "googlefit", "speed", arlistSpeeds);

            arlistSteps = new ArrayList();
            await ReadData("googlefit", userNameGooglefit, "step_count", "2020-07-12", "2020-07-13");
            StepCountAgregation(userNameGooglefit, "googlefit", "step_count", arlistSteps);

            // Get user name for Fitbit
            string userNameFitbit = dictionary["fitbit"];

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

        /// <summary>
        /// Perform authorization for all APIs
        /// </summary>
        /// <returns>Returns no value.</returns>
        public static async Task InitiateAuthorization() {
            
            Console.WriteLine("Initiate authorization...");

            //For each API
            foreach(KeyValuePair<string, string> item in dictionary) {
                string shimKey = item.Key;
                string userName = item.Value;
                // Create GET request
                string baseURL = $"http://localhost:8083/authorize/{shimKey}?username={userName}";
                try { 
                    using (HttpClient client = new HttpClient())
                    {
                        using (HttpResponseMessage res = await client.GetAsync(baseURL))
                        {
                            using (HttpContent content = res.Content)
                            {
                                string data = await content.ReadAsStringAsync();

                                // If there is some data
                                if (data != null)
                                {
                                    var dataObj = JObject.Parse(data);
                                    Console.WriteLine("Data:");
                                    Console.WriteLine(dataObj);

                                    // Get value of isAuthorized
                                    String isAuthorized = $"{dataObj["isAuthorized"]}";
                                    Console.WriteLine("{0} isAuthorized : {1}" , shimKey, isAuthorized);
                                    // If string is not null or empty
                                    if(!string.IsNullOrEmpty(isAuthorized)) {
                                        // If user is not authorized
                                        if(isAuthorized.Equals("False")) {
                                            // Get authorization URL
                                            String autorizationUrl = $"{dataObj["authorizationUrl"]}";
                                            Console.WriteLine("authorizationUrl : {0}" ,autorizationUrl);
                                            // If authorization string is not null or empty
                                            if(!string.IsNullOrEmpty(autorizationUrl)) {
                                                // Redirect your user to authorization URL. User will land on the third-party website where they can login and authorize access to their third-party user account.
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

        /// <summary>
        /// Read the data for API metric
        /// </summary>
        /// <param name="shimKey">Key of the shim.</param>
        /// <param name="userName">Username.</param>
        /// <param name="endpoint">Endpoint (kind of metric).</param>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End date.</param>
        /// <returns>Returns no value.</returns>
        public static async Task ReadData(string shimKey, string userName, string endpoint, string startDate, string endDate) {

            Console.WriteLine("Read data...");

            // Create request to pull data from a third-party API
            string baseURL = $"http://localhost:8083/data/{shimKey}/{endpoint}?username={userName}&dateStart={startDate}&&dtaEnd={endDate}&normalize=true";
            try { 
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage res = await client.GetAsync(baseURL))
                    {
                        using (HttpContent content = res.Content)
                        {
                            string data = await content.ReadAsStringAsync();
                            // If there is some data
                            if (data != null)
                            {
                                var dataObj = JObject.Parse(data);
                                Console.WriteLine("Shim key : {0}", shimKey);
                                Console.WriteLine("Endpoint : {0}", endpoint);
                                string body = $"{dataObj["body"]}";

                                // List of Body elements 
                                JArray jsonArray = JArray.Parse(body) as JArray;
                                dynamic bodies = jsonArray;
                                foreach (var item in bodies)
                                {
                                    if (string.Equals(endpoint, "body_height")) {
                                        Console.WriteLine("BODY_HEIGHT : {0}", item.body.body_height.value.ToString());
                                        SaveData(userName, shimKey, endpoint, item.body.body_height.value.ToString());
                                    } else if(string.Equals(endpoint, "body_weight")) {
                                        arlistBodyWeights.Add(item.body.body_weight.value.ToString());
                                    } else if (string.Equals(endpoint, "calories_burned")) {
                                        arlistCaloriesBurned.Add(item.body.kcal_burned.value.ToString());
                                    } else if (string.Equals(endpoint, "heart_rate")) {
                                        arlistHeartRates.Add(item.body.heart_rate.value.ToString());    
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

        /// <summary>
        /// Find max value and return result.
        /// </summary>
        /// <param name="arlist">Array List of strings.</param>
        /// <returns>Max value.</returns>
        public static double MaxValue(ArrayList arlist) {
            
            double max = 0.0;
            if(arlist.Count > 0) {

                max = double.MinValue;
                foreach (string item in arlist)
                {
                    if (Double.Parse(item) > max)
                    {
                        max = Double.Parse(item);
                    }
                 }
            }
            return max;
        }

        /// <summary>
        /// Find min value and return result.
        /// </summary>
        /// <param name="arlist">Array List of strings.</param>
        /// <returns>Min value.</returns>
        public static int MinValue(ArrayList arlist) {
            
            int min = 0;
            if(arlist.Count > 0) {

                min = int.MaxValue;
                foreach (string item in arlist)
                {
                    if (Int32.Parse(item) < min)
                    {
                        min = Int32.Parse(item);
                    }
                 }
            }
            return min;
        }

        /// <summary>
        /// Sum all values in Array List and return result.
        /// </summary>
        /// <param name="arlist">Array List of ints.</param>
        /// <returns>Sum of elements in Array List.</returns>
        public static int Add(ArrayList arlist) {
           
            int sum = 0;
            foreach (string item in arlist)
            {
                sum = sum + Int32.Parse(item);
            }
            return sum;
        }

        /// <summary>
        /// Sum all values in Array List and return result.
        /// </summary>
        /// <param name="arlist">Array List of doubles.</param>
        /// <returns>Sum of elements in Array List.</returns>
        public static double AddDouble(ArrayList arlist) {
           
            double sum = 0;
            foreach (string item in arlist)
            {
                sum = sum + Convert.ToDouble(item);
            }
            return sum;
        }

        /// <summary>
        /// Checks if ArrayList is empty.
        /// </summary>
        /// <param name="arlist">Array List</param>
        /// <returns>False, if Array List is not empty and true otherwise</returns>
        public static bool IsArrayListEmpty(ArrayList arlist) {
            
            if(arlist.Count > 0) {
                return false;
            } 

            return true;
        }

        /// <summary>
        /// Save last added weight.
        /// </summary>
        /// <param name="userName">Username.</param>
        /// <param name="shimKey">Key of the shim.</param>
        /// <param name="endpoint">Endpoint (kind of metric).</param>
        /// <param name="arlistBodyWeights">Array of body weights.</param>
        private static void BodyWeightAgregation(string userName, string shimKey, string endpoint, ArrayList arlistBodyWeights) {
            
            string weight = "";
            if(!IsArrayListEmpty(arlistBodyWeights)) {
                weight = arlistBodyWeights[arlistBodyWeights.Count-1].ToString();
            }
            Console.WriteLine("WEIGHT : {0}", weight);
            SaveData(userName, shimKey, endpoint, weight);
        }

        /// <summary>
        /// Save sum of all burned calories.
        /// </summary>
        /// <param name="userName">Username.</param>
        /// <param name="shimKey">Key of the shim.</param>
        /// <param name="endpoint">Endpoint (kind of metric).</param>
        /// <param name="arlistCaloriesBurned">Array of burned calories.</param>
         private static void CaloriesBurnedAgregation(string userName, string shimKey, string endpoint, ArrayList arlistCaloriesBurned){
            
            double sum = 0.0;
            if(!IsArrayListEmpty(arlistCaloriesBurned)) {
                sum = AddDouble(arlistCaloriesBurned);
            }
            Console.WriteLine("CALORIES_BURNED : {0}", sum.ToString());
            SaveData(userName, shimKey, endpoint, sum.ToString());
         }

        /// <summary>
        /// Save min heart rate.
        /// </summary>
        /// <param name="userName">Username.</param>
        /// <param name="shimKey">Key of the shim.</param>
        /// <param name="endpoint">Endpoint (kind of metric).</param>
        /// <param name="arlistHeartRates">Array of heart rates.</param>
        private static void HeartRateAgregation(string userName, string shimKey, string endpoint, ArrayList arlistHeartRates) {
            
            int min = 0;
            if(!IsArrayListEmpty(arlistHeartRates)) {
                min = MinValue(arlistHeartRates);
            }
            Console.WriteLine("HEART RATE : {0}", min.ToString());
            SaveData(userName, shimKey, endpoint, min.ToString());
        }

        /// <summary>
        /// Save count of all physical activities.
        /// </summary>
        /// <param name="userName">Username.</param>
        /// <param name="shimKey">Key of the shim.</param>
        /// <param name="endpoint">Endpoint (kind of metric).</param>
        /// <param name="physical_activity">Array of physical activities.</param>
        private static void PhysicalActivityAgregation(string userName, string shimKey, string endpoint, ArrayList physical_activity)
        {
            double sum = 0.0;
            if(!IsArrayListEmpty(physical_activity)) {
                sum = physical_activity.Count;
            }
            Console.WriteLine("PHYSICAL_ACTIVITY : {0}", sum.ToString());
            SaveData(userName, shimKey, endpoint, sum.ToString());
        }

        /// <summary>
        /// Save max speed.
        /// </summary>
        /// <param name="userName">Username.</param>
        /// <param name="shimKey">Key of the shim.</param>
        /// <param name="endpoint">Endpoint (kind of metric).</param>
        /// <param name="arlistSpeeds">Array of speeds.</param>
        private static void SpeedAgregation(string userName, string shimKey, string endpoint, ArrayList arlistSpeeds) {
            
            double max = 0.0;
            if(!IsArrayListEmpty(arlistSpeeds)) {
                max = MaxValue(arlistSpeeds);
            }
            Console.WriteLine("SPEED : {0}", max.ToString());
            SaveData(userName, shimKey, endpoint, max.ToString());
        }

        /// <summary>
        /// Save sum of all step counts.
        /// </summary>
        /// <param name="userName">Username.</param>
        /// <param name="shimKey">Key of the shim.</param>
        /// <param name="endpoint">Endpoint (kind of metric).</param>
        /// <param name="arlistSteps">Array of step counts.</param>
        public static void StepCountAgregation(string userName, string shimKey, string endpoint, ArrayList arlistSteps) {
            
            int sum = 0;
            if(!IsArrayListEmpty(arlistSteps)) {
                sum = Add(arlistSteps);
            }
            Console.WriteLine("STEP_COUNT : {0}", sum.ToString());
            SaveData(userName, shimKey, endpoint, sum.ToString());
        }

        /// <summary>
        /// Save last added body mass index.
        /// </summary>
        /// <param name="userName">Username.</param>
        /// <param name="shimKey">Key of the shim.</param>
        /// <param name="endpoint">Endpoint (kind of metric).</param>
        /// <param name="arlistBodyMassIndexes"></param>
        private static void BodyMaxIndexAgregation(string userName, string shimKey, string endpoint, ArrayList arlistBodyMassIndexes) {

            string bodyMassIndex = "";
            if(!IsArrayListEmpty(arlistBodyMassIndexes)) {
                bodyMassIndex = arlistBodyMassIndexes[arlistBodyMassIndexes.Count-1].ToString();
            }
            Console.WriteLine("BODY_MASS_INDEX : {0}", bodyMassIndex);
            SaveData(userName, shimKey, endpoint, bodyMassIndex);
        }
 
        /// <summary>
        /// Start the Hazelcast Client, connect to an already running Hazelcast Cluster and get the Distributed Map from Cluster
        /// </summary>
        public static void SetupStoreage() {   

            // Start the Hazelcast Client and connect to an already running Hazelcast Cluster on 127.0.0.1
            var client = HazelcastClient.NewHazelcastClient();
            // Get the Distributed Map from Cluster.
            mapMetricsAggregatedData = client.GetMap<string, double>("metrics-aggregated-data");
        }

        /// <summary>
        /// Store aggregated data in Hazelcast storage 
        /// </summary>
        /// <param name="userName">Username.</param>
        /// <param name="shimKey">Key of the shim.</param>
        /// <param name="endpoint">Endpoint (kind of metric).</param>
        /// <param name="value">Metric value.</param>
        private static void SaveData(string userName, string shimKey, string endpoint, string value) {
            
            //Create unique key
            string key = userName + "*" + shimKey + "*" + endpoint;
            mapMetricsAggregatedData.Put(key, double.Parse(value));
                       
            //client.Shutdown();
        }

        /// <summary>
        /// Read all content form Hazelcast storeage
        /// </summary>
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

        /// <summary>
        /// Delete all content form Hazelcast storeage
        /// </summary>
        public static void DeleteStoreageContent() 
        {
            mapMetricsAggregatedData.Clear();
        }
    }
}
