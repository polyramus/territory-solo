using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Static helpers for driving a TickSimulation through scripted inputs.
    /// Lives in Game.Core (not test assembly) so Editor tooling can use it too.
    /// </summary>
    public static class SimulationRunner
    {
        /// <summary>A single input scheduled to be applied at a specific tick number.</summary>
        public readonly struct TickInput
        {
            public readonly int AtTick;
            public readonly Axis6 PlayerIntent;

            public TickInput(int atTick, Axis6 intent)
            {
                AtTick = atTick;
                PlayerIntent = intent;
            }
        }

        /// <summary>Run the simulation for up to maxTicks, applying scheduled inputs. Returns final state.</summary>
        public static GameState RunToCompletion(
            TickSimulation sim,
            IEnumerable<TickInput> inputs = null,
            int maxTicks = 10_000)
        {
            var queue = inputs != null
                ? new Queue<TickInput>(inputs)
                : new Queue<TickInput>();

            for (int t = 0; t < maxTicks; t++)
            {
                while (queue.Count > 0 && queue.Peek().AtTick == t)
                    sim.SetPlayerIntent(queue.Dequeue().PlayerIntent);

                sim.Tick();
                // TODO M3: early-exit when sim.State.roundStatus != RoundStatus.Playing
            }

            return sim.State;
        }

        /// <summary>Run N ticks, capturing a deep-copy snapshot after every tick. Use for per-tick assertions.</summary>
        public static List<GameState> RunCapturing(
            TickSimulation sim,
            int ticks,
            IEnumerable<TickInput> inputs = null)
        {
            var queue = inputs != null
                ? new Queue<TickInput>(inputs)
                : new Queue<TickInput>();

            var snapshots = new List<GameState>(ticks);

            for (int t = 0; t < ticks; t++)
            {
                while (queue.Count > 0 && queue.Peek().AtTick == t)
                    sim.SetPlayerIntent(queue.Dequeue().PlayerIntent);

                sim.Tick();
                // Clone to avoid aliasing — GameState is a class, storing references would give N copies of the final state.
                snapshots.Add(sim.State.Clone());
            }

            return snapshots;
        }
    }
}
