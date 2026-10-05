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
    internal static class FarmersPantsArtValidation
    {
        internal const string ItemId = "item.cornberg.farmers-pants";
        internal const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        internal const string PreviewPath = "Assets/_DiceFree/Art/Validation/Previews/FarmersPants_Unity.png";
        private const string ArtRoot = "Assets/_DiceFree/Art/Characters/FarmersPants";
        private const string ModelPath = ArtRoot + "/Models/FarmersPants.fbx";
        private const string PrefabPath = ArtRoot + "/Prefabs/FarmersPantsPresentation.prefab";

        [CliCommand("dicefree.art.pants.prepare", "Import the farmer pants and bind them to the actual Cornberg Novice rig.")]
        private static object PrepareCommand() => Run(Prepare, "FARMERS_PANTS_PREPARED");
        [CliCommand("dicefree.art.pants.validate", "Validate source/import, lower-body weights, animation propagation and baseline preservation.")]
        private static object ValidateCommand() => Run(Validate, "FARMERS_PANTS_ART_OK");
        [CliCommand("dicefree.art.pants.capture-preview", "Capture baseline, equipped idle, locomotion and attack Novice fixtures.")]
        private static object PreviewCommand() => Run(CapturePreview, "FARMERS_PANTS_PREVIEW_OK");

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
            var wrapper = new GameObject("FarmersPantsPresentation");
            var imported = (GameObject)PrefabUtility.InstantiatePrefab(model);
            imported.transform.SetParent(wrapper.transform, false);
            var leather = Material("FarmersPants_DustyCanvas", new Color(.34f, .30f, .21f));
            var canvas = Material("FarmersPants_WornHem", new Color(.43f, .37f, .25f));
            foreach (var r in imported.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                r.sharedMaterials = new[] { r.name == "FarmersPants_Canvas" ? leather : canvas };
                r.updateWhenOffscreen = true;
            }
            PrefabUtility.SaveAsPrefabAsset(wrapper, PrefabPath);
            UnityEngine.Object.DestroyImmediate(wrapper);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var player = Player(scene);
            var novice = player.GetComponent<NovicePresentationDriver>().VisualRoot;
            var old = novice.Find("FarmersPants");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var pants = Bind(novice);
            var inventory = player.GetComponent<CarriedInventory>();
            const string itemPath = "Assets/_DiceFree/Settings/Items/Farmer's Pants.asset";
            var definition = AssetDatabase.LoadAssetAtPath<ItemDefinition>(itemPath);
            if (definition == null) { definition = ScriptableObject.CreateInstance<ItemDefinition>(); AssetDatabase.CreateAsset(definition, itemPath); }
            definition.stableId = ItemId; definition.displayName = "Farmer's Pants";
            definition.slot = EquipmentSlot.Legs; definition.sourceId = "prototype.test";
            definition.stats = new EquipmentStats { physicalDefense = 1, attributes = new AttributeValues { vitality = 1 } };
            EditorUtility.SetDirty(definition);
            inventory.Configure(inventory.Definitions.Where(d => d.stableId != ItemId).Concat(new[] { definition }).ToArray());
            var binding = player.GetComponent<EquipmentPresentation>();
            binding.Configure(binding.Bindings.Where(b => b.slot != EquipmentSlot.Legs).Concat(new[] { new EquipmentPresentation.Binding { slot = EquipmentSlot.Legs,
                definition = definition, visuals = new[] { pants }, baselineVisuals = new[] { BaselineLegs(novice) } } }).ToArray());
            pants.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);
            Require(EditorSceneManager.SaveScene(scene), "Could not save pants binding.");
            AssetDatabase.SaveAssets();
        }

        internal static GameObject Bind(Transform novice) => EquipmentArtAuthoring.Bind(novice, PrefabPath, "FarmersPants");

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
            foreach (string file in new[] { "build_farmers_pants.py", "FarmersPants.blend", "export_manifest.json" })
                Require(File.Exists("SourceArt/Characters/FarmersPants/" + file), "Missing source " + file);
            var importer = (ModelImporter)AssetImporter.GetAtPath(ModelPath);
            Require(importer.globalScale == 1 && !importer.importAnimation && !importer.addCollider && !importer.importCameras &&
                !importer.importLights && !importer.importBlendShapes && importer.materialImportMode == ModelImporterMaterialImportMode.None,
                "Unexpected pants importer configuration.");
            Require(!AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().Any(), "Pants imported animation.");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var player = Player(scene);
            var binding = player.GetComponent<EquipmentPresentation>().Bindings.Single(b => b.slot == EquipmentSlot.Legs);
            Require(binding.slot == EquipmentSlot.Legs && binding.definition.stableId == ItemId, "Wrong authored item/slot binding.");
            var pants = binding.visuals.Single();
            Require(!pants.activeSelf, "Saved baseline must have no visible pants.");
            var novice = player.GetComponent<NovicePresentationDriver>().VisualRoot;
            var body = novice.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single(r => r.name == "Novice_BodySkin");
            Require(body.enabled && novice.GetComponentsInChildren<SkinnedMeshRenderer>(true).Count(r => r.name.StartsWith("Novice_", StringComparison.Ordinal)) == 4,
                "The four original Novice baseline meshes must be preserved.");
            Require(pants.GetComponentsInChildren<Collider>(true).Length == 0 && pants.GetComponentsInChildren<Animator>(true).Length == 0,
                "Pants must share the Novice Animator and add no gameplay collider.");
            var clone = UnityEngine.Object.Instantiate(novice.gameObject);
            try
            {
                var shell = clone.transform.Find("FarmersPants").gameObject;
                shell.SetActive(true);
                ValidateSkin(clone, shell);
            }
            finally { UnityEngine.Object.DestroyImmediate(clone); }
        }

        internal static GameObject BaselineLegs(Transform novice) => novice.GetComponentsInChildren<SkinnedMeshRenderer>(true)
            .Single(r => r.name == "Novice_Baseline_Underwear").gameObject;

        internal static void ValidateSkin(GameObject novice, GameObject shell)
        {
            var animator = novice.GetComponentInChildren<Animator>();
            animator.Rebind(); animator.Update(0);
            var skins = shell.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Require(skins.Length == 4, "Expected canvas, waistband and two hems.");
            var allowed = new[] { HumanBodyBones.Hips, HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg }.Select(animator.GetBoneTransform).ToArray();
            foreach (var skin in skins)
            {
                var weights = skin.sharedMesh.boneWeights;
                Require(weights.Length == skin.sharedMesh.vertexCount, "Missing weights.");
                Require(weights.All(w => Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1) < .0001f), "Unnormalized weights.");
                foreach (var w in weights)
                {
                    var indices = new[] { w.boneIndex0, w.boneIndex1, w.boneIndex2, w.boneIndex3 };
                    var values = new[] { w.weight0, w.weight1, w.weight2, w.weight3 };
                    for (int i=0; i<4; i++) if (values[i] > 0) Require(allowed.Contains(skin.bones[indices[i]]), "Foreign weighted bone.");
                }
                Require(skin.sharedMesh.normals.Length == skin.sharedMesh.vertexCount && skin.sharedMesh.normals.All(n => n.sqrMagnitude > .0001f), "Invalid normals.");
                Require(skin.sharedMaterials.All(m => m != null && m.shader.name == "Universal Render Pipeline/Lit" && m.GetFloat("_Cull") == 2), "Invalid canvas material.");
            }
            var canvas = skins.Single(r => r.name == "FarmersPants_Canvas");
            Require(allowed.All(b => canvas.bones.Contains(b)), "Canvas must bind real pelvis/thigh/knee chain.");
            foreach (string clipName in new[] { "Idle", "Locomotion", "UnarmedAttack" })
            {
                var clip = Clips().Single(c => c.name.EndsWith(clipName, StringComparison.Ordinal));
                Vector3[] first = null;
                for (int sample=0; sample<5; sample++)
                {
                    SamplePose(novice, clip, sample/5f);
                    var all = skins.SelectMany(WorldVertices).ToArray();
                    if (first == null) first = all;
                    foreach (var side in new[] { "Left", "Right" })
                    {
                        var upper = animator.GetBoneTransform(side == "Left" ? HumanBodyBones.LeftUpperLeg : HumanBodyBones.RightUpperLeg);
                        var knee = animator.GetBoneTransform(side == "Left" ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg);
                        var foot = animator.GetBoneTransform(side == "Left" ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot);
                        var hem = skins.Single(r => r.name == side+"PantsHem");
                        var center = WorldVertices(hem).Aggregate(Vector3.zero,(sum,v)=>sum+v)/hem.sharedMesh.vertexCount;
                        Require(Vector3.Distance(center,foot.position) < .11f, "Hem detached from ankle in "+clipName);
                        var vertices = WorldVertices(canvas);
                        Require(vertices.Any(v => Vector3.Distance(v,knee.position) < .16f), "No canvas around actual knee.");
                        Require(vertices.Any(v => Vector3.Distance(v,upper.position) < .17f), "No canvas around actual thigh.");
                    }
                    if (sample == 2 && clipName == "Locomotion")
                        Require(all.Zip(first,Vector3.Distance).Max() > .02f, "Pants do not deform with locomotion.");
                }
                Debug.Log("FARMERS_PANTS_POSES_OK "+clipName+" samples=5 realPelvisThighKnee=true");
            }
            // The existing clips rotate thighs but do not articulate knees independently.
            SamplePose(novice, Clips().Single(c => c.name.EndsWith("Idle",StringComparison.Ordinal)),0);
            var leftKnee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            var leftHem = skins.Single(r => r.name == "LeftPantsHem");
            var before = WorldVertices(leftHem);
            leftKnee.localRotation *= Quaternion.Euler(25,0,0);
            Require(WorldVertices(leftHem).Zip(before,Vector3.Distance).Max() > .05f, "Knee flex did not propagate.");
            Debug.Log("FARMERS_PANTS_KNEE_FLEX_OK degrees=25 synthetic=true");
            Debug.Log("FARMERS_PANTS_UNITY_MESH_STATS vertices="+skins.Sum(r=>r.sharedMesh.vertexCount)+" triangles="+skins.Sum(r=>r.sharedMesh.triangles.Length/3));
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
                    if (i > 0) { Bind(novice.transform); BaselineLegs(novice.transform).SetActive(false); }
                    novice.transform.position = new Vector3((1.5f - i) * 1.45f, 0, 0);
                    novice.transform.rotation = Quaternion.Euler(0, 20, 0);
                    novice.GetComponent<Animator>().Rebind();
                    novice.GetComponent<Animator>().Update(0);
                    SamplePose(novice, Clips().Single(c => c.name.EndsWith(i == 0 ? "Idle" : poses[i], StringComparison.Ordinal)), i < 2 ? 0 : .5f);
                    var label = new GameObject("Pose label").AddComponent<TextMesh>();
                    label.text = i == 0 ? "UNEQUIPPED" : "PANTS\n" + poses[i].Replace("UnarmedAttack", "ATTACK").ToUpperInvariant();
                    label.fontSize = 40; label.characterSize = .025f; label.anchor = TextAnchor.MiddleCenter;
                    label.transform.position = novice.transform.position + new Vector3(0, -.32f, -.15f);
                    label.transform.rotation = Quaternion.Euler(30, 0, 0);
                }
                var light = new GameObject("Pants preview key").AddComponent<Light>();
                light.type = LightType.Directional; light.intensity = 1.5f;
                light.transform.rotation = Quaternion.Euler(35, -30, 0);
                var fill = new GameObject("Pants preview fill").AddComponent<Light>();
                fill.type = LightType.Directional; fill.intensity = .9f;
                fill.transform.rotation = Quaternion.Euler(25, 160, 0);
                var camera = new GameObject("Pants preview camera").AddComponent<Camera>();
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
                EditorSceneManager.CloseScene(scene, true);
                target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(image);
            }
        }

        internal static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}


