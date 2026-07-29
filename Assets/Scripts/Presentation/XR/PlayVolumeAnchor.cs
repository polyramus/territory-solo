// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Anchors the play volume to the real room using an <see cref="IPlayVolumeSource"/>.
    /// Positions the Play Volume Root at the floor center + half height once the source
    /// reports it is ready. The volume dimensions are configurable in the inspector.
    /// </summary>
    public sealed class PlayVolumeAnchor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _playVolumeRoot;

        [Header("Dimensions")]
        [Tooltip("Width of the play volume in meters (X axis).")]
        [SerializeField] private float _volumeWidth = 2f;

        [Tooltip("Height of the play volume in meters (Y axis).")]
        [SerializeField] private float _volumeHeight = 2f;

        [Tooltip("Depth of the play volume in meters (Z axis).")]
        [SerializeField] private float _volumeDepth = 2f;

        /// <summary>Play volume dimensions in world space.</summary>
        public Vector3 VolumeSize => new(_volumeWidth, _volumeHeight, _volumeDepth);

        /// <summary>The anchored play volume root transform (read-only).</summary>
        public Transform PlayVolumeRoot => _playVolumeRoot;

        private IPlayVolumeSource _source;
        private bool _anchored;

        private void Awake()
        {
            if (_playVolumeRoot == null)
                Debug.LogError($"{nameof(PlayVolumeAnchor)}: Play Volume Root is not assigned.", this);

            // Create the MRUK-backed source at runtime. In tests, inject a mock instead.
            _source = new MRUKPlayVolumeSource();
        }

        private void Start()
        {
            if (_source is MRUKPlayVolumeSource mruk)
                mruk.LoadFromDevice();
        }

        private void Update()
        {
            // Anchor once the source reports ready; subsequent updates are no-ops.
            if (!_anchored && _source?.IsReady == true)
            {
                ApplyAnchor(_source.FloorCenter.Value);
                _anchored = true;
            }
        }

        private void OnDestroy()
        {
            (_source as IDisposable)?.Dispose();
        }

        /// <summary>
        /// Called when the volume should be anchored. Can also be called manually
        /// (e.g., from a test or editor tool) with an explicit floor center.
        /// </summary>
        public void ApplyAnchor(Vector3 floorCenter)
        {
            if (_playVolumeRoot == null) return;

            _playVolumeRoot.position = new Vector3(
                floorCenter.x,
                floorCenter.y + _volumeHeight / 2f,
                floorCenter.z
            );
            _playVolumeRoot.rotation = Quaternion.identity;
            _anchored = true;
        }
    }
}
