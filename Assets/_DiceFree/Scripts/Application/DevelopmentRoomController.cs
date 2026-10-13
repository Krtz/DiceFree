using System;
using System.Collections;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.Development
{
    public sealed class DevelopmentRoomController : CustomizableHudWidget
    {
        public static DevelopmentRoomController Current { get; private set; }
        public CombatActor player;
        public ExplorationCamera explorationCamera;
        public WorldFogOfWar fog;
        public bool Busy { get; private set; }
        public int CompletedTransitions { get; private set; }
        public int Frames { get; private set; }
        public string Status { get; private set; } = "Preparing test Echo";
        public DevelopmentMap Map { get; private set; }
        private string levelText = "10";
        private Vector2 itemScroll;
        public override string LayoutId => "development-room";
        public override string DisplayName => "Development controls";
        public override Rect DefaultNormalizedBounds => new(.01f,.10f,.25f,.56f);
        public override Vector2 MinimumPixelSize => new(300,360);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => Current = null;
        private void Awake() => Current = this;
        private void OnDestroy() { if (Current == this) Current = null; }
        private void Update() => Frames++;

        private IEnumerator Start()
        {
            float until = Time.realtimeSinceStartup + 15;
            var persistence = player.GetComponent<ManifestationPersistence>();
            while (!persistence.Ready && Time.realtimeSinceStartup < until) yield return null;
            if (!persistence.Ready) { Status = persistence.Status; yield break; }
            Map = FindMap(gameObject.scene);
            if (Map == null || !Place(Map)) { Status = "Test map has no reachable spawn."; yield break; }
            Status = "Test Echo ready";
        }

        private static DevelopmentMap FindMap(Scene scene) =>
            scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<DevelopmentMap>(true)).FirstOrDefault();

        private bool Place(DevelopmentMap map)
        {
            if (!player.Motor.TeleportAcrossMaps(map.spawn)) return false;
            var home = player.GetComponent<RespawnAtAnchor>();
            home.ConfigureRegisteredAnchors(map.anchor);
            home.Configure(map.anchor.transform);
            home.Activate(map.anchor);
            explorationCamera.ConfigureMapBounds(map.bounds);
            explorationCamera.RecenterForTravel(player.transform);
            fog.Configure(player.transform, explorationCamera.GetComponent<Camera>(),
                new Vector2(map.bounds.xMin,map.bounds.yMin),new Vector2(map.bounds.xMax,map.bounds.yMax),28);
            Map = map;
            player.GetComponent<MinimapHud>().MapName = map.worldId == "world.test.2" ? "World 2 test" : "Development room";
            return true;
        }

        public bool SetLevel(int level)
        {
            if (Busy || !player.GetComponent<ManifestationPersistence>().Ready) return false;
            level = Mathf.Clamp(level,1,200);
            var xp = player.GetComponent<ExperienceProgression>();
            if (level < xp.Level)
                foreach (var state in player.GetComponents<MonoBehaviour>().OfType<IClassSkillState>())
                    state.RestoreState(Array.Empty<SkillRankState>());
            xp.RestoreState(level,0);
            levelText = level.ToString();
            Status = "Level set to " + level;
            return true;
        }

        public bool SelectClass(string classId)
        {
            if (Busy) return false;
            var controller = player.GetComponent<AdvancementController>();
            if (controller.HasManifestation(classId))
                return controller.TrySwitch(classId);
            if (!controller.TrySwitch("class.novice")) return false;
            SetLevel(Mathf.Max(10,player.Stats.Level));
            var edge = controller.Definitions.FirstOrDefault(value => value.targetClass.stableId == classId);
            bool result = edge != null && controller.TryAdvance(edge);
            Status = controller.Feedback;
            return result;
        }

        public void RestorePlayer()
        {
            player.GetComponent<BasicAttack>()?.Cancel();
            player.Health.Restore();
            var resources = player.GetComponent<ActorResourceController>();
            foreach (var profile in resources.Profiles)
                if (resources.Has(profile.resource.stableId))
                    resources.Restore(profile.resource.stableId,resources.Maximum(profile.resource.stableId));
            Status = "Health and resources restored";
        }

        public bool Travel(string sceneName)
        {
            if (Busy || Map == null || !player.Alive) return false;
            if (sceneName != "DevelopmentRoom" && sceneName != "World2TransitionTest") return false;
            if (sceneName == gameObject.scene.name) return false;
            if (!player.GetComponent<ManifestationPersistence>().Flush()) return false;
            Busy = true;
            player.GetComponent<BasicAttack>()?.Cancel();
            player.GetComponent<SkillTargetingController>()?.Cancel();
            player.Motor.Stop();
            StartCoroutine(TravelRoutine(sceneName));
            return true;
        }

        private IEnumerator TravelRoutine(string sceneName)
        {
            Scene previous = gameObject.scene;
            Status = "Loading " + sceneName;
            yield return SceneManager.LoadSceneAsync(sceneName,LoadSceneMode.Additive);
            var next = SceneManager.GetSceneByName(sceneName);
            var map = FindMap(next);
            if (map == null || !Place(map))
            {
                Status = "Destination spawn is unavailable.";
                yield return SceneManager.UnloadSceneAsync(next);
                Busy = false;
                yield break;
            }
            // The authored map is one root. All other roots are this session
            // (player, cameras, HUD, runtime helpers), preserved as the same objects.
            foreach (var root in previous.GetRootGameObjects())
                if (root.GetComponent<DevelopmentMap>() == null &&
                    root.GetComponent<DevelopmentRoomBootstrap>() == null)
                    SceneManager.MoveGameObjectToScene(root,next);
            SceneManager.SetActiveScene(next);
            yield return SceneManager.UnloadSceneAsync(previous);
            CompletedTransitions++;
            Status = "Arrived in " + Map.worldId + " - player state preserved";
            Busy = false;
        }

        private void OnGUI()
        {
            if (!DiceFree.Foundation.DevelopmentSession.Active || player == null) return;
            if (HudPointerBlocker.ModalOpen) return;
            Rect panel = Bounds;
            DrawPanel(panel);
            GUILayout.BeginArea(Inner(panel));
            GUILayout.Label("DEVELOPMENT ROOM",new GUIStyle(GUI.skin.label){fontStyle=FontStyle.Bold});
            GUILayout.Label(player.DisplayName);
            GUILayout.Label(Map == null ? "Loading map..." : Map.worldId);
            GUI.enabled = !Busy && player.GetComponent<ManifestationPersistence>().Ready;
            GUILayout.BeginHorizontal();
            foreach (var definition in player.GetComponent<AdvancementController>().Classes)
                if (GUILayout.Button(definition.stableId == "class.novice" ? "Novice" :
                    definition.stableId.Contains("physical") ? "Physical" :
                    definition.stableId.Contains("magical") ? "Magical" : definition.displayName))
                    SelectClass(definition.stableId);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Level",GUILayout.Width(40));
            levelText = GUILayout.TextField(levelText,3,GUILayout.Width(55));
            if (GUILayout.Button("Set") && int.TryParse(levelText,out int level)) SetLevel(level);
            if (GUILayout.Button("Restore HP / resources")) RestorePlayer();
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Reset training targets"))
                foreach (var dummy in CombatActor.All.Where(value => value != player && value.Stats.Definition.stableId == "dev.practice-target"))
                    dummy.Health.Restore();
            GUILayout.Label("Equipment / item testing");
            itemScroll = GUILayout.BeginScrollView(itemScroll,GUILayout.Height(Mathf.Max(80,panel.height*.28f)));
            var inventory = player.GetComponent<CarriedInventory>();
            foreach (var item in inventory.Definitions.Where(value=>value!=null))
                if (GUILayout.Button("Grant " + item.displayName)) { inventory.Grant(item.stableId); Status = "Granted " + item.displayName; }
            GUILayout.EndScrollView();
            if (GUILayout.Button(Map != null && Map.worldId == "world.test.2" ? "Return to World 1 test room" : "Travel to World 2 test"))
                Travel(Map != null && Map.worldId == "world.test.2" ? "DevelopmentRoom" : "World2TransitionTest");
            if (GUILayout.Button("Main menu")) player.GetComponent<DiceFree.Dungeons.ReturnToStartMenu>().Return();
            GUI.enabled = true;
            GUILayout.Label(Status,new GUIStyle(GUI.skin.label){wordWrap=true});
            GUILayout.EndArea();
        }
    }
}
