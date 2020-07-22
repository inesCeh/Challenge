using NUnit.Framework;
using System.Collections;

namespace ChallengeConsole.Tests
{
     [TestFixture]
    public class Tests
    {
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

        [Test]
        public void MinValue_EmptyArrayList_ReturnMinNumber()
        {
            ArrayList arlist= new ArrayList();

            double result = Program.MinValue(arlist);

            Assert.AreEqual(result, 0);
        }

        [Test]
        public void MinValue_NotEmptyArrayList_ReturnMinNumber()
        {
            ArrayList arlist= new ArrayList();
            arlist.Add("6");
            arlist.Add("3");
            arlist.Add("8");

            double result = Program.MinValue(arlist);

            Assert.AreEqual(result, 3);
        }
        
        [Test]
        public void CreateDictionary_ReturnTrue()
        {
            Program.CreateDictionary();

            Assert.IsNotNull(Program.dictionary);
        }

        [Test]
        public void CreateDictionary_NotEmptyArrayList_ReturnTrue()
        {
            Program.CreateDictionary();

            Assert.IsNotEmpty(Program.dictionary);
        }

        [Test]
        public void SetTimer_ReturnTrue()
        {
            Program.SetTimer();
            Assert.AreEqual(Program.aTimer.Interval, 30000);
            //Assert.AreEqual(Program.aTimer.Interval, 3600000);
        }

        [Test]
        public void GetStartDate_ReturnTrue()
        {
            string startDate = Program.GetStartDate();
            Assert.IsNotNull(startDate);
        }

        [Test]
        public void GetEndDate_ReturnTrue()
        {
            string endDate = Program.GetEndDate();
            Assert.IsNotNull(endDate);
        }
        
        [Test]
        public void CompareStartAndEndDate_ReturnTrue()
        {
            string startDate = Program.GetStartDate();
            string endDate = Program.GetEndDate();
            Assert.AreNotEqual(startDate, endDate);
        }
    }
}