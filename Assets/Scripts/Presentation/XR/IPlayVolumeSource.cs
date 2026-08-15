using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Abstraction over the room-scene understanding source that provides floor anchoring.
    /// PlayVolumeAnchor depends on this interface (not directly on MRUK) so it can be
    /// tested with a mock or JSON fixture in edit mode without a headset.
    /// </summary>
    public interface IPlayVolumeSource
    {
        /// <summary>World-space center of the detected floor, or null if unavailable.</summary>
        System.Nullable<Vector3> FloorCenter { get; }

        /// <summary>Whether the source has loaded room data and is ready to provide anchors.</summary>
        bool IsReady { get; }
    }
}
