using System;using System.IO;using System.Linq;using System.Collections.Generic;
using Unity.Pipeline.Commands;using UnityEditor;using UnityEngine;using UnityEngine.Rendering;
using Object=UnityEngine.Object;
namespace DiceFree.EditorTools
{
 public static class MageVfxAuthoring
 {
  public const string Folder="Assets/_DiceFree/Resources/MageVfxLicensed";
  static void Ensure(string path){if(AssetDatabase.IsValidFolder(path))return;int split=path.LastIndexOf('/');Ensure(path.Substring(0,split));AssetDatabase.CreateFolder(path.Substring(0,split),path.Substring(split+1));}
  static Material Convert(Material source,bool radiationPalette){
   string path=Folder+"/Materials/"+AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source))+".mat";
   var m=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));AssetDatabase.CreateAsset(m,path);}
   var saved=new SerializedObject(source);var textures=saved.FindProperty("m_SavedProperties.m_TexEnvs");Texture texture=null;Vector2 scale=Vector2.one,offset=Vector2.zero;
   foreach(SerializedProperty e in textures){string key=e.FindPropertyRelative("first").stringValue;if(key!="_MainTex"&&key!="_BaseMap")continue;
    var second=e.FindPropertyRelative("second");texture=second.FindPropertyRelative("m_Texture").objectReferenceValue as Texture;scale=second.FindPropertyRelative("m_Scale").vector2Value;offset=second.FindPropertyRelative("m_Offset").vector2Value;if(texture!=null)break;
   }
   Color color=Color.white;var colors=saved.FindProperty("m_SavedProperties.m_Colors");
   foreach(SerializedProperty e in colors){string key=e.FindPropertyRelative("first").stringValue;if(key=="_TintColor"||key=="_Color"||key=="_BaseColor"){color=e.FindPropertyRelative("second").colorValue;break;}}
   if(radiationPalette)color=new Color(.42f,1f,.12f,color.a);
   bool additive=source.shader.name.IndexOf("Additive",StringComparison.OrdinalIgnoreCase)>=0 || (source.HasProperty("_DstBlend")&&source.GetFloat("_DstBlend")==1);
   m.name="Mage URP "+source.name;m.shader=Shader.Find("Universal Render Pipeline/Particles/Unlit");m.SetTexture("_BaseMap",texture);m.SetTextureScale("_BaseMap",scale);m.SetTextureOffset("_BaseMap",offset);m.SetColor("_BaseColor",color);
   m.SetFloat("_Surface",1);m.SetFloat("_Blend",additive?2:0);m.SetFloat("_ZWrite",0);m.SetFloat("_Cull",0);
   m.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);m.SetFloat("_DstBlend",(float)(additive?BlendMode.One:BlendMode.OneMinusSrcAlpha));
   m.SetFloat("_SoftParticlesEnabled",0);m.SetFloat("_CameraFadingEnabled",0);
   m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.DisableKeyword("_ALPHAPREMULTIPLY_ON");m.DisableKeyword("_SOFTPARTICLES_ON");m.SetOverrideTag("RenderType","Transparent");m.renderQueue=3000;EditorUtility.SetDirty(m);return m;
  }
  [CliCommand("dicefree.mage.vfx.author","Build sanitized URP Mage particle prefabs from the selected owned source packs.")]
  public static object Run(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode only");
   AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);Ensure(Folder+"/Materials");
   var lines=File.ReadAllLines("SourceArt/Vfx/Mage/selected-prefabs.tsv");var result=new List<string>();
   foreach(string line in lines){
    var parts=line.Split('\t');if(parts.Length!=2)throw new InvalidOperationException("Invalid source map");
    var root=PrefabUtility.LoadPrefabContents(parts[1]);
    try{
     root.name=parts[0];
     foreach(var t in root.GetComponentsInChildren<Transform>(true)){t.gameObject.layer=2;GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);}
     foreach(var c in root.GetComponentsInChildren<MonoBehaviour>(true))Object.DestroyImmediate(c);
     foreach(var c in root.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
     foreach(var c in root.GetComponentsInChildren<Rigidbody>(true))Object.DestroyImmediate(c);
     foreach(var c in root.GetComponentsInChildren<AudioSource>(true))Object.DestroyImmediate(c);
     foreach(var c in root.GetComponentsInChildren<Light>(true))Object.DestroyImmediate(c);
     foreach(var c in root.GetComponentsInChildren<Animator>(true))Object.DestroyImmediate(c);
     foreach(var ps in root.GetComponentsInChildren<ParticleSystem>(true)){
      var main=ps.main;main.playOnAwake=false;main.scalingMode=ParticleSystemScalingMode.Hierarchy;main.maxParticles=Mathf.Min(main.maxParticles,256);
      if(parts[0]=="RadiationAura")main.startColor=new Color(.55f,1f,.16f,1f);
      if(parts[0]=="SandBurst"){
       main.startSize=Mathf.Min(main.startSize.constant,.65f);main.startSpeed=Mathf.Min(main.startSpeed.constant,.5f);main.startLifetime=Mathf.Min(main.startLifetime.constant,.85f);main.maxParticles=Mathf.Min(main.maxParticles,64);
      }
      if(parts[0]=="SandLoop"){
       main.startSize=Mathf.Min(main.startSize.constant,.45f);main.startSpeed=Mathf.Min(main.startSpeed.constant,.55f);main.startLifetime=Mathf.Min(main.startLifetime.constant,1.2f);main.maxParticles=Mathf.Min(main.maxParticles,80);
      }
      var collision=ps.collision;collision.enabled=false;var trigger=ps.trigger;trigger.enabled=false;var emission=ps.emission;
      if(emission.rateOverTime.mode==ParticleSystemCurveMode.Constant&&emission.rateOverTime.constant>60)emission.rateOverTime=60;
      if(parts[0]=="SandLoop"&&emission.rateOverTime.mode==ParticleSystemCurveMode.Constant)emission.rateOverTime=Mathf.Min(emission.rateOverTime.constant,10f);
      if(parts[0]=="SandBurst"&&emission.rateOverTime.mode==ParticleSystemCurveMode.Constant)emission.rateOverTime=Mathf.Min(emission.rateOverTime.constant,12f);
     }
     foreach(var r in root.GetComponentsInChildren<Renderer>(true)){
      r.sharedMaterials=r.sharedMaterials.Select(m=>m==null?null:Convert(m,parts[0]=="RadiationAura")).ToArray();r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;
     }
     string path=Folder+"/"+parts[0]+".prefab";PrefabUtility.SaveAsPrefabAsset(root,path);
     result.Add(parts[0]+": "+root.GetComponentsInChildren<ParticleSystem>(true).Length+" systems");
    }finally{PrefabUtility.UnloadPrefabContents(root);}
   }
   AssetDatabase.SaveAssets();return new{created=result.ToArray()};
  }
 }
}
