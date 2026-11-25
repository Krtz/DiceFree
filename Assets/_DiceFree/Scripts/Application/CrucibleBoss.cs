using System;
using System.Collections.Generic;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.UI;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Dungeons
{
    [RequireComponent(typeof(CombatActor))]
    public sealed class CrucibleBoss : MonoBehaviour,IEncounterHudSource,IInterruptibleCombatCast
    {
        [SerializeField] int cell;
        [SerializeField] AttackDefinition special;
        [SerializeField] Transform visual;
        [SerializeField] Material warningMaterial;
        readonly List<DragonDangerZone> zones=new();
        readonly List<DragonAttackTelegraph> warnings=new();
        IReadOnlyList<CombatActor> party=Array.Empty<CombatActor>();
        CombatActor actor;BasicAttack basic;Vector3 originalScale,lockedTarget;float nextCast,impactAt,duration;int sequence;bool winding;
        const string CastBlock="crimson-crucible.cast";
        public bool Armed {get;private set;}
        public CombatActor Actor=>actor;
        public string Name=>actor!=null?actor.DisplayName:name;
        public CombatActor EncounterActor=>actor;
        public int LivingGuards=>0;
        public string EncounterStatus=>$"Crimson Crucible · Cell {cell+1}/6";
        public string Mechanic=>cell switch{0=>"Gel bursts · leave the green circles",1=>"Lunar pounce · sidestep the blue cone",2=>"Bristlebreaker · leave the frontal slam",3=>"Grasping roots · move out of the blooms",4=>"Stormfork volley · slip between the fans",_=>"Crimson eruption · leave the red circles"};
        public string CastLabel=>Mechanic;
        public float CastDuration=>duration;
        public float CastRemaining=>winding?Mathf.Max(0,impactAt-Time.time):0;
        public bool EngagedWith(CombatActor viewer)=>Armed&&actor.Alive&&Owns(viewer)&&CrucibleArena.InCell(viewer.transform.position,cell,3);
        public void Configure(int index,Transform model,AttackDefinition attack,Material warning){cell=index;visual=model;special=attack;warningMaterial=warning;}
        void Awake(){actor=GetComponent<CombatActor>();basic=GetComponent<BasicAttack>();originalScale=visual.localScale;}
        void OnEnable()=>EncounterHud.Register(this);
        void OnDisable(){EncounterHud.Unregister(this);Cease();}
        public void Arm(IReadOnlyList<CombatActor> roster)
        {
            party=roster;Armed=true;nextCast=Time.time+2.2f;sequence=0;
            int count=0;foreach(var p in roster)if(p!=null&&p.Alive)count++;
            // Extra vitality supplies party HP through the ordinary stat package;
            // authored bosses have zero vitality regeneration.
            actor.Stats.SetAttributeContribution("crucible.party",new AttributeValues{vitality=Mathf.Max(0,count-1)*actor.Stats.MaximumHp*.65f/Mathf.Max(1,actor.Stats.Definition.VitalityHpCoefficient)});
            actor.Health.Restore();GetComponent<AggroBehaviour>().enabled=true;
        }
        bool Owns(CombatActor target)
        {if(target==null)return false;var owner=target.GetComponent<DefeatReporter>()?.CreditOwner;foreach(var p in party)if(p==target||owner!=null&&p==owner)return true;return false;}
        CombatActor Target()
        {
            if(basic.Target!=null&&basic.Target.Alive&&Owns(basic.Target)&&CrucibleArena.InCell(basic.Target.transform.position,cell))return basic.Target;
            CombatActor chosen=null;float distance=float.PositiveInfinity;
            foreach(var p in party)if(p!=null&&p.Alive&&!p.Concealed&&CrucibleArena.InCell(p.transform.position,cell)&&actor.HasSightOf(p))
            {float d=(p.transform.position-transform.position).sqrMagnitude;if(d<distance){distance=d;chosen=p;}}
            return chosen;
        }
        void Update()
        {
            if(!Armed||!actor.Alive){CancelCast();return;}
            if(winding)
            {
                if(actor.Effects!=null&&actor.Effects.Stunned){TryInterruptCast();return;}
                for(int i=0;i<warnings.Count;i++)if(warnings[i]!=null)warnings[i].Windup(1-CastRemaining/duration);
                actor.Motor.Stop();
                if(Time.time>=impactAt)Impact();return;
            }
            var target=Target();if(target==null)return;
            if(basic.Target==null)basic.Order(target);
            if(Time.time>=nextCast&&actor.CanAct)BeginCast(target);
        }
        void BeginCast(CombatActor target)
        {
            lockedTarget=target.transform.position;var at=transform.position;var forward=Vector3.ProjectOnPlane(lockedTarget-at,Vector3.up).normalized;if(forward.sqrMagnitude<.01f)forward=Vector3.forward;
            zones.Clear();duration=cell==5?1.45f:1.7f;sequence++;
            switch(cell)
            {
                case 1:zones.Add(new(at,forward,14,27));break;
                case 2:zones.Add(new(at,forward,10,65));break;
                case 4:for(int i=-1;i<=1;i++)zones.Add(new(at,Quaternion.Euler(0,i*34,0)*forward,16,13));break;
                case 0:zones.Add(sequence%2==0?new(at,Vector3.forward,7):new(lockedTarget,Vector3.forward,5));break;
                default:
                    foreach(var p in party)if(p!=null&&p.Alive&&CrucibleArena.InCell(p.transform.position,cell))zones.Add(new(p.transform.position,Vector3.forward,cell==3?4:4.7f));
                    if(cell==5&&sequence%2==0)zones.Add(new(at,Vector3.forward,7));break;
            }
            var color=cell switch{0=>new Color(.35f,1,.30f),1=>new Color(.32f,.72f,1),2=>new Color(1,.64f,.17f),3=>new Color(.28f,1,.65f),4=>new Color(.38f,.65f,1),_=>new Color(1,.16f,.22f)};
            foreach(var zone in zones)warnings.Add(DragonAttackTelegraph.Create(transform.parent,zone,color,warningMaterial));
            winding=true;impactAt=Time.time+duration;actor.SetActionBlocked(CastBlock,true);actor.Motor.SetMotionBlocked(CastBlock,true);actor.Motor.Stop();
        }
        void Impact()
        {
            winding=false;actor.SetActionBlocked(CastBlock,false);actor.Motor.SetMotionBlocked(CastBlock,false);
            foreach(var target in new List<CombatActor>(CombatActor.All))
            {
                if(!target.Alive||!Owns(target)||!actor.IsHostileTo(target)||!actor.HasSightOf(target))continue;
                foreach(var zone in zones)if(zone.Contains(target.transform.position))
                {DamageResolver.Hit(actor,target,special);if(cell==3)target.Effects?.ApplyMovementSpeedDebuff("crucible.roots",35f,2.2f);break;}
            }
            foreach(var warning in warnings)if(warning!=null)warning.Impact();warnings.Clear();zones.Clear();
            var landing=lockedTarget-Vector3.ProjectOnPlane(lockedTarget-transform.position,Vector3.up).normalized*2.1f;
            if(cell==1&&CrucibleArena.InCell(landing,cell)&&NavMesh.SamplePosition(landing,out var nav,1.5f,1))actor.Motor.Teleport(nav.position);
            nextCast=Time.time+(cell==5&&actor.Health.Current<actor.Health.Maximum*.5f?2.4f:4.2f);
        }
        public bool TryInterruptCast(){if(!winding)return false;CancelCast();nextCast=Time.time+2.5f;return true;}
        void CancelCast()
        {
            winding=false;if(actor!=null){actor.SetActionBlocked(CastBlock,false);actor.Motor.SetMotionBlocked(CastBlock,false);}
            foreach(var warning in warnings)if(warning!=null)Destroy(warning.gameObject);warnings.Clear();zones.Clear();
        }
        public void Cease(){Armed=false;CancelCast();if(basic!=null)basic.Cancel();if(actor!=null){actor.Stats.RemoveAttributeContribution("crucible.party");if(GetComponent<AggroBehaviour>() is {} ai)ai.enabled=false;}}
        void LateUpdate()
        {
            if(visual==null||!Armed||!actor.Alive)return;
            float pulse=winding?Mathf.Sin((1-CastRemaining/duration)*Mathf.PI)*.10f:Mathf.Sin(Time.time*2)*.015f;
            visual.localScale=Vector3.Scale(originalScale,new Vector3(1+pulse,1-pulse*.55f,1+pulse));
        }
    }
}
