using System;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.EditorTools
{
    public static class CornbergCombatSetup
    {
        private const string Data = "Assets/_DiceFree/Settings/Combat";

        // Additive migration of the existing scene. Never calls the Phase 1 generator or rebakes geometry.
        [MenuItem("DiceFree/Combat/Add combat to saved Cornberg")]
        public static void Install()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var playerInput = UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            if (playerInput == null) throw new InvalidOperationException("Existing Cornberg traversal player is required.");
            if (!AssetDatabase.IsValidFolder(Data)) AssetDatabase.CreateFolder("Assets/_DiceFree/Settings", "Combat");
            ConfigureLayer();
            var tuning = Asset<CombatTuning>("Cornberg provisional tuning", _ => { });
            var fists = Asset<AttackDefinition>("Novice fists", value => {
                value.stableId = "attack.novice.fists"; value.displayName = "Fists"; value.scaling = AttributeScaling.Highest;
            });
            var bump = Asset<AttackDefinition>("Crop Slime bump", value => {
                value.stableId = "attack.crop-slime.bump"; value.displayName = "Bump";
                value.baseDamage = 3; value.coefficient = 0; value.interval = 1.7f; value.windup = 0.45f; value.reach = 0.6f;
            });
            var novice = Asset<ActorDefinition>("Novice combat stats", value => {
                value.stableId = "class.novice"; value.displayName = "Novice"; value.basicAttack = fists; value.tuning = tuning;
            });
            var slime = Asset<ActorDefinition>("Crop Slime combat stats", value => {
                value.stableId = "enemy.crop-slime"; value.displayName = "Crop Slime"; value.baseHp = 12;
                value.baseAttributes = new AttributeValues(0); value.growth = new AttributeValues(0);
                value.moveSpeed = 3.4f; value.basicAttack = bump; value.tuning = tuning;
            });
            var player = SetupActor(playerInput.gameObject, novice, 0, 0.45f);
            var selection = Ensure<TargetSelection>(player.gameObject);
            Ensure<CombatInput>(player.gameObject);
            var playerVisual = player.transform.Find("Echo body");
            Ensure<ActorFeedback>(player.gameObject).Configure(playerVisual);
            var anchor = Named("Cornberg resurrection anchor"); anchor.transform.position = new Vector3(8,0,6);
            Ensure<RespawnAtAnchor>(player.gameObject).Configure(anchor.transform);
            var well = Named("Cornberg magical well healing area"); well.transform.position = new Vector3(-2,0,3);
            var healing = Ensure<HealingArea>(well);
            var enemyObject = GameObject.Find("Crop Slime - combat placeholder");
            if (enemyObject == null)
            {
                enemyObject = new GameObject("Crop Slime - combat placeholder");
                enemyObject.transform.position = new Vector3(47,0,-10);
                var body = Primitive("Slime body", enemyObject.transform, new Vector3(0,0.48f,0), new Vector3(1.4f,0.85f,1.4f),
                    Material("Crop Slime green", new Color(0.23f,0.72f,0.12f)));
                var eyeMaterial = Material("Slime eyes", new Color(0.035f,0.06f,0.02f));
                Primitive("Left eye", body, new Vector3(-0.18f,0.12f,0.43f), Vector3.one*0.14f, eyeMaterial);
                Primitive("Right eye", body, new Vector3(0.18f,0.12f,0.43f), Vector3.one*0.14f, eyeMaterial);
            }
            var enemy = SetupActor(enemyObject, slime, 1, 0.65f);
            var brain = Ensure<AggroBehaviour>(enemyObject);
            Ensure<ActorFeedback>(enemyObject).Configure(enemyObject.transform.Find("Slime body"));
            Ensure<PlayerCombatPanel>(Named("HUD - player combat")).Configure(player, healing);
            Ensure<TargetFrame>(Named("HUD - target")).Configure(selection);
            Ensure<CombatCommandsPanel>(Named("HUD - combat commands")).Configure(player, brain);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("CORNBERG_COMBAT_SETUP_OK: existing scene extended; no geography regenerated.");
        }
        private static CombatActor SetupActor(GameObject obj, ActorDefinition definition, int team, float radius)
        {
            obj.layer = 10;
            var agent = Ensure<NavMeshAgent>(obj); agent.enabled = false;
            agent.radius = radius; agent.height = 1.8f; agent.areaMask = 1;
            Ensure<TraversalMotor>(obj);
            Ensure<ActorStats>(obj).Configure(definition);
            Ensure<Health>(obj);
            var actor = Ensure<CombatActor>(obj); actor.Configure(team, radius);
            Ensure<BasicAttack>(obj);
            var collider = Ensure<CapsuleCollider>(obj); collider.center = new Vector3(0,0.9f,0); collider.height = 1.8f; collider.radius = radius;
            return actor;
        }
        private static T Ensure<T>(GameObject obj) where T : Component => obj.TryGetComponent<T>(out var value) ? value : obj.AddComponent<T>();
        private static GameObject Named(string name) => GameObject.Find(name) ?? new GameObject(name);
        private static T Asset<T>(string name, Action<T> initialize) where T : ScriptableObject
        {
            var path = Data + "/" + name + ".asset";
            var value = AssetDatabase.LoadAssetAtPath<T>(path);
            if (value != null) return value;
            value = ScriptableObject.CreateInstance<T>(); initialize(value); AssetDatabase.CreateAsset(value, path); return value;
        }
        private static Material Material(string name, Color color)
        {
            var path = "Assets/_DiceFree/Materials/Blockout/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); material.color = color; AssetDatabase.CreateAsset(material,path); }
            return material;
        }
        private static Transform Primitive(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj.name = name;
            obj.transform.SetParent(parent); obj.transform.localPosition = position; obj.transform.localScale = scale;
            UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>()); obj.GetComponent<Renderer>().sharedMaterial = material;
            obj.layer = 2; return obj.transform;
        }
        private static void ConfigureLayer()
        {
            var manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layer = manager.FindProperty("layers").GetArrayElementAtIndex(10);
            if (!string.IsNullOrEmpty(layer.stringValue) && layer.stringValue != "CombatActor") throw new InvalidOperationException("Layer 10 already assigned.");
            layer.stringValue = "CombatActor"; manager.ApplyModifiedProperties();
        }
    }
}
