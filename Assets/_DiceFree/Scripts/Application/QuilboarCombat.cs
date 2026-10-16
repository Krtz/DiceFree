using System;using System.Linq;using DiceFree.Combat;using DiceFree.AI;using UnityEngine;
namespace DiceFree.Briarback
{
    public enum QuilboarRole { Raider, SpearThrower, Totemcaller, Harrowtusk }
    [DisallowMultipleComponent,RequireComponent(typeof(CombatActor),typeof(BasicAttack),typeof(AggroBehaviour))]
    public sealed class QuilboarCombat:MonoBehaviour
    {
        public QuilboarRole role;public AttackDefinition slam;
        CombatActor actor;BasicAttack basic;float readyAt,impactAt;Vector3 impactCenter;
        GameObject warning;Material warningMaterial;Mesh warningMesh;
        const string Block="briarback.slam";
        public bool Winding {get;private set;}
        public Vector3 ImpactCenter=>impactCenter;
        public int SlamHits {get;private set;}
        public float CooldownRemaining=>Mathf.Max(0,readyAt-Time.time);
        public event Action SlamStarted;
        void Awake(){actor=GetComponent<CombatActor>();basic=GetComponent<BasicAttack>();}
        void OnEnable(){actor.Health.Died+=Cancel;actor.Health.Restored+=Restored;}
        void OnDisable(){if(actor?.Health!=null){actor.Health.Died-=Cancel;actor.Health.Restored-=Restored;}Cancel();}
        void Restored(){Cancel();readyAt=Time.time+4;}
        public bool BeginSlam(){
            if(role!=QuilboarRole.Harrowtusk||slam==null||Winding||!actor.CanAct||Time.time<readyAt)return false;
            Winding=true;readyAt=Time.time+9;impactAt=Time.time+1.1f;impactCenter=transform.position;
            basic.Cancel();actor.Motor.Stop();actor.SetActionBlocked(Block,true);actor.Motor.SetMotionBlocked(Block,true);
            ShowWarning();SlamStarted?.Invoke();return true;
        }
        void Update(){
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
            Winding=false;if(actor!=null){actor.SetActionBlocked(Block,false);actor.Motor.SetMotionBlocked(Block,false);}
            if(warning!=null)Destroy(warning);if(warningMesh!=null)Destroy(warningMesh);if(warningMaterial!=null)Destroy(warningMaterial);
            warning=null;warningMesh=null;warningMaterial=null;
        }
    }
}
