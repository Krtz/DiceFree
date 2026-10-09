using System;
using System.Collections;
using System.IO;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Dungeons;
using DiceFree.Foundation;
using DiceFree.Gameplay;
using DiceFree.UI;
using DiceFree.Quests;
using DiceFree.Advancement;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class PlaytestFixesValidation
    {
        const string Key="DiceFree.PlaytestFixes.";
        static IEnumerator flow;static double deadline;static int checks;
        static Keyboard keyboard;static Mouse mouse;
        static InputSettings.BackgroundBehavior background;static InputSettings.EditorInputBehaviorInPlayMode editorInput;
        static PlaytestFixesValidation()=>EditorApplication.playModeStateChanged+=Changed;
        static void Check(bool value,string message){if(!value)throw new InvalidOperationException(message);checks++;}
        [CliCommand("dicefree.playtest-fixes.playtest", "Isolated-save P0 cave, idle, tree ghosting, dummy and early-start regression.")]
        public static object Start()
        {
            Check(!EditorApplication.isPlayingOrWillChangePlaymode,"Stop Play Mode first.");
            for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)Check(!UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty,"Save scene edits first.");
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(Path.Combine(Path.GetTempPath(),"DiceFree-PlaytestFixes-"+Guid.NewGuid().ToString("N")));
            SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"error","");
            EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");EditorApplication.EnterPlaymode();return Status();
        }
        [CliCommand("dicefree.playtest-fixes.status", "Read actual P0 Play Mode regression results.")]
        public static object Status()=>new {status=SessionState.GetString(Key+"status","idle"),checks=SessionState.GetInt(Key+"checks",0),error=SessionState.GetString(Key+"error","")};
        static void Changed(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key+"status","")=="running")
            {Application.runInBackground=true;checks=0;flow=Flow();deadline=EditorApplication.timeSinceStartup+55;
                background=InputSystem.settings.backgroundBehavior;editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
                InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                keyboard=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
            if(state==PlayModeStateChange.ExitingPlayMode&&flow!=null)Finish("Play Mode interrupted.",false);
        }
        static void Tick(){try{Check(EditorApplication.timeSinceStartup<deadline,"P0 test timeout.");if(!flow.MoveNext())Finish(null);}catch(Exception e){Finish(e.ToString());}}
        static void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)Finish(message+"\n"+stack);}
        static void Finish(string error,bool stop=true){EditorApplication.update-=Tick;Application.logMessageReceived-=Log;flow=null;
            if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;
            SessionState.SetString(Key+"status",error==null?"passed":"failed");SessionState.SetString(Key+"error",error??"");SessionState.SetInt(Key+"checks",checks);if(error==null)Debug.Log("PLAYTEST_P0_P1_PLAYMODE_OK checks="+checks);if(stop)EditorApplication.ExitPlaymode();}
        static void KeyPress(Key key,Component receiver){InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));InputSystem.Update();receiver.SendMessage("Update");InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();}
        static void Click(Vector2 point,SkillTargetingController receiver){InputSystem.QueueStateEvent(mouse,new MouseState{position=point,buttons=1});InputSystem.Update();receiver.SendMessage("Update");InputSystem.QueueStateEvent(mouse,new MouseState{position=point});InputSystem.Update();}
        static void Capture(string name)
        {
            var camera=Camera.main;var previous=camera.targetTexture;var rt=RenderTexture.GetTemporary(1280,720,24);camera.targetTexture=rt;camera.Render();var active=RenderTexture.active;RenderTexture.active=rt;
            var png=new Texture2D(1280,720,TextureFormat.RGB24,false);png.ReadPixels(new Rect(0,0,1280,720),0,0);png.Apply();Directory.CreateDirectory("Logs");File.WriteAllBytes("Logs/PlaytestFixes-"+name+".png",png.EncodeToPNG());UnityEngine.Object.Destroy(png);RenderTexture.active=active;camera.targetTexture=previous;RenderTexture.ReleaseTemporary(rt);
        }
        static IEnumerator Flow()
        {
            CombatActor hero;
            do{yield return null;hero=UnityEngine.Object.FindFirstObjectByType<TraversalInput>()?.GetComponent<CombatActor>();}while(hero==null||!hero.Motor.Ready||!hero.GetComponent<ManifestationPersistence>().Ready);
            foreach(var ai in UnityEngine.Object.FindObjectsByType<DiceFree.AI.AggroBehaviour>())ai.enabled=false;
            var codex=hero.GetComponent<CodexProgression>();var overworld=CombatActor.All.First(a=>a!=hero&&a.Stats.Definition.stableId=="enemy.crop-slime");
            int kills=codex.Monsters.FirstOrDefault(m=>m.monsterId==overworld.Stats.Definition.stableId)?.kills??0;
            overworld.Health.ApplyDamage(hero,new DamageResult{mitigated=overworld.Health.Maximum*2});
            Check(codex.Monsters.Single(m=>m.monsterId==overworld.Stats.Definition.stableId).kills==kills+1,"Overworld kill absent from Codex.");
            overworld.Health.ApplyDamage(hero,new DamageResult{mitigated=999});Check(codex.Monsters.Single(m=>m.monsterId==overworld.Stats.Definition.stableId).kills==kills+1,"Duplicate corpse kill counted.");
            string codexJson=codex.CaptureJson();codex.RestoreJson(CodexProgression.CodexVersion,codexJson);Check(codex.Monsters.Single(m=>m.monsterId==overworld.Stats.Definition.stableId).kills==kills+1,"Codex persistence roundtrip lost kill.");
            var original=hero.Stats.Definition;var physical=AssetDatabase.LoadAssetAtPath<ActorDefinition>("Assets/_DiceFree/Settings/Progression/Physically Blessed Novice shell.asset");hero.Stats.Configure(physical);
            var auto=physical.basicAttack;float middle=DamageResolver.Calculate(hero.Stats,hero.Stats,auto).baseRaw;
            Check(Mathf.Approximately(DamageResolver.Calculate(hero.Stats,hero.Stats,auto,basicAttackRangeOffset:-3).baseRaw,middle-3),"Physical minimum offset wrong.");
            Check(Mathf.Approximately(DamageResolver.Calculate(hero.Stats,hero.Stats,auto,basicAttackRangeOffset:3).baseRaw,middle+3),"Physical maximum offset wrong.");
            var spell=UnityEngine.Object.Instantiate(auto);Check(Mathf.Approximately(DamageResolver.Calculate(hero.Stats,hero.Stats,spell,basicAttackRangeOffset:3).baseRaw,DamageResolver.Calculate(hero.Stats,hero.Stats,spell).baseRaw),"Spread affected spells.");UnityEngine.Object.Destroy(spell);hero.Stats.Configure(original);
            var journal=hero.GetComponent<QuestJournal>();var tracker=UnityEngine.Object.FindFirstObjectByType<QuestTracker>();var quests=journal.CaptureState();var completed=journal.Definitions.First(q=>q.stages.Last().kind!=ObjectiveKind.Any);
            var modified=journal.CaptureState();var record=modified.Single(p=>p.questId==completed.stableId);record.stage=completed.stages.Length-1;record.count=completed.stages.Last().count;record.status=QuestStatus.Completed;journal.RestoreState(modified);
            Check(!tracker.TrackedQuests.Contains(completed)&&journal.GetProgress(completed.stableId).status==QuestStatus.Completed,"Completed quest tracker/history wrong.");
            record.status=QuestStatus.ReadyToTurnIn;journal.RestoreState(modified);Check(tracker.TrackedQuests.Contains(completed),"Ready-to-turn-in quest hidden.");journal.RestoreState(quests);
            var journalPanel=hero.GetComponent<QuestJournalPanel>();journalPanel.Show();Check(journalPanel.Open&&journalPanel.BlocksPointer,"Journal button panel missing.");KeyPress(UnityEngine.InputSystem.Key.Escape,hero.GetComponent<EscapeMenuController>());Check(!journalPanel.Open,"Journal Escape close failed.");yield return null;
            var minimap=hero.GetComponent<MinimapHud>();var cameraPosition=Camera.main.transform.position;minimap.Zoom(100);Check(minimap.WorldRadius==60,"Minimap upper clamp wrong.");minimap.Zoom(.001f);Check(minimap.WorldRadius==8&&Camera.main.transform.position==cameraPosition,"Minimap zoom affects player camera.");minimap.Zoom(2.75f);
            var inventory=hero.GetComponent<CarriedInventory>();var equipment=hero.GetComponent<Equipment>();var wallet=hero.GetComponent<GoldWallet>();var shop=UnityEngine.Object.FindFirstObjectByType<ItemShop>();
            Check(hero.Motor.Teleport(shop.ApproachPosition+Vector3.back*1.5f)&&shop.CanInteract(hero),"Merchant interaction broken by NPC capsule.");
            wallet.Grant(shop.Offers[0].goldPrice);int beforeItems=inventory.Items.Length;long purchaseGold=wallet.Gold;bool purchaseAtomic=false,purchaseReentered=false;
            Action buyObserver=()=>{purchaseAtomic=inventory.Items.Length==beforeItems+1&&wallet.Gold==purchaseGold-shop.Offers[0].goldPrice;purchaseReentered=shop.Purchase(hero,0,out _);};inventory.Changed+=buyObserver;
            Check(shop.Purchase(hero,0,out _),"Atomic purchase failed.");inventory.Changed-=buyObserver;Check(purchaseAtomic&&!purchaseReentered,"Purchase observers saw partial state or reentrant duplicate.");
            var item=inventory.Grant(shop.Offers[0].item.stableId);Check(equipment.Equip(item.instanceId,shop.Offers[0].item.slot),"Sale fixture equip failed.");Check(!shop.Sell(hero,item.instanceId,out _),"Equipped item sold.");equipment.Unequip(shop.Offers[0].item.slot.ToString());
            var bound=inventory.Items;bound.Single(i=>i.instanceId==item.instanceId).bound=true;inventory.Restore(bound);Check(!shop.Sell(hero,item.instanceId,out _),"Bound item sold.");bound.Single(i=>i.instanceId==item.instanceId).bound=false;inventory.Restore(bound);
            var definition=shop.Offers[0].item;bool priorQuest=definition.questItem;
            try{definition.questItem=true;Check(!shop.Sell(hero,item.instanceId,out _),"Quest item sold.");}finally{definition.questItem=priorQuest;}
            long beforeGold=wallet.Gold,price=shop.ResalePrice(definition);bool observedAtomic=false,reentered=false;
            Action observe=()=>{observedAtomic=inventory.Find(item.instanceId)==null&&wallet.Gold==beforeGold+price;reentered=shop.Sell(hero,item.instanceId,out _);};inventory.Changed+=observe;
            Check(shop.Sell(hero,item.instanceId,out _),"Owned bag item sale failed.");inventory.Changed-=observe;Check(observedAtomic&&!reentered&&wallet.Gold==beforeGold+price&&inventory.Find(item.instanceId)==null,"Sale observers saw partial state or reentrant duplicate.");Check(!shop.Sell(hero,item.instanceId,out _),"Fast repeat sold missing instance.");
            var overflow=inventory.Grant(definition.stableId);wallet.Restore(long.MaxValue);Check(!shop.Sell(hero,overflow.instanceId,out _)&&inventory.Find(overflow.instanceId)!=null&&wallet.Gold==long.MaxValue,"Wallet overflow consumed item.");wallet.Restore(beforeGold+price);
            var npc=UnityEngine.Object.FindObjectsByType<VillageVisualIdle>().First();var visual=npc.Visual;var before=visual.position;
            float end=Time.time+1;int frame=Time.frameCount;while(Time.time<end)yield return null;
            Check(Time.frameCount>frame+1,"Player loop frozen.");Check(Vector3.Distance(before,visual.position)>.002f||Quaternion.Angle(visual.localRotation,Quaternion.identity)>1,"NPC idle invisible.");
            Check(npc.GetComponent<Collider>().bounds.size.y>1,"NPC capsule impractical.");
            var tree=UnityEngine.Object.FindObjectsByType<TreeCameraOccluder>().First();var mesh=tree.GetComponentsInChildren<Renderer>().First(r=>r.enabled);var originals=mesh.sharedMaterials;
            var fader=new GameObject("Isolated fade test").AddComponent<CameraOcclusionFader>();var center=mesh.bounds.center;
            fader.UpdateOcclusion(center-Vector3.forward*20,center+Vector3.forward*20);Check(mesh.sharedMaterials[0]!=originals[0],"Tree canopy did not ghost.");
            fader.UpdateOcclusion(center+Vector3.right*100,center+Vector3.right*100+Vector3.up);Check(mesh.sharedMaterials.SequenceEqual(originals),"Tree did not restore opacity.");
            fader.UpdateOcclusion(center-Vector3.forward*20,center+Vector3.forward*20);var enchanted=new Material(originals[0]);enchanted.color=Color.blue;var changed=mesh.sharedMaterials;changed[0]=enchanted;mesh.sharedMaterials=changed;
            fader.UpdateOcclusion(center-Vector3.forward*20,center+Vector3.forward*20);fader.UpdateOcclusion(center+Vector3.right*100,center+Vector3.right*100+Vector3.up);Check(mesh.sharedMaterials[0]==enchanted,"Ghost restore overwrote puzzle enchantment.");
            mesh.sharedMaterials=originals;UnityEngine.Object.Destroy(enchanted);UnityEngine.Object.Destroy(fader.gameObject);
            var portal=UnityEngine.Object.FindObjectsByType<DungeonPortal>().Single(p=>p.isActiveAndEnabled);
            Check(hero.Motor.Teleport(portal.ApproachPosition+Vector3.back*1.5f),"Cave approach unreachable.");
            Check(portal.CanInteract(hero),"I interaction blocked at original cave.");
            var collider=portal.transform.Find("Playtest cave interaction").GetComponent<Collider>();Check(Physics.Raycast(collider.bounds.center+Vector3.up*6,Vector3.down,out var hit,8,1<<11)&&hit.collider==collider,"Cave click hitbox missing.");
            var clickOrigin=collider.bounds.center+Vector3.back*6+Vector3.up*4;
            var clickRay=new Ray(clickOrigin,(collider.bounds.center-clickOrigin).normalized);
            Physics.Raycast(clickRay,out var picked,1500,(1<<8)|(1<<9)|(1<<10)|(1<<11),QueryTriggerInteraction.Ignore);
            Check(hero.GetComponent<Interactor>().ContextInteract(clickRay),"Right-click cave order failed; hit="+picked.collider?.name);
            do{yield return null;}while(SlimeDungeonRun.Current==null);
            var run=SlimeDungeonRun.Current;do{yield return null;}while(!run.StagingReady);
            var dummy=UnityEngine.Object.FindObjectsByType<DungeonSlime>().Single(s=>s.Role==SlimeRole.Dummy);
            Check(dummy.GetComponentsInChildren<Renderer>().Any(r=>r.enabled&&r.bounds.size.magnitude>1),"Practice dummy not visibly rendered.");
            int xp=hero.GetComponent<ExperienceProgression>().CurrentXp;long gold=hero.GetComponent<GoldWallet>().Gold;float hp=dummy.Actor.Health.Current;
            dummy.Actor.Health.ApplyDamage(hero,new DamageResult{mitigated=hp*2});Check(dummy.Actor.Alive&&dummy.Actor.Health.Current==hp&&dummy.Actor.Health.Invulnerable,"Dummy damage/defeat allowed.");
            Check(hero.Motor.Teleport(dummy.transform.position+Vector3.back*2),"Dummy approach blocked.");Check(hero.GetComponent<BasicAttack>().Order(dummy.Actor),"Dummy not attackable.");
            hero.GetComponent<BasicAttack>().Cancel();var combat=hero.GetComponent<CombatInput>();var targeting=hero.GetComponent<SkillTargetingController>();hero.GetComponent<TargetSelection>().Select(dummy.Actor);
            KeyPress(UnityEngine.InputSystem.Key.X,combat);Check(targeting.Mode==SkillTargetingMode.AttackMove&&hero.GetComponent<BasicAttack>().Target==null,"X attacked instead of entering reticle.");
            Click(new Vector2(25,Screen.height-100),targeting);Check(targeting.Active&&hero.GetComponent<BasicAttack>().Target==null,"UI click confirmed attack.");
            KeyPress(UnityEngine.InputSystem.Key.Escape,targeting);Check(!targeting.Active,"Escape did not cancel reticle.");yield return null;KeyPress(UnityEngine.InputSystem.Key.X,combat);
            end=Time.time+1;while(Time.time<end)yield return null;
            Capture("staging-dummy");Click(Camera.main.WorldToScreenPoint(dummy.GetComponent<Collider>().bounds.center),targeting);
            Check(!targeting.Active&&hero.GetComponent<BasicAttack>().Target==dummy.Actor,"Reticle enemy click did not attack.");
            end=Time.time+3;while(Time.time<end)yield return null;
            Check(hero.GetComponent<BasicAttack>().Hits+hero.GetComponent<BasicAttack>().Misses>0,"No dummy attack resolved.");Check(hero.GetComponent<ExperienceProgression>().CurrentXp==xp&&hero.GetComponent<GoldWallet>().Gold==gold,"Dummy granted rewards.");
            hero.GetComponent<BasicAttack>().Cancel();KeyPress(UnityEngine.InputSystem.Key.X,combat);
            var ground=SlimeDungeonRun.Origin+new Vector3(12,0,6);Click(Camera.main.WorldToScreenPoint(ground),targeting);
            Check(!targeting.Active&&combat.AttackMoving,"Ground click did not initiate attack-move.");
            yield return null;combat.SendMessage("Update");Check(hero.GetComponent<BasicAttack>().Target==dummy.Actor,"Attack-move did not acquire nearby hostile.");KeyPress(UnityEngine.InputSystem.Key.Escape,combat);Check(!combat.AttackMoving&&hero.GetComponent<BasicAttack>().Target==null,"Escape failed to cancel attack-move.");
            Check(!run.StartNow(dummy.Actor)&&!run.StartNow(null),"Non-initiator can start.");Check(!hero.GetComponent<RunLoadoutLock>().Locked,"Staging loadout already locked.");
            Check(run.StartNow(hero)&&run.Active&&hero.GetComponent<RunLoadoutLock>().Locked,"Early start failed to lock lease/loadout.");Check(!run.StartNow(hero),"Duplicate start allowed.");
            hero.Health.Invulnerable=true;
            var mini=UnityEngine.Object.FindObjectsByType<DungeonSlime>().Single(s=>s.Role==SlimeRole.Miniboss);
            Check(Mathf.Approximately(mini.Actor.Health.Maximum,800),"Miniboss not 5x HP.");
            Check(Mathf.Approximately(mini.transform.localScale.x,2.3f*3)&&Mathf.Approximately(mini.Actor.Radius,.65f*2.3f*3),"Miniboss size/hit radius must scale 3x.");
            Check(run.Tuning.boss.baseHp==3000&&run.Tuning.regent.baseHp==16000,"Normal/Regent boss health changed incorrectly.");
            var trash=UnityEngine.Object.FindObjectsByType<DungeonSlime>().First(s=>s.Role==SlimeRole.Trash);trash.GetComponent<DiceFree.AI.AggroBehaviour>().enabled=false;
            Check(trash.Actor.Motor.Teleport(hero.transform.position+Vector3.forward*1.5f),"Attack animation fixture unreachable.");
            var trashVisual=trash.transform.Find("Presentation");var trashScale=trashVisual.localScale;var trashAttack=trash.GetComponent<BasicAttack>();Check(trashAttack.Order(hero),"Regular slime attack order failed.");
            do{yield return null;}while(trashAttack.State!="Wind-up");yield return null;Check(trashVisual.localScale.y<trashScale.y*.9f,"Regular slime attack has no visible squash.");trashAttack.enabled=false;
            Check(hero.Motor.Teleport(mini.transform.position+Vector3.back*3),"Boss animation fixture unreachable.");
            var miniVisual=mini.transform.Find("Presentation");var miniScale=miniVisual.localScale;
            do{yield return null;}while(!mini.Busy);yield return null;Check(miniVisual.localScale!=miniScale,"Boss Slam anticipation has no visual deformation.");
            do{yield return null;}while(mini.Busy);Check(Vector3.Distance(miniVisual.localScale,miniScale)<.01f,"Slam did not reform presentation.");
            mini.Actor.Health.SetEncounterHp(mini.Actor.Health.Maximum*.69f);do{yield return null;}while(!mini.Dividing);
            var fragments=UnityEngine.Object.FindObjectsByType<DungeonSlime>().Where(s=>s.Role==SlimeRole.Fragment).ToArray();Check(fragments.Length==2&&fragments.All(f=>f.transform.localScale.x==1.6f&&Mathf.Approximately(f.Actor.Health.Maximum,300)),"Boss fragments must have exactly 300 HP.");
            fragments[0].Actor.Health.ApplyDamage(hero,new DamageResult{mitigated=999});do{yield return null;}while(mini.Dividing);Check(Vector3.Distance(miniVisual.localScale,miniScale)<.01f,"Divide did not visually reform.");
            long savedGold=wallet.Gold;var options=UnityEngine.Object.FindFirstObjectByType<OptionsPanel>();options.Show();
            Check(options.RequestReturnToStartMenu(),"Options save-and-return failed.");
            StartMenuController menu;do{yield return null;menu=UnityEngine.Object.FindFirstObjectByType<StartMenuController>();}while(menu==null||!menu.HasLoadedProfile);
            Check(!menu.LoadBlocked&&SlimeDungeonRun.Current==null,"Start menu failed to load saved progress or clear run.");Check(menu.TryPlayManifestation(original.stableId),"Saved manifestation failed to resume.");
            do{yield return null;hero=UnityEngine.Object.FindFirstObjectByType<TraversalInput>()?.GetComponent<CombatActor>();}while(hero==null||!hero.GetComponent<ManifestationPersistence>().Ready||!hero.Motor.Ready);
            Check(hero.GetComponent<GoldWallet>().Gold==savedGold&&hero.GetComponent<CarriedInventory>().Find(item.instanceId)==null,"Return/reload lost wallet or resurrected sold item.");
            Check(hero.GetComponent<CodexProgression>().Monsters.Single(m=>m.monsterId=="enemy.crop-slime").kills==kills+1,"Return/reload lost Codex.");
            var driver=hero.GetComponent<NovicePresentationDriver>();var animator=driver.VisualAnimator;var arm=animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);var elbow=animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            end=Time.time+.5f;while(Time.time<end)yield return null;
            Check(Vector3.Dot((elbow.position-arm.position).normalized,Vector3.down)>.8f,"Novice arm still in T pose.");var pose=arm.rotation;end=Time.time+1;while(Time.time<end)yield return null;Check(Quaternion.Angle(pose,arm.rotation)>.1f,"Novice idle arm motion absent.");
            portal=UnityEngine.Object.FindObjectsByType<DungeonPortal>().Single(p=>p.isActiveAndEnabled);hero.Motor.Teleport(portal.ApproachPosition+Vector3.back*1.5f);KeyPress(UnityEngine.InputSystem.Key.I,hero.GetComponent<TraversalInput>());yield return null;
            Check(SlimeDungeonRun.Current!=null,"I interaction failed, or return to menu leaked occupied dungeon lease.");
            run=SlimeDungeonRun.Current;do{yield return null;}while(!run.StagingReady);run.Abandon();yield return null;
        }
    }
}
