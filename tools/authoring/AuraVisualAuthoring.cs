using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AuraVisualAuthoring
{
    public static void Batch()
    {
        const string folder="Assets/_DiceFree/Resources/AuraVfx";
        Directory.CreateDirectory(folder);AssetDatabase.Refresh();
        var shader=Shader.Find("DiceFree/Ground Aura Sigil");
        if(shader==null || !shader.isSupported) throw new InvalidOperationException("Aura sigil shader unavailable.");
        const string path=folder+"/GroundSigil.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null) {material=new Material(shader);AssetDatabase.CreateAsset(material,path);}
        material.shader=shader;material.SetFloat("_Opacity",1);material.renderQueue=2940;
        EditorUtility.SetDirty(material);AssetDatabase.SaveAssets();
        Debug.Log("DICEFREE_AURA_SIGILS_AUTHORED_OK");
    }
}
