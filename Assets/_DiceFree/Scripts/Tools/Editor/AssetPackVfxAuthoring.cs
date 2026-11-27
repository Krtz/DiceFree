using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DiceFree.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace DiceFree.Tools.Editor
{
 public static class AssetPackVfxAuthoring
 {
  const string Root="Assets/_DiceFree/Resources/AssetLibraryLicensed/";
  [Serializable]public class Binding{public string key,source,sourceGuid;public float unitRadius,prewarmSeconds;}
  [Serializable]public class Receipt{public string status="passed";public Binding[] bindings;}
  public static void Build()
  {
   Directory.CreateDirectory(Root);Directory.CreateDirectory(Root+"Materials");AssetDatabase.Refresh();
   var library=AssetDatabase.LoadAssetAtPath<AssetPackVfxLibrary>(Root+"VisualEffects.asset");if(library==null){library=ScriptableObject.CreateInstance<AssetPackVfxLibrary>();AssetDatabase.CreateAsset(library,Root+"VisualEffects.asset");}
   var paths=AssetDatabase.GetAllAssetPaths().Where(p=>p.EndsWith(".prefab")).ToArray();
   var specs=new Dictionary<string,string>{
    {"aura.Mana","Bottom02-01"},{"aura.Elements","Bottom06-01"},{"aura.Defender","Bottom20-01"},{"aura.Speed","Bottom14-01"},{"aura.Frost","Bottom13-01"},{"aura.Radiation","Bottom16-01"},{"aura.Ranged","Bottom09-01"},{"aura.Coffee","Bottom11-01"},{"aura.Infernal","Bottom19-01"},
    {"cast.holy","MedicalEffect1"},{"cast.ice","IceEffect3"},{"cast.fire","FireEffect2"},{"cast.lightning","LightningEffect1"},{"cast.poison","PoisonEffect1"},{"cast.earth","DirtyEffect1"},{"cast.buff","HolyEffect4"},{"cast.dark","DarkEffect2"},{"cast.arcane","MagicEffect11"},{"cast.physical","FX_Orange_Slash_1"}
   };
   foreach(var theme in specs.Keys.Where(k=>k.StartsWith("cast.")).ToArray())specs[theme.Replace("cast.","impact.")]=specs[theme];
   var entries=new List<AssetPackVfxLibrary.Entry>();var receipt=new List<Binding>();var materials=new Dictionary<Material,Material>();var scene=EditorSceneManager.NewPreviewScene();
   try{foreach(var pair in specs){
    bool aura=pair.Key.StartsWith("aura.");string source=aura?"Assets/KoreanTraditionalPattern_Effect/Prefabs/Bottom/"+pair.Value+".prefab":paths.First(p=>Path.GetFileNameWithoutExtension(p)==pair.Value&&(p.StartsWith("Assets/100BestEffectPack/")||p.StartsWith("Assets/Eric VFX Studio/")));
    var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(source),scene);PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);go.transform.position=Vector3.zero;
    try{
     foreach(var behaviour in go.GetComponentsInChildren<MonoBehaviour>(true))Object.DestroyImmediate(behaviour);
     foreach(var collider in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(collider);
     foreach(var body in go.GetComponentsInChildren<Rigidbody>(true))Object.DestroyImmediate(body);
     foreach(var audio in go.GetComponentsInChildren<AudioSource>(true))Object.DestroyImmediate(audio);
     foreach(var light in go.GetComponentsInChildren<Light>(true))Object.DestroyImmediate(light);
     foreach(var t in go.GetComponentsInChildren<Transform>(true)){GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);t.gameObject.layer=aura?30:2;}
     foreach(var renderer in go.GetComponentsInChildren<Renderer>(true)){
      if(renderer.sharedMaterial==null||renderer.sharedMaterials.Any(m=>m!=null&&(m.shader==null||!m.shader.isSupported)))throw new Exception("Unsupported source material: "+source);
      renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
      if(aura)renderer.sharedMaterials=renderer.sharedMaterials.Select(original=>{
       if(original==null)return null;if(!materials.TryGetValue(original,out var adapted)){string id=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(original));string target=Root+"Materials/"+id+".mat";adapted=AssetDatabase.LoadAssetAtPath<Material>(target);if(adapted==null){adapted=new Material(original);AssetDatabase.CreateAsset(adapted,target);}else EditorUtility.CopySerialized(original,adapted);adapted.renderQueue=3040;EditorUtility.SetDirty(adapted);materials[original]=adapted;}return adapted;
      }).ToArray();
     }
     var systems=go.GetComponentsInChildren<ParticleSystem>(true);foreach(var ps in systems){ps.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.loop=aura;if(aura){main.startDelay=0;main.duration=Mathf.Clamp(main.startLifetime.constantMax*.65f,.35f,3);main.maxParticles=Mathf.Max(4,main.maxParticles);var emission=ps.emission;for(int b=0;b<emission.burstCount;b++){var burst=emission.GetBurst(b);burst.time=Mathf.Min(burst.time,main.duration*.5f);emission.SetBurst(b,burst);}}main.stopAction=ParticleSystemStopAction.None;main.playOnAwake=true;main.scalingMode=ParticleSystemScalingMode.Hierarchy;ps.useAutoRandomSeed=false;ps.randomSeed=123;ps.Simulate(1.6f,false,true,true);}
     var renderers=go.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();if(renderers.Length==0)throw new Exception("No source renderers: "+source);var bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);float radius=Mathf.Max(.1f,Mathf.Max(bounds.size.x,bounds.size.z)*.5f);if(aura){var pattern=systems.FirstOrDefault(p=>p.name=="Pattern");if(pattern!=null)radius=Mathf.Max(.1f,pattern.main.startSize.constantMax*.5f*Mathf.Max(Mathf.Abs(pattern.transform.lossyScale.x),Mathf.Abs(pattern.transform.lossyScale.z)));}if(radius>100)throw new Exception("Invalid effect bounds: "+source);
     float warmup=0;if(!aura){bool emits=false;foreach(float time in new[]{.04f,.08f,.16f,.25f,.4f,.6f}){foreach(var ps in systems)ps.Simulate(time,false,true,true);if(systems.Any(p=>p.gameObject.activeInHierarchy&&p.particleCount>0)){warmup=time;emits=true;break;}}if(!emits)throw new Exception("Native effect does not emit particles: "+source);}
     foreach(var ps in systems){ps.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);ps.useAutoRandomSeed=true;}
     string assetPath=Root+pair.Key.Replace('.','-')+".prefab";var prefab=PrefabUtility.SaveAsPrefabAsset(go,assetPath);if(prefab==null)throw new Exception("Failed to save curated prefab: "+source);entries.Add(new AssetPackVfxLibrary.Entry{key=pair.Key,prefab=prefab,unitRadius=radius,prewarmSeconds=warmup});receipt.Add(new Binding{key=pair.Key,source=source,sourceGuid=AssetDatabase.AssetPathToGUID(source),unitRadius=radius,prewarmSeconds=warmup});
    }finally{Object.DestroyImmediate(go);}
   }}finally{EditorSceneManager.ClosePreviewScene(scene);}
   library.entries=entries.ToArray();EditorUtility.SetDirty(library);AssetDatabase.SaveAssets();Directory.CreateDirectory("docs/assets");File.WriteAllText("docs/assets/asset-vfx-bindings-2026-11-27.json",JsonUtility.ToJson(new Receipt{bindings=receipt.ToArray()},true));Debug.Log("DICEFREE_ASSET_VFX_LIBRARY_OK "+entries.Count+" native pack bindings");
  }
 }
}