using System;
using System.Collections;
using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.Progression;
using DiceFree.Foundation;
using DiceFree.Gameplay;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor),typeof(BerserkerProgression),typeof(ActorResourceController))]
    public sealed class BerserkerCaster : MonoBehaviour, IManifestationSessionState
    {
        public const string RageId="resource.rage";
        public const string ShieldSource="berserker.coffee-shield";
        private const string HasteSource="berserker.breaker-haste", SpinSource="berserker.spin";
        [SerializeField] private AttackDefinition wideArc,spinDash,rageBreaker;
        private CombatActor actor;
        private BerserkerProgression ranks;
        private ActorResourceController resources;
        private BasicAttack basic;
        private IGreatWeaponUser equipment;
        private readonly float[] readyAt=new float[5];
        private float lastCombat,hasteUntil;
        private int casting=-1;
        private bool suspendedBasic,spinPassage;
        private ObstacleAvoidanceType priorAvoidance;
        private GroundAuraVisual shieldSigil;
        public bool Active => isActiveAndEnabled && ranks.Active && actor.Alive;
        public bool Busy => casting>=0;
        public float Rage => resources?.Current(RageId)??0;
        public float HasteRemaining => Mathf.Max(0,hasteUntil-Time.time);
        public int LastCastHits { get; private set; }
        public float LastRageSpent { get; private set; }
        public string Feedback { get; private set; }="";
        public event Action<int> CastStarted;
        public event Action<int> CastFinished;
        public static int Cost(int i) => i==0?10:i==2?30:i==4?25:0;
        public static float Cooldown(int i) => i==0?7:i==1?15:i==2?12:i==4?25:0;
        public static Vector2 Formula(int i,int rank)
        {
            float n=Mathf.Max(0,rank-1);
            return i==0?new(12+4*n,1.8f+.10f*n):i==1?new(8+3*n,1.2f+.08f*n):new(25+8*n,2.5f+.16f*n);
        }
        public static float RagePerBasic(int rank) => 1+Mathf.Ceil(Mathf.Clamp(rank,0,25)*.1f);
        public static float DecayPerSecond(int rank) => 5-4.9f*Mathf.Clamp(rank,0,25)/25;
        public static float ShieldFraction(int rank) => .05f+.30f*(Mathf.Clamp(rank,1,30)-1)/29;
        public float Remaining(int i) => (uint)i<5?Mathf.Max(0,readyAt[i]-Time.time):0;
        public float Range(int i) => i==1?12:i==2?1.2f:i==0?3:0;
        public float PreviewDamage(int i,int rank) { var f=Formula(i,rank); return (f.x+f.y*actor.Stats.Attributes.strength)*DamageMultiplier; }
        public float PreviewShield(int rank) => actor.Health.Maximum*ShieldFraction(rank);
        public bool HasMasteryWeapon => equipment?.HasGreatWeapon==true;
        public float DamageMultiplier
        {
            get
            {
                if(!Active)return 1;
                int r=ranks.Rank(3);
                float missing=.40f*r/25*Mathf.Clamp01((1-actor.Health.Current/Mathf.Max(1,actor.Health.Maximum))/.99f);
                return (HasMasteryWeapon?1+.005f*r:1)*(1+missing);
            }
        }
        private void Awake()
        {
            actor=GetComponent<CombatActor>(); ranks=GetComponent<BerserkerProgression>();
            resources=GetComponent<ActorResourceController>(); basic=GetComponent<BasicAttack>(); equipment=GetComponent<IGreatWeaponUser>();
        }
        private void OnEnable()
        {
            if(basic!=null)basic.Connected+=OnBasic;
            actor.Health.PacketResolved+=OnIncoming; actor.Health.Died+=ResetForManifestationLoad;
            if(shieldSigil==null)shieldSigil=GroundAuraVisual.Create(this,actor,GroundAuraStyle.Coffee,()=>Active&&actor.Health.TemporaryShieldFrom(ShieldSource)>0);
        }
        private void OnDisable()
        {
            if(basic!=null)basic.Connected-=OnBasic;
            if(actor?.Health!=null){actor.Health.PacketResolved-=OnIncoming;actor.Health.Died-=ResetForManifestationLoad;}
            ResetForManifestationLoad();
        }
        private void Update()
        {
            if(!Active){if(Busy||hasteUntil>0)ResetForManifestationLoad();return;}
            if(Busy&&!actor.CanAct){Interrupt();Feedback="Interrupted.";}
            if(hasteUntil>0&&Time.time>=hasteUntil){hasteUntil=0;actor.Stats.RemoveAttackSpeedPercentModifier(HasteSource);}
            if(Rage>0&&!actor.InCombat&&!Busy&&Time.time>=lastCombat+5)
                resources.TrySpend(RageId,Mathf.Min(Rage,DecayPerSecond(ranks.Rank(3))*Time.deltaTime));
        }
        private void OnBasic(CombatActor target,DamageResult packet)
        {
            if(!Active||target==null||!actor.IsHostileTo(target)||target.Health.Invulnerable||packet.Missed)return;
            Gain(RagePerBasic(ranks.Rank(3)));
        }
        private void OnIncoming(CombatActor source,DamageResult packet)
        {
            if(!Active||source==null||source==actor||!source.IsHostileTo(actor)||packet.Missed)return;
            float damage=packet.applied+packet.absorbed;
            if(damage>0)Gain(100*damage/Mathf.Max(1,actor.Health.Maximum));
        }
        private void Gain(float amount) { lastCombat=Time.time; resources.Restore(RageId,amount); }
        public bool CanPrepare(int i)
        {
            if((uint)i>=5||i==3)return Fail("Anger Management is passive.");
            if(!Active||!actor.CanAct)return Fail("Berserker cannot act.");
            if(ranks.Rank(i)<=0)return Fail("Learn this ability first.");
            if(Busy)return Fail("Finish the current swing first.");
            if(Remaining(i)>0)return Fail("Ability is cooling down.");
            if(!resources.CanSpend(RageId,Cost(i)))return Fail($"Requires {Cost(i)} Rage.");
            return true;
        }
        public bool CastWideArc(Vector3 point)
        {
            if(!CanPrepare(0)||!Finite(point))return false;
            Face(point); if(!Begin(0))return false; StartCoroutine(Arc()); return true;
        }
        private IEnumerator Arc()
        {
            yield return new WaitForSeconds(.35f);
            if(!ValidCast())yield break;
            EarlySkillVfx.PlayPriestPulse("Berserker sweeping steel",transform.position+transform.forward,2.1f,new Color(.94f,.58f,.22f));
            foreach(var target in new List<CombatActor>(CombatActor.All))
            {
                if(!actor.IsHostileTo(target)||!actor.HasSightOf(target))continue;
                var delta=Vector3.ProjectOnPlane(target.transform.position-transform.position,Vector3.up);
                if(delta.magnitude<=actor.Radius+target.Radius+3&&Vector3.Angle(transform.forward,delta)<=55)Hit(target,0);
            }
            Finish();
        }
        public bool CastSpinDash(Vector3 point)
        {
            if(!CanPrepare(1)||!Finite(point))return false;
            var agent=GetComponent<NavMeshAgent>();var path=new NavMeshPath();
            if(!actor.Motor.Ready||Vector3.Distance(transform.position,point)>12||
                !NavMesh.SamplePosition(point,out var hit,.75f,agent.areaMask)||
                !agent.CalculatePath(hit.position,path)||path.status!=NavMeshPathStatus.PathComplete)
                return Fail("Choose reachable ground within 12m.");
            float length=0;for(int i=1;i<path.corners.Length;i++)length+=Vector3.Distance(path.corners[i-1],path.corners[i]);
            if(length>15||length<.25f)return Fail("Choose an unobstructed dash route within range.");
            if(!Begin(1))return false;Face(point);StartCoroutine(Spin(path.corners));return true;
        }
        private IEnumerator Spin(Vector3[] corners)
        {
            var hits=new HashSet<CombatActor>();int corner=1;float elapsed=0,stalled=0,gained=0;
            var agent=GetComponent<NavMeshAgent>();priorAvoidance=agent.obstacleAvoidanceType;
            agent.obstacleAvoidanceType=ObstacleAvoidanceType.NoObstacleAvoidance;
            spinPassage=true;actor.SetEnemyPassage(SpinSource,true);
            actor.Motor.SetSpeedFactor(SpinSource,13/Mathf.Max(.1f,actor.Motor.BaseSpeed));
            while(corner<corners.Length&&elapsed<3&&ValidCast())
            {
                float remaining=Time.deltaTime;
                while(remaining>0&&corner<corners.Length)
                {
                    float step=Mathf.Min(.025f,remaining);remaining-=step;
                    var delta=Vector3.ProjectOnPlane(corners[corner]-transform.position,Vector3.up);
                    if(delta.magnitude<.22f){corner++;continue;}
                    var before=transform.position;
                    float fraction=Mathf.Min(1,delta.magnitude/Mathf.Max(.001f,actor.Motor.Speed*step));
                    actor.Motor.MoveDirect(delta.normalized*fraction,step);
                    stalled=Vector3.Distance(before,transform.position)<.005f?stalled+step:0;
                    foreach(var target in new List<CombatActor>(CombatActor.All))
                        if(actor.IsHostileTo(target)&&!hits.Contains(target)&&actor.HasSightOf(target)&&
                            Vector3.ProjectOnPlane(target.transform.position-transform.position,Vector3.up).magnitude<=1.8f+target.Radius)
                        {
                            hits.Add(target);
                            if(Hit(target,1)&&gained<20){float gain=Mathf.Min(2,20-gained);gained+=gain;Gain(gain);}
                        }
                    if(stalled>.3f){corner=corners.Length;Feedback="Spin Dash stopped at an obstacle.";break;}
                }
                elapsed+=Time.deltaTime;yield return null;
            }
            if(Busy)Finish();
        }
        public bool CastRageBreaker(CombatActor target)
        {
            if(!CanPrepare(2))return false;
            if(!TargetInRange(target))return Fail("Choose a visible enemy in melee range.");
            Face(target.transform.position);if(!Begin(2))return false;
            StartCoroutine(Breaker(target));return true;
        }
        private IEnumerator Breaker(CombatActor target)
        {
            yield return new WaitForSeconds(.55f);
            if(!ValidCast())yield break;
            if(TargetInRange(target)&&Hit(target,2))
            {
                EarlySkillVfx.Play(EarlySkillVfx.Cue.PhysicalHeavy,target.transform.position,1.1f);
                hasteUntil=Time.time+4;
                actor.Stats.SetAttackSpeedPercentModifier(HasteSource,2*ranks.Rank(2));
            }
            Finish();
        }
        public bool CastCoffeeShield()
        {
            if(!CanPrepare(4)||!Begin(4))return false;
            actor.Health.AddTemporaryShield(ShieldSource,PreviewShield(ranks.Rank(4)),5,"element.magical-coffee");
            EarlySkillVfx.PlayPriestPulse("Coffee shield",transform.position,1.2f,new Color(.70f,.40f,.13f));
            Finish();return true;
        }
        private bool TargetInRange(CombatActor target) => actor.IsHostileTo(target)&&actor.HasSightOf(target)&&
            Vector3.Distance(transform.position,target.transform.position)<=actor.Radius+target.Radius+1.2f;
        private bool Hit(CombatActor target,int i)
        {
            var attack=i==0?wideArc:i==1?spinDash:rageBreaker;
            var action=AccuracyResolver.Resolve(actor,attack,critical:i==2?actor.Stats.ExternalCriticalRule():null,target:target);
            var f=Formula(i,ranks.Rank(i));
            var packet=action.Hit.missed?new DamageResult{action=action,channel=attack.channel}:
                DamageResolver.Calculate(actor.Stats,target.Stats,attack,action.Critical,f.y,
                    skillFlatDamage:f.x,finalRawMultiplier:DamageMultiplier);
            packet.action=action;packet=target.Health.ApplyPacket(actor,packet);
            bool connected=!packet.Missed&&!target.Health.Invulnerable;
            if(connected)LastCastHits++;
            return connected;
        }
        private bool Begin(int i)
        {
            if(!resources.TrySpend(RageId,Cost(i)))return false;
            casting=i;LastCastHits=0;LastRageSpent=Cost(i);
            basic?.Cancel();suspendedBasic=basic!=null&&basic.enabled;if(suspendedBasic)basic.enabled=false;
            actor.Motor.Stop();Feedback=BerserkerProgression.Name(i);CastStarted?.Invoke(i);return true;
        }
        private bool ValidCast() { if(Busy&&Active&&actor.CanAct)return true;Interrupt();return false; }
        private void Finish()
        {
            int i=casting;if(i<0)return;
            CleanupCast();readyAt[i]=Time.time+Cooldown(i);CastFinished?.Invoke(i);
        }
        // Accepted actions pay their cost. Interrupted actions enter recovery when
        // they end; rejected targeting never pays or starts a cooldown.
        public void Interrupt()
        {
            int i=casting;StopAllCoroutines();CleanupCast();
            if(i>=0){readyAt[i]=Time.time+Cooldown(i);CastFinished?.Invoke(i);}
        }
        private void CleanupCast()
        {
            casting=-1;actor?.Motor?.RemoveSpeedFactor(SpinSource);
            if(spinPassage)
            {
                actor.SetEnemyPassage(SpinSource,false);var agent=GetComponent<NavMeshAgent>();
                if(agent!=null&&agent.obstacleAvoidanceType==ObstacleAvoidanceType.NoObstacleAvoidance)agent.obstacleAvoidanceType=priorAvoidance;
                GetComponent<ActorFootprintSeparation>()?.RebaseAfterTeleport();spinPassage=false;
            }
            actor?.Motor?.Stop();if(suspendedBasic&&basic!=null)basic.enabled=true;suspendedBasic=false;
        }
        public void ResetForManifestationLoad()
        {
            StopAllCoroutines();CleanupCast();Array.Clear(readyAt,0,5);hasteUntil=0;lastCombat=Time.time;
            actor?.Stats?.RemoveAttackSpeedPercentModifier(HasteSource);actor?.Health?.RemoveTemporaryShield(ShieldSource);
            if(resources!=null&&Rage>0)resources.TrySpend(RageId,Rage);
            LastCastHits=0;LastRageSpent=0;Feedback="";
        }
        public void Configure(AttackDefinition arc,AttackDefinition spin,AttackDefinition breaker) { wideArc=arc;spinDash=spin;rageBreaker=breaker; }
        private void Face(Vector3 point) { var d=Vector3.ProjectOnPlane(point-transform.position,Vector3.up);if(d.sqrMagnitude>.001f)transform.rotation=Quaternion.LookRotation(d); }
        private static bool Finite(Vector3 p) => float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z);
        private bool Fail(string message) { Feedback=message;return false; }
    }
}
