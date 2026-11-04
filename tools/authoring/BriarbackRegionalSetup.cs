using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using DiceFree.World;
public static class BriarbackRegionalSetup {
 public static object InstallAtmosphere(){var s=SceneManager.GetActiveScene();if(s.name!="Cornberg"||s.isDirty||EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Clean Cornberg required.");const string backup="Assets/_DiceFree/Settings/Recovery/Cornberg-before-regional-atmosphere-20261104.unity";if(System.IO.File.Exists(backup))throw new InvalidOperationException("Recovery exists.");var root=GameObject.Find("14 - Briarback quilboar frontier");if(root.GetComponent<BriarbackRegionalAtmosphere>()!=null)throw new InvalidOperationException("Atmosphere exists.");EditorSceneManager.SaveScene(s,backup,true);root.AddComponent<BriarbackRegionalAtmosphere>().Configure(UnityEngine.Object.FindAnyObjectByType<WorldDayNightCycle>());EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);return "Installed regional day/night blend.";}
 public static object ParticleMeshes(){return AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/_DiceFree/Resources"}).Select(AssetDatabase.GUIDToAssetPath).Select(p=>new {p,go=AssetDatabase.LoadAssetAtPath<GameObject>(p)}).Where(x=>x.go!=null).SelectMany(x=>x.go.GetComponentsInChildren<ParticleSystemRenderer>(true).Select(r=>new{x.p,r})).Select(x=>new{x.p,x.r.name,mesh=x.r.mesh}).Where(x=>x.mesh!=null&&!x.mesh.isReadable).Select(x=>new{x.p,x.name,mesh=x.mesh.name,source=AssetDatabase.GetAssetPath(x.mesh)}).ToArray();}
 public static object MotionBindings()=>AssetDatabase.FindAssets("t:AnimationClip",new[]{"Assets/_DiceFree/Art/Creatures/Briarback/RegionalMotion"}).Select(g=>AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(g))).Select(c=>new{c.name,c.length,bindings=AnimationUtility.GetCurveBindings(c).Where(b=>b.path.EndsWith("RightArm")).Select(b=>b.propertyName).ToArray()}).ToArray();
}
