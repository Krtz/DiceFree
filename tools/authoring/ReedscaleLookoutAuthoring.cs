using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.EditorTools;
using DiceFree.Items;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.Verdant;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public static partial class ReedscaleLookoutAuthoring
{
    private const string SceneRoot = "Reedscale Lookout - north of Lanternmere";
    private const string Art = "Assets/_DiceFree/Art/World/ReedscaleLookout";
    private const string Settings = "Assets/_DiceFree/Settings/ReedscaleLookout";
    private const string WorldArt = "Assets/_DiceFree/Art/World/";
    private const string ReportId = "work.reedscale.patrol-report";
    private static readonly Vector2 Desk = new(-46,408);
    private static readonly Vector2[] Trail =
    {
        new(-31,336),new(-32,343),new(-30,349),new(-32,354),new(-36,362),new(-36,366),new(-36,375),
        new(-40,381),new(-42,388),new(-43,397),new(-46,405.8f)
    };
    private static Material loam, track, stone, darkStone, bark, wood, canvas, reeds,
        darkWater, shallows, leather, scale, belly, teal, bronze, ivory, eye, embers, guild;
    private static readonly System.Random Random = new(31951);

    public static void Batch()
    {
        var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
        if (GameObject.Find(SceneRoot) != null || AssetDatabase.IsValidFolder(Art) || AssetDatabase.IsValidFolder(Settings))
            throw new InvalidOperationException("Reedscale Lookout already exists. Use the pre-authoring scene to regenerate it.");
        Directory.CreateDirectory(Art);
        Directory.CreateDirectory(Settings);
        AssetDatabase.Refresh();
        Palette();

        var root = Group(null,SceneRoot,Vector3.zero);
        Surface(root,"Rising forest banks and lookout ridge",GroundMesh(),loam,8);
        Surface(root,"North trail and lookout approach",TrailMesh(),track);
        BuildEnvironment(root);

        var journal = Object.FindObjectsByType<QuestJournal>().Single(j=>j.Definitions.Any(q=>q.stableId=="quest.verdant.lanternmere"));
        var steward = GameObject.Find("Guild Steward - timber commissions").GetComponent<QuestGiver>();
        var quest = Quest();
        journal.Configure(journal.Definitions.Append(quest).ToArray());
        steward.ConfigureAdditional(steward.Offers.Skip(1).Append(quest).ToArray());
        EditorUtility.SetDirty(journal);
        EditorUtility.SetDirty(steward);
        var report = PatrolReport(root,quest,journal.GetComponent<CombatActor>());

        var respawn = Asset<RespawnDefinition>("Reedscale respawn");
        respawn.delaySeconds = 95;
        EditorUtility.SetDirty(respawn);
        var tuning = AssetDatabase.LoadAssetAtPath<CombatTuning>("Assets/_DiceFree/Settings/Combat/Cornberg provisional tuning.asset");
        if(tuning==null) throw new InvalidOperationException("World 1 combat tuning missing.");
        var spear = Species("Spear Guard","spear-guard",tuning,2640,76,1.75f,.64f,4.15f,490);
        var bow = Species("Ridgebow Sentry","ridgebow-sentry",tuning,2095,65,7.2f,.91f,3.45f,535);
        var spearModel=ModelPrefab(false);
        var bowModel=ModelPrefab(true);
        var patrol=Group(root,"Reedscale defenders",Vector3.zero);
        var spearHomes=new[]{new Vector2(-48,381),new Vector2(-38,386),new Vector2(-47,394),new Vector2(-54,401)};
        var bowHomes=new[]{new Vector2(-53,387),new Vector2(-31,397),new Vector2(-41,405),new Vector2(-54,409)};
        var defenders=spearHomes.Select((at,i)=>Spawn(patrol,spear,spearModel,39+i%3,at,respawn,false))
            .Concat(bowHomes.Select((at,i)=>Spawn(patrol,bow,bowModel,39+i%3,at,respawn,true))).ToArray();

        var route=Object.FindObjectsByType<MonoBehaviour>().Single(c=>c.GetType().Name=="OgreRoadwardenRoute");
        using var routeData=new SerializedObject(route);
        var wreckage=routeData.FindProperty("wreckage");
        var closed=Enumerable.Range(0,wreckage.arraySize)
            .Select(i=>(GameObject)wreckage.GetArrayElementAtIndex(i).objectReferenceValue)
            .Where(piece=>piece.activeSelf).ToArray();
        try
        {
            foreach(var piece in closed) piece.SetActive(false);
            CornbergSceneBuilder.Rebake();
        }
        finally
        {
            foreach(var piece in closed) piece.SetActive(true);
        }

        var nav=NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            var lanternmere=Sample(new Vector3(-31,.3f,336.3f),1.3f);
            var path=new NavMeshPath();
            var approach=Sample(report.ApproachPosition,.85f);
            RequirePath(lanternmere,approach,path,"Lanternmere to the patrol report");
            RequirePath(approach,Sample(steward.ApproachPosition,.65f),path,"report to the Guild Steward");
            RequirePath(lanternmere,Sample(new Vector3(-36,BridgeHeight(370),370),.29f),path,"the footbridge deck");
            foreach(var actor in defenders)
            {
                var position=Sample(actor.transform.position,.8f);
                RequirePath(approach,position,path,actor.name);
                actor.transform.position=position;
                actor.GetComponent<AggroBehaviour>().ConfigureHomePosition(position);
            }
            report.transform.Find("Report approach").position=approach;
        }
        finally {nav.Remove();}

        EditorSceneManager.MarkSceneDirty(scene);
        if(!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save Cornberg.");
        AssetDatabase.SaveAssets();
        File.WriteAllLines("Logs/ReedscaleSceneIds.txt",new Object[]{journal,steward}
            .Select(o=>GlobalObjectId.GetGlobalObjectIdSlow(o).targetObjectId.ToString()));
        File.WriteAllText("Logs/ReedscaleAuthoring.ok","Lanternmere, bridge, lookout, eight defenders and Guild return are connected.");
    }

    private static QuestDefinition Quest()
    {
        var quest=Asset<QuestDefinition>("Orders from the Ridge");
        quest.stableId="quest.arborium.reedscale-lookout";
        quest.title="Orders from the Ridge";
        quest.requiredLevel=35;
        quest.offer="Scouts have seen lizardmen moving north of Lanternmere. Cross the creek and find their lookout. Bring back a patrol report so the Guild can learn what they're guarding.";
        quest.activeDialogue="Follow the trail north from Lanternmere and cross the larch footbridge. Look for the ochre-roofed watchtower. The patrol report is on a table beneath a hide canopy in the camp.";
        quest.turnInDialogue="These orders name the Ancient Road Dragon. The Reedscale lizardmen answer to it directly, and their patrols watch a mountain path northwest of Arborium. I'll pass your report to the Guild.";
        quest.locationHint="Speak to the Guild Steward inside Arborium's Adventurers' Guild.";
        quest.turnInHint="Bring the Reedscale patrol report back to the Guild Steward in Arborium.";
        quest.stages=new[]{new QuestObjective {kind=ObjectiveKind.Interact,objectiveId=ReportId,contentId=ReportId,count=1,
            instruction="Recover the patrol report at Reedscale Lookout, north of Lanternmere"}};
        quest.rewardXp=3600;
        var reward=Asset<FixedRewardDefinition>("Lookout scouting reward");
        reward.gold=125;
        quest.reward=reward;
        EditorUtility.SetDirty(reward);
        EditorUtility.SetDirty(quest);
        return quest;
    }

    private static TimedInteractionTarget PatrolReport(Transform root,QuestDefinition quest,CombatActor player)
    {
        var site=Group(root,"Reedscale patrol report",Ground(Desk));
        site.gameObject.layer=11;
        Part(site,"Map table",PrimitiveType.Cube,P(0,.87f,0),P(1.75f,.13f,1.04f),wood);
        for(int side=-1;side<=1;side+=2)
            Part(site,"Map table trestle",PrimitiveType.Cube,P(side*.59f,.41f,0),P(.19f,.84f,.84f),bark);
        var paper=Group(site,"Patrol orders and bronze seal",Vector3.zero);
        Part(paper,"Patrol report parchment",PrimitiveType.Cube,P(-.20f,.95f,.06f),P(.78f,.021f,.60f),ivory)
            .localRotation=Quaternion.Euler(0,10,0);
        Part(paper,"Order seal",PrimitiveType.Cylinder,P(.13f,.976f,.23f),P(.16f,.026f,.16f),bronze);
        for(int i=0;i<5;i++)
            Part(paper,"Ink marks",PrimitiveType.Cube,P(-.23f,.97f,-.11f+i*.077f),P(.45f-i*.024f,.008f,.018f),leather);
        var target=site.gameObject.AddComponent<TimedInteractionTarget>();
        target.ConfigureName("Reedscale patrol report");
        target.Configure(ReportId,"Recovering the patrol report",2.3f);
        target.ConfigureApproach(Group(site,"Report approach",Ground(Desk+new Vector2(0,-1.72f))-site.position));
        var collider=site.gameObject.AddComponent<BoxCollider>();
        collider.center=P(0,.53f,0);collider.size=P(1.86f,1.07f,1.15f);
        var gate=site.gameObject.AddComponent<QuestWorkGate>();
        gate.Configure(quest,ReportId);
        var marker=Group(site,"Active report marker",Vector3.up*1.8f);
        Part(marker,"Guild objective diamond",PrimitiveType.Cube,Vector3.zero,P(.24f,.24f,.24f),guild).localRotation=Quaternion.Euler(35,0,45);
        site.gameObject.AddComponent<GuildWorkSitePresentation>().Configure(player,gate,paper.gameObject,null,marker.gameObject);
        marker.gameObject.SetActive(false);
        return target;
    }

    private static void RequirePath(Vector3 a,Vector3 b,NavMeshPath path,string label)
    {
        if(!NavMesh.CalculatePath(a,b,1,path)||path.status!=NavMeshPathStatus.PathComplete)
            throw new InvalidOperationException("No complete navigation for "+label+" ("+a+" to "+b+").");
    }

    private static Vector3 Sample(Vector3 at,float radius)
    {
        if(!NavMesh.SamplePosition(at,out var hit,radius,1)) throw new InvalidOperationException("No walkable surface near "+at);
        return hit.position;
    }

    private static T Asset<T>(string name) where T : ScriptableObject
    {
        var item=ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(item,Settings+"/"+name+".asset");
        return item;
    }

    private static Material Tint(string name,string color,float gloss=.15f,string emission=null)
    {
        var material=new Material(Shader.Find("Universal Render Pipeline/Lit")) {name=name};
        material.SetColor("_BaseColor",ColorOf(color));
        material.SetFloat("_Smoothness",gloss);
        if(emission!=null){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",ColorOf(emission));}
        AssetDatabase.CreateAsset(material,Art+"/"+name+".mat");
        return material;
    }

    private static Color ColorOf(string value)
    {
        ColorUtility.TryParseHtmlString("#"+value,out var color);
        return color;
    }

    private static void Palette()
    {
        var moss=AssetDatabase.LoadAssetAtPath<Material>(WorldArt+"VerdantWilds/Moss and forest litter.mat");
        loam=Tint("Ridge moss and loam","7E8F66");
        loam.SetTexture("_BaseMap",moss.GetTexture("_BaseMap"));
        track=AssetDatabase.LoadAssetAtPath<Material>(WorldArt+"BriarbackRegion/Trampled trail.mat");
        stone=Tint("Weathered slate","657270");
        darkStone=Tint("Wet slate","455754");
        bark=Tint("Weathered larch","654633");
        wood=Tint("Hewn larch","946D47");
        canvas=Tint("Faded ochre canvas","AD7843");
        reeds=Tint("Reeds and scrub","829B4A");
        darkWater=Tint("Creek water","347679",.67f);
        shallows=Tint("Creek shallows","61A2A1",.54f);
        leather=Tint("Boiled leather","573A2E");
        scale=Tint("Jade scales","426952",.24f);
        belly=Tint("Sage throat and claws","8CA16B",.18f);
        teal=Tint("Reedscale indigo cloth","386366");
        bronze=Tint("Hammered bronze","B4905F",.42f);
        ivory=Tint("Bone and parchment","E6CC98");
        eye=Tint("Amber slit eyes","E9B549",.28f,"BB6622");
        embers=Tint("Brazier coals","FF8B35",.2f,"D7531E");
        guild=Tint("Guild objective light","6DD5FC",.1f,"398EBC");
    }

    private static Vector3 P(float x,float y,float z)=>new(x,y,z);
    private static Vector3 Ground(Vector2 point)=>P(point.x,Height(point.x,point.y),point.y);
    private static float Pick(float a,float b)=>Mathf.Lerp(a,b,(float)Random.NextDouble());
    private static float Ramp(float a,float b,float value)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,value));
    private static float CreekZ(float x)=>370.2f+Mathf.Sin((x+36)*.16f)*1.3f;
    private static float OldNorthHeight(float x,float z)
    {
        float hill=.28f+.47f*Mathf.Sin((x+12)*.121f)*Mathf.Cos((z-311)*.118f);
        float rim=Mathf.InverseLerp(341,353,z);
        return -.025f+Mathf.Clamp01(-x/16)*(hill+rim*rim*5.2f);
    }

    private static float Height(float x,float z)
    {
        float bank=6.22f+.38f*Mathf.Sin(x*.19f)*Mathf.Sin(z*.20f)+Ramp(377,395,z)*2.6f;
        float creek=1.78f*(1-Ramp(1.5f,4.7f,Mathf.Abs(z-CreekZ(x))));
        float rim=Mathf.Max(Ramp(414,426,z),Mathf.Max(Ramp(-63,-76,x),Ramp(-21,-13,x)));
        float ridge=bank-creek+rim*rim*3.5f;
        if(z<350) return OldNorthHeight(x,z)+.012f;
        float entry=OldNorthHeight(x,350)+.73f*(z-350);
        return Mathf.Lerp(entry,ridge,Ramp(354.3f,361.5f,z));
    }

    private static Mesh GroundMesh()
    {
        const int columns=64,rows=88;
        var vertices=new Vector3[(columns+1)*(rows+1)];
        var uv=new Vector2[vertices.Length];
        var tris=new List<int>();
        for(int j=0;j<=rows;j++)
        {
            float z=Mathf.Lerp(348.6f,426,j/(float)rows);
            float opening=Ramp(349,364,z);
            float left=Mathf.Lerp(-48,-77,opening),right=Mathf.Lerp(-18,-13,opening);
            for(int i=0;i<=columns;i++)
            {
                float x=Mathf.Lerp(left,right,i/(float)columns);
                int index=j*(columns+1)+i;
                vertices[index]=P(x,Height(x,z),z);
                uv[index]=new Vector2(x,z)*.105f;
                if(i<columns&&j<rows)Quad(tris,index,index+columns+1,index+1,index+columns+2);
            }
        }
        return SaveMesh("Reedscale ridge terrain",vertices,uv,tris,true);
    }

    private static Mesh TrailMesh()
    {
        var samples=new List<Vector2>();
        for(int i=0;i<Trail.Length-1;i++)
        {
            int steps=Mathf.CeilToInt(Vector2.Distance(Trail[i],Trail[i+1])/.55f);
            for(int j=0;j<steps;j++) samples.Add(Vector2.Lerp(Trail[i],Trail[i+1],j/(float)steps));
        }
        samples.Add(Trail[^1]);
        const int width=6;
        var vertices=new Vector3[samples.Count*(width+1)];
        var uv=new Vector2[vertices.Length];
        var tris=new List<int>();
        float length=0;
        for(int row=0;row<samples.Count;row++)
        {
            var center=samples[row];
            if(row>0)length+=Vector2.Distance(samples[row-1],center);
            var forward=(samples[Mathf.Min(row+1,samples.Count-1)]-samples[Mathf.Max(0,row-1)]).normalized;
            var right=new Vector2(-forward.y,forward.x);
            for(int col=0;col<=width;col++)
            {
                float across=col/(float)width;
                int index=row*(width+1)+col;
                vertices[index]=Ground(center+right*(across*2-1)*(1.68f+.19f*Mathf.Sin(length*.75f)))+Vector3.up*.045f;
                float feather=Mathf.Min(Ramp(0,2.3f,row*.55f),Ramp(0,2.8f,(samples.Count-1-row)*.55f));
                uv[index]=new Vector2(Mathf.Lerp(1,across,feather),length*.21f);
                if(col<width&&row<samples.Count-1&&(center.y<=366.0f||center.y>=374.7f))
                    Quad(tris,index,index+1,index+width+1,index+width+2);
            }
        }
        return SaveMesh("North trail ribbon",vertices,uv,tris);
    }

    private static Transform Group(Transform parent,string name,Vector3 point)
    {
        var root=new GameObject(name).transform;
        root.SetParent(parent,false);root.localPosition=point;
        return root;
    }

    private static Transform Part(Transform parent,string name,PrimitiveType shape,Vector3 point,Vector3 size,Material mat)
    {
        var node=GameObject.CreatePrimitive(shape);
        node.name=name;node.transform.SetParent(parent,false);
        node.transform.localPosition=point;node.transform.localScale=size;
        node.GetComponent<Renderer>().sharedMaterial=mat;
        Object.DestroyImmediate(node.GetComponent<Collider>());
        return node.transform;
    }

    private static void Beam(Transform parent,string name,Vector3 a,Vector3 b,float radius,Material mat)
    {
        var beam=Part(parent,name,PrimitiveType.Cylinder,(a+b)*.5f,P(radius*2,Vector3.Distance(a,b)*.5f,radius*2),mat);
        beam.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
    }

    private static void Solid(Transform parent,Vector3 at,Vector3 size)
    {
        var block=Group(parent,"Solid collision",at).gameObject;
        block.layer=9;block.AddComponent<BoxCollider>().size=size;
    }

    private static void Barrier(Transform parent,Vector3 a,Vector3 b,float height,float thickness)
    {
        var rail=Group(parent,"Solid rail collision",(a+b)*.5f+Vector3.up*height*.41f);
        rail.rotation=Quaternion.LookRotation(b-a);
        rail.gameObject.layer=9;rail.gameObject.AddComponent<BoxCollider>().size=P(thickness,height,Vector3.Distance(a,b)+.19f);
    }

    private static void Quad(List<int> triangles,int a,int b,int c,int d)
    {
        triangles.Add(a);triangles.Add(b);triangles.Add(c);
        triangles.Add(c);triangles.Add(b);triangles.Add(d);
    }

    private static Mesh SaveMesh(string name,Vector3[] vertices,Vector2[] uv,List<int> indices,bool collision=false)
    {
        var mesh=new Mesh {name=name,vertices=vertices,uv=uv};
        mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Art+"/"+name+".asset");
        if(collision)
        {
            using var settings=new SerializedObject(mesh);
            var triangle=settings.FindProperty("m_PreBakeTriangleCollisionMesh");
            if(triangle!=null){triangle.boolValue=true;settings.ApplyModifiedPropertiesWithoutUndo();}
        }
        return mesh;
    }

    private static void Surface(Transform root,string name,Mesh mesh,Material material,int layer=0)
    {
        var objectRoot=Group(root,name,Vector3.zero).gameObject;
        objectRoot.layer=layer;
        objectRoot.AddComponent<MeshFilter>().sharedMesh=mesh;
        objectRoot.AddComponent<MeshRenderer>().sharedMaterial=material;
        if(layer==8)objectRoot.AddComponent<MeshCollider>().sharedMesh=mesh;
    }
}
