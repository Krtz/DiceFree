using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using DiceFree.Advancement;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.UI;
using DiceFree.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class WolfEquippedReview
{
    static int checks;
    static readonly List<string> errors=new();
    static readonly List<object> rows=new();
    const string Output="VisualBaselines/WolfEquipped";
    static void Need(bool ok,string why){checks++;if(!ok)throw new InvalidOperationException(why);}
    static async Task Until(Func<bool> test,string why){var deadline=DateTime.UtcNow.AddSeconds(40);while(!test()){if(DateTime.UtcNow>deadline)throw new TimeoutException(why);await Task.Delay(25);}}
    static void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(text);}
    public static string Start()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Clean Edit Mode required");
        var guard=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t=>t!=null);
        guard.GetMethod("UseIsolatedSaveRootForNextPlay").Invoke(null,new object[]{Path.Combine(Path.GetTempPath(),"DiceFree-WolfEquipped-"+Guid.NewGuid())});
        EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");EditorApplication.EnterPlaymode();return "Started isolated wolf equipment review";
    }
    public static string Run(){checks=0;errors.Clear();rows.Clear();File.WriteAllText("Logs/WolfEquippedReview.json","{\"status\":\"running\"}");Application.logMessageReceived+=Log;_=Execute();return "Started; inspect Logs/WolfEquippedReview.json";}
    static void Save(RenderTexture rt,string name)
    {
        Directory.CreateDirectory(Output);var old=RenderTexture.active;RenderTexture.active=rt;
        var texture=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);
        texture.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);texture.Apply();
        File.WriteAllBytes(Output+"/"+name+".png",texture.EncodeToPNG());Object.Destroy(texture);RenderTexture.active=old;
    }
    static async Task Capture(string name){Directory.CreateDirectory(Output);await Task.Delay(150);ScreenCapture.CaptureScreenshot(Output+"/"+name+".png");await Task.Delay(150);}
    static async Task PoseFrame()
    {
        var ready=new TaskCompletionSource<bool>();
        void Complete(UnityEngine.Rendering.ScriptableRenderContext context,List<Camera> cameras)=>ready.TrySetResult(true);
        UnityEngine.Rendering.RenderPipelineManager.endContextRendering+=Complete;
        try{var timeout=Task.Delay(5000);Need(await Task.WhenAny(ready.Task,timeout)==ready.Task,"Rendered pose frame");}
        finally{UnityEngine.Rendering.RenderPipelineManager.endContextRendering-=Complete;}
    }
    static EquipmentPresentation.Binding Binding(CombatActor p,string id)=>p.GetComponent<EquipmentPresentation>().Bindings.Single(b=>b.definition?.stableId==id);
    static GameObject Visual(CombatActor p,string id)=>Binding(p,id).visuals.Single(g=>g!=null&&g.transform.IsChildOf(p.GetComponent<ClassPresentationSwitcher>().ActiveVisualRoot));
    static async Task Execute()
    {
        ExplorationCamera camera=null;
        var navType=typeof(EditorWindow).Assembly.GetType("UnityEditor.AI.NavMeshVisualizationSettings");
        var nav=navType?.GetProperty("showNavMesh",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
        var oldNav=nav?.GetValue(null);
        var viewType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        var view=viewType==null?null:Resources.FindObjectsOfTypeAll(viewType).FirstOrDefault();
        var gizmos=viewType?.GetProperty("drawGizmos",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
        var oldGizmos=view==null?null:gizmos?.GetValue(view);
        try
        {
            if(nav!=null)nav.SetValue(null,false);if(view!=null)gizmos?.SetValue(view,false);
            CombatActor p=null;
            await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.Motor.Ready&&p.GetComponent<ManifestationPersistence>().Ready,"Ready hero");
            p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.GetComponent<BasicAttack>().enabled=false;p.Health.Invulnerable=true;
            p.GetComponent<DiceFree.World.Interactor>().Cancel();
            var save=p.GetComponent<ManifestationPersistence>();
            var catalog=(ClassCatalog)typeof(ManifestationPersistence).GetField("classCatalog",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(save);
            bool Activate(string id)=>save.CurrentClassId==id||save.TryActivateExisting(id,out _)||save.TryForkAndActivate(catalog.Resolve(id),out _);
            var inventory=p.GetComponent<CarriedInventory>();var equipment=p.GetComponent<Equipment>();
            var items=Resources.LoadAll<ItemDefinition>("DiceFree/DungeonRewards/Wolf");Need(items.Length==6,"All six wolf items");
            Need(p.Motor.TeleportAcrossMaps(new Vector3(100,0,278)),"Clear city presentation position");
            p.transform.rotation=Quaternion.Euler(0,180,0);
            camera=Object.FindAnyObjectByType<ExplorationCamera>();camera.RecenterForTravel(p.transform);
            foreach(string classId in new[]{"class.novice","class.physically-blessed-novice","class.magically-touched-novice","class.mage","class.greatblade"})
            {
                Need(Activate(classId),"Activate "+classId);await Task.Delay(350);
                Need(p.Motor.TeleportAcrossMaps(new Vector3(100,0,278)),"City fixture after class restore");
                p.transform.rotation=Quaternion.Euler(0,180,0);camera.RecenterForTravel(p.transform);
                inventory=p.GetComponent<CarriedInventory>();equipment=p.GetComponent<Equipment>();
                var root=p.GetComponent<ClassPresentationSwitcher>().ActiveVisualRoot;
                var animator=ClassPresentationSwitcher.FindVisualAnimator(root);
                foreach(var item in items)
                {
                    var owned=inventory.Items.FirstOrDefault(i=>i.definitionId==item.stableId)??inventory.Grant(item.stableId);
                    var visual=Visual(p,item.stableId);
                    Need(visual.GetComponent<MeshFilter>()?.sharedMesh.vertexCount>100,"Sculpted mesh "+item.displayName);
                    Need(visual.GetComponentsInChildren<Collider>(true).Length==0,"Cosmetic gear has no colliders");
                    bool eligible=item.allowedClassIds.Length==0||item.allowedClassIds.Contains(classId);
                    Need(equipment.Equip(owned.instanceId,item.slot)==eligible,"Preserved eligibility "+classId+" / "+item.displayName);
                    await Task.Delay(160);await PoseFrame();
                    Need(visual.activeInHierarchy==eligible,"Equipment-driven visibility "+item.displayName);
                    if(!eligible)continue;
                    if(item.slot==EquipmentSlot.MainHand)
                    {
                        var pose=visual.GetComponent<WolfRewardWeaponPose>();Need(pose!=null&&pose.TwoHanded==(item.handsRequired==2),"Correct hand use");
                        float error=Vector3.Distance(visual.transform.position,animator.GetBoneTransform(HumanBodyBones.RightHand).position);
                        Need(error<.003f,"Weapon follows animated dominant hand: "+error);
                        float supportError=item.handsRequired==2?Vector3.Distance(animator.GetBoneTransform(HumanBodyBones.LeftHand).position,visual.transform.TransformPoint(Vector3.down*.22f)):0;
                        Need(supportError<.012f,"Staff support hand on grip "+supportError);
                        foreach(var baseline in Binding(p,item.stableId).baselineVisuals.Where(g=>g!=null&&g.transform.IsChildOf(root)))Need(!baseline.activeSelf,"Class default hidden "+baseline.name);
                        if(classId=="class.mage")Need(root.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Mage Orb").gameObject.activeInHierarchy,"One-hand wand preserves independent default focus");
                        rows.Add(new{classId,item=item.stableId,vertices=visual.GetComponent<MeshFilter>().sharedMesh.vertexCount,dominantHandError=error,supportHandError=supportError});
                        string shortId=item.stableId.Substring(WolfRewardEquipmentVisuals.Prefix.Length);
                        await Capture(classId+"-"+shortId+"-gameplay");
                        camera.SetPresentationPose(p.transform.position+new Vector3(2.4f,1.9f,-3.5f),Quaternion.LookRotation(p.transform.position+Vector3.up*1.0f-(p.transform.position+new Vector3(2.4f,1.9f,-3.5f))),32);
                        await Capture(classId+"-"+shortId+"-close");
                        Save(p.GetComponent<HudPortraitRenderer>().Texture,classId+"-"+shortId+"-portrait");
                        camera.ClearPresentationPose();
                        Need(equipment.Unequip(item.slot.ToString()),"Unequip weapon");await Task.Delay(100);
                        Need(!visual.activeSelf,"Unequipped weapon disappears");
                    }
                }
                // Multiple visits cannot accumulate duplicate props or stale class bindings.
                var presenter=p.GetComponent<WolfRewardEquipmentVisuals>();presenter.PrepareVisual(root,p.GetComponent<EquipmentPresentation>(),inventory);
                Need(items.All(i=>Binding(p,i.stableId).visuals.Count(g=>g!=null&&g.transform.IsChildOf(root))==1),"Idempotent class preparation");
                await Capture(classId+"-nightpelt-gameplay");
            }
            // Leave Mage with a full saved outfit for the actual menu preview.
            Need(Activate("class.mage"),"Return to Mage");await Task.Delay(250);inventory=p.GetComponent<CarriedInventory>();equipment=p.GetComponent<Equipment>();
            foreach(string id in new[]{"nightpelt-vest","moonfang-signet","starfall-wand"}){string full=WolfRewardEquipmentVisuals.Prefix+id;var item=inventory.Resolve(full);Need(equipment.Equip(inventory.Items.First(i=>i.definitionId==full).instanceId,item.slot),"Saved full outfit");}
            Need(save.Flush(),"Outfit save");
            Need(p.GetComponent<DiceFree.Dungeons.ReturnToStartMenu>().Return(),"Equipped menu return");
            StartMenuController menu=null;await Until(()=>(menu=Object.FindAnyObjectByType<StartMenuController>())!=null&&!DiceFree.ApplicationFlow.LoadingScreen.Busy,"Menu ready");
            await Task.Delay(900);await Capture("equipped-menu");
            var stage=Object.FindAnyObjectByType<MenuPortraitStage>();Need(stage!=null,"Real menu stage");
            var portrait=stage.Portraits.Single(s=>s.classId=="class.mage");Need(portrait.rendered,"Menu outfit rendered");
            Need(portrait.root.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("Wolf reward -",StringComparison.Ordinal))==3,"Menu shows three saved wolf pieces");
            Save(portrait.texture,"equipped-menu-mage");
            Need(menu.TryPlayManifestation("class.mage"),"Resume Mage");
            await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.GetComponent<ManifestationPersistence>().Ready&&!DiceFree.ApplicationFlow.LoadingScreen.Busy,"Saved outfit resume");
            await Task.Delay(350);Need(new[]{"nightpelt-vest","moonfang-signet","starfall-wand"}.All(id=>Visual(p,WolfRewardEquipmentVisuals.Prefix+id).activeInHierarchy),"Saved outfit visible after resume");
            Need(errors.Count==0,"Unity errors: "+string.Join(";",errors));
            File.WriteAllText("Logs/WolfEquippedReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{status="passed",checks,forms=5,items=6,rows,menuOutfit=true,savedResume=true,unityErrors=errors},Newtonsoft.Json.Formatting.Indented));
            Debug.Log("DICEFREE_WOLF_EQUIPPED_OK");
        }
        catch(Exception e){File.WriteAllText("Logs/WolfEquippedReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{status="failed",checks,rows,error=e.ToString(),unityErrors=errors},Newtonsoft.Json.Formatting.Indented));Debug.LogError(e);}
        finally{if(camera!=null)camera.ClearPresentationPose();if(oldNav!=null)nav.SetValue(null,oldNav);if(view!=null&&oldGizmos!=null)gizmos.SetValue(view,oldGizmos);Application.logMessageReceived-=Log;}
    }
}
