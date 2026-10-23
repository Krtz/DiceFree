using System;
using System.Collections;
using System.Collections.Generic;
using DiceFree.Foundation;
using DiceFree.Progression;
using DiceFree.Skills;
using UnityEngine;
using UnityEngine.AI;
using Kind=DiceFree.Progression.GreatbladeProgressionRules.Kind;

namespace DiceFree.Combat
{
    [DisallowMultipleComponent,RequireComponent(typeof(CombatActor),typeof(GreatbladeSkillProgression),typeof(ActorResourceController))]
    public sealed class GreatbladeSkillCaster:MonoBehaviour,IManifestationSessionState
    {
        public const string MomentumId="resource.momentum";
        const string CrySource="skill.greatblade.battlecry",HasteSource="skill.greatblade.cleavehaste";
        [SerializeField] GreatbladeCombatTuning tuning;
        readonly Dictionary<Kind,float> readyAt=new();
        CombatActor actor;
        GreatbladeSkillProgression skills;
        ActorResourceController resources;
        BasicAttack basic;
        float lastHit,cryUntil,hasteUntil,areaGain;
        bool suspendedBasic, spinPassage;
        ObstacleAvoidanceType priorAvoidance;
        const string SpinSource="skill.greatblade.spin";
        public bool Busy{get;private set;}
        public string Feedback{get;private set;}="";
        public GreatbladeCombatTuning Tuning=>tuning;
        public float Momentum=>resources?.Current(MomentumId)??0;
        public int LastCastHits{get;private set;}
        public float LastMomentumSpent{get;private set;}
        public event Action<Kind> CastStarted;
        public event Action<Kind> CastFinished;
        public void Configure(GreatbladeCombatTuning value)=>tuning=value;
        void Awake(){actor=GetComponent<CombatActor>();skills=GetComponent<GreatbladeSkillProgression>();resources=GetComponent<ActorResourceController>();basic=GetComponent<BasicAttack>();}
        void OnEnable(){if(basic!=null)basic.Connected+=OnBasic;actor.Health.Died+=ResetForManifestationLoad;}
        void OnDisable(){if(basic!=null)basic.Connected-=OnBasic;if(actor?.Health!=null)actor.Health.Died-=ResetForManifestationLoad;ResetForManifestationLoad();}
        void Update()
        {
            if(!skills.ActiveForCurrentClass){if(Busy||Momentum>0||cryUntil>0||hasteUntil>0)ResetForManifestationLoad();return;}
            if(Busy&&!actor.CanAct){CancelCast();Feedback="Interrupted.";}
            if(cryUntil>0&&Time.time>=cryUntil){cryUntil=0;actor.Stats.RemoveBasicAttackDamagePercentContribution(CrySource);}
            if(hasteUntil>0&&Time.time>=hasteUntil){hasteUntil=0;actor.Stats.RemoveAttackSpeedPercentModifier(HasteSource);}
            if(tuning!=null&&Momentum>0&&Time.time>lastHit+tuning.decayDelay&&!actor.InCombat&&!Busy)
                resources.TrySpend(MomentumId,Mathf.Min(Momentum,tuning.decayPerSecond*GreatbladeProgressionRules.MomentumDecayFactor(skills.Rank(Kind.HeavyRhythm))*Time.deltaTime));
        }
        void OnBasic(CombatActor target,DamageResult packet){if(skills.ActiveForCurrentClass&&packet.applied+packet.absorbed>0&&tuning!=null)Gain(tuning.basicMomentum);}
        void Gain(float value)
        {
            lastHit=Time.time;
            resources.Restore(MomentumId,value*GreatbladeProgressionRules.MomentumGainFactor(skills.Rank(Kind.HeavyRhythm)));
        }
        public float CooldownRemaining(Kind kind)=>readyAt.TryGetValue(kind,out float at)?Mathf.Max(0,at-Time.time):0;
        public float Cooldown(Kind kind)=>tuning==null?0:kind switch{Kind.WideArc=>tuning.arcCooldown,Kind.SpinDash=>tuning.spinCooldown,Kind.OverheadBreaker=>tuning.breakerCooldown,Kind.BattleCry=>tuning.cryCooldown,_=>0};
        public bool CanPrepare(Kind kind)
        {
            if(tuning==null||!skills.ActiveForCurrentClass||!actor.CanAct)return Fail("Greatblade cannot act.");
            if(GreatbladeProgressionRules.For(kind).passive)return Fail("That ability is passive.");
            if(skills.Rank(kind)==0)return Fail("Learn this ability at its required level.");
            if(Busy)return Fail("Finish the current swing first.");
            if(CooldownRemaining(kind)>0)return Fail("Ability is cooling down.");
            if(kind==Kind.OverheadBreaker&&Momentum<tuning.breakerMinimumMomentum)return Fail("Overhead Breaker needs at least "+tuning.breakerMinimumMomentum+" Momentum.");
            return true;
        }
        public bool CastWideArc(Vector3 point)
        {
            if(!CanPrepare(Kind.WideArc)||!Finite(point))return false;
            Face(point);Begin(Kind.WideArc);StartCoroutine(Arc());return true;
        }
        IEnumerator Arc()
        {
            yield return new WaitForSeconds(tuning.arcWindup);
            if(!ValidCast())yield break;
            LastCastHits=0;areaGain=0;
            int mastery=skills.Rank(Kind.GreatswordMastery);
            float angle=tuning.arcAngle+GreatbladeProgressionRules.MasteryArcWidth(mastery);
            foreach(var target in new List<CombatActor>(CombatActor.All))
            {
                if(!actor.IsHostileTo(target)||!actor.HasSightOf(target))continue;
                Vector3 delta=Vector3.ProjectOnPlane(target.transform.position-transform.position,Vector3.up);
                if(delta.magnitude>actor.Radius+target.Radius+tuning.arcReach||Vector3.Angle(transform.forward,delta)>angle*.5f)continue;
                Hit(target,tuning.wideArc,tuning.ArcCoefficient(skills.Rank(Kind.WideArc)),true);
            }
            if(GreatbladeProgressionRules.GetsMasteryCleaveHaste(mastery)){hasteUntil=Time.time+GreatbladeProgressionRules.MasteryHasteDuration(mastery);actor.Stats.SetAttackSpeedPercentModifier(HasteSource,GreatbladeProgressionRules.MasteryHastePercent(mastery));}
            Finish(Kind.WideArc);
        }
        public bool CastSpinDash(Vector3 point)
        {
            if(!CanPrepare(Kind.SpinDash)||!Finite(point))return false;
            var agent=GetComponent<NavMeshAgent>();var path=new NavMeshPath();
            if(!actor.Motor.Ready||Vector3.Distance(transform.position,point)>tuning.spinRange||
                !NavMesh.SamplePosition(point,out var hit,.75f,agent.areaMask)||
                !agent.CalculatePath(hit.position,path)||path.status!=NavMeshPathStatus.PathComplete)return Fail("Choose reachable ground within Spin Dash range.");
            float length=0;for(int i=1;i<path.corners.Length;i++)length+=Vector3.Distance(path.corners[i-1],path.corners[i]);
            if(length>tuning.spinRange*1.25f)return Fail("The safe route is too long for Spin Dash.");
            if(length<.25f)return Fail("Choose ground farther away.");
            Begin(Kind.SpinDash);StartCoroutine(Spin(path.corners));return true;
        }
        IEnumerator Spin(Vector3[] corners)
        {
            var hitOnce=new HashSet<CombatActor>();int corner=1;float elapsed=0,stalled=0;
            LastCastHits=0;areaGain=0;
            var agent=GetComponent<NavMeshAgent>();
            priorAvoidance=agent.obstacleAvoidanceType;
            agent.obstacleAvoidanceType=ObstacleAvoidanceType.NoObstacleAvoidance;
            spinPassage=true;actor.SetEnemyPassage(SpinSource,true);
            actor.Motor.SetSpeedFactor(SpinSource,tuning.spinSpeed/Mathf.Max(.1f,actor.Motor.BaseSpeed));
            while(corner<corners.Length&&elapsed<3&&ValidCast())
            {
                float remaining=Time.deltaTime;
                while(remaining>0&&corner<corners.Length)
                {
                    float step=Mathf.Min(.025f,remaining);remaining-=step;
                    Vector3 delta=Vector3.ProjectOnPlane(corners[corner]-transform.position,Vector3.up);
                    if(delta.magnitude<.22f){corner++;continue;}
                    Vector3 before=transform.position;
                    float fraction=Mathf.Min(1,delta.magnitude/Mathf.Max(.001f,actor.Motor.Speed*step));
                    actor.Motor.MoveDirect(delta.normalized*fraction,step);
                    float travelled=Vector3.Distance(before,transform.position);
                    stalled=travelled<.005f?stalled+step:0;
                    foreach(var target in new List<CombatActor>(CombatActor.All))
                        if(actor.IsHostileTo(target)&&!hitOnce.Contains(target)&&actor.HasSightOf(target)&&
                            Vector3.ProjectOnPlane(target.transform.position-transform.position,Vector3.up).magnitude<=tuning.spinRadius+target.Radius)
                        {hitOnce.Add(target);Hit(target,tuning.spinDash,tuning.SpinCoefficient(skills.Rank(Kind.SpinDash)),true);}
                    if(stalled>.3f){corner=corners.Length;Feedback="Spin Dash stopped at an obstacle.";break;}
                }
                elapsed+=Time.deltaTime;yield return null;
            }
            if(Busy)Finish(Kind.SpinDash);
        }
        public bool CastOverheadBreaker(CombatActor target)
        {
            if(!CanPrepare(Kind.OverheadBreaker))return false;
            if(!actor.IsHostileTo(target)||!actor.HasSightOf(target)||Vector3.Distance(transform.position,target.transform.position)>actor.Radius+target.Radius+tuning.breakerReach)return Fail("Choose a visible enemy in melee range.");
            LastMomentumSpent=Momentum;
            if(!resources.TrySpend(MomentumId,LastMomentumSpent))return false;
            Face(target.transform.position);Begin(Kind.OverheadBreaker);
            StartCoroutine(Breaker(target,LastMomentumSpent));return true;
        }
        IEnumerator Breaker(CombatActor target,float spent)
        {
            yield return new WaitForSeconds(tuning.breakerWindup);
            if(!ValidCast())yield break;
            LastCastHits=0;
            if(actor.IsHostileTo(target)&&actor.HasSightOf(target)&&Vector3.Distance(transform.position,target.transform.position)<=actor.Radius+target.Radius+tuning.breakerReach)
            {
                bool connected=Hit(target,tuning.overheadBreaker,tuning.BreakerCoefficient(skills.Rank(Kind.OverheadBreaker)),false,1+spent/100f*1.5f);
                int mastery=skills.Rank(Kind.GreatswordMastery);
                if(connected&&GreatbladeProgressionRules.GetsMasteryFinisherProtection(mastery))
                    actor.Health.AddTemporaryShield("greatblade.mastery",GreatbladeProgressionRules.MasteryShieldCapacity(actor.Health.Maximum),5,"element.metal");
            }
            Finish(Kind.OverheadBreaker);
        }
        public bool CastBattleCry()
        {
            if(!CanPrepare(Kind.BattleCry))return false;
            readyAt[Kind.BattleCry]=Time.time+tuning.cryCooldown;
            cryUntil=Time.time+tuning.cryDuration;
            actor.Stats.SetBasicAttackDamagePercentContribution(CrySource,tuning.cryDamagePercent+3*(skills.Rank(Kind.BattleCry)-1));
            Feedback="Battle Cry";CastStarted?.Invoke(Kind.BattleCry);CastFinished?.Invoke(Kind.BattleCry);return true;
        }
        bool Hit(CombatActor target,AttackDefinition attack,float coefficient,bool generates,float extra=1)
        {
            var resolution=AccuracyResolver.Resolve(actor,attack);
            float bonus=1+GreatbladeProgressionRules.DamageBonusPct(skills.Rank(Kind.GreatswordMastery))/100f;
            if(cryUntil>Time.time)bonus+= (tuning.cryDamagePercent+3*(skills.Rank(Kind.BattleCry)-1))/100f;
            var packet=resolution.Hit.missed?new DamageResult{action=resolution,channel=attack.channel}:
                DamageResolver.Calculate(actor.Stats,target.Stats,attack,resolution.Critical,coefficient,attack.RollDamageMultiplier(actor.Stats.Attributes,bonus*extra,coefficient));
            packet.action=resolution;packet=target.Health.ApplyPacket(actor,packet);
            if(packet.applied+packet.absorbed<=0)return false;
            LastCastHits++;
            if(generates){float gain=Mathf.Min(tuning.skillMomentum,Mathf.Max(0,tuning.areaMomentumCap-areaGain));areaGain+=gain;if(gain>0)Gain(gain);}
            return true;
        }
        void Begin(Kind kind)
        {
            Busy=true;LastCastHits=0;readyAt[kind]=Time.time+Cooldown(kind);
            basic?.Cancel();suspendedBasic=basic!=null&&basic.enabled;if(suspendedBasic)basic.enabled=false;
            actor.Motor.Stop();Feedback=kind.ToString();CastStarted?.Invoke(kind);
        }
        bool ValidCast(){if(Busy&&actor.CanAct&&skills.ActiveForCurrentClass)return true;CancelCast();return false;}
        void Finish(Kind kind){CancelCast();CastFinished?.Invoke(kind);}
        public void CancelCast()
        {
            StopAllCoroutines();Busy=false;
            actor?.Motor?.RemoveSpeedFactor(SpinSource);
            if(spinPassage)
            {
                actor?.SetEnemyPassage(SpinSource,false);
                var agent=GetComponent<NavMeshAgent>();
                if(agent!=null&&agent.obstacleAvoidanceType==ObstacleAvoidanceType.NoObstacleAvoidance)
                    agent.obstacleAvoidanceType=priorAvoidance;
                GetComponent<ActorFootprintSeparation>()?.RebaseAfterTeleport();
                spinPassage=false;
            }
            actor?.Motor?.Stop();
            if(suspendedBasic&&basic!=null)basic.enabled=true;suspendedBasic=false;
        }
        public void ResetForManifestationLoad()
        {
            CancelCast();readyAt.Clear();cryUntil=hasteUntil=0;lastHit=Time.time;
            actor?.Stats?.RemoveBasicAttackDamagePercentContribution(CrySource);
            actor?.Stats?.RemoveAttackSpeedPercentModifier(HasteSource);
            if(resources!=null&&Momentum>0)resources.TrySpend(MomentumId,Momentum);
            actor?.Health?.RemoveTemporaryShield("greatblade.mastery");
            Feedback="";LastMomentumSpent=0;LastCastHits=0;
        }
        void Face(Vector3 point){Vector3 d=Vector3.ProjectOnPlane(point-transform.position,Vector3.up);if(d.sqrMagnitude>.001f)transform.rotation=Quaternion.LookRotation(d);}
        static bool Finite(Vector3 p)=>!float.IsNaN(p.x)&&!float.IsNaN(p.y)&&!float.IsNaN(p.z)&&!float.IsInfinity(p.x)&&!float.IsInfinity(p.y)&&!float.IsInfinity(p.z);
        bool Fail(string text){Feedback=text;return false;}
    }
}
