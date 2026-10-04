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
    /// <summary>Focused Editor-only proof that Novice footwear replaces baseline feet and follows the rig.</summary>
    internal static class NoviceFootwearPipelineValidation
    {
        private const string SourceRoot = "SourceArt/Characters/NoviceShoes";
        private const string ModelPath = "Assets/_DiceFree/Art/Characters/NoviceShoes/Models/NoviceShoes.fbx";
        private const string PrefabPath = "Assets/_DiceFree/Art/Characters/NoviceShoes/Prefabs/NoviceShoesPresentation.prefab";
        private const string MaterialRoot = "Assets/_DiceFree/Art/Characters/NoviceShoes/Materials";
        private const string NovicePrefabPath = "Assets/_DiceFree/Art/Characters/Novice/Prefabs/NovicePresentation.prefab";
        private const string NoviceModelPath = "Assets/_DiceFree/Art/Characters/Novice/Models/Novice.fbx";
        private const string ScenePath = "Assets/_DiceFree/Art/Validation/Scenes/NoviceShoesPipelinePreview.unity";
        private const string PreviewPath = "Assets/_DiceFree/Art/Validation/Previews/NoviceShoes_Unity.png";

        [CliCommand("dicefree.art.shoes.prepare", "Prepare the Novice baseline-feet replacement and paired shoe presentation fixture.", Tags = new[] { "art", "dicefree/art" })]
        private static object PrepareCommand() => Run("shoes-prepare", "DICEFRE_SHOES_PREPARED", PrepareAssets);

        [CliCommand("dicefree.art.shoes.validate", "Validate paired footwear, real foot-bone attachment, and baseline replacement behavior.", Tags = new[] { "tests", "dicefree/art" })]
        private static object ValidateCommand() => Run("shoes-pipeline", "DICEFRE_SHOES_PIPELINE_OK", ValidateAssets);

        [CliCommand("dicefree.art.shoes.capture-preview", "Capture the Novice baseline-versus-shoes replacement preview.", Tags = new[] { "art", "capture", "dicefree/art" })]
        private static object CapturePreviewCommand() => Run("shoes-preview", "DICEFRE_SHOES_PREVIEW_OK", CapturePreview);

        [CliCommand("dicefree.art.shoes.build-windows", "Build the isolated Novice footwear preview as a Windows x64 Development player.", Tags = new[] { "builds", "dicefree/art" })]
        private static object BuildWindowsCommand() => BuildWindows();

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
                UnityEngine.Debug.LogError("DICEFRE_SHOES_PIPELINE_FAIL\n" + error);
            }
            timer.Stop();
            return new
            {
                success = error == null,
                validation = name,
                finalMarker = error == null ? marker : "DICEFRE_SHOES_PIPELINE_FAIL",
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
            EnsureFolder("Assets/_DiceFree/Art/Characters/NoviceShoes");
            EnsureFolder(MaterialRoot);
            EnsureFolder("Assets/_DiceFree/Art/Characters/NoviceShoes/Models");
            EnsureFolder("Assets/_DiceFree/Art/Characters/NoviceShoes/Prefabs");
            EnsureFolder("Assets/_DiceFree/Art/Validation/Scenes");
            EnsureFolder("Assets/_DiceFree/Art/Validation/Previews");

            var importer = ImportModel(ModelPath, withAnimation: false);
            Require(!AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().Any(), "Rigid footwear unexpectedly imported animation clips.");
            var imported = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            Require(imported != null, "Novice shoes FBX did not import as a model.");
            ValidateSourceFiles();
            FindUnique(imported.transform, "LeftShoeGroup");
            FindUnique(imported.transform, "RightShoeGroup");
            FindUnique(imported.transform, "LeftShoeAttachment");
            FindUnique(imported.transform, "RightShoeAttachment");

            var upper = CreateMaterial("Novice_Shoe_Warm_Brown", new Color(0.31f, 0.15f, 0.072f), 0.84f);
            var sole = CreateMaterial("Novice_Shoe_Dark_Sole", new Color(0.09f, 0.065f, 0.048f), 0.92f);
            var root = new GameObject("NoviceShoesPresentation");
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(imported);
            visual.name = "NoviceShoes_Visual";
            visual.transform.SetParent(root.transform, false);
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterials = renderer.sharedMaterials.Select(_ => renderer.name.Contains("Sole") ? sole : upper).ToArray();
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            CreatePreviewScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("DICEFRE_SHOES_PREPARED importerAnimation=" + importer.importAnimation + " materials=2");
        }

        private static void ValidateSourceFiles()
        {
            string root = ProjectRoot();
            Require(File.Exists(Path.Combine(root, SourceRoot, "NoviceShoes.blend")), "Editable Novice shoes source is missing.");
            Require(File.Exists(Path.Combine(root, SourceRoot, "build_novice_shoes.py")), "Reproducible Novice shoes export script is missing.");
            Require(File.Exists(Path.Combine(root, ModelPath)), "Novice shoes FBX is missing.");
            Require(Path.GetFileNameWithoutExtension(ModelPath) == "NoviceShoes", "Source and export identity do not match.");
            Require(File.Exists(Path.Combine(root, NovicePrefabPath)), "Novice presentation prefab is missing.");
        }

        private static void ValidateAssets()
        {
            ValidateSourceFiles();
            Require(File.Exists(Path.Combine(ProjectRoot(), ScenePath)), "Dedicated shoes replacement preview scene is missing.");
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            Require(importer != null && Mathf.Approximately(importer.globalScale, 1f), "Shoe ModelImporter or unit scale is invalid.");
            Require(!importer.importAnimation && !importer.importCameras && !importer.importLights && !importer.importBlendShapes && !importer.addCollider,
                "Rigid shoe importer must disable animation, cameras, lights, blend shapes and generated colliders.");
            Require(importer.materialImportMode == ModelImporterMaterialImportMode.None && importer.importNormals == ModelImporterNormals.Import,
                "Shoe materials must be Unity-authored and imported normals retained.");

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Require(model != null && prefab != null && prefab.transform.localPosition == Vector3.zero &&
                Quaternion.Angle(prefab.transform.localRotation, Quaternion.identity) <= 0.01f && prefab.transform.localScale == Vector3.one,
                "Shoe presentation root must preserve an identity transform.");
            foreach (string anchor in new[] { "LeftShoeAttachment", "RightShoeAttachment" })
                Require(model.GetComponentsInChildren<Transform>(true).Count(t => t.name == anchor) == 1, "Expected one imported DCC anchor: " + anchor);
            foreach (string group in new[] { "LeftShoeGroup", "RightShoeGroup" })
                Require(model.GetComponentsInChildren<Transform>(true).Count(t => t.name == group) == 1, "Expected one shoe group: " + group);
            var filters = model.GetComponentsInChildren<MeshFilter>(true).Where(f => f.sharedMesh != null).ToArray();
            int vertices = filters.Sum(f => f.sharedMesh.vertexCount);
            int triangles = filters.Sum(f => f.sharedMesh.triangles.Length / 3);
            Require(filters.Length == 4 && vertices > 0 && triangles > 0, "Expected two closed prototype parts per shoe; filters=" + filters.Length + ".");
            foreach (var filter in filters)
                Require(filter.sharedMesh.normals.Length == filter.sharedMesh.vertexCount && filter.sharedMesh.normals.All(n => n.sqrMagnitude > 0.000001f),
                    "Missing or degenerate imported shoe normals on " + filter.name + ".");
            Bounds bounds = MeasureBounds(model);
            Require(bounds.size.x > 0.15f && bounds.size.x < 0.55f && bounds.size.y > 0.08f && bounds.size.z > 0.20f,
                "The paired shoe bounds are zero or implausible: " + bounds.size + ".");
            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            var materials = renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().ToArray();
            Require(materials.Length == 2, "Expected separate upper and sole materials; got " + materials.Length + ".");
            foreach (var material in materials)
            {
                Require(material.shader != null && material.shader.name == "Universal Render Pipeline/Lit", "Shoe material must use URP/Lit.");
                Require(material.HasProperty("_Cull") && Mathf.Approximately(material.GetFloat("_Cull"), (float)UnityEngine.Rendering.CullMode.Back),
                    "Shoe hard-surface material must use ordinary backface culling.");
            }
            Require(prefab.GetComponentInChildren<Collider>(true) == null, "Shoe presentation must not contain gameplay colliders.");
            UnityEngine.Debug.Log("DICEFRE_SHOES_MESH_STATS bounds=" + bounds.size + " vertices=" + vertices + " triangles=" + triangles + " meshFilters=" + filters.Length + " materials=" + materials.Length);

            var opened = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var roots = opened.GetRootGameObjects();
                var baseline = roots.SingleOrDefault(go => go.name == "NoviceBaseline");
                var equipped = roots.SingleOrDefault(go => go.name == "NoviceWithShoes");
                Require(baseline != null && equipped != null, "Preview must contain baseline and equipped Novice fixtures.");
                ValidateReplacementState(baseline, equipped);
                ValidateAnimationFollowing(equipped);
            }
            finally { EditorSceneManager.CloseScene(opened, true); }
        }

        private static void ValidateReplacementState(GameObject baseline, GameObject equipped)
        {
            var baselineFeet = FindFeetRenderer(baseline);
            var equippedFeet = FindFeetRenderer(equipped);
            var pair = equipped.transform.GetComponentsInChildren<Transform>(true).SingleOrDefault(t => t.name == "NoviceShoesPresentation");
            Require(baselineFeet != null && baselineFeet.enabled, "Unequipped baseline fixture must show its separate feet renderer.");
            Require(equippedFeet != null && !equippedFeet.enabled, "Equipped fixture must disable/replace the baseline feet renderer.");
            Require(pair != null && pair.gameObject.activeSelf, "Equipped fixture must enable the paired shoe presentation.");
            var animator = equipped.GetComponent<Animator>();
            Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            Transform leftGroup = FindUnique(equipped.transform, "LeftShoeGroup");
            Transform rightGroup = FindUnique(equipped.transform, "RightShoeGroup");
            Transform leftAnchor = FindUnique(equipped.transform, "LeftShoeAttachment");
            Transform rightAnchor = FindUnique(equipped.transform, "RightShoeAttachment");
            Require(leftGroup.IsChildOf(leftFoot) && rightGroup.IsChildOf(rightFoot), "Each shoe must be bound under its matching animated foot bone.");
            float leftError = Vector3.Distance(leftAnchor.position, leftFoot.position);
            float rightError = Vector3.Distance(rightAnchor.position, rightFoot.position);
            Require(leftError <= 0.025f && rightError <= 0.025f,
                "Shoe DCC anchors must align with corresponding foot bones; left=" + leftError + " m right=" + rightError + " m.");
            Require(equipped.GetComponentInChildren<Collider>(true) == null && pair.GetComponentInChildren<Collider>(true) == null,
                "Wearable fixture must not add a collider derived from render geometry.");

            // Exercise removal and restoration on the same Novice instance.
            equippedFeet.enabled = true;
            leftGroup.gameObject.SetActive(false); rightGroup.gameObject.SetActive(false);
            Require(equippedFeet.enabled && !leftGroup.gameObject.activeSelf && !rightGroup.gameObject.activeSelf, "Removing the shoe fixture must restore baseline feet.");
            equippedFeet.enabled = false;
            leftGroup.gameObject.SetActive(true); rightGroup.gameObject.SetActive(true);
            Require(!equippedFeet.enabled && leftGroup.gameObject.activeSelf && rightGroup.gameObject.activeSelf, "Equipping the shoe fixture must replace baseline feet.");
            UnityEngine.Debug.Log("DICEFRE_SHOES_REPLACEMENT_OK baselineOn=true equippedBaselineOff=true removeRestores=true leftGripErrorM=" + leftError.ToString("F5") + " rightGripErrorM=" + rightError.ToString("F5"));
        }

        private static void ValidateAnimationFollowing(GameObject equipped)
        {
            var animator = equipped.GetComponent<Animator>();
            Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            Transform leftGroup = FindUnique(equipped.transform, "LeftShoeGroup");
            Transform rightGroup = FindUnique(equipped.transform, "RightShoeGroup");
            Transform leftAnchor = FindUnique(equipped.transform, "LeftShoeAttachment");
            Transform rightAnchor = FindUnique(equipped.transform, "RightShoeAttachment");
            Vector3 leftBefore = leftFoot.position, rightBefore = rightFoot.position;
            var clip = AssetDatabase.LoadAllAssetsAtPath(NoviceModelPath).OfType<AnimationClip>().FirstOrDefault(c => c.name.Contains("Locomotion"));
            Require(clip != null, "Novice Locomotion clip is required for shoe follow validation.");
            AnimationMode.StartAnimationMode();
            try
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(equipped, clip, clip.length * 0.5f);
                AnimationMode.EndSampling();
                float leftDelta = Vector3.Distance(leftBefore, leftFoot.position);
                float rightDelta = Vector3.Distance(rightBefore, rightFoot.position);
                float leftError = Vector3.Distance(leftAnchor.position, leftFoot.position);
                float rightError = Vector3.Distance(rightAnchor.position, rightFoot.position);
                Require(leftDelta >= 0.005f && rightDelta >= 0.005f, "Both left and right feet must move during sampled locomotion.");
                Require(leftGroup.IsChildOf(leftFoot) && rightGroup.IsChildOf(rightFoot) && leftError <= 0.025f && rightError <= 0.025f,
                    "Both shoe attachments must follow their animated feet; parent bones left=" + leftGroup.parent.name + "/" + leftFoot.name + " right=" + rightGroup.parent.name + "/" + rightFoot.name + " anchor errors left=" + leftError + " right=" + rightError + ".");
                UnityEngine.Debug.Log("DICEFRE_SHOES_ANIMATION_OK leftFootDeltaM=" + leftDelta.ToString("F5") + " rightFootDeltaM=" + rightDelta.ToString("F5") + " leftAnchorErrorM=" + leftError.ToString("F5") + " rightAnchorErrorM=" + rightError.ToString("F5"));
            }
            finally { if (AnimationMode.InAnimationMode()) AnimationMode.StopAnimationMode(); }
        }

        private static void CreatePreviewScene()
        {
            var previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var cameraObject = new GameObject("NoviceShoesPreviewCamera");
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 1.16f;
                camera.backgroundColor = new Color(0.12f, 0.15f, 0.18f);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.cullingMask = 1 << 31;
                camera.transform.position = new Vector3(2.6f, 2.15f, -5.3f);
                camera.transform.LookAt(new Vector3(0f, 0.88f, 0f));
                camera.tag = "MainCamera";
                var key = new GameObject("NoviceShoesPreviewKey").AddComponent<Light>();
                key.type = LightType.Directional; key.intensity = 1.15f; key.cullingMask = 1 << 31; key.transform.rotation = Quaternion.Euler(38f, -28f, 0f);
                var fill = new GameObject("NoviceShoesPreviewFill").AddComponent<Light>();
                fill.type = LightType.Directional; fill.intensity = 0.42f; fill.cullingMask = 1 << 31; fill.transform.rotation = Quaternion.Euler(18f, 145f, 0f);
                var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "PreviewGround_NotGameplay"; ground.layer = 31; ground.transform.position = new Vector3(0f, -0.04f, 0f); ground.transform.localScale = new Vector3(40f, 1f, 40f);
                ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial("NoviceShoes_Preview_Ground", new Color(0.19f, 0.22f, 0.24f), 0.9f);
                UnityEngine.Object.DestroyImmediate(ground.GetComponent<Collider>());

                var novicePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NovicePrefabPath);
                var shoesPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                var baseline = (GameObject)PrefabUtility.InstantiatePrefab(novicePrefab, scene);
                baseline.name = "NoviceBaseline"; baseline.layer = 31; baseline.transform.position = new Vector3(-0.72f, 0f, 0f); SetLayerRecursively(baseline, 31);
                var equipped = (GameObject)PrefabUtility.InstantiatePrefab(novicePrefab, scene);
                equipped.name = "NoviceWithShoes"; equipped.layer = 31; equipped.transform.position = new Vector3(0.72f, 0f, 0f); SetLayerRecursively(equipped, 31);
                // The validation scene must bind shoe groups to the actual bones.
                // Unpack these fixture instances so Unity permits the cross-prefab
                // hierarchy used by a character-owned wearable presentation.
                UnpackNestedPrefabInstances(equipped);
                var feet = FindFeetRenderer(equipped); Require(feet != null, "Novice baseline-feet renderer is missing while creating fixture."); feet.enabled = false;
                var pair = (GameObject)PrefabUtility.InstantiatePrefab(shoesPrefab, scene);
                pair.name = "NoviceShoesPresentation"; SetLayerRecursively(pair, 31);
                UnpackNestedPrefabInstances(pair);
                pair.transform.SetParent(equipped.transform, false);
                AttachSide(pair.transform, equipped.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftFoot), "LeftShoeGroup", "LeftShoeAttachment");
                AttachSide(pair.transform, equipped.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightFoot), "RightShoeGroup", "RightShoeAttachment");
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(previous); }
        }

        private static void AttachSide(Transform visualRoot, Transform foot, string groupName, string anchorName)
        {
            Transform group = FindUnique(visualRoot, groupName);
            Transform anchor = FindUnique(visualRoot, anchorName);
            group.SetParent(foot, true);
            // The exported anchor is the authored bone-head reference. Keep the
            // DCC relative pose while parenting so the preview has no correction.
            Require(Vector3.Distance(anchor.position, foot.position) <= 0.025f,
                anchorName + " did not import at its matching foot bone; error=" + Vector3.Distance(anchor.position, foot.position) + " m.");
        }

        private static void UnpackNestedPrefabInstances(GameObject root)
        {
            // The character and imported FBX each contain nested model prefab
            // instances. Unpack from the outside inward before binding the shoe
            // groups across those instance boundaries to the rig bones.
            while (true)
            {
                var instanceRoots = root.GetComponentsInChildren<Transform>(true)
                    .Where(t => PrefabUtility.IsPartOfPrefabInstance(t.gameObject))
                    .Select(t => PrefabUtility.GetNearestPrefabInstanceRoot(t.gameObject))
                    .Where(instanceRoot => instanceRoot != null)
                    .Distinct()
                    .ToArray();
                if (instanceRoots.Length == 0) return;
                foreach (var instanceRoot in instanceRoots)
                    if (PrefabUtility.IsPartOfPrefabInstance(instanceRoot))
                        PrefabUtility.UnpackPrefabInstance(instanceRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }

        private static void CapturePreview()
        {
            var previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                var camera = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<Camera>(true)).Single(c => c.name == "NoviceShoesPreviewCamera");
                const int width = 1440, height = 900;
                var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                var previousTarget = camera.targetTexture; var previousActive = RenderTexture.active; var previousBackground = camera.backgroundColor; var previousFlags = camera.clearFlags;
                var previousSize = camera.orthographicSize; var previousEnabled = camera.enabled; var previousCulling = camera.cullingMask;
                var previousColorSpace = QualitySettings.activeColorSpace;
                var image = new Texture2D(width, height, TextureFormat.RGBA32, false, previousColorSpace == ColorSpace.Linear);
                try
                {
                    camera.enabled = false; camera.cullingMask = 1 << 31; camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.backgroundColor = new Color(0.12f, 0.15f, 0.18f); camera.orthographicSize = previousSize; camera.targetTexture = target;
                    RenderTexture.active = target; camera.Render(); image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply(false, false);
                    File.WriteAllBytes(Path.Combine(ProjectRoot(), PreviewPath), image.EncodeToPNG());
                    AssetDatabase.ImportAsset(PreviewPath, ImportAssetOptions.ForceUpdate);
                }
                finally
                {
                    camera.targetTexture = previousTarget; camera.backgroundColor = previousBackground; camera.clearFlags = previousFlags; camera.enabled = previousEnabled; camera.cullingMask = previousCulling;
                    RenderTexture.active = previousActive; UnityEngine.Object.DestroyImmediate(image); target.Release(); UnityEngine.Object.DestroyImmediate(target);
                }
                UnityEngine.Debug.Log("DICEFRE_SHOES_PREVIEW details=baseline-left,feet-replaced-shoes-right size=" + width + "x" + height);
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(previous); }
        }

        private static object BuildWindows()
        {
            string outputRoot = Path.Combine(Path.GetTempPath(), "DiceFree-Shoes-Issue41");
            Directory.CreateDirectory(outputRoot);
            string output = Path.Combine(outputRoot, "NoviceShoesValidation.exe");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            string marker = report.summary.result == BuildResult.Succeeded ? "DICEFRE_SHOES_PLAYER_BUILD_OK" : "DICEFRE_SHOES_PLAYER_BUILD_FAIL";
            UnityEngine.Debug.Log(marker + " result=" + report.summary.result + " errors=" + report.summary.totalErrors + " warnings=" + report.summary.totalWarnings + " output=" + output + " bytes=" + report.summary.totalSize);
            return new { success = report.summary.result == BuildResult.Succeeded, validation = "shoes-player-build", finalMarker = marker, errors = report.summary.totalErrors, warnings = report.summary.totalWarnings, output, outputBytes = report.summary.totalSize, testedGitSha = GitHead() };
        }

        private static ModelImporter ImportModel(string path, bool withAnimation)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            Require(importer != null, "ModelImporter is missing: " + path);
            importer.globalScale = 1f;
            importer.importAnimation = withAnimation;
            importer.importCameras = false; importer.importLights = false; importer.importBlendShapes = false; importer.addCollider = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importNormals = ModelImporterNormals.Import;
            importer.SaveAndReimport();
            return importer;
        }

        private static SkinnedMeshRenderer FindFeetRenderer(GameObject novice) => novice.GetComponentsInChildren<SkinnedMeshRenderer>(true).SingleOrDefault(r => r.name == "Novice_BaselineFeet");
        private static Transform FindUnique(Transform root, string name)
        {
            var matches = root.GetComponentsInChildren<Transform>(true).Where(t => t.name == name).ToArray();
            Require(matches.Length == 1, "Expected one transform named " + name + "; found " + matches.Length + ".");
            return matches[0];
        }
        private static Bounds MeasureBounds(GameObject model)
        {
            var renderers = model.GetComponentsInChildren<Renderer>(true);
            Require(renderers.Length > 0, "Shoe model has no renderer.");
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
        private static Material CreateMaterial(string name, Color color, float roughness)
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
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 1f - roughness);
            material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Back);
            EditorUtility.SetDirty(material);
            return material;
        }
        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform) SetLayerRecursively(child.gameObject, layer);
        }
        private static string ProjectRoot() => Directory.GetParent(Application.dataPath).FullName;
        private static string GitHead()
        {
            string root = ProjectRoot();
            try { return File.ReadAllText(Path.Combine(root, ".git", "HEAD")).Trim(); }
            catch { return Environment.GetEnvironmentVariable("GIT_COMMIT") ?? string.Empty; }
        }
        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
