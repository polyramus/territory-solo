// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Presentation
{
    /// <summary>
    /// Wraps MRUK scene understanding as an <see cref="IPlayVolumeSource"/>.
    /// Loads room data from the device and exposes the floor center for anchoring.
    /// </summary>
    public sealed class MRUKPlayVolumeSource : IPlayVolumeSource, IDisposable
    {
        private bool _disposed;

        /// <inheritdoc />
        public System.Nullable<Vector3> FloorCenter { get; private set; }

        /// <inheritdoc />
        public bool IsReady => FloorCenter.HasValue;

        private readonly UnityAction<MRUKRoom> _onSceneLoaded;

        public MRUKPlayVolumeSource()
        {
            _onSceneLoaded = OnSceneLoaded;
            if (MRUK.Instance != null)
                MRUK.Instance.SceneLoadedEvent.AddListener(_onSceneLoaded);
        }

        /// <summary>Trigger MRUK to load scene understanding from the device.</summary>
        public void LoadFromDevice()
        {
            if (MRUK.Instance != null)
                MRUK.Instance.LoadSceneFromDevice();
        }

        private void OnSceneLoaded(MRUKRoom room)
        {
            if (room?.FloorAnchor != null)
                FloorCenter = room.FloorAnchor.transform.position;
        }

        public void Dispose()
        {
            if (_disposed || MRUK.Instance == null) return;
            MRUK.Instance.SceneLoadedEvent.RemoveListener(_onSceneLoaded);
            _disposed = true;
        }
    }
}
