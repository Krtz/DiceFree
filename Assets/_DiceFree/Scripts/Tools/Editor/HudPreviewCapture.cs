using System;
using System.IO;
using System.Linq;
using DiceFree.Combat;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class HudPreviewCapture
    {
        private const string Prefix = "DiceFree.HudPreview.";
        public const string PreviewPath = "Logs/Hud/hud-preview.png";
        private static float captureAt;
        private static float deadline;

        static HudPreviewCapture() => EditorApplication.playModeStateChanged += OnPlayState;

        [CliCommand("dicefree.hud.capture-preview", "Capture the current in-engine customizable HUD preview.", Tags = new[] { "ui", "hud", "capture" })]
        private static object Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("HUD preview capture requires idle Edit Mode.");

            string full = Path.GetFullPath(PreviewPath);
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            if (File.Exists(full)) File.Delete(full);

            SessionState.SetString(Prefix + "status", "running");
            SessionState.SetString(Prefix + "error", "");
            PersistenceTestGuard.DisableForNextPlay();
            EditorSceneManager.OpenScene(HudAuthoring.ScenePath);
            EditorApplication.EnterPlaymode();
            return Status();
        }

        [CliCommand("dicefree.hud.capture-preview-status", "Read the current HUD preview capture status.", Tags = new[] { "ui", "hud", "capture" })]
        private static object Status() => new
        {
            status = SessionState.GetString(Prefix + "status", "idle"),
            success = SessionState.GetString(Prefix + "status", "idle") == "passed",
            error = SessionState.GetString(Prefix + "error", ""),
            previewPath = Path.GetFullPath(PreviewPath)
        };

        private static void OnPlayState(PlayModeStateChange state)
        {
            if (SessionState.GetString(Prefix + "status", "") != "running") return;
            if (state != PlayModeStateChange.EnteredPlayMode) return;

            EditorApplication.isPaused = false;
            Time.timeScale = 1f;
            Screen.SetResolution(1600, 1000, false);
            captureAt = Time.realtimeSinceStartup + 1.2f;
            deadline = Time.realtimeSinceStartup + 8f;
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            try
            {
                if (!EditorApplication.isPlaying) return;
                if (Time.realtimeSinceStartup > deadline)
                    throw new InvalidOperationException("HUD preview capture timed out.");
                if (Time.realtimeSinceStartup < captureAt) return;

                var player = UnityEngine.Object.FindAnyObjectByType<DiceFree.Characters.TraversalInput>();
                var selection = player == null ? null : player.GetComponent<TargetSelection>();
                if (selection != null && selection.Selected == null)
                {
                    var actor = player.GetComponent<CombatActor>();
                    var target = CombatActor.All.FirstOrDefault(value => value != null && actor.IsHostileTo(value));
                    if (target != null) selection.Select(target);
                }

                string full = Path.GetFullPath(PreviewPath);
                ScreenCapture.CaptureScreenshot(full);
                EditorApplication.update -= Tick;
                EditorApplication.delayCall += WaitForFile;
            }
            catch (Exception error)
            {
                Fail(error);
            }
        }

        private static void WaitForFile()
        {
            string full = Path.GetFullPath(PreviewPath);
            if (!File.Exists(full))
            {
                if (Time.realtimeSinceStartup > deadline) { Fail(new IOException("HUD preview file was not written.")); return; }
                EditorApplication.delayCall += WaitForFile;
                return;
            }

            SessionState.SetString(Prefix + "status", "passed");
            SessionState.SetString(Prefix + "error", "");
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        }

        private static void Fail(Exception error)
        {
            EditorApplication.update -= Tick;
            SessionState.SetString(Prefix + "status", "failed");
            SessionState.SetString(Prefix + "error", error.ToString());
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        }
    }
}
