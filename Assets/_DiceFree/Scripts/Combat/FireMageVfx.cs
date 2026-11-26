using UnityEngine;
namespace DiceFree.Combat
{
    public static class FireMageVfx
    {
        private static Material Material()=>new Material(Resources.Load<Material>("AuraVfx/EmberFlame"));
        public static GameObject Ground(Vector3 point,float radius,float seconds,Color color)
        {
            var root=new GameObject("Burning embers");root.transform.position=point+Vector3.up*.08f;root.layer=2;
            var ring=root.AddComponent<LineRenderer>();ring.useWorldSpace=false;ring.loop=true;ring.positionCount=48;ring.widthMultiplier=.09f;
            var material=Material();ring.sharedMaterial=material;ring.startColor=ring.endColor=color;
            for(int i=0;i<48;i++){float a=i*Mathf.PI/24;ring.SetPosition(i,new Vector3(Mathf.Sin(a)*radius,0,Mathf.Cos(a)*radius));}
            var particles=root.AddComponent<ParticleSystem>();var main=particles.main;main.startLifetime=.5f;main.startSpeed=.7f;main.startSize=.18f;main.startColor=color;main.maxParticles=140;
            var shape=particles.shape;shape.shapeType=ParticleSystemShapeType.Circle;shape.radius=radius;shape.rotation=new Vector3(90,0,0);
            var emission=particles.emission;emission.rateOverTime=35;particles.GetComponent<ParticleSystemRenderer>().sharedMaterial=material;
            root.AddComponent<FireMageVfxLifetime>().Configure(material,seconds);return root;
        }
        public static ParticleSystem FlameGate(Transform parent,Vector3 point,Vector2 size)
        {
            var root=new GameObject("Rhythmic flame jets");root.layer=2;root.transform.position=point+Vector3.up*.12f;root.transform.SetParent(parent,true);
            var particles=root.AddComponent<ParticleSystem>();particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=particles.main;main.startLifetime=new ParticleSystem.MinMaxCurve(.35f,.7f);main.startSpeed=new ParticleSystem.MinMaxCurve(1.6f,2.8f);main.startSize=new ParticleSystem.MinMaxCurve(.18f,.48f);main.startColor=new ParticleSystem.MinMaxGradient(new Color(1,.15f,.01f,.7f),new Color(1,.7f,.08f,.95f));main.maxParticles=200;
            var shape=particles.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(size.x,size.y,.05f);shape.rotation=new Vector3(-90,0,0);
            var emission=particles.emission;emission.rateOverTime=125;
            var fade=particles.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(1,.35f,.03f),1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.12f),new GradientAlphaKey(0,1)});fade.color=gradient;
            var material=Material();particles.GetComponent<ParticleSystemRenderer>().sharedMaterial=material;root.AddComponent<FireMageVfxLifetime>().Configure(material,0);return particles;
        }
        public static GameObject Bomb(CombatActor target,float seconds,bool empowered)
        {var root=Ground(target.transform.position,.45f,seconds,empowered?new Color(1,.8f,.1f):new Color(1,.25f,.03f));root.transform.SetParent(target.transform,true);return root;}
    }
    public sealed class FireMageVfxLifetime:MonoBehaviour
    {
        private Material material;
        public void Configure(Material value,float seconds){material=value;if(seconds>0)Destroy(gameObject,seconds);}
        private void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
