using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Foundation;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor),typeof(ActorResourceController))]
    public sealed class PsionEvolutionCaster : MonoBehaviour,IChannelingAbility,IInterruptibleCombatCast,IManifestationSessionState
    {
        [SerializeField] private AttackDefinition bolt,storm,rupture,voidHit;
        [SerializeField] private ElementDefinition metal,voidElement;
        [SerializeField] private GameObject knightVisual,mageVisual;
        [SerializeField] private Material apparition;
        private CombatActor actor,target;
        private BasicAttack basic;
        private ActorResourceController mana;
        private PsionicKnightProgression knight;
        private MirageMageProgression mage;
        private readonly float[] cooldowns=new float[5];
        private readonly List<PsionEvolutionImage> images=new();
        private readonly List<Echo> echoes=new();
        private SharedDamageShield ownShield;
        private int casting,castRank,wave,session;
        private float began,chargesExpire,hasteExpire,haste;
        private Vector3 point,origin;
        private AttributeValues castStats;
        private PsionEvolutionImage[] castImages=Array.Empty<PsionEvolutionImage>();
        private int personalRank=-1;
        public CombatActor Actor=>actor;
        public PsionEvolutionProgression Progression=>knight.Active?knight:mage;
        public bool Knight=>knight.Active;
        public bool Active=>isActiveAndEnabled&&(knight.Active||mage.Active);
        public bool IsChanneling {get;private set;}
        public string ChannelLabel=>PsionEvolutionProgression.Name(Knight,casting);
        public float ChannelDuration=>!Knight&&casting==2?.7f:CastTime(Knight,casting);
        public float ChannelRemaining=>IsChanneling?Mathf.Max(0,began+ChannelDuration-Time.time):0;
        public int Charges {get;private set;}
        public float ChargeRemaining=>Mathf.Max(0,chargesExpire-Time.time);
        public float ShieldRemaining=>ownShield?.Remaining??0;
        public float HasteRemaining=>Mathf.Max(0,hasteExpire-Time.time);
        public float HastePercent=>HasteRemaining>0?haste:0;
        public float BasicPenetration=>Active&&Knight?Progression.Rank(0):0;
        public string Feedback {get;private set;}="";
        public IReadOnlyList<PsionEvolutionImage> Images=>images;
        public event Action<int> Casted;
        public static int Cost(bool k,int i,int r)=>k?i switch{1=>16+20*r,2=>18+20*r,3=>0,_=>0}:i switch{0=>10+20*r,1=>20+20*r,2=>20+22*r,3=>20+22*r,_=>0};
        public static float Cooldown(bool k,int i)=>k?i switch{1=>12,2=>22,3=>14,_=>0}:i switch{0=>5.5f,1=>12,2=>14,3=>26,_=>0};
        public static bool Passive(bool k,int i)=>i==4||k&&i==0;
        public static float CastTime(bool k,int i)=>k&&i==3||!k&&i is 0 or 2?.1f:.2f;
        public static float ImageFraction(int r)=>Mathf.Lerp(.1f,.5f,(r-1)/24f);
        public static float MirrorFraction(int r)=>Mathf.Lerp(.05f,.25f,(r-1)/24f);
        public static float ImageDuration(int r)=>Mathf.Lerp(1.6f,5,(r-1)/24f);
        public static float Raw(bool k,int i,int r,AttributeValues a)=>k?i switch
        {
            2=>180+48*r+(.5f+.03f*r)*a.agility+(.85f+.055f*r)*a.intelligence,
            3=>30+17*r+(.4f+.016f*(r-1))*a.agility+(.1f+.004f*(r-1))*a.intelligence,
            _=>0
        }:i switch
        {
            0=>45+11*r+(1.3f+.1f*r)*a.intelligence,
            1=>55+15*r+(.65f+.04f*r)*a.agility+(.8f+.07f*r)*a.intelligence,
            2=>25+7*r+(.7f+.07f*r)*a.intelligence,
            3=>160+42*r+(.85f+.055f*r)*a.intelligence,
            _=>0
        };
        public float Preview(int i,int r)=>Raw(Knight,i,r,actor.Stats.Attributes);
        public float Remaining(int i)=>Mathf.Max(0,cooldowns[i]-Time.time);
        public CriticalRule BasicCriticalRule()=>Active&&Knight?new CriticalRule(PsionEvolutionProgression.KnightId,"knight.basic-crit",true,Progression.Rank(0)*.01f,1.60f+Progression.Rank(0)*.01f):null;
        private void Awake(){actor=GetComponent<CombatActor>();basic=GetComponent<BasicAttack>();mana=GetComponent<ActorResourceController>();knight=GetComponent<PsionicKnightProgression>();mage=GetComponent<MirageMageProgression>();}
        private void OnEnable(){basic.Connected+=OnLanded;actor.Health.Died+=OnDeath;PsionSpeedPresence.Register(this);}
        private void OnDisable(){basic.Connected-=OnLanded;actor.Health.Died-=OnDeath;PsionSpeedPresence.Unregister(this);ResetForManifestationLoad();}
        private void OnDeath(){CancelChannel();Charges=0;actor.Stats.RemoveAttackSpeedPercentModifier("knight.image-end");haste=0;}
        public bool CanPrepare(int i)=>Active&&actor.CanAct&&(uint)i<5&&!Passive(Knight,i)&&Progression.Rank(i)>0&&!IsChanneling&&Remaining(i)==0&&mana.CanSpend(ResourceIds.Mana,Cost(Knight,i,Progression.Rank(i)))&&!GetComponents<MonoBehaviour>().OfType<IChannelingAbility>().Any(c=>!ReferenceEquals(c,this)&&c.IsChanneling);
        public bool CastBolt(CombatActor victim)
        {
            if(Knight||!CanPrepare(0)||!actor.CanTargetHostile(victim)||!ValidBolt(actor,victim))return false;
            target=victim;return Begin(0);
        }
        public bool CastImage(Vector3 p)=>CastGround(1,p,6);
        public bool CastStorm(Vector3 p)=>!Knight&&CastGround(2,p,9);
        private bool CastGround(int i,Vector3 p,float range)
        {
            if(!CanPrepare(i)||!NavMesh.SamplePosition(p,out var hit,.65f,1)||Vector3.Distance(transform.position,hit.position)>range||Physics.Linecast(transform.position+Vector3.up,hit.position+Vector3.up*.3f,1<<9))return false;
            if(i==1&&!FreePoint(hit.position))return false;point=hit.position;return Begin(i);
        }
        internal static bool FreePoint(Vector3 p)=>!Physics.CheckCapsule(p+Vector3.up*.4f,p+Vector3.up*1.3f,.35f,1<<9)&&!CombatActor.All.Any(a=>a.Alive&&(a.transform.position-p).sqrMagnitude<Mathf.Pow(a.Radius+.4f,2));
        public bool CastSelf(int i)=>CanPrepare(i)&&(Knight?i is 2 or 3:i==3)&&Begin(i);
        private bool Begin(int i)
        {
            basic.Cancel();Feedback="";casting=i;castRank=Progression.Rank(i);began=Time.time;origin=transform.position;wave=0;IsChanneling=true;
            Casted?.Invoke(i);return true;
        }
        private bool Commit()
        {
            if(!mana.TrySpend(ResourceIds.Mana,Cost(Knight,casting,castRank)))return false;
            cooldowns[casting]=Time.time+Cooldown(Knight,casting);castStats=actor.Stats.Attributes;
            castImages=images.Where(i=>i!=null&&i.Alive&&!i.Knight&&!i.Secondary).ToArray();return true;
        }
        private void Update()
        {
            images.RemoveAll(i=>i==null);if(Charges>0&&Time.time>=chargesExpire)Charges=0;
            if(haste>0&&Time.time>=hasteExpire){actor.Stats.RemoveAttackSpeedPercentModifier("knight.image-end");haste=0;}
            int pr=Active&&actor.Alive?Progression.Rank(4):0;
            if(pr!=personalRank){personalRank=pr;SetPersonal(pr);}
            PsionSpeedPresence.Tick();
            for(int n=echoes.Count-1;n>=0;n--)
            {
                var echo=echoes[n];if(Time.time<echo.at)continue;echoes.RemoveAt(n);
                if(echo.image!=null&&echo.image.Alive)ResolveEcho(echo);
            }
            if(!IsChanneling)return;
            if(!Active||!actor.CanAct||actor.Motor.Travelling||(transform.position-origin).sqrMagnitude>.0225f){CancelChannel();return;}
            float elapsed=Time.time-began;
            if(!Knight&&casting==2)
            {
                while(wave<3&&elapsed>=.1f+.3f*wave)
                {
                    if(wave==0&&!Commit()){CancelChannel();return;}
                    float raw=Raw(false,2,castRank,castStats);Pulse(actor,point,raw,wave);QueueEcho(2,raw,wave);wave++;
                }
                if(wave==3)CancelChannel();return;
            }
            if(elapsed<CastTime(Knight,casting))return;
            if(!Knight&&casting==0&&(!actor.CanTargetHostile(target)||!ValidBolt(actor,target))||casting==1&&!FreePoint(point)){CancelChannel();return;}
            if(!Commit()){CancelChannel();return;}
            float amount=Raw(Knight,casting,castRank,castStats);
            if(casting==1){QueueEcho(1,amount);Summon(point,castRank,false,null);}
            else if(Knight&&casting==3){Charges=1+castRank/3;chargesExpire=Time.time+12;EmpowermentRank=castRank;}
            else if(Knight){ownShield?.Expire();ownShield=ApplyShield(actor,transform.position,amount,6,0,voidElement);}
            else if(casting==0){LaunchBolt(actor,target,amount,Mathf.Min(3,.15f*castRank));QueueEcho(0,amount);}
            else {ownShield?.Expire();ownShield=ApplyShield(actor,transform.position,amount,8,10,metal);QueueEcho(3,amount);}
            CancelChannel();
        }
        private void SetPersonal(int r)
        {
            if(r==0){actor.Stats.RemoveAttackSpeedPercentModifier("psion3.personal");actor.Stats.RemoveMovementSpeedPercentModifier("psion3.personal");}
            else {actor.Stats.SetAttackSpeedPercentModifier("psion3.personal",r);actor.Stats.SetMovementSpeedPercentModifier("psion3.personal",r);}
        }
        public int EmpowermentRank {get;private set;}
        private void OnLanded(CombatActor victim,DamageResult result)
        {
            if(!Active||!Knight||Charges<=0||ChargeRemaining<=0)return;
            if(!mana.TrySpend(ResourceIds.Mana,4+2*EmpowermentRank)){Feedback="Not enough Mana for an empowered strike.";return;}
            Charges--;if(victim!=null&&victim.Alive)ApplyRaw(actor,victim,voidHit,Raw(true,3,EmpowermentRank,actor.Stats.Attributes));
            EarlySkillVfx.PlayPriestPulse("Empowered mindblade",victim.transform.position,.7f,new Color(.8f,.45f,1));
        }
        internal PsionEvolutionImage Summon(Vector3 p,int rank,bool secondary,PsionEvolutionImage parent,float explosionOverride=-1)
        {
            var image=PsionEvolutionImage.Summon(this,p,rank,secondary,parent,Knight?knightVisual:mageVisual,apparition,explosionOverride);
            images.Add(image);return image;
        }
        public void GrantImageHaste(int rank)
        {
            if(!Active||!Knight||!actor.Alive)return;
            float value=Mathf.Lerp(1,50,(rank-1)/24f),duration=Mathf.Lerp(1,5,(rank-1)/24f);
            if(haste>value&&Time.time<hasteExpire)return;
            haste=value;hasteExpire=Time.time+duration;actor.Stats.SetAttackSpeedPercentModifier("knight.image-end",value);
            EarlySkillVfx.PlayPriestPulse("Mirrored momentum",transform.position,1.1f,new Color(.8f,.65f,1));
        }
        internal void HitImageTarget(CombatActor source,CombatActor victim,float raw)=>ApplyRaw(source,victim,voidHit,raw);
        internal void CopyPersonalShield(CombatActor image,float fraction,float lifetime)
        {
            if(Knight&&ownShield?.Remaining>0)
                image.Health.AddSharedShield(new SharedDamageShield(ownShield.Remaining*fraction,Mathf.Min(lifetime,ownShield.DurationRemaining),voidElement));
        }
        internal void Explode(CombatActor source,Vector3 p,float raw)
        {
            foreach(var victim in CombatActor.All.ToArray())if(victim.Alive&&victim.Faction!=source.Faction&&(victim.transform.position-p).sqrMagnitude<=16)ApplyRaw(source,victim,rupture,raw);
            EarlySkillVfx.PlayPriestPulse("Mirage rupture",p,4,new Color(.65f,.4f,1));
        }
        internal static void ApplyRaw(CombatActor source,CombatActor victim,AttackDefinition attack,float raw)
        {if(source!=null&&victim!=null&&victim.Alive)victim.Health.ApplyPacket(source,DamageResolver.CalculateRaw(source.Stats,victim.Stats,attack,raw));}
        private bool ValidBolt(CombatActor source,CombatActor victim)=>Vector3.Distance(source.transform.position,victim.transform.position)<=7+source.Radius+victim.Radius&&TargetedCombatProjectile.HasClearLaunch(source,victim);
        private void LaunchBolt(CombatActor source,CombatActor victim,float raw,float stun)
        {
            int version=session;
            TargetedCombatProjectile.Launch(source,victim,"Mirage Psychic Bolt",new Color(.8f,.85f,1),.24f,24,recipient=>{
                if(version!=session)return;ApplyRaw(source,recipient,bolt,raw);if(stun>0&&recipient.Alive)recipient.Effects?.TryApplyStun(stun,false);
            });
        }
        private void Pulse(CombatActor source,Vector3 p,float raw,int waveIndex)
        {
            foreach(var victim in CombatActor.All.ToArray())if(victim.Alive&&victim.Faction!=source.Faction&&(victim.transform.position-p).sqrMagnitude<=16)ApplyRaw(source,victim,storm,raw);
            PsionStormVisual.Play(p,4,waveIndex);
        }
        private SharedDamageShield ApplyShield(CombatActor source,Vector3 p,float amount,float duration,float radius,ElementDefinition element)
        {
            var shield=new SharedDamageShield(amount,duration,element);
            foreach(var ally in CombatActor.All)
            {
                if(!ally.Alive||ally.Faction!=source.Faction||radius==0&&ally!=source||(ally.transform.position-p).sqrMagnitude>radius*radius&&ally!=source)continue;
                ally.Health.AddSharedShield(shield);EarlySkillVfx.PlayPriestPulse("Psionic barrier",ally.transform.position,.8f,new Color(.7f,.65f,1));
            }
            return shield;
        }
        private sealed class Echo {public PsionEvolutionImage image;public int skill,rank,wave;public float at,raw;public Vector3 point;public CombatActor target;}
        private void QueueEcho(int skill,float raw,int waveIndex=0)
        {
            foreach(var image in castImages)if(image.Alive)echoes.Add(new Echo{image=image,skill=skill,rank=castRank,wave=waveIndex,raw=raw,point=point,target=target,at=Time.time+.4f});
        }
        private void ResolveEcho(Echo e)
        {
            var source=e.image.Actor;float raw=e.raw*MirrorFraction(e.image.Rank);
            if(e.skill==0)
            {
                var victim=e.target;
                if(victim==null||!victim.Alive)victim=CombatActor.All.Where(v=>source.CanTargetHostile(v)&&ValidBolt(source,v)).OrderBy(v=>(v.transform.position-source.transform.position).sqrMagnitude).FirstOrDefault();
                if(source.CanTargetHostile(victim)&&ValidBolt(source,victim))LaunchBolt(source,victim,raw,0);
            }
            else if(e.skill==2)Pulse(source,e.point,raw,e.wave);
            else if(e.skill==3)ApplyShield(source,source.transform.position,raw,8,8,metal);
            else
            {
                // The original image occupies the selected point. Find a nearby clear ground position.
                for(int i=0;i<16;i++)
                {
                    var offset=new Vector3(Mathf.Cos(i*Mathf.PI/4),0,Mathf.Sin(i*Mathf.PI/4))*(1.8f+i/8f);
                    if(NavMesh.SamplePosition(e.point+offset,out var hit,1,1)&&FreePoint(hit.position)){Summon(hit.position,e.rank,true,e.image,e.raw);break;}
                }
            }
        }
        public bool TryInterruptCast(){if(!IsChanneling)return false;CancelChannel();return true;}
        public void CancelChannel(){IsChanneling=false;target=null;}
        public void ResetForManifestationLoad()
        {
            CancelChannel();session++;Charges=0;haste=0;personalRank=-1;echoes.Clear();foreach(var i in images)if(i!=null)i.Dismiss();images.Clear();
            ownShield?.Expire();ownShield=null;Array.Clear(cooldowns,0,5);actor.Stats.RemoveAttackSpeedPercentModifier("knight.image-end");SetPersonal(0);PsionSpeedPresence.Invalidate(actor);
        }
        public void Configure(AttackDefinition boltAttack,AttackDefinition stormAttack,AttackDefinition blast,AttackDefinition empowered,ElementDefinition metalType,ElementDefinition voidType,GameObject knightForm,GameObject mageForm,Material spectral)
        {bolt=boltAttack;storm=stormAttack;rupture=blast;voidHit=empowered;metal=metalType;voidElement=voidType;knightVisual=knightForm;mageVisual=mageForm;apparition=spectral;}
    }
}
