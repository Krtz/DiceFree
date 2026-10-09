using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;
namespace DiceFree.EditorTools {
internal static class CornbergVillageDress02 {
 const string ScenePath="Assets/_DiceFree/Scenes/Cornberg.unity";
 const string AssetsRoot="Assets/_DiceFree/Art/World/VisualBatch01/";
 const string Batch="VIS02_CornbergVillageLife";
 static readonly (string id,float x,float z,float scale,float yaw)[] Props={
 ("barrel_02",-16,14,.9f,10),("barrel_04",-15,14,1.1f,8),("crate_03",-17,15,1.2f,20),
 ("crate_04",-18,15,1.0f,35),("sign_02",-15,19,1.1f,5),("lantern_02",-14,18,.85f,5),
 ("cart_02",11,42,1.1f,47),("hay_03",9,44,1.1f,12),
 ("fence_04",27,50,1,0),("fence_01",30,50,1,0),("fence_02",33,50,1,0),
 ("wheat_01",17,59,1.55f,22),("wheat_03",26,58,1.45f,14),
 ("wheat_04",38,59,1.6f,5),("wheat_05",46,59,1.45f,16),
 ("stump_02",75,38,1.15f,11),("stump_03",79,39,1.05f,37),
 ("log_02",84,38,1.15f,20),("mushroom_03",85,39,1.15f,3),
 ("mushroom_04",88,39,.88f,3),("rock_03",82,37,.9f,47),
 ("tree_02",117,83,1.25f,32),("tree_04",121,81,1.3f,70),
 ("tree_05",126,78,1.2f,10),("pine_03",128,74,1.3f,40),
 ("pine_04",131,70,1.25f,53),("rock_05",125,79,1.1f,17),
 ("slimejar_01",-9,16,.8f,14),("slimejar_02",-10,16,.8f,11),
 ("anvil_01",-19,17,.85f,0)
 };
 static void Folder(string parent,string name){if(!AssetDatabase.IsValidFolder(parent+"/"+name))AssetDatabase.CreateFolder(parent,name);}
 [CliCommand("dicefree.visual02.cornberg.install","Add a second safely nonblocking Cornberg decor pass and URP-colored Blender materials.")]
 public static object Install(){
 if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Requires edit mode");
 for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Unsaved scene");
 var setup=EditorSceneManager.GetSceneManagerSetup();
 try{
 var s=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
 var ex=s.GetRootGameObjects().FirstOrDefault(o=>o.name==Batch);
 if(ex!=null)return new{success=true,alreadyInstalled=true,count=ex.transform.childCount};
 var shader=Shader.Find("Universal Render Pipeline/Lit");if(shader==null)throw new InvalidOperationException("URP Lit unavailable");
 Folder(AssetsRoot.TrimEnd('/'),"Materials");
 var root=new GameObject(Batch);SceneManager.MoveGameObjectToScene(root,s);
 int count=0;int renderers=0;
 foreach(var v in Props){
 var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(AssetsRoot+"Models/"+v.id+".fbx");if(prefab==null)throw new InvalidOperationException("Missing "+v.id);
 var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,s);
 go.name="VIS02_"+v.id+"_"+count;go.transform.SetParent(root.transform,true);
 go.transform.position=new Vector3(v.x,0,v.z);go.transform.rotation=Quaternion.Euler(0,v.yaw,0);go.transform.localScale=Vector3.one*v.scale;
 foreach(var col in go.GetComponentsInChildren<Collider>(true))UnityEngine.Object.DestroyImmediate(col);
 foreach(var r in go.GetComponentsInChildren<Renderer>(true)){
 var src=r.sharedMaterials;var mapped=new Material[src.Length];
 for(int j=0;j<src.Length;j++){
 string n=src[j]!=null?src[j].name:"Stone";n=n.Replace(" (Instance)","");
 string safe=System.Text.RegularExpressions.Regex.Replace(n,@"[^a-zA-Z0-9_-]","_");
 string path=AssetsRoot+"Materials/"+safe+".mat";
 var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
 if(mat==null){
 mat=new Material(shader){name=n};
 var k=n.ToLowerInvariant();Color c=new Color(.55f,.55f,.5f);
 if(k.Contains("oak")||k.Contains("bark"))c=new Color(.28f,.16f,.09f);
 else if(k.Contains("leaf")||k.Contains("moss"))c=new Color(.28f,.47f,.18f);
 else if(k.Contains("crop")||k.Contains("rope"))c=new Color(.71f,.62f,.27f);
 else if(k.Contains("iron"))c=new Color(.2f,.22f,.23f);
 else if(k.Contains("slime"))c=new Color(.15f,.68f,.38f);
 else if(k.Contains("shroom"))c=new Color(.72f,.32f,.19f);
 else if(k.Contains("soil"))c=new Color(.3f,.20f,.12f);
 mat.SetColor("_BaseColor",c);AssetDatabase.CreateAsset(mat,path);
 }mapped[j]=mat;
 }r.sharedMaterials=mapped;renderers++;
 }count++;
 }
 EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();
 return new{success=true,count,renderers,colliders=0};
 }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
 }
 [CliCommand("dicefree.visual02.cornberg.validate","Validate second Cornberg dressing pass is complete and nonblocking.")]
 public static object Validate(){
 var setup=EditorSceneManager.GetSceneManagerSetup();
 try{
 var s=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
 var g=s.GetRootGameObjects().FirstOrDefault(o=>o.name==Batch);
 if(g==null||g.transform.childCount!=Props.Length||g.GetComponentsInChildren<Collider>(true).Length!=0)throw new InvalidOperationException("VIS02 incomplete or colliders exist");
 return new{marker="VIS02_CORNBERG_OK",count=g.transform.childCount,colliders=0};
 }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
 }
}}
