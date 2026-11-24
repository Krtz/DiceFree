using System.Linq;
using DiceFree.Combat;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent]
    public sealed class PsionEvolutionImage : MonoBehaviour
    {
        private PsionEvolutionCaster owner;
        private CombatActor actor,windupTarget;
        private BasicAttack ownerBasic;
        private ActorDefinition definition;
        private CriticalRule critical;
        private Animator animator;
        private Vector2 damage;
        private float expires,nextAttack,impactAt,interval,windup,range,explosion,localChargeExpiry,localVoid;
        private float copiedHasteUntil;
        private int localCharges;
        private bool ended,dismissed;
        public bool Knight {get;private set;}
        public bool Secondary {get;private set;}
        public int Rank {get;private set;}
        public CombatActor Actor=>actor;
        public bool Alive=>!ended&&actor!=null&&actor.Alive&&Time.time<expires;
        public float Remaining=>Mathf.Max(0,expires-Time.time);
        public Vector2 SnapshotDamage=>damage;
        public int Charges=>localCharges;
        public float ExplosionRaw=>explosion;
        public static PsionEvolutionImage Summon(PsionEvolutionCaster caster,Vector3 point,int rank,bool secondary,PsionEvolutionImage primary,GameObject form,Material material,float explosionOverride=-1)
        {
            var root=new GameObject(secondary?"Secondary Mirage":"Mental Image — "+(caster.Knight?"Psionic Knight":"Mirage Mage")){layer=10};root.SetActive(false);
            SceneManager.MoveGameObjectToScene(root,caster.gameObject.scene);root.transform.SetPositionAndRotation(point,caster.transform.rotation);
            var image=root.AddComponent<PsionEvolutionImage>();image.owner=caster;image.Knight=caster.Knight;image.Secondary=secondary;image.Rank=rank;
            var source=caster.Actor;image.ownerBasic=source.GetComponent<BasicAttack>();
            float fraction=image.Knight?PsionEvolutionCaster.ImageFraction(rank):.1f;
            image.damage=secondary?primary.damage*.5f:image.ownerBasic.DamageRange*fraction;
            image.range=image.ownerBasic.Reach;
            image.critical=caster.BasicCriticalRule();
            image.explosion=explosionOverride>=0?explosionOverride:secondary?primary.explosion:PsionEvolutionCaster.Raw(false,1,rank,source.Stats.Attributes);
            float life=PsionEvolutionCaster.ImageDuration(rank)*(secondary?.5f:1);
            image.expires=Time.time+life;image.interval=Mathf.Max(image.ownerBasic.Definition.interval,image.ownerBasic.Definition.windup)/source.Stats.AttackSpeed;
            image.windup=image.ownerBasic.Definition.windup/source.Stats.AttackSpeed;
            image.definition=ScriptableObject.CreateInstance<ActorDefinition>();
            image.definition.stableId="summon."+(image.Knight?"psionic-knight":"mirage-mage")+(secondary?".secondary":".image");
            image.definition.displayName=secondary?"Secondary Mirage":"Mental Image";image.definition.tuning=source.Stats.Definition.tuning;
            image.definition.baseHp=secondary?primary.actor.Health.Maximum*.5f:source.Health.Maximum;
            image.definition.overrideVitalityHp=true;image.definition.hpPerVitality=0;image.definition.overrideVitalityRegeneration=true;
            image.definition.regenerationPerVitality=0;image.definition.baseAttributes=source.Stats.Attributes;
            image.definition.moveSpeed=source.Stats.Definition.moveSpeed;image.definition.basicAttack=image.ownerBasic.Definition;
            image.actor=root.AddComponent<CombatActor>();image.actor.Configure(source.Faction,.32f);
            var stats=root.GetComponent<ActorStats>();stats.Configure(image.definition);stats.ConfigureIncomingDamageScale(3.5f);
            stats.SetEquipmentContribution("image.speed-equipment",new EquipmentStats{attackSpeedPercent=source.Stats.EquipmentAttackSpeedPercent,movementSpeedPercent=source.Stats.EquipmentMovementSpeedPercent});
            int presence=caster.Progression.Rank(4);
            stats.SetAttackSpeedPercentModifier("image.inherited-passive",presence);stats.SetMovementSpeedPercentModifier("image.inherited-passive",presence);
            if(image.Knight&&caster.HastePercent>0){stats.SetAttackSpeedPercentModifier("image.copied-haste",caster.HastePercent*fraction);image.copiedHasteUntil=Time.time+Mathf.Min(life,caster.HasteRemaining);}
            image.interval=Mathf.Max(image.ownerBasic.Definition.interval,image.ownerBasic.Definition.windup);image.windup=image.ownerBasic.Definition.windup;
            var penetration=source.Stats.DefenseModifiers(DamageChannel.Physical);
            penetration.penetrationFlat+=caster.BasicPenetration;stats.SetDefenseModifier("image.passive",penetration);
            root.AddComponent<CombatStatusController>();root.AddComponent<DefeatReporter>().SetCreditOwner(source);
            var capsule=root.AddComponent<CapsuleCollider>();capsule.center=Vector3.up*.8f;capsule.height=1.6f;capsule.radius=.32f;
            if(image.Knight&&caster.Charges>0&&caster.ChargeRemaining>0)
            {
                image.localCharges=caster.Charges;image.localChargeExpiry=Time.time+Mathf.Min(life,caster.ChargeRemaining);
                image.localVoid=PsionEvolutionCaster.Raw(true,3,caster.EmpowermentRank,source.Stats.Attributes)*fraction;
            }
            if(form!=null)
            {
                var visual=Instantiate(form,root.transform);visual.name="Spectral reflection";visual.SetActive(true);
                image.animator=visual.GetComponentInChildren<Animator>(true);
                foreach(var r in visual.GetComponentsInChildren<Renderer>(true))
                {if(material!=null)r.sharedMaterials=r.sharedMaterials.Select(_=>material).ToArray();r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
            }
            root.SetActive(true);if(!image.Knight)image.actor.Motor.SetMotionBlocked("stationary-mirage",true);
            source.Effects?.CopyPositiveBuffsTo(image.actor.Effects,image.Knight?fraction:1,life);
            if(image.Knight)caster.CopyPersonalShield(image.actor,fraction,life);
            return image;
        }
        private void OnEnable()=>actor.Health.Died+=End;
        private void OnDisable(){if(actor!=null)actor.Health.Died-=End;}
        private void OnDestroy(){if(definition!=null)Destroy(definition);}
        private bool Valid(CombatActor v)=>actor.CanTargetHostile(v)&&(v.transform.position-transform.position).sqrMagnitude<=(Knight?144:Mathf.Pow(range+actor.Radius+v.Radius,2));
        private void Update()
        {
            if(ended)return;if(Time.time>=expires){End();return;}if(copiedHasteUntil>0&&Time.time>=copiedHasteUntil){actor.Stats.RemoveAttackSpeedPercentModifier("image.copied-haste");copiedHasteUntil=0;}if(!actor.CanAct){windupTarget=null;return;}
            var selected=owner!=null?owner.GetComponent<TargetSelection>()?.Selected:null;
            if(!Valid(selected))selected=ownerBasic?.Target;
            if(!Valid(selected))selected=CombatActor.All.Where(Valid).OrderBy(v=>(v.transform.position-transform.position).sqrMagnitude).FirstOrDefault();
            if(selected==null)
            {
                windupTarget=null;
                if(Knight&&owner!=null&&owner.Actor.Alive&&(owner.transform.position-transform.position).sqrMagnitude>9)actor.Motor.MoveTo(owner.transform.position-owner.transform.forward*2);
                else actor.Motor.Stop();return;
            }
            bool inRange=Vector3.Distance(transform.position,selected.transform.position)<=range+actor.Radius+selected.Radius&&actor.HasSightOf(selected);
            if(!inRange){windupTarget=null;if(Knight)actor.Motor.MoveTo(selected.transform.position);return;}
            actor.Motor.Stop();var heading=Vector3.ProjectOnPlane(selected.transform.position-transform.position,Vector3.up);if(heading.sqrMagnitude>.001f)transform.rotation=Quaternion.LookRotation(heading);
            if(windupTarget!=null)
            {
                if(Time.time<impactAt)return;var victim=windupTarget;windupTarget=null;
                if(Valid(victim)&&Vector3.Distance(transform.position,victim.transform.position)<=range+actor.Radius+victim.Radius&&actor.HasSightOf(victim))Hit(victim);
            }
            else if(Time.time>=nextAttack)
            {windupTarget=selected;impactAt=Time.time+windup/actor.Stats.AttackSpeed;nextAttack=Time.time+interval/actor.Stats.AttackSpeed;animator?.CrossFadeInFixedTime("Cast",.06f);}
        }
        private void Hit(CombatActor victim)
        {
            var action=AccuracyResolver.Resolve(actor,definition.basicAttack,critical:critical,target:victim,isBasicAttack:true);
            var result=action.Hit.missed?new DamageResult{channel=DamageChannel.Physical}:DamageResolver.CalculateRaw(actor.Stats,victim.Stats,definition.basicAttack,Random.Range(damage.x,damage.y),action.Critical);
            result.action=action;victim.Health.ApplyPacket(actor,result);
            if(!action.Hit.missed&&localCharges>0&&Time.time<localChargeExpiry)
            {localCharges--;if(victim.Alive)owner.HitImageTarget(actor,victim,localVoid);}
            if(!action.Hit.missed)actor.Effects?.ResolveBasicAttackAugments(actor,victim);
            EarlySkillVfx.PlayPriestPulse("Reflected mindblade",victim.transform.position,.35f,new Color(.7f,.6f,1));
        }
        private void End()
        {
            if(ended)return;ended=true;
            if(!dismissed&&owner!=null){if(Knight)owner.GrantImageHaste(Rank);else owner.Explode(actor,transform.position,explosion);}
            gameObject.SetActive(false);Destroy(gameObject,1.6f);
        }
        public void Dismiss(){dismissed=true;End();}
    }
}
