using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Characters;
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
public static class ArboriumRoomsAuthoring
{
 const string Folder="Assets/_DiceFree/Art/World/ArboriumRooms";
 static Material oak,cloth,ivory,gold,dark,glass;
 static int serial,rooms,boards,lights;
 static Transform floor,building;static ArboriumInterior interior;static float y;
 static Vector3 P(float x,float yy,float z)=>new Vector3(x,yy,z);
 static void Need(bool b,string message){if(!b)throw new InvalidOperationException(message);}
 static Material M(string n)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Art/World/ArboriumArrival/"+n+".mat");
 public static object Author()
 {
  var scene=SceneManager.GetActiveScene();Need(scene.name=="Cornberg"&&!scene.isDirty&&!EditorApplication.isPlayingOrWillChangePlaymode&&!AssetDatabase.IsValidFolder(Folder),"Clean unauthored Cornberg required");
  Need(EditorSceneManager.SaveScene(scene,"Assets/_DiceFree/Settings/Recovery/Cornberg-before-arborium-rooms-20261105.unity",true),"Scene recovery failed");Need(AssetDatabase.CopyAsset("Assets/_DiceFree/Settings/CornbergNavMesh.asset","Assets/_DiceFree/Settings/Recovery/CornbergNavMesh-before-arborium-rooms-20261105.asset"),"Nav recovery failed");AssetDatabase.CreateFolder("Assets/_DiceFree/Art/World","ArboriumRooms");
  oak=AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Art/World/ArboriumFloors/Interior oak.mat");cloth=M("guild blue cloth");ivory=M("ivory carved stone");gold=M("old gilded bronze");dark=M("deep recesses");glass=M("practical enchanted glass");Need(new[]{oak,cloth,ivory,gold,dark,glass}.All(m=>m!=null),"City palette missing");serial=rooms=boards=lights=0;
  var all=GameObject.Find("Arborium - gates and inhabited floors").GetComponentsInChildren<ArboriumInterior>();
  foreach(var b in all)
  {
   interior=b;building=b.transform;string title=b.BuildingName;
   string[] names=title.StartsWith("Adventurers")?new[]{"Reception","Common room","Training room","Planning room"}:title.StartsWith("Northern")?new[]{"Exchange reception","Records office","Counting room","Reserve archive"}:title.StartsWith("Artisan")?new[]{"Workshop","Assembly room","Storeroom"}:title.StartsWith("Mage")?new[]{"Study","Library","Experiment room","Observatory"}:new[]{"Sitting room","Dining and study","Bedroom"};
   Need(names.Length==b.FloorCount,"Room count mismatch");b.ConfigureFloorNames(names);EditorUtility.SetDirty(b);
   for(int f=0;f<b.FloorCount;f++)
   {
    y=.12f+f*b.StoreyHeight;floor=building.Find("Floor "+(f+1)+" - oak rooms and stairwell");Need(floor!=null,"Floor missing");
    var group=new GameObject("Furnished - "+names[f]).transform;group.SetParent(floor,false);floor=group;rooms++;
    float w=b.Footprint.x,d=b.Footprint.y;bool small=w<9;
    if(small)
    {
     if(f==0){Bench(-2.35f,4.05f,2.2f);Table(2.55f,4.05f,1.7f,.85f);Book(2.5f,1.03f,4.05f,ivory);}
     else if(f==1){Table(-2.3f,4.05f,2.3f,.9f);Book(-2.6f,1.03f,4.05f,cloth);Bench(2.6f,4.15f,1.7f);}
     else{Bed(-2.25f,4.05f);Cabinet(2.65f,4.05f,1.6f,.8f,1.2f);}
    }
    else if(title.StartsWith("Adventurers"))
    {
     if(f==0){Desk(-6,4.5f);Bench(6,4.8f,4);Rack(7,-4.5f);}
     if(f==1){foreach(float x in new[]{-6f,6f}){Table(x,3.8f,4,1.6f);Bench(x,5.4f,4);Bench(x,2.2f,4);for(int i=-1;i<=1;i++)Cup(x+i,1.04f,3.8f);}}
     if(f==2){Rack(-6,4);Rack(7,-4.5f);Practice(7,3.6f);Box("Training mat",P(7,y+.025f,3.6f),P(3,.035f,3),cloth);}
     if(f==3){Table(5,3.9f,4,2.4f);Box("Survey map",P(5,y+1.045f,3.9f),P(3,.025f,1.7f),ivory);for(int i=0;i<3;i++)Ball("Map marker",P(4+i,y+1.12f,4),.12f,gold);Shelf(7,-4.3f,4);}
    }
    else if(title.StartsWith("Northern"))
    {
     if(f==0){Cabinet(-6,2.1f,5,1.2f,1.1f);Box("Brass counter trim",P(-6,y+1.14f,2.1f),P(5.1f,.08f,1.3f),gold);Book(-7,1.22f,2.1f,ivory);Bench(6,4.4f,4);MoveBanker(-6,1);}
     if(f==1){Desk(-6,3.8f);Desk(6,3.8f);Shelf(7,-3.5f,4);}
     if(f==2){foreach(float x in new[]{-6f,6f}){Table(x,3.7f,4,1.6f);for(int k=0;k<6;k++)Box("Stack of counting tokens",P(x-1.2f+k*.45f,y+1.1f,3.7f),P(.24f,.1f,.24f),gold);}}
     if(f==3){foreach(float x in new[]{-6f,6f}){Cabinet(x,4,4,1.3f,1.5f);Crate(x,-3.5f,1.5f);Crate(x+2,-3.5f,1.2f);}}
    }
    else if(title.StartsWith("Artisan"))
    {
     if(f==0){Table(3.55f,3.2f,2.5f,1.2f);Tools(3.55f,3.2f);AlongWall(()=>Rack(3.6f,-3.8f),3.6f,-3.8f);}
     if(f==1){Table(3.5f,3.2f,2.5f,1.2f);Box("Folded cloth",P(3.4f,y+1.08f,3.2f),P(1,.15f,.7f),cloth);}
     if(f==2){Crate(3.5f,3.3f,1.4f);Crate(-3.5f,3.3f,1.1f);}
    }
    else
    {
     if(f==0){Desk(3.4f,3.3f);Book(3.3f,1.05f,3.3f,cloth);}
     if(f==1){Shelf(3.6f,3.2f,2.5f);AlongWall(()=>Shelf(3.6f,-3.5f,2.5f),3.6f,-3.5f);}
     if(f==2){Table(3.4f,3.3f,2.5f,1.3f);Ball("Study globe",P(3.4f,y+1.45f,3.3f),.62f,glass);Cup(2.7f,1.05f,3.3f);}
     if(f==3){Table(3.4f,3.3f,2.5f,1.5f);Box("Star chart",P(3.4f,y+1.04f,3.3f),P(1.9f,.025f,1.1f),ivory);Ball("Armillary globe",P(3.4f,y+1.65f,3.3f),.8f,gold);}
    }
    if(f==0){Directory(title,names,w,d);Lamp(small?2.8f:w/2-1.4f,d/2-1.4f);}
    SaveMeshes();
   }
  }
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Need(EditorSceneManager.SaveScene(scene),"Room save failed");CornbergSceneBuilder.Rebake();var result=new{rooms,boards,lights,bankInside=true,practiceTarget=true,meshAssets=serial};File.WriteAllText("Logs/ArboriumRoomsAuthoring.json",Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
 }
 static GameObject Shape(string name,PrimitiveType type,Vector3 position,Vector3 size,Material mat,bool solid=false)
 {
  var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(floor,false);go.transform.localPosition=position;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(go.GetComponent<Collider>());go.layer=2;
  if(solid){var c=new GameObject(name+" collision");c.layer=9;c.transform.SetParent(floor,false);c.transform.localPosition=position;c.AddComponent<BoxCollider>().size=size;}
  return go;
 }
 static void Box(string n,Vector3 p,Vector3 s,Material m,bool solid=false)=>Shape(n,PrimitiveType.Cube,p,s,m,solid);
 static void AlongWall(Action create,float x,float z){var parent=floor;var group=new GameObject("Wall-side furniture").transform;group.SetParent(parent,false);floor=group;create();floor=parent;var q=Quaternion.Euler(0,-90,0);group.localRotation=q;group.localPosition=P(4.75f,0,z)-q*P(x,0,z);}
 static void Ball(string n,Vector3 p,float diameter,Material m)=>Shape(n,PrimitiveType.Sphere,p,Vector3.one*diameter,m);
 static void Table(float x,float z,float width,float depth)
 {Box("Oak tabletop",P(x,y+.96f,z),P(width,.14f,depth),oak);foreach(float a in new[]{-1f,1f})foreach(float b in new[]{-1f,1f})Box("Turned table leg",P(x+a*(width/2-.18f),y+.45f,z+b*(depth/2-.18f)),P(.18f,.9f,.18f),oak);Collision("Table",x,z,width,depth,1.03f);}
 static void Collision(string name,float x,float z,float width,float depth,float height){var go=new GameObject(name+" collision");go.layer=9;go.transform.SetParent(floor,false);go.transform.localPosition=P(x,y+height/2,z);go.AddComponent<BoxCollider>().size=P(width,height,depth);}
 static void Bench(float x,float z,float width){Box("Bench cushion",P(x,y+.6f,z),P(width,.2f,.7f),cloth);Box("Bench back",P(x,y+1.1f,z+.35f),P(width,.85f,.13f),oak);foreach(float a in new[]{-1f,1f})Box("Bench trestle",P(x+a*(width/2-.2f),y+.24f,z),P(.16f,.48f,.65f),oak);Collision("Bench",x,z,width,.9f,1.5f);}
 static void Book(float x,float yy,float z,Material m){Box("Bound volume",P(x,y+yy,z),P(.55f,.1f,.4f),m);Box("Book leaves",P(x,y+yy,z),P(.5f,.065f,.41f),ivory);}
 static void Cup(float x,float yy,float z)=>Shape("Pewter cup",PrimitiveType.Cylinder,P(x,y+yy+.1f,z),P(.17f,.1f,.17f),gold);
 static void Desk(float x,float z){Table(x,z,2.6f,1.2f);Book(x-.55f,1.06f,z,ivory);Cup(x+.7f,1.05f,z);}
 static void Cabinet(float x,float z,float w,float d,float h){Box("Paneled cabinet",P(x,y+h/2,z),P(w,h,d),oak,true);foreach(float side in new[]{-1f,1f}){Box("Cabinet panel",P(x+side*w*.25f,y+h*.5f,z+d/2+.02f),P(w*.4f,h*.7f,.06f),dark);Ball("Bronze handle",P(x+side*.14f,y+h*.55f,z+d/2+.09f),.1f,gold);}}
 static void Crate(float x,float z,float size){Box("Sealed supply crate",P(x,y+size/2,z),Vector3.one*size,oak,true);foreach(float side in new[]{-1f,1f})Box("Crate binding",P(x+side*size*.35f,y+size/2,z),P(.1f,size+.04f,size+.04f),gold);}
 static void Bed(float x,float z){Box("Bed frame",P(x,y+.33f,z),P(2.3f,.6f,.95f),oak,true);Box("Linen mattress",P(x,y+.7f,z),P(2.1f,.2f,.85f),ivory);Box("Blue coverlet",P(x+.35f,y+.83f,z),P(1.35f,.08f,.86f),cloth);Box("Pillow",P(x-.75f,y+.88f,z),P(.45f,.16f,.76f),ivory);}
 static void Shelf(float x,float z,float width){Cabinet(x,z,width,.7f,2.2f);for(int k=0;k<9;k++)for(int row=0;row<2;row++)Box("Shelved volume",P(x-width*.4f+k*width*.1f,y+.65f+row*.8f,z+.42f),P(width*.06f,.5f,.3f),k%3==0?cloth:k%3==1?ivory:gold);}
 static void Rack(float x,float z){Box("Practice rack base",P(x,y+.15f,z),P(2.6f,.3f,.85f),oak,true);foreach(float a in new[]{-1f,1f})Box("Rack upright",P(x+a*1.1f,y+1.25f,z),P(.16f,2.5f,.16f),oak);Box("Rack rail",P(x,y+1.9f,z),P(2.6f,.16f,.18f),oak);for(int k=-1;k<=1;k++){Box("Wooden practice haft",P(x+k*.65f,y+1.2f,z),P(.09f,2.1f,.09f),oak);Box("Practice guard",P(x+k*.65f,y+.75f,z),P(.45f,.1f,.12f),gold);}}
 static void Tools(float x,float z){Box("Work hammer haft",P(x,y+1.08f,z),P(1.1f,.12f,.12f),oak);Box("Work hammer head",P(x+.45f,y+1.17f,z),P(.3f,.3f,.5f),dark);}
 static void Lamp(float x,float z){Box("Wall lamp bracket",P(x,y+2.2f,z),P(.22f,.55f,.22f),gold);Ball("Warm lamp globe",P(x,y+2.55f,z),.35f,glass);var go=new GameObject("Room lamplight");go.transform.SetParent(floor,false);go.transform.localPosition=P(x,y+2.5f,z);var light=go.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.72f,.4f);light.intensity=1.8f;light.range=7;light.shadows=LightShadows.None;light.cullingMask=~(1<<31);lights++;}
 static void Directory(string title,string[] names,float w,float d)
 {
  // Keep interaction geometry out of the four-metre entrance and upper stair routes.
  bool narrow=w>9&&w<12;float x=w<9?-2.7f:narrow?2:w/2-1.3f,z=d/2-1.3f;var go=Shape("Room directory - "+title,PrimitiveType.Cube,P(x,y+1.45f,z),P(.8f,1.5f,.12f),oak);go.layer=11;go.AddComponent<BoxCollider>();
  var target=go.AddComponent<ConversationTarget>();string id="arborium.directory."+title.ToLowerInvariant().Replace(' ','-');target.Configure(id,id,title+"\n\n"+string.Join("\n",names.Select((n,i)=>(i+1)+". "+n))+(title.StartsWith("Northern")?"\n\nSpeak to the exchange banker at the reception counter for your shared bank.":title.StartsWith("Adventurers")?"\n\nUse the training target on floor 3 to practise attacks and spells. It awards no experience.":"\n\nThe stairs at the rear connect every floor."));target.ConfigureName(title+" directory");var approach=new GameObject("Directory approach").transform;approach.SetParent(floor,false);approach.localPosition=P(narrow?.7f:x,y,z-1.2f);target.ConfigureApproach(approach);boards++;
 }
 static void MoveBanker(float x,float z)
 {
  var banker=GameObject.Find("Arborium exchange banker");Need(banker!=null,"Existing banker missing");banker.transform.position=building.TransformPoint(P(x,.12f,z));var approach=banker.transform.Find("City service approach");Need(approach!=null,"Bank approach missing");approach.position=building.TransformPoint(P(x,.12f,z+2.8f));
 }
 static void Practice(float x,float z)
 {
  var def=Object.Instantiate(AssetDatabase.LoadAssetAtPath<ActorDefinition>("Assets/_DiceFree/Settings/WolfSurvival/Practice target.asset"));def.stableId="practice.arborium.guild";def.displayName="Guild practice target";def.experienceReward=0;AssetDatabase.CreateAsset(def,Folder+"/Guild practice target.asset");
  var go=new GameObject("Guild practice target - no rewards");go.layer=10;go.transform.SetParent(floor,false);go.transform.localPosition=P(x,y,z);var agent=go.AddComponent<NavMeshAgent>();agent.enabled=false;go.AddComponent<ActorStats>().Configure(def,1);go.AddComponent<Health>();go.AddComponent<TraversalMotor>();var actor=go.AddComponent<CombatActor>();actor.Configure(1,.5f);go.AddComponent<CombatStatusController>();go.AddComponent<ArboriumPracticeTarget>();var capsule=go.AddComponent<CapsuleCollider>();capsule.center=Vector3.up;capsule.height=2;capsule.radius=.5f;
  Box("Training target post",P(x,y+1,z),P(.22f,2,.22f),oak);Box("Training target shoulders",P(x,y+1.6f,z),P(1.3f,.22f,.22f),oak);Ball("Training target head",P(x,y+2.15f,z),.48f,ivory);Shape("Straw practice body",PrimitiveType.Cylinder,P(x,y+1.2f,z),P(.7f,.5f,.7f),gold);
 }
 static void SaveMeshes()
 {
  // Batch original furniture by palette, retaining independent collision and interaction objects.
  var filters=floor.GetComponentsInChildren<MeshFilter>().Where(f=>f.gameObject.layer==2).ToArray();
  foreach(var group in filters.GroupBy(f=>f.GetComponent<Renderer>().sharedMaterial))
  {
   var mesh=new Mesh{name=(serial++).ToString("D3")+" "+interior.BuildingName+" furniture",indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(group.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=floor.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray());AssetDatabase.CreateAsset(mesh,Folder+"/"+mesh.name+".asset");var go=new GameObject(group.Key.name+" furnishings");go.layer=2;go.transform.SetParent(floor,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=group.Key;
   foreach(var f in group)Object.DestroyImmediate(f.gameObject);
  }
  foreach(var renderer in floor.GetComponentsInChildren<Renderer>()){var fade=renderer.gameObject.AddComponent<TreeCameraOccluder>();fade.ConfigureFocusDistance(40);fade.ConfigureExteriorOnly(building,interior.Footprint);}
 }
 public static object RepairLandingClearance()
 {
  Need(!EditorApplication.isPlayingOrWillChangePlaymode&&SceneManager.GetActiveScene().name=="Cornberg"&&!SceneManager.GetActiveScene().isDirty,"Clean Cornberg Edit Mode required");int changed=0;
  foreach(var b in Object.FindObjectsByType<ArboriumInterior>())foreach(var room in b.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Furnished - ")).ToArray())
  {
   bool remove=b.BuildingName=="Artisan Workshop"&&room.name=="Furnished - Storeroom";
   bool rotate=(b.BuildingName=="Artisan Workshop"&&room.name=="Furnished - Workshop"||b.BuildingName=="Mage Observatory"&&room.name=="Furnished - Library")&&room.Find("Wall-side furniture")==null;
   if(!remove&&!rotate)continue;float oldZ=b.BuildingName=="Artisan Workshop"?-3.8f:-3.5f;var q=Quaternion.Euler(0,-90,0);Vector3 Turn(Vector3 p)=>q*(p-P(3.6f,0,oldZ))+P(4.75f,0,oldZ);
   foreach(var f in room.GetComponentsInChildren<MeshFilter>().Where(f=>f.gameObject.layer==2))
   {
    var mesh=f.sharedMesh;Need(AssetDatabase.GetAssetPath(mesh).StartsWith(Folder+"/"),"Only owned furnishing meshes");var vertices=mesh.vertices;
    if(remove){var triangles=mesh.triangles;mesh.triangles=Enumerable.Range(0,triangles.Length/3).Where(i=>(vertices[triangles[i*3]].z+vertices[triangles[i*3+1]].z+vertices[triangles[i*3+2]].z)/3>-2).SelectMany(i=>new[]{triangles[i*3],triangles[i*3+1],triangles[i*3+2]}).ToArray();}
    else if(vertices.Any(v=>v.z<-2&&v.x>2&&v.x<4.2f)){for(int i=0;i<vertices.Length;i++)if(vertices[i].z<-2&&vertices[i].x>2)vertices[i]=Turn(vertices[i]);mesh.vertices=vertices;}
    mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);changed++;
   }
   foreach(var c in room.GetComponentsInChildren<Collider>().Where(c=>c.transform.localPosition.z<-2).ToArray())if(remove)Object.DestroyImmediate(c.gameObject);else if(Quaternion.Angle(c.transform.localRotation,Quaternion.identity)<1){c.transform.localPosition=Turn(c.transform.localPosition);c.transform.localRotation=q;}
  }
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());Need(EditorSceneManager.SaveScene(SceneManager.GetActiveScene()),"Clearance save failed");CornbergSceneBuilder.Rebake();return new{changed};
 }
 public static object RepairDirectories()
 {
  Need(!EditorApplication.isPlayingOrWillChangePlaymode&&!SceneManager.GetActiveScene().isDirty,"Clean Edit Mode required");int count=0;
  foreach(var b in Object.FindObjectsByType<ArboriumInterior>().Where(b=>b.Footprint.x>9&&b.Footprint.x<12))
  {var board=b.GetComponentsInChildren<ConversationTarget>().Single(t=>t.NpcId.StartsWith("arborium.directory."));board.transform.localPosition=P(2,1.57f,b.Footprint.y/2-1.3f);var approach=board.transform.parent.Find("Directory approach");approach.localPosition=P(.7f,.12f,b.Footprint.y/2-2.5f);count++;}
  EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());Need(EditorSceneManager.SaveScene(SceneManager.GetActiveScene()),"Directory save failed");return new{count};
 }
}
