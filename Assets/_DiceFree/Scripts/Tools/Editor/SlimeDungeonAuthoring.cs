using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Dungeons;
using DiceFree.Combat;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Items;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
namespace DiceFree.EditorTools
{
    public static class SlimeDungeonAuthoring
    {
        const string Data="Assets/_DiceFree/Settings/Dungeons";
        const string ScenePath="Assets/_DiceFree/Scenes/SlimeDungeon.unity";
        const string RewardPath="Assets/_DiceFree/Scenes/DungeonRewardRoom.unity";
        const string Art="Assets/_DiceFree/Art/World/CornbergBatch/";
        static SlimeDungeonTuning tuning;
        static Material green,blue,bark,ground,gel;
        static readonly Rect[] rooms={new Rect(0,0,17,88),new Rect(0,64,26,24),new Rect(16,59,34,12),new Rect(25,31,30,30),new Rect(54,45,12,40),new Rect(54,64,29,24),new Rect(47,26,10,14),new Rect(39,5,27,24)};
        [CliCommand("dicefree.slime.author","Author Slime Dungeon and reward maps; incrementally add Cornberg portal.")]
        public static object Author()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before authoring.");
            if(EditorSceneManager.GetSceneManagerSetup().Any(s=>SceneManager.GetSceneByPath(s.path).isDirty))throw new InvalidOperationException("Unsaved scene changes: refusing to author until they are saved.");
            if(!AssetDatabase.IsValidFolder(Data))AssetDatabase.CreateFolder("Assets/_DiceFree/Settings","Dungeons");
            green=Mat("Dungeon Foliage Green",new Color(.2f,.48f,.15f));blue=Mat("Dungeon Foliage Blue",new Color(.13f,.43f,.88f));
            bark=Mat("Dungeon Bark",new Color(.26f,.16f,.09f));ground=Mat("Dungeon Grass",new Color(.24f,.34f,.18f));gel=Mat("Dungeon Gel",new Color(.22f,.77f,.24f,.65f));
            tuning=Asset<SlimeDungeonTuning>("Slime playtest tuning");
            var combat=AssetDatabase.LoadAssetAtPath<CombatTuning>("Assets/_DiceFree/Settings/Combat/Cornberg provisional tuning.asset");
            if(combat==null)combat=AssetDatabase.FindAssets("t:CombatTuning").Select(g=>AssetDatabase.LoadAssetAtPath<CombatTuning>(AssetDatabase.GUIDToAssetPath(g))).First();
            var bump=Attack("Slime bump",4,1.5f);tuning.slam=Attack("Slime Slam",18,6);tuning.regentSlam=Attack("Regent Slam",40,6);
            tuning.addAttack=Attack("Tiny Slime bump",3,1.2f);tuning.regentAddAttack=Attack("Regent add bump",7,1.2f);
            tuning.green=Actor("Green Slime",tuning.trashHp,bump,combat);tuning.blue=Actor("Blue puzzle Slime",tuning.puzzleHp*25,bump,combat);
            tuning.puzzleGreen=Actor("Green puzzle Slime",tuning.puzzleHp,bump,combat);
            tuning.miniboss=Actor("Big Slime",tuning.minibossHp,bump,combat);tuning.boss=Actor("Slime Boss",tuning.bossHp,bump,combat);tuning.regent=Actor("Slime Regent",tuning.regentHp,bump,combat);
            tuning.miniboss.stableId="boss.big-slime";tuning.boss.stableId="boss.slime";tuning.regent.stableId="boss.slime-regent";
            tuning.fragment=Actor("Divided Slime",20,bump,combat);tuning.add=Actor("Tiny Slime",tuning.addHp,tuning.addAttack,combat);tuning.regentAdd=Actor("Regent Tiny Slime",tuning.regentAddHp,tuning.regentAddAttack,combat);tuning.dummy=Actor("Practice target — no rewards",500,bump,combat);
            var template=new GameObject("Dungeon Slime template");template.layer=10;
            var agent=template.AddComponent<NavMeshAgent>();agent.enabled=false;agent.radius=.6f;agent.height=1.5f;agent.areaMask=1;
            template.AddComponent<TraversalMotor>();template.AddComponent<ActorStats>().Configure(tuning.green);template.AddComponent<Health>();template.AddComponent<CombatActor>().Configure(1,.65f,"enemy-family.slime");
            template.AddComponent<BasicAttack>();template.AddComponent<AggroBehaviour>().Configure(9,20);
            template.AddComponent<CombatStatusController>();
            var capsule=template.AddComponent<CapsuleCollider>();capsule.radius=.65f;capsule.height=1.2f;capsule.center=Vector3.up*.6f;
            var presentation=Model("Slimes/Slime_Crop",template.transform,Vector3.zero,1);presentation.name="Presentation";
            tuning.slimeTemplate=PrefabUtility.SaveAsPrefabAsset(template,Data+"/DungeonSlime.prefab");UnityEngine.Object.DestroyImmediate(template);
            var bossModel=Model("Slimes/Slime_Boss",null,Vector3.zero,1);tuning.bossPresentation=PrefabUtility.SaveAsPrefabAsset(bossModel,Data+"/SlimeBossPresentation.prefab");UnityEngine.Object.DestroyImmediate(bossModel);
            var regentModel=Model("Slimes/Slime_Regent",null,Vector3.zero,1);tuning.regentPresentation=PrefabUtility.SaveAsPrefabAsset(regentModel,Data+"/SlimeRegentPresentation.prefab");UnityEngine.Object.DestroyImmediate(regentModel);
            Rewards();EditorUtility.SetDirty(tuning);AssetDatabase.SaveAssets();
            BuildDungeon();BuildReward();InstallPortal();AssetDatabase.SaveAssets();
            return new {success=true,marker="SLIME_DUNGEON_AUTHORED",scene=ScenePath,rewardScene=RewardPath};
        }
        static void BuildDungeon()
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
            var root=new GameObject("Slime Dungeon — outdoor forest");var origin=SlimeDungeonRun.Origin;
            foreach(var r in rooms)Cube("Forest corridor",root.transform,origin+new Vector3(r.center.x,-.25f,r.center.y),new Vector3(r.width,.5f,r.height),ground,8);
            string[] types={"Oak","Pine","Spruce","Birch","Willow","Cypress","Twisted","Maple","Aspen"};int count=0;
            // Dense varied forest along the union of walkable corridors. Never encroach on another room.
            for(int x=-6;x<=90;x+=3)for(int z=-6;z<=96;z+=3)
            {
                if(Inside(x,z,1.7f)||!Inside(x,z,7))continue;
                bool southPuzzle=x>=24&&x<=46&&z>=24&&z<=30;
                var t=Model("Trees/Tree_"+types[count++%types.Length],root.transform,origin+new Vector3(x,0,z),1.0f+(count%4)*.12f,southPuzzle);
                var wall=t.AddComponent<CapsuleCollider>();wall.radius=1.6f;wall.height=7;wall.center=Vector3.up*3; t.layer=9;
            }
            Gate(root.transform,"Staging gate",origin+new Vector3(8,2,16),new Vector3(18,4,2));
            Gate(root.transform,"Miniboss gate",origin+new Vector3(23,2,65),new Vector3(2,4,15));
            Gate(root.transform,"Normal route gate",origin+new Vector3(55,2,54),new Vector3(2,4,20));
            Gate(root.transform,"Enchanted Regent barrier",origin+new Vector3(52,2,30),new Vector3(12,4,2));
            var puzzleObj=new GameObject("Five ring puzzle");puzzleObj.transform.SetParent(root.transform);puzzleObj.transform.position=origin+new Vector3(40,0,46);
            var puzzle=puzzleObj.AddComponent<SlimeTreePuzzle>();puzzle.centralTree=Model("Trees/Tree_Oak",puzzleObj.transform,origin+new Vector3(40,0,46),1.7f).transform;puzzle.blueFoliage=blue;
            puzzle.rings=new Transform[5];
            for(int i=0;i<5;i++)
            {
                float a=i*Mathf.PI*2/5;var obj=new GameObject("Green capture ring "+(i+1));obj.transform.SetParent(puzzleObj.transform);obj.transform.position=puzzleObj.transform.position+new Vector3(Mathf.Cos(a)*5,0,Mathf.Sin(a)*5);
                var line=obj.AddComponent<LineRenderer>();line.loop=true;line.useWorldSpace=false;line.positionCount=48;line.widthMultiplier=.15f;line.sharedMaterial=Mat("Green ring",new Color(.18f,1,.23f));
                for(int j=0;j<48;j++){float b=j*Mathf.PI*2/48;line.SetPosition(j,new Vector3(Mathf.Cos(b)*1.15f,.08f,Mathf.Sin(b)*1.15f));}puzzle.rings[i]=obj.transform;
            }
            Label(root.transform,origin+new Vector3(8,2,4),"Shared staging — 60 seconds\nPractice dummy / prepare loadout");
            Label(root.transform,origin+new Vector3(12,2,77),"Big Slime — mandatory");Label(root.transform,origin+new Vector3(40,3,46),"Lure slimes into five green rings");
            Light(root.transform);Bake(root,origin+new Vector3(40,0,44),new Vector3(110,30,120),"SlimeDungeonNavMesh");
            Save(scene,ScenePath);EditorSceneManager.CloseScene(scene,true);
        }
        static void BuildReward()
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
            var root=new GameObject("Reusable neutral reward room");Cube("Neutral floor",root.transform,SlimeDungeonRun.RewardOrigin+Vector3.down*.25f,new Vector3(24,.5f,24),Mat("Neutral reward stone",new Color(.42f,.43f,.46f)),8);
            var controller=root.AddComponent<DungeonRewardRoom>();controller.variantAnchor=new GameObject("Future dungeon visual variant").transform;controller.variantAnchor.SetParent(root.transform);controller.variantAnchor.position=SlimeDungeonRun.RewardOrigin;
            Label(root.transform,SlimeDungeonRun.RewardOrigin+new Vector3(0,3,5),"Choose one reward, or bonus EXP and gold\nThen return to the dungeon entrance");
            Light(root.transform);Bake(root,SlimeDungeonRun.RewardOrigin,new Vector3(30,15,30),"DungeonRewardRoomNavMesh");Save(scene,RewardPath);EditorSceneManager.CloseScene(scene,true);
        }
        static void InstallPortal()
        {
            var scene=EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath,OpenSceneMode.Single);
            // Strict incremental installation: no generator, no rebake, no deletion of authored objects.
            var existing=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="Slime Dungeon entrance");
            var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();if(player==null)throw new InvalidOperationException("Cornberg player missing.");
            var inventory=player.GetComponent<CarriedInventory>();
            inventory.Configure(inventory.Definitions.Concat(tuning.normalRewards.items).Concat(tuning.regentRewards.items).Concat(new[]{tuning.normalRewards.rare,tuning.regentRewards.rare}).Where(i=>i!=null).Distinct().ToArray());
            EditorUtility.SetDirty(inventory);
            if(existing!=null){EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);return;}
            var obj=new GameObject("Slime Dungeon entrance");obj.layer=11;
            var probe=new Vector3(46,0,20);var nav=UnityEngine.Object.FindFirstObjectByType<WorldNavigation>();
            // Portal destination is snapped against Cornberg's existing saved NavMesh (no geometry rebake).
            NavMeshDataInstance instance=default;var data=nav?.Data;if(data!=null)instance=NavMesh.AddNavMeshData(data);
            if(!NavMesh.SamplePosition(probe,out var hit,25,NavMesh.AllAreas)){if(instance.valid)instance.Remove();throw new InvalidOperationException("No Cornberg navigation near portal.");}
            obj.transform.position=hit.position;if(instance.valid)instance.Remove();
            var portal=obj.AddComponent<DungeonPortal>();portal.definition=AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/_DiceFree/Settings/World/Dungeon - Slime.asset");portal.tuning=tuning;portal.ConfigureName("Slime Dungeon — enter shared staging");
            var collider=obj.AddComponent<CapsuleCollider>();collider.height=2;collider.radius=.9f;collider.center=Vector3.up;
            Model("Trees/Tree_Twisted",obj.transform,obj.transform.position,1.2f);
            Label(obj.transform,obj.transform.position+Vector3.up*3,"Slime Dungeon\nRight-click / interact to enter");
            EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Cornberg incremental portal save failed.");
        }
        static void Rewards()
        {
            var orb=Item("Slime Orb",EquipmentSlot.OffHand,new AttributeValues{intelligence=3,spirit=3,vitality=2},0,3);orb.allowedClassIds=new[]{"class.magically-touched-novice"};orb.stats.basicAttackMaximumBonus=1;
            var shield=Item("Slime Shield",EquipmentSlot.OffHand,new AttributeValues{vitality=5,strength=2},5,3);shield.allowedClassIds=new[]{"class.physically-blessed-novice"};
            var gloves=Item("Slimy Farmer's Gloves",EquipmentSlot.Hands,new AttributeValues{agility=3},0,0);gloves.stats.attackSpeedPercent=5;
            var hat=Item("Slimy Tophat",EquipmentSlot.Head,new AttributeValues(1),5,5);hat.stats.regeneration=10;
            foreach(var item in new[]{orb,shield,gloves,hat})EditorUtility.SetDirty(item);
            tuning.normalRewards=Asset<DungeonRewardPool>("Normal Slime rewards");tuning.regentRewards=Asset<DungeonRewardPool>("Regent rewards");
            foreach(var pool in new[]{tuning.normalRewards,tuning.regentRewards}){pool.stableId=pool==tuning.normalRewards?"loot.slime":"loot.slime-regent";pool.items=new[]{orb,shield,gloves};pool.rare=hat;EditorUtility.SetDirty(pool);}
            // Regent exclusives are deliberately absent until their design is approved.
        }
        static ItemDefinition Item(string name,EquipmentSlot slot,AttributeValues attrs,float physical,float magical){var i=Asset<ItemDefinition>(name);i.stableId="item.slime."+name.ToLowerInvariant().Replace(" ","-").Replace("'","");i.displayName=name;i.slot=slot;i.sourceId="dungeon.slime";i.stats=new EquipmentStats{attributes=attrs,physicalDefense=physical,magicalDefense=magical};return i;}
        static ActorDefinition Actor(string name,float hp,AttackDefinition attack,CombatTuning combat){var d=Asset<ActorDefinition>(name);d.stableId="enemy.dungeon."+name.ToLowerInvariant().Replace(" ","-");d.displayName=name;d.familyId="enemy-family.slime";d.baseHp=hp;d.baseAttributes=new AttributeValues(0);d.growth=new AttributeValues(0);d.moveSpeed=3.2f;d.tuning=combat;d.basicAttack=attack;d.experienceReward=0;EditorUtility.SetDirty(d);return d;}
        static AttackDefinition Attack(string name,float damage,float interval){var a=Asset<AttackDefinition>(name);a.stableId="attack.dungeon."+name.ToLowerInvariant().Replace(" ","-");a.displayName=name;a.baseDamage=damage;a.coefficient=0;a.interval=interval;a.reach=.7f;EditorUtility.SetDirty(a);return a;}
        static T Asset<T>(string name)where T:ScriptableObject{var p=Data+"/"+name+".asset";var a=AssetDatabase.LoadAssetAtPath<T>(p);if(a==null){a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,p);}return a;}
        static Material Mat(string name,Color c){var p=Data+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.color=c;EditorUtility.SetDirty(m);return m;}
        static GameObject Model(string name,Transform parent,Vector3 p,float size,bool isBlue=false)
        {
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Art+name+".fbx");if(asset==null)throw new InvalidOperationException("Required existing model missing: "+name);
            var obj=(GameObject)PrefabUtility.InstantiatePrefab(asset);obj.transform.SetParent(parent);obj.transform.position=p;obj.transform.localScale=Vector3.one*size;
            foreach(var t in obj.GetComponentsInChildren<Transform>())t.gameObject.layer=2;
            foreach(var r in obj.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>ArtMaterial(m,isBlue)).ToArray();
            return obj;
        }
        static Material ArtMaterial(Material original,bool isBlue)
        {
            string key=original==null?"Default":original.name.Replace("DF_","").Replace(" (Instance)","").Replace("_mesh","");
            if(key.Contains("Foliage")||key.Contains("LeafAccent"))return isBlue?blue:green;
            var existing=AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Materials/CornbergBatch/"+key+"_0.mat");
            if(existing!=null)return existing;
            return Mat("Art "+key,key.Contains("Gold")||key.Contains("Crown")?new Color(1,.75f,.1f):key.Contains("Hat")?new Color(.08f,.06f,.09f):original==null?Color.gray:original.color);
        }
        static bool Inside(float x,float z,float margin)=>rooms.Any(r=>x>=r.xMin-margin&&x<=r.xMax+margin&&z>=r.yMin-margin&&z<=r.yMax+margin);
        static GameObject Cube(string name,Transform parent,Vector3 p,Vector3 size,Material material,int layer){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.layer=layer;g.transform.SetParent(parent);g.transform.position=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=material;return g;}
        static void Gate(Transform parent,string name,Vector3 p,Vector3 size)
        {
            var gate=new GameObject(name);gate.layer=11;gate.transform.SetParent(parent);gate.transform.position=p;
            gate.AddComponent<BoxCollider>().size=size;var obstacle=gate.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;obstacle.size=size;obstacle.carving=true;
            bool horizontal=size.x>size.z;int count=Mathf.CeilToInt(Mathf.Max(size.x,size.z)/3);
            for(int i=0;i<count;i++){float offset=(i-(count-1)*.5f)*2.8f;Model("Trees/Tree_Twisted",gate.transform,p-Vector3.up*2+(horizontal?Vector3.right:Vector3.forward)*offset,1.05f,name=="Enchanted Regent barrier");}
        }
        static void Light(Transform parent){var l=new GameObject("Outdoor illumination").AddComponent<Light>();l.transform.SetParent(parent);l.type=LightType.Directional;l.intensity=1.1f;l.transform.rotation=Quaternion.Euler(55,-35,0);}
        static void Label(Transform parent,Vector3 p,string text){var l=new GameObject("Dungeon sign").AddComponent<TextMesh>();l.transform.SetParent(parent);l.transform.position=p;l.transform.rotation=Quaternion.Euler(50,180,0);l.text=text;l.characterSize=.13f;l.fontSize=40;l.anchor=TextAnchor.MiddleCenter;}
        static void Bake(GameObject root,Vector3 center,Vector3 size,string name)
        {
            Physics.SyncTransforms();var sources=new List<NavMeshBuildSource>();var bounds=new Bounds(center,size);
            NavMeshBuilder.CollectSources(bounds,(1<<8)|(1<<9),NavMeshCollectGeometry.PhysicsColliders,0,new List<NavMeshBuildMarkup>(),sources);
            var settings=NavMesh.GetSettingsByID(0);settings.agentRadius=.5f;settings.agentHeight=1.8f;settings.agentClimb=.4f;
            var data=NavMeshBuilder.BuildNavMeshData(settings,sources,bounds,Vector3.zero,Quaternion.identity);if(data==null)throw new InvalidOperationException("Dungeon navigation bake failed");
            string path=Data+"/"+name+".asset";var saved=AssetDatabase.LoadAssetAtPath<NavMeshData>(path);if(saved==null){AssetDatabase.CreateAsset(data,path);saved=data;}else{EditorUtility.CopySerialized(data,saved);UnityEngine.Object.DestroyImmediate(data);}root.AddComponent<WorldNavigation>().Configure(saved);EditorUtility.SetDirty(saved);
        }
        static void Save(Scene scene,string path){if(!EditorSceneManager.SaveScene(scene,path))throw new InvalidOperationException("Scene save failed");var entries=EditorBuildSettings.scenes.ToList();if(!entries.Any(s=>s.path==path)){entries.Add(new EditorBuildSettingsScene(path,true));EditorBuildSettings.scenes=entries.ToArray();}}
    }
}
