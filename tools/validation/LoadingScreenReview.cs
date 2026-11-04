using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.Advancement;
using DiceFree.ApplicationFlow;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Dungeons;
using DiceFree.Persistence;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class LoadingScreenReview
{
    public static string Start()
    {
        Need(!EditorApplication.isPlayingOrWillChangePlaymode&&!SceneManager.GetActiveScene().isDirty,"Clean Edit Mode required.");
        var guard=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t=>t!=null);
        guard.GetMethod("UseIsolatedSaveRootForNextPlay").Invoke(null,new object[]{Path.Combine(Path.GetTempPath(),"DiceFree-LoadingReview-"+Guid.NewGuid())});
        EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");EditorApplication.EnterPlaymode();return "Started isolated loading review.";
    }
    public static async Task<object> Run()
    {
        Need(Application.isPlaying,"Play Mode required.");
        StartMenuController menu=null;
        await Until(()=>(menu=Object.FindAnyObjectByType<StartMenuController>())!=null&&!LoadingScreen.Busy&&menu.ReadyForNewEcho,"Startup");
        bool initialized=false;LoadingScreen.Initialize("Preparing your adventure",()=>initialized=true);
        Need(LoadingScreen.Busy,"Initialization did not show overlay.");
        Directory.CreateDirectory("VisualBaselines/LoadingScreen");
        ScreenCapture.CaptureScreenshot("VisualBaselines/LoadingScreen/initializing.png");
        await Until(()=>initialized&&!LoadingScreen.Busy,"Initialization cleanup");
        Need(menu.TryStartNewEcho("Loading review"),"New Echo start failed: "+menu.Status);
        Need(LoadingScreen.Busy&&!menu.TryStartNewEcho("Duplicate"),"Duplicate transition was not rejected.");
        await World("Cornberg");
        var actor=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();
        Need(actor.GetComponent<ManifestationPersistence>().Flush(),"New Echo did not save.");
        Need(actor.GetComponent<ReturnToStartMenu>().Return(),"Saved return rejected.");
        Need(LoadingScreen.Busy,"Return did not use shared loading.");
        await Until(()=>(menu=Object.FindAnyObjectByType<StartMenuController>())!=null&&!LoadingScreen.Busy&&menu.HasLoadedProfile,"Return menu");
        var entry=menu.Entries.First(e=>e.active);
        Need(menu.TryPlayManifestation(entry.classId),"Resume failed.");
        await World("Cornberg");
        actor=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();
        Need(actor.GetComponent<ManifestationPersistence>().EchoName=="Loading review","Resume lost Echo identity.");
        Need(actor.GetComponent<ReturnToStartMenu>().Return(),"Second return failed.");
        await Until(()=>(menu=Object.FindAnyObjectByType<StartMenuController>())!=null&&!LoadingScreen.Busy,"Second menu");
        Need(menu.TryOpenDevelopmentRoom(),"Development entry failed.");
        await World("DevelopmentRoom");
        bool missing=false;
        Need(!LoadingScreen.TryLoadScene("Missing-Review-Scene","Unavailable",e=>missing=true)&&missing&&!LoadingScreen.Busy,"Missing scene error did not clean up.");
        await Task.Delay(150);
        Need(Object.FindObjectsByType<LoadingScreen>(FindObjectsInactive.Include).Length==0,"Loading overlay leaked after transitions.");
        return new{passed=true,startup=true,initialization=true,newEcho=true,saveAndReturn=true,resumed=true,developmentRoom=true,duplicateRejected=true,missingSceneRejected=true,overlayCleanup=true};
    }
    static async Task World(string name)
    {
        await Until(()=>SceneManager.GetActiveScene().name==name&&!LoadingScreen.Busy&&Object.FindAnyObjectByType<TraversalInput>()!=null&&
            Object.FindAnyObjectByType<TraversalInput>().GetComponent<ManifestationPersistence>().Ready,"World "+name);
    }
    static async Task Until(Func<bool> condition,string detail)
    {
        var deadline=DateTime.UtcNow.AddSeconds(50);while(!condition()){Need(DateTime.UtcNow<deadline,"Timed out: "+detail);await Task.Delay(25);}
    }
    static void Need(bool ok,string text){if(!ok)throw new InvalidOperationException(text);}
}
