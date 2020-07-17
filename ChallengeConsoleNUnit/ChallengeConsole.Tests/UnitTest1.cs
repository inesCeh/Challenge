using NUnit.Framework;
using System.Collections;

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
        public void Add_MultipleNumbers_ReturnsSumOfNumbers()
        {
            ArrayList arlist= new ArrayList();
            arlist.Add("1");
            arlist.Add("2");
            arlist.Add("3");
            
            int result = _program.Add(arlist);

            Assert.AreEqual(result, 6);
        }
    }
}