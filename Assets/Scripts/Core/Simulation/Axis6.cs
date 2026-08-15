using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Six cardinal directions on the integer grid. Enum layout is deliberate:
    /// each axis occupies a pair of consecutive values so that <c>(int)a / 2 == (int)b / 2</c>
    /// identifies opposite directions on the same axis.
    /// </summary>
    public enum Axis6 { PosX, NegX, PosY, NegY, PosZ, NegZ }

    /// <summary>Static helpers for <see cref="Axis6"/>.</summary>
    public static class Axis6Extensions
    {
        /// <summary>Convert an axis to its unit step in integer cell coordinates.</summary>
        public static Vector3Int ToVector(Axis6 axis) => axis switch
        {
            Axis6.PosX => Vector3Int.right,
            Axis6.NegX => Vector3Int.left,
            Axis6.PosY => Vector3Int.up,
            Axis6.NegY => Vector3Int.down,
            Axis6.PosZ => new Vector3Int(0, 0, 1),
            Axis6.NegZ => new Vector3Int(0, 0, -1),
            _ => throw new System.ArgumentOutOfRangeException(nameof(axis), $"Unknown axis: {axis}")
        };

        /// <summary>True when both axes are on the same dimension but opposite signs (e.g. PosX / NegX).</summary>
        public static bool IsOpposite(Axis6 a, Axis6 b) =>
            (int)a / 2 == (int)b / 2 && a != b;
    }
}
