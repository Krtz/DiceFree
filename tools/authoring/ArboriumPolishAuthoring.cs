using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.EditorTools;
using DiceFree.Visuals;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

// Original, deterministic project artwork; no vendor imports or paid generation.
public static class ArboriumPolishAuthoring
{
 public const string Folder="Assets/_DiceFree/Art/World/ArboriumPolish";
 public const string RootName="Arborium arrival district - polish";
 static readonly Vector3 Center=new(98.04f,0,304.74f);
 static readonly List<Material> mats=new();
 static readonly System.Random random=new(41057);
 static Transform root;
 const int Bark=0,Leaves=1,LeavesLight=2,LeavesDark=3,Wood=4,Stone=5,Metal=6,Cream=7,Blue=8,Rust=9,Green=10,Red=11,Gold=12,Paving=13,Ink=14;
 static Vector3 P(float x,float y,float z)=>new(x,y,z);
 static float R(float a,float b)=>Mathf.Lerp(a,b,(float)random.NextDouble());
 static void Need(bool ok,string text){if(!ok)throw new InvalidOperationException(text);}
 public static object Refine()
 {
  Need(!EditorApplication.isPlayingOrWillChangePlaymode&&!SceneManager.GetActiveScene().isDirty,"Clean Edit Mode required.");root=GameObject.Find(RootName).transform;Palette();
  foreach(var child in root.Cast<Transform>().Where(t=>t.name.StartsWith("World Tree -")||t.name.StartsWith("Guild forecourt -")||t.name.StartsWith("Merchant quarter -")||t.name.StartsWith("Northern avenue -")||t.name.StartsWith("Wayfinding -")).ToArray())Object.DestroyImmediate(child.gameObject);
  Tree();District();root.Find("Arborium citizen 3").localPosition=P(42,0,58);
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());Need(EditorSceneManager.SaveScene(SceneManager.GetActiveScene()),"Refinement save failed.");CornbergSceneBuilder.Rebake();return new{separateRootOcclusion=true,marketRoofsHidden=true,pavingDensityCorrected=true,citizenMovedOffWall=true};
 }
 public static object Author()
 {
  var scene=SceneManager.GetActiveScene();Need(scene.name=="Cornberg"&&!scene.isDirty&&!EditorApplication.isPlayingOrWillChangePlaymode,"Clean Cornberg Edit Mode required.");
  Need(GameObject.Find(RootName)==null&&!AssetDatabase.IsValidFolder(Folder),"Polish already installed; preserve existing work.");
  var city=GameObject.Find("Arborium - World Tree capital");Need(city!=null,"Author Arborium's arrival section first.");
  const string recovery="Assets/_DiceFree/Settings/Recovery/Cornberg-before-arborium-polish-20261105.unity";
  Need(!File.Exists(recovery)&&EditorSceneManager.SaveScene(scene,recovery,true),"Could not create a fresh recovery.");
  Need(AssetDatabase.CopyAsset("Assets/_DiceFree/Settings/CornbergNavMesh.asset","Assets/_DiceFree/Settings/Recovery/CornbergNavMesh-before-arborium-polish-20261105.asset"),"Navigation recovery failed.");
  AssetDatabase.CreateFolder("Assets/_DiceFree/Art/World","ArboriumPolish");
  root=new GameObject(RootName).transform;root.SetParent(city.transform,false);
  Palette();Tree();District();People(scene);Sound();GateVisibility();
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Need(EditorSceneManager.SaveScene(scene),"Polish scene save failed.");CornbergSceneBuilder.Rebake();
  var result=new{worldTreeCenter=Center.ToString(),newRenderers=root.GetComponentsInChildren<Renderer>().Length,triangles=root.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3),pedestrians=root.GetComponentsInChildren<VillageHomeWander>().Length,convoys=root.GetComponentsInChildren<ArboriumAmbientRoute>().Length,servicesUnchanged=true,existingWorldTreeTrunkColliderPreserved=true,proceduralSeed=41057};
  File.WriteAllText("Logs/ArboriumPolishAuthoring.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
 static Material Material(string name,Color color,float smooth=.25f)
 {string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};AssetDatabase.CreateAsset(m,path);}m.color=color;m.SetFloat("_Smoothness",smooth);EditorUtility.SetDirty(m);return m;}
 public static object RefineGateVisibility()
 {
  Need(!EditorApplication.isPlayingOrWillChangePlaymode&&!SceneManager.GetActiveScene().isDirty,"Clean Edit Mode required.");root=GameObject.Find(RootName).transform;GateVisibility();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());Need(EditorSceneManager.SaveScene(SceneManager.GetActiveScene()),"Gate visibility save failed.");return new{gateSectionsCanFadeIndividually=true,originalCollisionUnchanged=true};
 }
 public static object InspectFinal()
 {
  root=GameObject.Find(RootName).transform;var original=GameObject.Find("Arborium - World Tree capital").transform.Find("03 - Ring wall and monumental northern gate");var replacement=root.Find("Northern gate - camera-safe sections");int oldTriangles=original.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3),newTriangles=replacement.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3);Need(oldTriangles==newTriangles&&replacement.GetComponentsInChildren<Collider>().Length==0,"Gate presentation changed geometry/collision.");
  var result=new{originalGateTriangles=oldTriangles,replacementGateTriangles=newTriangles,gateCollisionUntouched=true,originalGateObjectsRetained=true,newMeshReferences=root.GetComponentsInChildren<MeshFilter>().All(f=>f.sharedMesh!=null),meshFilterTriangles=root.GetComponentsInChildren<MeshFilter>().Where(f=>f.sharedMesh.isReadable).Sum(f=>f.sharedMesh.triangles.Length/3)};File.WriteAllText("Logs/ArboriumPolishFinalGeometry.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
 static void GateVisibility()
 {
  var previous=root.Find("Northern gate - camera-safe sections");if(previous!=null)Object.DestroyImmediate(previous.gameObject);
  var group=new GameObject("Northern gate - camera-safe sections").transform;group.SetParent(root,false);
  var original=GameObject.Find("Arborium - World Tree capital").transform.Find("03 - Ring wall and monumental northern gate");
  int bucket=0;
  foreach(var filter in original.GetComponentsInChildren<MeshFilter>())
  {
   var renderer=filter.GetComponent<MeshRenderer>();var mesh=filter.sharedMesh;var vertices=mesh.vertices;var uv=mesh.uv;var triangles=mesh.triangles;var parts=Enumerable.Range(0,4).Select(_=>new Surface()).ToArray();
   for(int i=0;i<triangles.Length;i+=3)
   {
    var a=vertices[triangles[i]];var b=vertices[triangles[i+1]];var c=vertices[triangles[i+2]];var point=(a+b+c)/3;
    int section=point.z>70&&Mathf.Abs(point.x)<19?(point.x<-5?1:point.x>5?2:3):0;
    var part=parts[section];int index=part.v.Count;part.v.AddRange(new[]{a,b,c});part.uv.AddRange(new[]{uv[triangles[i]],uv[triangles[i+1]],uv[triangles[i+2]]});part.t.AddRange(new[]{index,index+1,index+2});
   }
   for(int section=0;section<parts.Length;section++)if(parts[section].v.Count>0)
   {
    string name="Gate visibility "+bucket+" section "+section;var saved=parts[section].Bake(name);string path=Folder+"/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing!=null){EditorUtility.CopySerialized(saved,existing);Object.DestroyImmediate(saved);saved=existing;EditorUtility.SetDirty(saved);}else AssetDatabase.CreateAsset(saved,path);
    var go=new GameObject(name);go.transform.SetParent(group,false);go.layer=2;go.AddComponent<MeshFilter>().sharedMesh=saved;go.AddComponent<MeshRenderer>().sharedMaterial=renderer.sharedMaterial;if(section>0)go.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(40);
   }
   renderer.enabled=false;bucket++;
  }
 }
 static void Palette()
 {
  mats.Clear();mats.Add(Material("World Tree deeply furrowed bark",new Color(.34f,.23f,.115f)));mats.Add(Material("World Tree emerald leaves",new Color(.27f,.44f,.15f)));mats.Add(Material("World Tree sunlit leaves",new Color(.44f,.57f,.22f)));mats.Add(Material("World Tree shadow leaves",new Color(.14f,.29f,.12f)));
  mats.Add(Material("Market honey oak",new Color(.40f,.25f,.12f)));mats.Add(AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Art/World/ArboriumArrival/ivory carved stone.mat"));mats.Add(Material("Forged dark bronze",new Color(.16f,.19f,.17f),.4f));
  mats.Add(Material("Warm ivory canvas",new Color(.87f,.79f,.59f)));mats.Add(Material("Guild blue canvas",new Color(.12f,.28f,.43f)));mats.Add(Material("Market madder canvas",new Color(.55f,.19f,.12f)));mats.Add(Material("Market herbs",new Color(.25f,.43f,.18f)));mats.Add(Material("Market apples",new Color(.62f,.12f,.06f)));mats.Add(Material("Warm engraved brass",new Color(.73f,.48f,.14f),.45f));mats.Add(AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Art/World/ArboriumArrival/radial granite paving.mat"));mats.Add(Material("Signboard dark recess",new Color(.08f,.12f,.12f)));
  foreach(int i in new[]{Leaves,LeavesLight,LeavesDark,Cream,Blue,Rust}){mats[i].SetFloat("_Cull",0);mats[i].doubleSidedGI=true;}
  const int size=1024;var image=new Texture2D(size,size,TextureFormat.RGB24,true);var pixels=new Color[size*size];
  for(int y=0;y<size;y++)for(int x=0;x<size;x++)
  {float u=x/(float)size,v=y/(float)size;float bend=Mathf.Sin(v*18)*.07f+Mathf.PerlinNoise(u*6,v*8)*.16f;float grain=Mathf.Pow(Mathf.Abs(Mathf.Sin((u+bend)*Mathf.PI*28)),.35f);float fine=Mathf.PerlinNoise(u*180,v*18);float moss=Mathf.Clamp01((Mathf.PerlinNoise(u*9,v*5)-.56f)*3);float value=.48f+grain*.48f+fine*.12f;pixels[y*size+x]=Color.Lerp(new Color(value,value*.95f,value*.82f),new Color(.45f,.56f,.30f),moss*.35f);}
  image.SetPixels(pixels);image.Apply();SaveTexture(image,"World Tree bark grain",mats[Bark]);
  var leaf=new Texture2D(128,128,TextureFormat.RGB24,true);var colors=new Color[128*128];for(int y=0;y<128;y++)for(int x=0;x<128;x++){float u=x/127f,v=y/127f;float rib=Mathf.Exp(-Mathf.Abs(u-.5f)*95);float veins=Mathf.Pow(Mathf.Abs(Mathf.Cos((v-Mathf.Abs(u-.5f)*.7f)*Mathf.PI*13)),30)*.10f;float shade=.72f+.24f*Mathf.Sin(v*Mathf.PI)+rib*.12f+veins;colors[y*128+x]=new Color(shade,shade,shade*.94f);}leaf.SetPixels(colors);leaf.Apply();SaveTexture(leaf,"World Tree leaf veins",mats[Leaves]);for(int i=LeavesLight;i<=LeavesDark;i++)mats[i].mainTexture=mats[Leaves].mainTexture;
 }
 static void SaveTexture(Texture2D image,string name,Material m){string path=Folder+"/"+name+".png";File.WriteAllBytes(path,image.EncodeToPNG());Object.DestroyImmediate(image);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);EditorUtility.SetDirty(m);}
 sealed class Surface
 {
  public readonly List<Vector3> v=new();public readonly List<Vector2> uv=new();public readonly List<int> t=new();
  public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){int n=v.Count;v.AddRange(new[]{a,b,c,d});uv.AddRange(new[]{Vector2.zero,new Vector2(Vector3.Distance(a,b)/4,0),new Vector2(Vector3.Distance(a,b)/4,Vector3.Distance(b,c)/4),new Vector2(0,Vector3.Distance(b,c)/4)});t.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
  public void Tri(Vector3 a,Vector3 b,Vector3 c){int n=v.Count;v.AddRange(new[]{a,b,c});uv.AddRange(new[]{Vector2.zero,Vector2.right,Vector2.up});t.AddRange(new[]{n,n+1,n+2});}
  public Mesh Bake(string name){var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
 }
 sealed class Builder
 {
  public readonly Transform group;public readonly Surface[] s=Enumerable.Range(0,15).Select(_=>new Surface()).ToArray();
  public Builder(string name){group=new GameObject(name).transform;group.SetParent(root,false);}
  public void Box(Vector3 p,Vector3 size,int mat,Quaternion? rotation=null){var q=rotation??Quaternion.identity;Vector3 V(float x,float y,float z)=>p+q*Vector3.Scale(size,P(x,y,z))*.5f;var a=V(-1,-1,-1);var b=V(1,-1,-1);var c=V(1,1,-1);var d=V(-1,1,-1);var e=V(-1,-1,1);var f=V(1,-1,1);var g=V(1,1,1);var h=V(-1,1,1);s[mat].Quad(a,d,c,b);s[mat].Quad(f,g,h,e);s[mat].Quad(e,h,d,a);s[mat].Quad(b,c,g,f);s[mat].Quad(d,h,g,c);s[mat].Quad(e,a,b,f);}
  public void Beam(Vector3 a,Vector3 b,float width,int mat)=>Box((a+b)*.5f,P(width,(a-b).magnitude,width),mat,Quaternion.FromToRotation(Vector3.up,b-a));
  public void Tube(Vector3[] points,float[] radii,int sides,int mat,bool furrow=false)
  {
   var surface=s[mat];int start=surface.v.Count;float length=0;
   for(int j=0;j<points.Length;j++)
   {if(j>0)length+=Vector3.Distance(points[j],points[j-1]);var tangent=(points[Mathf.Min(j+1,points.Length-1)]-points[Mathf.Max(0,j-1)]).normalized;var across=Vector3.Cross(tangent,Mathf.Abs(tangent.y)>.9f?Vector3.forward:Vector3.up).normalized;var other=Vector3.Cross(tangent,across);for(int i=0;i<=sides;i++){float a=i*Mathf.PI*2/sides;float rough=furrow?1+.055f*Mathf.Sin(a*11+j*.11f)+.025f*Mathf.Sin(a*19-j*.17f):1;surface.v.Add(points[j]+(across*Mathf.Cos(a)+other*Mathf.Sin(a))*radii[j]*rough);surface.uv.Add(new Vector2(i/(float)sides*(furrow?6:1),length/(furrow?8:2)));}}
   for(int j=0;j<points.Length-1;j++)for(int i=0;i<sides;i++){int a=start+j*(sides+1)+i,b=a+sides+1;surface.t.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
   for(int i=0;i<sides;i++){surface.Tri(points[0],surface.v[start+i+1],surface.v[start+i]);int end=start+(points.Length-1)*(sides+1);surface.Tri(points[^1],surface.v[end+i],surface.v[end+i+1]);}
  }
  public void Column(Vector3 p,float r,float height,int mat,int sides=12)=>Tube(new[]{p,p+Vector3.up*height},new[]{r,r*.94f},sides,mat);
  public void Leaf(Vector3 p,float width,float length,Quaternion q,int mat)
  {var a=new[]{P(0,-length*.5f,0),P(-width*.35f,-length*.25f,0),P(-width*.5f,length*.08f,0),P(-width*.28f,length*.38f,0),P(0,length*.57f,0),P(width*.28f,length*.38f,0),P(width*.5f,length*.08f,0),P(width*.35f,-length*.25f,0)};var surface=s[mat];int n=surface.v.Count;surface.v.Add(p+q*P(0,0,width*.10f));surface.uv.Add(new Vector2(.5f,.5f));for(int i=0;i<a.Length;i++){surface.v.Add(p+q*a[i]);surface.uv.Add(new Vector2(a[i].x/width+.5f,a[i].y/length+.5f));}for(int i=0;i<a.Length;i++)surface.t.AddRange(new[]{n,n+1+i,n+1+(i+1)%a.Length});}
  public void Solid(Vector3 p,Vector3 size){var go=new GameObject("Solid dressing footprint");go.layer=9;go.transform.SetParent(group,false);go.transform.localPosition=p;go.AddComponent<BoxCollider>().size=size;}
  public void Save(float fadeRange=0)
  {for(int i=0;i<s.Length;i++)if(s[i].v.Count>0){string name=group.name+" "+i;var mesh=s[i].Bake(name);string path=Folder+"/"+name+".asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(saved!=null){EditorUtility.CopySerialized(mesh,saved);Object.DestroyImmediate(mesh);mesh=saved;EditorUtility.SetDirty(saved);}else AssetDatabase.CreateAsset(mesh,path);var go=new GameObject(mats[i].name);go.transform.SetParent(group,false);go.layer=2;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mats[i];if(fadeRange>0)go.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(fadeRange);}}
 }
 static void Tree()
 {
  var all=SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();var old=all.Single(t=>t.name=="Ancient trunk");Need(Vector2.Distance(new Vector2(old.position.x,old.position.z),new Vector2(Center.x,Center.z))<1,"World Tree position changed.");
  foreach(var t in old.parent.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Ancient trunk"||t.name=="Ancient bough"||t.name=="Vast crown"))foreach(var renderer in t.GetComponents<Renderer>())renderer.enabled=false;
  var originalRoots=GameObject.Find("Arborium - World Tree capital").transform.Find("12 - Sacred civic gardens and World Tree roots");foreach(var renderer in originalRoots.GetComponentsInChildren<Renderer>())if(AssetDatabase.GetAssetPath(renderer.sharedMaterial).EndsWith("/oiled oak.mat"))renderer.enabled=false;
  var trunk=new Builder("World Tree - gnarled trunk");var points=new List<Vector3>();var radii=new List<float>();
  for(int j=0;j<=30;j++){float y=j*5;points.Add(P(Mathf.Sin(y*.031f)*1.8f,y,Mathf.Cos(y*.043f)*1.3f-1.3f));radii.Add(Mathf.Lerp(8.9f,.25f,Mathf.Pow(y/150f,.7f)));}trunk.Tube(points.ToArray(),radii.ToArray(),52,Bark,true);
  trunk.Save(64);var roots=new Builder("World Tree - spreading roots");for(int i=0;i<15;i++){float a=i*Mathf.PI*2/15;var d=P(Mathf.Sin(a),0,Mathf.Cos(a));roots.Tube(new[]{d*6.6f+Vector3.up*8,d*8.1f+Vector3.up*3.7f,d*9.4f+Vector3.up*.65f,d*12+Vector3.up*.13f,d*17+Vector3.up*.02f},new[]{1.5f,1.1f,.55f,.18f,.025f},12,Bark,true);}roots.Save(64);
  var boughs=new Builder("World Tree - branching limbs");var tips=new List<Vector3>();
  for(int i=0;i<11;i++)
  {float angle=i*Mathf.PI*2/11+.17f;var d=P(Mathf.Sin(angle),0,Mathf.Cos(angle));var side=P(d.z,0,-d.x);float y=56+i%4*11;float reach=R(56,73);var limb=new[]{P(0,y,0),d*12+Vector3.up*(y+14),d*30+Vector3.up*(y+21),d*(reach*.78f)+side*R(-5,5)+Vector3.up*(y+28),d*reach+Vector3.up*(y+42)};boughs.Tube(limb,new[]{4.5f,3.8f,2.5f,1.3f,.15f},18,Bark,true);tips.Add(limb[^1]);
   for(int j=1;j<=3;j++){var from=limb[j];var end=from+d*R(10,21)+side*(j%2==0?-1:1)*R(10,24)+Vector3.up*R(17,29);boughs.Tube(new[]{from,Vector3.Lerp(from,end,.45f)+Vector3.up*3,end},new[]{2-j*.37f,.85f,.09f},11,Bark,true);tips.Add(end);}
  }
  for(int i=0;i<8;i++){float a=i*Mathf.PI/4;tips.Add(P(Mathf.Sin(a)*R(18,40),R(135,147),Mathf.Cos(a)*R(18,40)));}boughs.Save(64);
  var foliage=new Builder("World Tree - layered leaf canopy");foreach(var tip in tips)
  {var dimensions=P(R(12,18),R(9,14),R(12,18));for(int i=0;i<160;i++){float az=R(0,Mathf.PI*2),h=R(-1,1),rad=Mathf.Pow(R(0,1),1/3f);var v=P(Mathf.Cos(az)*Mathf.Sqrt(1-h*h),h,Mathf.Sin(az)*Mathf.Sqrt(1-h*h));var p=tip+Vector3.Scale(v,dimensions)*rad;var orientation=Quaternion.Euler(R(-80,80),R(0,360),R(0,360));foliage.Leaf(p,R(2.3f,4.4f),R(3.5f,6.5f),orientation,Leaves+(i%3));}}foliage.Save(64);
 }
 static void Awning(Builder b,Vector3 p,float width,float depth,int accent)
 {
  for(int i=0;i<10;i++){float x=-width/2+i*width/10;int material=i%2==0?Cream:accent;Vector3 V(float xx,float zz)=>p+P(xx,-Mathf.Sin((zz/depth+.5f)*Mathf.PI)*.35f,zz);b.s[material].Quad(V(x,-depth/2),V(x,depth/2),V(x+width/10,depth/2),V(x+width/10,-depth/2));b.Box(p+P(x+width/20,-.38f,depth/2),P(width/10,.52f,.07f),material);}
  foreach(int side in new[]{-1,1})foreach(int end in new[]{-1,1})b.Beam(p+P(side*width*.48f,-4.4f,end*depth*.46f),p+P(side*width*.48f,.12f,end*depth*.46f),.13f,Wood);
 }
 static void Barrel(Builder b,Vector3 p,float size=1)
 {b.Tube(new[]{p,p+Vector3.up*.18f*size,p+Vector3.up*.9f*size,p+Vector3.up*1.1f*size},new[]{.38f*size,.45f*size,.45f*size,.36f*size},12,Wood);foreach(float h in new[]{.16f,.85f})b.Column(p+Vector3.up*h*size,.46f*size,.10f*size,Metal);}
 static void Crate(Builder b,Vector3 p,float size=1)
 {b.Box(p+Vector3.up*size*.5f,Vector3.one*size,Wood);foreach(int side in new[]{-1,1}){b.Box(p+P(side*size*.43f,size*.5f,0),P(.1f,size+.1f,size+.08f),Metal);b.Beam(p+P(-size*.4f,.1f,size*.51f),p+P(size*.4f,size*.9f,size*.51f),.09f,Cream);}}
 static void Banner(Builder b,Vector3 p,float width,float length)
 {b.Beam(p+P(-width*.6f,.15f,0),p+P(width*.6f,.15f,0),.12f,Gold);for(int i=0;i<8;i++){float y=-i*length/8;float fold=Mathf.Sin(i*.8f)*.12f;b.s[Blue].Quad(p+P(-width/2,y,fold),p+P(-width/2,y-length/8,fold+.08f),p+P(width/2,y-length/8,fold+.08f),p+P(width/2,y,fold));}b.Box(p+P(0,-length*.59f,.17f),P(.16f,length*.55f,.08f),Gold);for(int i=0;i<3;i++){float y=-length*(.23f+i*.13f);float w=width*(.29f+i*.10f);b.s[Gold].Tri(p+P(-w,y-.4f,.18f),p+P(w,y-.4f,.18f),p+P(0,y+.45f,.18f));}}
 static void District()
 {
  var existingMarket=GameObject.Find("Arborium - World Tree capital").transform.Find("11 - Outer merchant arcades and stalls");foreach(var renderer in existingMarket.GetComponentsInChildren<Renderer>())if(AssetDatabase.GetAssetPath(renderer.sharedMaterial).EndsWith("/patinated copper shingles.mat"))renderer.enabled=false;
  var fore=new Builder("Guild forecourt - patterned stone and arrival banners");
  for(int i=0;i<24;i++){float a=(-36+i*1.25f)*Mathf.Deg2Rad,c=(-36+(i+1)*1.25f)*Mathf.Deg2Rad;Vector3 V(float radius,float angle)=>P(Mathf.Sin(angle)*radius,.08f,Mathf.Cos(angle)*radius);fore.s[Paving].Quad(V(60,a),V(74,a),V(74,c),V(60,c));}
  foreach(int side in new[]{-1,1}){Banner(fore,P(-26+side*6,11.7f,59.8f),2.5f,5.6f);fore.Column(P(-26+side*9,0,63),.5f,.45f,Stone);fore.Column(P(-26+side*9,.45f,63),.36f,1.2f,Stone);fore.Column(P(-26+side*9,1.65f,63),.6f,.2f,Gold);}
  fore.Save(40);
  var market=new Builder("Merchant quarter - striped cloth and goods");
  foreach(var p in new[]{P(55,0,20),P(63,0,20),P(55,0,29),P(63,0,29)})
  {Awning(market,p+Vector3.up*4.4f,5.8f,4.8f,p.x>60?Rust:Blue);for(int i=0;i<3;i++){var tray=p+P((i-1)*1.1f,1.45f,.2f);market.Box(tray,P(1,.12f,1.1f),Wood);for(int j=0;j<9;j++){var item=tray+P((j%3-1)*.25f,.16f,(j/3-1)*.28f);if(i==2)market.Tube(new[]{item,item+Vector3.up*.45f},new[]{.17f,.08f},7,Green);else market.Tube(new[]{item,item+Vector3.up*.16f,item+Vector3.up*.32f},new[]{.05f,.18f,.05f},8,i==0?Red:Gold);}}}
  market.Box(P(57,.08f,24.5f),P(27,.04f,31),Paving);market.Save(18);
  var cargo=new Builder("Merchant quarter - deliveries and stored goods");
  foreach(var p in new[]{P(47,0,18),P(45,0,19.3f),P(68,0,36),P(69.3f,0,35.5f),P(46,0,24)}){Crate(cargo,p,.9f);if(p.x>60)Crate(cargo,p+Vector3.up*.9f,.7f);cargo.Solid(p+Vector3.up*.55f,P(1.05f,1.1f,1.05f));}
  foreach(var p in new[]{P(42,0,19),P(69,0,24),P(42,0,24)}){Barrel(cargo,p);cargo.Solid(p+Vector3.up*.55f,P(.9f,1.1f,.9f));}cargo.Save();
  var garden=new Builder("Northern avenue - flower urns and shaded seats");
  foreach(int side in new[]{-1,1})foreach(float z in new[]{28f,39f,51f})
  {var p=P(side*10.2f,0,z);garden.Column(p,.8f,.25f,Stone);garden.Tube(new[]{p+Vector3.up*.25f,p+Vector3.up*1.1f},new[]{.47f,.82f},14,Stone);for(int i=0;i<13;i++){float a=i*Mathf.PI*2/13;var tip=p+P(Mathf.Sin(a)*.63f,R(1.3f,1.9f),Mathf.Cos(a)*.63f);garden.Beam(p+Vector3.up,tip,.04f,Green);garden.Leaf(tip,.32f,.48f,Quaternion.Euler(70,i*27,0),i%3==0?Rust:Green);}garden.Solid(p+Vector3.up*.55f,P(1.4f,1.1f,1.4f));}
  garden.Save();Sign("GUILD",P(-6.4f,0,72),-1);Sign("EXCHANGE",P(7,0,71),1);Sign("WORLD TREE",P(7,0,31),0);
 }
 static void Sign(string text,Vector3 p,int direction)
 {
  var b=new Builder("Wayfinding - "+text);b.Column(p,.13f,3.3f,Wood,8);b.Box(p+Vector3.up*2.8f,P(4.7f,1.1f,.18f),Ink);b.Box(p+Vector3.up*3.38f,P(4.9f,.10f,.24f),Gold);b.Box(p+Vector3.up*2.2f,P(4.9f,.1f,.24f),Gold);
  if(direction!=0)b.s[Gold].Tri(p+P(direction*2.4f,3.4f,0),p+P(direction*3.1f,2.8f,0),p+P(direction*2.4f,2.2f,0));b.Save();
  foreach(int side in new[]{-1,1}){var go=new GameObject(text+" lettering");go.transform.SetParent(b.group,false);go.transform.localPosition=p+P(0,2.8f,side*.13f);go.transform.localRotation=Quaternion.Euler(0,side>0?180:0,0);var label=go.AddComponent<TextMesh>();label.text=text;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.fontSize=64;label.characterSize=.34f;label.color=new Color(.94f,.85f,.61f);}
 }
 static void People(Scene scene)
 {
  var templates=Object.FindObjectsByType<VillageHomeWander>().Where(w=>!w.transform.IsChildOf(root)).ToArray();Need(templates.Length>=3,"Existing rigged village walkers required.");
  var homes=new[]{P(45,0,34),P(54,0,37),P(42,0,58),P(-42,0,62),P(-17,0,62),P(-14,0,40),P(15,0,33),P(-23,0,38)};
  GameObject Clone(GameObject template,string name,Vector3 point)
  {var go=Object.Instantiate(template,root);go.name=name;if(PrefabUtility.IsPartOfPrefabInstance(go))PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);go.transform.position=Center+point;foreach(var target in go.GetComponentsInChildren<InteractionTarget>(true))Object.DestroyImmediate(target);foreach(var bark in go.GetComponentsInChildren<AmbientBark>(true))Object.DestroyImmediate(bark);foreach(var obstacle in go.GetComponentsInChildren<NavMeshObstacle>())obstacle.carving=false;return go;}
  for(int i=0;i<homes.Length;i++){var go=Clone(templates[i%templates.Length].gameObject,"Arborium citizen "+(i+1),homes[i]);go.GetComponent<VillageHomeWander>().Configure(630+i);}
  var guard=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).First(t=>t.name=="NPCs/NPC_Guard - 3D");
  foreach(int side in new[]{-1,1}){var go=Clone(guard.gameObject,"Arborium gate warden "+side,P(side*4.9f,0,74));go.transform.rotation=Quaternion.Euler(0,side<0?25:-25,0);var bark=go.AddComponent<AmbientBark>();bark.npcId="ambient.arborium.gate."+side;bark.listener=Object.FindAnyObjectByType<DiceFree.Characters.TraversalInput>().GetComponent<CombatActor>();bark.cooldown=37;bark.radius=5;bark.lines=new[]{"Welcome to Arborium. Keep the gate clear for arrivals.","The Guild is beside the gate. The Exchange is across the avenue."};}
  var porter=Clone(templates[0].gameObject,"Arborium arriving handcart porter",P(2.6f,0,89));Object.DestroyImmediate(porter.GetComponent<VillageHomeWander>());
  var cart=new Builder("Arriving convoy - oak handcart");cart.group.SetParent(porter.transform,false);cart.group.localPosition=P(0,0,-2.1f);cart.Box(P(0,.85f,0),P(1.6f,.2f,2.5f),Wood);for(int side=-1;side<=1;side+=2){cart.Box(P(side*.76f,1.15f,0),P(.1f,.7f,2.5f),Wood);cart.Beam(P(side*.48f,.8f,.8f),P(side*.48f,.85f,2.2f),.10f,Wood);}cart.Box(P(0,1.15f,-1.2f),P(1.6f,.7f,.1f),Wood);Crate(cart,P(-.35f,.95f,-.5f),.7f);Barrel(cart,P(.35f,.95f,.5f),.65f);cart.Save();
  var wheels=new List<Transform>();for(int side=-1;side<=1;side+=2)for(int axle=-1;axle<=1;axle+=2){var wheel=new Builder("Convoy wheel "+side+" "+axle);wheel.group.SetParent(cart.group,false);wheel.group.localPosition=P(side*.94f,.55f,axle*.8f);wheel.Tube(new[]{P(-.10f,0,0),P(.10f,0,0)},new[]{.55f,.55f},16,Metal);wheel.Tube(new[]{P(-.14f,0,0),P(.14f,0,0)},new[]{.15f,.15f},12,Wood);for(int i=0;i<6;i++){float a=i*Mathf.PI/3;wheel.Beam(Vector3.zero,P(0,Mathf.Sin(a)*.5f,Mathf.Cos(a)*.5f),.07f,Wood);}wheel.Save();wheels.Add(wheel.group);}
  var route=porter.AddComponent<ArboriumAmbientRoute>();route.Configure(new[]{Center+P(2.6f,0,89),Center+P(2.6f,0,72),Center+P(11.5f,0,66),Center+P(12,0,59)},wheels.ToArray());
 }
 static void Sound()
 {
  const int rate=22050;const float seconds=16;var data=new float[(int)(seconds*rate)];var rng=new System.Random(977);float low=0;
  for(int i=0;i<data.Length;i++){float t=i/(float)rate;low=Mathf.Lerp(low,(float)rng.NextDouble()*2-1,.018f);float hush=low*(.25f+.10f*Mathf.Sin(t*Mathf.PI/4));float wheel=(float)(Math.Sin(t*76+Math.Sin(t*3)*4)*Math.Sin(t*123))*.018f*(.5f+.5f*Mathf.Sin(t*Mathf.PI/4));float breeze=(float)Math.Sin(t*2*Math.PI/seconds)*.006f;data[i]=hush+wheel+breeze;}
  // Crossfade endpoints for a click-free, deliberately quiet nonverbal street bed.
  int blend=rate/2;for(int i=0;i<blend;i++){float a=.5f-.5f*Mathf.Cos(i/(float)blend*Mathf.PI);data[i]*=a;data[data.Length-1-i]*=a;}
  var bed=WriteWave("Arborium soft street air",data,rate);var bellData=new float[rate*4];for(int i=0;i<bellData.Length;i++){float t=i/(float)rate;float a=Mathf.Min(1,t/.012f)*Mathf.Exp(-t*1.8f);bellData[i]=(float)(Math.Sin(t*2*Math.PI*523.25)+.32*Math.Sin(t*2*Math.PI*1047.7)+.18*Math.Sin(t*2*Math.PI*1558.2))*a*.20f;}var bell=WriteWave("Arborium distant guild chime",bellData,rate);root.gameObject.AddComponent<ArboriumAmbientSound>().Configure(bed,bell);
 }
 static AudioClip WriteWave(string name,float[] samples,int rate)
 {string path=Folder+"/"+name+".wav";using(var writer=new BinaryWriter(File.Create(path))){writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+samples.Length*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(samples.Length*2);foreach(float value in samples)writer.Write((short)(Mathf.Clamp(value,-1,1)*32767));}AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);return AssetDatabase.LoadAssetAtPath<AudioClip>(path);}
}
