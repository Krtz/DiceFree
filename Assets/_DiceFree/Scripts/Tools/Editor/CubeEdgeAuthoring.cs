using System;using System.Collections.Generic;using System.Linq;
using DiceFree.Development;using DiceFree.World;using UnityEngine;using UnityEngine.AI;
using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.SceneManagement;using Unity.Pipeline.Commands;
using Object=UnityEngine.Object;
namespace DiceFree.EditorTools{
public static class CubeEdgeAuthoring{
public const string Folder="Assets/_DiceFree/Art/World/CubePrototype";
static readonly string[] Scenes={"DevelopmentRoom","World2TransitionTest"};
static string Preview(int i)=>Folder+"/"+Scenes[i]+"Face.prefab";
static void FolderFor(string p){if(AssetDatabase.IsValidFolder(p))return;var parent=System.IO.Path.GetDirectoryName(p).Replace('\\','/');FolderFor(parent);AssetDatabase.CreateFolder(parent,System.IO.Path.GetFileName(p));}
static GameObject Cube(string name,Transform parent,Vector3 at,Vector3 scale,Material material,int layer=9){
var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.layer=layer;g.transform.SetParent(parent);g.transform.position=at;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=material;return g;}
static Material Pips(){string p=Folder+"/FacePips.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.SetColor("_BaseColor",new Color(1,.82f,.38f));m.SetFloat("_Smoothness",.15f);EditorUtility.SetDirty(m);return m;}
[CliCommand("dicefree.cube-edge.author","Extend only the two test maps with square cube faces, seam crossings, mesh-only neighbour previews and navigation.")]
public static object Run(){
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
var setup=EditorSceneManager.GetSceneManagerSetup();if(setup.Any(s=>SceneManager.GetSceneByPath(s.path).isDirty))throw new InvalidOperationException("Save scenes first.");
FolderFor(Folder);var pips=Pips();
try{
for(int i=0;i<2;i++){
var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/"+Scenes[i]+".unity");
var map=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<DevelopmentMap>(true)).Single();
var origin=new Vector3(map.bounds.center.x,0,map.bounds.center.y);map.bounds=new Rect(origin.x-26,origin.z-26,52,52);
var floor=map.transform.Find("Test floor");floor.localScale=new Vector3(48,.5f,48);
var trim=map.transform.Find("East wall").GetComponent<Renderer>().sharedMaterial;
foreach(var name in new[]{"North wall","South wall"}){
var wall=map.transform.Find(name);if(wall==null)continue;
wall.position=origin+new Vector3(0,1.5f,name=="North wall"?24:-24);wall.localScale=new Vector3(48,3,.6f);
}
foreach(var name in new[]{"East wall","West wall"}){
var wall=map.transform.Find(name);wall.position=origin+new Vector3(name=="East wall"?24:-24,1.5f,0);wall.localScale=new Vector3(.6f,3,48);
}
int sign=i==0?1:-1;string seam=sign>0?"North":"South";
var old=map.transform.Find(seam+" wall");if(old!=null)Object.DestroyImmediate(old.gameObject);
foreach(int side in new[]{-1,1}){
string name=seam+" seam wall "+side;var wall=map.transform.Find(name);
if(wall==null)Cube(name,map.transform,origin+new Vector3(side*13.5f,1.5f,sign*24),new Vector3(21,3,.6f),trim);
}
foreach(var grid in map.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name=="Floor grid")){
var scale=grid.transform.localScale;if(scale.x<1)scale.z=46;else scale.x=46;grid.transform.localScale=scale;
}
foreach(int z in new[]{-20,20})if(map.transform.Find("Outer grid "+z)==null)
Cube("Outer grid "+z,map.transform,origin+new Vector3(0,.012f,z),new Vector3(46,.02f,.025f),trim,2);
foreach(var g in map.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Die pip ")).ToArray())Object.DestroyImmediate(g.gameObject);
for(int pip=0;pip<=i;pip++){
var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name="Die pip "+pip;g.layer=2;g.transform.SetParent(map.transform);
g.transform.position=origin+new Vector3(i==0?7:pip==0?-5:5,.028f,-8);g.transform.localScale=new Vector3(3.8f,.015f,3.8f);
g.GetComponent<Renderer>().sharedMaterial=pips;Object.DestroyImmediate(g.GetComponent<Collider>());
}
var portal=map.GetComponentInChildren<DevelopmentPortal>(true);portal.transform.position=origin+new Vector3(0,.08f,sign*21);
portal.transform.localScale=new Vector3(5,.10f,2.5f);portal.GetComponent<Collider>().isTrigger=true;
portal.ConfigureName(i==0?"Cross the cube edge to World 2":"Cross the cube edge to World 1");
foreach(var text in map.GetComponentsInChildren<TextMesh>(true).Where(t=>t.text.Contains("PORTAL")||t.text.Contains("RETURN TO WORLD 1"))){
text.text=i==0?"CUBE EDGE TO WORLD 2":"CUBE EDGE TO WORLD 1";text.transform.position=origin+new Vector3(0,3,sign*21);}
var edge=map.GetComponent<CubeEdgeMap>()??map.gameObject.AddComponent<CubeEdgeMap>();
edge.outwardSign=sign;edge.halfSize=24;edge.destinationScene=Scenes[1-i];edge.arrivalSpawnId=map.worldId+"."+seam.ToLowerInvariant();
edge.link=new WorldEdgeLink{edgeId=seam.ToLowerInvariant(),destinationFaceId=i==0?"world.test.2":"world.test.1",
destinationSpawnId=(i==0?"world.test.2.south":"world.test.1.north"),transitionStyle=WorldTransitionStyle.CubeRotate};
foreach(var controller in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<DevelopmentRoomController>(true)))
if(controller.GetComponent<CubeEdgePresentation>()==null)controller.gameObject.AddComponent<CubeEdgePresentation>();
Bake(map,origin,Scenes[i]);
BuildPreview(map,origin,Preview(i));
EditorUtility.SetDirty(map);EditorUtility.SetDirty(edge);EditorSceneManager.MarkSceneDirty(scene);
if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Scene save failed");
}
AssetDatabase.SaveAssets();
for(int i=0;i<2;i++){
var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/"+Scenes[i]+".unity");
var map=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<DevelopmentMap>(true)).Single();
var edge=map.GetComponent<CubeEdgeMap>();edge.facePreview=AssetDatabase.LoadAssetAtPath<GameObject>(Preview(i));
edge.adjacentPreview=AssetDatabase.LoadAssetAtPath<GameObject>(Preview(1-i));EditorUtility.SetDirty(edge);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
}
AssetDatabase.SaveAssets();
}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
return new{success=true,faces=2,faceSize=48,sourceSceneUnchanged="Cornberg",marker="DICEFREE_CUBE_EDGE_AUTHORED"};
}
static void BuildPreview(DevelopmentMap map,Vector3 origin,string path){
var root=new GameObject("Visual-only "+map.worldId);
try{foreach(var r in map.GetComponentsInChildren<MeshRenderer>(true)){
if(r.GetComponent<TextMesh>()!=null||!r.enabled)continue;var f=r.GetComponent<MeshFilter>();if(f==null||f.sharedMesh==null)continue;
var g=new GameObject(r.name);g.layer=2;g.transform.SetParent(root.transform);g.transform.SetPositionAndRotation(r.transform.position-origin,r.transform.rotation);g.transform.localScale=r.transform.lossyScale;
g.AddComponent<MeshFilter>().sharedMesh=f.sharedMesh;g.AddComponent<MeshRenderer>().sharedMaterials=r.sharedMaterials;
}PrefabUtility.SaveAsPrefabAsset(root,path);}finally{Object.DestroyImmediate(root);}
}
static void Bake(DevelopmentMap map,Vector3 origin,string name){
Physics.SyncTransforms();var sources=new List<NavMeshBuildSource>();NavMeshBuilder.CollectSources(map.transform,(1<<8)|(1<<9),NavMeshCollectGeometry.PhysicsColliders,0,new List<NavMeshBuildMarkup>(),sources);
var settings=NavMesh.GetSettingsByID(0);settings.agentRadius=.675f;settings.agentHeight=1.8f;
var data=NavMeshBuilder.BuildNavMeshData(settings,sources,new Bounds(origin,new Vector3(55,12,55)),Vector3.zero,Quaternion.identity);
if(data==null)throw new InvalidOperationException("Cube test navigation failed");
var saved=AssetDatabase.LoadAssetAtPath<NavMeshData>("Assets/_DiceFree/Settings/Development/"+name+"NavMesh.asset");
if(saved==null)throw new InvalidOperationException("Existing navigation missing");
EditorUtility.CopySerialized(data,saved);Object.DestroyImmediate(data);EditorUtility.SetDirty(saved);
map.GetComponent<WorldNavigation>().Configure(saved);
}
}}
