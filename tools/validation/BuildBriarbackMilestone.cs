using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Pipeline.Config;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildBriarbackMilestone
{
    public static object Run()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Build from clean Edit Mode.");
        var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
        if(!scenes[0].EndsWith("/StartMenu.unity")||!scenes.Any(s=>s.EndsWith("/DevelopmentRoom.unity")))
            throw new InvalidOperationException("Expected the complete StartMenu-first scene list.");
        var project=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
        var directory=Path.Combine(project,"Builds","BriarbackMilestone-"+DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(directory);
        var paths=new[]{"ProjectSettings/Packages/com.unity.pipeline/RuntimePipelineConfig.json",
            "Assets/Settings/Pipeline/Resources/RuntimePipelineConfig.asset",
            "Assets/Settings/Pipeline/Resources/RuntimePipelineConfig.asset.meta",
            "Assets/Settings/Pipeline/Resources/RuntimePipelineBuildInfo.asset",
            "Assets/Settings/Pipeline/Resources/RuntimePipelineBuildInfo.asset.meta"};
        var backups=paths.ToDictionary(p=>p,p=>File.Exists(p)?File.ReadAllBytes(p):null);
        bool previousDevelopment=EditorUserBuildSettings.development;
        var config=RuntimePipelineConfig.Load()??ScriptableObject.CreateInstance<RuntimePipelineConfig>();
        BuildReport report;
        try
        {
            // Local development-player verification only. Restore authored settings byte-for-byte.
            config.enableInBuilds=true;config.autoStart=true;config.Save();
            EditorUserBuildSettings.development=true;
            DiceFree.EditorTools.BuildWarningRepairs.Run();
            report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,
                locationPathName=Path.Combine(directory,"DiceFree.exe"),target=BuildTarget.StandaloneWindows64,
                options=BuildOptions.Development});
        }
        finally
        {
            EditorUserBuildSettings.development=previousDevelopment;
            foreach(var item in backups)
            {
                var full=Path.GetFullPath(item.Key);
                if(!full.StartsWith(project+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Settings restoration escaped the project.");
                if(item.Value!=null){Directory.CreateDirectory(Path.GetDirectoryName(full));File.WriteAllBytes(full,item.Value);}
                else if(File.Exists(full))File.Delete(full);
            }
            UnityEngine.Object.DestroyImmediate(config);
            AssetDatabase.Refresh();
        }
        var result=new Record{result=report.summary.result.ToString(),errors=report.summary.totalErrors,
            warnings=report.summary.totalWarnings,sceneCount=scenes.Length,bytes=(long)report.summary.totalSize,
            executable=Path.Combine(directory,"DiceFree.exe"),development=true,localRuntimeAutomation=true,
            includesExistingWorkspaceWip=true};
        File.WriteAllText(Path.Combine(directory,"build-report.json"),JsonUtility.ToJson(result,true));
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/BriarbackMilestoneBuildPath.txt",directory);
        if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors>0)throw new InvalidOperationException("Windows build failed its clean-report gate: "+result.result+", errors="+result.errors);
        return result;
    }
    [Serializable]public sealed class Record{public string result,executable;public int errors,warnings,sceneCount;public long bytes;public bool development,localRuntimeAutomation,includesExistingWorkspaceWip;}
}
