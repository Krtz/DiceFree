using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Dungeons;
using DiceFree.Items;
using DiceFree.Quests;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class WorldTwoExpeditionAuthoring
{
    public const string First="quest.world2.first-foothold",Survey="quest.world2.two-watchfires",Report="quest.world2.unfinished-record";
    public const string Needle="work.world2.needle-survey",Crown="work.world2.crown-survey";
    public const string Envoy="npc.willowford.envoy-tamsin",Conversation="report.world2.two-pips";
    const string ScenePath="Assets/_DiceFree/Scenes/WorldTwoArrival.unity";
    const string Data="Assets/_DiceFree/Settings/AshfallReach/Expedition";
    const string Native="Assets/_DiceFree/Art/World/";
    const string GroupName="Verdant Order - World Two expedition";
    static Transform group;
    static Material jade,ivory,brass,wood,glow;

    public static void Batch()
    {
        Directory.CreateDirectory(Data);AssetDatabase.Refresh();
        jade=Tint("Expedition green",new Color(.12f,.31f,.24f));ivory=Tint("Survey parchment",new Color(.84f,.79f,.59f));
        brass=Tint("Expedition brass",new Color(.68f,.44f,.15f));wood=Tint("Dispatch leather",new Color(.19f,.095f,.055f));
        glow=Tint("Survey objective glow",new Color(.20f,.84f,.95f),true);
        var first=Quest("A Foothold Beyond the Pines",First,8000,new[]{AncientRoadDragonLairPortal.QuestId},120);
        first.offer="The Verdant Council's dispatch reached us before you. We can investigate the dragon's appearance only if this outpost stays supplied. Break eight Ashscale patrol fighters, then report back. Spearhands and ridgebows both count.";
        first.activeDialogue="Keep to the roads outside the palisade and defeat eight Ashscales. Do not assume they serve the dragon you killed; that is what we are here to investigate.";
        first.turnInDialogue="The supply road is breathing again. Now we can do the work the Order sent us for: observe this face carefully, and separate what we know from what we only suspect.";
        first.locationHint="Hunt Ashscale spearhands and ridgebows outside Cinderwatch in Ashfall Reach.";
        first.turnInHint="Return to Expedition Warden Ivara, beside the southern gate inside Cinderwatch.";
        first.stages=new[]{new QuestObjective{kind=ObjectiveKind.Kill,familyId="ashscale.lizardman",count=8,
            creditMode=QuestCreditMode.NearbyParty,creditRadius=50,instruction="Defeat 8 Ashscale patrol fighters"}};

        var survey=Quest("The Two Watchfires",Survey,18000,new[]{First},220);
        survey.offer="The Basalt Needle and Ember Crown are the two great landmarks of this face. We left survey cases at the end of each approach trail. Chart both sites and record the patrol movements you can see. Visit them in either order, then bring me your observations.";
        survey.activeDialogue="The Needle station lies east of the basalt spires, south of Willowford. The Crown station overlooks the crater from the northwest end of its approach road. Inspect both survey cases; keep your footing and watch for patrols.";
        survey.turnInDialogue="Two landmarks, two guarded approaches. The reports give us a reliable map of the patrols, but nothing yet explains why the Ancient Road Dragon appeared in Arborium. Envoy Tamsin is gathering the Order's records in Willowford. Take our findings to her.";
        survey.locationHint="Survey the trail ends beside the Basalt Needle and northwest of Ember Crown. Both sites are marked on the atlas while this quest is active.";
        survey.turnInHint=first.turnInHint;
        survey.stages=new[]{new QuestObjective{kind=ObjectiveKind.All,count=2,instruction="Survey both World Two pip landmarks",alternatives=new[]{
            new QuestLeaf{kind=ObjectiveKind.Interact,objectiveId=Needle,contentId=Needle,count=1,instruction="Chart the Basalt Needle watchpost"},
            new QuestLeaf{kind=ObjectiveKind.Interact,objectiveId=Crown,contentId=Crown,count=1,instruction="Chart the Ember Crown watchpost"}}}};

        var report=Quest("An Unfinished Record",Report,10000,new[]{Survey},150);
        report.offer="Take the field report west along the road to Willowford. Envoy Tamsin has a desk near the inn and spring. Tell her what you found at the two watchposts; she will send a careful account to the Verdant Council.";
        report.activeDialogue="Follow the western road from Cinderwatch into Steamroot Vale. Meet Envoy Tamsin just north of Willowford's main road, beside the expedition banner.";
        report.turnInDialogue="Willowford now holds the expedition's first complete field record. The reason for the dragon's appearance remains an open question.";
        report.locationHint="Speak to Envoy Tamsin in Willowford, north of the spring and east of the inn.";
        report.turnInHint="Acknowledge Tamsin's conversation to file the field report.";
        report.completeOnObjectives=true;
        report.stages=new[]{new QuestObjective{kind=ObjectiveKind.TalkTo,contentId=Envoy,conversationId=Conversation,count=1,
            instruction="Deliver Ivara's field report to Envoy Tamsin in Willowford"}};
        var quests=new[]{first,survey,report};foreach(var quest in quests)EditorUtility.SetDirty(quest);

        foreach(string scenePath in new[]{"Assets/_DiceFree/Scenes/Cornberg.unity","Assets/_DiceFree/Scenes/DevelopmentRoom.unity"})
        {
            var scene=EditorSceneManager.OpenScene(scenePath);
            foreach(var journal in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<QuestJournal>(true)))
            {
                journal.Configure(journal.Definitions.Where(q=>q!=null&&!quests.Any(n=>n.stableId==q.stableId)).Concat(quests).ToArray());
                EditorUtility.SetDirty(journal);
            }
            EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new IOException("Could not save expedition quest roster.");
        }
        var dragon=Load<QuestDefinition>("Assets/_DiceFree/Settings/Arborium/The Ancient Road Dragon.asset");
        dragon.turnInDialogue="With the Ancient Road Dragon dead, Arborium has time to recover. Follow the trail south of Ironpine's main road; the small fire will clear the pines blocking the pass to World 2. The Verdant Order's expedition awaits you there. Speak to Expedition Warden Ivara inside Cinderwatch's southern gate. Find out why the dragon came to Arborium.";
        EditorUtility.SetDirty(dragon);

        var two=EditorSceneManager.OpenScene(ScenePath);Physics.SyncTransforms();
        var navigation=two.GetRootGameObjects().Single(g=>g.GetComponent<WorldNavigation>()!=null).GetComponent<WorldNavigation>();
        var old=navigation.transform.Find(GroupName);if(old!=null)Object.DestroyImmediate(old.gameObject);
        group=new GameObject(GroupName).transform;group.SetParent(navigation.transform,false);
        var instance=NavMesh.AddNavMeshData(navigation.Data);
        Vector3 start,ivaraAt,tamsinAt,needleAt,crownAt;
        try
        {
            start=Nav(AshfallReach.Cinderwatch);
            var ivara=Npc("Expedition Warden Ivara","Guard",new Vector2(7.6f,-57.3f),new Vector2(6,-57.3f),260);
            var giver=ivara.gameObject.AddComponent<QuestGiver>();giver.Configure(first);giver.ConfigureAdditional(survey,report);
            giver.ConfigureName(ivara.name);giver.ConfigureApproach(ivara.Find("Conversation approach"));ivaraAt=giver.ApproachPosition;
            Banner(ivara.position+new Vector3(1.8f,0,.8f));
            LibraryProp("cart_01",ivara.position+new Vector3(3,0,-.1f),2.5f,75);
            LibraryProp("barrel_01",ivara.position+new Vector3(1.8f,0,-1.6f),.9f,30);
            var tamsin=Npc("Envoy Tamsin","Herbalist",new Vector2(-143,-57.5f),new Vector2(-143,-59.4f),190);
            var talk=tamsin.gameObject.AddComponent<ConversationTarget>();talk.ConfigureName(tamsin.name);
            talk.Configure(Envoy,Conversation,"Ivara's field notes, and your account of both watchposts. Good. We have a map and witnesses now, rather than another rumour. I will send the record to the Verdant Council. The Ashscales keep watch around both landmarks, but that alone does not make them the dragon's servants. Nothing here yet explains why he appeared outside the Dice. Willowford will hold the expedition's records while we decide where to investigate next.");
            talk.ConfigureApproach(tamsin.Find("Conversation approach"));tamsinAt=talk.ApproachPosition;
            var conversationGate=tamsin.gameObject.AddComponent<QuestConversationGate>();conversationGate.Configure(report,null,null);
            tamsin.gameObject.AddComponent<QuestWorldMarker>();Banner(tamsin.position+new Vector3(-1.7f,0,.8f));
            LibraryProp("crate_01",tamsin.position+new Vector3(1.3f,0,.3f),1.0f,12);
            LibraryProp("lantern_01",tamsin.position+new Vector3(1.3f,1.05f,.3f),.42f,0,false);
            needleAt=SurveySite("Basalt Needle survey case",Needle,survey,AshfallReach.NeedlePath.Last()+new Vector2(4,3),-35);
            crownAt=SurveySite("Ember Crown survey case",Crown,survey,AshfallReach.CrownPath.Last()+new Vector2(-4,1),35);
        }
        finally{instance.Remove();}
        var nav=BuildNavigation(navigation.transform);navigation.Configure(nav);EditorUtility.SetDirty(navigation);
        var check=NavMesh.AddNavMeshData(nav);
        try
        {
            foreach(var point in new[]{ivaraAt,tamsinAt,needleAt,crownAt})RequirePath(start,point);
            foreach(var route in new[]{AshfallReach.ArrivalTrail,AshfallReach.SettlementRoad,AshfallReach.WillowfordRoad,AshfallReach.NeedlePath,AshfallReach.CrownPath})
                foreach(var point in route)RequirePath(start,Nav(point));
        }
        finally{check.Remove();}
        EditorSceneManager.MarkSceneDirty(two);if(!EditorSceneManager.SaveScene(two))throw new IOException("Could not save World Two expedition.");
        AssetDatabase.SaveAssets();Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/WorldTwoExpeditionAuthoring.json",JsonUtility.ToJson(new Summary {quests=3,npcs=2,surveySites=2,
            firstQuest=First,surveyQuest=Survey,reportQuest=Report,ivara=ivaraAt,tamsin=tamsinAt,needle=needleAt,crown=crownAt},true));
        Debug.Log("DICEFREE_WORLD_TWO_EXPEDITION_AUTHORED_OK");
    }
    [Serializable]sealed class Summary {public int quests,npcs,surveySites;public string firstQuest,surveyQuest,reportQuest;public Vector3 ivara,tamsin,needle,crown;}
    static QuestDefinition Quest(string name,string id,int xp,string[] prerequisites,int gold)
    {
        var path=Data+"/"+name+".asset";var value=AssetDatabase.LoadAssetAtPath<QuestDefinition>(path);
        if(value==null){value=ScriptableObject.CreateInstance<QuestDefinition>();AssetDatabase.CreateAsset(value,path);}
        value.name=value.title=name;value.stableId=id;value.version=1;value.requiredLevel=35;value.category=QuestCategory.Main;
        value.completedQuestIds=prerequisites;value.rewardXp=xp;
        string rewardPath=Data+"/"+name+" reward.asset";var reward=AssetDatabase.LoadAssetAtPath<FixedRewardDefinition>(rewardPath);
        if(reward==null){reward=ScriptableObject.CreateInstance<FixedRewardDefinition>();AssetDatabase.CreateAsset(reward,rewardPath);}
        reward.gold=gold;reward.items=Array.Empty<ItemDefinition>();EditorUtility.SetDirty(reward);value.reward=reward;return value;
    }
    static T Load<T>(string path)where T:Object=>AssetDatabase.LoadAssetAtPath<T>(path)??throw new IOException("Missing library asset: "+path);
    static Material Tint(string name,Color color,bool emissive=false)
    {
        string path=Data+"/"+name+".mat";var value=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(value==null){value=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(value,path);}
        value.color=color;value.SetFloat("_Smoothness",.3f);
        if(emissive){value.EnableKeyword("_EMISSION");value.SetColor("_EmissionColor",color*.65f);}
        EditorUtility.SetDirty(value);return value;
    }
    static Vector3 Ground(Vector2 local)
    {
        var point=AshfallReach.Origin+new Vector3(local.x,0,local.y);
        if(!Physics.Raycast(point+Vector3.up*120,Vector3.down,out var hit,260,1<<8,QueryTriggerInteraction.Ignore))throw new Exception("No World Two terrain at "+local);
        return hit.point;
    }
    static Vector3 Nav(Vector2 local)
    {
        var point=Ground(local);
        if(!NavMesh.SamplePosition(point,out var hit,2.8f,1))throw new Exception("No expedition navigation at "+local);
        return hit.position;
    }
    static Transform Place(Transform parent,string name,Vector3 point)
    {var node=new GameObject(name).transform;node.SetParent(parent,false);node.position=point;return node;}
    static GameObject Shape(Transform parent,string name,PrimitiveType type,Vector3 at,Vector3 size,Material material)
    {
        var node=GameObject.CreatePrimitive(type);node.name=name;Object.DestroyImmediate(node.GetComponent<Collider>());
        node.layer=2;node.transform.SetParent(parent,false);node.transform.localPosition=at;node.transform.localScale=size;
        node.GetComponent<Renderer>().sharedMaterial=material;return node;
    }
    static void Solid(Transform root,Vector3 center,Vector3 size)
    {var node=Place(root,"Expedition physical footprint",root.position);node.gameObject.layer=9;var collider=node.gameObject.AddComponent<BoxCollider>();collider.center=center;collider.size=size;}
    static Transform Npc(string name,string model,Vector2 local,Vector2 approach,float yaw)
    {
        var npc=Place(group,name,Nav(local));npc.rotation=Quaternion.Euler(0,yaw,0);npc.gameObject.layer=11;
        var body=(GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(Native+"CornbergBatch/NPCs/NPC_"+model+".fbx"),npc);
        PrefabUtility.UnpackPrefabInstance(body,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        var renderers=body.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
        body.transform.localScale*=2.22f/bounds.size.y;bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
        body.transform.position+=new Vector3(npc.position.x-bounds.center.x,npc.position.y-bounds.min.y,npc.position.z-bounds.center.z);
        foreach(var renderer in renderers)if(renderer.name.IndexOf("ClothPrimary",StringComparison.OrdinalIgnoreCase)>=0)renderer.sharedMaterial=jade;
        npc.gameObject.AddComponent<VillageVisualIdle>().Configure(body.transform,name.Length*17);
        var capsule=npc.gameObject.AddComponent<CapsuleCollider>();capsule.radius=.40f;capsule.height=2.1f;capsule.center=Vector3.up*1.05f;
        Solid(npc,new(0,1,0),new(.58f,2,.58f));Place(npc,"Conversation approach",Nav(approach));return npc;
    }
    static void Banner(Vector3 point)
    {
        var root=Place(group,"Verdant expedition banner",point);
        Shape(root,"Brass staff",PrimitiveType.Cylinder,new(0,1.6f,0),new(.08f,1.6f,.08f),brass);
        Shape(root,"Order green banner",PrimitiveType.Cube,new(.45f,2.25f,0),new(.9f,1.1f,.045f),jade);
        Shape(root,"Ivory expedition diamond",PrimitiveType.Cube,new(.45f,2.3f,-.035f),new(.29f,.29f,.055f),ivory).transform.localRotation=Quaternion.Euler(0,0,45);
    }
    static Transform LibraryProp(string model,Vector3 point,float size,float yaw,bool floor=true)
    {
        var root=Place(group,"Library expedition prop - "+model,point);root.rotation=Quaternion.Euler(0,yaw,0);
        var source=Load<GameObject>(Native+"VisualBatch01/Models/"+model+".fbx");
        var visual=(GameObject)PrefabUtility.InstantiatePrefab(source,root);PrefabUtility.UnpackPrefabInstance(visual,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        foreach(var t in visual.GetComponentsInChildren<Transform>(true))t.gameObject.layer=2;
        foreach(var collider in visual.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(collider);
        var renderers=visual.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
        visual.transform.localScale*=size/Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));
        bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
        visual.transform.position+=new Vector3(point.x-bounds.center.x,point.y-bounds.min.y,point.z-bounds.center.z);
        if(floor)Solid(root,Vector3.up*Mathf.Min(size*.35f,.55f),new(Mathf.Min(size*.7f,1.3f),Mathf.Min(size*.7f,1.1f),Mathf.Min(size*.65f,1.1f)));
        return root;
    }
    static Vector3 SurveySite(string name,string id,QuestDefinition quest,Vector2 local,float yaw)
    {
        var point=Nav(local);var root=LibraryProp("crate_01",point,1.15f,yaw);root.name=name;root.gameObject.layer=11;
        float top=root.GetComponentsInChildren<Renderer>().Max(r=>r.bounds.max.y)-point.y;
        Shape(root,"Leather survey folio",PrimitiveType.Cube,new(0,top+.03f,-.03f),new(.63f,.055f,.42f),wood);
        Shape(root,"Open field record",PrimitiveType.Cube,new(0,top+.065f,-.03f),new(.56f,.018f,.35f),ivory);
        Shape(root,"Brass chart divider",PrimitiveType.Cube,new(.10f,top+.087f,-.03f),new(.035f,.015f,.33f),brass);
        var target=root.gameObject.AddComponent<TimedInteractionTarget>();target.Configure(id,"Charting the watchpost",3);target.ConfigureName(name);
        var toward=local+(id==Needle?new Vector2(-1.7f,-1):new Vector2(1.7f,-1));
        var approach=Place(group,name+" approach",Nav(toward));target.ConfigureApproach(approach);
        var collider=root.gameObject.AddComponent<BoxCollider>();collider.center=Vector3.up*.6f;collider.size=new(1.25f,1.2f,1.25f);
        var gate=root.gameObject.AddComponent<QuestWorkGate>();gate.Configure(quest,id);
        var beacon=Place(root,"Survey objective marker",root.position+Vector3.up*1.8f);
        Shape(beacon,"Survey diamond",PrimitiveType.Cube,Vector3.zero,Vector3.one*.24f,glow).transform.localRotation=Quaternion.Euler(35,0,45);
        beacon.gameObject.SetActive(false);root.gameObject.AddComponent<GuildWorkSitePresentation>().Configure(null,gate,null,null,beacon.gameObject);
        LibraryProp("barrel_01",point+new Vector3(1.5f,0,.3f),.8f,32);
        LibraryProp("lantern_01",point+new Vector3(-1.0f,0,.7f),.65f,0);
        Banner(point+new Vector3(0,0,1.3f));return approach.position;
    }
    static NavMeshData BuildNavigation(Transform root)
    {
        Physics.SyncTransforms();var sources=new List<NavMeshBuildSource>();var blockers=new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(root,1<<8,NavMeshCollectGeometry.PhysicsColliders,0,new List<NavMeshBuildMarkup>(),sources);
        NavMeshBuilder.CollectSources(root,1<<9,NavMeshCollectGeometry.PhysicsColliders,1,new List<NavMeshBuildMarkup>(),blockers);sources.AddRange(blockers);
        var settings=NavMesh.GetSettingsByID(0);settings.overrideVoxelSize=true;settings.voxelSize=.25f;settings.agentSlope=40;settings.agentClimb=.4f;
        var bounds=AshfallReach.Bounds;var nav=NavMeshBuilder.BuildNavMeshData(settings,sources,new Bounds(new Vector3(bounds.center.x,45,bounds.center.y),new Vector3(bounds.width,140,bounds.height)),Vector3.zero,Quaternion.identity);
        if(nav==null)throw new Exception("Expedition navigation bake failed");
        string path=Data+"/World Two expedition navigation.asset";var existing=AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
        if(existing==null)AssetDatabase.CreateAsset(nav,path);else{EditorUtility.CopySerialized(nav,existing);Object.DestroyImmediate(nav);nav=existing;EditorUtility.SetDirty(nav);}return nav;
    }
    static void RequirePath(Vector3 from,Vector3 to)
    {
        if(!NavMesh.SamplePosition(to,out var target,1,1))throw new Exception("Quest approach lacks navigation: "+to);
        var path=new NavMeshPath();if(!NavMesh.CalculatePath(from,target.position,1,path)||path.status!=NavMeshPathStatus.PathComplete)throw new Exception("Disconnected expedition approach: "+to);
    }
}
