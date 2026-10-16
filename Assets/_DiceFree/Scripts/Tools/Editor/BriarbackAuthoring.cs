using System;using System.IO;using System.Linq;using System.Collections.Generic;
using DiceFree.Combat;using DiceFree.AI;using DiceFree.World;using DiceFree.Quests;using DiceFree.Items;using DiceFree.Briarback;using DiceFree.Characters;using DiceFree.Core;using DiceFree.UI;using DiceFree.Advancement;
using UnityEditor;using UnityEditor.Animations;using UnityEditor.SceneManagement;using UnityEngine;using UnityEngine.AI;using Unity.Pipeline.Commands;using Object=UnityEngine.Object;
using static DiceFree.EditorTools.BlockoutShapes;
namespace DiceFree.EditorTools
{
 public static class BriarbackAuthoring
 {
  public const string RootName="14 - Briarback quilboar frontier",Folder="Assets/_DiceFree/Settings/Briarback",Art="Assets/_DiceFree/Art/Creatures/Briarback";
  public const string Scene="Assets/_DiceFree/Scenes/Cornberg.unity",QuestId="quest.briarback.northern-trail",EliteId="briarback.harrowtusk",Family="briarback.quilboar",WatchId="area.briarback.watchpost";
  public static readonly Vector2[] Route={new(248,201),new(248,225),new(248,243),new(250,263),new(245,284),new(232,304),new(241,324),new(261,344),new(251,364),new(228,378),new(206,378)};
  public static readonly Vector2[] EliteRoute={new(241,324),new(264,320),new(288,317),new(313,324)};
  public static Rect Bounds=>new(-85,-70,438,476);
  static readonly Color[] Skin={new(.40f,.25f,.15f),new(.30f,.35f,.29f),new(.37f,.25f,.43f),new(.46f,.18f,.10f)};
  static Material earth,grass,leaf,wood,stone,bone,dark;
  static Vector3 G(float x,float z)=>CornbergLandscape.Ground(x,z);
  static Vector3 G(Vector2 p)=>G(p.x,p.y);
  static T Asset<T>(string path)where T:ScriptableObject{var value=AssetDatabase.LoadAssetAtPath<T>(path);if(value!=null)return value;value=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(value,path);return value;}
  static Material Mat(string name,Color color){
   string path=Art+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};AssetDatabase.CreateAsset(m,path);}
   m.color=color;m.SetFloat("_Smoothness",.12f);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
  }
  static Mesh MeshAsset(string name,Vector3[] vertices,int[] triangles){
   string path=Folder+"/"+name+".asset";var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);
   if(m==null){m=new Mesh{name=name};AssetDatabase.CreateAsset(m,path);}
   m.Clear();m.vertices=vertices;m.triangles=triangles;m.RecalculateNormals();m.RecalculateBounds();EditorUtility.SetDirty(m);return m;
  }
  static void Terrain(Transform root){
   const int nx=78,nz=76;var v=new Vector3[(nx+1)*(nz+1)];var t=new int[nx*nz*6];
   for(int z=0;z<=nz;z++)for(int x=0;x<=nx;x++)v[z*(nx+1)+x]=G(190+x*2,246+z*2)+Vector3.up*.006f;
   int n=0;for(int z=0;z<nz;z++)for(int x=0;x<nx;x++){int i=z*(nx+1)+x;t[n++]=i;t[n++]=i+nx+1;t[n++]=i+1;t[n++]=i+1;t[n++]=i+nx+1;t[n++]=i+nx+2;}
   var ground=MeshAsset("Briarback ground",v,t);ground.SetPreBakeCollisionMesh(false,true);
   MeshObject("Continuous Briarback walkable terrain",ground,grass,root,true);
  }
  static void Trail(Transform root,string name,Vector2[] points,float width){
   var vertices=new List<Vector3>();var triangles=new List<int>();
   for(int s=1;s<points.Length;s++){
    var a=points[s-1];var b=points[s];var direction=(b-a).normalized;var side=new Vector2(-direction.y,direction.x)*width*.5f;
    int count=Mathf.CeilToInt(Vector2.Distance(a,b)/2);
    for(int k=0;k<count;k++){
     Vector2 p=Vector2.Lerp(a,b,k/(float)count),q=Vector2.Lerp(a,b,(k+1)/(float)count);
     int i=vertices.Count;vertices.Add(G(p-side)+Vector3.up*.045f);vertices.Add(G(p+side)+Vector3.up*.045f);vertices.Add(G(q-side)+Vector3.up*.045f);vertices.Add(G(q+side)+Vector3.up*.045f);
     triangles.AddRange(new[]{i,i+1,i+2,i+2,i+1,i+3});
    }
   }
   MeshObject(name,MeshAsset(name,vertices.ToArray(),triangles.ToArray()),earth,root);
  }
  static float Distance(Vector2 p,Vector2[] route){
   float d=float.MaxValue;for(int i=1;i<route.Length;i++){var v=route[i]-route[i-1];var q=route[i-1]+v*Mathf.Clamp01(Vector2.Dot(p-route[i-1],v)/v.sqrMagnitude);d=Mathf.Min(d,Vector2.Distance(p,q));}return d;
  }
  static Mesh Cone(){
   var v=new Vector3[10];v[0]=Vector3.up;for(int i=0;i<8;i++)v[i+1]=new Vector3(Mathf.Cos(i*Mathf.PI/4)*.5f,0,Mathf.Sin(i*Mathf.PI/4)*.5f);v[9]=Vector3.zero;
   var t=new List<int>();for(int i=0;i<8;i++){int a=i+1,b=(i+1)%8+1;t.AddRange(new[]{0,b,a,9,a,b});}
   return MeshAsset("Original eight-sided thorn cone",v,t.ToArray());
  }
  static void ConeAt(string name,Transform parent,Vector3 position,Vector3 scale,Material material,Mesh mesh){
   var go=MeshObject(name,mesh,material,parent);go.transform.position=position;go.transform.localScale=scale;
  }
  static void Scenery(Transform root,Mesh cone){
   var r=new System.Random(190316);var group=Group("Briarback thornwood, rocks and camp clearings",root);
   Vector2[] centers={new(270,270),new(210,309),new(279,348),new(289,312),new(316,324)};
   for(int i=0;i<135;i++){
    var p=new Vector2(196+(float)r.NextDouble()*144,253+(float)r.NextDouble()*138);
    if(Distance(p,Route)<8||Distance(p,EliteRoute)<7||centers.Any(c=>Vector2.Distance(p,c)<12))continue;
    float h=4+(float)r.NextDouble()*3;
    var tree=Group("Woodland tree",group);
    Shape("Trunk",PrimitiveType.Cylinder,G(p)+Vector3.up*h*.30f,new Vector3(.7f,h*.30f,.7f),wood,tree);
    ConeAt("Canopy",tree,G(p)+Vector3.up*h*.48f,new Vector3(h*.8f,h*.7f,h*.8f),leaf,cone);
    if(i%3==0)Shape("Grounded mossy rock",PrimitiveType.Sphere,G(p+new Vector2(2,1))+Vector3.up*.55f,new Vector3(2,1.4f,1.7f),stone,group);
   }
   for(int i=0;i<30;i++){
    float z=251+i*4.9f;foreach(float x in new[]{191f,345f}){
     var rock=Shape("Wooded cliff boundary",PrimitiveType.Sphere,G(x,z)+Vector3.up*2,new Vector3(5,5,6),stone,group);
     rock.transform.rotation=Quaternion.Euler(0,i*31,0);
    }
   }
   for(int i=0;i<31;i++)Shape("Northern wooded ridge",PrimitiveType.Sphere,G(191+i*5,397)+Vector3.up*2,new Vector3(6,5,5),stone,group);
  }
  static void Camp(Transform root,Vector2 p,string name,Mesh cone){
   var camp=Group(name,root);
   for(int i=0;i<3;i++){
    float a=i*2.1f;var at=p+new Vector2(Mathf.Cos(a)*7,Mathf.Sin(a)*7);
    Box("Solid hide-shelter footprint",G(at)+Vector3.up*.45f,new Vector3(2.8f,.9f,2.8f),wood,camp);
    ConeAt("Quilboar hide shelter",camp,G(at),new Vector3(4,3,4),wood,cone);
   }
   Shape("Stone-ring campfire",PrimitiveType.Cylinder,G(p)+Vector3.up*.1f,new Vector3(2.1f,.12f,2.1f),dark,camp,false);
   ConeAt("Amber campfire",camp,G(p)+Vector3.up*.12f,new Vector3(.7f,1.2f,.7f),bone,cone);
   for(int i=0;i<5;i++){
    var at=p+new Vector2(5+i*.7f,-5);
    Shape("Quilboar camp stockade",PrimitiveType.Cylinder,G(at)+Vector3.up,new Vector3(.28f,1,.28f),wood,camp);
    ConeAt("Stockade thorn",camp,G(at)+Vector3.up*2,new Vector3(.3f,.5f,.3f),bone,cone);
   }
  }
  static AnimatorController PrepareModel(){
   AssetDatabase.ImportAsset(Art+"/Models/Quilboar.fbx",ImportAssetOptions.ForceSynchronousImport);
   var importer=(ModelImporter)AssetImporter.GetAtPath(Art+"/Models/Quilboar.fbx");
   importer.animationType=ModelImporterAnimationType.Generic;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;importer.importAnimation=true;importer.addCollider=false;importer.importCameras=false;importer.importLights=false;importer.preserveHierarchy=true;
   var clips=importer.defaultClipAnimations;foreach(var clip in clips){clip.loopTime=clip.name.Contains("Idle")||clip.name.Contains("Locomotion");clip.loopPose=clip.loopTime;}importer.clipAnimations=clips;importer.SaveAndReimport();
   string path=Art+"/Quilboar.controller";var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path)??AnimatorController.CreateAnimatorControllerAtPath(path);
   var motions=AssetDatabase.LoadAllAssetsAtPath(Art+"/Models/Quilboar.fbx").OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__")).ToArray();
   foreach(string name in new[]{"Idle","Locomotion","Attack","Slam","Death"}){
    var clip=motions.FirstOrDefault(c=>c.name.EndsWith("|"+name)||c.name==name);if(clip==null)throw new InvalidOperationException("Missing original creature animation "+name+"; "+string.Join(",",motions.Select(c=>c.name)));
    var machine=controller.layers[0].stateMachine;var state=machine.states.Select(s=>s.state).FirstOrDefault(s=>s.name==name)??machine.AddState(name);state.motion=clip;if(name=="Idle")machine.defaultState=state;
   }
   EditorUtility.SetDirty(controller);return controller;
  }
  static void Weapon(Transform model,QuilboarRole role,Mesh cone){
   var hand=model.GetComponentsInChildren<Transform>(true).First(t=>t.name=="RightHand");var group=Group("Quilboar "+role+" weapon",model);
   Vector3 p=hand.position;float length=role==QuilboarRole.Raider?.7f:1.3f;
   var shaft=Shape("Wooden weapon shaft",PrimitiveType.Cylinder,p+model.up*(length*.22f),new Vector3(.07f,length*.5f,.07f),wood,group,false);
   if(role==QuilboarRole.Harrowtusk)Box("Harrowtusk oversized stone hammer",p+model.up*.9f,new Vector3(.75f,.42f,.42f),stone,group,false);
   else if(role==QuilboarRole.Totemcaller){
    Shape("Nature focus",PrimitiveType.Sphere,p+model.up*.95f,Vector3.one*.26f,leaf,group,false);
    ConeAt("Bone staff crown",group,p+model.up*1.1f,new Vector3(.18f,.35f,.18f),bone,cone);
   }else if(role==QuilboarRole.SpearThrower)ConeAt("Bone javelin point",group,p+model.up*.88f,new Vector3(.16f,.35f,.16f),bone,cone);
   else Shape("Heavy raider club",PrimitiveType.Sphere,p+model.up*.55f,new Vector3(.30f,.40f,.28f),bone,group,false);
   group.SetParent(hand,true);
  }
  static ActorDefinition Definition(CombatTuning tuning,QuilboarRole role,int level){
   string key=role+"-L"+level;var a=Asset<AttackDefinition>(Folder+"/"+key+" attack.asset");
   a.stableId="attack.briarback."+key.ToLowerInvariant();a.displayName=role==QuilboarRole.SpearThrower?"Bone javelin":role==QuilboarRole.Totemcaller?"Briar bolt":"Heavy club";
   a.baseDamage=role==QuilboarRole.Harrowtusk?75:36+(level-21)*2;a.coefficient=0;a.weights=default;a.minimumDamageMultiplier=.85f;a.maximumDamageMultiplier=1.15f;
   a.channel=role==QuilboarRole.Totemcaller?DamageChannel.Magical:DamageChannel.Physical;a.element=role==QuilboarRole.Totemcaller?AssetDatabase.LoadAssetAtPath<ElementDefinition>(MagicalTouchedAuthoring.NaturePath):null;
   a.requiresAccuracy=true;a.interval=role==QuilboarRole.Harrowtusk?2.3f:2.1f;a.windup=.65f;a.reach=role==QuilboarRole.SpearThrower?9:role==QuilboarRole.Totemcaller?10:1.1f;
   var d=Asset<ActorDefinition>(Folder+"/"+key+" stats.asset");d.stableId=role==QuilboarRole.Harrowtusk?EliteId:"briarback."+role.ToString().ToLowerInvariant();d.familyId=Family;
   d.displayName=role==QuilboarRole.Harrowtusk?"Harrowtusk":role==QuilboarRole.Totemcaller?"Quilboar Totemcaller":role==QuilboarRole.SpearThrower?"Quilboar Spear Thrower":"Quilboar Raider";
   d.tags=new[]{"briarback","quilboar"};d.experienceReward=role==QuilboarRole.Harrowtusk?900:210+(level-21)*12;d.tuning=tuning;d.baseAttributes=new AttributeValues{vitality=5,strength=5,intelligence=5};d.growth=default;
   d.baseHp=role==QuilboarRole.Harrowtusk?5200:1150+(level-21)*110;d.overrideVitalityRegeneration=true;d.regenerationPerVitality=0;
   d.moveSpeed=role==QuilboarRole.Harrowtusk?3.5f:4;d.physicalDefense=role==QuilboarRole.Harrowtusk?22:10;d.magicalDefense=role==QuilboarRole.Harrowtusk?18:8;d.basicAttack=a;
   EditorUtility.SetDirty(a);EditorUtility.SetDirty(d);return d;
  }
  static GameObject Enemy(Transform root,GameObject template,Vector2 at,QuilboarRole role,int level,AnimatorController controller,Mesh cone,int number){
   var go=Object.Instantiate(template,G(at),Quaternion.identity,root);
   if(PrefabUtility.IsPartOfPrefabInstance(go))PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
   go.name=role+" "+number+" (Lv "+level+")";
   foreach(Transform child in go.transform.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
   foreach(var c in go.GetComponents<MonoBehaviour>())if(c is not ActorStats&&c is not Health&&c is not CombatActor&&c is not TraversalMotor&&c is not BasicAttack&&c is not AggroBehaviour&&c is not CombatStatusController&&c is not DefeatReporter)Object.DestroyImmediate(c);
   go.GetComponent<ActorStats>().Configure(Definition(template.GetComponent<ActorStats>().Definition.tuning,role,level),level);
   go.GetComponent<CombatActor>().Configure(1,role==QuilboarRole.Harrowtusk?.85f:.50f);
   go.GetComponent<AggroBehaviour>().Configure(12,role==QuilboarRole.Harrowtusk?30:26);go.GetComponent<AggroBehaviour>().ConfigureAutoAggro(new AutoAggroPolicy{alwaysAutoAggro=true});
   go.GetComponent<CombatStatusController>().ConfigureControl(role==QuilboarRole.Harrowtusk?.5f:0,false);
   var respawn=Asset<RespawnDefinition>(Folder+"/"+(role==QuilboarRole.Harrowtusk?"Elite":"Warband")+" respawn.asset");respawn.delaySeconds=role==QuilboarRole.Harrowtusk?180:65;EditorUtility.SetDirty(respawn);go.AddComponent<OverworldRespawn>().Configure(respawn);
   var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Art+"/Models/Quilboar.fbx"),go.transform);
   model.name="Quilboar original animated model";model.transform.localPosition=Vector3.zero;model.transform.localScale=Vector3.one*(role==QuilboarRole.Harrowtusk?1.5f:1);
   var toes=model.GetComponentsInChildren<Transform>().First(t=>t.name=="LeftToes");var foot=model.GetComponentsInChildren<Transform>().First(t=>t.name=="LeftFoot");if((toes.position-foot.position).z<0)model.transform.localRotation=Quaternion.Euler(0,180,0);
   var animator=model.GetComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
   foreach(var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>()){
    renderer.updateWhenOffscreen=true;renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>{
     string name=m.name;Color color=name.Contains("Hide")?Skin[(int)role]:name.Contains("Bone")?new Color(.83f,.73f,.49f):name.Contains("Eye")?new Color(.95f,.43f,.05f):name.Contains("Leather")?new Color(.22f,.15f,.08f):new Color(.12f,.07f,.04f);
     return Mat(name+(name.Contains("Hide")?" "+role:""),color);
    }).ToArray();
   }
   Weapon(model.transform,role,cone);
   var ai=go.AddComponent<QuilboarCombat>();ai.role=role;
   if(role==QuilboarRole.Harrowtusk){
    var slam=Asset<AttackDefinition>(Folder+"/Harrowtusk slam.asset");slam.stableId="attack.briarback.harrowtusk.slam";slam.displayName="Bristlebreaker Slam";slam.baseDamage=160;slam.coefficient=0;slam.requiresAccuracy=false;slam.reach=4;EditorUtility.SetDirty(slam);ai.slam=slam;
   }
   var p=go.AddComponent<QuilboarPresentation>();p.model=animator;
   var agent=go.GetComponent<NavMeshAgent>();agent.enabled=false;agent.radius=role==QuilboarRole.Harrowtusk?.85f:.5f;agent.height=role==QuilboarRole.Harrowtusk?2.9f:2;
   var collider=go.GetComponent<CapsuleCollider>();collider.radius=agent.radius;collider.height=agent.height;collider.center=Vector3.up*agent.height*.5f;
   return go;
  }
  static ItemDefinition Reward(){
   var item=Asset<ItemDefinition>(Folder+"/Bristlehide Mantle.asset");item.stableId=BriarbackMantleVisual.ItemId;item.displayName="Bristlehide Mantle";item.slot=EquipmentSlot.Back;item.itemLevel=25;item.rarityId="rarity.uncommon";item.sourceId=QuestId;item.bound=true;item.canSell=false;item.allowedClassIds=Array.Empty<string>();
   item.flavor="A sturdy trail mantle, reinforced with the warband's blunt bone quills.";item.stats=new EquipmentStats{physicalDefense=3,magicalDefense=2,attributes=new AttributeValues{vitality=3}};EditorUtility.SetDirty(item);return item;
  }
  [CliCommand("dicefree.briarback.author","Build the connected northern quilboar region, warband, elite and quest reward.")]
  public static object Run(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode only");
   var setup=EditorSceneManager.GetSceneManagerSetup();
   try{
    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    earth=Mat("Briarback worn trail",new Color(.46f,.37f,.24f));grass=Mat("Briarback dry woodland",new Color(.37f,.43f,.23f));leaf=Mat("Briarback olive canopy",new Color(.30f,.38f,.17f));wood=Mat("Briarback weathered timber",new Color(.30f,.20f,.11f));stone=Mat("Briarback ridge rock",new Color(.42f,.44f,.35f));bone=Mat("Briarback pale bone",new Color(.82f,.69f,.42f));dark=Mat("Briarback charcoal",new Color(.12f,.10f,.07f));
    var controller=PrepareModel();var cone=Cone();var item=Reward();
    var reward=Asset<FixedRewardDefinition>(Folder+"/Northern trail reward.asset");reward.gold=150;reward.items=new[]{item};EditorUtility.SetDirty(reward);
    var quest=Asset<QuestDefinition>(Folder+"/The Northern Trail.asset");quest.stableId=QuestId;quest.title="The Northern Trail";quest.requiredLevel=1;quest.requiredClassId="";quest.rewardXp=1800;quest.reward=reward;quest.completeOnObjectives=false;quest.repeatable=false;
    quest.offer="The western highway is ruined, and the northern trail is our only way toward the Big City. Check the old watchpost, drive back the quilboar warband and defeat their camp chief, Harrowtusk. Bring your report back here.";
    quest.activeDialogue="Scout the old watchpost north of Trimery, defeat eight quilboars, then find Harrowtusk in the eastern stockade. Return to Scout Runa at the north gate.";
    quest.locationHint="Trimery north gate → old watchpost → Briarback camps → Harrowtusk's eastern stockade → Scout Runa.";
    quest.stages=new[]{new QuestObjective{kind=ObjectiveKind.ReachArea,objectiveId="scout-watchpost",areaId=WatchId,count=1,instruction="Scout the old watchpost north of Trimery"},
     new QuestObjective{kind=ObjectiveKind.Kill,objectiveId="thin-warband",familyId=Family,count=8,instruction="Defeat 8 quilboars in Briarback"},
     new QuestObjective{kind=ObjectiveKind.Kill,objectiveId="defeat-harrowtusk",contentId=EliteId,count=1,instruction="Defeat Harrowtusk in the eastern stockade, then return to Scout Runa"}};
    EditorUtility.SetDirty(quest);AssetDatabase.SaveAssets();
    var scene=EditorSceneManager.OpenScene(Scene);var previous=GameObject.Find(RootName);if(previous!=null)Object.DestroyImmediate(previous);
    var root=Group(RootName,GameObject.Find("Cornberg - World 1 starter pocket").transform);
    OctoberPlaytestRepairs.RepairOuterNavigationEdges(scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray());
    Terrain(root);Trail(root,"Briarback northern detour",Route,5);Trail(root,"Harrowtusk eastern camp trail",EliteRoute,4);
    Trail(root,"Briarback western scouting loop",new[]{new Vector2(250,263),new Vector2(220,270),new Vector2(210,296),new Vector2(232,304)},3);
    Scenery(root,cone);Camp(root,new Vector2(270,270),"Southern raider camp",cone);Camp(root,new Vector2(210,309),"Western spear camp",cone);Camp(root,new Vector2(279,348),"Upper totem camp",cone);Camp(root,new Vector2(289,312),"Eastern warband stockade",cone);Camp(root,new Vector2(316,324),"Harrowtusk's stockade",cone);
    var watch=Group("Old north-trail watchpost",root);watch.position=G(250,263);watch.gameObject.AddComponent<ReachArea>().Configure(WatchId,6);
    Shape("Old watchpost foundation",PrimitiveType.Cylinder,G(254,265)+Vector3.up*.2f,new Vector3(3,.2f,3),stone,root);
    Box("Broken lookout post",G(254,265)+Vector3.up*1.3f,new Vector3(.5f,2.6f,.5f),wood,root);
    Label("BRIARBACK — QUILBOAR TERRITORY\nDanger: levels 21–30",G(248,246)+Vector3.up*4,root);
    Label("Western ridge trail — toward the Big City",G(206,378)+Vector3.up*3,root);
    var template=Object.FindObjectsByType<CombatActor>().First(a=>a.GetComponent<ActorStats>().Definition.stableId=="enemy.road-slime").gameObject;
    int count=0;Vector2[] camps={new(270,270),new(210,309),new(279,348),new(289,312)};
    for(int c=0;c<4;c++)for(int i=0;i<3;i++)Enemy(root,template,camps[c]+new Vector2((i-1)*3,i%2==0?-2:2),(QuilboarRole)i,21+c*2,controller,cone,++count);
    Enemy(root,template,new Vector2(316,324),QuilboarRole.Harrowtusk,30,controller,cone,++count);
    var npcSource=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).First(t=>t.name=="NPCs/NPC_Guard - 3D");
    var npc=Object.Instantiate(npcSource.gameObject,root);npc.name="Scout Runa - northern trail";npc.transform.position=G(242,201);npc.transform.rotation=Quaternion.Euler(0,150,0);
    foreach(var interaction in npc.GetComponentsInChildren<InteractionTarget>(true))Object.DestroyImmediate(interaction);
    var giver=npc.AddComponent<QuestGiver>();giver.Configure(quest);giver.ConfigureName("Scout Runa");
    var approach=new GameObject("Roadside conversation point").transform;approach.SetParent(npc.transform);approach.position=npc.transform.position+Vector3.back*2;giver.ConfigureApproach(approach);
    var player=Object.FindObjectsByType<TraversalInput>().Single();var camera=Object.FindAnyObjectByType<ExplorationCamera>();var fog=Object.FindAnyObjectByType<WorldFogOfWar>();
    root.gameObject.AddComponent<OverworldMapBounds>().Configure(Bounds);var driver=root.gameObject.AddComponent<OverworldBoundsPresentation>();driver.player=player;driver.exploration=camera;driver.fog=fog;
    fog.Configure(player.transform,camera.GetComponent<Camera>(),new Vector2(Bounds.xMin,Bounds.yMin),new Vector2(Bounds.xMax,Bounds.yMax),fog.VisionRadius);EditorUtility.SetDirty(fog);
    foreach(string path in new[]{Scene,"Assets/_DiceFree/Scenes/DevelopmentRoom.unity"}){
     if(path!=Scene)scene=EditorSceneManager.OpenScene(path);
     foreach(var p in Object.FindObjectsByType<DiceFree.Persistence.ManifestationPersistence>(FindObjectsInactive.Include)){
      var inv=p.GetComponent<CarriedInventory>();inv.Configure(inv.Definitions.Where(d=>d.stableId!=item.stableId).Append(item).ToArray());EditorUtility.SetDirty(inv);
      var journal=p.GetComponent<QuestJournal>();journal.Configure(journal.Definitions.Where(q=>q.stableId!=QuestId).Append(quest).ToArray());EditorUtility.SetDirty(journal);
      if(p.GetComponent<BriarbackMantleVisual>()==null)p.gameObject.AddComponent<BriarbackMantleVisual>();
     }
     EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    var catalog=Resources.Load<MenuPresentationCatalog>("MenuPresentationCatalog");catalog.items=catalog.items.Where(d=>d.stableId!=item.stableId).Append(item).ToArray();EditorUtility.SetDirty(catalog);
    AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(Scene);WorldEnvironmentAuthoring.InstallScene(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());CornbergSceneBuilder.Rebake();
    return new{marker="DICEFREE_BRIARBACK_AUTHORED",enemies=count,camps=5,quest=QuestId,reward=item.stableId};
   }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
  }
 }
}
