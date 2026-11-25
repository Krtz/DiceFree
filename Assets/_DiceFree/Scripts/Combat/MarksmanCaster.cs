using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Foundation;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent,RequireComponent(typeof(MarksmanProgression),typeof(ActorResourceController))]
    public sealed class MarksmanCaster : MonoBehaviour,IChannelingAbility,IManifestationSessionState
    {
        const string Mastery="passive.marksman.mastery",Strafe="buff.marksman.strafe";
        static readonly List<MarksmanCaster> sources=new();
        [SerializeField] AttackDefinition aimed,rain,power;
        readonly float[] ready=new float[6];
        CombatActor actor,aimedTarget;BasicAttack basic;MarksmanProgression ranks;ActorResourceController mana;
        IChannelingAbility[] others;float releaseAt,strafeUntil;int casting,castRank,skyShots;Vector3 castOrigin,groundPoint;GroundAuraVisual aura;
        public bool Active=>isActiveAndEnabled&&ranks.Active&&actor.Alive;
        public CombatActor Actor=>actor;
        public MarksmanProgression Progression=>ranks;
        public float RangeBonus=>Active ? .2f*ranks.Rank(2):0;
        public bool IsChanneling=>releaseAt>0;
        public string ChannelLabel=>MarksmanProgression.Name(casting);
        public float ChannelDuration=>1.5f;
        public float ChannelRemaining=>IsChanneling?Mathf.Max(0,releaseAt-Time.time):0;
        public event Action<int> Casted;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetSources()=>sources.Clear();
        void Awake(){actor=GetComponent<CombatActor>();basic=GetComponent<BasicAttack>();ranks=GetComponent<MarksmanProgression>();mana=GetComponent<ActorResourceController>();}
        void Start(){others=GetComponents<MonoBehaviour>().OfType<IChannelingAbility>().Where(c=>!ReferenceEquals(c,this)).ToArray();aura=GroundAuraVisual.Create(this,actor,GroundAuraStyle.Ranged,()=>Active&&ranks.Rank(4)>0);}
        void OnEnable(){sources.Add(this);ranks.Changed+=Passive;actor.Health.Died+=Clear;Passive();}
        void OnDisable(){sources.Remove(this);ranks.Changed-=Passive;actor.Health.Died-=Clear;actor.Stats.RemoveAttributeContribution(Mastery);ResetForManifestationLoad();}
        void OnDestroy(){if(aura!=null)aura.Remove();}
        void Passive(){int r=ranks.Rank(2);if(r>0)actor.Stats.SetAttributeContribution(Mastery,new AttributeValues{agility=r*3});else actor.Stats.RemoveAttributeContribution(Mastery);if(!ranks.Active)Clear();}
        public CriticalRule CriticalRule()=>Active&&ranks.Rank(2)>0?new CriticalRule(Mastery,"marksman.bow-crit",true,ranks.Rank(2)*.015f,1.6f):null;
        public static bool IsRanged(BasicAttack attack)=>attack!=null&&attack.Definition!=null&&attack.Definition.reach>=3;
        public static float AuraMultiplier(CombatActor recipient)
        {
            if(recipient==null||!recipient.Alive||!IsRanged(recipient.GetComponent<BasicAttack>()))return 1;
            int rank=0;foreach(var source in sources)if(source!=null&&source.Active&&source.actor.IsFriendlyTo(recipient)&&(source.transform.position-recipient.transform.position).sqrMagnitude<=source.basic.Reach*source.basic.Reach)rank=Mathf.Max(rank,source.ranks.Rank(4));
            return 1+rank*.01f;
        }
        public static int Cost(int i,int rank)=>i switch{0=>18+12*rank,1=>20+15*rank,3=>10+9*rank,5=>18+13*rank,_=>0};
        public static int Cooldown(int i)=>i switch{0=>7,1=>22,3=>18,5=>12,_=>0};
        public float Remaining(int i)=>Mathf.Max(0,ready[i]-Time.time);
        public float Range(int i)=>i==1?12:basic.Reach+(i==5?1:0);
        public static Vector2 Formula(int i,int rank){float n=Mathf.Max(0,rank-1);return i switch{0=>new(15.2f+.6f*n,2.15f+.091f*n),1=>new(3.2f+.176f*n,.4f+.022f*n),_=>new(9.2f+.4f*n,1.25f+.058f*n)};}
        public float PreviewDamage(int i,int rank){var f=Formula(i,rank);return f.x+f.y*actor.Stats.Attributes.agility;}
        public bool CanPrepare(int i)=>(uint)i<6&&i!=2&&i!=4&&Active&&actor.CanAct&&!IsChanneling&&ranks.Rank(i)>0&&Remaining(i)<=0&&mana.CanSpend(ResourceIds.Mana,Cost(i,ranks.Rank(i)))&&others?.Any(c=>c.IsChanneling)!=true;
        bool Spend(int i){if(!mana.TrySpend(ResourceIds.Mana,Cost(i,ranks.Rank(i))))return false;ready[i]=Time.time+Cooldown(i);basic.Cancel();return true;}
        public bool CastAimed(CombatActor target)
        {
            if(!CanPrepare(0)||!actor.CanTargetHostile(target)||!InRange(target,Range(0))||!Spend(0))return false;
            aimedTarget=target;Face(target.transform.position);Begin(0);return true;
        }
        bool InRange(CombatActor target,float range)=>Vector3.Distance(transform.position,target.transform.position)<=range+actor.Radius+target.Radius&&actor.HasSightOf(target);
        public bool CastRain(Vector3 point)
        {
            if(!CanPrepare(1)||Vector3.Distance(transform.position,point)>12+actor.Radius||!NavMesh.SamplePosition(point,out var hit,.75f,1)||Physics.Linecast(transform.position+Vector3.up,hit.position+Vector3.up*.3f,1<<9,QueryTriggerInteraction.Ignore)||!Spend(1))return false;
            groundPoint=hit.position;Face(point);Begin(1);return true;
        }
        void Begin(int i){casting=i;castRank=ranks.Rank(i);skyShots=0;castOrigin=transform.position;releaseAt=Time.time+1.5f;Casted?.Invoke(i);}
        void Update()
        {
            if(strafeUntil>0&&(!Active||Time.time>=strafeUntil))ClearStrafe();
            if(!IsChanneling)return;
            if(!Active||!actor.CanAct||actor.Motor.Travelling||(transform.position-castOrigin).sqrMagnitude>.025f){CancelChannel();return;}
            if(casting==1&&skyShots<2&&1.5f-ChannelRemaining>=(skyShots==0?.4f:1.08f)){var from=transform.position+Vector3.up*1.2f;RangerVfx.Arrow(from,from+Vector3.up*13+transform.forward*3,.4f,skyShots++==0?1.4f:.8f,RangerVfx.ArrowTint);}
            if(Time.time<releaseAt)return;releaseAt=0;
            if(casting==1)StartCoroutine(Rain(groundPoint,PreviewDamage(1,castRank)));
            else if(actor.CanTargetHostile(aimedTarget)&&InRange(aimedTarget,Range(0)))
            {
                var target=aimedTarget;float raw=PreviewDamage(0,castRank),penetration=.1f*castRank/30;
                var action=AccuracyResolver.Resolve(actor,aimed,critical:CriticalRule());
                Launch(target,()=>Hit(target,aimed,raw,action,penetration));
            }
            aimedTarget=null;
        }
        public bool CastStrafe(){if(!CanPrepare(3)||!Spend(3))return false;int r=ranks.Rank(3);actor.Stats.SetAttackSpeedPercentModifier(Strafe,r*2);actor.Stats.SetMovementSpeedPercentModifier(Strafe,-r*2);strafeUntil=Time.time+Mathf.Ceil(1+r/5f);Casted?.Invoke(3);return true;}
        public bool CastPower(Vector3 point)
        {
            var direction=Vector3.ProjectOnPlane(point-transform.position,Vector3.up);if(!CanPrepare(5)||direction.sqrMagnitude<.01f||!Spend(5))return false;
            Face(point);StartCoroutine(Power(direction.normalized,Range(5),PreviewDamage(5,ranks.Rank(5))));Casted?.Invoke(5);return true;
        }
        IEnumerator Power(Vector3 direction,float reach,float raw)
        {
            var start=transform.position+Vector3.up*.8f;float travelled=0;int hits=0;var struck=new HashSet<CombatActor>();var action=Guaranteed(power);
            while(travelled<reach&&Active)
            {
                float end=Mathf.Min(reach,travelled+28*Time.deltaTime);bool wall=Physics.Linecast(start+direction*travelled,start+direction*end,out var block,1<<9,QueryTriggerInteraction.Ignore);if(wall)end=Vector3.Dot(block.point-start,direction);
                foreach(var target in CombatActor.All.Where(t=>actor.CanTargetHostile(t)&&!struck.Contains(t)).OrderBy(t=>Vector3.Dot(t.transform.position-start,direction)).ToArray())
                {
                    var delta=target.transform.position-start;float along=Vector3.Dot(delta,direction);float width=.75f+target.Radius;var side=Vector3.ProjectOnPlane(delta-direction*along,Vector3.up);
                    if(along<Mathf.Max(0,travelled-target.Radius)||along>end||side.sqrMagnitude>width*width)continue;
                    struck.Add(target);Hit(target,power,raw*Mathf.Pow(.8f,hits++),action);
                }
                RangerVfx.Arrow(start+direction*travelled,start+direction*end,.10f,1.3f,new Color(.8f,1,.5f));travelled=end;if(wall)yield break;yield return null;
            }
        }
        IEnumerator Rain(Vector3 point,float raw)
        {
            var action=Guaranteed(rain);for(int wave=0;wave<6;wave++){RangerVfx.Rain(point,3,wave);yield return new WaitForSeconds(1f/3);if(!Active)yield break;foreach(var target in CombatActor.All.Where(t=>actor.CanTargetHostile(t)&&(t.transform.position-point).sqrMagnitude<=9).ToArray())Hit(target,rain,raw,action);}
        }
        static ActionResolution Guaranteed(AttackDefinition attack)=>ActionResolution.Resolve(new ActionProvenance(attack.stableId,MarksmanProgression.Id));
        void Hit(CombatActor target,AttackDefinition attack,float raw,ActionResolution action,float penetration=0)
        {if(!actor.CanTargetHostile(target))return;var result=DamageResolver.CalculateRaw(actor.Stats,target.Stats,attack,raw,action.Critical,percentDefensePenetration:penetration);result.action=action;target.Health.ApplyPacket(actor,result);}
        public void Launch(CombatActor target,Action impact)=>StartCoroutine(Fly(target,impact));
        IEnumerator Fly(CombatActor target,Action impact)
        {
            var from=transform.position+Vector3.up*1.2f;float flight=Mathf.Clamp(Vector3.Distance(from,target.transform.position)/29,.09f,.56f);var arrow=RangerVfx.TrackArrow(from,target.transform.position+Vector3.up,new Color(.95f,.85f,.4f),flight);
            for(float t=0;t<flight;t+=Time.deltaTime){if(!Active||!actor.CanTargetHostile(target))yield break;RangerVfx.PlaceArrow(arrow,from,target.transform.position+Vector3.up,t/flight);yield return null;}if(Active&&actor.CanTargetHostile(target))impact();
        }
        void Face(Vector3 p){var d=Vector3.ProjectOnPlane(p-transform.position,Vector3.up);if(d.sqrMagnitude>.001f)transform.rotation=Quaternion.LookRotation(d);}
        public void CancelChannel(){releaseAt=0;aimedTarget=null;}
        void ClearStrafe(){strafeUntil=0;actor.Stats.RemoveAttackSpeedPercentModifier(Strafe);actor.Stats.RemoveMovementSpeedPercentModifier(Strafe);}
        void Clear(){CancelChannel();StopAllCoroutines();ClearStrafe();}
        public void ResetForManifestationLoad(){Clear();Array.Clear(ready,0,6);}
        public void Configure(AttackDefinition a,AttackDefinition r,AttackDefinition p){aimed=a;rain=r;power=p;}
    }
}
