using System;using System.Linq;using DiceFree.Combat;using DiceFree.AI;using UnityEngine;
namespace DiceFree.Briarback
{
    // Keep the original values stable because the provisional Briarback scene
    // serializes them. New production roles are appended as the region is migrated.
    public enum QuilboarRole { Raider, SpearThrower, Totemcaller, Harrowtusk, Warrior, Brute, FireShaman, EarthShaman, LightningShaman, IceShaman }
    [DefaultExecutionOrder(-10),DisallowMultipleComponent,RequireComponent(typeof(CombatActor),typeof(BasicAttack),typeof(AggroBehaviour))]
    public sealed class QuilboarCombat:MonoBehaviour,IBasicAttackApproachProvider
    {
        public QuilboarRole role;public AttackDefinition slam;
        [Min(0)] public float protectionRadius=6f;
        CombatActor actor;BasicAttack basic;float readyAt,impactAt,shieldReadyAt,shieldUntil;Vector3 impactCenter;
        CombatActor spearMeleeAttacker;float spearMeleeReach;
        GameObject warning;Material warningMaterial;Mesh warningMesh;
        const string Block="briarback.slam";
        public bool Winding {get;private set;}
        public bool IsShaman=>role==QuilboarRole.Totemcaller||role==QuilboarRole.FireShaman||
            role==QuilboarRole.EarthShaman||role==QuilboarRole.LightningShaman||role==QuilboarRole.IceShaman;
        public bool SpearInMelee=>role==QuilboarRole.SpearThrower&&spearMeleeAttacker!=null;
        public bool WarriorShieldActive=>role==QuilboarRole.Warrior&&Time.time<shieldUntil;
        public Vector3 ImpactCenter=>impactCenter;
        public int SlamHits {get;private set;}
        public float CooldownRemaining=>Mathf.Max(0,readyAt-Time.time);
        public event Action SlamStarted;
        void Awake(){actor=GetComponent<CombatActor>();basic=GetComponent<BasicAttack>();}
        void OnEnable(){actor.Health.Died+=Cancel;actor.Health.Restored+=Restored;actor.Health.Damaged+=OnDamaged;}
        void OnDisable(){if(actor?.Health!=null){actor.Health.Died-=Cancel;actor.Health.Restored-=Restored;actor.Health.Damaged-=OnDamaged;}ClearSpearStance();ClearWarriorShield();Cancel();}
        void Restored(){ClearSpearStance();ClearWarriorShield();Cancel();readyAt=Time.time+4;shieldReadyAt=Time.time+15;}
        void OnDamaged(CombatActor source,DamageResult result){
            // Landed melee hits are the prototype trigger; the design also leaves
            // attack-attempt versus landed-hit behavior open for later review.
            if(role!=QuilboarRole.SpearThrower||source==null||result.channel!=DamageChannel.Physical||!actor.IsHostileTo(source))return;
            var incoming=source.GetComponent<BasicAttack>();
            if(incoming==null||incoming.Target!=actor||incoming.Definition==null||incoming.Definition.reach>2f||!incoming.InRange(actor))return;
            spearMeleeAttacker=source;spearMeleeReach=incoming.Definition.reach;basic.ReachOverride=spearMeleeReach;
        }
        void ClearSpearStance(){spearMeleeAttacker=null;spearMeleeReach=0;if(basic!=null)basic.ReachOverride=-1f;}
        public bool TryRaiseWarriorShield(){
            if(role!=QuilboarRole.Warrior||!actor.CanAct||Time.time<shieldReadyAt)return false;
            shieldUntil=Time.time+2f;shieldReadyAt=Time.time+15f;
            // Both existing channels are covered and actions remain available as
            // provisional choices; final coverage/action rules are still open.
            actor.Stats.SetDamageTakenModifier("briarback.warrior-shield.physical",new DamageTakenModifier(DamageChannel.Physical,.7f));
            actor.Stats.SetDamageTakenModifier("briarback.warrior-shield.magical",new DamageTakenModifier(DamageChannel.Magical,.7f));
            return true;
        }
        void ClearWarriorShield(){
            shieldUntil=0;
            if(actor?.Stats==null)return;
            actor.Stats.RemoveDamageTakenModifier("briarback.warrior-shield.physical");
            actor.Stats.RemoveDamageTakenModifier("briarback.warrior-shield.magical");
        }
        bool IsRangedAlly(QuilboarCombat other)=>other!=null&&
            (other.role==QuilboarRole.SpearThrower||other.role==QuilboarRole.Totemcaller||
             other.role==QuilboarRole.FireShaman||other.role==QuilboarRole.EarthShaman||
             other.role==QuilboarRole.LightningShaman||other.role==QuilboarRole.IceShaman);
        /// <summary>
        /// If free of an existing engagement, take up the threat against a nearby
        /// ranged tribal ally. The six-metre guard radius is a provisional tuning
        /// value; the design leaves protection distance and threat weighting open.
        /// </summary>
        public bool TryProtectRangedAlly(){
            if(role!=QuilboarRole.Warrior||!actor.CanAct||basic.Target!=null||Winding)return false;
            var aggro=GetComponent<AggroBehaviour>();
            if(aggro==null||aggro.Returning||Vector3.Distance(transform.position,aggro.Home)>aggro.Leash)return false;
            CombatActor selected=null;float nearest=float.PositiveInfinity;
            foreach(var ally in CombatActor.All){
                if(ally==actor||!actor.IsFriendlyTo(ally)||
                    !IsRangedAlly(ally.GetComponent<QuilboarCombat>())||
                    Vector3.Distance(transform.position,ally.transform.position)>protectionRadius)continue;
                var threat=ally.GetComponent<BasicAttack>()?.Target;
                if(!actor.IsHostileTo(threat)||Vector3.Distance(threat.transform.position,aggro.Home)>aggro.Leash)continue;
                float distance=(threat.transform.position-transform.position).sqrMagnitude;
                if(distance<nearest){selected=threat;nearest=distance;}
            }
            // Engage through ordinary movement and body collision. This avoids a
            // bespoke taunt or overriding a Warrior's active target.
            return selected!=null&&basic.Order(selected);
        }
        /// <summary>
        /// While answering a threat to a nearby ranged ally, pursue a point just
        /// in front of that ally on the threat-to-ally line. BasicAttack retains
        /// ownership of target validation, range, facing, cooldown and impact.
        /// </summary>
        public bool TryGetApproachPosition(CombatActor target,out Vector3 position){
            position=default;
            if(role!=QuilboarRole.Warrior||target==null||!actor.IsHostileTo(target))return false;
            var aggro=GetComponent<AggroBehaviour>();
            if(aggro==null||aggro.Returning||Vector3.Distance(transform.position,aggro.Home)>aggro.Leash)return false;
            CombatActor selected=null;float nearest=float.PositiveInfinity;
            foreach(var ally in CombatActor.All){
                if(ally==actor||!actor.IsFriendlyTo(ally)||
                    !IsRangedAlly(ally.GetComponent<QuilboarCombat>())||
                    Vector3.Distance(transform.position,ally.transform.position)>protectionRadius||
                    ally.GetComponent<BasicAttack>()?.Target!=target)continue;
                float distance=(ally.transform.position-transform.position).sqrMagnitude;
                if(distance<nearest){selected=ally;nearest=distance;}
            }
            if(selected==null)return false;
            var towardThreat=Vector3.ProjectOnPlane(target.transform.position-selected.transform.position,Vector3.up);
            if(towardThreat.sqrMagnitude<.01f)return false;
            towardThreat.Normalize();
            float distanceToThreat=Vector3.Distance(selected.transform.position,target.transform.position);
            float standOff=Mathf.Min(selected.Radius+actor.Radius+.35f,
                Mathf.Max(.2f,distanceToThreat-target.Radius-actor.Radius-.2f));
            position=selected.transform.position+towardThreat*standOff;
            return true;
        }
        public bool BeginSlam(){
            if(role!=QuilboarRole.Harrowtusk||slam==null||Winding||!actor.CanAct||Time.time<readyAt)return false;
            Winding=true;readyAt=Time.time+9;impactAt=Time.time+1.1f;impactCenter=transform.position;
            basic.Cancel();actor.Motor.Stop();actor.SetActionBlocked(Block,true);actor.Motor.SetMotionBlocked(Block,true);
            ShowWarning();SlamStarted?.Invoke();return true;
        }
        void Update(){
            if(role==QuilboarRole.Warrior){
                TryProtectRangedAlly();
                if(Time.time>=shieldUntil)ClearWarriorShield();
                if(actor.CanAct&&basic.Target!=null&&Time.time>=shieldReadyAt)TryRaiseWarriorShield();
            }
            if(SpearInMelee&&(!spearMeleeAttacker.Alive||
                Vector3.Distance(transform.position,spearMeleeAttacker.transform.position)>
                actor.Radius+spearMeleeAttacker.Radius+spearMeleeReach))ClearSpearStance();
            if(Winding){
                if(!actor.Alive||actor.Effects?.Stunned==true||GetComponent<AggroBehaviour>().Returning){Cancel();return;}
                if(Time.time<impactAt)return;
                foreach(var target in CombatActor.All.ToArray()){
                    if(!actor.IsHostileTo(target)||Vector3.Distance(target.transform.position,impactCenter)>4+target.Radius||!actor.HasSightOf(target))continue;
                    DamageResolver.Hit(actor,target,slam);SlamHits++;
                }
                Cancel();return;
            }
            if(role==QuilboarRole.Harrowtusk&&basic.Target!=null&&Vector3.Distance(transform.position,basic.Target.transform.position)<4.5f)BeginSlam();
        }
        void ShowWarning(){
            warning=new GameObject("Harrowtusk slam - dodge amber circle");warning.layer=2;warning.transform.position=impactCenter+Vector3.up*.08f;
            var vertices=new Vector3[66];var triangles=new int[32*6];
            for(int i=0;i<=32;i++){float a=i*Mathf.PI*2/32;vertices[i*2]=new Vector3(Mathf.Cos(a)*3.7f,0,Mathf.Sin(a)*3.7f);vertices[i*2+1]=new Vector3(Mathf.Cos(a)*4,0,Mathf.Sin(a)*4);}
            for(int i=0;i<32;i++){int v=i*2,t=i*6;triangles[t]=v;triangles[t+1]=v+2;triangles[t+2]=v+1;triangles[t+3]=v+1;triangles[t+4]=v+2;triangles[t+5]=v+3;}
            warningMesh=new Mesh{name="Original slam warning ring",vertices=vertices,triangles=triangles};warningMesh.RecalculateNormals();
            warning.AddComponent<MeshFilter>().sharedMesh=warningMesh;
            warningMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit")){color=new Color(1,.5f,.06f)};
            warning.AddComponent<MeshRenderer>().sharedMaterial=warningMaterial;
        }
        public void Cancel(){
            if(actor==null||!actor.Alive)ClearWarriorShield();
            Winding=false;if(actor!=null){actor.SetActionBlocked(Block,false);actor.Motor.SetMotionBlocked(Block,false);}
            if(warning!=null)Destroy(warning);if(warningMesh!=null)Destroy(warningMesh);if(warningMaterial!=null)Destroy(warningMaterial);
            warning=null;warningMesh=null;warningMaterial=null;
        }
    }
}
