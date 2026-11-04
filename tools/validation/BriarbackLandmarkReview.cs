using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Persistence;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

public static class BriarbackLandmarkReview
{
    static string Output => Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Logs","BriarbackLive");
    public static string Start()
    {
        if(EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Clean Cornberg Edit Mode required.");
        var guard=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t=>t!=null);
        guard.GetMethod("UseIsolatedSaveRootForNextPlay").Invoke(null,new object[]{Path.Combine(Path.GetTempPath(),"DiceFree-LandmarkReview-"+Guid.NewGuid())});
        EditorApplication.EnterPlaymode();
        return "Opened isolated Play Mode.";
    }
    public static async Task<object> Run()
    {
        Need(Application.isPlaying,"Play Mode required");
        var gameViewType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        var gizmos=gameViewType?.GetProperty("showGizmos",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
        var gameView=gameViewType!=null ? Resources.FindObjectsOfTypeAll(gameViewType).FirstOrDefault() : null;
        var old=gameView!=null && gizmos!=null ? gizmos.GetValue(gameView) : null;
        var navType=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.AI.NavMeshVisualizationSettings")).FirstOrDefault(t=>t!=null);
        var nav=navType?.GetProperty("showNavMesh",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);
        var oldNav=nav?.GetValue(null);
        if(old!=null) gizmos.SetValue(gameView,false);
        if(oldNav!=null) nav.SetValue(null,false);
        try { return await CaptureGameplay(); }
        finally
        {
            if(oldNav!=null) nav.SetValue(null,oldNav);
            if(old!=null && gameView!=null) gizmos.SetValue(gameView,old);
        }
    }
    static async Task<object> CaptureGameplay()
    {
        Directory.CreateDirectory(Output);
        CombatActor actor=null;
        await Until(() => (actor=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null &&
             actor.GetComponent<ManifestationPersistence>().Ready && actor.Motor.Ready);
        actor.Health.Invulnerable=true;
        var scholar=Object.FindObjectsByType<QuestGiver>().Single(g=>g.name=="Trimery Elemental Scholar - Briarback");
        var camera=Object.FindAnyObjectByType<ExplorationCamera>();
        var interaction=actor.GetComponent<Interactor>();
        Move(actor,scholar.ApproachPosition);
        camera.RecenterForTravel(actor.transform);
        Need(interaction.Order(scholar),"Scholar was not available through normal interaction");
        await Until(()=>interaction.Active==scholar);
        var bounds=Object.FindAnyObjectByType<QuestInteractionPanel>().Bounds;
        Need(bounds.width>0 && bounds.yMin>0 && bounds.yMax<Screen.height*.70f,
            "Quest panel is still overlapping the bottom HUD.");
        await Capture("scholar-offer");
        Need(scholar.Accept(actor,scholar.Quest),"Quest offer could not be accepted");
        await Capture("scholar-active");
        interaction.Cancel();
        string[] elements={"Fire","Earth","Lightning","Ice"};
        var guardians=GameObject.Find("Briarback elemental guardians - additive").transform;
        foreach(var guardian in guardians.GetComponentsInChildren<CombatActor>())
        {
            var field=typeof(CombatActor).GetField("familyOverride",BindingFlags.NonPublic|BindingFlags.Instance);
            var original=field.GetValue(guardian);
            try
            {
                foreach(var value in new string[]{null,""})
                {
                    field.SetValue(guardian,value);
                    Need(guardian.FamilyId==guardian.Stats.Definition.familyId,"Missing family fallback on "+guardian.name);
                }
                field.SetValue(guardian,"test.explicit-family");
                Need(guardian.FamilyId=="test.explicit-family","Explicit family override was lost.");
            }
            finally {field.SetValue(guardian,original);}
        }
        var journal=actor.GetComponent<QuestJournal>();
        for(int i=0;i<elements.Length;i++)
        {
            var element=elements[i];
            var progress=journal.GetProgress(scholar.Quest.stableId);
            Need(progress.status==QuestStatus.Active && progress.stage==i,"Tracker did not advance to "+element);
            var site=GameObject.Find(element+" Shaman settlement - provisional").transform;
            Move(actor,site.position+new Vector3(2,0,-7));
            camera.RecenterForTravel(actor.transform);
            await Capture(element.ToLowerInvariant()+"-approach");
            var guardian=guardians.GetComponentsInChildren<CombatActor>().Single(g=>g.name.StartsWith(element+"Shaman guardian",StringComparison.Ordinal));
            guardian.Health.Invulnerable=false;
            guardian.Health.ApplyPacket(actor,new DamageResult{mitigated=1000000});
            Need(!guardian.Alive,"Guardian fixture did not count at "+element);
        }
        Need(journal.GetProgress(scholar.Quest.stableId).status==QuestStatus.ReadyToTurnIn,"The four sites did not unlock turn-in");
        Move(actor,scholar.ApproachPosition);
        camera.RecenterForTravel(actor.transform);
        Need(interaction.Order(scholar),"Could not reopen the scholar for turn-in");
        await Until(()=>interaction.Active==scholar);
        await Capture("scholar-turn-in");
        Need(scholar.TurnIn(actor,scholar.Quest) && journal.GetProgress(scholar.Quest.stableId).status==QuestStatus.Completed,
            "The live scholar could not complete the hunt.");
        return new{passed=true,scholarInteraction=true,questAccepted=true,sites=4,turnIn=true,output=Output};
    }
    static void Move(CombatActor actor,Vector3 p)
    {
        Need(NavMesh.SamplePosition(p,out var hit,3,NavMesh.AllAreas) && actor.Motor.Teleport(hit.position),
            "No navigable player position at "+p);
        Physics.SyncTransforms();
    }
    static async Task Capture(string name)
    {
        await Task.Delay(350);
        var file=Path.Combine(Output,name+".png");
        ScreenCapture.CaptureScreenshot(file);
        await Task.Delay(350);
        Need(File.Exists(file),"Game View screenshot missing: "+file);
    }
    static async Task Until(Func<bool> condition)
    {
        var deadline=DateTime.UtcNow.AddSeconds(30);
        while(!condition())
        {
            Need(DateTime.UtcNow<deadline,"Timed out waiting for gameplay state");
            await Task.Delay(30);
        }
    }
    static void Need(bool state,string error){if(!state)throw new InvalidOperationException(error);}
}
