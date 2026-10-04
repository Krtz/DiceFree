using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;

namespace DiceFree.EditorTools
{
    /// <summary>Focused Editor-only proof of the Novice empty-baseline Head wearable attachment.</summary>
    internal static class NoviceHatPipelineValidation
    {
        private const string SourceRoot = "SourceArt/Characters/NoviceHat";
        private const string ModelPath = "Assets/_DiceFree/Art/Characters/NoviceHat/Models/NoviceHat.fbx";
        private const string PrefabPath = "Assets/_DiceFree/Art/Characters/NoviceHat/Prefabs/NoviceHatPresentation.prefab";
        private const string MaterialRoot = "Assets/_DiceFree/Art/Characters/NoviceHat/Materials";
        private const string NovicePrefabPath = "Assets/_DiceFree/Art/Characters/Novice/Prefabs/NovicePresentation.prefab";
        private const string NoviceModelPath = "Assets/_DiceFree/Art/Characters/Novice/Models/Novice.fbx";
        private const string ScenePath = "Assets/_DiceFree/Art/Validation/Scenes/NoviceHatPipelinePreview.unity";
        private const string PreviewPath = "Assets/_DiceFree/Art/Validation/Previews/NoviceHat_Unity.png";

        [CliCommand("dicefree.art.hat.prepare", "Prepare the prototype Novice hat and empty-head-slot preview fixture.", Tags = new[] { "art", "dicefree/art" })]
        private static object PrepareCommand() => Run("novice-hat-prepare", "DICEFRE_NOVICE_HAT_PREPARED", PrepareAssets);

        [CliCommand("dicefree.art.hat.validate", "Validate source/import, empty baseline, Head attachment and animation following.", Tags = new[] { "tests", "dicefree/art" })]
        private static object ValidateCommand() => Run("novice-hat-pipeline", "DICEFRE_NOVICE_HAT_PIPELINE_OK", ValidateAssets);

        [CliCommand("dicefree.art.hat.capture-preview", "Capture the baseline-versus-hat Novice preview.", Tags = new[] { "art", "capture", "dicefree/art" })]
        private static object CapturePreviewCommand() => Run("novice-hat-preview", "DICEFRE_NOVICE_HAT_PREVIEW_OK", CapturePreview);

        [CliCommand("dicefree.art.hat.build-windows", "Build the isolated Novice-hat preview as Windows x64 Development.", Tags = new[] { "builds", "dicefree/art" })]
        private static object BuildWindowsCommand() => BuildWindows();

        private static object Run(string name, string marker, Action action)
        {
            var timer = Stopwatch.StartNew();
            string error = null;
            try { action(); UnityEngine.Debug.Log(marker); }
            catch (Exception exception) { error = exception.ToString(); UnityEngine.Debug.LogError("DICEFRE_NOVICE_HAT_PIPELINE_FAIL\n" + error); }
            timer.Stop();
            return new { success = error == null, validation = name, finalMarker = error == null ? marker : "DICEFRE_NOVICE_HAT_PIPELINE_FAIL", elapsedSeconds = timer.Elapsed.TotalSeconds, error, modelPath = ModelPath, prefabPath = PrefabPath, previewScenePath = ScenePath, previewPath = PreviewPath, testedGitSha = GitHead() };
        }

        private static void PrepareAssets()
        {
            EnsureFolder("Assets/_DiceFree/Art/Characters/NoviceHat");
            EnsureFolder(MaterialRoot);
            EnsureFolder("Assets/_DiceFree/Art/Characters/NoviceHat/Models");
            EnsureFolder("Assets/_DiceFree/Art/Characters/NoviceHat/Prefabs");
            EnsureFolder("Assets/_DiceFree/Art/Validation/Scenes");
            EnsureFolder("Assets/_DiceFree/Art/Validation/Previews");

            AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            Require(importer != null, "Novice hat ModelImporter is missing.");
            importer.globalScale = 1f;
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.addCollider = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importNormals = ModelImporterNormals.Import;
            importer.SaveAndReimport();
            Require(!AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().Any(), "Rigid hat unexpectedly imported animation clips.");

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            Require(model != null, "Novice hat FBX did not import.");
            ValidateSourceFiles();
            var crown = CreateMaterial("Novice_Hat_Muted_Green_Fabric", new Color(0.20f, 0.27f, 0.22f), 0.14f);
            var band = CreateMaterial("Novice_Hat_Warm_Brown_Band", new Color(0.24f, 0.13f, 0.075f), 0.10f);
            var root = new GameObject("NoviceHatPresentation");
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
            visual.name = "NoviceHat_Visual";
            visual.transform.SetParent(root.transform, false);
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                bool isBand = renderer.name.Contains("Band");
                renderer.sharedMaterials = renderer.sharedMaterials.Select(_ => isBand ? band : crown).ToArray();
            }
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            CreatePreviewScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("DICEFRE_NOVICE_HAT_PREPARED importerAnimation=" + importer.importAnimation + " materials=2 attachmentRoot=FBX-origin");
        }

        private static void ValidateSourceFiles()
        {
            string root = ProjectRoot();
            Require(File.Exists(Path.Combine(root, SourceRoot, "NoviceHat.blend")), "Editable Blender hat source is missing.");
            Require(File.Exists(Path.Combine(root, SourceRoot, "build_novice_hat.py")), "Reproducible hat export script is missing.");
            Require(File.Exists(Path.Combine(root, ModelPath)), "Novice hat FBX export is missing.");
            Require(Path.GetFileNameWithoutExtension(ModelPath) == "NoviceHat", "Source/export identity does not match.");
            Require(File.Exists(Path.Combine(root, NovicePrefabPath)), "Novice character presentation prefab is missing.");
        }

        private static void ValidateAssets()
        {
            ValidateSourceFiles();
            Require(File.Exists(Path.Combine(ProjectRoot(), ScenePath)), "Dedicated Novice hat preview scene is missing.");
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            Require(importer != null && Mathf.Approximately(importer.globalScale, 1f), "Hat importer or global scale is invalid.");
            Require(!importer.importAnimation && !importer.importCameras && !importer.importLights && !importer.importBlendShapes && !importer.addCollider,
                "Rigid hat importer must disable animation, cameras, lights, blend shapes and generated colliders.");
            Require(importer.materialImportMode == ModelImporterMaterialImportMode.None && importer.importNormals == ModelImporterNormals.Import,
                "Hat must use Unity-authored materials and imported normals.");
            var imported = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Require(imported != null && prefab != null, "Imported hat model or presentation prefab is missing.");
            Require(prefab.transform.localPosition == Vector3.zero && Quaternion.Angle(prefab.transform.localRotation, Quaternion.identity) <= 0.01f && prefab.transform.localScale == Vector3.one,
                "Hat presentation prefab root must have an identity transform.");
            Require(imported.transform.localPosition == Vector3.zero && Quaternion.Angle(imported.transform.localRotation, Quaternion.identity) <= 0.01f && imported.transform.localScale == Vector3.one,
                "Imported FBX attachment root must retain its authored identity pivot.");
            var filters = imported.GetComponentsInChildren<MeshFilter>(true).Where(f => f.sharedMesh != null).ToArray();
            Require(filters.Length == 2, "Expected the simple crown and band meshes; got " + filters.Length + ".");
            Require(filters.Any(f => f.name == "NoviceHat_Crown") && filters.Any(f => f.name == "NoviceHat_Band"),
                "Imported hat mesh names must preserve the source crown/band identity.");
            int vertices = filters.Sum(f => f.sharedMesh.vertexCount);
            int triangles = filters.Sum(f => f.sharedMesh.triangles.Length / 3);
            foreach (var filter in filters)
                Require(filter.sharedMesh.normals.Length == filter.sharedMesh.vertexCount && filter.sharedMesh.normals.All(n => n.sqrMagnitude > 0.000001f),
                    "Hat mesh has missing/degenerate imported normals: " + filter.name + ".");
            var materials = prefab.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().ToArray();
            Require(materials.Length == 2, "Expected two separate canvas/band materials; got " + materials.Length + ".");
            foreach (var material in materials)
            {
                Require(material.shader != null && material.shader.name == "Universal Render Pipeline/Lit", "Hat materials must use URP/Lit.");
                Require(material.HasProperty("_Cull") && Mathf.Approximately(material.GetFloat("_Cull"), (float)UnityEngine.Rendering.CullMode.Back), "Hat must use ordinary backface culling.");
            }
            Require(prefab.GetComponentInChildren<Collider>(true) == null, "Hat presentation must not contain gameplay colliders.");
            Bounds bounds = MeasureBounds(imported);
            Require(bounds.size.x > 0.25f && bounds.size.x < 0.42f && bounds.size.y > 0.14f && bounds.size.y < 0.27f && bounds.size.z > 0.25f && bounds.size.z < 0.42f,
                "Hat size is zero or implausible after Unity axis conversion: " + bounds.size + ".");
            UnityEngine.Debug.Log("DICEFRE_NOVICE_HAT_MESH_STATS bounds=" + bounds.size + " vertices=" + vertices + " triangles=" + triangles + " meshFilters=" + filters.Length + " materials=" + materials.Length);

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var roots = scene.GetRootGameObjects();
                var baseline = roots.SingleOrDefault(go => go.name == "NoviceBaseline");
                var equipped = roots.SingleOrDefault(go => go.name == "NoviceWithHat");
                Require(baseline != null && equipped != null, "Preview must contain baseline and hat-equipped Novice fixtures.");
                ValidateHeadSlot(baseline, equipped);
                ValidateHeadMotion(equipped);
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }

        private static void ValidateHeadSlot(GameObject baseline, GameObject equipped)
        {
            Transform baselineHead = FindUnique(baseline.transform, "Head");
            Transform head = FindUnique(equipped.transform, "Head");
            var animator = equipped.GetComponent<Animator>();
            Require(animator != null && animator.avatar != null && animator.avatar.isValid && animator.avatar.isHuman &&
                animator.GetBoneTransform(HumanBodyBones.Head) == head,
                "Semantic Head reference must be the valid Humanoid Avatar's Head bone.");
            Transform hat = equipped.transform.GetComponentsInChildren<Transform>(true).SingleOrDefault(t => t.name == "NoviceHatPresentation");
            Require(baseline.GetComponentsInChildren<Transform>(true).All(t => t.name != "NoviceHatPresentation") &&
                !baseline.GetComponentsInChildren<Renderer>(true).Any(r => r.name.StartsWith("NoviceHat_", StringComparison.Ordinal)),
                "The Novice baseline Head slot must remain empty.");
            Require(hat != null && hat.gameObject.activeSelf && hat.parent == head, "Equipped hat presentation must be an active direct child of the semantic Head bone.");
            Require(hat.localPosition == Vector3.zero && Quaternion.Angle(hat.localRotation, Quaternion.identity) <= 0.01f && hat.localScale == Vector3.one,
                "Hat consumer attachment must use local zero/identity/unit scale.");
            float positionError = Vector3.Distance(hat.position, head.position);
            float rotationError = Quaternion.Angle(hat.rotation, head.rotation);
            Require(positionError <= 0.0001f && rotationError <= 0.01f, "Hat mount does not match the Head bone; position=" + positionError + " m rotation=" + rotationError + " deg.");
            Require(equipped.GetComponentsInChildren<Collider>(true).Length == 0, "Hat fixture must not add a gameplay collider.");

            hat.gameObject.SetActive(false);
            Require(!hat.gameObject.activeSelf && baseline.GetComponentsInChildren<Renderer>(true).All(r => !r.name.StartsWith("NoviceHat_", StringComparison.Ordinal)),
                "Removing the hat fixture must return to the empty head baseline.");
            hat.gameObject.SetActive(true);
            UnityEngine.Debug.Log("DICEFRE_NOVICE_HAT_SLOT_OK baseline=empty equipped=hat localPosition=" + hat.localPosition + " localRotation=" + hat.localRotation.eulerAngles + " headPositionErrorM=" + positionError.ToString("F5") + " headRotationErrorDeg=" + rotationError.ToString("F3"));
        }

        private static void ValidateHeadMotion(GameObject equipped)
        {
            Transform head = FindUnique(equipped.transform, "Head");
            Transform hat = equipped.transform.GetComponentsInChildren<Transform>(true).Single(t => t.name == "NoviceHatPresentation");
            var animator = equipped.GetComponent<Animator>();
            var clip = new AnimationClip { name = "NoviceHatHeadFollowProbe", hideFlags = HideFlags.HideAndDontSave, legacy = true, wrapMode = WrapMode.ClampForever };
            string path = AnimationUtility.CalculateTransformPath(head, equipped.transform);
            Quaternion restRotation = head.localRotation;
            Quaternion targetRotation = restRotation * Quaternion.Euler(0f, 0f, 24f);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalRotation.x"), AnimationCurve.Linear(0f, restRotation.x, 0.5f, targetRotation.x));
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalRotation.y"), AnimationCurve.Linear(0f, restRotation.y, 0.5f, targetRotation.y));
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalRotation.z"), AnimationCurve.Linear(0f, restRotation.z, 0.5f, targetRotation.z));
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalRotation.w"), AnimationCurve.Linear(0f, restRotation.w, 0.5f, targetRotation.w));
            Vector3 beforePosition = head.position;
            Quaternion beforeRotation = head.rotation;
            bool animatorWasEnabled = animator.enabled;
            AnimationMode.StartAnimationMode();
            try
            {
                // Sample the transient bone-only clip directly. Disabling the
                // Humanoid Animator prevents it from restoring its own pose over
                // this isolated attachment-follow probe.
                animator.enabled = false;
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(equipped, clip, 0.5f);
                AnimationMode.EndSampling();
                float headRotationDelta = Quaternion.Angle(beforeRotation, head.rotation);
                float headPositionDelta = Vector3.Distance(beforePosition, head.position);
                float mountPositionError = Vector3.Distance(hat.position, head.position);
                float mountRotationError = Quaternion.Angle(hat.rotation, head.rotation);
                UnityEngine.Debug.Log("DICEFRE_NOVICE_HAT_HEAD_PROBE path=" + path + " parent=" + head.parent.name + " before=" + beforePosition + " after=" + head.position + " localPosition=" + head.localPosition + " localEuler=" + head.localEulerAngles);
                Require(headRotationDelta >= 10f, "Head follow probe did not animate the imported Head bone; rotation delta=" + headRotationDelta + " deg.");
                Require(headPositionDelta <= 0.01f && mountPositionError <= 0.0001f && mountRotationError <= 0.01f,
                    "Hat attachment did not follow the animated Head bone; head moved " + headPositionDelta + " m, mount error=" + mountPositionError + " m / " + mountRotationError + " deg.");
                UnityEngine.Debug.Log("DICEFRE_NOVICE_HAT_ANIMATION_OK headRotationDeltaDeg=" + headRotationDelta.ToString("F2") + " headPositionDeltaM=" + headPositionDelta.ToString("F5") + " mountPositionErrorM=" + mountPositionError.ToString("F5") + " mountRotationErrorDeg=" + mountRotationError.ToString("F3"));
            }
            finally
            {
                if (AnimationMode.InAnimationMode()) AnimationMode.StopAnimationMode();
                animator.enabled = animatorWasEnabled;
                UnityEngine.Object.DestroyImmediate(clip);
            }
        }

        private static void CreatePreviewScene()
        {
            var previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var cameraObject = new GameObject("NoviceHatPreviewCamera");
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true; camera.orthographicSize = 1.10f; camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.12f, 0.15f, 0.18f); camera.cullingMask = 1 << 31;
                camera.transform.position = new Vector3(2.5f, 1.85f, -5.2f); camera.transform.LookAt(new Vector3(0f, 0.92f, 0f)); camera.tag = "MainCamera";
                var key = new GameObject("NoviceHatPreviewKey").AddComponent<Light>(); key.type = LightType.Directional; key.intensity = 1.15f; key.cullingMask = 1 << 31; key.transform.rotation = Quaternion.Euler(38f, -28f, 0f);
                var fill = new GameObject("NoviceHatPreviewFill").AddComponent<Light>(); fill.type = LightType.Directional; fill.intensity = 0.42f; fill.cullingMask = 1 << 31; fill.transform.rotation = Quaternion.Euler(18f, 145f, 0f);
                var ground = GameObject.CreatePrimitive(PrimitiveType.Plane); ground.name = "PreviewGround_NotGameplay"; ground.layer = 31; ground.transform.position = new Vector3(0f, -0.04f, 0f); ground.transform.localScale = new Vector3(40f, 1f, 40f);
                ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial("NoviceHat_Preview_Ground", new Color(0.19f, 0.22f, 0.24f), 0.10f);
                UnityEngine.Object.DestroyImmediate(ground.GetComponent<Collider>());

                var novicePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NovicePrefabPath);
                var hatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                var baseline = (GameObject)PrefabUtility.InstantiatePrefab(novicePrefab, scene); baseline.name = "NoviceBaseline"; baseline.transform.position = new Vector3(-0.68f, 0f, 0f); SetLayerRecursively(baseline, 31); UnpackNestedPrefabInstances(baseline);
                var equipped = (GameObject)PrefabUtility.InstantiatePrefab(novicePrefab, scene); equipped.name = "NoviceWithHat"; equipped.transform.position = new Vector3(0.68f, 0f, 0f); SetLayerRecursively(equipped, 31); UnpackNestedPrefabInstances(equipped);
                var head = FindUnique(equipped.transform, "Head");
                var hat = (GameObject)PrefabUtility.InstantiatePrefab(hatPrefab, scene); SetLayerRecursively(hat, 31); UnpackNestedPrefabInstances(hat);
                hat.transform.SetParent(head, false); hat.transform.localPosition = Vector3.zero; hat.transform.localRotation = Quaternion.identity; hat.transform.localScale = Vector3.one;
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(previous); }
        }

        private static void CapturePreview()
        {
            var previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                var camera = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<Camera>(true)).Single(c => c.name == "NoviceHatPreviewCamera");
                const int width = 1440, height = 900;
                var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                var previousTarget = camera.targetTexture; var previousActive = RenderTexture.active; var previousEnabled = camera.enabled; var previousCulling = camera.cullingMask;
                var image = new Texture2D(width, height, TextureFormat.RGBA32, false, QualitySettings.activeColorSpace == ColorSpace.Linear);
                try
                {
                    camera.enabled = false; camera.cullingMask = 1 << 31; camera.targetTexture = target;
                    RenderTexture.active = target; camera.Render(); image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply(false, false);
                    File.WriteAllBytes(Path.Combine(ProjectRoot(), PreviewPath), image.EncodeToPNG());
                    AssetDatabase.ImportAsset(PreviewPath, ImportAssetOptions.ForceUpdate);
                }
                finally
                {
                    camera.targetTexture = previousTarget; camera.enabled = previousEnabled; camera.cullingMask = previousCulling;
                    RenderTexture.active = previousActive; UnityEngine.Object.DestroyImmediate(image); target.Release(); UnityEngine.Object.DestroyImmediate(target);
                }
                UnityEngine.Debug.Log("DICEFRE_NOVICE_HAT_PREVIEW_DETAILS baseline-left,empty-head-versus-hat-right " + width + "x" + height);
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(previous); }
        }

        private static object BuildWindows()
        {
            string outputRoot = Path.Combine(Path.GetTempPath(), "DiceFree-Hat-Issue42");
            Directory.CreateDirectory(outputRoot);
            string output = Path.Combine(outputRoot, "NoviceHatValidation.exe");
            var options = new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = output, target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            bool success = report.summary.result == BuildResult.Succeeded;
            string marker = success ? "DICEFRE_NOVICE_HAT_PLAYER_BUILD_OK" : "DICEFRE_NOVICE_HAT_PLAYER_BUILD_FAIL";
            UnityEngine.Debug.Log(marker + " result=" + report.summary.result + " errors=" + report.summary.totalErrors + " warnings=" + report.summary.totalWarnings + " output=" + output + " bytes=" + report.summary.totalSize);
            return new { success, validation = "novice-hat-player-build", finalMarker = marker, output, outputBytes = report.summary.totalSize, errors = report.summary.totalErrors, warnings = report.summary.totalWarnings, testedGitSha = GitHead() };
        }

        private static Material CreateMaterial(string name, Color color, float smoothness)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name }; AssetDatabase.CreateAsset(material, path); }
            material.shader = Shader.Find("Universal Render Pipeline/Lit"); material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", smoothness); EditorUtility.SetDirty(material);
            return material;
        }

        private static Bounds MeasureBounds(GameObject model)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            try
            {
                var renderers = instance.GetComponentsInChildren<Renderer>(true);
                Require(renderers.Length > 0, "Hat model has no renderers.");
                Bounds result = new Bounds(instance.transform.InverseTransformPoint(renderers[0].bounds.center), Vector3.zero);
                bool initialized = false;
                foreach (var renderer in renderers)
                {
                    Bounds bounds = renderer.bounds;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3 corner = new Vector3((i & 1) == 0 ? bounds.min.x : bounds.max.x, (i & 2) == 0 ? bounds.min.y : bounds.max.y, (i & 4) == 0 ? bounds.min.z : bounds.max.z);
                        Vector3 local = instance.transform.InverseTransformPoint(corner);
                        if (!initialized) { result = new Bounds(local, Vector3.zero); initialized = true; } else result.Encapsulate(local);
                    }
                }
                return result;
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }

        private static void UnpackNestedPrefabInstances(GameObject root)
        {
            while (true)
            {
                var instanceRoots = root.GetComponentsInChildren<Transform>(true)
                    .Where(t => PrefabUtility.IsPartOfPrefabInstance(t.gameObject))
                    .Select(t => PrefabUtility.GetNearestPrefabInstanceRoot(t.gameObject))
                    .Where(r => r != null).Distinct().ToArray();
                if (instanceRoots.Length == 0) return;
                foreach (var instanceRoot in instanceRoots)
                    if (PrefabUtility.IsPartOfPrefabInstance(instanceRoot)) PrefabUtility.UnpackPrefabInstance(instanceRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }

        private static Transform FindUnique(Transform root, string name)
        {
            var matches = root.GetComponentsInChildren<Transform>(true).Where(t => t.name == name).ToArray();
            Require(matches.Length == 1, "Expected one transform named " + name + "; found " + matches.Length + ".");
            return matches[0];
        }

        private static void SetLayerRecursively(GameObject root, int layer) { root.layer = layer; foreach (Transform child in root.transform) SetLayerRecursively(child.gameObject, layer); }
        private static void EnsureFolder(string path) { if (AssetDatabase.IsValidFolder(path)) return; string parent = Path.GetDirectoryName(path).Replace('\\', '/'); string name = Path.GetFileName(path); if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent); AssetDatabase.CreateFolder(parent, name); }
        private static string ProjectRoot() => Directory.GetParent(Application.dataPath).FullName;
        private static string GitHead() => Environment.GetEnvironmentVariable("GIT_COMMIT") ?? string.Empty;
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
