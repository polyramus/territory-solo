using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// A single snake head that advances one cell per tick along its current heading.
    /// Heading changes are queued via SetIntent and applied at the start of the next
    /// Advance() — this gives a clean turn-at-tick-boundary model (D10).
    /// 180° reversals are silently ignored (a snake cannot reverse into itself).
    /// </summary>
    public sealed class SnakeHead
    {
        private Axis6? _pendingIntent;

        public Vector3Int Position { get; private set; }
        public Axis6 Heading { get; private set; }

        public SnakeHead(Vector3Int startPosition, Axis6 startHeading)
        {
            Position = startPosition;
            Heading = startHeading;
        }

        /// <summary>Queue a direction change. 180° reversals are silently ignored.</summary>
        public void SetIntent(Axis6 direction)
        {
            if (!Axis6Extensions.IsOpposite(direction, Heading))
                _pendingIntent = direction;
        }

        /// <summary>Apply pending intent (if any), then advance one cell along current heading.</summary>
        public void Advance()
        {
            if (_pendingIntent.HasValue)
            {
                Heading = _pendingIntent.Value;
                _pendingIntent = null;
            }

            Position += Axis6Extensions.ToVector(Heading);
        }
    }
}
