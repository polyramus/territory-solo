namespace Game.Core
{
    /// <summary>
    /// Fixed-step simulation. Advancing time is <see cref="Tick"/> and nothing else —
    /// callers never drive the state by frame time, so two machines fed the same tick
    /// count reach identical state (D6).
    /// </summary>
    public sealed class TickSimulation
    {
        public GameState State { get; } = new GameState();

        public void Tick()
        {
            State.tick++;
        }
    }
}
