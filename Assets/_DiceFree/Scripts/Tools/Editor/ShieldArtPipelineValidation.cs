using System;
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
    /// <summary>Editor-only proof for the Novice offhand attachment and shield presentation pipeline.</summary>
    internal static class ShieldArtPipelineValidation
    {
        private const string SourceRoot = "SourceArt/Weapons/CornbergRoundShield";
        private const string ModelPath = "Assets/_DiceFree/Art/Weapons/CornbergRoundShield/Models/CornbergRoundShield.fbx";
        private const string MaterialRoot = "Assets/_DiceFree/Art/Weapons/CornbergRoundShield/Materials";
        private const string PrefabPath = "Assets/_DiceFree/Art/Weapons/CornbergRoundShield/Prefabs/CornbergRoundShieldPresentation.prefab";
        private const string NovicePrefabPath = "Assets/_DiceFree/Art/Characters/Novice/Prefabs/NovicePresentation.prefab";
        private const string NoviceModelPath = "Assets/_DiceFree/Art/Characters/Novice/Models/Novice.fbx";
        private const string ScenePath = "Assets/_DiceFree/Art/Validation/Scenes/NoviceShieldPipelinePreview.unity";
        private const string PreviewPath = "Assets/_DiceFree/Art/Validation/Previews/NoviceShield_Unity.png";

        [CliCommand("dicefree.art.shield.prepare", "Import the Cornberg prototype shield and prepare the Novice offhand attachment fixture.", Tags = new[] { "art", "dicefree/art" })]
        private static object PrepareCommand() => Run("shield-prepare", "DICEFRE_SHIELD_PREPARED", PrepareAssets);

        [CliCommand("dicefree.art.shield.validate", "Validate the shield import, presentation prefab, and Novice LeftHandOffhand contract.", Tags = new[] { "tests", "dicefree/art" })]
        private static object ValidateCommand() => Run("shield-pipeline", "DICEFRE_SHIELD_PIPELINE_OK", ValidateAssets);

        [CliCommand("dicefree.art.shield.capture-preview", "Render the baseline and shield-fixture Novice poses to the checked-in preview.", Tags = new[] { "art", "capture", "dicefree/art" })]
        private static object CapturePreviewCommand() => Run("shield-preview-capture", "DICEFRE_SHIELD_PREVIEW_OK", CapturePreviewAsset);

        [CliCommand("dicefree.art.shield.build-windows", "Build the isolated Novice shield preview as a Windows x64 Development player.", Tags = new[] { "builds", "dicefree/art" })]
        private static object BuildWindowsCommand() => BuildWindowsPlayer();

        private static object Run(string name, string marker, Action action)
        {
            var timer = Stopwatch.StartNew();
            string error = null;
            try
            {
                action();
                UnityEngine.Debug.Log(marker);
            }
            catch (Exception exception)
            {
                error = exception.ToString();
                UnityEngine.Debug.LogError("DICEFRE_SHIELD_PIPELINE_FAIL\n" + error);
            }
            timer.Stop();
            return new
            {
                success = error == null,
                validation = name,
                finalMarker = error == null ? marker : "DICEFRE_SHIELD_PIPELINE_FAIL",
                elapsedSeconds = timer.Elapsed.TotalSeconds,
                error,
                modelPath = ModelPath,
                prefabPath = PrefabPath,
                previewScenePath = ScenePath,
                previewPath = PreviewPath,
                testedGitSha = GitHead()
            };
        }

        private static void PrepareAssets()
        {
            EnsureFolder("Assets/_DiceFree/Art/Weapons/CornbergRoundShield");
            EnsureFolder(MaterialRoot);
            EnsureFolder("Assets/_DiceFree/Art/Weapons/CornbergRoundShield/Models");
            EnsureFolder("Assets/_DiceFree/Art/Weapons/CornbergRoundShield/Prefabs");
            EnsureFolder("Assets/_DiceFree/Art/Validation/Scenes");
            EnsureFolder("Assets/_DiceFree/Art/Validation/Previews");

            AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            Require(importer != null, "Shield FBX ModelImporter is missing.");
            ConfigureImporter(importer);
            AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceSynchronousImport);

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            Require(model != null, "Shield FBX did not import as a model GameObject.");
            ValidateSourceFiles();
            RequireExpectedParts(model);
            FindUniqueTransform(model.transform, "Grip");
            FindUniqueTransform(model.transform, "ShieldFront");

            var wood = CreateMaterial("Cornberg_Shield_Warm_Oak", new Color(0.36f, 0.20f, 0.095f), 0f, 0.82f);
            var iron = CreateMaterial("Cornberg_Shield_Forged_Iron", new Color(0.25f, 0.30f, 0.32f), 0.30f, 0.62f);
            var boss = CreateMaterial("Cornberg_Shield_Iron_Boss", new Color(0.40f, 0.47f, 0.49f), 0.42f, 0.52f);
            var leather = CreateMaterial("Cornberg_Shield_Dark_Leather", new Color(0.13f, 0.065f, 0.035f), 0f, 0.86f);

            var root = new GameObject("CornbergRoundShieldPresentation");
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
            visual.name = "CornbergRoundShield_Visual";
            visual.transform.SetParent(root.transform, false);
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                string part = renderer.gameObject.name;
                if (part.Contains("Board")) renderer.sharedMaterials = new[] { wood };
                else if (part.Contains("Rim")) renderer.sharedMaterials = new[] { iron };
                else if (part.Contains("Boss")) renderer.sharedMaterials = new[] { boss };
                else if (part.Contains("Handle")) renderer.sharedMaterials = new[] { leather };
            }
            CopyImportedAnchorToWrapper(root.transform, visual.transform, "Grip");
            CopyImportedAnchorToWrapper(root.transform, visual.transform, "ShieldFront");
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);

            ConfigureNoviceOffhandSocket();
            CreatePreviewScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ConfigureNoviceOffhandSocket()
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(NovicePrefabPath);
            try
            {
                var animator = contents.GetComponentInChildren<Animator>(true);
                Require(animator != null && animator.avatar != null && animator.avatar.isHuman,
                    "Novice presentation must have an imported Humanoid Avatar before offhand socket authoring.");
                Transform leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                Transform socket = contents.GetComponentsInChildren<Transform>(true).SingleOrDefault(t => t.name == "LeftHandOffhand");
                Require(leftHand != null && socket != null && socket.parent == leftHand,
                    "LeftHandOffhand must be a child transform of the imported LeftHand bone.");
                Quaternion characterForward = Quaternion.LookRotation(Vector3.back, Vector3.up);
                socket.localPosition = new Vector3(0f, 0.025f, 0f);
                socket.localRotation = Quaternion.Inverse(leftHand.rotation) * (contents.transform.rotation * characterForward);
                PrefabUtility.SaveAsPrefabAsset(contents, NovicePrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }

        private static void ValidateAssets()
        {
            ValidateSourceFiles();
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            Require(importer != null && Mathf.Approximately(importer.globalScale, 1f), "Shield import scale must be 1.");
            Require(!importer.importAnimation && !importer.importCameras && !importer.importLights && !importer.importBlendShapes && !importer.addCollider,
                "Rigid shield importer must exclude animation, cameras, lights, blend shapes and generated colliders.");
            Require(importer.materialImportMode == ModelImporterMaterialImportMode.None && importer.importNormals == ModelImporterNormals.Import,
                "Shield presentation materials must be Unity-authored and imported normals retained.");
            Require(!AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().Any(), "Rigid shield unexpectedly contains animation clips.");

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            Require(model != null, "Imported shield FBX is missing.");
            RequireExpectedParts(model);
            var filters = model.GetComponentsInChildren<MeshFilter>(true).Where(f => f.sharedMesh != null).ToArray();
            int vertices = filters.Sum(f => f.sharedMesh.vertexCount);
            int triangles = filters.Sum(f => f.sharedMesh.triangles.Length / 3);
            foreach (var filter in filters)
            {
                Vector3[] normals = filter.sharedMesh.normals;
                Require(normals.Length == filter.sharedMesh.vertexCount && normals.All(n => n.sqrMagnitude > 0.000001f),
                    "Missing or degenerate authored normals on " + filter.name + ".");
            }
            Bounds bounds = MeasureBounds(model);
            Require(bounds.size.x >= 0.58f && bounds.size.x <= 0.78f && bounds.size.y >= 0.70f && bounds.size.y <= 0.86f && bounds.size.z >= 0.10f && bounds.size.z <= 0.42f,
                "Shield bounds should be a human-scale prototype heater shield; got " + bounds.size + ".");
            UnityEngine.Debug.Log("DICEFRE_SHIELD_MESH_STATS dimensions=" + bounds.size + " vertices=" + vertices + " triangles=" + triangles + " meshFilters=" + filters.Length);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Require(prefab != null && prefab.transform.localPosition == Vector3.zero && prefab.transform.localRotation == Quaternion.identity && prefab.transform.localScale == Vector3.one,
                "Shield presentation root must keep an identity transform at the grip anchor.");
            Transform visual = prefab.transform.Find("CornbergRoundShield_Visual");
            Require(visual != null && Quaternion.Angle(visual.localRotation, model.transform.localRotation) <= 0.1f,
                "Shield prefab must preserve the tested FBX import-axis conversion.");
            Transform importedGrip = FindUniqueTransform(visual, "Grip");
            Transform importedFront = FindUniqueTransform(visual, "ShieldFront");
            Transform gripAlias = prefab.transform.Find("Grip");
            Transform frontAlias = prefab.transform.Find("ShieldFront");
            Require(gripAlias != null && frontAlias != null, "Presentation wrapper must expose Grip and ShieldFront aliases.");
            Vector3 importedGripRoot = prefab.transform.InverseTransformPoint(importedGrip.position);
            Vector3 importedFrontRoot = prefab.transform.InverseTransformPoint(importedFront.position);
            Require(Vector3.Distance(importedGripRoot, Vector3.zero) <= 0.01f, "Imported Grip must remain at shield presentation root.");
            Require(Vector3.Distance(importedGripRoot, prefab.transform.InverseTransformPoint(gripAlias.position)) <= 0.001f &&
                Vector3.Distance(importedFrontRoot, prefab.transform.InverseTransformPoint(frontAlias.position)) <= 0.001f,
                "Presentation anchor aliases must pass through imported DCC anchors.");
            Vector3 importedFrontAxis = (importedFrontRoot - importedGripRoot).normalized;
            Require(importedFrontRoot.z > 0.15f && Vector3.Angle(importedFrontAxis, Vector3.forward) <= 1f,
                "Imported ShieldFront must point along Unity local +Z after the authored Blender -Y facing convention.");

            var novicePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NovicePrefabPath);
            Require(novicePrefab != null, "Novice presentation prefab is missing.");
            var noviceAnimator = novicePrefab.GetComponentInChildren<Animator>(true);
            Transform noviceLeftHand = noviceAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
            Transform offhand = novicePrefab.GetComponentsInChildren<Transform>(true).Single(t => t.name == "LeftHandOffhand");
            float socketForwardError = Vector3.Angle(offhand.forward, -novicePrefab.transform.forward);
            Require(offhand.parent == noviceLeftHand && Vector3.Distance(offhand.localPosition, new Vector3(0f, 0.025f, 0f)) <= 0.0001f && socketForwardError <= 0.1f,
                "Reusable Novice prefab must own a LeftHandOffhand orientation that faces the authored body forward (-Z); error=" + socketForwardError + " degrees.");

            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            foreach (var mat in renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct())
            {
                Require(mat.shader != null && mat.shader.name == "Universal Render Pipeline/Lit", "Shield materials must use URP/Lit.");
                Require(mat.HasProperty("_Cull") && Mathf.Approximately(mat.GetFloat("_Cull"), (float)UnityEngine.Rendering.CullMode.Back),
                    "Shield hard-surface materials must use ordinary backface culling: " + mat.name);
            }
            Require(renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().Count() == 4,
                "Expected four explicit shield presentation materials.");
            Require(prefab.GetComponentInChildren<Collider>(true) == null, "Shield art prefab must not contain gameplay colliders.");

            var opened = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var roots = opened.GetRootGameObjects();
                GameObject baseline = roots.SingleOrDefault(go => go.name == "NoviceBaseline");
                GameObject shieldNovice = roots.SingleOrDefault(go => go.name == "NoviceWithShield");
                Require(baseline != null && shieldNovice != null, "Preview scene must show a baseline Novice and a separate shield fixture.");
                Require(baseline.GetComponentsInChildren<Transform>(true).All(t => t.name != "CornbergRoundShieldPresentation"),
                    "Baseline Novice fixture unexpectedly has an equipped shield.");
                var shield = shieldNovice.GetComponentsInChildren<Transform>(true).SingleOrDefault(t => t.name == "CornbergRoundShieldPresentation");
                var socket = shieldNovice.GetComponentsInChildren<Transform>(true).Single(t => t.name == "LeftHandOffhand");
                Require(shield != null && shield.parent == socket && shield.localPosition == Vector3.zero &&
                    Quaternion.Angle(shield.localRotation, Quaternion.identity) <= 0.01f && shield.localScale == Vector3.one,
                    "Preview shield must attach directly to LeftHandOffhand with local identity and no scene correction.");
                var grip = shield.Find("Grip");
                var front = shield.Find("ShieldFront");
                Require(grip != null && front != null, "Attached shield has no pass-through anchor aliases.");
                float gripError = Vector3.Distance(grip.position, socket.position);
                float orientationError = Vector3.Angle(socket.forward, (front.position - grip.position).normalized);
                Require(gripError <= 0.02f, "Shield Grip is not aligned to LeftHandOffhand; error=" + gripError + " m.");
                Require(orientationError <= 1f, "ShieldFront does not follow LeftHandOffhand forward; error=" + orientationError + " degrees.");
                UnityEngine.Debug.Log("DICEFRE_SHIELD_SOCKET gripErrorM=" + gripError.ToString("F5") +
                    " frontSocketErrorDeg=" + orientationError.ToString("F3") +
                    " socketForward=" + socket.forward + " fixtureLocalPosition=" + shield.localPosition +
                    " fixtureLocalRotation=" + shield.localRotation.eulerAngles + " fixtureLocalScale=" + shield.localScale);

                ValidateSocketFollowsAnimation(shieldNovice, socket, grip, front);
            }
            finally { EditorSceneManager.CloseScene(opened, true); }
        }

        private static void ValidateSocketFollowsAnimation(GameObject novice, Transform socket, Transform grip, Transform front)
        {
            var clips = AssetDatabase.LoadAllAssetsAtPath(NoviceModelPath).OfType<AnimationClip>().ToArray();
            var locomotion = clips.FirstOrDefault(c => c.name.Contains("Locomotion"));
            Require(locomotion != null, "Novice locomotion clip is missing for offhand follow proof.");
            Vector3 restSocket = socket.position;
            Quaternion restRotation = socket.rotation;
            AnimationMode.StartAnimationMode();
            try
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(novice, locomotion, locomotion.length * 0.5f);
                AnimationMode.EndSampling();
                float positionDelta = Vector3.Distance(restSocket, socket.position);
                float rotationDelta = Quaternion.Angle(restRotation, socket.rotation);
                float gripError = Vector3.Distance(grip.position, socket.position);
                float frontError = Vector3.Angle(socket.forward, (front.position - grip.position).normalized);
                Require(positionDelta >= 0.001f || rotationDelta >= 0.5f,
                    "The LeftHandOffhand socket did not follow the animated hand bone.");
                Require(gripError <= 0.02f && frontError <= 1f,
                    "Shield anchor alignment drifted during locomotion sampling; grip=" + gripError + " m, front=" + frontError + " degrees.");
                UnityEngine.Debug.Log("DICEFRE_SHIELD_SOCKET_ANIMATION_OK positionDeltaM=" + positionDelta.ToString("F5") +
                    " rotationDeltaDeg=" + rotationDelta.ToString("F3") + " animatedGripErrorM=" + gripError.ToString("F5") +
                    " animatedFrontErrorDeg=" + frontError.ToString("F3"));
            }
            finally { if (AnimationMode.InAnimationMode()) AnimationMode.StopAnimationMode(); }
        }

        private static void CreatePreviewScene()
        {
            var previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var cameraObject = new GameObject("NoviceShieldPreviewCamera");
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 1.18f;
                camera.backgroundColor = new Color(0.12f, 0.15f, 0.18f);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.cullingMask = 1 << 31;
                camera.transform.position = new Vector3(2.6f, 2.15f, -5.3f);
                camera.transform.LookAt(new Vector3(0f, 0.88f, 0f));
                camera.tag = "MainCamera";

                CreateDirectionalLight("NoviceShieldPreviewKey", 1.2f, Quaternion.Euler(38f, -28f, 0f));
                CreateDirectionalLight("NoviceShieldPreviewFill", 0.45f, Quaternion.Euler(18f, 145f, 0f));
                var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "PreviewGround_NotGameplay";
                ground.transform.position = new Vector3(0f, -0.04f, 0f);
                ground.transform.localScale = new Vector3(40f, 1f, 40f);
                ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial("NoviceShield_Preview_Ground", new Color(0.19f, 0.22f, 0.24f), 0f, 0.9f);
                UnityEngine.Object.DestroyImmediate(ground.GetComponent<Collider>());

                var novice = AssetDatabase.LoadAssetAtPath<GameObject>(NovicePrefabPath);
                var shieldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                var baseline = (GameObject)PrefabUtility.InstantiatePrefab(novice, scene);
                baseline.name = "NoviceBaseline";
                baseline.transform.position = new Vector3(-0.72f, 0f, 0f);
                var shieldNovice = (GameObject)PrefabUtility.InstantiatePrefab(novice, scene);
                shieldNovice.name = "NoviceWithShield";
                shieldNovice.transform.position = new Vector3(0.72f, 0f, 0f);
                var socket = shieldNovice.GetComponentsInChildren<Transform>(true).Single(t => t.name == "LeftHandOffhand");
                var shield = (GameObject)PrefabUtility.InstantiatePrefab(shieldPrefab, scene);
                shield.name = "CornbergRoundShieldPresentation";
                shield.transform.SetParent(socket, false);
                shield.transform.localPosition = Vector3.zero;
                shield.transform.localRotation = Quaternion.identity;
                shield.transform.localScale = Vector3.one;

                foreach (var root in scene.GetRootGameObjects()) SetLayerRecursively(root, 31);
                foreach (var renderer in shield.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(previous); }
        }

        private static void CapturePreviewAsset()
        {
            ValidateAssets();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            Camera camera = null;
            RenderTexture target = null;
            Texture2D image = null;
            RenderTexture prior = RenderTexture.active;
            try
            {
                camera = scene.GetRootGameObjects().Select(root => root.GetComponent<Camera>()).FirstOrDefault(candidate => candidate != null);
                Require(camera != null, "Shield preview scene camera is missing.");
                target = new RenderTexture(1440, 900, 24);
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image = new Texture2D(1440, 900, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1440, 900), 0, 0);
                image.Apply();
                File.WriteAllBytes(Path.Combine(ProjectRoot(), PreviewPath), image.EncodeToPNG());
                AssetDatabase.ImportAsset(PreviewPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.SaveAssets();
                UnityEngine.Debug.Log("DICEFRE_SHIELD_PREVIEW details=baseline-left,shield-fixture-right size=1440x900 camera=dedicated-orthographic");
            }
            finally
            {
                if (camera != null) camera.targetTexture = null;
                RenderTexture.active = prior;
                if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
                if (image != null) UnityEngine.Object.DestroyImmediate(image);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static object BuildWindowsPlayer()
        {
            var timer = Stopwatch.StartNew();
            string output = Path.Combine(Path.GetTempPath(), "DiceFree-Shield-Issue40", "CornbergShieldValidation.exe");
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
                long totalBytes = Directory.Exists(Path.GetDirectoryName(output))
                    ? Directory.GetFiles(Path.GetDirectoryName(output), "*", SearchOption.AllDirectories).Sum(path => new FileInfo(path).Length) : 0L;
                bool success = report.summary.result == BuildResult.Succeeded;
                string marker = success ? "DICEFRE_SHIELD_PLAYER_BUILD_OK" : "DICEFRE_SHIELD_PLAYER_BUILD_FAIL";
                UnityEngine.Debug.Log(marker + " result=" + report.summary.result +
                    " errors=" + report.summary.totalErrors + " warnings=" + report.summary.totalWarnings + " output=" + output);
                return new
                {
                    success,
                    validation = "shield-windows-development-build",
                    finalMarker = marker,
                    elapsedSeconds = timer.Elapsed.TotalSeconds,
                    error = success ? null : report.summary.result.ToString(),
                    buildTarget = "StandaloneWindows64",
                    developmentBuild = true,
                    outputPath = output,
                    executableBytes = File.Exists(output) ? new FileInfo(output).Length : 0L,
                    totalOutputBytes = totalBytes,
                    errors = report.summary.totalErrors,
                    warnings = report.summary.totalWarnings,
                    testedGitSha = GitHead()
                };
            }
            catch (Exception exception)
            {
                timer.Stop();
                UnityEngine.Debug.LogError("DICEFRE_SHIELD_PLAYER_BUILD_FAIL\n" + exception);
                return new { success = false, validation = "shield-windows-development-build", finalMarker = "DICEFRE_SHIELD_PLAYER_BUILD_FAIL", elapsedSeconds = timer.Elapsed.TotalSeconds, error = exception.ToString(), outputPath = output, testedGitSha = GitHead() };
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
            Require(File.Exists(FullPath(SourceRoot + "/CornbergRoundShield.blend")), "Editable Blender shield source is missing.");
            Require(File.Exists(FullPath(SourceRoot + "/build_cornberg_round_shield.py")), "Shield source/export script is missing.");
            Require(File.Exists(FullPath(ModelPath)), "Exported Unity shield FBX is missing.");
            Require(Path.GetFileNameWithoutExtension(ModelPath) == "CornbergRoundShield", "Shield export name must match the source asset identity.");
        }

        private static void RequireExpectedParts(GameObject model)
        {
            string[] names = model.GetComponentsInChildren<MeshFilter>(true).Where(filter => filter.sharedMesh != null).Select(filter => filter.name).ToArray();
            string[] expected = { "CornbergRoundShield_Board", "CornbergRoundShield_Rim", "CornbergRoundShield_Boss", "CornbergRoundShield_Handle" };
            Require(names.Length == expected.Length && expected.All(name => names.Count(actual => actual == name) == 1),
                "FBX must preserve exactly one board, rim, boss and handle mesh. Found: " + string.Join(", ", names));
        }

        private static Bounds MeasureBounds(GameObject model)
        {
            var renderers = model.GetComponentsInChildren<Renderer>(true);
            Require(renderers.Length > 0, "Shield model has no renderers.");
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        private static Transform FindUniqueTransform(Transform root, string name)
        {
            Transform[] found = root.GetComponentsInChildren<Transform>(true).Where(t => t.name == name).ToArray();
            Require(found.Length == 1, "Expected one " + name + " transform under " + root.name + "; found " + found.Length + ".");
            return found[0];
        }

        private static void CopyImportedAnchorToWrapper(Transform root, Transform visual, string name)
        {
            Transform imported = FindUniqueTransform(visual, name);
            var alias = new GameObject(name);
            alias.transform.SetParent(root, false);
            alias.transform.localPosition = root.InverseTransformPoint(imported.position);
            alias.transform.localRotation = Quaternion.Inverse(root.rotation) * imported.rotation;
        }

        private static Material CreateMaterial(string name, Color color, float metallic, float roughness)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = Shader.Find("Universal Render Pipeline/Lit");
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", 1f - roughness);
            material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Back);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateDirectionalLight(string name, float intensity, Quaternion rotation)
        {
            var go = new GameObject(name);
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
            go.transform.rotation = rotation;
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            foreach (Transform child in root.transform) SetLayerRecursively(child.gameObject, layer);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static string FullPath(string relative) => Path.GetFullPath(Path.Combine(ProjectRoot(), relative.Replace('/', Path.DirectorySeparatorChar)));
        private static string ProjectRoot() => Directory.GetParent(Application.dataPath).FullName;
        private static string GitHead() => Environment.GetEnvironmentVariable("GIT_COMMIT") ?? string.Empty;
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
