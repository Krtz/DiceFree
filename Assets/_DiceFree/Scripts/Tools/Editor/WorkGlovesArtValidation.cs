using System;
using System.IO;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;

namespace DiceFree.EditorTools
{
    internal static class WorkGlovesArtValidation
    {
        internal const string ItemId = "item.cornberg.work-gloves";
        internal const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        internal const string PreviewPath = "Assets/_DiceFree/Art/Validation/Previews/CornbergWorkGloves_Unity.png";
        private const string ArtRoot = "Assets/_DiceFree/Art/Characters/CornbergWorkGloves";
        private const string ModelPath = ArtRoot + "/Models/CornbergWorkGloves.fbx";
        private const string PrefabPath = ArtRoot + "/Prefabs/CornbergWorkGlovesPresentation.prefab";

        [CliCommand("dicefree.art.gloves.prepare", "Import the work gloves and bind them to the actual Cornberg Novice rig.")]
        private static object PrepareCommand() => Run(Prepare, "WORK_GLOVES_PREPARED");
        [CliCommand("dicefree.art.gloves.validate", "Validate source/import, hand weights, animation propagation and baseline preservation.")]
        private static object ValidateCommand() => Run(Validate, "WORK_GLOVES_ART_OK");
        [CliCommand("dicefree.art.gloves.capture-preview", "Capture baseline, equipped idle, locomotion and attack Novice fixtures.")]
        private static object PreviewCommand() => Run(CapturePreview, "WORK_GLOVES_PREVIEW_OK");

        private static object Run(Action action, string marker)
        {
            try { action(); Debug.Log(marker); return new { success = true, finalMarker = marker, previewPath = PreviewPath }; }
            catch (Exception e) { Debug.LogException(e); return new { success = false, error = e.ToString() }; }
        }

        private static void Prepare()
        {
            Require(!EditorApplication.isPlaying, "Prepare requires Edit Mode.");
            Directory.CreateDirectory(ArtRoot + "/Materials");
            Directory.CreateDirectory(ArtRoot + "/Prefabs");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var importer = (ModelImporter)AssetImporter.GetAtPath(ModelPath);
            importer.globalScale = 1;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.importAnimation = false;
            importer.importCameras = false; importer.importLights = false;
            importer.importBlendShapes = false; importer.addCollider = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importNormals = ModelImporterNormals.Import;
            importer.isReadable = true;
            importer.SaveAndReimport();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var wrapper = new GameObject("CornbergWorkGlovesPresentation");
            var imported = (GameObject)PrefabUtility.InstantiatePrefab(model);
            imported.transform.SetParent(wrapper.transform, false);
            var leather = Material("WorkGloves_WornLeather", new Color(.37f, .22f, .105f));
            var canvas = Material("WorkGloves_CanvasCuff", new Color(.58f, .44f, .26f));
            foreach (var r in imported.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                r.sharedMaterials = new[] { r.name.EndsWith("Cuff", StringComparison.Ordinal) ? canvas : leather };
                r.updateWhenOffscreen = true;
            }
            PrefabUtility.SaveAsPrefabAsset(wrapper, PrefabPath);
            UnityEngine.Object.DestroyImmediate(wrapper);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var player = Player(scene);
            var novice = player.GetComponent<NovicePresentationDriver>().VisualRoot;
            var old = novice.Find("CornbergWorkGloves");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var gloves = Bind(novice);
            var binding = player.GetComponent<EquipmentPresentation>() ?? player.AddComponent<EquipmentPresentation>();
            binding.Configure(new[] { new EquipmentPresentation.Binding { slot = EquipmentSlot.Hands,
                definition = player.GetComponent<CarriedInventory>().Resolve(ItemId), visuals = new[] { gloves } } });
            gloves.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);
            Require(EditorSceneManager.SaveScene(scene), "Could not save glove binding.");
            AssetDatabase.SaveAssets();
        }

        internal static GameObject Bind(Transform novice)
        {
            // Retain the same source-rig bind poses. Resolve the exported rig once
            // during authoring; runtime presentation never searches bone names.
            var root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), novice.gameObject.scene);
            PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            root.name = "CornbergWorkGloves";
            root.transform.SetParent(novice, false);
            var bones = novice.GetComponentsInChildren<Transform>(true)
                .Where(t => !t.IsChildOf(root.transform)).GroupBy(t => t.name).ToDictionary(g => g.Key, g => g.First());
            var skins = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var skin in skins)
            {
                skin.bones = skin.bones.Select(b => bones[b.name]).ToArray();
                skin.rootBone = bones[skin.rootBone.name];
                skin.transform.SetParent(root.transform, true);
            }
            foreach (var child in root.transform.Cast<Transform>().Where(t => t.GetComponent<SkinnedMeshRenderer>() == null).ToArray())
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            return root;
        }

        private static Material Material(string name, Color color)
        {
            string path = ArtRoot + "/Materials/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); }
            mat.SetColor("_BaseColor", color); mat.SetFloat("_Smoothness", .1f);
            mat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Back);
            EditorUtility.SetDirty(mat); return mat;
        }

        internal static GameObject Player(Scene scene) => scene.GetRootGameObjects().Single(g => g.GetComponent<NovicePresentationDriver>() != null);

        private static void Validate()
        {
            foreach (string file in new[] { "build_work_gloves.py", "CornbergWorkGloves.blend", "export_manifest.json" })
                Require(File.Exists("SourceArt/Characters/CornbergWorkGloves/" + file), "Missing source " + file);
            var importer = (ModelImporter)AssetImporter.GetAtPath(ModelPath);
            Require(importer.globalScale == 1 && !importer.importAnimation && !importer.addCollider && !importer.importCameras &&
                !importer.importLights && !importer.importBlendShapes && importer.materialImportMode == ModelImporterMaterialImportMode.None,
                "Unexpected glove importer configuration.");
            Require(!AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().Any(), "Gloves imported animation.");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var player = Player(scene);
            var binding = player.GetComponent<EquipmentPresentation>().Bindings.Single();
            Require(binding.slot == EquipmentSlot.Hands && binding.definition.stableId == ItemId, "Wrong authored item/slot binding.");
            var gloves = binding.visuals.Single();
            Require(!gloves.activeSelf, "Saved baseline must have no visible gloves.");
            var novice = player.GetComponent<NovicePresentationDriver>().VisualRoot;
            var body = novice.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single(r => r.name == "Novice_BodySkin");
            Require(body.enabled && novice.GetComponentsInChildren<SkinnedMeshRenderer>(true).Count(r => !r.transform.IsChildOf(gloves.transform)) == 4,
                "The four original Novice baseline meshes must be preserved.");
            Require(gloves.GetComponentsInChildren<Collider>(true).Length == 0 && gloves.GetComponentsInChildren<Animator>(true).Length == 0,
                "Gloves must share the Novice Animator and add no gameplay collider.");
            var clone = UnityEngine.Object.Instantiate(novice.gameObject);
            try
            {
                var shell = clone.transform.Find("CornbergWorkGloves").gameObject;
                shell.SetActive(true);
                ValidateSkin(clone, shell);
            }
            finally { UnityEngine.Object.DestroyImmediate(clone); }
        }

        internal static void ValidateSkin(GameObject novice, GameObject shell)
        {
            var animator = novice.GetComponentInChildren<Animator>();
            animator.Rebind(); animator.Update(0);
            var skins = shell.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Require(skins.Length == 4, "Expected leather and cuff for each hand.");
            var body = novice.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single(r => r.name == "Novice_BodySkin");
            foreach (var skin in skins)
            {
                string side = skin.name.StartsWith("Left", StringComparison.Ordinal) ? "Left" : "Right";
                var hand = animator.GetBoneTransform(side == "Left" ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
                var foreArmName = side + "ForeArm";
                var weights = skin.sharedMesh.boneWeights;
                bool leather = skin.name.EndsWith("Leather", StringComparison.Ordinal);
                bool cuff = skin.name.EndsWith("Cuff", StringComparison.Ordinal);
                Require(leather || cuff, "Unexpected glove mesh " + skin.name);
                Require(leather ? skin.bones.Contains(hand) : skin.bones.Any(b => b != null && b.name == foreArmName),
                    "Wrong primary glove binding " + skin.name);
                Require(weights.All(w =>
                    (w.weight0 == 0 || skin.bones[w.boneIndex0] == hand || skin.bones[w.boneIndex0].name == foreArmName) &&
                    (w.weight1 == 0 || skin.bones[w.boneIndex1] == hand || skin.bones[w.boneIndex1].name == foreArmName) &&
                    w.weight2 == 0 && w.weight3 == 0), "Wrong weighted hand binding " + skin.name);
                Require(weights.Length == skin.sharedMesh.vertexCount && weights.All(w => Mathf.Abs(w.weight0 + w.weight1 + w.weight2 + w.weight3 - 1) < .0001f), "Invalid skin weights.");
                Require(skin.sharedMesh.normals.Length == skin.sharedMesh.vertexCount && skin.sharedMesh.normals.All(n => n.sqrMagnitude > .0001f), "Invalid glove normals.");
                Require(skin.sharedMaterials.All(m => m != null && m.shader.name == "Universal Render Pipeline/Lit" && m.GetFloat("_Cull") == 2), "Invalid glove material.");
            }
            foreach (string clipName in new[] { "Idle", "Locomotion", "UnarmedAttack" })
            {
                var clip = Clips().Single(c => c.name.EndsWith(clipName, StringComparison.Ordinal));
                Vector3[] first = null;
                for (int sample = 0; sample < 5; sample++)
                {
                    SamplePose(novice, clip, sample / 5f);
                    var all = skins.SelectMany(WorldVertices).ToArray();
                    if (first == null) first = all;
                    foreach (var skin in skins)
                    {
                        var vertices = WorldVertices(skin);
                        var hand = animator.GetBoneTransform(skin.name.StartsWith("Left", StringComparison.Ordinal) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
                        Require(vertices.All(v => Vector3.Distance(v, hand.position) < .22f), "Glove detached from hand in " + clipName);
                    }
                    foreach (var side in new[] { "Left", "Right" })
                    {
                        var hand = animator.GetBoneTransform(side == "Left" ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
                        var palm = skins.Single(r => r.name == side + "GloveLeather");
                        float handError = Vector3.Distance(HandCentroid(body, hand), HandCentroid(palm, hand));
                        Require(handError < .04f, side + " glove does not cover the actual skinned hand in " + clipName + ": " + handError + "m");
                    }
                    if (sample == 2 && clipName != "Idle")
                        Require(all.Zip(first, Vector3.Distance).Max() > .005f, "Glove skin did not follow " + clipName);
                }
                Debug.Log("WORK_GLOVES_POSES_OK " + clipName + " samples=5 bothHands=true skinHandCentroidError<0.04m");
            }
            Debug.Log("WORK_GLOVES_UNITY_MESH_STATS vertices=" + skins.Sum(r => r.sharedMesh.vertexCount) + " triangles=" + skins.Sum(r => r.sharedMesh.triangles.Length / 3));
        }

        private static Vector3 HandCentroid(SkinnedMeshRenderer skin, Transform hand)
        {
            int index = Array.IndexOf(skin.bones, hand);
            var weights = skin.sharedMesh.boneWeights;
            var vertices = WorldVertices(skin);
            var selected = vertices.Where((v, i) => weights[i].boneIndex0 == index && weights[i].weight0 > .99f).ToArray();
            Require(selected.Length > 10, "Actual hand skin has no meaningful matching hand weights: " + skin.name);
            return selected.Aggregate(Vector3.zero, (sum, v) => sum + v) / selected.Length;
        }

        internal static AnimationClip[] Clips() => AssetDatabase.LoadAllAssetsAtPath("Assets/_DiceFree/Art/Characters/Novice/Models/Novice.fbx")
            .OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();

        private static void SamplePose(GameObject novice, AnimationClip clip, float normalizedTime)
        {
            var animator = novice.GetComponentInChildren<Animator>();
            string state = clip.name.EndsWith("UnarmedAttack", StringComparison.Ordinal) ? "UnarmedAttack" :
                clip.name.EndsWith("Locomotion", StringComparison.Ordinal) ? "Locomotion" : "Idle";
            animator.Play("Base Layer." + state, 0, normalizedTime);
            animator.Update(0);
        }

        private static Vector3[] WorldVertices(SkinnedMeshRenderer skin)
        {
            var baked = new Mesh();
            try { skin.BakeMesh(baked); return baked.vertices.Select(skin.transform.TransformPoint).ToArray(); }
            finally { UnityEngine.Object.DestroyImmediate(baked); }
        }

        private static void CapturePreview()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var previous = RenderTexture.active;
            var target = new RenderTexture(1600, 900, 24) { antiAliasing = 4 };
            var image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            try
            {
                EditorSceneManager.SetActiveScene(scene);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_DiceFree/Art/Characters/Novice/Prefabs/NovicePresentation.prefab");
                var poses = new[] { "Baseline", "Idle", "Locomotion", "UnarmedAttack" };
                for (int i = 0; i < poses.Length; i++)
                {
                    var novice = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                    if (i > 0) Bind(novice.transform);
                    novice.transform.position = new Vector3((1.5f - i) * 1.45f, 0, 0);
                    novice.transform.rotation = Quaternion.Euler(0, 20, 0);
                    novice.GetComponent<Animator>().Rebind();
                    novice.GetComponent<Animator>().Update(0);
                    SamplePose(novice, Clips().Single(c => c.name.EndsWith(i == 0 ? "Idle" : poses[i], StringComparison.Ordinal)), i < 2 ? 0 : .5f);
                    var label = new GameObject("Pose label").AddComponent<TextMesh>();
                    label.text = i == 0 ? "UNEQUIPPED" : "GLOVES\n" + poses[i].Replace("UnarmedAttack", "ATTACK").ToUpperInvariant();
                    label.fontSize = 40; label.characterSize = .025f; label.anchor = TextAnchor.MiddleCenter;
                    label.transform.position = novice.transform.position + new Vector3(0, -.32f, -.15f);
                    label.transform.rotation = Quaternion.Euler(30, 0, 0);
                }
                var light = new GameObject("Glove preview key").AddComponent<Light>();
                light.type = LightType.Directional; light.intensity = 1.5f;
                light.transform.rotation = Quaternion.Euler(35, -30, 0);
                var fill = new GameObject("Glove preview fill").AddComponent<Light>();
                fill.type = LightType.Directional; fill.intensity = .9f;
                fill.transform.rotation = Quaternion.Euler(25, 160, 0);
                var camera = new GameObject("Glove preview camera").AddComponent<Camera>();
                camera.orthographic = true; camera.orthographicSize = 1.85f;
                camera.transform.position = new Vector3(0, 3.8f, 6);
                camera.transform.LookAt(new Vector3(0, .85f, 0));
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.13f, .16f, .19f);
                foreach (var text in scene.GetRootGameObjects().Select(g => g.GetComponent<TextMesh>()).Where(t => t != null))
                    text.transform.rotation = camera.transform.rotation;
                foreach (var t in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))) t.gameObject.layer = 31;
                camera.cullingMask = 1 << 31; camera.targetTexture = target;
                camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); image.Apply();
                File.WriteAllBytes(PreviewPath, image.EncodeToPNG());
                AssetDatabase.ImportAsset(PreviewPath);
            }
            finally
            {
                RenderTexture.active = previous;
                target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(image);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        internal static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
