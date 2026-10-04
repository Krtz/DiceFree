using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;

namespace DiceFree.EditorTools
{
    /// <summary>Editor-only import, presentation, and player-compatibility proof for the Cornberg field sword.</summary>
    internal static class CornbergSwordPipelineValidation
    {
        private const string ModelPath = "Assets/_DiceFree/Art/Weapons/CornbergFieldSword/Models/CornbergFieldSword.fbx";
        private const string MaterialRoot = "Assets/_DiceFree/Art/Weapons/CornbergFieldSword/Materials";
        private const string PrefabPath = "Assets/_DiceFree/Art/Weapons/CornbergFieldSword/Prefabs/CornbergFieldSwordPresentation.prefab";
        private const string ScenePath = "Assets/_DiceFree/Art/Validation/Scenes/CornbergSwordPipelinePreview.unity";
        private const string PreviewPath = "Assets/_DiceFree/Art/Validation/Previews/CornbergSword_Unity.png";
        private const string ArtRoot = "Assets/_DiceFree/Art";
        private const string SourceRoot = "SourceArt/Weapons/CornbergFieldSword";
        private static object lastDetails;

        [CliCommand("dicefree.art.sword.prepare", "Import the Cornberg sword and create its URP presentation prefab and isolated preview scene.", Tags = new[] { "art", "dicefree/art" })]
        private static object Prepare() => Run("sword-pipeline-prepare", "DICEFRE_SWORD_PIPELINE_PREPARED", PrepareAssets);

        [CliCommand("dicefree.art.sword.validate", "Validate the Cornberg sword source import, URP prefab boundary, and dedicated preview scene.", Tags = new[] { "tests", "dicefree/art" })]
        private static object Validate() => Run("sword-pipeline", "DICEFRE_SWORD_PIPELINE_OK", ValidateAssets);

        [CliCommand("dicefree.art.sword.capture-preview", "Render the dedicated Cornberg sword preview camera to its checked-in PNG.", Tags = new[] { "art", "capture", "dicefree/art" })]
        private static object CapturePreview() => Run("sword-preview-capture", "DICEFRE_SWORD_PREVIEW_OK", CapturePreviewAsset);

        [CliCommand("dicefree.art.sword.build-windows", "Build the isolated sword preview as a Windows Development player without changing gameplay scenes or build settings.", Tags = new[] { "builds", "dicefree/art" })]
        private static object BuildWindows() => BuildWindowsPlayer();

        private static object Run(string name, string marker, Action action)
        {
            var timer = Stopwatch.StartNew();
            string error = null;
            lastDetails = null;
            try
            {
                action();
                UnityEngine.Debug.Log(marker);
            }
            catch (Exception exception)
            {
                error = exception.ToString();
                UnityEngine.Debug.LogError("DICEFRE_SWORD_PIPELINE_FAIL\n" + error);
            }
            timer.Stop();
            bool success = error == null;
            return new
            {
                success,
                validation = name,
                finalMarker = success ? marker : "DICEFRE_SWORD_PIPELINE_FAIL",
                elapsedSeconds = timer.Elapsed.TotalSeconds,
                error,
                modelPath = ModelPath,
                prefabPath = PrefabPath,
                previewScenePath = ScenePath,
                details = lastDetails,
                testedGitSha = GitHead()
            };
        }

        private static void PrepareAssets()
        {
            EnsureFolder("Assets/_DiceFree/Art/Weapons/CornbergFieldSword");
            EnsureFolder(MaterialRoot);
            EnsureFolder("Assets/_DiceFree/Art/Weapons/CornbergFieldSword/Prefabs");
            EnsureFolder("Assets/_DiceFree/Art/Validation/Scenes");
            EnsureFolder("Assets/_DiceFree/Art/Validation/Previews");

            AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            Require(importer != null, "Unity could not create a ModelImporter for " + ModelPath);
            ConfigureImporter(importer);
            AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceSynchronousImport);

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            Require(model != null, "Imported FBX is not available as a GameObject.");
            ValidateSourceFiles();
            RequireExpectedImportedParts(model);
            FindUniqueTransform(model.transform, "Grip");
            FindUniqueTransform(model.transform, "Tip");
            Bounds modelBounds = MeasureBounds(model);
            UnityEngine.Debug.Log("DICEFRE_SWORD_IMPORTED_BOUNDS size=" + modelBounds.size + " min=" + modelBounds.min + " max=" + modelBounds.max);
            foreach (var filter in model.GetComponentsInChildren<MeshFilter>(true).Where(f => f.sharedMesh != null))
                UnityEngine.Debug.Log("DICEFRE_SWORD_PART name=" + filter.name + " position=" + filter.transform.localPosition + " localBounds=" + filter.sharedMesh.bounds + " worldBounds=" + filter.GetComponent<Renderer>().bounds);

            var steel = CreateMaterial("Cornberg_Practical_Steel", new Color(0.26f, 0.36f, 0.40f), 0.52f, 0.36f);
            var edge = CreateMaterial("Cornberg_Polished_Edge", new Color(0.48f, 0.58f, 0.61f), 0.48f, 0.32f);
            var forged = CreateMaterial("Cornberg_Warm_Forged_Iron", new Color(0.48f, 0.25f, 0.11f), 0.30f, 0.38f);
            var leather = CreateMaterial("Cornberg_Dark_Grip_Leather", new Color(0.17f, 0.075f, 0.04f), 0.02f, 0.70f);

            var root = new GameObject("CornbergFieldSwordPresentation");
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
            visual.name = "CornbergFieldSword_Visual";
            visual.transform.SetParent(root.transform, false);
            // Preserve the FBX root correction produced by the importer; it maps
            // the DCC up-axis into the prefab's declared local +Z sword direction.

            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                string part = renderer.gameObject.name;
                if (part.IndexOf("Blade", StringComparison.OrdinalIgnoreCase) >= 0)
                    renderer.sharedMaterials = renderer.sharedMaterials.Length > 1 ? new[] { steel, edge } : new[] { steel };
                else if (part.IndexOf("Grip", StringComparison.OrdinalIgnoreCase) >= 0)
                    renderer.sharedMaterials = new[] { leather };
                else if (part.IndexOf("Hilt", StringComparison.OrdinalIgnoreCase) >= 0 || part.IndexOf("Pommel", StringComparison.OrdinalIgnoreCase) >= 0)
                    renderer.sharedMaterials = new[] { forged };
                else
                    renderer.sharedMaterials = Enumerable.Repeat(steel, Math.Max(1, renderer.sharedMaterials.Length)).ToArray();
            }

            CopyImportedAnchorToWrapper(root.transform, visual.transform, "Grip");
            CopyImportedAnchorToWrapper(root.transform, visual.transform, "Tip");

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            CreatePreviewScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ValidateAssets()
        {
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            Require(importer != null, "Sword FBX importer is missing.");
            Require(!importer.importAnimation, "Sword FBX unexpectedly imports animation.");
            Require(importer.materialImportMode == ModelImporterMaterialImportMode.None, "Sword FBX unexpectedly imports materials; presentation materials must be authored in Unity.");
            Require(Mathf.Approximately(importer.globalScale, 1f), "Sword FBX global import scale must remain 1.");
            Require(!importer.importCameras && !importer.importLights && !importer.importBlendShapes,
                "Rigid sword import must exclude cameras, lights and blend shapes.");
            Require(!importer.addCollider, "Sword FBX importer must not generate a collider.");
            AnimationClip[] importedClips = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().ToArray();
            Require(importedClips.Length == 0,
                "Rigid sword FBX unexpectedly contains imported animation clips.");

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            Require(model != null, "Sword FBX model asset is missing.");
            ValidateSourceFiles();
            RequireExpectedImportedParts(model);
            Bounds bounds = MeasureBounds(model);
            UnityEngine.Debug.Log("DICEFRE_SWORD_BOUNDS size=" + bounds.size + " min=" + bounds.min + " max=" + bounds.max);
            Require(bounds.size.z >= 0.9f && bounds.size.z <= 1.25f, "Expected roughly one metre of sword along local +Z; got bounds " + bounds.size + ". Check Blender FBX axis conversion.");
            Require(bounds.size.z > bounds.size.y * 2.5f, "Sword length is not aligned with local +Z.");
            Require(bounds.max.z > 0.85f && bounds.min.z > -0.35f, "Sword pivot is not at the grip with its blade extending toward +Z.");

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Require(prefab != null, "Sword presentation prefab is missing; run dicefree.art.sword.prepare first.");
            Require(prefab.transform.localPosition == Vector3.zero && prefab.transform.localRotation == Quaternion.identity && prefab.transform.localScale == Vector3.one,
                "Presentation prefab root transform must remain identity at the grip pivot.");
            var visualRoot = prefab.transform.Find("CornbergFieldSword_Visual");
            Require(visualRoot != null && Quaternion.Angle(visualRoot.localRotation, model.transform.localRotation) < 0.1f,
                "Presentation prefab must preserve the FBX importer root rotation that maps the DCC axis to Unity +Z.");
            Transform importedGrip = FindUniqueTransform(visualRoot, "Grip");
            Transform importedTip = FindUniqueTransform(visualRoot, "Tip");
            Transform wrapperGrip = prefab.transform.Find("Grip");
            Transform wrapperTip = prefab.transform.Find("Tip");
            Require(wrapperGrip != null && wrapperTip != null, "Prefab must expose pass-through Grip and Tip aliases.");
            Vector3 importedGripRoot = prefab.transform.InverseTransformPoint(importedGrip.position);
            Vector3 importedTipRoot = prefab.transform.InverseTransformPoint(importedTip.position);
            Vector3 wrapperGripRoot = prefab.transform.InverseTransformPoint(wrapperGrip.position);
            Vector3 wrapperTipRoot = prefab.transform.InverseTransformPoint(wrapperTip.position);
            Require(Vector3.Distance(importedGripRoot, wrapperGripRoot) <= 0.001f && Vector3.Distance(importedTipRoot, wrapperTipRoot) <= 0.001f,
                "Presentation aliases must match the FBX-authored semantic anchors.");
            Require(importedGripRoot.magnitude <= 0.01f, "Imported FBX Grip must match the root pivot; got " + importedGripRoot + ".");
            Require(Vector3.Distance(importedTipRoot, new Vector3(0f, 0f, 0.905f)) <= 0.015f,
                "Imported FBX Tip must be about 0.905 m forward along local +Z; got " + importedTipRoot + ".");
            Require(Vector3.Dot(importedTipRoot - importedGripRoot, Vector3.forward) > 0.90f,
                "Imported FBX Tip must lie forward of Grip along local +Z.");
            Require(prefab.GetComponentInChildren<Collider>(true) == null, "Presentation prefab must not add a gameplay hit collider.");
            Require(prefab.GetComponentInChildren<Animator>(true) == null && prefab.GetComponentInChildren<Animation>(true) == null,
                "Rigid sword presentation unexpectedly contains an animation component.");
            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            Require(renderers.Length >= 3, "Expected blade, hilt and grip renderers in the prefab.");
            foreach (var renderer in renderers)
                foreach (var material in renderer.sharedMaterials)
                    Require(material != null && material.shader != null && material.shader.name == "Universal Render Pipeline/Lit",
                        "All sword parts need explicit URP/Lit materials; bad material on " + renderer.name);
            foreach (var material in renderers.SelectMany(renderer => renderer.sharedMaterials).Where(material => material != null).Distinct())
                Require(material.HasProperty("_Cull") && Mathf.Approximately(material.GetFloat("_Cull"), (float)UnityEngine.Rendering.CullMode.Back),
                    "Sword materials must use standard single-sided URP backface culling: " + material.name);

            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Require(scene != null, "Dedicated sword presentation preview scene is missing.");
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { ArtRoot });
            Require(sceneGuids.Any(guid => AssetDatabase.GUIDToAssetPath(guid) == ScenePath), "Sword preview scene is not under the art validation boundary.");
            var openedScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var roots = openedScene.GetRootGameObjects();
                Require(roots.Any(go => go.name == "CornbergSwordPreviewCamera" && go.GetComponent<Camera>() != null), "Preview scene is missing its camera.");
                Require(roots.Any(go => go.name == "CornbergFieldSwordPresentation"), "Preview scene is missing the sword prefab instance.");
                var scaleReference = roots.FirstOrDefault(go => go.name == "ScaleReference_1p8m_NotCharacter");
                Require(scaleReference != null && Mathf.Abs(scaleReference.transform.localScale.z - 1.8f) < 0.01f,
                    "Preview scene must include the clearly named 1.8 m non-character scale reference.");
            }
            finally { EditorSceneManager.CloseScene(openedScene, true); }

            var filters = model.GetComponentsInChildren<MeshFilter>(true).Where(filter => filter.sharedMesh != null).ToArray();
            int vertices = filters.Sum(filter => filter.sharedMesh.vertexCount);
            int triangles = filters.Sum(filter => filter.sharedMesh.triangles.Length / 3);
            int uniqueMaterials = prefab.GetComponentsInChildren<Renderer>(true)
                .SelectMany(renderer => renderer.sharedMaterials).Where(material => material != null)
                .Distinct().Count();
            Require(filters.Length > 0 && vertices > 0 && triangles > 0, "Imported sword mesh counts must be nonzero.");
            Require(filters.Length <= 64 && vertices <= 1000000 && triangles <= 1000000,
                "Imported sword mesh counts are implausibly large for a single rigid prop; this is a gross-import sanity guard, not a content budget.");
            foreach (var filter in filters)
            {
                Vector3[] normals = filter.sharedMesh.normals;
                Require(normals.Length == filter.sharedMesh.vertexCount && normals.All(normal => normal.sqrMagnitude > 0.000001f),
                    "Mesh normals are missing or degenerate on " + filter.name + ". Keep normal backface culling enabled and fix the source geometry.");
            }
            Require(uniqueMaterials > 0, "Sword presentation has no assigned materials.");
            lastDetails = new
            {
                dimensionsMeters = new { x = bounds.size.x, y = bounds.size.y, z = bounds.size.z },
                meshFilterCount = filters.Length,
                vertexCount = vertices,
                triangleCount = triangles,
                uniqueMaterialCount = uniqueMaterials,
                sourceGripBlender = new[] { 0f, 0f, 0f },
                sourceTipBlender = new[] { 0f, 0.905f, 0f },
                importedGripUnity = new[] { importedGripRoot.x, importedGripRoot.y, importedGripRoot.z },
                importedTipUnity = new[] { importedTipRoot.x, importedTipRoot.y, importedTipRoot.z },
                wrapperGripUnity = new[] { wrapperGripRoot.x, wrapperGripRoot.y, wrapperGripRoot.z },
                wrapperTipUnity = new[] { wrapperTipRoot.x, wrapperTipRoot.y, wrapperTipRoot.z },
                importedVisualRootEulerDegrees = new[] { visualRoot.localRotation.eulerAngles.x, visualRoot.localRotation.eulerAngles.y, visualRoot.localRotation.eulerAngles.z },
                importer = new { globalScale = importer.globalScale, animation = importer.importAnimation, animationClipCount = importedClips.Length, normals = importer.importNormals.ToString(), camera = importer.importCameras, lights = importer.importLights, blendShapes = importer.importBlendShapes, collider = importer.addCollider, embeddedMaterials = importer.materialImportMode.ToString() },
                materialCullMode = "Back",
                meshPartNames = filters.Select(filter => filter.name).ToArray()
            };
            UnityEngine.Debug.Log("DICEFRE_SWORD_MESH_STATS vertices=" + vertices + " triangles=" + triangles + " meshFilters=" + filters.Length + " uniqueMaterials=" + uniqueMaterials + " dimensions=" + bounds.size);
            UnityEngine.Debug.Log("DICEFRE_SWORD_ANCHORS importedGrip=" + importedGripRoot + " importedTip=" + importedTipRoot + " wrapperGrip=" + wrapperGripRoot + " wrapperTip=" + wrapperTipRoot);
        }

        private static void CreatePreviewScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("CornbergSwordPreviewCamera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 0.95f;
            camera.backgroundColor = new Color(0.13f, 0.16f, 0.19f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.transform.position = new Vector3(2.25f, 3.9f, -0.1f);
            camera.transform.LookAt(new Vector3(0.08f, 0.28f, 0.48f));
            camera.tag = "MainCamera";

            var lightObject = new GameObject("CornbergSwordPreviewKey");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.transform.rotation = Quaternion.Euler(40f, -35f, 0f);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "PreviewGround_NotGameplay";
            floor.transform.position = new Vector3(0f, -0.08f, 0.45f);
            floor.transform.localScale = new Vector3(1.7f, 1f, 1.7f);
            var floorRenderer = floor.GetComponent<Renderer>();
            floorRenderer.sharedMaterial = CreateMaterial("Cornberg_Sword_Preview_Slate", new Color(0.18f, 0.22f, 0.25f), 0f, 0.85f);
            UnityEngine.Object.DestroyImmediate(floor.GetComponent<Collider>());

            var reference = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            reference.name = "ScaleReference_1p8m_NotCharacter";
            reference.transform.position = new Vector3(0.48f, -0.035f, 0.42f);
            reference.transform.localScale = new Vector3(0.025f, 0.025f, 1.8f);
            reference.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Cornberg_Sword_Preview_Scale", new Color(0.38f, 0.42f, 0.44f), 0f, 0.9f);
            UnityEngine.Object.DestroyImmediate(reference.GetComponent<Collider>());

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var sword = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            sword.name = "CornbergFieldSwordPresentation";
            sword.transform.position = new Vector3(-0.03f, 0.015f, 0.42f);
            sword.transform.rotation = Quaternion.identity;
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }

        private static void CapturePreviewAsset()
        {
            ValidateAssets();
            Scene previewScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            RenderTexture target = null;
            Texture2D image = null;
            Camera camera = null;
            RenderTexture priorTarget = null;
            RenderTexture priorActive = RenderTexture.active;
            try
            {
                camera = previewScene.GetRootGameObjects()
                    .Select(root => root.GetComponent<Camera>())
                    .FirstOrDefault(candidate => candidate != null);
                Require(camera != null, "Sword preview scene has no camera to capture.");
                const int width = 1280;
                const int height = 800;
                target = new RenderTexture(width, height, 24);
                priorTarget = camera.targetTexture;
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image = new Texture2D(width, height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();

                string outputPath = ProjectAssetFullPath(PreviewPath);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                File.WriteAllBytes(outputPath, image.EncodeToPNG());
                AssetDatabase.ImportAsset(PreviewPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.SaveAssets();
                lastDetails = new { previewPath = PreviewPath, width, height, captureMethod = "dedicated preview-scene Camera.Render into a fixed-size RenderTexture" };
            }
            finally
            {
                if (camera != null) camera.targetTexture = priorTarget;
                RenderTexture.active = priorActive;
                if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
                if (image != null) UnityEngine.Object.DestroyImmediate(image);
                EditorSceneManager.CloseScene(previewScene, true);
            }
        }

        private static void ConfigureImporter(ModelImporter importer)
        {
            bool changed = false;
            if (!Mathf.Approximately(importer.globalScale, 1f)) { importer.globalScale = 1f; changed = true; }
            if (importer.importAnimation) { importer.importAnimation = false; changed = true; }
            if (importer.materialImportMode != ModelImporterMaterialImportMode.None) { importer.materialImportMode = ModelImporterMaterialImportMode.None; changed = true; }
            if (importer.importCameras) { importer.importCameras = false; changed = true; }
            if (importer.importLights) { importer.importLights = false; changed = true; }
            if (importer.importBlendShapes) { importer.importBlendShapes = false; changed = true; }
            if (importer.addCollider) { importer.addCollider = false; changed = true; }
            if (importer.importNormals != ModelImporterNormals.Import) { importer.importNormals = ModelImporterNormals.Import; changed = true; }
            if (changed) importer.SaveAndReimport();
        }

        private static void ValidateSourceFiles()
        {
            Require(File.Exists(ProjectAssetFullPath(SourceRoot + "/CornbergFieldSword.blend")), "Editable Blender source is missing or misnamed.");
            Require(File.Exists(ProjectAssetFullPath(SourceRoot + "/build_cornberg_field_sword.py")), "Sword source/export script is missing or misnamed.");
            Require(File.Exists(ProjectAssetFullPath(ModelPath)), "Unity FBX export is missing.");
            Require(Path.GetFileNameWithoutExtension(ModelPath) == "CornbergFieldSword", "FBX name must match CornbergFieldSword source identity.");
        }

        private static void RequireExpectedImportedParts(GameObject model)
        {
            string[] names = model.GetComponentsInChildren<MeshFilter>(true).Where(filter => filter.sharedMesh != null)
                .Select(filter => filter.name).ToArray();
            string[] requiredParts = { "CornbergFieldSword_Blade", "CornbergFieldSword_Hilt", "CornbergFieldSword_Grip" };
            Require(names.Length == requiredParts.Length && requiredParts.All(part => names.Count(name => name == part) == 1),
                "Imported sword must preserve exactly one each of its Blade, Hilt and Grip mesh parts. Found: " + string.Join(", ", names));
        }

        private static Transform FindUniqueTransform(Transform root, string name)
        {
            Transform[] matches = root.GetComponentsInChildren<Transform>(true).Where(transform => transform.name == name).ToArray();
            Require(matches.Length == 1, "Expected exactly one imported " + name + " anchor under " + root.name + "; found " + matches.Length + ".");
            return matches[0];
        }

        private static void CopyImportedAnchorToWrapper(Transform wrapperRoot, Transform importedVisual, string name)
        {
            Transform imported = FindUniqueTransform(importedVisual, name);
            var alias = new GameObject(name);
            alias.transform.SetParent(wrapperRoot, false);
            alias.transform.localPosition = wrapperRoot.InverseTransformPoint(imported.position);
            alias.transform.localRotation = Quaternion.Inverse(wrapperRoot.rotation) * imported.rotation;
            alias.transform.localScale = Vector3.one;
        }

        private static string ProjectAssetFullPath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static object BuildWindowsPlayer()
        {
            var timer = Stopwatch.StartNew();
            string output = "Builds/ArtValidation/CornbergSwordPipelinePreview.exe";
            try
            {
                ValidateAssets();
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                var options = new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = output,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development
                };
                BuildReport report = BuildPipeline.BuildPlayer(options);
                timer.Stop();
                long outputBytes = Directory.Exists(Path.GetDirectoryName(output))
                    ? Directory.GetFiles(Path.GetDirectoryName(output), "*", SearchOption.AllDirectories).Sum(path => new FileInfo(path).Length)
                    : 0L;
                bool success = report.summary.result == BuildResult.Succeeded;
                if (success) UnityEngine.Debug.Log("DICEFRE_SWORD_PLAYER_BUILD_OK");
                else UnityEngine.Debug.LogError("DICEFRE_SWORD_PLAYER_BUILD_FAIL: " + report.summary.result + " errors=" + report.summary.totalErrors);
                return new
                {
                    success,
                    validation = "sword-windows-development-build",
                    finalMarker = success ? "DICEFRE_SWORD_PLAYER_BUILD_OK" : "DICEFRE_SWORD_PLAYER_BUILD_FAIL",
                    elapsedSeconds = timer.Elapsed.TotalSeconds,
                    error = success ? null : "Build result " + report.summary.result + ", errors=" + report.summary.totalErrors,
                    buildTarget = "StandaloneWindows64",
                    developmentBuild = true,
                    outputPath = output,
                    executableBytes = File.Exists(output) ? new FileInfo(output).Length : 0L,
                    totalOutputBytes = outputBytes,
                    warnings = report.summary.totalWarnings,
                    errors = report.summary.totalErrors,
                    details = lastDetails,
                    testedGitSha = GitHead()
                };
            }
            catch (Exception exception)
            {
                timer.Stop();
                UnityEngine.Debug.LogError("DICEFRE_SWORD_PLAYER_BUILD_FAIL\n" + exception);
                return new
                {
                    success = false,
                    validation = "sword-windows-development-build",
                    finalMarker = "DICEFRE_SWORD_PLAYER_BUILD_FAIL",
                    elapsedSeconds = timer.Elapsed.TotalSeconds,
                    error = exception.ToString(),
                    buildTarget = "StandaloneWindows64",
                    developmentBuild = true,
                    outputPath = output,
                    testedGitSha = GitHead()
                };
            }
        }

        private static Material CreateMaterial(string name, Color color, float metallic, float smoothness)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                Require(shader != null, "Universal Render Pipeline/Lit shader is unavailable.");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = Shader.Find("Universal Render Pipeline/Lit");
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            // Keep standard URP backface culling. Missing/inverted faces must be
            // fixed in the source mesh rather than hidden by double-sided shading.
            if (material.HasProperty("_Cull")) material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Back);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Bounds MeasureBounds(GameObject model)
        {
            var filters = model.GetComponentsInChildren<MeshFilter>(true).Where(f => f.sharedMesh != null).ToArray();
            Require(filters.Length > 0, "Imported sword has no mesh filters.");
            bool hasBounds = false;
            Bounds aggregate = default;
            foreach (var filter in filters)
            {
                var meshBounds = filter.sharedMesh.bounds;
                Matrix4x4 matrix = filter.transform.localToWorldMatrix;
                for (int i = 0; i < 8; i++)
                {
                    var corner = new Vector3((i & 1) == 0 ? meshBounds.min.x : meshBounds.max.x,
                        (i & 2) == 0 ? meshBounds.min.y : meshBounds.max.y,
                        (i & 4) == 0 ? meshBounds.min.z : meshBounds.max.z);
                    Vector3 point = matrix.MultiplyPoint3x4(corner);
                    if (!hasBounds) { aggregate = new Bounds(point, Vector3.zero); hasBounds = true; }
                    else aggregate.Encapsulate(point);
                }
            }
            return aggregate;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static string GitHead()
        {
            try
            {
                var process = new Process { StartInfo = new ProcessStartInfo("git", "rev-parse HEAD") { UseShellExecute = false, RedirectStandardOutput = true, CreateNoWindow = true } };
                process.Start();
                string value = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(3000);
                return value;
            }
            catch { return string.Empty; }
        }
    }
}
