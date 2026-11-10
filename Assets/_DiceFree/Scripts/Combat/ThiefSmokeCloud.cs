using System.Collections.Generic;
using DiceFree.Combat;
using UnityEngine;
using UnityEngine.Rendering;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent]
    public sealed class ThiefSmokeCloud : MonoBehaviour
    {
        public const float Radius = 4;
        private const float Duration = 5;
        private const double Interval = .25;
        private static readonly List<ThiefSmokeCloud> active = new();
        private static readonly Dictionary<CombatActor,double> lastDamage = new();
        private readonly List<CombatActor> victims = new();
        private CombatActor owner;
        private AttackDefinition attack;
        private int rank;
        private double placedAt, nextTick, expires;
        private Material smokeMaterial;
        private Texture2D smokeTexture;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() { active.Clear(); lastDamage.Clear(); }

        public static ThiefSmokeCloud Place(CombatActor source,Vector3 position,AttackDefinition attack,int rank)
        {
            var go = new GameObject("Thief Smoke Bomb");
            go.transform.position = position;
            var cloud = go.AddComponent<ThiefSmokeCloud>();
            cloud.owner = source;
            cloud.attack = attack;
            cloud.rank = rank;
            cloud.placedAt = Time.timeAsDouble;
            cloud.nextTick = cloud.placedAt + Interval;
            cloud.expires = cloud.placedAt + Duration;
            cloud.CreateSmoke();
            active.Add(cloud);
            return cloud;
        }

        public static float BasicMissChance(CombatActor attacker)
        {
            double now = Time.timeAsDouble;
            return FirstCloud(attacker,now) != null ? .20f : 0;
        }

        private static ThiefSmokeCloud FirstCloud(CombatActor victim,double now)
        {
            foreach (var cloud in active)
                if (cloud != null && cloud.placedAt <= now && cloud.expires >= now && cloud.owner != null &&
                    cloud.owner.IsHostileTo(victim) &&
                    ThiefSkillCaster.Within(cloud.transform.position,victim.transform.position,Radius)) return cloud;
            return null;
        }

        private void Update()
        {
            double now = Time.timeAsDouble;
            if (owner == null || !owner.Alive) { Dismiss(); return; }
            while (nextTick <= now && nextTick <= expires)
            {
                double tick = nextTick;
                nextTick += Interval;
                victims.Clear();
                foreach (var victim in CombatActor.All)
                    if (FirstCloud(victim,tick) == this) victims.Add(victim);
                foreach (var victim in victims)
                {
                    if (!owner.IsHostileTo(victim) ||
                        lastDamage.TryGetValue(victim,out var last) && tick-last<Interval-.00001) continue;
                    lastDamage[victim] = tick;
                    victim.Health.ApplyPacket(owner,DamageResolver.Calculate(owner.Stats,victim.Stats,attack,
                        rawMultiplier:ThiefSkillCaster.PeriodicScale(rank)));
                }
            }
            if (now > expires) Dismiss();
        }

        public void Dismiss()
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private void OnDisable()
        {
            active.Remove(this);
            if (active.Count == 0) lastDamage.Clear();
        }

        private void OnDestroy()
        {
            Destroy(smokeMaterial);
            Destroy(smokeTexture);
        }

        private void CreateSmoke()
        {
            var fog = new GameObject("Rolling smoke");
            fog.transform.SetParent(transform,false);
            fog.transform.localPosition = Vector3.up*.24f;
            var particles = fog.AddComponent<ParticleSystem>();
            particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.loop = false;
            main.duration = Duration;
            main.startLifetime = new ParticleSystem.MinMaxCurve(.9f,1.55f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.55f,2.45f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(.18f,.49f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(.34f,.40f,.48f,.36f),new Color(.68f,.70f,.71f,.55f));
            main.startRotation = new ParticleSystem.MinMaxCurve(0,6.28f);
            main.maxParticles = 105;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 3.5f;
            shape.radiusThickness = 1;
            shape.rotation = new Vector3(90,0,0);
            var emission = particles.emission;
            emission.rateOverTime = 33;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0,34) });
            var noise = particles.noise;
            noise.enabled = true;
            noise.strength = .28f;
            noise.frequency = .43f;
            var fade = particles.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1) },
                new[] { new GradientAlphaKey(0,0),new GradientAlphaKey(.9f,.21f),new GradientAlphaKey(.65f,.64f),new GradientAlphaKey(0,1) });
            fade.color = gradient;
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Universal Render Pipeline/Unlit");
            smokeMaterial = new Material(shader) {
                name = "Thief silver smoke", renderQueue = (int)RenderQueue.Transparent
            };
            smokeMaterial.SetFloat("_Surface",1);
            smokeMaterial.SetFloat("_Blend",0);
            smokeMaterial.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);
            smokeMaterial.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
            smokeMaterial.SetFloat("_ZWrite",0);
            smokeMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            smokeTexture = new Texture2D(64,64,TextureFormat.RGBA32,false) { wrapMode=TextureWrapMode.Clamp };
            var pixels = new Color[64*64];
            for (int y=0;y<64;y++)
                for (int x=0;x<64;x++)
                {
                    float u=(x-31.5f)/31.5f,v=(y-31.5f)/31.5f;
                    float fadeOut=Mathf.Clamp01(1-Mathf.Sqrt(u*u+v*v));
                    float texture=.58f+.42f*Mathf.PerlinNoise(x*.09f+2,y*.09f+7);
                    pixels[x+y*64]=new Color(1,1,1,fadeOut*fadeOut*texture);
                }
            smokeTexture.SetPixels(pixels);
            smokeTexture.Apply(false,true);
            smokeMaterial.SetTexture("_BaseMap",smokeTexture);
            renderer.sharedMaterial = smokeMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            particles.Play();
            EarlySkillVfx.PlayPriestPulse("Smoke Bomb",transform.position,Radius,new Color(.56f,.64f,.74f),false);
        }

        internal static void KnifeFan(Vector3 point)
        {
            EarlySkillVfx.PlayPriestPulse("Fan of Knives",point,Radius,new Color(.81f,.91f,1f));
            for (int i=0;i<8;i++)
            {
                float angle=i*Mathf.PI*.25f;
                var outward = new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                EarlySkillVfx.PlayPriestPulse("Knife glint",point+outward*(Radius*.76f),.22f,new Color(.76f,.88f,1));
            }
        }
    }
}
