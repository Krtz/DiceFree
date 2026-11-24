using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class WorldPointerFeedbackAuthoring
{
    public static void Batch()
    {
        const string folder="Assets/_DiceFree/Resources/WorldHoverFeedback";
        Directory.CreateDirectory(folder);
        AssetDatabase.Refresh();
        var outline=Shader.Find("DiceFree/World Hover Outline");
        if(outline==null || !outline.isSupported) throw new InvalidOperationException("Hover outline shader unavailable.");
        Create(folder+"/Outline.mat",outline,material=>material.SetFloat("_OutlineWidth",2.2f));
        Create(folder+"/Mask.mat",Shader.Find("DiceFree/World Hover Mask"),material=>{});
        Create(folder+"/Ring.mat",Shader.Find("Universal Render Pipeline/Unlit"),material=>
        {
            material.SetFloat("_Surface",1); material.SetFloat("_Blend",0);
            material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite",0); material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType","Transparent");
            material.SetColor("_BaseColor",Color.white); material.renderQueue=2950;
        });
        AssetDatabase.SaveAssets();
        Debug.Log("DICEFREE_POINTER_MATERIALS_OK");
    }
    private static void Create(string path,Shader shader,Action<Material> configure)
    {
        if(shader==null) throw new InvalidOperationException("Missing material shader: "+path);
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null) { material=new Material(shader); AssetDatabase.CreateAsset(material,path); }
        material.shader=shader; configure(material); EditorUtility.SetDirty(material);
    }
}
