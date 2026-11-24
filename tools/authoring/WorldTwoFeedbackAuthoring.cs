using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Quests;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class WorldTwoFeedbackAuthoring
{
    const string WorldOne = "Assets/_DiceFree/Scenes/Cornberg.unity";
    const string WorldTwo = "Assets/_DiceFree/Scenes/WorldTwoArrival.unity";
    const string Output = "Assets/_DiceFree/Settings/AshfallReach";
    const string BountyPath = Output + "/Cinderwatch repeat ashscale hunt.asset";
    static readonly Vector3 Origin = AshfallReach.Origin;

    public static void Batch()
    {
        if(!AssetDatabase.IsValidFolder(Output)) AssetDatabase.CreateFolder("Assets/_DiceFree/Settings", "AshfallReach");
        var bounty=AssetDatabase.LoadAssetAtPath<QuestDefinition>(BountyPath);
        if(bounty==null)
        {
            bounty=ScriptableObject.CreateInstance<QuestDefinition>();
            AssetDatabase.CreateAsset(bounty,BountyPath);
        }
        bounty.stableId="quest.cinderwatch.repeat-ashscale-hunt";
        bounty.title="Another 15 Ashscales"; bounty.version=1; bounty.requiredLevel=1;
        bounty.offer="Ashscale patrols are closing around Cinderwatch. Defeat fifteen Ashscales, then return for a bounty worth twenty Ashscale spearhands.";
        bounty.activeDialogue="Both spearhands and ridgebows count. Return when fifteen Ashscales have fallen.";
        bounty.turnInDialogue="The roads are safer for another day. Here is your bounty; there is always another patrol to break.";
        bounty.locationHint="Hunt Ashscale spearhands and ridgebows in Ashfall Reach and Steamroot Vale.";
        bounty.turnInHint="Return to the Cinderwatch Bounty Warden inside the palisade.";
        bounty.rewardXp=19700; bounty.repeatable=true;
        bounty.stages=new[]{new QuestObjective{kind=ObjectiveKind.Kill,familyId="ashscale.lizardman",count=15,
            creditMode=QuestCreditMode.NearbyParty,creditRadius=50,instruction="Defeat 15 Ashscales"}};
        EditorUtility.SetDirty(bounty);

        var one=EditorSceneManager.OpenScene(WorldOne);
        var giverTemplate=All<QuestGiver>(one).First(g=>g.Offers.Any(q=>q!=null&&q.stableId=="quest.briarback.repeat-bounty"));
        var dieTemplate=All<ResurrectionDiceVisual>(one).First(d=>d.Anchor.StableId=="anchor.trimery").transform;
        while(dieTemplate.parent!=null && dieTemplate.name!="Trimery Dice revive stone") dieTemplate=dieTemplate.parent;
        if(dieTemplate.name!="Trimery Dice revive stone")throw new Exception("Trimery dice template missing");
        foreach(var journal in All<QuestJournal>(one))
        {
            journal.Configure(journal.Definitions.Where(q=>q!=null&&q.stableId!=bounty.stableId).Concat(new[]{bounty}).ToArray());
            EditorUtility.SetDirty(journal);
        }
        AddCornbergCorner(one);
        EditorSceneManager.MarkSceneDirty(one);EditorSceneManager.SaveScene(one);

        var two=EditorSceneManager.OpenScene(WorldTwo,OpenSceneMode.Additive);
        SceneManager.SetActiveScene(two);
        var root=two.GetRootGameObjects().Single(g=>g.GetComponent<WorldNavigation>()!=null).transform;
        if(root.Find("World Two traveller improvements")!=null)throw new Exception("Feedback pass already authored; preserve existing work");
        var changes=new GameObject("World Two traveller improvements").transform;
        changes.SetParent(root,false);
        int trees=RegisterTreeCanopies(root);
        var thicket=BuildThicket(changes);
        var die=Object.Instantiate(dieTemplate.gameObject);
        SceneManager.MoveGameObjectToScene(die,two);die.transform.SetParent(changes,true);die.name="Willowford resurrection dice";
        if(PrefabUtility.IsPartOfPrefabInstance(die))PrefabUtility.UnpackPrefabInstance(die,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        foreach(var area in die.GetComponentsInChildren<ReachArea>(true))Object.DestroyImmediate(area);
        var anchor=die.GetComponentInChildren<ResurrectionAnchor>(true);
        var desired=Ground(new Vector2(-145,-64));
        die.transform.position+=desired-anchor.transform.position;
        var visual=die.GetComponentInChildren<ResurrectionDiceVisual>(true);
        anchor.name="Willowford resurrection anchor";anchor.Configure("anchor.willowford",visual.WorldPip.transform,4.5f);
        foreach(var text in die.GetComponentsInChildren<TextMesh>(true))text.text="Willowford";

        var npc=Object.Instantiate(giverTemplate.gameObject);
        SceneManager.MoveGameObjectToScene(npc,two);npc.transform.SetParent(changes,true);
        npc.name="Cinderwatch Bounty Warden";npc.transform.position=Ground(new Vector2(10,-36.5f));npc.transform.rotation=Quaternion.Euler(0,245,0);
        var giver=npc.GetComponent<QuestGiver>();giver.Configure(bounty);giver.ConfigureAdditional(Array.Empty<QuestDefinition>());giver.ConfigureName(npc.name);
        var approach=new GameObject("Bounty conversation approach").transform;approach.SetParent(changes,false);approach.position=Ground(new Vector2(8.8f,-36.5f));giver.ConfigureApproach(approach);
        var nav=BakeNavigation(root);
        var instance=NavMesh.AddNavMeshData(nav);
        try
        {
            RequirePath(Ground(AshfallReach.Cinderwatch),approach.position,"Cinderwatch bounty");
            RequirePath(Ground(AshfallReach.Cinderwatch),anchor.transform.position,"Willowford resurrection dice");
            RequirePath(Ground(AshfallReach.Cinderwatch),Origin+new Vector3(-96,1,-31),"Vaelis remains reachable");
            RequirePath(Ground(AshfallReach.Cinderwatch),Ground(new Vector2(-198,-62)),"River bridge remains open");
        }
        finally{instance.Remove();}
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(two);EditorSceneManager.SaveScene(two);
        var dev=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/DevelopmentRoom.unity");
        foreach(var journal in All<QuestJournal>(dev)){journal.Configure(journal.Definitions.Where(q=>q!=null&&q.stableId!=bounty.stableId).Concat(new[]{bounty}).ToArray());EditorUtility.SetDirty(journal);}
        EditorSceneManager.MarkSceneDirty(dev);EditorSceneManager.SaveScene(dev);
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/WorldTwoFeedbackAuthoring.json",JsonUtility.ToJson(new AuthorReport{existingTreeCanopies=trees,boundaryTrees=thicket,
            anchorId="anchor.willowford",bountyId=bounty.stableId,requiredKills=15,rewardXp=19700},true));
        Debug.Log("DICEFREE_WORLD_TWO_FEEDBACK_AUTHORING_OK");
    }
    [Serializable]class AuthorReport{public int existingTreeCanopies,boundaryTrees,requiredKills,rewardXp;public string anchorId,bountyId;}
    static T[] All<T>(Scene scene) where T:Component=>scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
    static Vector3 Ground(Vector2 point)
    {
        var p=Origin+new Vector3(point.x,0,point.y);
        if(!Physics.Raycast(p+Vector3.up*70,Vector3.down,out var hit,150,1<<8))throw new Exception("Missing World Two ground: "+point);
        return hit.point;
    }
    static void Canopy(GameObject tree,bool collision)
    {
        if(tree.GetComponent<TreeCameraOccluder>()==null)tree.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(9);
        if(!collision)return;
        var old=tree.GetComponent<CapsuleCollider>();
        var trunk=new GameObject("Trunk"){layer=9};trunk.transform.SetParent(tree.transform,false);
        var collider=trunk.AddComponent<CapsuleCollider>();
        collider.radius=old!=null?old.radius:.43f;collider.height=old!=null?old.height:3.4f;collider.center=old!=null?old.center:Vector3.up*1.7f;
        if(old!=null)Object.DestroyImmediate(old);
    }
    static int RegisterTreeCanopies(Transform root)
    {
        int count=0;
        foreach(var node in root.GetComponentsInChildren<Transform>(true).ToArray())
        {
            if(!PrefabUtility.IsAnyPrefabInstanceRoot(node.gameObject))continue;
            var path=PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(node.gameObject);
            if(path.IndexOf("/Trees/",StringComparison.OrdinalIgnoreCase)<0)continue;
            Canopy(node.gameObject,node.GetComponent<CapsuleCollider>()!=null);count++;
        }
        return count;
    }
    static int BuildThicket(Transform parent)
    {
        var wall=new GameObject("Northern Steamroot thicket — no crossing to Cornberg").transform;wall.SetParent(parent,false);
        var pine=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_DiceFree/Art/World/CornbergBatch/Trees/Tree_Pine.fbx");
        var rock=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/_DiceFree/Settings/Briarback/FourfoldHollow/Original faceted basalt.asset");
        var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Art/World/World1Handcrafted/Slate.mat");
        var random=new System.Random(24112026);int count=0;
        for(int row=0;row<4;row++)for(float x=-226;x<-57;x+=4.2f)
        {
            float z=21+row*3.7f+(float)random.NextDouble();float tx=x+(float)random.NextDouble()*2;
            if(Mathf.Abs(tx-AshfallReach.RiverX(z))<8)continue;
            var tree=(GameObject)PrefabUtility.InstantiatePrefab(pine,wall);tree.name="Dense northern pine";
            tree.transform.position=Ground(new Vector2(tx,z));tree.transform.rotation=Quaternion.Euler(0,random.Next(360),0);
            tree.transform.localScale=Vector3.one*(1.1f+(float)random.NextDouble()*.55f);Canopy(tree,true);count++;
            if(count%4==0)
            {
                var stone=new GameObject("Thicket mossy boulder"){layer=9};stone.transform.SetParent(wall,false);stone.transform.position=tree.transform.position+new Vector3(1,-.3f,.8f);
                stone.transform.localScale=new Vector3(2.7f,2.1f,2.6f);stone.AddComponent<MeshFilter>().sharedMesh=rock;stone.AddComponent<MeshRenderer>().sharedMaterial=material;stone.AddComponent<MeshCollider>().sharedMesh=rock;
            }
        }
        return count;
    }
    static void AddCornbergCorner(Scene scene)
    {
        if(scene.GetRootGameObjects().Any(g=>g.name=="Cornberg southwest corner ridge"))return;
        var root=new GameObject("Cornberg southwest corner ridge").transform;
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/_DiceFree/Settings/Briarback/FourfoldHollow/Original faceted basalt.asset");
        var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Art/World/World1Handcrafted/Slate.mat");
        var positions=new[]{new Vector3(-66,-2,-51),new Vector3(-65,-2,-37),new Vector3(-52,-2,-53),new Vector3(-39,-2,-54)};
        foreach(var p in positions)
        {
            var ridge=new GameObject("Corner mountain spur"){layer=9};ridge.transform.SetParent(root,false);ridge.transform.position=p;
            ridge.transform.localScale=new Vector3(12,15,12);ridge.AddComponent<MeshFilter>().sharedMesh=mesh;ridge.AddComponent<MeshRenderer>().sharedMaterial=mat;
        }
    }
    static NavMeshData BakeNavigation(Transform root)
    {
        Physics.SyncTransforms();var sources=new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(root,(1<<8)|(1<<9),NavMeshCollectGeometry.PhysicsColliders,0,new List<NavMeshBuildMarkup>(),sources);
        var data=NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(0),sources,new Bounds(Origin+new Vector3(-85,4,-60),new Vector3(294,35,194)),Vector3.zero,Quaternion.identity);
        if(data==null)throw new Exception("World Two navigation bake failed");
        data.name="World Two traveller navigation";
        var path=Output+"/World Two traveller navigation.asset";var previous=AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
        if(previous==null)AssetDatabase.CreateAsset(data,path);else{EditorUtility.CopySerialized(data,previous);Object.DestroyImmediate(data);data=previous;EditorUtility.SetDirty(data);}
        root.GetComponent<WorldNavigation>().Configure(data);return data;
    }
    static void RequirePath(Vector3 from,Vector3 to,string label)
    {
        if(!NavMesh.SamplePosition(from,out var start,2,1)||!NavMesh.SamplePosition(to,out var end,2,1))throw new Exception("No navigation for "+label);
        var path=new NavMeshPath();if(!NavMesh.CalculatePath(start.position,end.position,1,path)||path.status!=NavMeshPathStatus.PathComplete)throw new Exception("Disconnected "+label);
    }
}
