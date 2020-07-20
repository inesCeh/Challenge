using NUnit.Framework;
using System.Collections;
using Moq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ChallengeConsole.Tests
{
     [TestFixture]
    public class Tests
    {
        private Program _program;

        [SetUp]
        public void Setup()
        {
            _program = new Program();

        }

        /*
        [Test]
        public void Test1()
        {
            Assert.Pass();
        }*/

        [Test]
        public void Add_EmptyArrayList_ReturnsSumOfNumbers()
        {
            ArrayList arlist= new ArrayList();
            
            int result = Program.Add(arlist);

            Assert.AreEqual(result, 0);
        }

        [Test]
        public void Add_NotEmptyArrayList_ReturnsSumOfNumbers()
        {
            ArrayList arlist= new ArrayList();
            arlist.Add("1");
            arlist.Add("2");
            arlist.Add("3");
            
            int result = Program.Add(arlist);

            Assert.AreEqual(result, 6);
        }

        [Test]
        public void IsArrayListEmpty_EmptyArrayList_ReturnTrue()
        {
            ArrayList arlist= new ArrayList();

            bool result = Program.IsArrayListEmpty(arlist);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsArrayListEmpty_NotEmptyArrayList_ReturnFalse()
        {
            ArrayList arlist= new ArrayList();
            arlist.Add("1");
            arlist.Add("2");
            arlist.Add("3");

            bool result = Program.IsArrayListEmpty(arlist);

            Assert.IsFalse(result);
        }

        [Test]
        public void MaxValue_EmptyArrayList_ReturnMaxNumber()
        {
            ArrayList arlist= new ArrayList();

            double result = Program.MaxValue(arlist);

            Assert.AreEqual(result, 0);
        }

        [Test]
        public void MaxValue_NotEmptyArrayList_ReturnMaxNumber()
        {
            ArrayList arlist= new ArrayList();
            arlist.Add("1");
            arlist.Add("6");
            arlist.Add("3");

            double result = Program.MaxValue(arlist);

            Assert.AreEqual(result, 6);
        }

        [Test]
        public void IsValid_UnvalidEmailAdress_ReturnFalse()
        {
            string emailAddress = "challengeinesgmail.com";

            bool result = Program.IsValid(emailAddress);

            Assert.IsFalse(result);
        }

        [Test]
        public void IsValid_ValidEmailAdress_ReturnTrue()
        {
            string emailAddress = "challengeines@gmail.com";

            bool result = Program.IsValid(emailAddress);

            Assert.IsTrue(result);
        }

        [Test]
        public void SetupStoreage_ReturnTrue()
        {
            Program.SetupStoreage();
            Assert.IsNotNull(Program.mapMetricsAggregatedData);
        }

        
        [Test]
        public void AddDouble_EmptyArrayList_ReturnsSumOfNumbers()
        {
            ArrayList arlist= new ArrayList();
            
            double result = Program.AddDouble(arlist);

            Assert.AreEqual(result, 0);
        }

        [Test]
        public void AddDouble_NotEmptyArrayList_ReturnsSumOfNumbers()
        {
            ArrayList arlist= new ArrayList();
            arlist.Add("1,1");
            arlist.Add("2,2");
            arlist.Add("3,3");
            
            double result = Program.AddDouble(arlist);

            Assert.AreEqual(result, 6.6, 0.1);
        }
    }
}