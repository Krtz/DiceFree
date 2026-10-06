using System;
using System.IO;
using System.Linq;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class PhysicalBlessedBuildValidation
    {
        [CliCommand("dicefree.physical.build-windows", "Build the #51 Physically Blessed Windows Development player with StartMenu as scene 0.", Tags = new[] { "build", "physical" })]
        public static object BuildWindows()
        {
            PhysicalBlessedValidation.Validate();

            var scenes = EditorBuildSettings.scenes
                .Where(value => value.enabled)
                .Select(value => value.path)
                .ToArray();

            PhysicalBlessedValidation.Require(
                scenes.Length >= 2 &&
                scenes[0] == AdvancementAuthoring.StartMenuScenePath &&
                scenes[1] == CornbergSceneBuilder.ScenePath,
                "Physical build must preserve StartMenu -> Cornberg scene order.");

            string directory = Path.Combine(Path.GetTempPath(), "DiceFree-Physical-Issue51");
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);
            string output = Path.Combine(directory, "DiceFree.exe");

            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultIsNativeResolution = false;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 800;

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });

            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException(
                    "Physically Blessed Windows build failed: " + report.summary.result);

            Debug.Log(
                "DICEFREE_PHYSICAL_PLAYER_BUILD_OK: " +
                report.summary.totalSize +
                " bytes; start scene=" +
                scenes[0]);

            return new
            {
                success = true,
                finalMarker = "DICEFREE_PHYSICAL_PLAYER_BUILD_OK",
                result = report.summary.result.ToString(),
                errors = report.summary.totalErrors,
                warnings = report.summary.totalWarnings,
                totalBytes = report.summary.totalSize,
                executable = output,
                firstScene = scenes[0],
                sceneCount = scenes.Length
            };
        }
    }
}
