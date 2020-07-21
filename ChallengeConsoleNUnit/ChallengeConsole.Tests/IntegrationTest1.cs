using NUnit.Framework;

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
    }
}