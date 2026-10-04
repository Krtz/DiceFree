using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;

namespace DiceFree.EditorTools
{
    /// <summary>Focused Editor proof for the first DiceFree character rig and presentation pipeline.</summary>
    internal static class NoviceArtPipelineValidation
    {
        private const string SourceRoot = "SourceArt/Characters/Novice";
        private const string ModelPath = "Assets/_DiceFree/Art/Characters/Novice/Models/Novice.fbx";
        private const string MaterialRoot = "Assets/_DiceFree/Art/Characters/Novice/Materials";
        private const string PrefabPath = "Assets/_DiceFree/Art/Characters/Novice/Prefabs/NovicePresentation.prefab";
        private const string ControllerPath = "Assets/_DiceFree/Art/Characters/Novice/Animations/Novice.controller";
        private const string ScenePath = "Assets/_DiceFree/Art/Validation/Scenes/NoviceCharacterPipelinePreview.unity";
        private const string PreviewPath = "Assets/_DiceFree/Art/Validation/Previews/NoviceCharacter_Unity.png";
        private const string SwordPath = "Assets/_DiceFree/Art/Weapons/CornbergFieldSword/Prefabs/CornbergFieldSwordPresentation.prefab";
        private const string Marker = "DICEFRE_NOVICE_PIPELINE_OK";

        [CliCommand("dicefree.art.novice.prepare", "Import and create the Novice character presentation proof assets.", Tags = new[] { "art", "dicefree/art" })]
        private static object PrepareCommand() => Run("novice-prepare", "DICEFRE_NOVICE_PREPARED", PrepareAssets);

        [CliCommand("dicefree.art.novice.validate", "Validate the Novice source, humanoid import, baseline slots, rig sockets and sword attachment fixture.", Tags = new[] { "tests", "dicefree/art" })]
        private static object ValidateCommand() => Run("novice-art", Marker, ValidateAssets);

        [CliCommand("dicefree.art.novice.capture-preview", "Render the two-pose Novice validation scene to its checked-in PNG.", Tags = new[] { "art", "capture", "dicefree/art" })]
        private static object CapturePreviewCommand() => Run("novice-preview-capture", "DICEFRE_NOVICE_PREVIEW_OK", CapturePreviewAsset);

        [CliCommand("dicefree.art.novice.build-windows", "Build the isolated Novice preview scene as a Windows Development player.", Tags = new[] { "builds", "dicefree/art" })]
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
                UnityEngine.Debug.LogError("DICEFRE_NOVICE_PIPELINE_FAIL\n" + error);
            }
            timer.Stop();
            return new
            {
                success = error == null,
                validation = name,
                finalMarker = error == null ? marker : "DICEFRE_NOVICE_PIPELINE_FAIL",
                elapsedSeconds = timer.Elapsed.TotalSeconds,
                error,
                sourceRoot = SourceRoot,
                modelPath = ModelPath,
                prefabPath = PrefabPath,
                previewScenePath = ScenePath,
                previewPath = PreviewPath,
                testedGitSha = GitHead()
            };
        }

        private static void PrepareAssets()
        {
            EnsureFolder(MaterialRoot);
            EnsureFolder("Assets/_DiceFree/Art/Characters/Novice/Prefabs");
            EnsureFolder("Assets/_DiceFree/Art/Characters/Novice/Animations");
            EnsureFolder("Assets/_DiceFree/Art/Validation/Scenes");
            EnsureFolder("Assets/_DiceFree/Art/Validation/Previews");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            Require(importer != null, "Novice FBX has no ModelImporter: " + ModelPath);
            importer.globalScale = 1f;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.addCollider = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importNormals = ModelImporterNormals.Import;
            importer.preserveHierarchy = true;
            importer.SaveAndReimport();
            AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceSynchronousImport);

            var imported = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            Require(imported != null, "Unity did not import the Novice model.");
            var clips = ImportedClips().ToArray();
            Require(clips.Any(c => c.name.Contains("Idle")), "FBX Idle animation did not import.");
            Require(clips.Any(c => c.name.Contains("Locomotion")), "FBX Locomotion animation did not import.");
            Require(clips.Any(c => c.name.Contains("UnarmedAttack")), "FBX UnarmedAttack animation did not import.");
            var animator = imported.GetComponent<Animator>();
            Require(animator != null && animator.avatar != null && animator.avatar.isValid && animator.avatar.isHuman,
                "Unity did not generate a valid Humanoid Avatar from the Novice rig.");

            var materials = new Dictionary<string, Material>
            {
                ["Novice_Skin"] = CreateMaterial("Novice_Skin", new Color(0.66f, 0.48f, 0.36f), 0.24f),
                ["Novice_Baseline_Tee"] = CreateMaterial("Novice_Baseline_Tee", new Color(0.22f, 0.39f, 0.41f), 0.30f),
                ["Novice_Baseline_Underwear"] = CreateMaterial("Novice_Baseline_Underwear", new Color(0.29f, 0.25f, 0.24f), 0.34f),
                ["Novice_Eyes"] = CreateMaterial("Novice_Eyes", new Color(0.08f, 0.07f, 0.065f), 0.38f)
            };
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(imported);
            instance.name = "NovicePresentation";
            foreach (var renderer in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                renderer.sharedMaterials = renderer.sharedMaterials.Select(old => ResolveMaterial(renderer, old, materials)).ToArray();
            }
            var controller = CreateController(clips);
            var instanceAnimator = instance.GetComponent<Animator>();
            instanceAnimator.runtimeAnimatorController = controller;
            instanceAnimator.applyRootMotion = false;
            instanceAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            Transform rightHand = FindBone(instance.transform, "RightHand");
            Transform leftHand = FindBone(instance.transform, "LeftHand");
            // Preserve the intended character-space bind-pose presentation, but
            // convert it through the imported hand bind rotation. The prefab
            // socket, rather than the preview scene, owns the final orientation.
            Quaternion weaponForwardInCharacterSpace = Quaternion.Euler(-135f, 0f, 0f);
            Quaternion weaponSocketLocalRotation = Quaternion.Inverse(rightHand.rotation) *
                (instance.transform.rotation * weaponForwardInCharacterSpace);
            CreateSocket(rightHand, "RightHandWeapon", new Vector3(0f, 0.025f, 0f), weaponSocketLocalRotation);
            // The authored Novice faces Blender +Y, which the tested FBX basis
            // maps to Unity local -Z. Keep the shield's front socket aligned
            // with that actual character-forward direction.
            Quaternion offhandForwardInCharacterSpace = Quaternion.LookRotation(Vector3.back, Vector3.up);
            Quaternion offhandSocketLocalRotation = Quaternion.Inverse(leftHand.rotation) *
                (instance.transform.rotation * offhandForwardInCharacterSpace);
            CreateSocket(leftHand, "LeftHandOffhand", new Vector3(0f, 0.025f, 0f), offhandSocketLocalRotation);
            // The humanoid Head and Foot bones themselves are the canonical
            // semantic anchors. Keep their required names unique; later wearable
            // assets can carry their own small offsets from these bone origins.

            var slotRoot = new GameObject("BaselineVisualSlots").transform;
            slotRoot.SetParent(instance.transform, false);
            new GameObject("BaselineTShirtSlot").transform.SetParent(slotRoot, false);
            new GameObject("BaselineUnderwearSlot").transform.SetParent(slotRoot, false);

            PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
            UnityEngine.Object.DestroyImmediate(instance);
            CreatePreviewScene();
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("DICEFRE_NOVICE_PREPARED_DETAILS avatar=Humanoid clips=" + string.Join(",", clips.Select(c => c.name).ToArray()) + " materials=" + materials.Count);
        }

        private static void ValidateAssets()
        {
            var root = ProjectRoot();
            Require(File.Exists(Path.Combine(root, SourceRoot, "Novice.blend")), "Editable Novice.blend source is missing.");
            Require(File.Exists(Path.Combine(root, SourceRoot, "build_novice.py")), "Novice reproducible Blender export script is missing.");
            Require(File.Exists(Path.Combine(root, SourceRoot, "novice_body_profile.json")), "Explicit authored Novice body profile is missing.");
            Require(File.Exists(Path.Combine(root, ModelPath)), "Novice FBX export is missing.");
            Require(Path.GetFileNameWithoutExtension(ModelPath) == "Novice", "Source and export names must match the Novice identity.");
            Require(File.Exists(Path.Combine(root, PrefabPath)), "Novice presentation prefab is missing.");
            Require(File.Exists(Path.Combine(root, ControllerPath)), "Novice animation controller is missing.");
            Require(File.Exists(Path.Combine(root, ScenePath)), "Novice art-preview scene is missing.");

            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            Require(importer != null, "ModelImporter is missing.");
            Require(Mathf.Approximately(importer.globalScale, 1f), "Novice global scale must be 1.");
            Require(importer.animationType == ModelImporterAnimationType.Human && importer.avatarSetup == ModelImporterAvatarSetup.CreateFromThisModel,
                "Novice import must create its own Humanoid Avatar.");
            Require(importer.importAnimation && !importer.importCameras && !importer.importLights && !importer.importBlendShapes && !importer.addCollider,
                "Novice importer animation/camera/light/blend-shape/collider settings are not appropriate.");
            Require(importer.materialImportMode == ModelImporterMaterialImportMode.None, "FBX embedded materials must not be imported.");

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Require(model != null && prefab != null, "Imported model or presentation prefab could not be loaded.");
            var importedTransforms = model.GetComponentsInChildren<Transform>(true);
            Require(!importedTransforms.Any(t => t.name == "RightHandWeapon" || t.name == "LeftHandOffhand"),
                "DCC reference socket empties are helpers only and must not be exported in the FBX.");
            var animator = prefab.GetComponent<Animator>();
            Require(animator != null && animator.avatar != null && animator.avatar.isValid && animator.avatar.isHuman,
                "Presentation prefab does not contain a valid Humanoid Avatar.");
            var skin = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).ToArray();
            Require(skin.Length == 4, "Expected separate body skin, face details, T-shirt and underwear skinned meshes; got " + skin.Length + ".");
            Require(skin.Any(r => r.name == "Novice_BodySkin") && skin.Any(r => r.name == "Novice_FaceDetails") && skin.Any(r => r.name == "Novice_Baseline_TShirt") && skin.Any(r => r.name == "Novice_Baseline_Underwear"),
                "Body skin, readable face details, baseline T-shirt and underwear must remain separate presentation meshes.");
            foreach (var renderer in skin)
            {
                Require(renderer.sharedMesh != null && renderer.sharedMesh.vertexCount > 0, renderer.name + " has no mesh.");
                Require(renderer.rootBone != null && renderer.bones.Length > 0 && renderer.bones.All(b => b != null), renderer.name + " has a missing rig bone.");
                Require(renderer.sharedMaterials.Length == renderer.sharedMesh.subMeshCount && renderer.sharedMaterials.All(m => m != null), renderer.name + " has missing material assignments.");
                Require(renderer.sharedMesh.boneWeights.Length == renderer.sharedMesh.vertexCount && renderer.sharedMesh.boneWeights.Any(w => w.weight0 > 0f), renderer.name + " has no usable skin weights.");
            }
            Require(prefab.GetComponentsInChildren<Collider>(true).Length == 0, "Art presentation prefab must not contain gameplay/render-derived colliders.");

            var required = new Dictionary<string, string>
            {
                ["RightHandWeapon"] = "RightHand", ["LeftHandOffhand"] = "LeftHand", ["Head"] = "Neck",
                ["LeftFoot"] = "LeftLeg", ["RightFoot"] = "RightLeg"
            };
            foreach (var pair in required)
            {
                var socket = prefab.GetComponentsInChildren<Transform>(true).Where(t => t.name == pair.Key).ToArray();
                Require(socket.Length == 1, "Required semantic socket must be unique: " + pair.Key + " count=" + socket.Length);
                Require(socket[0].parent != null && socket[0].parent.name == pair.Value, pair.Key + " must be directly rig-relative to " + pair.Value + ".");
            }
            var prefabWeaponSocket = prefab.GetComponentsInChildren<Transform>(true).Single(t => t.name == "RightHandWeapon");
            Require(prefabWeaponSocket.localPosition == new Vector3(0f, 0.025f, 0f),
                "RightHandWeapon local position no longer matches the authored hand-grip offset.");
            Require(Quaternion.Angle(prefabWeaponSocket.localRotation, Quaternion.identity) > 1f,
                "RightHandWeapon must own the non-identity bind-pose orientation needed by the sword.");
            UnityEngine.Debug.Log("DICEFRE_NOVICE_SOCKET_AUTHORING localPosition=" + prefabWeaponSocket.localPosition +
                " localRotation=" + prefabWeaponSocket.localRotation.eulerAngles +
                " localRotationQuaternion=" + prefabWeaponSocket.localRotation);
            var prefabLeftHand = FindBone(prefab.transform, "LeftHand");
            var prefabOffhandSocket = prefab.GetComponentsInChildren<Transform>(true).Single(t => t.name == "LeftHandOffhand");
            float offhandForwardError = Vector3.Angle(prefabOffhandSocket.forward, -prefab.transform.forward);
            Require(Vector3.Distance(prefabOffhandSocket.localPosition, new Vector3(0f, 0.025f, 0f)) <= 0.0001f &&
                prefabOffhandSocket.parent == prefabLeftHand && offhandForwardError <= 0.1f,
                "LeftHandOffhand must keep its hand-relative grip point while its authored forward axis matches character forward; angle=" + offhandForwardError + " degrees.");
            UnityEngine.Debug.Log("DICEFRE_NOVICE_OFFHAND_SOCKET forwardErrorDeg=" + offhandForwardError.ToString("F3") +
                " localPosition=" + prefabOffhandSocket.localPosition + " localRotation=" + prefabOffhandSocket.localRotation.eulerAngles);
            Require(prefab.transform.Find("BaselineVisualSlots/BaselineTShirtSlot") != null && prefab.transform.Find("BaselineVisualSlots/BaselineUnderwearSlot") != null,
                "Separate baseline clothing slot roots are missing.");
            var clips = ImportedClips().ToArray();
            foreach (string part in new[] { "Idle", "Locomotion", "UnarmedAttack" })
                Require(clips.Any(c => c.name.Contains(part) && c.length > 0.1f), "Animation clip is missing or empty: " + part);

            var path = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var roots = path.GetRootGameObjects();
                var presentations = roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Where(t => t.name == "NovicePresentation").ToArray();
                Require(presentations.Length == 2, "Preview must show baseline Novice and sword-fixture Novice.");
                var sword = presentations[1].GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "CornbergFieldSwordPresentation");
                Require(sword != null, "Sword fixture is not attached to the second Novice presentation.");
                var weaponSocket = presentations[1].GetComponentsInChildren<Transform>(true).Single(t => t.name == "RightHandWeapon");
                var grip = sword.transform.Find("Grip");
                var tip = sword.transform.Find("Tip");
                Require(grip != null, "Sword presentation wrapper has no canonical Grip alias.");
                Require(tip != null, "Sword presentation wrapper has no canonical Tip alias.");
                Require(Vector3.Distance(sword.transform.localPosition, Vector3.zero) <= 0.0001f &&
                    Quaternion.Angle(sword.transform.localRotation, Quaternion.identity) <= 0.01f &&
                    Vector3.Distance(sword.transform.localScale, Vector3.one) <= 0.0001f,
                    "Preview sword must use the semantic socket contract with local position zero, rotation identity and scale one.");
                float gripError = Vector3.Distance(grip.position, weaponSocket.position);
                Require(gripError <= 0.03f, "Sword Grip does not align with the RightHandWeapon socket; distance=" + gripError + "m.");
                float orientationError = Vector3.Angle(weaponSocket.forward, (tip.position - grip.position).normalized);
                Require(orientationError <= 5f,
                    "Sword Grip-to-Tip direction does not follow RightHandWeapon forward; angle=" + orientationError + " degrees.");
                UnityEngine.Debug.Log("DICEFRE_NOVICE_SWORD_SOCKET gripErrorM=" + gripError.ToString("F5") +
                    " gripTipSocketForwardErrorDeg=" + orientationError.ToString("F3") +
                    " localPosition=" + sword.transform.localPosition + " localRotation=" + sword.transform.localRotation.eulerAngles +
                    " localScale=" + sword.transform.localScale);

                var locomotion = clips.First(c => c.name.Contains("Locomotion"));
                var animatedArm = FindBone(presentations[0], "LeftArm");
                Quaternion restRotation = animatedArm.localRotation;
                // Bake the scene instance that AnimationMode samples, rather
                // than the prefab asset's source component transforms.
                var bodySkin = presentations[0].GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .First(r => r.name == "Novice_BodySkin");
                var restMesh = new Mesh { name = "NoviceSkinRestValidation" };
                var animatedMesh = new Mesh { name = "NoviceSkinAnimatedValidation" };
                try
                {
                    bodySkin.BakeMesh(restMesh);
                    var restVertices = restMesh.vertices;
                    AnimationMode.StartAnimationMode();
                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(presentations[0].gameObject, locomotion, locomotion.length * 0.5f);
                    AnimationMode.EndSampling();
                    Require(Quaternion.Angle(restRotation, animatedArm.localRotation) > 0.5f,
                        "Locomotion clip imported but did not animate the humanoid arm bone.");
                    bodySkin.BakeMesh(animatedMesh);
                    var animatedVertices = animatedMesh.vertices;
                    Require(restVertices.Length == animatedVertices.Length, "Baked skinned mesh vertex count changed during animation sampling.");
                    int deformedVertices = 0;
                    float maximumVertexDelta = 0f;
                    for (int i = 0; i < restVertices.Length; i++)
                    {
                        float delta = Vector3.Distance(restVertices[i], animatedVertices[i]);
                        if (delta > 0.001f) deformedVertices++;
                        if (delta > maximumVertexDelta) maximumVertexDelta = delta;
                    }
                    Require(deformedVertices >= 10 && maximumVertexDelta >= 0.01f,
                        "Locomotion moved bones but produced no meaningful baked skinned-mesh deformation; changedVertices=" +
                        deformedVertices + " maxDelta=" + maximumVertexDelta + "m.");
                    UnityEngine.Debug.Log("DICEFRE_NOVICE_SKIN_DEFORMATION_OK renderer=" + bodySkin.name +
                        " changedVertices=" + deformedVertices + "/" + restVertices.Length +
                        " maxDeltaM=" + maximumVertexDelta.ToString("F5"));
                }
                finally
                {
                    if (AnimationMode.InAnimationMode()) AnimationMode.StopAnimationMode();
                    UnityEngine.Object.DestroyImmediate(restMesh);
                    UnityEngine.Object.DestroyImmediate(animatedMesh);
                }
            }
            finally { EditorSceneManager.CloseScene(path, true); }

            var filters = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            int vertices = filters.Sum(r => r.sharedMesh.vertexCount);
            int triangles = filters.Sum(r => r.sharedMesh.triangles.Length / 3);
            int materials = filters.SelectMany(r => r.sharedMaterials).Distinct().Count();
            var bounds = MeasureBounds(prefab);
            Require(bounds.size.y > 1.55f && bounds.size.y < 2.1f, "Novice bounds height is outside the expected human-scale range: " + bounds.size);
            Require(vertices > 0 && triangles > 0 && materials >= 4, "Novice geometry/material counts are empty or missing one of the presentation materials.");
            UnityEngine.Debug.Log("DICEFRE_NOVICE_STATS dimensions=" + bounds.size + " vertices=" + vertices + " triangles=" + triangles + " skinnedMeshes=" + skin.Length + " materials=" + materials);
        }

        private static void CreatePreviewScene()
        {
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var cameraObject = new GameObject("NoviceArtPreviewCamera");
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 1.18f;
                camera.backgroundColor = new Color(0.12f, 0.15f, 0.18f);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.cullingMask = 1 << 31;
                camera.transform.position = new Vector3(2.6f, 2.15f, -5.3f);
                camera.transform.LookAt(new Vector3(0f, 0.88f, 0f));
                camera.tag = "MainCamera";

                var keyObject = new GameObject("NoviceArtPreviewKeyLight");
                var light = keyObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.shadows = LightShadows.None;
                light.transform.rotation = Quaternion.Euler(38f, -28f, 0f);
                var fillObject = new GameObject("NoviceArtPreviewFillLight");
                var fill = fillObject.AddComponent<Light>();
                fill.type = LightType.Directional;
                fill.intensity = 0.45f;
                fill.shadows = LightShadows.None;
                fill.transform.rotation = Quaternion.Euler(18f, 145f, 0f);

                var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "PreviewGround_NotGameplay";
                ground.transform.position = new Vector3(0f, -0.04f, 0f);
                ground.transform.localScale = new Vector3(40f, 1f, 40f);
                ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Novice_Preview_Ground", new Color(0.19f, 0.22f, 0.24f), 0.86f);
                UnityEngine.Object.DestroyImmediate(ground.GetComponent<Collider>());

                var model = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                var baseline = (GameObject)PrefabUtility.InstantiatePrefab(model, scene);
                baseline.name = "NovicePresentation";
                baseline.transform.position = new Vector3(-0.67f, 0f, 0f);
                baseline.transform.rotation = Quaternion.identity;
                var swordNovice = (GameObject)PrefabUtility.InstantiatePrefab(model, scene);
                swordNovice.name = "NovicePresentation";
                swordNovice.transform.position = new Vector3(0.67f, 0f, 0f);
                swordNovice.transform.rotation = Quaternion.identity;
                var socket = swordNovice.GetComponentsInChildren<Transform>(true).Single(t => t.name == "RightHandWeapon");
                var swordPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SwordPath);
                Require(swordPrefab != null, "Accepted Cornberg Field Sword presentation prefab is missing.");
                var sword = (GameObject)PrefabUtility.InstantiatePrefab(swordPrefab, scene);
                sword.name = "CornbergFieldSwordPresentation";
                sword.transform.SetParent(socket, false);
                sword.transform.localPosition = Vector3.zero;
                sword.transform.localRotation = Quaternion.identity;
                foreach (var renderer in sword.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
                foreach (var previewRoot in scene.GetRootGameObjects()) SetLayerRecursively(previewRoot, 31);
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        private static void CapturePreviewAsset()
        {
            ValidateAssets();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            Camera camera = null;
            RenderTexture target = null;
            Texture2D image = null;
            var prior = RenderTexture.active;
            try
            {
                camera = scene.GetRootGameObjects().Select(o => o.GetComponent<Camera>()).FirstOrDefault(c => c != null);
                Require(camera != null, "Novice preview scene has no camera.");
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
                UnityEngine.Debug.Log("DICEFRE_NOVICE_PREVIEW details=baseline-left,sword-fixture-right size=1440x900 camera=dedicated-orthographic");
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
            var output = Path.Combine(Path.GetTempPath(), "DiceFree-Novice-Issue39", "NoviceValidation.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };
            var report = BuildPipeline.BuildPlayer(options);
            timer.Stop();
            bool success = report.summary.result == BuildResult.Succeeded;
            UnityEngine.Debug.Log((success ? "DICEFRE_NOVICE_PLAYER_BUILD_OK" : "DICEFRE_NOVICE_PLAYER_BUILD_FAIL") + " result=" + report.summary.result + " errors=" + report.summary.totalErrors + " warnings=" + report.summary.totalWarnings + " output=" + output);
            return new { success, validation = "novice-player-build", finalMarker = success ? "DICEFRE_NOVICE_PLAYER_BUILD_OK" : "DICEFRE_NOVICE_PLAYER_BUILD_FAIL", elapsedSeconds = timer.Elapsed.TotalSeconds, outputPath = output, outputBytes = Directory.Exists(Path.GetDirectoryName(output)) ? Directory.GetFiles(Path.GetDirectoryName(output), "*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length) : 0, errors = report.summary.totalErrors, warnings = report.summary.totalWarnings, testedGitSha = GitHead() };
        }

        private static AnimatorController CreateController(AnimationClip[] clips)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller != null) AssetDatabase.DeleteAsset(ControllerPath);
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var idle = clips.First(c => c.name.Contains("Idle"));
            var locomotion = clips.First(c => c.name.Contains("Locomotion"));
            var attack = clips.First(c => c.name.Contains("UnarmedAttack"));
            var layer = controller.layers[0];
            var machine = layer.stateMachine;
            var idleState = machine.AddState("Idle");
            idleState.motion = idle;
            machine.defaultState = idleState;
            var moveState = machine.AddState("Locomotion");
            moveState.motion = locomotion;
            var attackState = machine.AddState("UnarmedAttack");
            attackState.motion = attack;
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            var outMove = idleState.AddTransition(moveState);
            outMove.hasExitTime = false;
            outMove.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            var inIdle = moveState.AddTransition(idleState);
            inIdle.hasExitTime = false;
            inIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            var toAttack = machine.AddAnyStateTransition(attackState);
            toAttack.hasExitTime = false;
            toAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            var fromAttack = attackState.AddTransition(idleState);
            fromAttack.hasExitTime = true;
            fromAttack.exitTime = 0.95f;
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static Material ResolveMaterial(SkinnedMeshRenderer renderer, Material old, Dictionary<string, Material> materials)
        {
            string name = old != null ? old.name : string.Empty;
            if (renderer.name.IndexOf("FaceDetails", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Eyes", StringComparison.OrdinalIgnoreCase) >= 0) return materials["Novice_Eyes"];
            if (renderer.name.IndexOf("TShirt", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Tee", StringComparison.OrdinalIgnoreCase) >= 0) return materials["Novice_Baseline_Tee"];
            if (renderer.name.IndexOf("Underwear", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Underwear", StringComparison.OrdinalIgnoreCase) >= 0) return materials["Novice_Baseline_Underwear"];
            return materials["Novice_Skin"];
        }

        private static Material CreateMaterial(string name, Color color, float smoothness)
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
            material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static IEnumerable<AnimationClip> ImportedClips() => AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal));

        private static Transform FindBone(Transform root, string name)
        {
            var matches = root.GetComponentsInChildren<Transform>(true).Where(t => t.name == name).ToArray();
            Require(matches.Length == 1, "Expected exactly one rig bone named " + name + ", found " + matches.Length + ".");
            return matches[0];
        }

        private static Transform CreateSocket(Transform bone, string name, Vector3 position, Quaternion rotation)
        {
            var socket = new GameObject(name).transform;
            socket.SetParent(bone, false);
            socket.localPosition = position;
            socket.localRotation = rotation;
            return socket;
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            foreach (Transform child in root.transform) SetLayerRecursively(child.gameObject, layer);
        }

        private static Bounds MeasureBounds(GameObject prefab)
        {
            var renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(r => r.sharedMesh != null).ToArray();
            Require(renderers.Length > 0, "No skinned renderers found.");
            bool hasPoint = false;
            var bounds = new Bounds(prefab.transform.position, Vector3.zero);
            foreach (var renderer in renderers)
            {
                Bounds meshBounds = renderer.sharedMesh.bounds;
                for (int mask = 0; mask < 8; mask++)
                {
                    var corner = new Vector3(
                        (mask & 1) == 0 ? meshBounds.min.x : meshBounds.max.x,
                        (mask & 2) == 0 ? meshBounds.min.y : meshBounds.max.y,
                        (mask & 4) == 0 ? meshBounds.min.z : meshBounds.max.z);
                    Vector3 point = prefab.transform.InverseTransformPoint(renderer.transform.TransformPoint(corner));
                    if (!hasPoint) { bounds = new Bounds(point, Vector3.zero); hasPoint = true; }
                    else bounds.Encapsulate(point);
                }
            }
            return bounds;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static string ProjectRoot() => Directory.GetParent(Application.dataPath).FullName;
        private static string GitHead() => Environment.GetEnvironmentVariable("GIT_COMMIT") ?? string.Empty;
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
