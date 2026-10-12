using DiceFree.Combat;
using DiceFree.Foundation;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>
    /// All classes, including Novice, have the same Return to Revive Stone skill.
    /// This is travel while alive, not the separate death-respawn action.
    /// Cast is interrupted by movement, incoming damage, stun or death.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor),typeof(RespawnAtAnchor))]
    public sealed class ReturnHomeAbility : MonoBehaviour
    {
        public const float CooldownDuration = 600f;
        public const float CastDuration = 10f;
        private const string ChannelBlock="skill.return-home";
        private CombatActor actor;
        private RespawnAtAnchor anchor;
        private float readyAt;
        private float finishAt;
        private Vector3 castingPosition;
        public bool Casting { get; private set; }
        public float CooldownRemaining => Mathf.Max(0f,readyAt-Time.time);
        public float CastRemaining => Casting ? Mathf.Max(0f,finishAt-Time.time) : 0f;
        public string Feedback { get; private set; } = "";

        private void Awake()
        {
            actor=GetComponent<CombatActor>();
            anchor=GetComponent<RespawnAtAnchor>();
        }
        private void OnEnable()
        {
            if(actor!=null && actor.Health!=null)
            {
                actor.Health.Damaged+=OnDamaged;
                actor.Health.Died+=OnDeath;
            }
        }
        private void OnDisable()
        {
            if(actor!=null && actor.Health!=null)
            {
                actor.Health.Damaged-=OnDamaged;
                actor.Health.Died-=OnDeath;
            }
            Cancel();
        }
        private void OnDamaged(CombatActor source,DamageResult damage) => Cancel();
        private void OnDeath()
        {
            Cancel();
            readyAt=0; // Issue #38: death resets Return Home cooldown.
        }

        private bool HasActiveThreat
        {
            get
            {
                if(actor==null)return false;
                if(actor.InCombat)return true;
                foreach(var hostile in CombatActor.All)
                {
                    if(hostile==null || !hostile.Alive || !hostile.IsHostileTo(actor))continue;
                    foreach(var component in hostile.GetComponents<MonoBehaviour>())
                        if(component is IActiveThreatSource source &&
                           source.Threatens(actor))return true;
                }
                return false;
            }
        }

        public bool CanBegin =>
            !Casting && CooldownRemaining<=0.001f && actor!=null &&
            actor.CanAct && anchor!=null && anchor.HasHomeAnchor &&
            !IsInsideDungeon && !HasActiveThreat;
        private bool IsInsideDungeon =>
            GetComponent<RunLoadoutLock>()?.InsideDungeon==true;

        public bool TryBegin()
        {
            if(!CanBegin)
            {
                Feedback=CooldownRemaining>0f?"Return Home is on cooldown.":
                    IsInsideDungeon?"Cannot return home from inside a dungeon.":
                    HasActiveThreat?"Cannot return home while enemies have aggro.":
                    "Return Home unavailable.";
                return false;
            }
            Casting=true;
            castingPosition=transform.position;
            finishAt=Time.time+CastDuration;
            actor.Motor.Stop();
            actor.Motor.SetMotionBlocked(ChannelBlock,true);
            GetComponent<BasicAttack>()?.Cancel();
            GetComponent<Interactor>()?.Cancel();
            foreach(var component in GetComponents<MonoBehaviour>())
                if(component is ISharedCombatCommands commands)
                {commands.CancelAttackMove();break;}
            GetComponent<SkillTargetingController>()?.Cancel();
            foreach(var component in GetComponents<MonoBehaviour>())
                if(component is IQueuedOrders queued){queued.ClearOrders();break;}
            Feedback="Returning to "+anchor.AnchorName+"…";
            return true;
        }

        public void Cancel()
        {
            if(!Casting)return;
            Casting=false;
            if(actor!=null)actor.Motor.SetMotionBlocked(ChannelBlock,false);
            Feedback="Return Home interrupted.";
        }
        private void Update()
        {
            if(!Casting)return;
            if(actor==null || !actor.CanAct || IsInsideDungeon || HasActiveThreat ||
               Vector3.ProjectOnPlane(transform.position-castingPosition,
                   Vector3.up).sqrMagnitude>.06f)
            {
                Cancel();
                return;
            }
            if(Time.time<finishAt)return;
            Casting=false;
            actor.Motor.SetMotionBlocked(ChannelBlock,false);
            if(anchor.TeleportHome())
            {
                readyAt=Time.time+CooldownDuration;
                Feedback="Returned to "+anchor.AnchorName+".";
            }
            else Feedback="Revive stone unavailable; no cooldown consumed.";
        }
    }
}
