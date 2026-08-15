using NUnit.Framework;
using UnityEngine;

namespace Game.Core.Tests
{
    /// <summary>Edit-mode tests for SnakeHead movement, intent queueing, and state serialization.</summary>
    public class SnakeHeadTests
    {
        // ── Advance in a straight line ────────────────────────────────

        [Test]
        public void AdvanceNTimes_FromOriginAlongPosX_PositionEqualsN()
        {
            var sim = TestFixtures.OriginSimulation();

            for (int i = 0; i < 10; i++)
                sim.Tick();

            Assert.AreEqual(new Vector3Int(10, 0, 0), sim.State.playerHead.position);
        }

        [TestCase(Axis6.PosX, new Vector3Int(5, 0, 0))]
        [TestCase(Axis6.NegX, new Vector3Int(-5, 0, 0))]
        [TestCase(Axis6.PosY, new Vector3Int(0, 5, 0))]
        [TestCase(Axis6.NegY, new Vector3Int(0, -5, 0))]
        [TestCase(Axis6.PosZ, new Vector3Int(0, 0, 5))]
        [TestCase(Axis6.NegZ, new Vector3Int(0, 0, -5))]
        public void AdvanceNTimes_AllSixDirections_PositionCorrect(Axis6 heading, Vector3Int expected)
        {
            var sim = new TickSimulation(Vector3Int.zero, heading);

            for (int i = 0; i < 5; i++)
                sim.Tick();

            Assert.AreEqual(expected, sim.State.playerHead.position);
        }

        // ── Intent queueing and turn-at-boundary ──────────────────────

        [Test]
        public void SetIntentOnTick2_PositionChangesAxisAtTick2()
        {
            var sim = TestFixtures.OriginSimulation(); // heading PosX

            sim.Tick();  // tick 1: (1,0,0) along +X
            sim.SetPlayerIntent(Axis6.PosZ);
            sim.Tick();  // tick 2: intent applied → heading PosZ → position (1,0,1)

            Assert.AreEqual(new Vector3Int(1, 0, 1), sim.State.playerHead.position);
            Assert.AreEqual(Axis6.PosZ, sim.State.playerHead.heading);
        }

        [Test]
        public void SetIntentSameAsCurrentHeading_NoChange()
        {
            var sim = TestFixtures.OriginSimulation(); // heading PosX

            sim.SetPlayerIntent(Axis6.PosX); // same as current
            sim.Tick();

            Assert.AreEqual(new Vector3Int(1, 0, 0), sim.State.playerHead.position);
            Assert.AreEqual(Axis6.PosX, sim.State.playerHead.heading);
        }

        // ── 180° reversal blocked ────────────────────────────────────

        [Test]
        public void SetIntent180Reversal_IntentIgnored()
        {
            var sim = TestFixtures.OriginSimulation(); // heading PosX

            sim.SetPlayerIntent(Axis6.NegX); // 180° reversal — should be ignored
            sim.Tick();

            Assert.AreEqual(new Vector3Int(1, 0, 0), sim.State.playerHead.position);
            Assert.AreEqual(Axis6.PosX, sim.State.playerHead.heading);
        }

        [TestCase(Axis6.PosX, Axis6.NegX)]
        [TestCase(Axis6.NegX, Axis6.PosX)]
        [TestCase(Axis6.PosY, Axis6.NegY)]
        [TestCase(Axis6.NegY, Axis6.PosY)]
        [TestCase(Axis6.PosZ, Axis6.NegZ)]
        [TestCase(Axis6.NegZ, Axis6.PosZ)]
        public void SetIntent180Reversal_AllAxes_Ignored(Axis6 heading, Axis6 opposite)
        {
            var sim = new TickSimulation(Vector3Int.zero, heading);

            sim.SetPlayerIntent(opposite);
            sim.Tick();

            Assert.AreEqual(heading, sim.State.playerHead.heading, "Heading should not change on 180° reversal");
        }

        // ── State snapshot accuracy ───────────────────────────────────

        [Test]
        public void StateSnapshot_PositionAndHeadingMatchAfterEachTick()
        {
            var snapshots = SimulationRunner.RunCapturing(
                TestFixtures.OriginSimulation(),
                ticks: 5,
                inputs: new[] { new SimulationRunner.TickInput(2, Axis6.PosZ) });

            // Tick 1: (1,0,0), heading PosX
            Assert.AreEqual(new Vector3Int(1, 0, 0), snapshots[0].playerHead.position);
            Assert.AreEqual(Axis6.PosX, snapshots[0].playerHead.heading);

            // Tick 2: intent applied → heading PosZ → (1,0,1)
            Assert.AreEqual(new Vector3Int(1, 0, 1), snapshots[1].playerHead.position);
            Assert.AreEqual(Axis6.PosZ, snapshots[1].playerHead.heading);

            // Tick 3-5: continue along +Z
            for (int i = 2; i < 5; i++)
                Assert.AreEqual(new Vector3Int(1, 0, i), snapshots[i].playerHead.position);
        }

        [Test]
        public void RunCapturing_SnapshotsAreIndependentClones()
        {
            var snapshots = SimulationRunner.RunCapturing(TestFixtures.OriginSimulation(), ticks: 5);

            // Each snapshot should have a different position — if they were aliased, all would be (5,0,0).
            Assert.AreEqual(new Vector3Int(1, 0, 0), snapshots[0].playerHead.position);
            Assert.AreEqual(new Vector3Int(5, 0, 0), snapshots[4].playerHead.position);
        }

        // ── Serialization round-trip with head state ──────────────────

        [Test]
        public void GameStateWithHead_RoundTrips()
        {
            var sim = TestFixtures.OriginSimulation();
            sim.SetPlayerIntent(Axis6.PosZ);
            sim.Tick();
            sim.Tick();

            var json = JsonUtility.ToJson(sim.State);
            var restored = JsonUtility.FromJson<GameState>(json);

            Assert.AreEqual(sim.State.tick, restored.tick);
            Assert.AreEqual(sim.State.playerHead.position, restored.playerHead.position);
            Assert.AreEqual(sim.State.playerHead.heading, restored.playerHead.heading);
        }

        [Test]
        public void GameStateClone_IsDeepCopy()
        {
            var sim = TestFixtures.OriginSimulation();
            sim.Tick();

            var clone = sim.State.Clone();
            Assert.AreEqual(sim.State.tick, clone.tick);
            Assert.AreEqual(sim.State.playerHead.position, clone.playerHead.position);

            // Mutate original — clone should be unaffected
            sim.State.tick = 999;
            Assert.AreEqual(1, clone.tick, "Clone tick should not change when original is mutated");
        }
    }
}
