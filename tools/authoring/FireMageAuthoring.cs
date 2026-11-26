using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Skills;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

public static class FireMageAuthoring
{
    public const string Data="Assets/_DiceFree/Settings/FireMage/";
    private static ActorDefinition actor;
    private static ClassResourceProfile mana,heat;
    private static AttackDefinition fire;
    private static QuestDefinition quest;
    private static AdvancementDefinition edge;
    private static GameObject form;
    private static Texture2D[] icons;
    public static void Batch()
    {
        Directory.CreateDirectory(Data);Directory.CreateDirectory("Logs");AssetDatabase.Refresh();
        const string particles="Assets/_DiceFree/Resources/AuraVfx/EmberFlame.mat";
        if(AssetDatabase.LoadAssetAtPath<Material>(particles)==null)AssetDatabase.CreateAsset(new Material(Shader.Find("DiceFree/Ember Flame")),particles);
        DataAssets();Form();Menu();Equipment();PlayerScene("Cornberg");PlayerScene("DevelopmentRoom");Camp();Trial();
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");Debug.Log("DICEFREE_FIRE_MAGE_AUTHOR_OK");
    }
    private static T Load<T>(string path)where T:Object=>AssetDatabase.LoadAssetAtPath<T>(path)??throw new InvalidOperationException("Missing "+path);
    private static T Asset<T>(string name)where T:ScriptableObject
    {string path=Data+name+".asset";var value=AssetDatabase.LoadAssetAtPath<T>(path);if(value==null){value=ScriptableObject.CreateInstance<T>();value.name=name;AssetDatabase.CreateAsset(value,path);}return value;}
    private static T Add<T>(Component c)where T:Component=>c.GetComponent<T>()??c.gameObject.AddComponent<T>();
    private static void DataAssets()
    {
        var parent=Load<ActorDefinition>("Assets/_DiceFree/Settings/Skills/Mage/Mage class.asset");
        var basic=Asset<AttackDefinition>("Fire Mage basic");EditorUtility.CopySerialized(parent.basicAttack,basic);basic.stableId="attack.fire-mage.basic";basic.displayName="Ember Wand";basic.coefficient=1;
        actor=Asset<ActorDefinition>("Fire Mage class");EditorUtility.CopySerialized(parent,actor);actor.stableId=FireMageProgression.Id;actor.displayName="Fire Mage";actor.basicAttack=basic;
        actor.baseAttributes=new AttributeValues{vitality=19,strength=7,agility=12,intelligence=33,spirit=13};actor.growth=new AttributeValues{vitality=2,strength=.75f,agility=1,intelligence=6,spirit=1.5f};
        actor.baseHp=10;actor.overrideVitalityHp=true;actor.hpPerVitality=15;
        mana=Asset<ClassResourceProfile>("Fire Mage Mana");var parentMana=Load<ClassResourceProfile>("Assets/_DiceFree/Settings/Skills/Mage/Mage Mana.asset");EditorUtility.CopySerialized(parentMana,mana);
        mana.stableId="resource-profile.fire-mage.mana";mana.classId=actor.stableId;mana.baseMaximum=160;mana.maximumPerIntelligence=5;mana.maximumPerSpirit=0;mana.baseRegenPerSecond=4;mana.regenPerIntelligence=0;mana.regenPerSpirit=.1f;
        var resource=Asset<ResourceDefinition>("Heat");resource.stableId=FireMageCaster.HeatId;resource.displayName="Heat";
        heat=Asset<ClassResourceProfile>("Fire Mage Heat");heat.stableId="resource-profile.fire-mage.heat";heat.classId=actor.stableId;heat.resource=resource;heat.baseMaximum=100;
        heat.baseRegenPerSecond=heat.regenPerSpirit=heat.regenPerIntelligence=heat.maximumPerIntelligence=heat.maximumPerSpirit=0;heat.initialFraction=0;heat.loadPolicy=ResourceRetentionPolicy.ResetOnLoad;
        fire=Asset<AttackDefinition>("Fire spell packet");fire.stableId="attack.fire-mage.fire";fire.displayName="Fire magic";fire.channel=DamageChannel.Magical;fire.element=Load<ElementDefinition>("Assets/_DiceFree/Settings/Combat/Elements/Fire.asset");fire.baseDamage=0;fire.coefficient=1;fire.weights=new AttributeValues{intelligence=1};fire.scaling=AttributeScaling.Weighted;fire.requiresAccuracy=false;
        quest=Asset<QuestDefinition>("The Ember Walk");quest.stableId=FireMageMentor.QuestId;quest.title="The Ember Walk";quest.requiredClassId="class.mage";quest.requiredLevel=60;
        quest.offer="A flame rewards attention, not haste. Walk the marked stone trail through my Ember Walk and reach the far beacon alive. Amber tiles warn before fire erupts; molten ground is never safe. Your spells will be sealed, but walking and healing potions remain yours. If you fall, you may try again from the entrance. Return to me when you reach the beacon.";
        quest.activeDialogue="Follow the pale path. Watch the rhythm of the fire. You need no spell to understand it. Spent potions stay spent, even when the trial lets you begin again.";
        quest.turnInDialogue="You listened to the fire without letting it command you. Now carry that patience into every spell you cast.";
        quest.locationHint="Maelis Cinderweave's safe training camp at the base of Ember Crown, southeast Ashfall Reach, World 2.";quest.turnInHint="Return to Maelis Cinderweave at Ember Crown's base camp.";
        quest.stages=new[]{new QuestObjective{kind=ObjectiveKind.Escort,contentId=FireEscapeSite.TrialId,objectiveId="ember-walk-exit",count=1,instruction="Reach the far beacon alive in the solo Ember Walk; return to Maelis"}};
        edge=Asset<AdvancementDefinition>("Fire Mage advancement");edge.stableId="advance.mage.fire-mage";edge.displayName="Fire Mage";edge.sourceClass=parent;edge.targetClass=actor;edge.requiredLevel=60;edge.requiredQuestId=quest.stableId;
        icons=Enumerable.Range(0,6).Select(Icon).ToArray();
        foreach(string path in new[]{"Assets/_DiceFree/Settings/Progression/Class catalog.asset","Assets/_DiceFree/Settings/Development/Priest/Development Priest catalog.asset"}){var catalog=Load<ClassCatalog>(path);catalog.Configure(catalog.Classes.Where(c=>c.stableId!=actor.stableId).Append(actor).ToArray());EditorUtility.SetDirty(catalog);}
        foreach(var value in new Object[]{basic,actor,mana,heat,resource,fire,quest,edge})EditorUtility.SetDirty(value);
    }
    private static void Form()
    {
        var root=new GameObject("Cinderweave Fire Mage");
        try
        {
            var model=(GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>("Assets/_DiceFree/Art/Characters/Dawnwarden/Dawnwarden.fbx"),root.transform);
            var animator=model.GetComponent<Animator>();
            animator.runtimeAnimatorController=Load<RuntimeAnimatorController>("Assets/_DiceFree/Resources/PlayerMotion/TierTwo/Priest.controller");animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                skin.updateWhenOffscreen=true;skin.sharedMaterials=skin.sharedMaterials.Select(original=>{
                    string name=original.name.ToLowerInvariant();if(!new[]{"wool","linen","lining","silk","brass"}.Any(name.Contains))return original;
                    string path=Data+"Cinderweave "+original.name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                    if(material==null){material=new Material(original);AssetDatabase.CreateAsset(material,path);}
                    material.color=name.Contains("brass")?new(.95f,.55f,.12f):name.Contains("lining")||name.Contains("silk")?new(.9f,.16f,.025f):new(.22f,.055f,.035f);EditorUtility.SetDirty(material);return material;
                }).ToArray();
            }
            foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))if(skin.name.Contains("Lantern"))Object.DestroyImmediate(skin.gameObject);
            var right=model.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Grip.R");var left=model.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Hand.L");
            if(right==null||left==null)throw new InvalidOperationException("Fire Mage humanoid lacks normal hands.");
            var wand=Group(right,"Emberglass wand",Vector3.zero);wand.localRotation=Quaternion.Euler(0,0,-90);
            wand.localScale=new Vector3(1/Mathf.Abs(right.lossyScale.x),1/Mathf.Abs(right.lossyScale.y),1/Mathf.Abs(right.lossyScale.z));
            Part(wand,"Obsidian wand",new(0,.20f,0),new(.04f,.48f,.04f),Paint("Obsidian",new(.09f,.06f,.055f)),PrimitiveType.Cylinder);
            Part(wand,"Emberglass focus",new(0,.47f,0),new(.12f,.21f,.12f),Paint("Living ember",new(1,.34f,.035f),true),PrimitiveType.Sphere);
            var focus=Group(left,"Left-hand focus",Vector3.zero);focus.localScale=new Vector3(1/Mathf.Abs(left.lossyScale.x),1/Mathf.Abs(left.lossyScale.y),1/Mathf.Abs(left.lossyScale.z));
            Part(focus,"Levitating ember",new(.02f,.17f,.04f),Vector3.one*.17f,Paint("Living ember",new(1,.34f,.035f),true),PrimitiveType.Sphere);
            foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=2;
            form=PrefabUtility.SaveAsPrefabAsset(root,Data+"Fire Mage form.prefab");
        }finally{Object.DestroyImmediate(root);}
    }
    private static void Menu()
    {
        var menu=Load<MenuPresentationCatalog>("Assets/_DiceFree/Resources/MenuPresentationCatalog.asset");var root=new GameObject("Fire Mage menu");
        try{root.AddComponent<ActorStats>().Configure(actor);root.AddComponent<CarriedInventory>().Configure(menu.items);root.AddComponent<Equipment>();PrefabUtility.InstantiatePrefab(form,root.transform);var preview=PrefabUtility.SaveAsPrefabAsset(root,Data+"Fire Mage menu.prefab");menu.forms=menu.forms.Where(f=>f.classId!=actor.stableId).Append(new MenuPresentationCatalog.Form{classId=actor.stableId,tier=3,preview=preview}).ToArray();EditorUtility.SetDirty(menu);}finally{Object.DestroyImmediate(root);}
    }
    private static void Equipment()
    {
        foreach(var guid in AssetDatabase.FindAssets("t:ItemDefinition",new[]{"Assets/_DiceFree/Settings","Assets/_DiceFree/Resources/DiceFree"}))
        {var item=Load<ItemDefinition>(AssetDatabase.GUIDToAssetPath(guid));if(item.allowedClassIds?.Contains("class.mage")!=true||item.allowedClassIds.Contains(actor.stableId))continue;item.allowedClassIds=item.allowedClassIds.Append(actor.stableId).ToArray();EditorUtility.SetDirty(item);}
    }
    private static void PlayerScene(string name)
    {
        var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/"+name+".unity");var controller=Object.FindObjectsByType<AdvancementController>(FindObjectsInactive.Include).Single();var player=controller.GetComponent<CombatActor>();
        var catalog=Load<ClassCatalog>(name=="Cornberg"?"Assets/_DiceFree/Settings/Progression/Class catalog.asset":"Assets/_DiceFree/Settings/Development/Priest/Development Priest catalog.asset");
        controller.Configure(catalog,controller.Definitions.Where(e=>e.stableId!=edge.stableId).Append(edge).ToArray());player.GetComponent<ManifestationPersistence>().ConfigureClassCatalog(catalog);
        Add<FireMageProgression>(player);Add<FireMageCaster>(player).Configure(fire);Add<FireMageAbilityProvider>(player).Configure(icons);
        var resources=player.GetComponent<ActorResourceController>();resources.Configure(resources.Profiles.Where(p=>p.classId!=actor.stableId).Concat(new[]{mana,heat}).ToArray());
        var journal=player.GetComponent<QuestJournal>();journal.Configure(journal.Definitions.Where(q=>q.stableId!=quest.stableId).Append(quest).ToArray());
        var switcher=new SerializedObject(player.GetComponent<ClassPresentationSwitcher>());var bindings=switcher.FindProperty("bindings");
        int index=Enumerable.Range(0,bindings.arraySize).Where(i=>bindings.GetArrayElementAtIndex(i).FindPropertyRelative("classId").stringValue==actor.stableId).DefaultIfEmpty(-1).First();
        if(index<0){index=bindings.arraySize;bindings.InsertArrayElementAtIndex(index);}else if(bindings.GetArrayElementAtIndex(index).FindPropertyRelative("visualRoot").objectReferenceValue is Transform old)Object.DestroyImmediate(old.gameObject);
        var visual=(GameObject)PrefabUtility.InstantiatePrefab(form,player.transform);visual.SetActive(false);var binding=bindings.GetArrayElementAtIndex(index);
        binding.FindPropertyRelative("classId").stringValue=actor.stableId;binding.FindPropertyRelative("visualRoot").objectReferenceValue=visual.transform;binding.FindPropertyRelative("animator").objectReferenceValue=visual.GetComponentInChildren<Animator>(true);switcher.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    private static Vector3 Open(Vector3 wanted)
    {
        for(int ring=0;ring<30;ring++)for(int i=0;i<16;i++)
        {float angle=i*Mathf.PI/8;var p=wanted+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*ring;if(!Physics.Raycast(p+Vector3.up*150,Vector3.down,out var ground,300,1<<8))continue;if(NavMesh.SamplePosition(ground.point,out var hit,1,1))return hit.position;}
        throw new InvalidOperationException("No navigable Ember Crown camp near "+wanted);
    }
    private static void Camp()
    {
        var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/WorldTwoArrival.unity");var nav=Object.FindAnyObjectByType<WorldNavigation>();var lease=NavMesh.AddNavMeshData(nav.Data);Physics.SyncTransforms();try{var old=GameObject.Find("Maelis Cinderweave - Ember Crown camp");if(old!=null)Object.DestroyImmediate(old);
        var camp=new GameObject("Maelis Cinderweave - Ember Crown camp");
        camp.transform.position=Open(AshfallReach.Origin+new Vector3(AshfallReach.EmberCrown.x-28,0,AshfallReach.EmberCrown.y+70));
        var npc=Group(camp.transform,"Maelis Cinderweave",Vector3.zero).gameObject;npc.layer=11;
        var body=(GameObject)PrefabUtility.InstantiatePrefab(form,npc.transform);body.transform.localRotation=Quaternion.Euler(0,200,0);
        var collider=npc.AddComponent<CapsuleCollider>();collider.center=Vector3.up;collider.radius=.4f;collider.height=2;
        var approach=Group(npc.transform,"Camp approach",Vector3.zero);approach.position=Open(npc.transform.position+Vector3.back*2);
        var mentor=npc.AddComponent<FireMageMentor>();mentor.Configure(quest,edge,Array.Empty<ItemDefinition>(),fire);mentor.ConfigureApproach(approach);
        var stone=Paint("Camp basalt",new(.20f,.18f,.18f));var brass=Paint("Camp brass",new(.55f,.32f,.09f));
        for(int side=-1;side<=1;side+=2)
        {Part(camp.transform,"Brazier pedestal",new(side*2.5f,.45f,1),new(.7f,.9f,.7f),stone);Part(camp.transform,"Brazier bowl",new(side*2.5f,1,1),new(.9f,.15f,.9f),brass,PrimitiveType.Sphere);Part(camp.transform,"Controlled flame",new(side*2.5f,1.25f,1),new(.3f,.5f,.3f),Paint("Living ember",new(1,.34f,.035f),true),PrimitiveType.Sphere);}
        Part(camp.transform,"Rolled crimson blanket",new(-2,.18f,-1.3f),new(1,.35f,.6f),Paint("Camp crimson",new(.35f,.05f,.025f)));
        Part(camp.transform,"Research table",new(2,.55f,-1.3f),new(1.5f,.12f,.8f),stone);
        File.WriteAllText("Logs/FireMagePlacement.txt","Maelis Cinderweave at Ember Crown base: "+npc.transform.position+"; approach "+approach.position);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}finally{lease.Remove();}
    }
    private static void Trial()
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var root=new GameObject("Ember Walk - solo escape course");var site=root.AddComponent<FireEscapeSite>();var origin=new Vector3(70000,0,70000);
        site.entrance=origin;site.finish=origin+new Vector3(0,0,48);site.mapBounds=new Rect(origin.x-32,origin.z-8,64,64);
        var safe=new List<Rect>();var lava=Paint("Molten lava",new(.65f,.075f,.008f),true);var floor=Paint("Pale safe stone",new(.47f,.41f,.31f));var basalt=Paint("Trial basalt",new(.12f,.10f,.10f));
        foreach(var material in new[]{lava,floor,basalt}){material.shader=Shader.Find("DiceFree/Ember Terrain");material.SetFloat("_Lava",material==lava?1:0);EditorUtility.SetDirty(material);}
        Part(root.transform,"Continuous lava floor",origin+new Vector3(0,-.3f,24),new(62,.6f,62),lava,PrimitiveType.Cube,8);
        void Safe(float x,float z,float width,float depth){var rect=new Rect(origin.x+x-width*.5f,origin.z+z-depth*.5f,width,depth);safe.Add(rect);Part(root.transform,"Safe basalt walkway",new Vector3(rect.center.x,.035f,rect.center.y),new(width,.07f,depth),floor);}
        for(int row=0;row<5;row++)Safe(row==0?12:row==4?-12:0,row*12,row==0||row==4?28:52,7.4f);
        for(int row=0;row<4;row++)Safe(row%2==0?24:-24,row*12+6,7.4f,19.4f);
        // Solid divider ridges prevent mouse pathfinding from skipping the bends.
        for(int row=0;row<4;row++)Part(root.transform,"Interlocking basalt ridge",origin+new Vector3(row%2==0?-5:5,1.8f,row*12+6),new(50,3.6f,4),basalt,PrimitiveType.Cube,9);
        for(int side=-1;side<=1;side+=2){Part(root.transform,"Course side cliff",origin+new Vector3(side*31,2.5f,24),new(2,5,64),basalt,PrimitiveType.Cube,9);Part(root.transform,"Course end cliff",origin+new Vector3(0,2.5f,24+side*32),new(64,5,2),basalt,PrimitiveType.Cube,9);}
        site.safeGround=safe.ToArray();var flames=new List<FireEscapeSite.Flame>();
        for(int row=0;row<5;row++)for(int lane=0;lane<2;lane++)
        {
            float x=row==0?8+lane*8:row==4?-16+lane*8:-11+lane*22;var point=origin+new Vector3(x,.03f,row*12);
            var tile=Part(root.transform,"Amber warning - flame gate",point+Vector3.up*.09f,new(2,.05f,7.4f),Paint("Warning flame "+row+"-"+lane,new(1,.6f,.1f),true));
            flames.Add(new FireEscapeSite.Flame{bounds=new Rect(point.x-1,point.z-3.7f,2,7.4f),offset=row*.8f+lane*2.5f,visual=tile.GetComponent<Renderer>()});
        }
        site.flames=flames.ToArray();
        foreach(var point in new[]{site.entrance,site.finish})
        {Part(root.transform,"Safe beacon",point+new Vector3(0,.12f,0),new(1.4f,.15f,1.4f),Paint("Beacon blue",new(.12f,.6f,.75f),true),PrimitiveType.Cylinder);}
        var light=new GameObject("Warm cavern daylight").AddComponent<Light>();light.type=LightType.Directional;light.transform.rotation=Quaternion.Euler(55,-35,0);light.intensity=1.15f;light.color=new Color(1,.78f,.57f);light.transform.SetParent(root.transform);
        RenderSettings.ambientLight=new Color(.3f,.22f,.18f);
        Physics.SyncTransforms();var bounds=new Bounds(origin+new Vector3(0,3,24),new Vector3(70,25,75));var sources=new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(bounds,(1<<8)|(1<<9),NavMeshCollectGeometry.PhysicsColliders,0,new List<NavMeshBuildMarkup>(),sources);
        var settings=NavMesh.GetSettingsByID(0);settings.agentRadius=.5f;settings.agentHeight=1.8f;settings.agentClimb=.4f;
        var baked=NavMeshBuilder.BuildNavMeshData(settings,sources,bounds,Vector3.zero,Quaternion.identity);if(baked==null)throw new InvalidOperationException("Ember Walk navigation bake failed.");
        var saved=AssetDatabase.LoadAssetAtPath<NavMeshData>(Data+"Ember Walk NavMesh.asset");if(saved==null){AssetDatabase.CreateAsset(baked,Data+"Ember Walk NavMesh.asset");saved=baked;}else{EditorUtility.CopySerialized(baked,saved);Object.DestroyImmediate(baked);}
        root.AddComponent<WorldNavigation>().Configure(saved);EditorUtility.SetDirty(saved);
        EditorSceneManager.SaveScene(scene,"Assets/_DiceFree/Scenes/"+FireEscapeSite.SceneName+".unity");
        var entries=EditorBuildSettings.scenes.ToList();if(!entries.Any(s=>s.path.EndsWith("/"+FireEscapeSite.SceneName+".unity"))){entries.Add(new EditorBuildSettingsScene("Assets/_DiceFree/Scenes/"+FireEscapeSite.SceneName+".unity",true));EditorBuildSettings.scenes=entries.ToArray();}
    }
    private static Transform Group(Transform parent,string name,Vector3 point){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=point;return t;}
    private static GameObject Part(Transform parent,string name,Vector3 point,Vector3 size,Material material,PrimitiveType type=PrimitiveType.Cube,int layer=2)
    {var go=GameObject.CreatePrimitive(type);go.name=name;go.layer=layer;go.transform.SetParent(parent,false);go.transform.localPosition=point;if(type==PrimitiveType.Cylinder)size.y*=.5f;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;if(layer==2)Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
    private static Material Paint(string name,Color color,bool glow=false)
    {string path=Data+name+".mat";var value=AssetDatabase.LoadAssetAtPath<Material>(path);if(value==null){value=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(value,path);}value.color=color;if(glow){value.EnableKeyword("_EMISSION");value.SetColor("_EmissionColor",color*1.4f);}EditorUtility.SetDirty(value);return value;}
    private static Texture2D Icon(int i)
    {
        string path=Data+FireMageProgression.Name(i)+" icon.asset";var value=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(value==null){value=new Texture2D(96,96,TextureFormat.RGBA32,false);AssetDatabase.CreateAsset(value,path);}var colors=new Color[96*96];
        for(int y=0;y<96;y++)for(int x=0;x<96;x++){var p=new Vector2((x-47.5f)/48,(y-47.5f)/48);float a=Mathf.Atan2(p.y,p.x),r=p.magnitude;float shape=i==1||i==5?Mathf.Abs(r-(i==1?.55f:.46f+.09f*Mathf.Sin(a*8))):i==3?Mathf.Min(Mathf.Abs(r-.43f),Mathf.Abs(p.x+p.y-.65f)):Mathf.Abs(r-(.36f+.15f*Mathf.Sin(a*(i==2?3:2))));float ink=1-Mathf.SmoothStep(.025f,.095f,shape);colors[y*96+x]=Color.Lerp(new(.065f,.012f,.008f),new(1,.28f+.1f*i,.04f),ink);}
        value.SetPixels(colors);value.Apply();EditorUtility.SetDirty(value);return value;
    }
}
