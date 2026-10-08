using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.AI;
using UnityEngine;
using UnityEngine.AI;
namespace DiceFree.Dungeons
{
    public enum SlimeRole {Trash,Dummy,Puzzle,Fragment,Add,Miniboss,Boss,Regent}
    public sealed class DungeonSlime : MonoBehaviour
    {
        public SlimeDungeonRun Run {get;private set;}
        public SlimeRole Role {get;private set;}
        public bool Blue {get;private set;}
        public bool Captured {get;private set;}
        public CombatActor Actor {get;private set;}
        public bool Dividing {get;private set;}
        public bool Busy=>busy;
        public Vector3 LastCastSnapshot {get;private set;}
        public Vector3 LastImpactPosition {get;private set;}
        public int ThresholdsConsumed {get;private set;}
        bool busy,ceased,engaged;float nextAttack;Vector3 home;
        Transform visual;float visualHeight;
        readonly List<GameObject> markers=new();
        readonly List<DungeonSlime> fragments=new();
        readonly List<DungeonSlime> adds=new();
        public void Initialize(SlimeDungeonRun run,SlimeRole role,bool blue)
        {
            Run=run;Role=role;Blue=blue;Actor=GetComponent<CombatActor>();home=transform.position;
            visual=transform.Find("Presentation");if(visual!=null)visualHeight=visual.localPosition.y;
            Actor.Health.Died+=Died;
            if(role==SlimeRole.Dummy){GetComponent<AggroBehaviour>().enabled=false;GetComponent<BasicAttack>().enabled=false;}
            if(IsBoss||role==SlimeRole.Fragment){GetComponent<AggroBehaviour>().enabled=false;GetComponent<BasicAttack>().enabled=false;}
            if(role==SlimeRole.Puzzle||role==SlimeRole.Add)GetComponent<AggroBehaviour>().ConfigureAutoAggro(new AutoAggroPolicy{alwaysAutoAggro=true});
            if(blue)Tint(new Color(.16f,.45f,.95f));
        }
        public bool IsBoss=>Role>=SlimeRole.Miniboss;
        public bool TryCastRegentAttack(bool rolling)
        {
            if(Role!=SlimeRole.Regent||busy||!Run.Active)return false;
            var players=Run.Living.ToArray();if(players.Length==0)return false;
            StartCoroutine(rolling?Roll(players):Bounce(players));return true;
        }
        void Update()
        {
            if(ceased||Captured)return;
            if(Role==SlimeRole.Dummy){if(!Actor.Alive||Actor.Health.Current<Actor.Health.Maximum*.2f)Actor.Health.Restore();return;}
            if(!Run.Active||!Actor.Alive)return;
            if(IsBoss&&!Actor.CanAct)return;
            if(!IsBoss)return;
            var living=Run.Living.ToArray();if(living.Length==0)return;
            if(engaged&&living.All(p=>Vector3.Distance(p.transform.position,home)>24)){ResetEncounter();return;}
            if(!engaged){if(living.All(p=>Vector3.Distance(p.transform.position,home)>12))return;engaged=true;nextAttack=Time.time+2;}
            if(busy)return;
            float ratio=Actor.Health.Current/Actor.Health.Maximum;
            if(ThresholdsConsumed==0&&ratio<=.7f||ThresholdsConsumed==1&&ratio<=.3f){ThresholdsConsumed++;StartCoroutine(Divide());return;}
            if(Time.time<nextAttack)return;
            nextAttack=Time.time+Run.Tuning.slamInterval;
            if(Role==SlimeRole.Regent){int attack=Random.Range(0,3);if(attack==1){StartCoroutine(Roll(living));return;}if(attack==2){StartCoroutine(Bounce(living));return;}}
            StartCoroutine(Slam(living[Random.Range(0,living.Length)].transform.position,Radius));
        }
        float Radius=>Role==SlimeRole.Miniboss?Run.Tuning.minibossSlamRadius:Role==SlimeRole.Regent?Run.Tuning.regentSlamRadius:Run.Tuning.bossSlamRadius;
        string BossId=>Role==SlimeRole.Miniboss?"boss.big-slime":Role==SlimeRole.Regent?"boss.slime-regent":"boss.slime";
        IEnumerator Slam(Vector3 destination,float radius)
        {
            busy=true;Run.Witness(BossId,"mechanic.slime-slam");Actor.Motor.Stop();
            var mark=TrackMarker(destination,radius,new Color(1,.2f,.07f,.7f));
            float start=Time.time;float warning=Run.Tuning.slamWarning;
            while(Time.time-start<warning){if(visual!=null)visual.localPosition=new Vector3(0,visualHeight+Mathf.Sin((Time.time-start)/warning*Mathf.PI)*2,0);yield return null;}
            if(visual!=null)visual.localPosition=new Vector3(0,visualHeight,0);
            Actor.Motor.Teleport(destination);HitCircle(destination,radius);Destroy(mark);busy=false;
        }
        IEnumerator Divide()
        {
            busy=true;Dividing=true;Actor.Motor.Stop();Run.Witness(BossId,"mechanic.divide");
            float stored=Actor.Health.Current;Actor.Health.Invulnerable=true;
            foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=false;
            foreach(var c in GetComponents<Collider>())c.enabled=false;
            int count=Role==SlimeRole.Miniboss?2:Role==SlimeRole.Boss?3:4;
            float duration=Role==SlimeRole.Miniboss?Run.Tuning.minibossReunion:Role==SlimeRole.Boss?Run.Tuning.bossReunion:Run.Tuning.regentReunion;
            Vector3 center=home;fragments.Clear();
            for(int i=0;i<count;i++)
            {
                float angle=i*Mathf.PI*2/count;var spawn=center+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*Run.Tuning.reunionRadius;
                if(NavMesh.SamplePosition(spawn,out var sample,3,NavMesh.AllAreas))spawn=sample.position;
                var f=Run.Spawn(Run.Tuning.fragment,spawn,.9f,SlimeRole.Fragment);
                f.Actor.Stats.SetEncounterMaximumHp(Actor.Health.Maximum*Run.Tuning.fragmentHpFraction);f.Actor.Health.Restore();
                fragments.Add(f);
                // The stat contribution survives CombatActor.Start; setting only Motor.speed here
                // would be overwritten on the fragment's first frame.
                float driftSpeed=Vector3.Distance(spawn,center)/duration;
                f.Actor.Stats.SetMovementSpeedPercentModifier("divide.drift",(driftSpeed/f.Actor.Stats.MoveSpeed-1)*100);
            }
            float end=Time.time+duration,nextAdd=Time.time+2;
            // Reunion is driven by movement into the common center; timeout protects blocked navigation.
            while(Time.time<end && fragments.Count(f=>f!=null&&f.Actor.Alive)>1)
            {
                foreach(var f in fragments)if(f!=null&&f.Actor.Alive)f.Actor.Motor.MoveTo(center);
                if(Role!=SlimeRole.Miniboss && Time.time>=nextAdd)
                {
                    nextAdd+=2;var add=Run.Spawn(Role==SlimeRole.Regent?Run.Tuning.regentAdd:Run.Tuning.add,center+Vector3.right*3,.55f,SlimeRole.Add);
                    adds.RemoveAll(a=>a==null);adds.Add(add);add.GetComponent<AggroBehaviour>().Configure(20,100);
                    var target=Run.Living.FirstOrDefault();if(target!=null)add.GetComponent<BasicAttack>().Order(target);
                }
                if(fragments.Where(f=>f!=null&&f.Actor.Alive).All(f=>Vector3.Distance(f.transform.position,center)<.9f))break;
                yield return null;
            }
            var survivors=fragments.Where(f=>f!=null&&f.Actor.Alive).ToArray();
            float hp=stored+(survivors.Length>1?survivors.Sum(f=>f.Actor.Health.Current):0);
            foreach(var f in fragments)if(f!=null)Destroy(f.gameObject);fragments.Clear();
            Actor.Health.SetEncounterHp(hp);Actor.Health.Invulnerable=false;
            foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=true;
            foreach(var c in GetComponents<Collider>())c.enabled=true;
            Actor.Motor.Teleport(center);Dividing=false;busy=false;nextAttack=Time.time+2;
        }
        IEnumerator Roll(CombatActor[] players)
        {
            busy=true;Run.Witness(BossId,"mechanic.slime-regent-roll");
            var destination=players.OrderByDescending(a=>Vector3.Distance(a.transform.position,transform.position)).First().transform.position;
            LastCastSnapshot=destination;
            var origin=transform.position;var marker=TrackMarker(destination,2,Color.yellow);yield return new WaitForSeconds(1.5f);Destroy(marker);
            float duration=Mathf.Max(.3f,Vector3.Distance(origin,destination)/12);float start=Time.time,nextTrail=0;
            while(Time.time-start<duration)
            {
                var p=Vector3.Lerp(origin,destination,(Time.time-start)/duration);Actor.Motor.Teleport(p);
                if(Time.time>=nextTrail){nextTrail=Time.time+.25f;StartCoroutine(Trail(p));}yield return null;
            }
            Actor.Motor.Teleport(destination);LastImpactPosition=transform.position;busy=false;nextAttack=Time.time+Run.Tuning.slamInterval;
        }
        IEnumerator Trail(Vector3 p)
        {
            var marker=TrackMarker(p,1.2f,new Color(.8f,.75f,.1f,.7f));float end=Time.time+5;
            while(Time.time<end){if(Run.Active)HitCircle(p,1.2f);yield return new WaitForSeconds(.5f);}Destroy(marker);
        }
        IEnumerator Bounce(CombatActor[] players)
        {
            busy=true;Run.Witness(BossId,"mechanic.slime-regent-bounce");
            var positions=players.Select(a=>a.transform.position).ToArray();var markers=positions.Select(p=>TrackMarker(p,2,Color.yellow)).ToArray();
            LastCastSnapshot=positions[0];
            yield return new WaitForSeconds(2);
            for(int i=0;i<positions.Length;i++){Actor.Motor.Teleport(positions[i]);LastImpactPosition=transform.position;HitCircle(positions[i],2);Destroy(markers[i]);yield return new WaitForSeconds(.75f);}busy=false;nextAttack=Time.time+Run.Tuning.slamInterval;
        }
        void HitCircle(Vector3 p,float radius)
        {
            if(!Run.Active)return;
            foreach(var a in Run.Living.ToArray())if(Vector3.Distance(a.transform.position,p)<=radius+a.Radius)DamageResolver.Hit(Actor,a,Role==SlimeRole.Regent?Run.Tuning.regentSlam:Run.Tuning.slam);
        }
        public static GameObject Marker(Vector3 p,float radius,Color color)
        {
            var obj=new GameObject("Static danger telegraph");obj.transform.position=p+Vector3.up*.06f;
            var line=obj.AddComponent<LineRenderer>();line.loop=true;line.useWorldSpace=false;line.positionCount=64;line.widthMultiplier=.13f;
            line.sharedMaterial=SlimeDungeonVisuals.TelegraphMaterial;line.startColor=line.endColor=color;
            for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;line.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));}
            return obj;
        }
        GameObject TrackMarker(Vector3 p,float radius,Color color){markers.RemoveAll(m=>m==null);var mark=Marker(p,radius,color);markers.Add(mark);return mark;}
        public void Capture(Vector3 p,Vector3 center)
        {
            Captured=true;Actor.Health.Invulnerable=true;GetComponent<AggroBehaviour>().enabled=false;GetComponent<BasicAttack>().enabled=false;Actor.Motor.Stop();Actor.Motor.Teleport(p);Actor.Motor.SetMotionBlocked("puzzle.capture",true);
            transform.rotation=Quaternion.LookRotation(center-p);
            GetComponent<NavMeshAgent>().enabled=false; // Captives cannot be moved by idle avoidance.
        }
        public void Tint(Color color)
        {
            foreach(var r in GetComponentsInChildren<Renderer>())
            {
                var props=new MaterialPropertyBlock();r.GetPropertyBlock(props);props.SetColor("_BaseColor",color);r.SetPropertyBlock(props);
            }
        }
        public void Cease(){ceased=true;StopAllCoroutines();foreach(var m in markers)if(m!=null)Destroy(m);markers.Clear();Actor.Health.Invulnerable=true;GetComponent<AggroBehaviour>().enabled=false;GetComponent<BasicAttack>().enabled=false;Actor.Motor.Stop();foreach(var f in fragments)if(f!=null)Destroy(f.gameObject);}
        public void ResetEncounter()
        {
            StopAllCoroutines();foreach(var m in markers)if(m!=null)Destroy(m);markers.Clear();
            foreach(var f in fragments)if(f!=null)Destroy(f.gameObject);fragments.Clear();
            foreach(var a in adds)if(a!=null)Destroy(a.gameObject);adds.Clear();
            ThresholdsConsumed=0;Dividing=busy=engaged=false;Actor.Health.Invulnerable=false;Actor.Health.Restore();Actor.Motor.Teleport(home);
            foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=true;foreach(var c in GetComponents<Collider>())c.enabled=true;
            if(visual!=null)visual.localPosition=new Vector3(0,visualHeight,0);
        }
        void Died(){if(Role==SlimeRole.Dummy){Actor.Health.Restore();return;}Run.Killed(this);if(!IsBoss)Destroy(gameObject,1);}
        void OnDestroy(){foreach(var m in markers)if(m!=null)Destroy(m);foreach(var f in fragments)if(f!=null)Destroy(f.gameObject);if(Actor!=null)Actor.Health.Died-=Died;}
    }
    public static class SlimeDungeonVisuals
    {
        static Material material;
        public static Material TelegraphMaterial=>material!=null?material:material=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
    }
}
