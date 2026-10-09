using System;
using System.IO;
using System.Linq;
using DiceFree.Dungeons;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.Characters;
using DiceFree.Foundation;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class SlimeDungeonValidation
    {
        const string Key="DiceFree.Slime.Validation.";
        static int phase,captureCount;static double deadline;static float actionAt,storedHp;static int baselineXp;static long baselineGold;
        static CombatActor player;static DungeonSlime boss,pulled;static bool testRegent,testWipe;static string testItem;static Vector3 snapshot;
        static SlimeDungeonValidation(){EditorApplication.playModeStateChanged+=Changed;}
        [CliCommand("dicefree.slime.validate","Validate occupancy, reward probabilities, item stats and dungeon scene authoring.")]
        public static object Validate()
        {
            var d=AssetDatabase.LoadAssetAtPath<DungeonDefinition>("Assets/_DiceFree/Settings/World/Dungeon - Slime.asset");
            var t=AssetDatabase.LoadAssetAtPath<SlimeDungeonTuning>("Assets/_DiceFree/Settings/Dungeons/Slime playtest tuning.asset");
            Require(d!=null&&t!=null&&d.stagingSeconds==60,"Missing authored definition or 60-second staging");
            Require(t.blue.baseHp==25*t.puzzleGreen.baseHp,"Blue HP must be exactly 25x");
            Require(t.bossPresentation!=null&&t.regentPresentation!=null,"Existing boss art not wired");
            var registry=new SessionMapRegistry();Require(registry.TryClaimDungeonStaging(d.stableId,"party-a","a",DateTime.UtcNow,out var first,out _),"First claim");
            Require(registry.TryClaimDungeonStaging(d.stableId,"party-a","b",DateTime.UtcNow,out var joined,out _)&&joined.leaseToken==first.leaseToken,"Party must share occupancy");
            Require(!registry.TryClaimDungeonStaging(d.stableId,"party-b","c",DateTime.UtcNow,out _,out _),"Other party must be refused");
            var variant=DungeonConditionEvaluator.Evaluate(d,new DungeonConditionContext{participants=new[]{new DungeonParticipantSnapshot{level=50}}});Require(variant.HasRoute("route.slime-regent"),"Exactly level 50 must qualify");
            Require(registry.TryBeginDungeon(d.stableId,first.leaseToken,variant,DateTime.UtcNow,out _,out _),"Begin lifecycle");
            Require(!registry.TryClaimDungeonStaging(d.stableId,"party-a","late",DateTime.UtcNow,out _,out _),"Late join must fail");
            var randomState=UnityEngine.Random.state;UnityEngine.Random.InitState(54);int rare=0,two=0;const int n=20000;
            for(int i=0;i<n;i++){var offer=t.normalRewards.Roll();if(offer.Contains(t.normalRewards.rare)){Require(offer.Length==1,"Hat override must be sole option");rare++;}else if(offer.Length==2){Require(offer[0]!=offer[1],"Duplicate offer");two++;}}
            UnityEngine.Random.state=randomState;Require(Math.Abs((float)rare/n-.05f)<.01f,"Rare distribution");Require(Math.Abs((float)two/(n-rare)-.25f)<.02f,"Two-option distribution");
            Require(t.normalRewards.rare.stats.regeneration==10,"Tophat regen missing");Require(t.normalRewards.items.Any(i=>i.stats.basicAttackMaximumBonus==1),"Orb upper-end stat missing");
            Require(EditorBuildSettings.scenes.Any(s=>s.enabled&&s.path.EndsWith("/SlimeDungeon.unity"))&&EditorBuildSettings.scenes.Any(s=>s.enabled&&s.path.EndsWith("/DungeonRewardRoom.unity")),"Separate maps missing build entries");
            return new{success=true,marker="SLIME_DUNGEON_DATA_OK",rolls=n,rare,two};
        }
        [CliCommand("dicefree.slime.playtest","Start isolated-save real 60-second staging and dungeon lifecycle test; poll dicefree.slime.status.")]
        public static object Start([CliArg("regent","Test secret Regent instead of normal boss")]bool regent=false,[CliArg("wipe","Test full wipe and Cornberg return after staging")]bool wipe=false)
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Editor must be stopped");
            SessionState.SetBool(Key+"regent",regent);SessionState.SetBool(Key+"options",EditorSettings.enterPlayModeOptionsEnabled);SessionState.SetInt(Key+"flags",(int)EditorSettings.enterPlayModeOptions);
            SessionState.SetBool(Key+"wipe",wipe);
            EditorSettings.enterPlayModeOptionsEnabled=false;
            string root=Path.Combine(Path.GetTempPath(),"DiceFree-Slime-"+Guid.NewGuid().ToString("N"));PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);SessionState.SetString(Key+"root",root);
            SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"error","");EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);EditorApplication.EnterPlaymode();return Status();
        }
        [CliCommand("dicefree.slime.status","Read the real dungeon Play Mode validation result.")]
        public static object Status()=>new{status=SessionState.GetString(Key+"status","idle"),success=SessionState.GetString(Key+"status","")=="passed",phase=phase,error=SessionState.GetString(Key+"error",""),saveRoot=SessionState.GetString(Key+"root","")};
        static void Changed(PlayModeStateChange state)
        {
            if(SessionState.GetString(Key+"status","")!="running")return;
            if(state==PlayModeStateChange.EnteredPlayMode){Application.runInBackground=true;phase=0;captureCount=0;testRegent=SessionState.GetBool(Key+"regent",false);testWipe=SessionState.GetBool(Key+"wipe",false);deadline=EditorApplication.timeSinceStartup+180;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
            if(state==PlayModeStateChange.EnteredEditMode){EditorApplication.update-=Tick;Application.logMessageReceived-=Log;RestoreOptions();}
        }
        static void RestoreOptions(){EditorSettings.enterPlayModeOptions= (EnterPlayModeOptions)SessionState.GetInt(Key+"flags",0);EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"options",false);}
        static void Log(string message,string stack,LogType type){if(type==LogType.Exception||type==LogType.Error)Fail(message+"\n"+stack);}
        static void Tick()
        {
            if(!EditorApplication.isPlaying||SessionState.GetString(Key+"status","")!="running")return;
            try
            {
                Require(EditorApplication.timeSinceStartup<deadline,"Playtest timeout");
                if(phase==0)
                {
                    var input=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();if(input==null)return;player=input.GetComponent<CombatActor>();
                    if(!player.Motor.Ready||!player.GetComponent<ManifestationPersistence>().Ready)return;
                    player.GetComponent<ExperienceProgression>().RestoreState(testRegent?50:5,0);player.Health.Restore();
                    var portal=UnityEngine.Object.FindObjectsByType<DungeonPortal>().Single(p=>p.isActiveAndEnabled);Require(portal!=null,"Cornberg portal missing");
                    Require(player.Motor.Teleport(portal.ApproachPosition+Vector3.back*1.5f),"Portal approach navigation");
                    portal.Interact(player);Require(SlimeDungeonRun.Current!=null,"Interactable portal failed to enter");phase=1;return;
                }
                var run=SlimeDungeonRun.Current;
                if(phase==1)
                {
                    var dummy=Slimes().FirstOrDefault(s=>s.Role==SlimeRole.Dummy);if(dummy==null)return;
                    Require(!run.Active&&!player.GetComponent<RunLoadoutLock>().Locked,"Staging incorrectly skipped/locked");if(testWipe){player.GetComponent<ExperienceProgression>().Grant(1);player.GetComponent<GoldWallet>().Grant(17);}baselineXp=player.GetComponent<ExperienceProgression>().CurrentXp;baselineGold=player.GetComponent<GoldWallet>().Gold;
                    var gloves=run.Tuning.normalRewards.items.Single(i=>i.slot==EquipmentSlot.Hands);testItem=player.GetComponent<CarriedInventory>().Grant(gloves.stableId).instanceId;
                    Require(player.GetComponent<Equipment>().Equip(testItem,EquipmentSlot.Hands),"Staging gear change blocked");Require(player.GetComponent<Equipment>().Unequip(EquipmentSlot.Hands.ToString()),"Staging unequip blocked");
                    if(testRegent)
                    {
                        Vector3 stagingPosition=player.transform.position;var persistence=player.GetComponent<ManifestationPersistence>();
                        var target=AssetDatabase.LoadAssetAtPath<ActorDefinition>("Assets/_DiceFree/Settings/Progression/Physically Blessed Novice shell.asset");
                        Require(persistence.TryForkAndActivate(target,out var classError),"Staging class preparation failed: "+classError);
                        Require(Vector3.Distance(player.transform.position,stagingPosition)<1,"Staging class change escaped to overworld");
                        Require(persistence.TryActivateExisting("class.novice",out classError),"Staging switch back failed: "+classError);
                        Require(Vector3.Distance(player.transform.position,stagingPosition)<1,"Staging class switch escaped to overworld");
                    }
                    Require(player.Motor.Teleport(dummy.transform.position+Vector3.back*1.5f),"Practice dummy approach");Require(player.GetComponent<BasicAttack>().Order(dummy.Actor),"Dummy not attackable");actionAt=Time.time+3;phase=2;return;
                }
                if(phase==2)
                {
                    if(Time.time<actionAt)return;Require(player.GetComponent<BasicAttack>().Hits>0,"Existing basic attack did not hit practice dummy");
                    var skills=player.GetComponent<DiceFree.Skills.NoviceSkillProgression>();
                    skills.RestoreState(new[]{new DiceFree.Skills.SkillRankState(NoviceSkillsAuthoring.StrengthId,1)});
                    var dummy=Slimes().Single(s=>s.Role==SlimeRole.Dummy);
                    Require(player.GetComponent<NoviceSkillCaster>().Cast(skills.Definition(NoviceSkillsAuthoring.StrengthId),dummy.Actor),"Existing class skill failed against dungeon practice target");
                    Require(player.GetComponent<ExperienceProgression>().CurrentXp==baselineXp&&player.GetComponent<GoldWallet>().Gold==baselineGold,"Dummy farming rewards");player.GetComponent<BasicAttack>().Cancel();player.Health.Invulnerable=true;phase=3;return;
                }
                if(phase==3)
                {
                    if(!run.Active)return;Require(player.GetComponent<RunLoadoutLock>().Locked,"Run loadout did not lock");Require(run.SecretEligible==testRegent,"Variant not snapshotted correctly");
                    Require(!player.GetComponent<Equipment>().Equip(testItem,EquipmentSlot.Hands),"Active run gear mutation allowed");Require(!player.GetComponent<ManifestationPersistence>().TryActivateExisting("class.novice",out _),"Active class mutation allowed");
                    if(testWipe){player.Health.Invulnerable=false;player.Health.ApplyDamage(null,new DamageResult{mitigated=player.Health.Maximum*2});phase=20;return;}
                    boss=Slimes().Single(s=>s.Role==SlimeRole.Miniboss);player.Motor.Teleport(boss.transform.position+Vector3.back*3);boss.Actor.Health.SetEncounterHp(boss.Actor.Health.Maximum*.69f);storedHp=boss.Actor.Health.Current;phase=4;return;
                }
                if(phase==4)
                {
                    if(!boss.Dividing)return;var fragments=Slimes().Where(s=>s.Role==SlimeRole.Fragment).ToArray();Require(fragments.Length==2&&fragments.All(f=>f.Actor.Health.Maximum==300),"Miniboss two-way Divide must use 300 HP fragments");Require(Mathf.Approximately(boss.transform.localScale.x,2.3f*3),"Miniboss actual scale must be 3x");Kill(fragments[0]);phase=5;return;
                }
                if(phase==5)
                {
                    if(boss.Dividing)return;Require(Math.Abs(boss.Actor.Health.Current-storedHp)<.05f,"Successful Divide did not restore pre-Divide HP");
                    boss.Actor.Health.SetEncounterHp(boss.Actor.Health.Maximum*.29f);storedHp=boss.Actor.Health.Current;phase=6;return;
                }
                if(phase==6){if(!boss.Dividing)return;Require(boss.ThresholdsConsumed==2,"Second threshold missing");actionAt=Time.time;phase=7;return;}
                if(phase==7)
                {
                    if(boss.Dividing)return;Require(Time.time-actionAt>=8.5f,"Fragment drift reunited too early");Require(boss.Actor.Health.Current>storedHp,"Failed Divide did not heal");boss.Actor.Health.SetEncounterHp(boss.Actor.Health.Maximum*.25f);actionAt=Time.time+1;phase=8;return;
                }
                if(phase==8)
                {
                    if(Time.time<actionAt)return;Require(!boss.Dividing&&boss.ThresholdsConsumed==2,"Consumed threshold repeated after healing");Kill(boss);phase=9;return;
                }
                if(phase==9)
                {
                    var puzzle=UnityEngine.Object.FindFirstObjectByType<SlimeTreePuzzle>();var free=Slimes().Where(s=>s.Role==SlimeRole.Puzzle&&!s.Captured&&s.Actor.Alive).ToArray();if(free.Length<3)return;
                    Require(free.Length==3&&free.Count(s=>s.Blue)==(testRegent?1:0),"Puzzle free/color invariant");
                    if(captureCount==0)
                    {
                        pulled=testRegent?free.Single(s=>s.Blue):free[0];
                        foreach(var other in free.Where(s=>s!=pulled))other.GetComponent<AggroBehaviour>().enabled=false;
                        Require(pulled.GetComponent<AggroBehaviour>().AutoAggroPolicy.Allows(pulled.Actor,player),"Puzzle acquisition incorrectly suppressed at high level");
                        Require(player.Motor.Teleport(pulled.transform.position+Vector3.back*3),"Ordinary lure approach failed");actionAt=Time.time+1;phase=21;return;
                    }
                    if(captureCount<5){var slime=testRegent?free.Single(s=>s.Blue):free[0];var empty=puzzle.rings.First(r=>!Slimes().Any(s=>s.Captured&&Vector3.Distance(s.transform.position,r.position)<.7f));slime.GetComponent<AggroBehaviour>().enabled=false;slime.GetComponent<BasicAttack>().enabled=false;Require(slime.Actor.Motor.Teleport(empty.position),"Ring navigation");captureCount++;phase=10;return;}
                }
                if(phase==10)
                {
                    var puzzle=UnityEngine.Object.FindFirstObjectByType<SlimeTreePuzzle>();if(puzzle.Filled<captureCount)return;
                    if(captureCount<5){phase=9;return;}
                    Require(run.PuzzleSolved&&run.SecretSolved==testRegent,"Puzzle branch result");Require(!Slimes().Any(s=>s.Role==SlimeRole.Puzzle&&!s.Captured),"Free slimes remained after solve");
                    boss=Slimes().Single(s=>s.Role==(testRegent?SlimeRole.Regent:SlimeRole.Boss));player.Motor.Teleport(boss.transform.position+Vector3.back*4);boss.Actor.Health.SetEncounterHp(boss.Actor.Health.Maximum*.69f);storedHp=boss.Actor.Health.Current;phase=11;return;
                }
                if(phase==11)
                {
                    if(!boss.Dividing)return;var fragments=Slimes().Where(s=>s.Role==SlimeRole.Fragment).ToArray();Require(fragments.Length==(testRegent?4:3)&&fragments.All(f=>Mathf.Approximately(f.Actor.Health.Maximum,testRegent?1600f:300f)),"Final boss Divide count / expected fragment health");Require(Mathf.Approximately(boss.Actor.Health.Maximum,testRegent?16000f:3000f),"Actual final boss HP incorrect");actionAt=Time.time+2.4f;phase=12;return;
                }
                if(phase==12)
                {
                    if(Time.time<actionAt)return;Require(Slimes().Any(s=>s.Role==SlimeRole.Add),"Uncapped phase spawning did not start");var fragments=Slimes().Where(s=>s.Role==SlimeRole.Fragment&&s.Actor.Alive).ToArray();foreach(var f in fragments.Skip(1))Kill(f);phase=13;return;
                }
                if(phase==13)
                {
                    if(boss.Dividing)return;Require(Math.Abs(boss.Actor.Health.Current-storedHp)<.1f,"Final Divide success HP");Require(Slimes().Any(s=>s.Role==SlimeRole.Add&&s.Actor.Alive),"Adds did not persist after phase");
                    if(testRegent){snapshot=player.transform.position;Require(boss.TryCastRegentAttack(true),"Regent rolling cast failed");actionAt=Time.time+2;player.Motor.Teleport(snapshot+Vector3.right*6);phase=15;return;}
                    Complete();return;
                }
                if(phase==14)
                {
                    var room=UnityEngine.Object.FindFirstObjectByType<DungeonRewardRoom>();if(room==null||room.Pending==0)return;Require(player.transform.position.x>29000,"Reward scene transfer missing");Require(!player.GetComponent<RunLoadoutLock>().Locked,"Completion loadout remained locked");
                    int before=player.GetComponent<CarriedInventory>().Items.Length;Require(room.Choose(player,testRegent?-1:0),"Independent reward choice failed");if(!testRegent)Require(player.GetComponent<CarriedInventory>().Items.Length==before+1,"Reward item not granted");Require(player.transform.position.x<1000,"Entrance return failed");Require(!player.GetComponent<RunLoadoutLock>().Locked,"Return did not unlock");Pass();
                }
                if(phase==15)
                {
                    if(Time.time<actionAt)return;Require(GameObject.Find("Static danger telegraph")!=null,"Regent damaging trail missing");if(boss.Busy)return;
                    Require(Vector3.Distance(boss.LastCastSnapshot,snapshot)<.1f,"Regent roll did not snapshot cast start");Require(Vector3.Distance(boss.LastImpactPosition,snapshot)<1,$"Regent roll endpoint {boss.LastImpactPosition} differs from snapshot {snapshot}");snapshot=player.transform.position;Require(boss.TryCastRegentAttack(false),"Regent bouncing cast failed");
                    player.Motor.Teleport(snapshot+Vector3.right*3);actionAt=Time.time+3;phase=16;return;
                }
                if(phase==16){if(Time.time<actionAt||boss.Busy)return;Require(Vector3.Distance(boss.LastCastSnapshot,snapshot)<.1f,"Regent bounce did not snapshot cast start");Require(Vector3.Distance(boss.LastImpactPosition,snapshot)<1,$"Regent bounce endpoint {boss.LastImpactPosition} differs from snapshot {snapshot}");Complete();}
                if(phase==20)
                {
                    if(run!=null)return;Require(player.Alive&&player.transform.position.x<1000,"Wipe did not return to Cornberg alive");
                    Require(player.GetComponent<ExperienceProgression>().CurrentXp==baselineXp&&player.GetComponent<GoldWallet>().Gold==baselineGold,"Wipe lost previously earned XP/gold");
                    Require(!player.GetComponent<RunLoadoutLock>().Locked&&player.GetComponent<RunLoadoutLock>().Owner==null,"Wipe did not release participation");
                    Require(player.GetComponent<DiceFree.Gameplay.CodexProgression>().Dungeons.Any(d=>d.dungeonId=="dungeon.slime"&&d.wipes==1&&d.playerDeaths==1),"Wipe/death semantic events missing");Pass();
                }
                if(phase==21)
                {
                    if(Time.time<actionAt)return;Require(pulled.GetComponent<BasicAttack>().Target==player,"Ordinary puzzle aggro did not acquire player");
                    var puzzle=UnityEngine.Object.FindFirstObjectByType<SlimeTreePuzzle>();var ring=puzzle.rings[0].position;
                    var goal=ring+(ring-pulled.transform.position).normalized*3;
                    Require(player.Motor.MoveTo(goal),"Existing click-to-move motor cannot lure into ring");actionAt=Time.time+15;phase=22;return;
                }
                if(phase==22){Require(Time.time<actionAt,"Ordinary lure failed to capture slime into ring");if(!pulled.Captured)return;captureCount=1;phase=10;}
            }
            catch(Exception e){Fail(e.ToString());}
        }
        static DungeonSlime[] Slimes()=>UnityEngine.Object.FindObjectsByType<DungeonSlime>(FindObjectsSortMode.None);
        static void Kill(DungeonSlime s)=>s.Actor.Health.ApplyDamage(player,new DamageResult{mitigated=s.Actor.Health.Maximum*2});
        static void Complete(){Kill(boss);Require(SlimeDungeonRun.Current.Completing&&!SlimeDungeonRun.Current.Active,"Final boss did not immediately end combat");phase=14;}
        static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        static void Pass(){SessionState.SetString(Key+"status","passed");Debug.Log("SLIME_DUNGEON_PLAYMODE_OK");Stop();}
        static void Fail(string error){if(SessionState.GetString(Key+"status","")!="running")return;SessionState.SetString(Key+"error",error);SessionState.SetString(Key+"status","failed");Stop();}
        static void Stop(){EditorApplication.update-=Tick;Application.logMessageReceived-=Log;RestoreOptions();EditorApplication.ExitPlaymode();}
    }
}
