using System;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Serializable snapshot of the whole simulation. This is the seam a phone spectator
    /// or a remote peer attaches to later (D6) — it must stay plain data, no behaviour.
    /// </summary>
    [Serializable]
    public sealed class GameState
    {
        public int tick;
        public SnakeHeadState playerHead;
        // Expanded in M2 (occupied cells) and M3 (round status, AI head)

        /// <summary>Create a deep copy. Required for snapshot capture — storing references to the same mutable object is an aliasing trap.</summary>
        public GameState Clone() => new GameState { tick = tick, playerHead = playerHead };
    }

    /// <summary>Serializable value-type snapshot of a single snake head's state.</summary>
    [Serializable]
    public struct SnakeHeadState
    {
        public Vector3Int position;
        public Axis6 heading;
    }
}
