using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Core;
using DiceFree.World;
using DiceFree.EditorTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class ArboriumFloorsAuthoring
{
 public const string Folder="Assets/_DiceFree/Art/World/ArboriumFloors";
 public const string RootName="Arborium - gates and inhabited floors";
 static readonly Vector3 Center=new(98.04f,0,304.74f);
 static Transform root;
 static Material stone,ivory,wood,roof,glass,gold,dark,paving,cloth;
 static int serial;
 static Vector3 P(float x,float y,float z)=>new(x,y,z);
 static void Need(bool b,string why){if(!b)throw new InvalidOperationException(why);}
 public static object Author()
 {
  var scene=SceneManager.GetActiveScene();Need(scene.name=="Cornberg"&&!scene.isDirty&&!EditorApplication.isPlayingOrWillChangePlaymode,"Clean Cornberg Edit Mode required");
  Need(GameObject.Find(RootName)==null&&!AssetDatabase.IsValidFolder(Folder),"Already authored; preserve existing assets");
  Need(!File.Exists("Assets/_DiceFree/Settings/Recovery/Cornberg-before-arborium-floors-20261105.unity")&&!File.Exists("Assets/_DiceFree/Settings/Recovery/CornbergNavMesh-before-arborium-floors-20261105.asset"),"Recovery already exists; preserve it");
  Need(EditorSceneManager.SaveScene(scene,"Assets/_DiceFree/Settings/Recovery/Cornberg-before-arborium-floors-20261105.unity",true),"Scene recovery failed");
  Need(AssetDatabase.CopyAsset("Assets/_DiceFree/Settings/CornbergNavMesh.asset","Assets/_DiceFree/Settings/Recovery/CornbergNavMesh-before-arborium-floors-20261105.asset"),"Navigation recovery failed");
  AssetDatabase.CreateFolder("Assets/_DiceFree/Art/World","ArboriumFloors");serial=0;
  Material M(string name)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Art/World/ArboriumArrival/"+name+".mat");
  stone=M("weathered limestone");ivory=M("ivory carved stone");wood=M("oiled oak");roof=M("patinated copper shingles");glass=M("practical enchanted glass");gold=M("old gilded bronze");dark=M("deep recesses");paving=M("radial granite paving");cloth=M("guild blue cloth");
  Need(new[]{stone,ivory,wood,roof,glass,gold,dark,paving,cloth}.All(m=>m!=null),"Existing city palette missing");
  root=new GameObject(RootName).transform;root.position=Center;
  var ridge=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Single(t=>t.name=="Distant wooded ridge");ridge.gameObject.SetActive(false);
  Gates();OpenEastApproach();
  Building("04 - Adventurers Guild fortified hall","Adventurers Guild",P(-26,0,51),23,16,4,4.2f,true);
  Building("05 - Northern exchange and bank","Northern Exchange",P(28,0,51),23,14,4,3.3f,false);
  Building("06 - Artisan quarter first workshop","Artisan Workshop",P(53,0,-9),11,12,3,3.4f,false);
  Building("07 - Mage quarter first observatory","Mage Observatory",P(-53,0,-9),11,12,4,3.5f,false);
  Building("08 - Inner noble residence west","West Residence",P(-28,0,-18),8,10,3,3.6f,false);
  Building("09 - Inner noble residence east","East Residence",P(28,0,-18),8,10,3,3.6f,false);
  FinishMaterials();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Need(EditorSceneManager.SaveScene(scene),"Scene save failed");CornbergSceneBuilder.Rebake();
  var result=new{ridgeRemoved=true,eastWestGates=true,buildings=6,walkableLevels=21,stairs="continuous dogleg stairs",meshAssets=serial};
  File.WriteAllText("Logs/ArboriumFloorsAuthoring.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
 static int OpenEastApproach()
 {
  int cleared=0;
  foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Where(t=>t.name=="Wooded cliff boundary"&&Mathf.Abs(t.position.x-191)<.1f&&t.position.z>297&&t.position.z<312))
  {t.gameObject.SetActive(false);cleared++;}
  return cleared;
 }
 public static object FinishEastApproach()
 {
  Need(!EditorApplication.isPlayingOrWillChangePlaymode&&!SceneManager.GetActiveScene().isDirty,"Clean Edit Mode required");int cleared=OpenEastApproach();EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());Need(EditorSceneManager.SaveScene(SceneManager.GetActiveScene()),"Approach save failed");CornbergSceneBuilder.Rebake();return new{clearedBoundaryRocks=cleared};
 }
 static void FinishMaterials()
 {
  // Original plank texture; the established exterior palette/material assets remain untouched.
  string path=Folder+"/Interior oak.mat";var oak=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(oak==null)
  {
   const int size=512;var texture=new Texture2D(size,size,TextureFormat.RGB24,true);var pixels=new Color[size*size];
   for(int y=0;y<size;y++)for(int x=0;x<size;x++)
   {int plank=y/64,shift=plank%2*128;bool seam=y%64<2||(x+shift)%256<2;float grain=Mathf.PerlinNoise(x*.035f,y*.7f)*.13f+Mathf.PerlinNoise(x*.008f,y*.13f)*.1f;float tone=seam?.43f:.78f+grain+(plank%3)*.035f;pixels[y*size+x]=new Color(tone,tone*.94f,tone*.83f);}
   texture.SetPixels(pixels);texture.Apply();string png=Folder+"/Interior oak planks.png";File.WriteAllBytes(png,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(png,ImportAssetOptions.ForceSynchronousImport);
   oak=new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Art/World/ArboriumArrival/oiled oak.mat")){name="Arborium interior oak",color=new Color(.49f,.35f,.22f)};oak.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(png);AssetDatabase.CreateAsset(oak,path);
  }
  var owned=GameObject.Find(RootName);
  var originalOak=AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Art/World/ArboriumArrival/oiled oak.mat");
  foreach(var renderer in owned.GetComponentsInChildren<MeshRenderer>())if(renderer.sharedMaterial==originalOak||renderer.sharedMaterial==oak)
  {
   renderer.sharedMaterial=oak;var mesh=renderer.GetComponent<MeshFilter>().sharedMesh;var v=mesh.vertices;var n=mesh.normals;var uv=new Vector2[v.Length];
   for(int i=0;i<v.Length;i++)uv[i]=Mathf.Abs(n[i].y)>.7f?new Vector2(v[i].x,v[i].z)*.5f:Mathf.Abs(n[i].x)>.7f?new Vector2(v[i].z,v[i].y)*.5f:new Vector2(v[i].x,v[i].y)*.5f;
   mesh.uv=uv;EditorUtility.SetDirty(mesh);
  }
  // Preserve the old buildings' outdoor hero visibility without grouping the whole city.
  foreach(var building in owned.GetComponentsInChildren<ArboriumInterior>())
   foreach(Transform part in building.transform)
    if(part.name.StartsWith("Storey ")||part.name.StartsWith("Floor ")||part.name=="Copper roof and carved eaves")
     foreach(var renderer in part.GetComponentsInChildren<MeshRenderer>())
     {
      var fade=renderer.GetComponent<TreeCameraOccluder>();if(fade==null){fade=renderer.gameObject.AddComponent<TreeCameraOccluder>();fade.ConfigureFocusDistance(40);}
      if(part.name.StartsWith("Floor "))fade.ConfigureExteriorOnly(building.transform,building.Footprint);
     }
 }
 public static object FinishPresentation()
 {
  Need(!EditorApplication.isPlayingOrWillChangePlaymode&&!SceneManager.GetActiveScene().isDirty,"Clean Edit Mode required");
  var source=GameObject.Find("Arborium - World Tree capital").transform.Find("03 - Ring wall and monumental northern gate");int index=0;
  foreach(var filter in source.GetComponentsInChildren<MeshFilter>())
  {
   var parts=GateParts(filter);
   for(int j=0;j<parts.Length;j++)if(parts[j].v.Count>0)
   {string name="Gate "+(index++).ToString("D3")+" section "+j;var saved=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/"+name+".asset");Need(saved!=null,"Gate mesh missing "+name);var revised=parts[j].Bake(name);EditorUtility.CopySerialized(revised,saved);Object.DestroyImmediate(revised);EditorUtility.SetDirty(saved);}
  }
  FinishMaterials();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());Need(EditorSceneManager.SaveScene(SceneManager.GetActiveScene()),"Presentation save failed");return new{originalGateUvsPreserved=true,oakPlanks=true};
 }
 sealed class Surface
 {
  public readonly List<Vector3> v=new();public readonly List<Vector2> uv=new();public readonly List<int> t=new();
  public void Tri(Vector3 a,Vector3 b,Vector3 c)=>Tri(a,b,c,Vector2.zero,Vector2.right,Vector2.up);
  public void Tri(Vector3 a,Vector3 b,Vector3 c,Vector2 ua,Vector2 ub,Vector2 uc){int i=v.Count;v.AddRange(new[]{a,b,c});uv.AddRange(new[]{ua,ub,uc});t.AddRange(new[]{i,i+1,i+2});}
  public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){Tri(a,b,c);Tri(a,c,d);}
  public Mesh Bake(string n){var m=new Mesh{name=n,indexFormat=IndexFormat.UInt32};m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
 }
 sealed class Part
 {
  public readonly Transform group;readonly Dictionary<Material,Surface> s=new();
  public Part(Transform parent,string name){group=new GameObject(name).transform;group.SetParent(parent,false);}
  Surface S(Material m){if(!s.TryGetValue(m,out var x)){x=new Surface();s.Add(m,x);}return x;}
  public void Box(Vector3 p,Vector3 size,Material mat,bool solid=false,int layer=9,Quaternion? rotation=null)
  {
   var q=rotation??Quaternion.identity;Vector3 V(float x,float y,float z)=>p+q*Vector3.Scale(size,P(x,y,z))*.5f;
   var a=V(-1,-1,-1);var b=V(1,-1,-1);var c=V(1,1,-1);var d=V(-1,1,-1);var e=V(-1,-1,1);var f=V(1,-1,1);var g=V(1,1,1);var h=V(-1,1,1);var s=S(mat);
   s.Quad(a,d,c,b);s.Quad(f,g,h,e);s.Quad(e,h,d,a);s.Quad(b,c,g,f);s.Quad(d,h,g,c);s.Quad(e,a,b,f);
   if(solid){var go=new GameObject(layer==8?"Walkable surface":"Solid wall");go.layer=layer;go.transform.SetParent(group,false);go.transform.localPosition=p;go.transform.localRotation=q;go.AddComponent<BoxCollider>().size=size;}
  }
  public void Roof(Vector3 p,float w,float d,float h){var a=p+P(-w/2,0,-d/2);var b=p+P(w/2,0,-d/2);var c=p+P(w/2,0,d/2);var e=p+P(-w/2,0,d/2);var top=p+Vector3.up*h;var s=S(roof);s.Tri(a,top,b);s.Tri(b,top,c);s.Tri(c,top,e);s.Tri(e,top,a);}
  public void Save(){foreach(var kv in s){string n=(serial++).ToString("D3")+" "+group.name;var mesh=kv.Value.Bake(n);AssetDatabase.CreateAsset(mesh,Folder+"/"+n+".asset");var go=new GameObject(kv.Key.name);go.layer=2;go.transform.SetParent(group,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=kv.Key;}}
 }
 static Surface[] GateParts(MeshFilter filter)
 {
  var mesh=filter.sharedMesh;var v=mesh.vertices;var uv=mesh.uv;var t=mesh.triangles;var parts=Enumerable.Range(0,10).Select(_=>new Surface()).ToArray();
  for(int i=0;i<t.Length;i+=3)
  {
   var a=v[t[i]];var b=v[t[i+1]];var c=v[t[i+2]];var p=(a+b+c)/3;var ua=uv[t[i]];var ub=uv[t[i+1]];var uc=uv[t[i+2]];
   bool north=p.z>70&&Mathf.Abs(p.x)<19;bool side=Mathf.Abs(p.x)>70&&Mathf.Abs(p.z)<19;int section=north?(p.x<-5?1:p.x>5?2:3):0;
   if(!side)parts[section].Tri(a,b,c,ua,ub,uc);
   if(north)foreach(int turn in new[]{1,-1}){var q=Quaternion.Euler(0,turn*90,0);parts[(turn==1?3:6)+section].Tri(q*a,q*b,q*c,ua,ub,uc);}
  }
  return parts;
 }
 static void Gates()
 {
  GameObject.Find("Northern gate - camera-safe sections").SetActive(false);
  var source=GameObject.Find("Arborium - World Tree capital").transform.Find("03 - Ring wall and monumental northern gate");
  var walls=new GameObject("North east and west gates - camera-safe sections").transform;walls.SetParent(root,false);
  foreach(var filter in source.GetComponentsInChildren<MeshFilter>())
  {
   var parts=GateParts(filter);
   for(int j=0;j<parts.Length;j++)if(parts[j].v.Count>0)
   {
    string name="Gate "+(serial++).ToString("D3")+" section "+j;var saved=parts[j].Bake(name);AssetDatabase.CreateAsset(saved,Folder+"/"+name+".asset");var go=new GameObject(name);go.transform.SetParent(walls,false);go.layer=2;go.AddComponent<MeshFilter>().sharedMesh=saved;go.AddComponent<MeshRenderer>().sharedMaterial=filter.GetComponent<Renderer>().sharedMaterial;if(j>0)go.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(40);
   }
  }
  var colliders=source.GetComponentsInChildren<BoxCollider>();
  foreach(var c in colliders)
  {
   var p=c.transform.localPosition;
   if(Mathf.Abs(p.x)>70&&Mathf.Abs(p.z)<19)c.enabled=false;
   if(p.z>70&&Mathf.Abs(p.x)<19)foreach(int turn in new[]{1,-1})
   {var q=Quaternion.Euler(0,turn*90,0);var go=new GameObject(turn==1?"East gate pier or tower":"West gate pier or tower");go.layer=9;go.transform.SetParent(walls,false);go.transform.localPosition=q*p;go.transform.localRotation=q*c.transform.localRotation;var copy=go.AddComponent<BoxCollider>();copy.size=c.size;copy.center=c.center;}
  }
  foreach(int side in new[]{-1,1})
  {
   var apron=new Part(root,side<0?"West gate approach":"East gate approach");apron.Box(P(side*83,.015f,0),P(26,.09f,18),paving,true,8);
   foreach(int z in new[]{-1,1}){apron.Box(P(side*85,.11f,z*7.9f),P(19,.08f,.3f),ivory);apron.Box(P(side*92,1.3f,z*8),P(.8f,2.6f,.8f),stone,true);apron.Box(P(side*92,2.85f,z*8),P(1.1f,.5f,1.1f),gold);}
   apron.Save();
  }
 }
 static void Building(string originalName,string title,Vector3 position,float w,float d,int count,float h,bool guild)
 {
  var original=GameObject.Find("Arborium - World Tree capital").transform.Find(originalName);
  Need(original!=null,"Original building missing "+originalName);foreach(var r in original.GetComponentsInChildren<Renderer>())r.enabled=false;foreach(var c in original.GetComponentsInChildren<Collider>())c.enabled=false;
  var building=new GameObject(title+" - walkable building").transform;building.SetParent(root,false);building.localPosition=position;
  var surfaces=new List<ArboriumInterior.Part>();
  Part Make(string n,ArboriumInterior.PartKind kind,int level,Vector3 normal=default){var p=new Part(building,n);surfaces.Add(new ArboriumInterior.Part{root=p.group.gameObject,kind=kind,floor=level,outward=normal});return p;}
  // Identical generous stair widths in every building; larger halls retain broad side rooms.
  float halfWell=2.04f,flightWidth=1.88f,bottom=-d/2+2.2f;int steps=Mathf.CeilToInt(h/.4f);float run=steps*.29f,end=bottom+run,landing=1.5f,holeEnd=end+landing;
  for(int f=0;f<count;f++)
  {
   float y=.12f+f*h;
   var floor=Make("Floor "+(f+1)+" - oak rooms and stairwell",ArboriumInterior.PartKind.Floor,f);
   void Slab(float x0,float x1,float z0,float z1){if(x1-x0>.01f&&z1-z0>.01f)floor.Box(P((x0+x1)/2,y-.10f,(z0+z1)/2),P(x1-x0,.2f,z1-z0),f==0?paving:wood,true,8);}
   if(f==0)Slab(-w/2+.35f,w/2-.35f,-d/2+.35f,d/2-.35f);
   else {Slab(-w/2+.35f,-halfWell,-d/2+.35f,d/2-.35f);Slab(halfWell,w/2-.35f,-d/2+.35f,d/2-.35f);Slab(-halfWell,halfWell,-d/2+.35f,bottom);Slab(-halfWell,halfWell,holeEnd,d/2-.35f);}
   // Border, floor runner and wall-side furniture leave the stair and door approaches free.
   floor.Box(P(0,y+.015f,(holeEnd+d/2-.5f)/2),P(Mathf.Min(w-2,5),.025f,Mathf.Max(.4f,(d/2-.5f-holeEnd)*.65f)),cloth);
   if(w>10){float x=-w/2+1.15f;floor.Box(P(x,y+.6f,1.1f),P(.75f,1.2f,3.4f),wood,true);floor.Box(P(x,y+1.28f,1.1f),P(.85f,.12f,3.5f),gold);for(int k=0;k<8;k++)floor.Box(P(x,y+1.5f,-.3f+k*.4f),P(.45f,.35f,.22f),k%2==0?cloth:ivory);}
   // Stairwell guards run only along the hole's long edges, clear of the rear exits.
   if(f>0)foreach(int side in new[]{-1,1}){floor.Box(P(side*(halfWell+.04f),y+1.02f,(bottom+holeEnd)/2),P(.09f,.11f,holeEnd-bottom),wood);for(float z=bottom+.2f;z<holeEnd;z+=.85f)floor.Box(P(side*(halfWell+.04f),y+.51f,z),P(.07f,1.02f,.07f),gold);}
   floor.Save();
   foreach(var face in new[]{Vector3.forward,Vector3.back,Vector3.left,Vector3.right})
   {
    var wall=Make("Storey "+(f+1)+" - "+face+" wall",ArboriumInterior.PartKind.Wall,f,face);
    bool front=face.z>0,across=face.z!=0;float length=across?w:d;
    Vector3 V(float u,float yy)=>across?P(u,yy,face.z*d/2):P(face.x*w/2,yy,u);
    Vector3 Size(float span,float height,float thickness)=>across?P(span,height,thickness):P(thickness,height,span);
    if(front&&f==0){float doorway=Mathf.Min(4.5f,w-2);float span=(w-doorway)/2;foreach(int side in new[]{-1,1})wall.Box(V(side*(doorway/2+span/2),y+h/2),Size(span,h,.46f),stone,true);wall.Box(V(0,y+h-.25f),Size(doorway,.5f,.46f),ivory,true);}
    else wall.Box(V(0,y+h/2),Size(length,h,.46f),stone,true);
    wall.Box(V(0,y+.2f),Size(length+.15f,.28f,.58f),dark);
    // Open front door has no sill collider or decorative slab across the entrance.
    if(front&&f==0){foreach(int side in new[]{-1,1})wall.Box(V(side*(Mathf.Min(4.5f,w-2)/2+.13f),y+(h-.5f)/2),Size(.26f,h-.5f,.7f),ivory);}
    int windows=Mathf.Max(1,Mathf.FloorToInt(length/4.5f));
    for(int j=0;j<windows;j++){float u=(j-(windows-1)*.5f)*3.6f;if(front&&f==0&&Mathf.Abs(u)<3)continue;var p=V(u,y+h*.58f);p+=face*.27f;wall.Box(p,Size(1.05f,1.45f,.06f),glass);wall.Box(p+Vector3.up*.8f,Size(1.35f,.16f,.18f),ivory);wall.Box(p-Vector3.up*.8f,Size(1.35f,.16f,.18f),ivory);wall.Box(p,Size(.075f,1.45f,.1f),gold);}
    wall.Box(V(0,y+h-.2f),Size(length+.3f,.3f,.7f),ivory);wall.Save();
   }
   if(f<count-1)
   {
    var stair=Make("Stairs "+(f+1)+" to "+(f+2),ArboriumInterior.PartKind.Stair,f);
    float rise=h/2,angle=Mathf.Atan2(rise,run)*Mathf.Rad2Deg,diagonal=Mathf.Sqrt(run*run+rise*rise);
    for(int flight=0;flight<2;flight++)
    {
     float x=flight==0?-1.02f:1.02f;
     var q=Quaternion.Euler(flight==0?-angle:angle,0,0);float midY=y+(flight==0?rise*.5f:rise*1.5f);
     // Smooth navigation ramp sits just below the individually visible treads.
     stair.Box(P(x,midY-.065f,bottom+run*.5f),P(flightWidth,.12f,diagonal),wood,true,8,q);
     for(int step=0;step<steps;step++)
     {float z=flight==0?bottom+(step+.5f)*run/steps:end-(step+.5f)*run/steps;float yy=y+flight*rise+(step+1)*rise/steps;stair.Box(P(x,yy-.06f,z),P(flightWidth+.04f,.12f,run/steps+.02f),wood);stair.Box(P(x,yy+.006f,z+(flight==0?-.13f:.13f)),P(flightWidth,.025f,.045f),gold);}
     foreach(float edge in new[]{x-flightWidth/2,x+flightWidth/2}){stair.Box(P(edge,midY+1.05f,bottom+run*.5f),P(.08f,.1f,diagonal),wood,false,9,q);for(int k=0;k<=steps;k+=3){float z=bottom+k*run/steps;float yy=y+(flight==0?k*rise/steps:h-k*rise/steps);stair.Box(P(edge,yy+.52f,z),P(.06f,1.04f,.06f),gold);}}
    }
    stair.Box(P(0,y+rise-.1f,end+landing/2),P(halfWell*2,.2f,landing+.04f),wood,true,8);stair.Box(P(0,y+rise+1.05f,holeEnd-.05f),P(halfWell*2,.1f,.1f),wood);
    stair.Save();
   }
  }
  var roofPart=Make("Copper roof and carved eaves",ArboriumInterior.PartKind.Roof,count);
  roofPart.Roof(P(0,.12f+count*h,0),w+2,d+2,guild?9:Mathf.Min(7,w*.55f));roofPart.Box(P(0,count*h,0),P(w+1,.4f,d+1),ivory);
  if(guild)foreach(int side in new[]{-1,1}){var p=P(side*(w/2-1),0,d/2-1);roofPart.Box(p+Vector3.up*12,P(4.5f,24,4.5f),stone,true);roofPart.Roof(p+Vector3.up*24,6,6,9);roofPart.Box(p+Vector3.up*22,P(5.2f,.7f,5.2f),ivory);}
  roofPart.Save();
  building.gameObject.AddComponent<ArboriumInterior>().Configure(title,new Vector2(w,d),h,count,surfaces.ToArray());
 }
}
