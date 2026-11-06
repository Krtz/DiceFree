using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class GuildForestryAuthoring
{
    const string Folder="Assets/_DiceFree/Settings/Arborium/GuildForestry";
    public const string Commission="quest.arborium.timber-commission",Clearing="quest.arborium.room-to-work",Survey="quest.arborium.road-survey";
    public const string Foreman="npc.trimery.guild-foreman";
    static readonly Vector3[] ClearingPoints={new Vector3(226,0,151),new Vector3(235,0,140),new Vector3(251,0,140),new Vector3(268,0,145),new Vector3(281,0,157)};
    static readonly Vector3[] SurveyPoints={new Vector3(217,0,180),new Vector3(247,0,139),new Vector3(78,0,353)};
    static Material oak,ivory,bronze,dark,blue,glass;
    static void Need(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
    static Transform[] All()=>SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
    sealed class NavigationScope:IDisposable
    {
        readonly NavMeshDataInstance instance;
        public NavigationScope(){instance=NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);}
        public void Dispose()=>instance.Remove();
    }
    public static object Inspect()
    {
        using var navigation=new NavigationScope();
        var all=All();var data=AssetDatabase.LoadAssetAtPath<NavMeshData>("Assets/_DiceFree/Settings/CornbergNavMesh.asset");var tri=NavMesh.CalculateTriangulation();return new{scene=SceneManager.GetActiveScene().name,dirty=SceneManager.GetActiveScene().isDirty,data=data!=null?new{position=data.position.ToString(),bounds=data.sourceBounds.ToString(),owner=AssetDatabase.GetAssetPath(Object.FindAnyObjectByType<WorldNavigation>().Data),triangles=tri.indices.Length,vertices=tri.vertices.Take(4).Select(p=>p.ToString())}:null,
            guild=all.Where(t=>t.GetComponent<ArboriumInterior>()?.BuildingName.StartsWith("Adventurers")==true).Select(t=>new{t.name,position=t.position.ToString(),rotation=t.eulerAngles.ToString()}),
            trimery=all.Where(t=>t.name.IndexOf("sawmill",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("Trimery",StringComparison.Ordinal)>=0&&t.parent==null).Select(t=>new{t.name,position=t.position.ToString()}),
            points=ClearingPoints.Concat(SurveyPoints).Select(p=>new{requested=p.ToString(),nav=NavMesh.SamplePosition(p,out var h,4,-1),point=h.position.ToString()}).ToArray(),
            npc=all.Where(t=>t.name=="NPC_Guard - 3D"||t.name=="Adventurers Guild Registrar").Select(t=>new{t.name,position=t.position.ToString()})};
    }
    static Vector3 Nav(Vector3 p){Need(NavMesh.SamplePosition(p,out var hit,4,-1),"No navigation at "+p);return hit.position;}
    static QuestObjective Talk(string text)=>new QuestObjective{kind=ObjectiveKind.TalkTo,contentId=Foreman,conversationId="conversation.trimery.guild-foreman",count=1,instruction=text};
    static QuestDefinition Quest(string filename,string id,string title,string offer,int xp,int gold,string prerequisite,params QuestObjective[] stages)
    {
        var q=ScriptableObject.CreateInstance<QuestDefinition>();q.name=q.title=title;q.stableId=id;q.offer=offer;q.activeDialogue=offer;q.locationHint="Adventurers' Guild reception in Arborium; Trimery sawmill and marked forestry sites.";q.rewardXp=xp;q.stages=stages;
        q.completedQuestIds=string.IsNullOrEmpty(prerequisite)?Array.Empty<string>():new[]{prerequisite};
        var reward=ScriptableObject.CreateInstance<FixedRewardDefinition>();reward.gold=gold;AssetDatabase.CreateAsset(reward,Folder+"/"+filename+" reward.asset");q.reward=reward;
        AssetDatabase.CreateAsset(q,Folder+"/"+filename+".asset");return q;
    }
    static QuestObjective Work(string[] labels,string prefix)=>new QuestObjective{kind=ObjectiveKind.All,count=labels.Length,instruction="Complete every marked site, in any order",alternatives=labels.Select((text,i)=>new QuestLeaf{kind=ObjectiveKind.Interact,objectiveId=prefix+"."+(i+1),contentId=prefix+"."+(i+1),instruction=text,count=1}).ToArray()};
    static Transform Group(string name,Transform parent,Vector3 local){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=local;return t;}
    static GameObject Shape(string name,PrimitiveType type,Transform parent,Vector3 at,Vector3 size,Material mat,Vector3 angles=default)
    {
        var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=at;g.transform.localScale=size;g.transform.localEulerAngles=angles;g.layer=2;Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=mat;return g;
    }
    static void Log(Transform parent,Vector3 point,float length,float angle,float radius)
    {
        var log=Group("Barked timber and cut ends",parent,point);log.localEulerAngles=new Vector3(0,angle,0);
        Shape("Oak bark",PrimitiveType.Cylinder,log,Vector3.zero,new Vector3(radius*2,length*.5f,radius*2),oak,new Vector3(90,0,0));
        for(int side=-1;side<=1;side+=2)
        {
            Shape("Fresh timber end grain",PrimitiveType.Cylinder,log,new Vector3(0,0,side*(length*.5f+.003f)),new Vector3(radius*1.85f,.007f,radius*1.85f),ivory,new Vector3(90,0,0));
            Shape("Heartwood",PrimitiveType.Cylinder,log,new Vector3(0,0,side*(length*.5f+.013f)),new Vector3(radius*.55f,.008f,radius*.55f),oak,new Vector3(90,0,0));
        }
    }
    static GameObject Npc(Transform template,Transform parent,string name,Vector3 at)
    {
        var go=Object.Instantiate(template.gameObject,parent);go.name=name;go.transform.SetPositionAndRotation(Nav(at),Quaternion.Euler(0,180,0));
        if(PrefabUtility.IsPartOfPrefabInstance(go))PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        foreach(var interaction in go.GetComponentsInChildren<InteractionTarget>(true))Object.DestroyImmediate(interaction);
        foreach(var marker in go.GetComponentsInChildren<QuestWorldMarker>(true))Object.DestroyImmediate(marker);
        return go;
    }
    static void Board(Transform parent,string heading,Vector3 at)
    {
        var board=Group(heading,parent,at);
        for(int side=-1;side<=1;side+=2)Shape("Carved oak board post",PrimitiveType.Cube,board,new Vector3(side*.72f,.85f,0),new Vector3(.12f,1.7f,.13f),oak);
        Shape("Timber notice board",PrimitiveType.Cube,board,new Vector3(0,1.25f,0),new Vector3(1.65f,1.05f,.12f),oak);
        Shape("Guild blue heading strip",PrimitiveType.Cube,board,new Vector3(0,1.63f,.08f),new Vector3(1.5f,.19f,.04f),blue);
        for(int i=0;i<3;i++){Shape("Parchment commission",PrimitiveType.Cube,board,new Vector3((i-1)*.43f,1.23f,.08f),new Vector3(.35f,.54f,.02f),ivory,new Vector3(0,0,(i-1)*-4));Shape("Bronze paper pin",PrimitiveType.Sphere,board,new Vector3((i-1)*.43f,1.45f,.11f),Vector3.one*.055f,bronze);}
    }
    public static object Author()
    {
        using var navigation=new NavigationScope();
        var scene=SceneManager.GetActiveScene();Need(scene.name=="Cornberg"&&!scene.isDirty&&!EditorApplication.isPlayingOrWillChangePlaymode&&!AssetDatabase.IsValidFolder(Folder),"Clean unauthored Cornberg required");
        var all=All();var guild=all.Single(t=>t.GetComponent<ArboriumInterior>()?.BuildingName.StartsWith("Adventurers")==true);
        var template=all.Single(t=>t.name=="Adventurers Guild Registrar");var player=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();
        foreach(var p in ClearingPoints.Concat(SurveyPoints))Nav(p);
        Need(EditorSceneManager.SaveScene(scene,"Assets/_DiceFree/Settings/Recovery/Cornberg-before-guild-forestry-20261106.unity",true),"Scene recovery failed");
        AssetDatabase.CreateFolder("Assets/_DiceFree/Settings/Arborium","GuildForestry");
        Material M(string n)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Art/World/ArboriumArrival/"+n+".mat");
        oak=AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Art/World/ArboriumFloors/Interior oak.mat");ivory=M("ivory carved stone");bronze=M("old gilded bronze");dark=M("deep recesses");blue=M("guild blue cloth");glass=M("practical enchanted glass");Need(new[]{oak,ivory,bronze,dark,blue,glass}.All(m=>m!=null),"Missing first-party palette");
        var commission=Quest("Timber Commission",Commission,"The Timber Commission","Trimery supplies Arborium with timber, but the ruined trade road has strained the crews. Speak to the Guild Foreman by Trimery's sawmill, then return to the Guild Steward in this reception. Reach Trimery by the existing Briarback detour.",1500,75,null,Talk("Discuss the timber crews with Trimery's Guild Foreman"));
        var clearing=Quest("Room to Work",Clearing,"Room to Work","Clear the five blue-marked deadfalls along Trimery's southern timberline. Work on each marked pile until it is cleared; moving, attacking or taking damage interrupts the work. Complete the sites in any order, speak to the Guild Foreman, then return here for your reward.",2500,100,Commission,
            Work(new[]{"West palisade deadfall","Pine-edge deadfall","Southern worksite","Eastern timber pile","East palisade deadfall"},"work.trimery.deadfall"),Talk("Report the cleared timberline to the Guild Foreman"));
        var survey=Quest("Road Survey",Survey,"A Road Worth Reopening","Survey the wrecked trade route at Trimery's western gate, the southern timberline and the Guild's planning table upstairs. The Roadwarden destroyed the highway under orders; who issued them is still unknown. Bring the three reports back to reception. This survey prepares a later restoration effort; it does not reopen the highway.",1800,125,Clearing,
            Work(new[]{"Inspect the western highway wreckage","Survey the southern timberline","Review the Guild planning table on floor four"},"work.arborium.survey"));
        var root=Group("Guild forestry commission - persistent work sites",null,Vector3.zero);
        var reception=guild.Find("Floor 1 - oak rooms and stairwell");Need(reception!=null,"Guild reception missing");
        var steward=Npc(template,reception,"Guild Steward - timber commissions",guild.TransformPoint(new Vector3(-3,0,3)));
        var giver=steward.AddComponent<QuestGiver>();giver.Configure(commission);giver.ConfigureAdditional(clearing,survey);giver.ConfigureName("Guild Steward");
        var stewardApproach=Group("Steward approach",reception,Vector3.zero);stewardApproach.position=Nav(guild.TransformPoint(new Vector3(-3,0,1.3f)));giver.ConfigureApproach(stewardApproach);
        Board(reception,"Adventurers' Guild timber commissions",new Vector3(-5,.12f,4.1f));
        var foreman=Npc(template,root,"Trimery Guild Foreman",new Vector3(239,0,188));
        var talk=foreman.AddComponent<ConversationTarget>();talk.ConfigureName("Guild Foreman");talk.Configure(Foreman,"conversation.trimery.guild-foreman","The southern timberline is choked with deadfall. Clear the five marked piles so our crews have room to work. We can then survey a possible southern route, but opening a passage needs a proper expedition. The western trade road was deliberately wrecked by the Ogre Roadwarden. He remains beyond the destruction, approachable from Arborium's side. We still do not know whose orders he followed. Take your findings back to the Guild Steward in Arborium.");
        var approach=Group("Foreman approach",root,Nav(new Vector3(240,0,186.5f)));talk.ConfigureApproach(approach);
        Board(root,"Trimery forestry work orders",Nav(new Vector3(239,0,192)));
        for(int i=0;i<5;i++)Site(root,player,clearing,clearing.stages[0].alternatives[i],Nav(ClearingPoints[i]),false,null);
        for(int i=0;i<3;i++)
        {
            var parent=i==2?guild.Find("Floor 4 - oak rooms and stairwell"):root;
            Vector3 at=i==2?guild.TransformPoint(new Vector3(5,.12f+3*guild.GetComponent<ArboriumInterior>().StoreyHeight,3.9f)):Nav(SurveyPoints[i]);
            Site(parent,player,survey,survey.stages[0].alternatives[i],at,true,i==2?"Guild planning table":null);
        }
        foreach(var journal in Object.FindObjectsByType<QuestJournal>(FindObjectsInactive.Include)){journal.Configure(journal.Definitions.Concat(new[]{commission,clearing,survey}).ToArray());EditorUtility.SetDirty(journal);}
        var panel=player.gameObject.AddComponent<TimedInteractionPanel>();panel.Configure(player.GetComponent<Interactor>());
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Need(EditorSceneManager.SaveScene(scene),"Scene save failed");
        return new{quests=3,workSites=5,surveySites=3,steward=steward.transform.position.ToString(),foreman=foreman.transform.position.ToString(),navigationUnchanged=true};
    }
    static void Site(Transform parent,CombatActor player,QuestDefinition quest,QuestLeaf objective,Vector3 at,bool survey,string special)
    {
        var site=Group(special??objective.instruction,parent,Vector3.zero);site.position=at;site.gameObject.layer=11;
        var target=site.gameObject.AddComponent<TimedInteractionTarget>();target.ConfigureName(objective.instruction);target.Configure(objective.contentId,survey?"Recording the survey":"Clearing deadfall",survey?1.8f:3f);
        var approach=Group("Clear work approach",site,Vector3.zero);approach.position=special!=null?at-parent.forward*1.8f:Nav(at+Vector3.back*1.3f);target.ConfigureApproach(approach);
        var collider=site.gameObject.AddComponent<BoxCollider>();collider.center=new Vector3(0,.65f,0);collider.size=new Vector3(1.7f,1.3f,1.7f);
        var gate=site.gameObject.AddComponent<QuestWorkGate>();gate.Configure(quest,objective.objectiveId);
        var before=Group("Before work",site,Vector3.zero);var after=Group("Completed work",site,Vector3.zero);
        if(!survey)
        {
            Log(before,new Vector3(-.2f,.35f,0),3.1f,74,.25f);Log(before,new Vector3(.4f,.70f,.15f),2.6f,121,.21f);
            Log(before,new Vector3(-.8f,.65f,.2f),1.6f,35,.12f);
            for(int j=0;j<4;j++)Log(after,new Vector3((j%2)*.48f-.24f,.22f+(j/2)*.42f,.2f),1.4f,90,.20f);
            Shape("Work stump",PrimitiveType.Cylinder,after,new Vector3(-1,.18f,.45f),new Vector3(.55f,.18f,.55f),oak);
            Shape("Pale stump end",PrimitiveType.Cylinder,after,new Vector3(-1,.37f,.45f),new Vector3(.52f,.01f,.52f),ivory);
            for(int j=0;j<5;j++)Shape("Wood chips",PrimitiveType.Cube,after,new Vector3(-.8f+j*.14f,.025f,-.35f+j%2*.15f),new Vector3(.12f,.025f,.06f),ivory,new Vector3(0,j*39,0));
        }
        else
        {
            if(special==null){Shape("Survey trestle",PrimitiveType.Cube,site,new Vector3(0,.85f,0),new Vector3(1.55f,.13f,1.1f),oak);
                for(int side=-1;side<=1;side+=2)Shape("Trestle support",PrimitiveType.Cube,site,new Vector3(side*.58f,.40f,0),new Vector3(.10f,.8f,.7f),oak);}
            float height=special==null?.94f:1.075f;
            Shape("Blank survey sheet",PrimitiveType.Cube,before,new Vector3(0,height,0),new Vector3(1.2f,.02f,.8f),ivory);
            Shape("Recorded survey map",PrimitiveType.Cube,after,new Vector3(0,height,0),new Vector3(1.2f,.02f,.8f),ivory);
            for(int j=0;j<4;j++)Shape("Survey ink line",PrimitiveType.Cube,after,new Vector3((j-1.5f)*.20f,height+.017f,0),new Vector3(.025f,.008f,.65f),blue,new Vector3(0,j*17,0));
        }
        var beacon=Group("Active guild marker",site,new Vector3(0,1.8f,0));
        Shape("Blue commission stone",PrimitiveType.Cube,beacon,Vector3.zero,new Vector3(.18f,.30f,.18f),glass,new Vector3(0,45,45));
        Shape("Guild ribbon",PrimitiveType.Cube,beacon,new Vector3(0,-.25f,0),new Vector3(.24f,.22f,.025f),blue);
        site.gameObject.AddComponent<GuildWorkSitePresentation>().Configure(player,gate,before.gameObject,after.gameObject,beacon.gameObject);
        after.gameObject.SetActive(false);beacon.gameObject.SetActive(false);
    }
    public static object RefinePlanningTable()
    {
        Need(!EditorApplication.isPlayingOrWillChangePlaymode&&!SceneManager.GetActiveScene().isDirty,"Clean Edit Mode required");
        var target=Object.FindObjectsByType<TimedInteractionTarget>().Single(t=>t.ContentId=="work.arborium.survey.3");
        var guild=target.GetComponentInParent<ArboriumInterior>();
        target.transform.position=guild.transform.TransformPoint(new Vector3(5,.12f+3*guild.StoreyHeight,3.9f));
        target.transform.Find("Clear work approach").localPosition=new Vector3(0,0,-1.8f);
        foreach(var child in target.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Survey trestle"||t.name=="Trestle support").ToArray())Object.DestroyImmediate(child.gameObject);
        foreach(var child in target.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Blank survey sheet"||t.name=="Recorded survey map"||t.name=="Survey ink line"))child.localPosition=new Vector3(child.localPosition.x,child.name=="Survey ink line"?1.092f:1.075f,child.localPosition.z);
        var clearing=AssetDatabase.LoadAssetAtPath<QuestDefinition>(Folder+"/Room to Work.asset");clearing.stages[0].alternatives[4].instruction="East palisade deadfall";EditorUtility.SetDirty(clearing);
        Object.FindObjectsByType<TimedInteractionTarget>().Single(t=>t.ContentId=="work.trimery.deadfall.5").ConfigureName("East palisade deadfall");
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());Need(EditorSceneManager.SaveScene(SceneManager.GetActiveScene()),"Scene save failed");return new{reusedExistingPlanningTable=true};
    }
}
