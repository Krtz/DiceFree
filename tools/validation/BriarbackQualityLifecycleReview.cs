using System;
using System.Linq;
using DiceFree.World;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public static class BriarbackQualityLifecycleReview
{
    public static object Seed()
    {
        if(!EditorApplication.isPlaying||GraphicsQualityRuntime.Pipeline==null)throw new InvalidOperationException("Run in isolated Play Mode.");
        CoreBuildData.instance.Dispose();var assets=CoreBuildData.instance.renderPipelineAssets;
        if(!assets.Any(a=>a==GraphicsQualityRuntime.Pipeline))throw new InvalidOperationException("The transient build-cache regression fixture was not created.");
        return new{cachedSessionClone=true,assets=assets.Select(a=>a.name).ToArray()};
    }
    public static object Verify()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
        var assets=CoreBuildData.instance.renderPipelineAssets;
        if(assets.Any(a=>a==null||!AssetDatabase.Contains(a)))throw new InvalidOperationException("Transient/destroyed pipeline remained in Editor build data.");
        for(int i=0;i<QualitySettings.names.Length;i++){var asset=QualitySettings.GetRenderPipelineAssetAt(i);if(asset==null||!AssetDatabase.Contains(asset))throw new InvalidOperationException("Quality-level pipeline assignment was not restored.");}
        return new{passed=true,allQualityAssignmentsRestored=true,buildCacheUsesProjectAssets=true,assets=assets.Select(a=>a.name).ToArray()};
    }
}
