using NUnit.Framework;

namespace Game.Core.Tests
{
    public class TickSimulationTests
    {
        [Test]
        public void NewSimulation_StartsAtTickZero()
        {
            var sim = new TickSimulation();

            Assert.AreEqual(0, sim.State.tick);
        }

        [Test]
        public void Tick_IncrementsStateTickOnce()
        {
            var sim = new TickSimulation();

            sim.Tick();

            Assert.AreEqual(1, sim.State.tick);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(60)]
        public void TickCalledNTimes_StateTickEqualsN(int n)
        {
            var sim = new TickSimulation();

            for (int i = 0; i < n; i++)
                sim.Tick();

            Assert.AreEqual(n, sim.State.tick);
        }

        [Test]
        public void TwoSimulationsTickedIdentically_ReachIdenticalState()
        {
            var a = new TickSimulation();
            var b = new TickSimulation();

            for (int i = 0; i < 10; i++)
            {
                a.Tick();
                b.Tick();
            }

            Assert.AreEqual(a.State.tick, b.State.tick);
        }
    }
}
