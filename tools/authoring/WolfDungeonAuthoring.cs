using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Dungeons;
using DiceFree.Ironpine;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
public static class WolfDungeonAuthoring
{
 public const string Folder="Assets/_DiceFree/Settings/WolfSurvival";
 public const string ScenePath="Assets/_DiceFree/Scenes/WolfSurvival.unity";
 static Transform root;static Material soil,trail,wood,stone,glow;static WolfDungeonTuning tuning;
 static readonly Vector3 Origin=WolfDungeonRun.Origin;
 static void Need(bool b,string text){if(!b)throw new InvalidOperationException(text);}
 static Vector3 P(float x,float y,float z)=>new(x,y,z);
 public static object Author()
 {
  var world=SceneManager.GetActiveScene();Need(world.name=="Cornberg"&&!world.isDirty&&!EditorApplication.isPlayingOrWillChangePlaymode,"Clean Cornberg Edit Mode required");
  Need(!AssetDatabase.IsValidFolder(Folder)&&!File.Exists(ScenePath)&&GameObject.Find("Wolf Survival entrance")==null,"Already authored; preserve existing work");
  const string recovery="Assets/_DiceFree/Settings/Recovery/Cornberg-before-wolf-survival-20261105.unity";Need(!File.Exists(recovery)&&EditorSceneManager.SaveScene(world,recovery,true),"Recovery already exists or failed");
  AssetDatabase.CreateFolder("Assets/_DiceFree/Settings","WolfSurvival");
  tuning=ScriptableObject.CreateInstance<WolfDungeonTuning>();AssetDatabase.CreateAsset(tuning,Folder+"/Wolf Survival.asset");
  var definition=ScriptableObject.CreateInstance<DungeonDefinition>();definition.stableId="dungeon.ironpine.wolf-survival";definition.sceneId="WolfSurvival";definition.stagingSeconds=60;AssetDatabase.CreateAsset(definition,Folder+"/Dungeon definition.asset");tuning.definition=definition;
  tuning.weak=Template(WildlifeKind.GreyWolf,15,"Weak Wolf","weak");tuning.strong=Template(WildlifeKind.DarkWolf,18,"Strong Wolf","strong");tuning.alpha=Template(WildlifeKind.AlphaWolf,20,"Alpha Wolf","alpha");
  var dummy=Object.Instantiate(tuning.weak.GetComponent<ActorStats>().Definition);dummy.stableId="practice.wolf-survival";dummy.displayName="Practice target";dummy.experienceReward=0;dummy.baseHp=500;AssetDatabase.CreateAsset(dummy,Folder+"/Practice target.asset");tuning.dummy=dummy;EditorUtility.SetDirty(tuning);
  soil=Mat("Moonlit forest floor",new Color(.22f,.29f,.25f));trail=Mat("Trampled wolf trails",new Color(.29f,.27f,.22f));wood=Mat("Weathered timber",new Color(.19f,.14f,.095f));stone=Mat("Forest slate",new Color(.28f,.33f,.39f));glow=Mat("Moonlit trail markers",new Color(.33f,.75f,.89f));glow.EnableKeyword("_EMISSION");glow.SetColor("_EmissionColor",new Color(.2f,.65f,.85f)*1.7f);EditorUtility.SetDirty(glow);
  BuildArena();Polish();SceneManager.SetActiveScene(world);InstallPortal(world);
  var scenes=EditorBuildSettings.scenes.ToList();scenes.Add(new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(world);Need(EditorSceneManager.SaveScene(world),"World entrance save failed");
  var result=new{scene=ScenePath,waves=10,firstPoints=3,finalPoints=12,finalAlphas=4,location="Moonlit Ironpine clearing",entrance=GameObject.Find("Wolf Survival entrance").transform.position.ToString(),wolfLevels=new[]{15,18,20},originalWorldWolvesUnchanged=true};File.WriteAllText("Logs/WolfDungeonAuthoring.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
 static GameObject Template(WildlifeKind kind,int level,string title,string id)
 {
  var source=Object.FindObjectsByType<IronpineWildlife>().First(w=>w.Kind==kind&&w.GetComponent<ActorStats>().Level==level);
  var copy=Object.Instantiate(source.gameObject);copy.name=title;copy.transform.SetParent(null);copy.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
  foreach(var c in copy.GetComponents<MonoBehaviour>())if(!(c is TraversalMotor||c is ActorStats||c is Health||c is CombatActor||c is BasicAttack||c is AggroBehaviour||c is CombatStatusController||c is DefeatReporter||c is IronpineWildlife||c is IronpineCreaturePresentation||c is IronpineCreatureAudio))Object.DestroyImmediate(c);
  var def=Object.Instantiate(source.GetComponent<ActorStats>().Definition);def.stableId="enemy.wolf-survival."+id;def.displayName=title;AssetDatabase.CreateAsset(def,Folder+"/"+title+" stats.asset");copy.GetComponent<ActorStats>().Configure(def,level);
  var wildlife=copy.GetComponent<IronpineWildlife>();wildlife.Configure(kind,wildlife.Visual,Array.Empty<Vector3>());
  var agent=copy.GetComponent<NavMeshAgent>();agent.enabled=false;
  var saved=PrefabUtility.SaveAsPrefabAsset(copy,Folder+"/"+title+".prefab");Object.DestroyImmediate(copy);return saved;
 }
 static Material Mat(string name,Color color){var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color};m.SetFloat("_Smoothness",.12f);AssetDatabase.CreateAsset(m,Folder+"/"+name+".mat");return m;}
 static GameObject Part(string name,PrimitiveType type,Transform parent,Vector3 local,Vector3 scale,Material m,int layer=2)
 {
  var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=local;go.transform.localScale=scale;go.layer=layer;go.GetComponent<Renderer>().sharedMaterial=m;if(layer==2)Object.DestroyImmediate(go.GetComponent<Collider>());return go;
 }
 static GameObject Tree(Transform parent,string type,Vector3 local,float scale)
 {
  var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_DiceFree/Art/World/CornbergBatch/Trees/Tree_"+type+".fbx");Need(source!=null,"Existing forest source missing");var go=(GameObject)PrefabUtility.InstantiatePrefab(source);go.transform.SetParent(parent,false);go.transform.localPosition=local;go.transform.localScale=Vector3.one*scale;
  foreach(var t in go.GetComponentsInChildren<Transform>())t.gameObject.layer=2;
  foreach(var r in go.GetComponentsInChildren<Renderer>())
  {
   r.sharedMaterials=r.sharedMaterials.Select(original=>
   {
    string key=original==null?"Default":original.name.Replace("DF_","").Replace(" (Instance)","").Replace("_mesh","");
    var existing=AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Materials/CornbergBatch/"+key+"_0.mat");
    return existing??(key.Contains("Foliage")||key.Contains("Leaf")?soil:wood);
   }).ToArray();
  }
  go.AddComponent<DiceFree.Core.TreeCameraOccluder>().ConfigureFocusDistance(18);return go;
 }
 static void Disk(string name,Vector3 local,float radius,Material material,bool collider)
 {
  var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();v.Add(Vector3.zero);uv.Add(Vector2.zero);
  for(int i=0;i<=128;i++){float a=i*Mathf.PI*2/128;var p=P(Mathf.Sin(a)*radius,0,Mathf.Cos(a)*radius);v.Add(p);uv.Add(new Vector2(p.x,p.z)/5);}
  for(int i=0;i<128;i++)t.AddRange(new[]{0,i+1,i+2});
  var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();if(collider)mesh.SetPreBakeCollisionMesh(false,true);AssetDatabase.CreateAsset(mesh,Folder+"/"+name+".asset");
  var go=new GameObject(name);go.transform.SetParent(root,false);go.transform.localPosition=local;go.layer=collider?8:2;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;if(collider)go.AddComponent<MeshCollider>().sharedMesh=mesh;
 }
 static void BuildArena()
 {
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);root=new GameObject("Moonlit Clearing - Wolf Survival").transform;root.position=Origin;
  Disk("Continuous clearing floor",Vector3.zero,36,soil,true);Disk("Central trampled clearing",P(0,.025f,0),20,trail,false);
  for(int ring=0;ring<2;ring++)for(int i=0;i<42;i++)
  {
   float a=i*Mathf.PI*2/42+ring*.075f,r=31+ring*5.5f;
   var p=P(Mathf.Sin(a)*r,0,Mathf.Cos(a)*r);Tree(root,i%3==0?"Spruce":"Pine",p,1.0f+(i%4)*.17f);
   if(ring==0){var solid=new GameObject("Boundary tree trunk");solid.layer=9;solid.transform.SetParent(root,false);solid.transform.localPosition=p;var c=solid.AddComponent<CapsuleCollider>();c.radius=.8f;c.height=7;c.center=Vector3.up*3.5f;}
  }
  foreach(var gate in new[]{P(-23,0,9),P(23,0,9),P(0,0,25)})
  {
   var dir=gate.normalized;var perpendicular=Vector3.Cross(Vector3.up,dir);var path=Part("Pack entry trail",PrimitiveType.Cube,root,gate*.64f+Vector3.up*.035f,P(4,.04f,gate.magnitude*.8f),trail);path.transform.localRotation=Quaternion.LookRotation(dir);
   foreach(int side in new[]{-1,1})
   {
    var p=gate+perpendicular*side*3.1f;Part("Old trail marker",PrimitiveType.Cylinder,root,p+Vector3.up*1.0f,P(.45f,1,.45f),wood);
    Part("Moonstone trail lamp",PrimitiveType.Sphere,root,p+Vector3.up*2.15f,P(.42f,.55f,.42f),glow);
    var light=new GameObject("Soft trail glow").AddComponent<Light>();light.transform.SetParent(root,false);light.transform.localPosition=p+Vector3.up*2.4f;light.type=LightType.Point;light.range=8;light.intensity=1.2f;light.color=new Color(.38f,.7f,1);light.shadows=LightShadows.None;
   }
  }
  for(int i=0;i<9;i++)
  {
   float a=(i*40+14)*Mathf.Deg2Rad;var p=P(Mathf.Sin(a)*27.5f,0,Mathf.Cos(a)*27.5f);
   var rock=Part("Slate at clearing edge",PrimitiveType.Cube,root,p+Vector3.up*.5f,P(1.7f,.95f,1.3f),stone,9);rock.transform.localRotation=Quaternion.Euler(i*3,17+i*31,9);
  }
  var moon=new GameObject("Clearing moonlight").AddComponent<Light>();moon.transform.SetParent(root,false);moon.type=LightType.Directional;moon.color=new Color(.55f,.7f,1);moon.intensity=.95f;moon.shadows=LightShadows.Soft;moon.transform.rotation=Quaternion.Euler(57,-32,0);
  root.gameObject.AddComponent<WolfArenaPresentation>();
  Physics.SyncTransforms();var sources=new List<NavMeshBuildSource>();var bounds=new Bounds(Origin+Vector3.up*4,P(85,25,85));
  var markups=root.GetComponentsInChildren<Collider>().Where(c=>c.gameObject.layer==9).Select(c=>new NavMeshBuildMarkup{root=c.transform,overrideArea=true,area=1}).ToList();
  NavMeshBuilder.CollectSources(bounds,(1<<8)|(1<<9),NavMeshCollectGeometry.PhysicsColliders,0,markups,sources);
  var settings=NavMesh.GetSettingsByID(0);settings.agentRadius=.75f;settings.agentHeight=2.2f;settings.agentClimb=.4f;settings.overrideVoxelSize=true;settings.voxelSize=.16f;
  var data=NavMeshBuilder.BuildNavMeshData(settings,sources,bounds,Vector3.zero,Quaternion.identity);Need(data!=null,"Arena navigation failed");AssetDatabase.CreateAsset(data,Folder+"/Wolf Survival NavMesh.asset");root.gameObject.AddComponent<WorldNavigation>().Configure(data);
  Need(EditorSceneManager.SaveScene(scene,ScenePath),"Arena scene save failed");EditorSceneManager.CloseScene(scene,true);
 }
 static void InstallPortal(Scene world)
 {
  SceneManager.SetActiveScene(world);var nav=Object.FindAnyObjectByType<WorldNavigation>();var instance=NavMesh.AddNavMeshData(nav.Data);Vector3 at;
  try{Need(NavMesh.SamplePosition(P(174,0,89),out var hit,10,-1),"No safe Ironpine entrance approach");at=hit.position;}finally{if(instance.valid)instance.Remove();}
  var go=new GameObject("Wolf Survival entrance");go.layer=11;go.transform.position=at;var portal=go.AddComponent<WolfDungeonPortal>();portal.tuning=tuning;portal.ConfigureName("Moonlit Clearing — Wolf Survival");var collider=go.AddComponent<CapsuleCollider>();collider.radius=.8f;collider.height=3;collider.center=Vector3.up*1.5f;
  foreach(int side in new[]{-1,1}){var p=P(side*2.1f,.9f,0);Part("Trail cairn",PrimitiveType.Cube,go.transform,p,P(.75f,1.8f,.8f),stone);Part("Moonlit marker",PrimitiveType.Sphere,go.transform,p+Vector3.up*1.1f,Vector3.one*.5f,glow);}
  var arch=Part("Clearing trail lintel",PrimitiveType.Cube,go.transform,P(0,2.9f,0),P(5,.38f,.6f),wood);arch.transform.localRotation=Quaternion.Euler(0,0,-3);
  var label=new GameObject("Wolf Survival trail sign").AddComponent<TextMesh>();label.transform.SetParent(go.transform,false);label.transform.localPosition=P(0,3.7f,0);label.transform.localRotation=Quaternion.Euler(52,45,0);label.text="MOONLIT CLEARING\nWolf Survival • 10 waves";label.characterSize=.075f;label.fontSize=42;label.anchor=TextAnchor.MiddleCenter;label.color=new Color(.75f,.9f,1);
 }
 public static object Polish()
 {
  Need(!EditorApplication.isPlayingOrWillChangePlaymode,"Edit Mode required");var previous=SceneManager.GetActiveScene();var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
  try
  {
   root=scene.GetRootGameObjects().Single(g=>g.name=="Moonlit Clearing - Wolf Survival").transform;Need(root.Find("Clearing polish")==null,"Polish already authored");SceneManager.SetActiveScene(scene);
   soil=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Moonlit forest floor.mat");trail=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Trampled wolf trails.mat");wood=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Weathered timber.mat");stone=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Forest slate.mat");
   void PaintTexture(Material mat,string name,bool dirt)
   {
    const int n=512;var tex=new Texture2D(n,n,TextureFormat.RGB24,true);var pixels=new Color[n*n];
    for(int y=0;y<n;y++)for(int x=0;x<n;x++)
    {
     float a=Mathf.PerlinNoise(x*.025f,y*.025f),b=Mathf.PerlinNoise(x*.17f+23,y*.17f+11),grain=Mathf.PerlinNoise(x*.71f,y*.71f);
     var color=dirt?Color.Lerp(new Color(.20f,.16f,.11f),new Color(.39f,.33f,.23f),a):Color.Lerp(new Color(.11f,.18f,.095f),new Color(.28f,.32f,.18f),a);
     color*=.84f+b*.25f+grain*.12f;if(!dirt&&b>.63f)color=Color.Lerp(color,new Color(.32f,.25f,.13f),(b-.63f)*1.5f);pixels[y*n+x]=color;
    }
    tex.SetPixels(pixels);tex.Apply();string path=Folder+"/"+name+".png";File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);mat.color=Color.white;EditorUtility.SetDirty(mat);
   }
   PaintTexture(soil,"Forest duff",false);PaintTexture(trail,"Packed earth",true);
   Disk("Forest beyond the clearing",P(0,-.08f,0),68,soil,false);
   var polish=new GameObject("Clearing polish").transform;polish.SetParent(root,false);
   for(int ring=0;ring<2;ring++)for(int i=0;i<47;i++)
   {float a=(i*360f/47+ring*4)*Mathf.Deg2Rad,r=44+ring*10+Mathf.Sin(i*2.3f)*2;Tree(polish,i%4==0?"Spruce":"Pine",P(Mathf.Sin(a)*r,0,Mathf.Cos(a)*r),1.2f+i%5*.13f);}
   var vertices=new List<Vector3>();var triangles=new List<int>();
   void Tri(Vector3 a,Vector3 b,Vector3 c){int at=vertices.Count;vertices.AddRange(new[]{a,b,c});triangles.AddRange(new[]{at,at+1,at+2});}
   Vector3 Rock(int ring,int i){float a=i*Mathf.PI*2/9;float r=(ring==1?.52f:.35f)*(1+.13f*Mathf.Sin(i*2.7f+ring));return P(Mathf.Sin(a)*r,(ring-1)*.48f,Mathf.Cos(a)*r);}
   for(int i=0;i<9;i++)
   {int j=(i+1)%9;for(int ring=0;ring<2;ring++){Tri(Rock(ring,i),Rock(ring+1,j),Rock(ring+1,i));Tri(Rock(ring,i),Rock(ring,j),Rock(ring+1,j));}Tri(P(0,.5f,0),Rock(2,i),Rock(2,j));Tri(P(0,-.5f,0),Rock(0,j),Rock(0,i));}
   var rock=new Mesh{name="Original faceted forest boulder"};rock.SetVertices(vertices);rock.SetTriangles(triangles,0);rock.RecalculateNormals();rock.RecalculateBounds();AssetDatabase.CreateAsset(rock,Folder+"/Faceted boulder.asset");
   foreach(var t in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Slate at clearing edge"))t.GetComponent<MeshFilter>().sharedMesh=rock;
   var fern=Mat("Forest floor leaves",new Color(.16f,.28f,.12f));fern.SetFloat("_Cull",0);EditorUtility.SetDirty(fern);
   vertices.Clear();triangles.Clear();
   for(int i=0;i<60;i++)
   {
    float a=(i*137.5f)*Mathf.Deg2Rad,r=25.8f+i%5*.47f;var at=P(Mathf.Sin(a)*r,.06f,Mathf.Cos(a)*r);
    for(int leaf=0;leaf<7;leaf++){float b=leaf*Mathf.PI*2/7;var dir=P(Mathf.Sin(b),0,Mathf.Cos(b));var side=Vector3.Cross(Vector3.up,dir)*.14f;var tip=at+dir*(.55f+i%3*.13f)+Vector3.up*.26f;Tri(at,at+dir*.25f+side+Vector3.up*.35f,tip);Tri(at,tip,at+dir*.25f-side+Vector3.up*.35f);}
   }
   var foliage=new Mesh{name="Original clearing floor plants"};foliage.SetVertices(vertices);foliage.SetTriangles(triangles,0);foliage.RecalculateNormals();foliage.RecalculateBounds();AssetDatabase.CreateAsset(foliage,Folder+"/Clearing floor plants.asset");var plants=new GameObject("Low ferns at forest edge");plants.layer=2;plants.transform.SetParent(polish,false);plants.AddComponent<MeshFilter>().sharedMesh=foliage;plants.AddComponent<MeshRenderer>().sharedMaterial=fern;
   for(int i=0;i<8;i++)
   {float a=(i*45+19)*Mathf.Deg2Rad;var log=Part("Fallen timber at forest edge",PrimitiveType.Cylinder,polish,P(Mathf.Sin(a)*28,.33f,Mathf.Cos(a)*28),P(.6f,1.7f,.6f),wood);log.transform.localRotation=Quaternion.Euler(90,25+i*35,0);}
   AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Need(EditorSceneManager.SaveScene(scene),"Polished arena save failed");return new{groundTextures=true,forestSkirt=true,additionalTrees=94,facetedRocks=9,fernClusters=60,fallenTimber=8};
  }
  finally{EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(previous);}
 }
}
