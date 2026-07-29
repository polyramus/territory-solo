// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.CoreUtils;

namespace Game.Editor
{
    /// <summary>
    /// Editor tool that generates the Main scene with full XR passthrough setup,
    /// MRUK anchoring, play volume root, and boundary geometry.
    /// Run from: Tools → Voxel Territory → Generate Main Scene
    /// </summary>
    public static class CreateMainScene
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("Tools/Voxel Territory/Generate Main Scene")]
        public static void Generate()
        {
            // 1. Create or clear the scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. XR Origin (creates camera rig hierarchy at runtime)
            var xrOrigin = new GameObject("XR Origin");
            xrOrigin.AddComponent<XROrigin>();

            // 3. Camera Offset → Main Camera with passthrough components
            //    XROrigin creates this at runtime, but we build it in-editor for visibility.
            var cameraOffset = new GameObject("Camera Offset");
            cameraOffset.transform.SetParent(xrOrigin.transform);

            var mainCamera = new GameObject("Main Camera");
            mainCamera.transform.SetParent(cameraOffset.transform);
            var camera = mainCamera.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0, 0, 0, 0); // transparent — passthrough shows through
            mainCamera.tag = "MainCamera";

            // AR Foundation passthrough components (required for color passthrough on Quest)
            mainCamera.AddComponent<ARCameraManager>();
            var arBackground = mainCamera.AddComponent<ARCameraBackground>();

            // Assign the Meta OpenXR camera background material if available
            var metaMaterial = FindMetaPassthroughMaterial();
            if (metaMaterial != null)
                arBackground.material = metaMaterial;

            // 4. Controller placeholders — M1 replaces these with WorldAxisInput + Input System actions
            CreateControllerPlaceholder(xrOrigin, "Left Controller");
            CreateControllerPlaceholder(xrOrigin, "Right Controller");

            // 5. MRUK prefab — find and instantiate from package
            var mrukPrefab = FindMRUKPrefab();
            if (mrukPrefab != null)
            {
                Instantiate(mrukPrefab);
                Debug.Log($"MRUK prefab instantiated from: {AssetDatabase.GetAssetPath(mrukPrefab)}");
            }
            else
            {
                // Fallback placeholder — user may need to drag the MRUK prefab manually
                var mrukFallback = new GameObject("MRUK");
                Debug.LogWarning(
                    "MRUK prefab not found in package — created placeholder. " +
                    "Drag the MRUK prefab from com.meta.xr.mrutilitykit into this GameObject.",
                    mrukFallback);
            }

            // 6. Play Volume Root with boundary geometry (wireframe edges + tinted North face)
            var playVolumeRoot = new GameObject("Play Volume Root");
            CreateBoundaryGeometry(playVolumeRoot);

            // 7. Game Manager — holds PlayVolumeAnchor and (later in M1) GameLoop
            var gameManager = new GameObject("Game Manager");
            var anchor = gameManager.AddComponent<Game.Presentation.PlayVolumeAnchor>();
            anchor._playVolumeRoot = playVolumeRoot.transform;

            // 8. Save scene
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);

            // 9. Replace build settings with our Main scene (remove MRUK sample scenes)
            SetBuildScenes();

            Debug.Log($"Main scene generated at {ScenePath} and added to Build Settings.");
        }

        private static void CreateControllerPlaceholder(GameObject parent, string name)
        {
            // Empty placeholder — M1 adds WorldAxisInput with Input System actions.
            // With OpenXR + Input System, controllers are managed by XROrigin at runtime;
            // we don't need TrackedDeviceControl (legacy).
            var controller = new GameObject(name);
            controller.transform.SetParent(parent.transform);
        }

        private static void CreateBoundaryGeometry(GameObject parent)
        {
            const float size = 2f;
            var half = size / 2f;

            // 12 edges of a cube as (start, end) pairs relative to center
            var edgePairs = new Vector3[][]
            {
                // Bottom face (Y = -half) — X and Z axes
                new[] { new Vector3(-half, -half, -half), new Vector3( half, -half, -half) },
                new[] { new Vector3( half, -half, -half), new Vector3( half, -half,  half) },
                new[] { new Vector3( half, -half,  half), new Vector3(-half, -half,  half) },
                new[] { new Vector3(-half, -half,  half), new Vector3(-half, -half, -half) },

                // Top face (Y = +half) — X and Z axes
                new[] { new Vector3(-half,  half, -half), new Vector3( half,  half, -half) },
                new[] { new Vector3( half,  half, -half), new Vector3( half,  half,  half) },
                new[] { new Vector3( half,  half,  half), new Vector3(-half,  half,  half) },
                new[] { new Vector3(-half,  half,  half), new Vector3(-half,  half, -half) },

                // Vertical edges — Y axis (green tinted)
                new[] { new Vector3(-half, -half, -half), new Vector3(-half,  half, -half) },
                new[] { new Vector3( half, -half, -half), new Vector3( half,  half, -half) },
                new[] { new Vector3( half, -half,  half), new Vector3( half,  half,  half) },
                new[] { new Vector3(-half, -half,  half), new Vector3(-half,  half,  half) },
            };

            var unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlitShader == null)
                unlitShader = Shader.Find("Sprites/Default"); // fallback

            foreach (var pair in edgePairs)
            {
                var line = new GameObject("Edge");
                line.transform.SetParent(parent.transform);
                var lr = line.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.startWidth = 0.01f;
                lr.endWidth = 0.01f;
                lr.material = new Material(unlitShader);

                // Color vertical edges green (Y axis), horizontal cyan (X/Z axes)
                var isVertical = Mathf.Approximately(pair[0].x, pair[1].x) && Mathf.Approximately(pair[0].z, pair[1].z);
                var color = isVertical ? new Color(0.2f, 1f, 0.3f, 0.8f) : new Color(0.3f, 0.9f, 1f, 0.7f);

                lr.startColor = color;
                lr.endColor = color;
                lr.SetPosition(0, pair[0]);
                lr.SetPosition(1, pair[1]);
            }

            // Tinted face on -X boundary to mark "North" (world-frame legibility — required by D5)
            var northFace = new GameObject("North Face (-X)");
            northFace.transform.SetParent(parent.transform);
            var meshFilter = northFace.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateQuadMesh(size, size);
            var meshRenderer = northFace.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(unlitShader);
            meshRenderer.material.color = new Color(0.2f, 0.6f, 1f, 0.15f); // tinted blue, semi-transparent
            northFace.transform.localPosition = new Vector3(-half, 0, 0);
        }

        private static Mesh CreateQuadMesh(float width, float height)
        {
            var mesh = new Mesh();
            var halfW = width / 2f;
            var halfH = height / 2f;
            mesh.vertices = new Vector3[]
            {
                new(-halfW, -halfH, 0),
                new( halfW, -halfH, 0),
                new( halfW,  halfH, 0),
                new(-halfW,  halfH, 0)
            };
            mesh.uv = new Vector2[]
            {
                Vector2.zero, Vector2.right, Vector2.one, Vector2.up
            };
            mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();
            return mesh;
        }

        private static CameraBackgroundMaterial FindMetaPassthroughMaterial()
        {
            // Search for the Meta OpenXR camera background material in the package folder.
            // The material type is CameraBackgroundMaterial (AR Foundation).
            var guids = AssetDatabase.FindAssets("t:CameraBackgroundMaterial");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("meta-openxr"))
                    return AssetDatabase.LoadAssetAtPath<CameraBackgroundMaterial>(path);
            }

            // Fallback: search for any material with "Passthrough" or "Camera" in the name under meta-openxr
            guids = AssetDatabase.FindAssets("t:Material _metaopenxr");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("meta-openxr") && (path.Contains("Passthrough") || path.Contains("Camera")))
                    return AssetDatabase.LoadAssetAtPath<CameraBackgroundMaterial>(path);
            }

            Debug.LogWarning(
                "Meta OpenXR CameraBackgroundMaterial not found — passthrough may appear black. " +
                "Assign the material manually on ARCameraBackground in the Main scene.");
            return null;
        }

        private static GameObject FindMRUKPrefab()
        {
            // Search for the MRUK prefab in the com.meta.xr.mrutilitykit package folder
            var guids = AssetDatabase.FindAssets("t:Prefab MRUK");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("com.meta.xr.mrutilitykit"))
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            // Broader search — any prefab containing "MRUK" in the package
            guids = AssetDatabase.FindAssets("t:Prefab MRUK");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("com.meta.xr.mrutilitykit"))
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            Debug.LogWarning(
                "MRUK prefab not found — check that com.meta.xr.mrutilitykit is installed and imported. " +
                "Drag the MRUK prefab manually into the scene if needed.");
            return null;
        }

        private static void SetBuildScenes()
        {
            var scenes = new[] { new EditorBuildSettingsScene(ScenePath, enabled: true) };
            EditorBuildSettings.scenes = scenes;
        }
    }
}
