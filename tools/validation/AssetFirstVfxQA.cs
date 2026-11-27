using System;
using System.IO;
using System.Linq;
using DiceFree.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AssetFirstVfxQA
{
 [Serializable]public class Report{public string status="passed";public int entries,auras,skillTemplates;public string[] keys;}
 public static void Build(){Validate();BuildFireMage.Run();}
 public static void Validate()
 {
  var library=Resources.Load<AssetPackVfxLibrary>("AssetLibraryLicensed/VisualEffects");if(library==null||library.entries.Length!=29)throw new Exception("Expected29 native bindings");
  if(library.entries.Select(e=>e.key).Distinct().Count()!=29)throw new Exception("Duplicate binding key");
  foreach(var entry in library.entries){
   if(entry.prefab==null||entry.unitRadius<.1f||entry.unitRadius>100)throw new Exception("Invalid binding: "+entry.key);
   if(entry.prefab.GetComponentsInChildren<ParticleSystem>(true).Length==0)throw new Exception("No native particles: "+entry.key);
   if(entry.prefab.GetComponentsInChildren<Collider>(true).Length!=0||entry.prefab.GetComponentsInChildren<Rigidbody>(true).Length!=0||entry.prefab.GetComponentsInChildren<AudioSource>(true).Length!=0||entry.prefab.GetComponentsInChildren<Light>(true).Length!=0||entry.prefab.GetComponentsInChildren<MonoBehaviour>(true).Length!=0)throw new Exception("Effect contains non-decorative or demo components: "+entry.key);
   foreach(var t in entry.prefab.GetComponentsInChildren<Transform>(true))if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)!=0)throw new Exception("Missing script in "+entry.key);
   foreach(var renderer in entry.prefab.GetComponentsInChildren<Renderer>(true)){
    if(renderer.sharedMaterial==null||renderer.sharedMaterials.Any(m=>m!=null&&(m.shader==null||!m.shader.isSupported)))throw new Exception("Unsupported material "+entry.key);
    if(entry.key.StartsWith("aura.")&&renderer.sharedMaterials.Any(m=>m!=null&&m.renderQueue!=3040))throw new Exception("Aura must draw over transparent roads");
   }
  }
  foreach(GroundAuraStyle style in Enum.GetValues(typeof(GroundAuraStyle)))if(library.Find("aura."+style)==null)throw new Exception("Missing aura style "+style);
  Directory.CreateDirectory("Logs");File.WriteAllText("Logs/AssetFirstVfxReview.json",JsonUtility.ToJson(new Report{entries=29,auras=9,skillTemplates=20,keys=library.entries.Select(e=>e.key).ToArray()},true));EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");Debug.Log("DICEFREE_NATIVE_ASSET_VFX_OK29");
 }
}