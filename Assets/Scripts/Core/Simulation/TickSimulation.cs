using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Fixed-step simulation. Advancing time is <see cref="Tick"/> and nothing else —
    /// callers never drive the state by frame time, so two machines fed the same tick
    /// count reach identical state (D6).
    /// </summary>
    public sealed class TickSimulation
    {
        private readonly SnakeHead _playerHead;

        public GameState State { get; private set; }

        /// <summary>Parameterless constructor for backward compatibility with M0 tests.</summary>
        public TickSimulation()
        {
            State = new GameState();
            _playerHead = null;
        }

        /// <summary>Create a simulation with the player head at a known start position and heading.</summary>
        public TickSimulation(Vector3Int startPos, Axis6 startHeading)
        {
            _playerHead = new SnakeHead(startPos, startHeading);
            State = Snapshot();
        }

        /// <summary>Queue a direction change for the player head. 180° reversals are silently ignored.</summary>
        public void SetPlayerIntent(Axis6 direction) => _playerHead?.SetIntent(direction);

        public void Tick()
        {
            State.tick++;
            _playerHead?.Advance();
            if (_playerHead != null)
                State = Snapshot();
        }

        private GameState Snapshot() => new GameState
        {
            tick = State.tick,
            playerHead = new SnakeHeadState
            {
                position = _playerHead.Position,
                heading = _playerHead.Heading
            }
        };
    }
}
