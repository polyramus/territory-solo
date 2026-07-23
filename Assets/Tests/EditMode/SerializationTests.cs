using NUnit.Framework;
using UnityEngine;

namespace Game.Core.Tests
{
    public class SerializationTests
    {
        [Test]
        public void EmptyState_RoundTrips()
        {
            var state = new GameState();

            var restored = JsonUtility.FromJson<GameState>(JsonUtility.ToJson(state));

            Assert.IsNotNull(restored);
            Assert.AreEqual(state.tick, restored.tick);
        }

        [Test]
        public void TickedState_RoundTripsTick()
        {
            var sim = new TickSimulation();
            sim.Tick();
            sim.Tick();

            var json = JsonUtility.ToJson(sim.State);
            var restored = JsonUtility.FromJson<GameState>(json);

            Assert.AreEqual(2, sim.State.tick, "precondition: two ticks applied");
            Assert.AreEqual(sim.State.tick, restored.tick);
        }
    }
}
