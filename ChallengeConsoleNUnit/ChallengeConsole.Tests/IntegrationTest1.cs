using NUnit.Framework;
using System.Collections;

namespace ChallengeConsole.Tests
{
     [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void SetupStoreage_ReturnTrue()
        {
            Program.SetupStoreage();
            Assert.IsNotNull(Program.mapMetricsAggregatedData);
        }

        
        [Test]
        public void BodyWeightAgregation_NotEmptyArrayList_ReturnTrue()
        {
            Program.SetupStoreage();

            string userName = "integrationTest@example.com";
            string shimKey = "googlefit";
            string endpoint = "body_weight";
            ArrayList arlistBodyWeights = new ArrayList();
            arlistBodyWeights.Add("80");
            arlistBodyWeights.Add("90");
            arlistBodyWeights.Add("100");

            Program.BodyWeightAgregation(userName, shimKey, endpoint, arlistBodyWeights);
 
            string key = userName + "*" + shimKey + "*" + endpoint;
            double value =  Program.mapMetricsAggregatedData.Get(key);
            Assert.AreEqual(value, 100);
        }

        [Test]
        public void CaloriesBurnedAgregation_NotEmptyArrayList_ReturnTrue()
        {
            Program.SetupStoreage();

            string userName = "integrationTest@example.com";
            string shimKey = "googlefit";
            string endpoint = "calories_burned";
            ArrayList arlistCaloriesBurned = new ArrayList();
            arlistCaloriesBurned.Add("100,0");
            arlistCaloriesBurned.Add("200,0");
            arlistCaloriesBurned.Add("600,0");

            Program.CaloriesBurnedAgregation(userName, shimKey, endpoint, arlistCaloriesBurned);
 
            string key = userName + "*" + shimKey + "*" + endpoint;
            double value =  Program.mapMetricsAggregatedData.Get(key);
            Assert.AreEqual(value, 900.0);
        }

        [Test]
        public void PhysicalActivityAgregation_NotEmptyArrayList_ReturnTrue()
        {
            Program.SetupStoreage();

            string userName = "integrationTest@example.com";
            string shimKey = "googlefit";
            string endpoint = "physical_activity";
            ArrayList arlistPhysicalActivity = new ArrayList();
            arlistPhysicalActivity.Add("Run");
            arlistPhysicalActivity.Add("Walking");
            arlistPhysicalActivity.Add("Walking");

            Program.PhysicalActivityAgregation(userName, shimKey, endpoint, arlistPhysicalActivity);
 
            string key = userName + "*" + shimKey + "*" + endpoint;
            double value =  Program.mapMetricsAggregatedData.Get(key);
            Assert.AreEqual(value, 3);
        }

        [Test]
        public void SpeedAgregation_NotEmptyArrayList_ReturnTrue()
        {
            Program.SetupStoreage();

            string userName = "integrationTest@example.com";
            string shimKey = "googlefit";
            string endpoint = "speed";
            ArrayList arlistSpeeds = new ArrayList();
            arlistSpeeds.Add("10,2");
            arlistSpeeds.Add("20,5");
            arlistSpeeds.Add("13,4");

            Program.SpeedAgregation(userName, shimKey, endpoint, arlistSpeeds);
 
            string key = userName + "*" + shimKey + "*" + endpoint;
            double value =  Program.mapMetricsAggregatedData.Get(key);
            Assert.AreEqual(value, 20.5);
        }


        [Test]
        public void StepCountAgregation_NotEmptyArrayList_ReturnTrue()
        {
            Program.SetupStoreage();

            string userName = "integrationTest@example.com";
            string shimKey = "googlefit";
            string endpoint = "step_count";
            ArrayList arlistSteps = new ArrayList();
            arlistSteps.Add("100");
            arlistSteps.Add("200");
            arlistSteps.Add("500");

            Program.StepCountAgregation(userName, shimKey, endpoint, arlistSteps);
 
            string key = userName + "*" + shimKey + "*" + endpoint;
            double value =  Program.mapMetricsAggregatedData.Get(key);
            Assert.AreEqual(value, 800);
        }

        [Test]
        public void BodyMaxIndexAgregation_NotEmptyArrayList_ReturnTrue()
        {
            Program.SetupStoreage();

            string userName = "integrationTest@example.com";
            string shimKey = "googlefit";
            string endpoint = "body_mass_index";
            ArrayList arlistBodyMassIndexes = new ArrayList();
            arlistBodyMassIndexes.Add("20,4");
            arlistBodyMassIndexes.Add("24,8");
            arlistBodyMassIndexes.Add("23,5");

            Program.BodyMaxIndexAgregation(userName, shimKey, endpoint, arlistBodyMassIndexes);
 
            string key = userName + "*" + shimKey + "*" + endpoint;
            double value =  Program.mapMetricsAggregatedData.Get(key);
            Assert.AreEqual(value, 23.5);
        }

        [Test]
        public void HeartRateAgregation_NotEmptyArrayList_ReturnTrue()
        {
            Program.SetupStoreage();

            string userName = "integrationTest@example.com";
            string shimKey = "googlefit";
            string endpoint = "heart_rate";
            ArrayList arlistHeartRates = new ArrayList();
            arlistHeartRates.Add("88");
            arlistHeartRates.Add("64");
            arlistHeartRates.Add("72");

            Program.HeartRateAgregation(userName, shimKey, endpoint, arlistHeartRates);
 
            string key = userName + "*" + shimKey + "*" + endpoint;
            double value =  Program.mapMetricsAggregatedData.Get(key);
            Assert.AreEqual(value, 64);
        }   
    }
}