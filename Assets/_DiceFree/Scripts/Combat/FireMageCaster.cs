using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Foundation;
using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.Combat
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor),typeof(FireMageProgression),typeof(ActorResourceController))]
    public sealed class FireMageCaster : MonoBehaviour,IManifestationSessionState,IIncomingDamageRedirector
    {
        public const string HeatId="resource.heat", ShieldId="fire-mage.flame-ward";
        [SerializeField] private AttackDefinition fire;
        private CombatActor actor;
        private FireMageProgression ranks;
        private ActorResourceController resources;
        private readonly float[] readyAt=new float[6];
        private float lastFire,wardUntil;
        private int wardRank,casting=-1;
        public bool Busy=>casting>=0;
        public bool Active=>isActiveAndEnabled&&ranks.Active&&actor.Alive;
        public float Heat=>resources.Current(HeatId);
        public float WardRemaining=>Mathf.Max(0,wardUntil-Time.time);
        public string Feedback{get;private set;}="";
        public event Action<int> Casted;
        private sealed class Effect
        {
            public int kind,ticks;
            public CombatActor target;
            public Vector3 position;
            public float next;
            public bool spread;
            public Snapshot shot;
        }
        // Offense is captured on application; target mitigation is resolved per packet.
        private readonly struct Snapshot
        {
            public readonly float raw,elementPenetration;
            public readonly DefenseModifier penetration;
            public Snapshot(ActorStats stats,AttackDefinition attack,float amount)
            {raw=amount;penetration=stats.DefenseModifiers(attack.channel);elementPenetration=stats.Penetration(attack.element,ElementalContext.Damage);}
            private Snapshot(float amount,DefenseModifier defense,float elemental){raw=amount;penetration=defense;elementPenetration=elemental;}
            public Snapshot Scale(float factor)=>new(raw*factor,penetration,elementPenetration);
        }
        private readonly List<Effect> effects=new();
        private readonly Dictionary<CombatActor,Action> deathHooks=new();
        private readonly Queue<Effect> detonationQueue=new();
        private readonly List<GameObject> visuals=new();
        private bool detonating;
        private static readonly HashSet<FireMageCaster> auras=new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ClearRegistry()=>auras.Clear();
        public static int ManaCost(int i,int r)=>i==0?20+15*r:i==1?10+6*r:i==2?30+15*r:i==3?50+15*r:0;
        public static float Cooldown(int i,int r)=>i==0?Mathf.Lerp(14,7,(r-1)/34f):i==1?Mathf.Lerp(28,20,(r-1)/24f):i==2?Mathf.Lerp(14,10,(r-1)/29f):i==3?Mathf.Lerp(22,15,(r-1)/29f):0;
        public static Vector2 Formula(int i,int r)=>i==0?new(30+8*r,.3f+.04f*r):i==2?new(9.5f+68*(r-1)/29f,.085f+.34f*(r-1)/29f):new(57+408*(r-1)/29f,.51f+2.04f*(r-1)/29f);
        public static float WardFraction(int r)=>Mathf.Lerp(.30f,.65f,(r-1)/24f);
        public static float WardEfficiency(int r)=>Mathf.Lerp(1.9f,1.3f,(r-1)/24f);
        public static float WardDuration(int r)=>Mathf.Lerp(4,10,(r-1)/24f);
        public static float ShieldAmount(int r,float intelligence)=>50+(.5f+.1f*r)*intelligence;
        public float PreviewDamage(int i,int rank){var f=Formula(i,rank);return f.x+f.y*actor.Stats.Attributes.intelligence;}
        public float Remaining(int i)=>(uint)i<6?Mathf.Max(0,readyAt[i]-Time.time):0;
        public float Range=>12;
        public int ActiveEffects=>effects.Count;
        private void Awake(){actor=GetComponent<CombatActor>();ranks=GetComponent<FireMageProgression>();resources=GetComponent<ActorResourceController>();}
        private void OnEnable()
        {
            auras.Add(this);actor.Health.Died+=ResetForManifestationLoad;
            GroundAuraVisual.Create(this,actor,GroundAuraStyle.Infernal,()=>Active&&ranks.Rank(5)>0);
        }
        private void OnDisable(){auras.Remove(this);if(actor?.Health!=null)actor.Health.Died-=ResetForManifestationLoad;ResetForManifestationLoad();}
        public static float ResistanceFor(ActorStats stats,ElementDefinition element)
        {
            if(element?.stableId!="element.fire"||!stats.TryGetComponent<CombatActor>(out var target)||!target.Alive)return 0;
            float allies=0,enemies=0;
            foreach(var aura in auras)
                if(aura!=null&&aura.Active&&aura.ranks.Rank(5)>0&&(aura.transform.position-target.transform.position).sqrMagnitude<=225)
                {float amount=.005f*aura.ranks.Rank(5);if(aura.actor.IsHostileTo(target))enemies=Mathf.Max(enemies,amount);else allies=Mathf.Max(allies,amount);}
            return allies-enemies;
        }
        private void Update()
        {
            if(!Active)return;
            if(Busy&&!actor.CanAct){StopAllCoroutines();casting=-1;Feedback="Cast interrupted.";}
            if(Time.time>lastFire+10&&Heat>0)resources.TrySpend(HeatId,Mathf.Min(Heat,10*Time.deltaTime));
            foreach(var effect in effects.ToArray())
            {
                if(!effects.Contains(effect)||Time.time<effect.next)continue;
                effect.next+=1;
                if(effect.kind==3){QueueDetonation(effect);continue;}
                if(effect.kind==0){foreach(var target in Enemies(effect.position,4))Apply(target,effect.shot);}
                else if(effect.target!=null&&effect.target.Alive){Apply(effect.target,effect.shot);Track(MageSpellVfx.EmberImpact(effect.target.transform.position));}
                else{effects.Remove(effect);continue;}
                if(--effect.ticks<=0)effects.Remove(effect);
            }
            foreach(var target in deathHooks.Keys.ToArray())if(target==null||!effects.Any(e=>e.target==target))Unhook(target);
            visuals.RemoveAll(v=>v==null);
        }
        public bool CanPrepare(int i)
        {
            if((uint)i>=4)return Fail("This skill is passive.");
            if(!Active||!actor.CanAct||GetComponent<RunLoadoutLock>()?.AbilitiesBlocked==true)return Fail("Spells are unavailable during this trial.");
            if(ranks.Rank(i)==0)return Fail("Learn this spell first.");
            if(Busy||Remaining(i)>0)return Fail("Spell is recovering.");
            if(!resources.CanSpend(ResourceIds.Mana,ManaCost(i,ranks.Rank(i))))return Fail("Not enough Mana.");
            return true;
        }
        public bool Cast(int i,CombatActor target=null)
        {
            if(!CanPrepare(i))return false;
            if(i!=1&&(!actor.IsHostileTo(target)||!actor.HasSightOf(target)||Vector3.Distance(transform.position,target.transform.position)>Range+actor.Radius+target.Radius))return Fail("Choose a visible hostile target within 12m.");
            int rank=ranks.Rank(i);bool empowered=Heat>=100;
            if(!resources.TrySpend(ResourceIds.Mana,ManaCost(i,rank)))return false;
            if(empowered)resources.TrySpend(HeatId,100);
            lastFire=Time.time;readyAt[i]=Time.time+Cooldown(i,rank);casting=i;
            GetComponent<BasicAttack>()?.Cancel();Casted?.Invoke(i);
            // Accepted spells keep their resource/cooldown cost if interrupted; Heat
            // generation occurs only once an ordinary spell is actually released.
            if(i==0||i==3)StartCoroutine(Release(i,rank,empowered,target));else Execute(i,rank,empowered,target);
            return true;
        }
        private IEnumerator Release(int i,int rank,bool empowered,CombatActor target)
        {
            yield return new WaitForSeconds(.2f);
            if(!Active||!actor.CanAct){casting=-1;yield break;}
            Execute(i,rank,empowered,target);
        }
        private void Execute(int i,int rank,bool empowered,CombatActor target)
        {
            casting=-1;
            if(i!=1&&(target==null||!target.Alive||!actor.IsHostileTo(target))){Feedback="Target lost.";return;}
            if(!empowered)resources.Restore(HeatId,10+ranks.Rank(4));
            lastFire=Time.time;Feedback=(empowered?"Empowered ":"")+FireMageProgression.Name(i);
            if(i==1)
            {
                wardRank=rank;wardUntil=Time.time+WardDuration(rank);
                if(empowered)actor.Health.AddTemporaryShield(ShieldId,ShieldAmount(rank,actor.Stats.Attributes.intelligence),10,"element.fire");
                Track(MageSpellVfx.EmberImpact(transform.position));return;
            }
            var shot=new Snapshot(actor.Stats,fire,PreviewDamage(i,rank));
            if(i==0)
            {
                float flight=Mathf.Clamp(Vector3.Distance(transform.position,target.transform.position)/18,.1f,.7f);
                Track(MageSpellVfx.Bolt(transform.position+Vector3.up*1.2f,target,flight,new Color(1,.28f,.025f)));
                StartCoroutine(Projectile(target,shot,empowered,flight));
            }
            else if(i==2)
            {
                var old=effects.FirstOrDefault(e=>e.kind==2&&e.target==target);float amount=shot.raw*(empowered?3:1);
                if(old!=null){if(old.shot.raw>amount)shot=old.shot;else shot=shot.Scale(empowered?3:1);effects.Remove(old);}else shot=shot.Scale(empowered?3:1);
                Add(new Effect{kind=2,target=target,next=Time.time+1,ticks=8,shot=shot});Track(MageSpellVfx.EmberImpact(target.transform.position));
            }
            else AddBomb(target,shot,empowered);
        }
        private IEnumerator Projectile(CombatActor target,Snapshot shot,bool empowered,float delay)
        {
            yield return new WaitForSeconds(delay);
            if(!Active||target==null||!target.Alive)yield break;
            var point=target.transform.position;Apply(target,shot);Track(MageSpellVfx.EmberImpact(point));
            if(empowered){effects.Add(new Effect{kind=0,position=point,next=Time.time+1,ticks=6,shot=shot.Scale(.25f)});Track(FireMageVfx.Ground(point,4,6,new Color(1,.23f,.015f)));}
        }
        private void Add(Effect effect)
        {
            effects.Add(effect);var target=effect.target;
            if(target!=null&&!deathHooks.ContainsKey(target)){Action action=()=>OnCarrierDeath(target);deathHooks.Add(target,action);target.Health.Died+=action;}
        }
        private void Unhook(CombatActor target)
        {if(deathHooks.Remove(target,out var action)&&target!=null&&target.Health!=null)target.Health.Died-=action;}
        private void AddBomb(CombatActor target,Snapshot shot,bool empowered)
        {Add(new Effect{kind=3,target=target,position=target.transform.position,next=Time.time+3,shot=shot,spread=empowered});Track(FireMageVfx.Bomb(target,3,empowered));}
        private void OnCarrierDeath(CombatActor target)
        {foreach(var bomb in effects.Where(e=>e.kind==3&&e.target==target).ToArray())QueueDetonation(bomb);}
        private void QueueDetonation(Effect bomb)
        {
            if(!effects.Remove(bomb))return;
            if(bomb.target!=null)bomb.position=bomb.target.transform.position;
            detonationQueue.Enqueue(bomb);if(detonating)return;
            detonating=true;
            try
            {
                while(detonationQueue.Count>0)
                {
                    var next=detonationQueue.Dequeue();var targets=Enemies(next.position,3);
                    Track(MageSpellVfx.EmberImpact(next.position));Track(FireMageVfx.Ground(next.position,3,.45f,new Color(1,.55f,.07f)));
                    // Attach secondaries before resolving damage so lethal initial
                    // explosions trigger their immediate death detonation as well.
                    if(next.spread)foreach(var victim in targets)if(victim!=next.target)AddBomb(victim,next.shot,false);
                    foreach(var victim in targets)if(victim.Alive)Apply(victim,next.shot.Scale(next.spread?2:1));
                }
            }
            finally{detonating=false;}
        }
        private CombatActor[] Enemies(Vector3 point,float radius)=>CombatActor.All.Where(t=>actor.IsHostileTo(t)&&Vector3.Distance(t.transform.position,point)<=radius+t.Radius).ToArray();
        private void Apply(CombatActor target,Snapshot shot)
        {
            if(target==null||!target.Alive)return;
            float defense=DefenseMath.Effective(target.Stats.Defense(fire.channel),target.Stats.DefenseModifiers(fire.channel),shot.penetration);
            var resistance=new ElementalResistanceResolution(target.Stats.RawResistance(fire.element),shot.elementPenetration,target.Stats.ResistanceCap(fire.element),ElementalContext.Damage);
            float after=shot.raw*DefenseMath.DamageMultiplier(defense,target.Stats.Definition.tuning);
            target.Health.ApplyPacket(actor,new DamageResult{baseRaw=shot.raw,raw=shot.raw,defense=defense,channel=fire.channel,element=fire.element,afterDefense=after,resistance=resistance.effective,elementalResistance=resistance,mitigated=after*(1-resistance.effective)*target.Stats.IncomingDamageMultiplier(fire.channel)});
        }
        public float RedirectDamage(float incoming)
        {
            if(!Active||Time.time>=wardUntil||incoming<=0)return incoming;
            float efficiency=WardEfficiency(wardRank),blocked=Mathf.Min(incoming*WardFraction(wardRank),resources.Current(ResourceIds.Mana)/efficiency);
            return blocked>0&&resources.TrySpend(ResourceIds.Mana,blocked*efficiency)?incoming-blocked:incoming;
        }
        private void Track(GameObject visual){if(visual!=null)visuals.Add(visual);}
        public void ResetForManifestationLoad()
        {
            StopAllCoroutines();casting=-1;wardUntil=0;wardRank=0;Array.Clear(readyAt,0,6);effects.Clear();detonationQueue.Clear();
            foreach(var target in deathHooks.Keys.ToArray())Unhook(target);
            foreach(var visual in visuals)if(visual!=null)Destroy(visual);visuals.Clear();
            actor?.Health?.RemoveTemporaryShield(ShieldId);
            if(resources!=null&&Heat>0)resources.TrySpend(HeatId,Heat);lastFire=Time.time;
        }
        public void Configure(AttackDefinition definition)=>fire=definition;
        private bool Fail(string message){Feedback=message;return false;}
    }
}
