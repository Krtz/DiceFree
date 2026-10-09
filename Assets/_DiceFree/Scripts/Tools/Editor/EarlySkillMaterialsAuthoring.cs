using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.Pipeline.Commands;

namespace DiceFree.EditorTools
{
    internal static class EarlySkillMaterialsAuthoring
    {
        const string Parent="Assets/_DiceFree";
        const string Folder="Assets/_DiceFree/Resources/EarlySkillVfx";

        [CliCommand("dicefree.visual.skills.materials.install","Create URP particle/line material assets to prevent shader stripping in builds.")]
        public static object Install()
        {
            if (!AssetDatabase.IsValidFolder(Parent+"/Resources"))
                AssetDatabase.CreateFolder(Parent,"Resources");
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder(Parent+"/Resources","EarlySkillVfx");
            var texturePath=Folder+"/SoftGlow.png";
            var bitmap=new Texture2D(64,64,TextureFormat.RGBA32,false);
            for(int y=0;y<64;y++)
                for(int x=0;x<64;x++)
                {
                    float radial=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/32f;
                    bitmap.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-radial),2)));
                }
            bitmap.Apply();
            File.WriteAllBytes(Path.Combine(Directory.GetParent(Application.dataPath).FullName,texturePath),bitmap.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(bitmap);
            AssetDatabase.ImportAsset(texturePath,ImportAssetOptions.ForceUpdate);
            var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);
            importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false;
            importer.SaveAndReimport();
            var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

            Material Make(string name,string shaderName)
            {
                var path=Folder+"/"+name+".mat";
                var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(mat==null)
                {
                    var shader=Shader.Find(shaderName);
                    if(shader==null)throw new InvalidOperationException("Missing VFX shader: "+shaderName);
                    mat=new Material(shader) {name=name};
                    AssetDatabase.CreateAsset(mat,path);
                }
                if(mat.HasProperty("_Surface"))mat.SetFloat("_Surface",1);
                if(mat.HasProperty("_Blend"))mat.SetFloat("_Blend",0);
                if(mat.HasProperty("_SrcBlend"))mat.SetFloat("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if(mat.HasProperty("_DstBlend"))mat.SetFloat("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if(mat.HasProperty("_ZWrite"))mat.SetFloat("_ZWrite",0);
                if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",Color.white);
                mat.SetOverrideTag("RenderType","Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue=3000;
                EditorUtility.SetDirty(mat);
                return mat;
            }
            var spark=Make("SoftSparks","Universal Render Pipeline/Particles/Unlit");
            if(spark.HasProperty("_BaseMap"))spark.SetTexture("_BaseMap",tex);
            if(spark.HasProperty("_MainTex"))spark.SetTexture("_MainTex",tex);
            Make("SoftRing","Universal Render Pipeline/Unlit");
            AssetDatabase.SaveAssets();
            return new{success=true,texture=texturePath,sparkMaterial=spark.name,shader=spark.shader.name};
        }
    }
}
