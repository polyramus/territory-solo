using UnityEngine;

namespace Game.Core.Tests
{
    /// <summary>Factory methods for creating TickSimulation instances with known starting conditions.</summary>
    public static class TestFixtures
    {
        /// <summary>Create a simulation with the player at (1,1,1) heading +X. Default fixture for most tests.</summary>
        public static TickSimulation DefaultSimulation() =>
            new TickSimulation(
                startPos: new Vector3Int(1, 1, 1),
                startHeading: Axis6.PosX);

        /// <summary>Create a simulation with the player at origin heading +X. Useful for boundary tests.</summary>
        public static TickSimulation OriginSimulation() =>
            new TickSimulation(
                startPos: Vector3Int.zero,
                startHeading: Axis6.PosX);

        // TODO M3: Add AI-snake fixtures (aiStart, aiHeading) and HeadOnCollisionSimulation
    }
}
