using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class CornbergPlayerBuild
    {
        public static void RecreateAndBuild()
        {
            if (!Application.isBatchMode)
                throw new System.InvalidOperationException("Use the confirmed Recreate menu when working interactively.");
            CornbergSceneBuilder.Create();
            CornbergValidation.ValidateNavigation();
            Build();
        }

        [MenuItem("DiceFree/Build/Windows Cornberg playtest")]
        public static void Build()
        {
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultIsNativeResolution = false;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 800;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = System.Array.ConvertAll(
                    System.Array.FindAll(EditorBuildSettings.scenes, value => value.enabled),
                    value => value.path),
                locationPathName = "Builds/Cornberg/DiceFree.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.InvalidOperationException("Cornberg player build failed: " + report.summary.result);
            Debug.Log("CORNBERG_PLAYER_BUILD_OK: " + report.summary.totalSize + " bytes");
        }
    }
}
