using System;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.IO;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.UI;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;

namespace DiceFree.EditorTools
{
    /// <summary>Integrates the accepted Novice art beneath Cornberg's existing gameplay actor root.</summary>
    internal static class CornbergNovicePresentationValidation
    {
        private const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string PlayerName = "Echo - traversal placeholder";
        private const string NovicePrefabPath = "Assets/_DiceFree/Art/Characters/Novice/Prefabs/NovicePresentation.prefab";
        private const string PreviewPath = "Assets/_DiceFree/Art/Validation/Previews/CornbergNovicePlayer_Unity.png";

        [CliCommand("dicefree.art.novice.cornberg.prepare", "Attach the accepted Novice presentation to the existing Cornberg player root.", Tags = new[] { "art", "dicefree/art" })]
        private static object PrepareCommand() => Run("novice-cornberg-prepare", "DICEFRE_NOVICE_CORNBERG_PREPARED", PrepareScene);

        [CliCommand("dicefree.art.novice.cornberg.validate", "Validate the Cornberg player presentation boundary and retained gameplay root.", Tags = new[] { "tests", "dicefree/art" })]
        private static object ValidateCommand() => Run("novice-cornberg-presentation", "DICEFRE_NOVICE_CORNBERG_PRESENTATION_OK", ValidateScene);

        [CliCommand("dicefree.art.novice.cornberg.capture-preview", "Capture the actual Cornberg scene with its Novice player presentation.", Tags = new[] { "art", "capture", "dicefree/art" })]
        private static object CapturePreviewCommand() => Run("novice-cornberg-preview", "DICEFRE_NOVICE_CORNBERG_PREVIEW_OK", CapturePreview);

        [CliCommand("dicefree.art.novice.cornberg.build-windows", "Build the Cornberg player presentation for Windows x64 Development.", Tags = new[] { "builds", "dicefree/art" })]
        private static object BuildWindowsCommand() => BuildWindows();

        private static object Run(string name, string marker, Action action)
        {
            var timer = Stopwatch.StartNew();
            string error = null;
            try { action(); Debug.Log(marker); }
            catch (Exception exception) { error = exception.ToString(); Debug.LogError("DICEFRE_NOVICE_CORNBERG_PRESENTATION_FAIL\n" + error); }
            timer.Stop();
            return new { success = error == null, validation = name, finalMarker = error == null ? marker : "DICEFRE_NOVICE_CORNBERG_PRESENTATION_FAIL", elapsedSeconds = timer.Elapsed.TotalSeconds, error, scenePath = ScenePath, previewPath = PreviewPath, testedGitSha = GitHead() };
        }

        private static void PrepareScene()
        {
            Require(File.Exists(Path.Combine(ProjectRoot(), NovicePrefabPath)), "Accepted Novice presentation prefab is missing.");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var player = FindPlayer(scene);
            foreach (string placeholderName in new[] { "Echo body", "Facing" })
            {
                var placeholder = player.transform.Find(placeholderName);
                Require(placeholder != null, "Expected player placeholder visual is missing: " + placeholderName + ".");
                placeholder.gameObject.SetActive(false);
            }

            var visual = player.transform.Find("NovicePresentation");
            if (visual == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NovicePrefabPath);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                instance.name = "NovicePresentation";
                instance.transform.SetParent(player.transform, false);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                visual = instance.transform;
            }
            var animator = visual.GetComponentInChildren<Animator>(true);
            Require(animator != null, "Novice presentation prefab must own its Animator.");
            var driver = player.GetComponent<NovicePresentationDriver>() ?? player.gameObject.AddComponent<NovicePresentationDriver>();
            driver.Configure(visual, animator);
            var feedback = player.GetComponent<ActorFeedback>();
            Require(feedback != null, "Existing player ActorFeedback is missing.");
            feedback.Configure(visual);
            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(scene);
            Require(EditorSceneManager.SaveScene(scene), "Failed to save Cornberg after installing the Novice presentation.");
            AssetDatabase.SaveAssets();
        }

        private static void ValidateScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var player = FindPlayer(scene);
            var actor = player.GetComponent<CombatActor>();
            var motor = player.GetComponent<TraversalMotor>();
            var input = player.GetComponent<TraversalInput>();
            var attack = player.GetComponent<BasicAttack>();
            var health = player.GetComponent<Health>();
            var agent = player.GetComponent<NavMeshAgent>();
            var capsule = player.GetComponent<CapsuleCollider>();
            var combatInput = player.GetComponent<CombatInput>();
            var interactor = player.GetComponent<DiceFree.World.Interactor>();
            var registry = player.GetComponent<DiceFree.World.InteractionRegistry>();
            var selection = player.GetComponent<TargetSelection>();
            Require(actor != null && motor != null && input != null && attack != null && health != null && agent != null && capsule != null &&
                combatInput != null && interactor != null && registry != null && selection != null,
                "The existing gameplay-owned player root lost a required gameplay/input component.");
            Require(Mathf.Approximately(actor.Radius, 0.45f) && Mathf.Approximately(capsule.radius, 0.45f) && Mathf.Approximately(capsule.height, 1.8f) &&
                Vector3.Distance(capsule.center, new Vector3(0f, 0.9f, 0f)) <= 0.001f && Mathf.Approximately(agent.radius, 0.45f) && Mathf.Approximately(agent.height, 1.8f),
                "Authored gameplay actor/NavMesh dimensions changed while integrating presentation.");
            Require(player.GetComponents<Collider>().Length == 1 && player.GetComponentsInChildren<Collider>(true).Length == 1,
                "The Novice visual must not add or derive gameplay colliders.");
            foreach (string placeholderName in new[] { "Echo body", "Facing" })
                Require(player.transform.Find(placeholderName) != null && !player.transform.Find(placeholderName).gameObject.activeSelf,
                    "Duplicate placeholder visual remains active: " + placeholderName + ".");

            var visual = player.transform.Find("NovicePresentation");
            var driver = player.GetComponent<NovicePresentationDriver>();
            var feedback = player.GetComponent<ActorFeedback>();
            Require(visual != null && visual.gameObject.activeSelf && visual.parent == player.transform && driver != null && feedback != null,
                "Novice visual child, actor presentation driver, or feedback binding is missing.");
            var animator = visual.GetComponentInChildren<Animator>(true);
            Require(animator != null && animator.avatar != null && animator.avatar.isValid && animator.avatar.isHuman &&
                animator.runtimeAnimatorController != null && animator.applyRootMotion == false,
                "Integrated Novice Animator/Avatar configuration is invalid.");
            Require(animator.parameters.Any(p => p.name == "Speed" && p.type == AnimatorControllerParameterType.Float) &&
                animator.parameters.Any(p => p.name == "Attack" && p.type == AnimatorControllerParameterType.Trigger),
                "Novice Animator controller does not expose its authored Speed and Attack parameters.");
            var worldCamera = (Camera)typeof(TraversalInput).GetField("worldCamera", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(input);
            Require(worldCamera != null && Camera.main == worldCamera,
                "Existing traversal input must keep its authored Cornberg exploration camera.");
            Require(driver.VisualRoot == visual && driver.VisualAnimator == animator && feedback.enabled,
                "Presentation driver or death-feedback visual reference points to the wrong transform.");
            Require(visual.GetComponentsInChildren<Collider>(true).Length == 0,
                "Novice presentation must not contribute physics colliders.");
            Debug.Log("DICEFRE_NOVICE_CORNBERG_BOUNDARY_OK playerRoot=" + player.name + " capsule=" + capsule.height.ToString("F2") + "m/" + capsule.radius.ToString("F2") + "m navMesh=" + agent.height.ToString("F2") + "m/" + agent.radius.ToString("F2") + "m visual=" + visual.name + " avatar=Humanoid rootMotion=false camera=input-bound interaction=registry combat=selected-target");
        }

        private static void CapturePreview()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ValidateScene();
            var scene = SceneManager.GetActiveScene();
            var player = FindPlayer(scene);
            var focus = player.transform.position + Vector3.up * 0.9f;
            var cameraObject = new GameObject("Issue43PreviewCamera");
            var camera = cameraObject.AddComponent<Camera>();
            const int width = 1280, height = 800;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var previousActive = RenderTexture.active;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.orthographic = true;
                camera.orthographicSize = 3.2f;
                camera.aspect = (float)width / height;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 1500f;
                camera.clearFlags = CameraClearFlags.Skybox;
                cameraObject.transform.position = focus + new Vector3(4.2f, 3.4f, -5.4f);
                cameraObject.transform.LookAt(focus, Vector3.up);
                camera.targetTexture = target;
                camera.Render(); RenderTexture.active = target; image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
                string fullPath = Path.Combine(ProjectRoot(), PreviewPath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)); File.WriteAllBytes(fullPath, image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previousActive;
                target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
            AssetDatabase.ImportAsset(PreviewPath, ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("DICEFRE_NOVICE_CORNBERG_PREVIEW_DETAILS scene=" + scene.path + " resolution=" + width + "x" + height);
        }

        private static object BuildWindows()
        {
            string root = Path.Combine(Path.GetTempPath(), "DiceFree-Novice-Cornberg-Issue43");
            Directory.CreateDirectory(root);
            string output = Path.Combine(root, "DiceFree.exe");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = output, target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            bool success = report.summary.result == BuildResult.Succeeded;
            string marker = success ? "DICEFRE_NOVICE_CORNBERG_PLAYER_BUILD_OK" : "DICEFRE_NOVICE_CORNBERG_PLAYER_BUILD_FAIL";
            Debug.Log(marker + " result=" + report.summary.result + " errors=" + report.summary.totalErrors + " warnings=" + report.summary.totalWarnings + " output=" + output + " outputBytes=" + report.summary.totalSize);
            return new { success, validation = "novice-cornberg-player-build", finalMarker = marker, output, outputBytes = report.summary.totalSize, errors = report.summary.totalErrors, warnings = report.summary.totalWarnings, testedGitSha = GitHead() };
        }

        private static CombatActor FindPlayer(Scene scene)
        {
            var players = scene.GetRootGameObjects().Where(go => go.name == PlayerName).Select(go => go.GetComponent<CombatActor>()).Where(actor => actor != null).ToArray();
            Require(players.Length == 1, "Expected exactly one authored Cornberg player actor root named " + PlayerName + ".");
            return players[0];
        }

        private static string ProjectRoot() => Directory.GetParent(Application.dataPath).FullName;
        private static string GitHead()
        {
            try
            {
                string safeRoot = ProjectRoot().Replace('\\', '/');
                var start = new ProcessStartInfo("git", "-c \"safe.directory=" + safeRoot + "\" rev-parse HEAD") { WorkingDirectory = ProjectRoot(), UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
                using (var process = Process.Start(start)) { string value = process.StandardOutput.ReadToEnd().Trim(); process.WaitForExit(); return process.ExitCode == 0 ? value : ""; }
            }
            catch { return ""; }
        }
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
