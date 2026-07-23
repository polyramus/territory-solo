using System;

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
        // Expanded in M1 (snake head) and M2 (occupied cells)
    }
}
