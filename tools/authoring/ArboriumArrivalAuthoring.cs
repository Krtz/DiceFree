using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.EditorTools;
using DiceFree.Quests;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

// Original, reproducible city architecture. No imported art or new asset dependencies.
public static class ArboriumArrivalAuthoring
{
 public const string RootName="Arborium - World Tree capital";
 public const string Folder="Assets/_DiceFree/Art/World/ArboriumArrival";
 public const string QuestPath="Assets/_DiceFree/Settings/Arborium/Road to Arborium.asset";
 public const string QuestId="quest.trimery.road-to-arborium";
 public const string GateId="area.arborium.north-gate";
 public const string RegistrarId="npc.arborium.guild-registrar";
 static Vector3 center=new(98.04f,0,304.74f);
 static readonly List<Material> materials=new();
 static Transform root;
 const int Stone=0,LightStone=1,Dark=2,Roof=3,Gold=4,Glass=5,Grass=6,Paving=7,Wood=8,Cloth=9,Leaf=10;
 static Vector3 P(float x,float y,float z)=>new(x,y,z);
 static void Need(bool value,string text){if(!value)throw new InvalidOperationException(text);}
 public static object Author()
 {
  var scene=SceneManager.GetActiveScene();Need(scene.name=="Cornberg"&&!scene.isDirty&&!EditorApplication.isPlayingOrWillChangePlaymode,"Clean Cornberg Edit Mode required.");
  Need(GameObject.Find(RootName)==null&&!AssetDatabase.IsValidFolder(Folder),"Arborium arrival already authored; do not overwrite it.");
  var trunk=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Single(t=>t.name=="Ancient trunk");
  Need(Vector2.Distance(new Vector2(trunk.position.x,trunk.position.z),new Vector2(center.x,center.z))<1,"Established World Tree has moved; inspect before authoring.");
  const string recovery="Assets/_DiceFree/Settings/Recovery/Cornberg-before-arborium-arrival-20261105.unity";
  Need(!File.Exists(recovery)&&EditorSceneManager.SaveScene(scene,recovery,true),"Recovery already exists or could not save.");
  Need(AssetDatabase.CopyAsset("Assets/_DiceFree/Settings/CornbergNavMesh.asset","Assets/_DiceFree/Settings/Recovery/CornbergNavMesh-before-arborium-20261105.asset"),"Navigation backup failed.");
  AssetDatabase.CreateFolder("Assets/_DiceFree/Art/World","ArboriumArrival");
  if(!AssetDatabase.IsValidFolder("Assets/_DiceFree/Settings/Arborium"))AssetDatabase.CreateFolder("Assets/_DiceFree/Settings","Arborium");
  root=new GameObject(RootName).transform;root.position=center;root.gameObject.AddComponent<DiceFree.Visuals.ArboriumCityPresentation>();
  Palette();GroundAndStreets();WallsAndGate();Buildings();Gardens();StreetFurniture();ServicesAndQuest(scene);OpenCityApproach();
  // Keep the existing macro-landmark in its established position and shape.
  foreach(var t in trunk.parent.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Ancient trunk"||t.name=="Vast crown"||t.name=="Ancient bough"))
   if(t.GetComponent<TreeCameraOccluder>()==null)t.gameObject.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(64);
  var treeSolid=new GameObject("World Tree trunk - solid sacred core");treeSolid.transform.SetParent(root,false);treeSolid.transform.localPosition=P(0,14,0);treeSolid.layer=9;var solid=treeSolid.AddComponent<CapsuleCollider>();solid.radius=9.5f;solid.height=30;
  foreach(var well in Object.FindObjectsByType<HealingArea>()){var so=new SerializedObject(well);so.FindProperty("hpPerSecond").floatValue=25;so.ApplyModifiedPropertiesWithoutUndo();}
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Need(EditorSceneManager.SaveScene(scene),"City scene save failed.");
  CornbergSceneBuilder.Rebake();
  return Report();
 }
 public static object Report()
 {
  var result=new{city="Arborium",firstSlice=true,worldTreePositionPreserved=true,treeCenter=center.ToString(),radius=78,northGate=(center+P(0,0,78)).ToString(),guildRepresentative=(center+P(-26,0,65)).ToString(),route="Trimery north → Briarback west → Arborium south",directRoadStillBroken=true,quest=QuestId,wellHpPerSecond=25,chieftainsBaneXp=10000};
  File.WriteAllText("Logs/ArboriumAuthoring.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
 public static object FinishAndInspect()
 {
  Need(!EditorApplication.isPlayingOrWillChangePlaymode,"Edit Mode required.");root=GameObject.Find(RootName).transform;
  if(root.GetComponent<DiceFree.Visuals.ArboriumCityPresentation>()==null)root.gameObject.AddComponent<DiceFree.Visuals.ArboriumCityPresentation>();
  var nav=NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
  try
  {
   var captain=GameObject.Find("Trimery Road Captain - Arborium journey");if(NavMesh.SamplePosition(new Vector3(256,0,205),out var hit,8,NavMesh.AllAreas))captain.transform.position=hit.position;
   var cityNpcs=root.GetComponentsInChildren<InteractionTarget>(true).Select(n=>new{n.name,position=n.transform.position.ToString(),approach=n.ApproachPosition.ToString(),nav=NavMesh.SamplePosition(n.ApproachPosition,out var p,2,NavMesh.AllAreas)?p.position.ToString():"none"}).ToArray();
   var points=new[]{new Vector3(250,0,210),new Vector3(248,0,250),new Vector3(230,0,367),center+P(137,0,61),center+P(114,0,73),center+P(92,0,86),center+P(0,0,86),center+P(0,0,72),center+P(-26,0,65),center+P(0,0,30),center+P(30,0,0)};
   var routes=points.Skip(1).Select((end,i)=>{var path=new NavMeshPath();bool a=NavMesh.SamplePosition(points[i],out var from,5,-1),b=NavMesh.SamplePosition(end,out var to,5,-1);return new{from=points[i].ToString(),to=end.ToString(),fromFound=a,toFound=b,complete=a&&b&&NavMesh.CalculatePath(from.position,to.position,-1,path)&&path.status==NavMeshPathStatus.PathComplete};}).ToArray();
   EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());Need(EditorSceneManager.SaveScene(SceneManager.GetActiveScene()),"Finish save failed.");Report();return new{cityNpcs,routes,captain=captain.transform.position.ToString(),renderers=root.GetComponentsInChildren<Renderer>().Length,meshes=root.GetComponentsInChildren<MeshFilter>().Length};
  }finally{if(nav.valid)nav.Remove();}
 }
 static int OpenCityApproach()
 {
  int opened=0;
  foreach(var t in Object.FindObjectsByType<Transform>().Where(t=>t.name=="Wooded cliff boundary"&&Mathf.Abs(t.position.x-191)<1&&t.position.z>382&&t.position.z<396))
  {t.gameObject.SetActive(false);opened++;}
  return opened;
 }
 public static object ConnectApproach()
 {
  Need(!EditorApplication.isPlayingOrWillChangePlaymode,"Edit Mode required.");root=GameObject.Find(RootName).transform;int opened=OpenCityApproach();
  var wall=root.Find("03 - Ring wall and monumental northern gate");
  foreach(int side in new[]{-1,1})if(!wall.GetComponentsInChildren<BoxCollider>().Any(c=>Vector3.Distance(c.transform.localPosition,P(side*7,3.5f,78))<.1f)){var go=new GameObject("Solid gate pier");go.transform.SetParent(wall,false);go.transform.localPosition=P(side*7,3.5f,78);go.layer=9;go.AddComponent<BoxCollider>().size=P(2,7,4);}
  foreach(var n in root.GetComponentsInChildren<InteractionTarget>(true))
  {
   var approach=n.transform.Find("City service approach");if(approach==null){approach=new GameObject("City service approach").transform;approach.SetParent(n.transform,false);}
   var offset=n.name=="Arborium market trader"?Vector3.back*1.8f:Vector3.forward*1.8f;approach.position=n.transform.position+offset;n.ConfigureApproach(approach);
  }
  EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());Need(EditorSceneManager.SaveScene(SceneManager.GetActiveScene()),"Connection save failed.");CornbergSceneBuilder.Rebake();return new{openedBoundaryRocks=opened,inspection=FinishAndInspect()};
 }
 public static object FinishGeometry()
 {
  Need(!EditorApplication.isPlayingOrWillChangePlaymode,"Edit Mode required.");root=GameObject.Find(RootName).transform;var mesh=root.Find("01 - Capital foundation and northern detour").GetComponentInChildren<MeshFilter>().sharedMesh;
  var vertices=mesh.vertices;var triangles=mesh.triangles;int fixedFaces=0;for(int i=0;i<triangles.Length;i+=3)if(Vector3.Cross(vertices[triangles[i+1]]-vertices[triangles[i]],vertices[triangles[i+2]]-vertices[triangles[i]]).y<0){int a=triangles[i];triangles[i]=triangles[i+2];triangles[i+2]=a;fixedFaces++;}mesh.triangles=triangles;mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssets();
  foreach(var child in root.Cast<Transform>().Where(t=>t.name.Contains("hall")||t.name.Contains("bank")||t.name.Contains("quarter")||t.name.Contains("residence")))if(child.GetComponent<TreeCameraOccluder>()==null)child.gameObject.AddComponent<TreeCameraOccluder>();
  return new{upwardFloorFacesFixed=fixedFaces,connections=ConnectApproach()};
 }
 public static object FinishPresentation()
 {
  Need(!EditorApplication.isPlayingOrWillChangePlaymode,"Edit Mode required.");root=GameObject.Find(RootName).transform;
  foreach(var fade in root.GetComponentsInChildren<TreeCameraOccluder>())fade.ConfigureFocusDistance(40);
  var trunk=Object.FindObjectsByType<Transform>().Single(t=>t.name=="Ancient trunk");foreach(var fade in trunk.parent.GetComponentsInChildren<TreeCameraOccluder>())fade.ConfigureFocusDistance(64);
  var anchor=root.GetComponentInChildren<ResurrectionAnchor>();anchor.gameObject.name="Arborium resurrection anchor";
  EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());Need(EditorSceneManager.SaveScene(SceneManager.GetActiveScene()),"Presentation save failed.");return new{cityOcclusionRange=40,worldTreeOcclusionRange=64,anchorName=anchor.name};
 }
 public static object PrepareFloorCollision()
 {
  Need(!EditorApplication.isPlayingOrWillChangePlaymode,"Edit Mode required.");var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/01 - Capital foundation and northern detour - 6.asset");Need(mesh!=null,"City floor missing.");mesh.SetPreBakeCollisionMesh(false,true);EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssetIfDirty(mesh);return new{preBakedTriangleCollision=mesh.HasPreBakeCollisionMesh(false)};
 }
 static void Palette()
 {
  materials.Clear();
  var colors=new[]{new Color(.62f,.58f,.46f),new Color(.83f,.80f,.66f),new Color(.10f,.16f,.19f),new Color(.10f,.32f,.34f),new Color(.71f,.49f,.16f),new Color(.23f,.77f,.78f),new Color(.29f,.39f,.24f),new Color(.54f,.53f,.45f),new Color(.28f,.18f,.10f),new Color(.09f,.24f,.42f),new Color(.23f,.44f,.28f)};
  var names=new[]{"weathered limestone","ivory carved stone","deep recesses","patinated copper shingles","old gilded bronze","practical enchanted glass","capital meadow","radial granite paving","oiled oak","guild blue cloth","sacred garden foliage"};
  for(int i=0;i<names.Length;i++)
  {
   var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Arborium "+names[i],color=colors[i]};m.SetFloat("_Smoothness",i==Glass?.65f:.18f);if(i==Gold||i==Roof)m.SetFloat("_Metallic",.45f);
   if(i==Glass){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",colors[i]*1.8f);}
   if(i==Stone||i==Roof||i==Paving)
   {
    const int size=256;var tex=new Texture2D(size,size,TextureFormat.RGB24,true){name=names[i],wrapMode=TextureWrapMode.Repeat};var pixels=new Color[size*size];
    for(int y=0;y<size;y++)for(int x=0;x<size;x++){int row=y/32,shift=(row%2)*32;bool seam=y%32<2||(x+shift)%64<2;float noise=Mathf.PerlinNoise(x*.19f,y*.19f)*.10f+Mathf.PerlinNoise(x*.035f,y*.035f)*.08f;float tint=seam?.60f:.87f+noise;pixels[y*size+x]=new Color(tint,tint,tint);}
    tex.SetPixels(pixels);tex.Apply();var path=Folder+"/"+names[i]+".png";File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
   }
   AssetDatabase.CreateAsset(m,Folder+"/"+names[i]+".mat");materials.Add(m);
  }
 }
 sealed class Surface
 {
  public readonly List<Vector3> v=new();public readonly List<Vector2> uv=new();public readonly List<int> indices=new();
  public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){int i=v.Count;v.AddRange(new[]{a,b,c,d});uv.AddRange(new[]{Vector2.zero,new Vector2(Vector3.Distance(a,b)/4,0),new Vector2(Vector3.Distance(a,b)/4,Vector3.Distance(b,c)/4),new Vector2(0,Vector3.Distance(b,c)/4)});indices.AddRange(new[]{i,i+1,i+2,i,i+2,i+3});}
  public void Tri(Vector3 a,Vector3 b,Vector3 c){int i=v.Count;v.AddRange(new[]{a,b,c});uv.AddRange(new[]{Vector2.zero,Vector2.right,Vector2.up});indices.AddRange(new[]{i,i+1,i+2});}
  public Mesh Mesh(string name){var m=new Mesh{name=name,indexFormat=IndexFormat.UInt32};m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(indices,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
 }
 sealed class Builder
 {
  readonly Surface[] parts=Enumerable.Range(0,11).Select(_=>new Surface()).ToArray();public readonly Transform group;
  public Builder(string name){group=new GameObject(name).transform;group.SetParent(root,false);}
  public void Box(Vector3 p,Vector3 s,int mat,Quaternion? rotation=null){var q=rotation??Quaternion.identity;Vector3 V(float x,float y,float z)=>p+q*Vector3.Scale(s,P(x,y,z))*.5f;var a=V(-1,-1,-1);var b=V(1,-1,-1);var c=V(1,1,-1);var d=V(-1,1,-1);var e=V(-1,-1,1);var f=V(1,-1,1);var g=V(1,1,1);var h=V(-1,1,1);parts[mat].Quad(a,d,c,b);parts[mat].Quad(f,g,h,e);parts[mat].Quad(e,h,d,a);parts[mat].Quad(b,c,g,f);parts[mat].Quad(d,h,g,c);parts[mat].Quad(e,a,b,f);}
  public void Tube(Vector3 p,float lower,float upper,float height,int mat,int sides=20){for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;Vector3 V(float r,float h,float angle)=>p+P(Mathf.Sin(angle)*r,h,Mathf.Cos(angle)*r);var x=V(lower,0,a);var y=V(lower,0,b);var z=V(upper,height,b);var w=V(upper,height,a);parts[mat].Quad(x,y,z,w);parts[mat].Tri(p+Vector3.up*height,w,z);parts[mat].Tri(p,y,x);}}
  public void Ring(Vector3 p,float inner,float outer,int mat,int segments=128){for(int i=0;i<segments;i++){float a=i*Mathf.PI*2/segments,b=(i+1)*Mathf.PI*2/segments;Vector3 V(float r,float t)=>p+P(Mathf.Sin(t)*r,0,Mathf.Cos(t)*r);parts[mat].Quad(V(inner,a),V(outer,a),V(outer,b),V(inner,b));}}
  public void Beam(Vector3 a,Vector3 b,float w,int mat)=>Box((a+b)*.5f,P(w,(a-b).magnitude,w),mat,Quaternion.FromToRotation(Vector3.up,b-a));
  public void RoadQuad(Vector3 a,Vector3 b,Vector3 c,Vector3 d)=>parts[Grass].Quad(d,c,b,a);
  public void Roof(Vector3 p,float w,float d,float h){var a=p+P(-w/2,0,-d/2);var b=p+P(w/2,0,-d/2);var c=p+P(w/2,0,d/2);var e=p+P(-w/2,0,d/2);var t=p+P(0,h,0);parts[ArboriumArrivalAuthoring.Roof].Tri(a,t,b);parts[ArboriumArrivalAuthoring.Roof].Tri(b,t,c);parts[ArboriumArrivalAuthoring.Roof].Tri(c,t,e);parts[ArboriumArrivalAuthoring.Roof].Tri(e,t,a);}
  public void Arch(Vector3 p,float inner,float outer,int mat){for(int i=0;i<18;i++){float a=i*Mathf.PI/18,b=(i+1)*Mathf.PI/18;Vector3 V(float r,float t,float z)=>p+P(Mathf.Cos(t)*r,Mathf.Sin(t)*r,z);var aa=V(inner,a,-.5f);var ab=V(inner,b,-.5f);var ba=V(outer,a,-.5f);var bb=V(outer,b,-.5f);var ac=V(inner,a,.5f);var ad=V(inner,b,.5f);var bc=V(outer,a,.5f);var bd=V(outer,b,.5f);parts[mat].Quad(aa,ab,bb,ba);parts[mat].Quad(ac,bc,bd,ad);parts[mat].Quad(ba,bb,bd,bc);parts[mat].Quad(aa,ac,ad,ab);}}
  public void SolidBox(Vector3 p,Vector3 size,Quaternion? rotation=null){var go=new GameObject("Solid footprint");go.layer=9;go.transform.SetParent(group,false);go.transform.localPosition=p;go.transform.localRotation=rotation??Quaternion.identity;go.AddComponent<BoxCollider>().size=size;}
  public void Save(bool walkable=false){for(int i=0;i<parts.Length;i++)if(parts[i].v.Count>0){string n=group.name+" - "+i;var mesh=parts[i].Mesh(n);if(walkable)mesh.SetPreBakeCollisionMesh(false,true);AssetDatabase.CreateAsset(mesh,Folder+"/"+n+".asset");var go=new GameObject(materials[i].name);go.transform.SetParent(group,false);go.layer=walkable?8:2;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=materials[i];if(walkable)go.AddComponent<MeshCollider>().sharedMesh=mesh;}if(group.name.Contains("hall")||group.name.Contains("bank")||group.name.Contains("quarter")||group.name.Contains("residence"))group.gameObject.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(40);}
 }
 static void GroundAndStreets()
 {
  var ground=new Builder("01 - Capital foundation and northern detour");ground.Ring(P(0,.02f,0),0,83,Grass);
  var route=new[]{P(137,0,61),P(114,0,73),P(92,0,86),P(0,0,86),P(0,0,72)};
  for(int i=1;i<route.Length;i++){var a=route[i-1];var b=route[i];var d=(b-a).normalized;var side=Vector3.Cross(Vector3.up,d)*6;ground.RoadQuad(a+side,b+side,b-side,a-side);ground.Ring(a+Vector3.up*.02f,0,6,Grass);}
  ground.Save(true);
  var street=new Builder("02 - Three concentric streets and radial avenues");
  foreach(float r in new[]{24f,44f,66f}){float half=r==66?4.5f:3;street.Ring(P(0,.055f,0),r-half,r+half,Paving);street.Ring(P(0,.065f,0),r-half,r-half+.18f,Gold);street.Ring(P(0,.065f,0),r+half-.18f,r+half,Gold);}
  street.Box(P(0,.04f,46),P(12,.03f,64),Paving);street.Box(P(0,.04f,-45),P(8,.03f,62),Paving);street.Box(P(46,.04f,0),P(62,.03f,8),Paving);street.Box(P(-46,.04f,0),P(62,.03f,8),Paving);
  for(int i=1;i<route.Length;i++){var a=route[i-1];var b=route[i];street.Box((a+b)*.5f+Vector3.up*.045f,P(8,.03f,(b-a).magnitude+2),Paving,Quaternion.LookRotation(b-a));}
  street.Save();
  var road=AssetDatabase.FindAssets("t:TravelSurfaceDefinition").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<TravelSurfaceDefinition>).First(d=>d!=null&&Mathf.Approximately(d.speedBonusPercent,15));
  foreach(var f in street.group.GetComponentsInChildren<MeshFilter>())if(f.GetComponent<Renderer>().sharedMaterial==materials[Paving])f.gameObject.AddComponent<TravelSurface>().Configure("surface-area.arborium.streets",road,f,new Bounds(Vector3.zero,Vector3.one*10000));
 }
 static void Tower(Builder b,Vector3 p,float radius=4.5f,float height=22)
 {
  b.Tube(p,radius+1,radius+1,1,Dark);b.Tube(p+Vector3.up,radius,radius,height,Stone);b.Tube(p+Vector3.up*(height-1),radius+.5f,radius+.5f,1.1f,LightStone);b.Tube(p+Vector3.up*(height+1),radius+1,0,9,Roof);b.Tube(p+Vector3.up*(height+10),.25f,0,3,Gold);
  for(int j=0;j<8;j++){float a=j*Mathf.PI/4;var q=Quaternion.Euler(0,j*45,0);b.Box(p+P(Mathf.Sin(a)*(radius+.05f),height-4,Mathf.Cos(a)*(radius+.05f)),P(.7f,2.5f,.18f),Glass,q);}
  b.SolidBox(p+Vector3.up*height*.5f,P(radius*1.65f,height,radius*1.65f));
 }
 static void WallsAndGate()
 {
  var b=new Builder("03 - Ring wall and monumental northern gate");
  for(int i=2;i<70;i++){float a=i*5*Mathf.Deg2Rad;var p=P(Mathf.Sin(a)*78,0,Mathf.Cos(a)*78);var q=Quaternion.Euler(0,i*5,0);b.Box(p+Vector3.up*5,P(6.9f,10,2.6f),Stone,q);b.Box(p+Vector3.up*9.7f,P(7,1,3.1f),LightStone,q);b.Box(p+Vector3.up*.7f,P(7.1f,1.4f,3.8f),Dark,q);b.SolidBox(p+Vector3.up*5,P(7,10,2.6f),q);for(int k=-1;k<=1;k++)b.Box(p+q*P(k*2.1f,11,0),P(1.15f,2,3),LightStone,q);if(i%9==0)Tower(b,p,3.6f,15);}
  Tower(b,P(-12,0,78));Tower(b,P(12,0,78));b.Arch(P(0,7,78),6,8,LightStone);b.Box(P(-7,3.5f,78),P(2,7,4),Stone);b.Box(P(7,3.5f,78),P(2,7,4),Stone);b.SolidBox(P(-7,3.5f,78),P(2,7,4));b.SolidBox(P(7,3.5f,78),P(2,7,4));b.Box(P(0,16,78),P(17,2,4),Stone);b.Box(P(0,17.1f,78),P(18,.5f,4.5f),Gold);Banner(b,P(-12,16,83),3,5);Banner(b,P(12,16,83),3,5);b.Save();
  var gate=new GameObject("Arborium north gate arrival");gate.transform.SetParent(root,false);gate.transform.localPosition=P(0,.1f,74);gate.AddComponent<ReachArea>().Configure(GateId,7);
  Label("ARBORIUM",P(0,18.6f,81),2.2f);Label("Northern Gate",P(0,3,86),.65f);
 }
 static void Banner(Builder b,Vector3 p,float width,float length)
 {
  b.Box(p+Vector3.up*.25f,P(width+.8f,.16f,.2f),Gold);b.Box(p-Vector3.up*length*.5f,P(width,length,.1f),Cloth);b.Box(p-P(0,length*.58f,-.07f),P(.16f,length*.4f,.12f),Gold);b.Tube(p-P(0,length*.43f,-.15f),width*.28f,0,.6f,Gold,8);
 }
 static void Building(string name,Vector3 p,float w,float d,float height,bool guild=false)
 {
  var b=new Builder(name);b.Box(p+Vector3.up*.35f,P(w+1,.7f,d+1),Dark);b.Box(p+Vector3.up*height*.5f,P(w,height,d),Stone);b.SolidBox(p+Vector3.up*height*.5f,P(w,height,d));
  foreach(float h in new[]{1f,height*.48f,height-.6f})b.Box(p+Vector3.up*h,P(w+.65f,.55f,d+.65f),LightStone);
  for(int side=-1;side<=1;side+=2)for(int i=-2;i<=2;i++)
  {
   var front=p+P(i*w/6,height*.67f,side*(d*.5f+.08f));b.Box(front,P(1.5f,3.8f,.2f),Dark);b.Box(front+P(0,0,side*.12f),P(1,3.2f,.12f),Glass);b.Box(front+P(0,-.2f,side*.20f),P(.12f,3.3f,.10f),Gold);b.Box(front+P(0,0,side*.20f),P(1.1f,.12f,.1f),Gold);
  }
  for(int side=-1;side<=1;side+=2)for(int i=-1;i<=1;i++){b.Box(p+P(side*(w/2+.35f),height*.45f,i*d/3),P(.9f,height*.9f,1.1f),LightStone);}
  b.Roof(p+Vector3.up*height,w+2,d+2,guild?9:6);var door=p+P(0,0,d*.5f+.2f);b.Box(door+Vector3.up*2.4f,P(4.5f,4.8f,.35f),Wood);b.Arch(door+Vector3.up*3.5f,2.25f,2.9f,LightStone);b.Box(door+P(0,2,.25f),P(.1f,4,.1f),Gold);for(int sign=-1;sign<=1;sign+=2)b.Box(door+P(sign*1.6f,2,.25f),P(.12f,3.2f,.12f),Gold);
  if(guild){for(int side=-1;side<=1;side+=2){Tower(b,p+P(side*(w/2-1),0,d/2-1),3,24);Banner(b,p+P(side*6,height*.7f,d/2+.4f),2.5f,5);}b.Box(door+P(0,8,1.5f),P(8,.6f,4),LightStone);}
  b.Save();
 }
 static void Buildings()
 {
  Building("04 - Adventurers Guild fortified hall",P(-26,0,51),23,16,17,true);
  Building("05 - Northern exchange and bank",P(28,0,51),23,14,13);
  Building("06 - Artisan quarter first workshop",P(53,0,-9),11,12,10);
  Building("07 - Mage quarter first observatory",P(-53,0,-9),11,12,14);
  Building("08 - Inner noble residence west",P(-28,0,-18),8,10,11);
  Building("09 - Inner noble residence east",P(28,0,-18),8,10,11);
  var magic=new Builder("10 - Mage quarter curious orrery");var p=P(-51,0,15);magic.Tube(p,3.2f,3.2f,.6f,Dark);magic.Tube(p+Vector3.up*.6f,2.8f,2.8f,.3f,Gold);magic.Ring(p+Vector3.up*4,1.8f,2,Gold,32);magic.Tube(p+Vector3.up*3.2f,.7f,0,2.2f,Glass,8);for(int i=0;i<4;i++){float a=i*Mathf.PI/2;magic.Tube(p+P(Mathf.Sin(a)*2.2f,4.2f,Mathf.Cos(a)*2.2f),.28f,0,.7f,Glass,8);}magic.Save();
  var market=new Builder("11 - Outer merchant arcades and stalls");for(int i=0;i<4;i++){var p2=P(55+i%2*8,0,20+i/2*9);market.Box(p2+Vector3.up*.65f,P(4.5f,1.3f,2),Wood);market.Roof(p2+Vector3.up*3.6f,5.5f,3.5f,1.2f);for(int side=-1;side<=1;side+=2)market.Box(p2+P(side*2.3f,1.8f,0),P(.14f,3.6f,.14f),Wood);for(int n=0;n<4;n++)market.Tube(p2+P(n*.7f-1,1.3f,0),.25f,.22f,.4f,n%2==0?Gold:Leaf,8);market.SolidBox(p2+Vector3.up*.65f,P(4.5f,1.3f,2));}market.Save();
  Label("Adventurers' Guild",P(-26,4.5f,64),.8f);Label("Northern Exchange",P(28,4,62),.7f);
 }
 static void Gardens()
 {
  var b=new Builder("12 - Sacred civic gardens and World Tree roots");
  b.Ring(P(0,.09f,0),10.5f,17.5f,Grass);b.Ring(P(0,.12f,0),17.5f,18,LightStone);
  for(int i=0;i<8;i++){float angle=(i*45+22.5f)*Mathf.Deg2Rad;var dir=P(Mathf.Sin(angle),0,Mathf.Cos(angle));var q=Quaternion.Euler(0,i*45+22.5f,0);b.Box(dir*17+Vector3.up*.48f,P(3,.8f,1.4f),LightStone,q);b.Box(dir*17+Vector3.up*.95f,P(2.7f,.5f,1.1f),Leaf,q);b.Beam(dir*8+Vector3.up*2,dir*14+Vector3.up*.25f,1.5f,Wood);for(int k=-1;k<=1;k++)b.Tube(dir*17+q*P(k*.7f,1.1f,0),.18f,0,.5f,k%2==0?Glass:Gold,7);}
  // A public civic walk around the tree; no invented order name, palace or entry gate.
  for(int i=0;i<6;i++){float a=(i*60+30)*Mathf.Deg2Rad;var p=P(Mathf.Sin(a)*36,0,Mathf.Cos(a)*36);b.Ring(p+Vector3.up*.12f,0,3.3f,LightStone,32);b.Ring(p+Vector3.up*.14f,0,2.6f,Grass,32);b.Tube(p+Vector3.up*.15f,1.2f,.4f,3.8f,Leaf,10);}
  b.Save();
 }
 static void StreetFurniture()
 {
  var b=new Builder("13 - Practical magical lamps and city furniture");
  foreach(float r in new[]{24f,44f,66f})for(int i=0;i<12;i++){float a=(i*30+12)*Mathf.Deg2Rad;var p=P(Mathf.Sin(a)*(r+4),0,Mathf.Cos(a)*(r+4));b.Tube(p,.45f,.35f,.4f,LightStone,8);b.Tube(p+Vector3.up*.4f,.12f,.09f,4.6f,Gold,8);b.Tube(p+Vector3.up*4.7f,.45f,.23f,.8f,Glass,6);b.Tube(p+Vector3.up*5.5f,.6f,0,.6f,Roof,6);}
  for(int side=-1;side<=1;side+=2)for(int i=0;i<3;i++){var p=P(side*9,0,33+i*10);b.Box(p+Vector3.up*.5f,P(1.2f,.25f,3.1f),Wood);b.Box(p+P(side*.5f,1,0),P(.18f,.8f,3.1f),Wood);}
  b.Save();
 }
 static void Label(string name,Vector3 p,float size){var go=new GameObject("Sign - "+name);go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localRotation=Quaternion.Euler(0,180,0);var text=go.AddComponent<TextMesh>();text.text=name;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.fontSize=64;text.characterSize=size*.08f;text.color=new Color(.95f,.87f,.60f);}
 static void ServicesAndQuest(Scene scene)
 {
  var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
  GameObject Clone(string source,string name,Vector3 local,bool clear)
  {
   var template=all.First(t=>t.name==source);var go=Object.Instantiate(template.gameObject,root);go.name=name;go.transform.position=center+local;go.transform.rotation=Quaternion.identity;
   if(PrefabUtility.IsPartOfPrefabInstance(go))PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
   if(clear){foreach(var interaction in go.GetComponentsInChildren<InteractionTarget>(true))Object.DestroyImmediate(interaction);foreach(var old in go.GetComponentsInChildren<QuestWorldMarker>(true))Object.DestroyImmediate(old);}
   else foreach(var interaction in go.GetComponentsInChildren<InteractionTarget>(true)){interaction.ConfigureName(name);interaction.ConfigureContext(null);var approach=new GameObject("City service approach").transform;approach.SetParent(go.transform,false);approach.position=go.transform.position+(name=="Arborium market trader"?Vector3.back:Vector3.forward)*1.8f;interaction.ConfigureApproach(approach);}
   return go;
  }
  Clone("NPCs/NPC_Merchant - 3D","Arborium market trader",P(54,0,27),false);Clone("NPCs/NPC_Banker - 3D","Arborium exchange banker",P(28,0,62),false);
  var rep=Clone("NPCs/NPC_Guard - 3D","Adventurers Guild Registrar",P(-26,0,65),true);var talk=rep.AddComponent<ConversationTarget>();talk.ConfigureName("Guild Registrar");talk.Configure(RegistrarId,"conversation.arborium.arrival","Welcome to Arborium. The northern trail brought you here safely; your report from Trimery is recorded. The Adventurers' Guild stands beside the northern gate, and the Exchange is across the avenue. Follow the ring roads toward the World Tree. The direct timber highway remains broken.");
  var quest=ScriptableObject.CreateInstance<QuestDefinition>();quest.name="Road to Arborium";quest.stableId=QuestId;quest.title="Road to Arborium";quest.offer="Trimery's direct timber road is broken. Travel north through Briarback, follow the western ridge trail, then turn south into Arborium's Northern Gate. Deliver our road report to the Guild Registrar outside the Adventurers' Guild. The Chieftain is an optional hunt, not a requirement for this journey.";quest.activeDialogue="Go north through Briarback, west along the ridge and south into Arborium. Speak to the Guild Registrar by the fortified guildhall beside the Northern Gate.";quest.locationHint="Trimery north → Briarback west → Arborium Northern Gate → Guild Registrar.";quest.rewardXp=1500;quest.completeOnObjectives=true;quest.stages=new[]{new QuestObjective{kind=ObjectiveKind.ReachArea,areaId=GateId,count=1,instruction="Enter Arborium through the Northern Gate"},new QuestObjective{kind=ObjectiveKind.TalkTo,contentId=RegistrarId,conversationId="conversation.arborium.arrival",count=1,instruction="Speak to the Guild Registrar outside the Adventurers' Guild"}};AssetDatabase.CreateAsset(quest,QuestPath);
  var captain=Clone("NPCs/NPC_Guard - 3D","Trimery Road Captain - Arborium journey",P(157.96f,0,-99.74f),true);captain.transform.position=new Vector3(256,0,205);var giver=captain.AddComponent<QuestGiver>();giver.Configure(quest);giver.ConfigureName("Trimery Road Captain");
  foreach(var journal in Object.FindObjectsByType<QuestJournal>(FindObjectsInactive.Include)){journal.Configure(journal.Definitions.Concat(new[]{quest}).ToArray());EditorUtility.SetDirty(journal);}
  var dieTemplate=all.Single(t=>t.name=="Trimery Dice revive stone");var die=Object.Instantiate(dieTemplate.gameObject,root);die.name="Arborium Dice revive stone";var oldBody=dieTemplate.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Trimery D6 revive beacon");die.transform.position=dieTemplate.position+center+P(-9,0,66)-new Vector3(oldBody.position.x,0,oldBody.position.z);if(PrefabUtility.IsPartOfPrefabInstance(die))PrefabUtility.UnpackPrefabInstance(die,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
  foreach(var area in die.GetComponentsInChildren<ReachArea>(true))Object.DestroyImmediate(area);
  var anchor=die.GetComponentInChildren<ResurrectionAnchor>();anchor.gameObject.name="Arborium resurrection anchor";anchor.Configure("anchor.arborium",die.GetComponentInChildren<ResurrectionDiceVisual>()?.WorldPip?.transform??die.transform,4.5f);
  foreach(var text in die.GetComponentsInChildren<TextMesh>(true))text.text="Arborium";
 }
}
