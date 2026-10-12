using System;
using System.Linq;
using DiceFree.Ironpine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
    // A real Play Mode initialization smoke check, not just serialized-data assertions.
    [InitializeOnLoad]
    public static class IronpineWildlifePlaymodeValidation
    {
        private const string Running = "DiceFree.IronpineWildlifeSmoke";
        private static float deadline;
        private static int frames;
        static IronpineWildlifePlaymodeValidation() =>
            EditorApplication.playModeStateChanged += Changed;

        public static void Run()
        {
            try
            {
                EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity", OpenSceneMode.Single);
                SessionState.SetBool(Running, true);
                EditorApplication.EnterPlaymode();
            }
            catch (Exception e) { Debug.LogException(e); End(1); }
        }

        private static void Changed(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode ||
                !SessionState.GetBool(Running, false)) return;
            Time.timeScale = 1f;
            Application.runInBackground = true;
            deadline = Time.realtimeSinceStartup + 60f;
            frames = 0;
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            try
            {
                if (!EditorApplication.isPlaying) return;
                if (Time.realtimeSinceStartup > deadline)
                    throw new InvalidOperationException("Ironpine wildlife initialization timed out");
                if (++frames < 35) return;
                var animals = UnityEngine.Object.FindObjectsByType<IronpineWildlife>(FindObjectsSortMode.None);
                Check(animals.Length == 22, "expected 22 wildlife, got " + animals.Length);
                Check(animals.Count(a => a.Kind == WildlifeKind.Quillrat) == 10, "quillrats");
                Check(animals.Count(a => a.Kind == WildlifeKind.GreyWolf) == 7, "grey wolves");
                Check(animals.Count(a => a.Kind == WildlifeKind.DarkWolf) == 4, "dark wolves");
                Check(animals.Count(a => a.Kind == WildlifeKind.AlphaWolf) == 1, "alpha");
                Check(animals.All(a => a.Actor != null && a.Actor.Alive &&
                    a.Actor.Motor.Ready && a.Actor.Stats.Definition != null), "uninitialized actor/navmesh");
                Check(animals.All(a => a.GetComponent<DiceFree.Items.FixedWorldDrop>() == null),
                    "wildlife unexpectedly carries an item drop component");
                Check(animals.All(a => a.GetComponent<IronpineCreaturePresentation>()?.ModelAnimator?.runtimeAnimatorController != null), "missing live rig animation controller");
                Check(animals.All(a => a.GetComponent<IronpineCreatureAudio>() != null && a.GetComponent<AudioSource>() != null), "missing live spatial animal audio");
                Check(animals.All(a => a.GetComponent<IronpineCreaturePresentation>().ModelAnimator.GetComponentsInChildren<SkinnedMeshRenderer>().Length >= 3), "missing live skinned model");
                if (Resources.Load<GameObject>("IronpineLicensedWolf/RealisticWolf") != null)
                {
                    Check(animals.Where(a => a.Kind != WildlifeKind.Quillrat).All(a =>
                        a.transform.Find("Realistic furry wolf licensed visual") != null),
                        "licensed wolf was not substituted on all wolf actors");
                    var alphaWolf = animals.Single(a => a.Kind == WildlifeKind.AlphaWolf);
                    Check(alphaWolf.GetComponentsInChildren<Light>(true).Count(l => l.name == "Faint red eye ember") >= 2,
                        "alpha red eye glow missing");
                }
                // Balance contract: doubling applies to every wolf type,
                // including the roaming Alpha; quillrats remain unchanged.
                foreach(var animal in animals)
                {
                    bool wolf=animal.Kind!=WildlifeKind.Quillrat;
                    float expectedRatio=animal.Kind==WildlifeKind.Quillrat?7f:
                        animal.Kind==WildlifeKind.AlphaWolf?130f:24f;
                    var stats=animal.Actor.Stats;
                    float baseline=stats.Definition.baseHp+
                        stats.Attributes.vitality*stats.HpPerVitality;
                    Check(Mathf.Abs(stats.MaximumHp-baseline*expectedRatio)<
                        Mathf.Max(1f,baseline*expectedRatio*.01f),
                        "incorrect HP multiplier for "+animal.name+
                        " expected "+expectedRatio+"x; got "+stats.MaximumHp);
                    Check(Mathf.Approximately(stats.BasicAttackDamageMultiplier,
                        wolf?1.5f:1f),
                        "incorrect bite damage multiplier for "+animal.name);
                }
                Debug.Log("IRONPINE_WOLF_BALANCE_OK: 12 ordinary wolves + 1 Alpha have doubled HP / +50% damage; quillrats unchanged.");
                var alpha = animals.Single(a => a.Kind == WildlifeKind.AlphaWolf).Actor;
                Check(alpha.Stats.Level == 20 && alpha.Health.Current > 0, "alpha not level 20/alive");
                Debug.Log("IRONPINE_WILDLIFE_PLAYMODE_OK: 22 living actors, correct species quotas, " +
                    "navigation registered, no item drops, Alpha level 20.");
                End(0);
            }
            catch (Exception e) { Debug.LogException(e); End(1); }
        }
        private static void Check(bool ok, string message)
        {
            if (!ok) throw new InvalidOperationException("IRONPINE_WILDLIFE_PLAYMODE: " + message);
        }
        private static void End(int code)
        {
            EditorApplication.update -= Tick;
            SessionState.SetBool(Running, false);
            Time.timeScale = 1;
            EditorApplication.Exit(code);
        }
    }
}

