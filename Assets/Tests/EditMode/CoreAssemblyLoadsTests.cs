using NUnit.Framework;

namespace Game.Core.Tests
{
    public class CoreAssemblyLoadsTests
    {
        [Test]
        public void CoreAssemblyLoads()
        {
            var sim = new TickSimulation();

            Assert.IsNotNull(sim);
            Assert.IsNotNull(sim.State);
        }
    }
}