using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class WorldTwoRoutesStaticReview
{
    public static void Batch()
    {
        var paths=AssetDatabase.FindAssets("",new[]{WorldTwoLibraryAuthoring.Output}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>!AssetDatabase.IsValidFolder(p)).ToArray();
        var dependencies=AssetDatabase.GetDependencies(paths,true);
        var external=dependencies.Where(p=>p.StartsWith("Assets/")&&!p.StartsWith(WorldTwoLibraryAuthoring.Output+"/")).ToArray();
        if(external.Length>0)throw new InvalidOperationException("Private source dependency: "+string.Join(",",external));
        File.WriteAllText("Logs/WorldTwoRoutesStaticReview.json",JsonUtility.ToJson(new Record{portableAssets=paths.Length,privateSourceDependencies=external.Length,paths=paths},true));Debug.Log("DICEFREE_WORLD_TWO_ASSET_DEPENDENCIES_OK");
    }
    [Serializable]sealed class Record{public int portableAssets,privateSourceDependencies;public string[] paths;}
}
