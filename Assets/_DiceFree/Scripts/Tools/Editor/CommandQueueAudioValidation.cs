using System;
using DiceFree.Characters;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.EditorTools
{
    public static class CommandQueueAudioValidation
    {
        private static void Require(bool ok,string error)
        {
            if(!ok) throw new InvalidOperationException(error);
        }

        [CliCommand("dicefree.commands.audio.runtime-smoke",
            "Check audio imports and shift-order queue semantics in Play Mode.")]
        public static object RuntimeSmoke()
        {
            Require(EditorApplication.isPlaying,"Play Mode required");
            var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            Require(player!=null,"Player absent");
            var queue=player.GetComponent<PlayerCommandQueue>();
            var audio=player.GetComponent<PlayerAudioDirector>();
            var combat=player.GetComponent<CombatInput>();
            Require(queue!=null&&combat!=null,"Command queue was not registered");
            Require(audio!=null&&audio.AudioConfigured&&audio.AmbientPlaying,
                "Audio was not configured or ambience is not playing");
            var position=player.transform.position;
            Vector3 first=position+new Vector3(2f,0,0f);
            Vector3 second=position+new Vector3(2f,0,2f);
            bool firstFound=NavMesh.SamplePosition(first,out var a,1.5f,NavMesh.AllAreas);
            bool secondFound=NavMesh.SamplePosition(second,out var b,1.5f,NavMesh.AllAreas);
            Require(firstFound&&secondFound,"No navigable test ground next to player");

            queue.ClearOrders();
            Require(queue.SubmitMove(a.position,false),"Failed to issue base walk");
            Require(queue.SubmitMove(b.position,true),"Failed to append second walk");
            Require(queue.SubmitAction(()=>{SessionState.SetBool(
                "DiceFree.QueuedAction.Fired",true);return true;},true),
                "Failed to append skill-like instant command");
            Require(queue.PendingOrders==3,"Queue must hold 3 ordered commands");
            SessionState.SetBool("DiceFree.QueuedAction.Fired",false);
            SessionState.SetFloat("DiceFree.Queue.LastX",b.position.x);
            SessionState.SetFloat("DiceFree.Queue.LastZ",b.position.z);
            return new{success=true,queuedCommands=queue.PendingOrders,
                audioClips=Resources.LoadAll<AudioClip>("Audio").Length,
                ambienceActive=audio.AmbientPlaying,
                start=position.ToString("F2"),first=a.position.ToString("F2"),
                second=b.position.ToString("F2")};
        }

        [CliCommand("dicefree.commands.audio.runtime-status",
            "Check queued path completion and deferred action order.")]
        public static object RuntimeStatus()
        {
            Require(EditorApplication.isPlaying,"Play Mode required");
            var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            var queue=player.GetComponent<PlayerCommandQueue>();
            Vector3 destination=new Vector3(
                SessionState.GetFloat("DiceFree.Queue.LastX",0),
                player.transform.position.y,
                SessionState.GetFloat("DiceFree.Queue.LastZ",0));
            float remaining=Vector2.Distance(
                new Vector2(player.transform.position.x,player.transform.position.z),
                new Vector2(destination.x,destination.z));
            bool action=SessionState.GetBool("DiceFree.QueuedAction.Fired",false);
            Require(action && remaining<.9f && !queue.HasOrders,
                "Shift queue not finished: remaining="+remaining+
                "m, action="+action+", pending="+queue.PendingOrders);
            Require(queue.SubmitMove(destination+Vector3.left*1.5f,true),
                "Could not add a new queued order");
            Require(queue.PendingOrders==1,"Expected a queued order");
            queue.ClearOrders();
            Require(!queue.HasOrders,"Stop/clear must empty the queue");
            return new{success=true,remainingDistance=remaining,
                deferredActionExecuted=action,
                queueCleared=true};
        }

        [CliCommand("dicefree.commands.skills.enqueue-test",
            "Queue walk, unit skill, ground skill and instant buff in that order.")]
        public static object EnqueueSkills()
        {
            Require(EditorApplication.isPlaying,"Play Mode required");
            var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            var queue=player.GetComponent<PlayerCommandQueue>();
            var owner=player.GetComponent<DiceFree.Combat.CombatActor>();
            var audio=player.GetComponent<PlayerAudioDirector>();
            var pos=player.transform.position;
            var next=pos+new Vector3(2.0f,0,0);
            Require(NavMesh.SamplePosition(next,out var hit,1.5f,NavMesh.AllAreas),
                "No nearby walkable test ground");
            var audioPrefs=audio.GetComponent<IPlayerAudioSettings>();
            Require(audioPrefs!=null,"Sound sliders cannot find player audio settings");
            float originalFx=audioPrefs.SfxLevel;
            float originalAmbient=audioPrefs.AmbientLevel;
            audioPrefs.SetLevels(.32f,.21f);
            Require(Mathf.Abs(audioPrefs.SfxLevel-.32f)<.01f &&
                Mathf.Abs(audioPrefs.AmbientLevel-.21f)<.01f,
                "Audio settings did not take effect");
            audioPrefs.SetLevels(originalFx,originalAmbient);

            SessionState.SetString("DiceFree.QueuedSkills.Trace","");
            queue.ClearOrders();
            Require(queue.SubmitMove(hit.position,false),"Movement could not start");
            Require(queue.SubmitUnit(owner,2f,unit=>{
                SessionState.SetString("DiceFree.QueuedSkills.Trace",
                    SessionState.GetString("DiceFree.QueuedSkills.Trace","")+"U");
                return true;},true),"Could not queue target spell");
            Require(queue.SubmitGround(hit.position,2f,point=>{
                SessionState.SetString("DiceFree.QueuedSkills.Trace",
                    SessionState.GetString("DiceFree.QueuedSkills.Trace","")+"G");
                return true;},true),"Could not queue ground spell");
            Require(queue.SubmitAction(()=>{
                SessionState.SetString("DiceFree.QueuedSkills.Trace",
                    SessionState.GetString("DiceFree.QueuedSkills.Trace","")+"I");
                return true;},true),"Could not queue instant buff");
            Require(queue.PendingOrders==4,"Four orders must be queued");
            return new{success=true,pendingOrders=queue.PendingOrders,
                target=hit.position.ToString("F2"),
                audioLevelsEditable=true};
        }

        [CliCommand("dicefree.commands.skills.queue-status",
            "Verify all queued skill callbacks fired in order after movement.")]
        public static object QueueStatus()
        {
            Require(EditorApplication.isPlaying,"Play Mode required");
            var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            var queue=player.GetComponent<PlayerCommandQueue>();
            string trace=SessionState.GetString("DiceFree.QueuedSkills.Trace","");
            Require(trace=="UGI" && !queue.HasOrders,
                "Wrong queued skill order: "+trace+
                "; remaining="+queue.PendingOrders);
            return new{success=true,executedInOrder=trace,queueEmpty=true};
        }

        [CliCommand("dicefree.commands.attack-move.enqueue-test",
            "Queue attack-move and ensure next order is deferred until it completes.")]
        public static object AttackMoveEnqueueTest()
        {
            Require(EditorApplication.isPlaying,"Play Mode required");
            var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            var queue=player.GetComponent<PlayerCommandQueue>();
            var combat=player.GetComponent<CombatInput>();
            Vector3 dest=player.transform.position+new Vector3(2f,0,0f);
            Require(NavMesh.SamplePosition(dest,out var hit,1.5f,NavMesh.AllAreas),
                "No navigable attack-move test destination");
            SessionState.SetBool("DiceFree.Queue.AfterAttackMove",false);
            queue.ClearOrders();
            Require(queue.SubmitAttackMove(hit.position,combat.OrderAttackMove,false),
                "Could not queue attack-move");
            Require(queue.SubmitAction(()=>{
                SessionState.SetBool("DiceFree.Queue.AfterAttackMove",true);
                return true;},true),"Could not append command behind attack-move");
            Require(queue.PendingOrders==2,"Expected attack-move then deferred action");
            return new{success=true,attackMoveQueued=true,pendingOrders=queue.PendingOrders,
                destination=hit.position.ToString("F2")};
        }

        [CliCommand("dicefree.commands.attack-move.queue-status",
            "Confirm attack-move reaches destination before the next queued order.")]
        public static object AttackMoveStatus()
        {
            Require(EditorApplication.isPlaying,"Play Mode required");
            var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            var queue=player.GetComponent<PlayerCommandQueue>();
            var combat=player.GetComponent<CombatInput>();
            bool finished=SessionState.GetBool("DiceFree.Queue.AfterAttackMove",false);
            Require(finished && !queue.HasOrders && !combat.AttackMoving,
                "Attack-move queue did not finish; action="+finished+
                " remaining="+queue.PendingOrders+" combat="+combat.AttackMoving);
            return new{success=true,attackMoveFinished=true,deferredNextCommandRan=finished};
        }

        [CliCommand("dicefree.commands.attack-move.targeting-regression",
            "Reproduce X then Shift-click while movement is already queued.")]
        public static object AttackMoveTargetingRegression()
        {
            Require(EditorApplication.isPlaying,"Play Mode required");
            var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            var queue=player.GetComponent<PlayerCommandQueue>();
            var combat=player.GetComponent<CombatInput>();
            var targeter=player.GetComponent<SkillTargetingController>();
            Vector3 first=player.transform.position+new Vector3(2f,0,0);
            Vector3 second=player.transform.position+new Vector3(2f,0,2f);
            bool one=NavMesh.SamplePosition(first,out var a,1.5f,NavMesh.AllAreas);
            bool two=NavMesh.SamplePosition(second,out var b,1.5f,NavMesh.AllAreas);
            Require(one&&two,"No nearby navigable waypoints");
            queue.ClearOrders();
            Require(queue.SubmitMove(a.position,false),"First movement cannot queue");
            Require(queue.PendingOrders==1,"Missing first waypoint");
            // Simulate pressing X BEFORE holding Shift to confirm, which used
            // to clear the pending walk in CombatInput.BeginAttackTargeting.
            combat.BeginAttackTargeting();
            Require(queue.PendingOrders==1,
                "Starting attack-move targeting erased a queued walk");
            Require(targeter.Active&&targeter.Mode==SkillTargetingMode.AttackMove,
                "Attack-move cursor did not activate");
            Require(targeter.QueueAttackMoveFromTargeting(b.position),
                "Shift+click could not append attack-move");
            Require(queue.PendingOrders==2 && !targeter.Active,
                "Attack-move did not append in order or failed to exit targeting");
            SessionState.SetFloat("DiceFree.QueuedAttackMove.LastX",b.position.x);
            SessionState.SetFloat("DiceFree.QueuedAttackMove.LastZ",b.position.z);
            return new{success=true,movementPreserved=true,
                queuedAttackMove=true,totalOrders=2};
        }

        [CliCommand("dicefree.commands.attack-move.targeting-status",
            "Confirm queued walk followed by Shift attack-move reaches both targets.")]
        public static object AttackMoveTargetingStatus()
        {
            Require(EditorApplication.isPlaying,"Play Mode required");
            var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            var queue=player.GetComponent<PlayerCommandQueue>();
            var combat=player.GetComponent<CombatInput>();
            float x=SessionState.GetFloat("DiceFree.QueuedAttackMove.LastX",0);
            float z=SessionState.GetFloat("DiceFree.QueuedAttackMove.LastZ",0);
            float d=Vector2.Distance(new Vector2(player.transform.position.x,
                player.transform.position.z),new Vector2(x,z));
            var motor=player.GetComponent<TraversalMotor>();
            var agent=player.GetComponent<NavMeshAgent>();
            var orderField=typeof(PlayerCommandQueue).GetField("current",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            var current=orderField?.GetValue(queue);
            var input=DiceFree.Input.InputBindings.Current;
            Require(d<.95f&&!queue.HasOrders&&!combat.AttackMoving,
                "Queued X+Shift attack-move stalled: dist="+d+
                " pending="+queue.PendingOrders+" current="+(current?.ToString()??"none")+
                " motor="+motor.Travelling+" ready="+motor.Ready+
                " agentPath="+agent.hasPath+" pendingPath="+agent.pathPending+
                " agentStopped="+agent.isStopped+" navDst="+agent.destination+
                " player="+player.transform.position+" speed="+agent.speed+
                " canAct="+player.GetComponent<DiceFree.Combat.CombatActor>().CanAct+
                " suppressed="+input.Suppressed+" timeScale="+Time.timeScale+
                " playPaused="+EditorApplication.isPaused);
            return new{success=true,walkThenAttackMove=true,
                remainingDistance=d};
        }

        [CliCommand("dicefree.commands.audio.catalog-check",
            "Check the selected imported AudioClips exist as assets.")]
        public static object CatalogCheck()
        {
            Require(!EditorApplication.isPlaying,"Edit Mode required");
            string[] required={"CornbergBirds","StepGrass","StepWood",
                "WeaponSwing","WeaponImpact","Heal","MagicSand","MagicIce",
                "MagicFire","Buff","PlayerHurt","LevelUp","UiSelect"};
            foreach(var name in required)
            {
                var asset=AssetDatabase.LoadAssetAtPath<AudioClip>(
                    "Assets/_DiceFree/Resources/Audio/"+name+".wav");
                Require(asset!=null,"Missing imported sound "+name);
            }
            return new{success=true,clipsValidated=required.Length};
        }
    }
}
