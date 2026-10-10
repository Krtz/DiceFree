using UnityEngine;

namespace DiceFree.Combat
{
    /// <summary>
    /// Three-tier VFX language for starter skills. Decorative only: no gameplay colliders,
    /// delayed damage, root motion, lights or changes to combat rules.
    /// Tier 0 (Novice) uses modest, warm motion; Tier 1 branches add recognizable
    /// elemental/martial shape. Late-tier spectacle intentionally remains unspent.
    /// </summary>
    public static class EarlySkillVfx
    {
        public enum Cue
        {
            NoviceStrike, NoviceSand, NoviceHaste, NoviceHeal,
            PhysicalHeavy, PhysicalGuard, PhysicalQuickening, PhysicalArrowArea, PhysicalArrowHit,
            MagicalSand, MagicalMend, MagicalFire, MagicalIceWarning, MagicalIceImpact, MagicalAutoImpact
        }

        private static Material sparkMaterial;
        private static Material lineMaterial;
        private static Texture2D sparkTexture;

        private static Material ParticleMaterial()
        {
            if (sparkMaterial != null) return sparkMaterial;
            sparkMaterial = Resources.Load<Material>("EarlySkillVfx/SoftSparks");
            if (sparkMaterial != null) return sparkMaterial;
            sparkTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            sparkTexture.name = "DiceFree radial spark (runtime)";
            sparkTexture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 32f;
                    float a = Mathf.Pow(Mathf.Clamp01(1f - d), 2f) * 0.9f;
                    sparkTexture.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            sparkTexture.Apply(false, true);
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit");
            sparkMaterial = new Material(shader) { name = "DiceFree soft skill particles" };
            if (sparkMaterial.HasProperty("_BaseMap")) sparkMaterial.SetTexture("_BaseMap", sparkTexture);
            if (sparkMaterial.HasProperty("_MainTex")) sparkMaterial.SetTexture("_MainTex", sparkTexture);
            ConfigureTransparency(sparkMaterial);
            return sparkMaterial;
        }

        private static Material RingMaterial()
        {
            if (lineMaterial != null) return lineMaterial;
            lineMaterial = Resources.Load<Material>("EarlySkillVfx/SoftRing");
            if (lineMaterial != null) return lineMaterial;
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            lineMaterial = new Material(shader) { name = "DiceFree subtle skill ring" };
            if (lineMaterial.HasProperty("_BaseColor")) lineMaterial.SetColor("_BaseColor", Color.white);
            ConfigureTransparency(lineMaterial);
            return lineMaterial;
        }

        private static void ConfigureTransparency(Material material)
        {
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0);
            if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0);
            if (material.HasProperty("_SrcBlend"))
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend"))
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = 3000;
        }

        public static void Play(Cue cue, Vector3 point, float range = 0.65f) =>
            Spawn(cue, point, range, false);

        // Used only by isolated Editor previews; never changes gameplay scenes.
        public static void EditorPreview(Cue cue, Vector3 point, float range = .65f) =>
            Spawn(cue, point, range, true);

        private static void Spawn(Cue cue, Vector3 point, float range, bool editorPreview)
        {
            if (!Application.isPlaying && !editorPreview) return;
            bool magic = cue >= Cue.MagicalSand;
            bool physical = cue >= Cue.PhysicalHeavy && cue <= Cue.PhysicalArrowHit;
            bool area = cue == Cue.PhysicalArrowArea || cue == Cue.MagicalIceWarning || cue == Cue.MagicalIceImpact;
            bool support = cue == Cue.NoviceHaste || cue == Cue.NoviceHeal ||
                           cue == Cue.PhysicalGuard || cue == Cue.PhysicalQuickening ||
                           cue == Cue.MagicalMend || cue == Cue.MagicalFire;
            Color color;
            switch (cue)
            {
                case Cue.NoviceSand: color = new Color(1f, .68f, .25f); break;
                case Cue.NoviceHaste: color = new Color(1f, .88f, .46f); break;
                case Cue.NoviceHeal: color = new Color(.63f, 1f, .72f); break;
                case Cue.PhysicalHeavy: color = new Color(1f, .35f, .18f); break;
                case Cue.PhysicalGuard: color = new Color(.52f, .76f, .90f); break;
                case Cue.PhysicalQuickening: color = new Color(1f, .74f, .24f); break;
                case Cue.PhysicalArrowArea: case Cue.PhysicalArrowHit:
                    color = new Color(.82f, .92f, 1f); break;
                case Cue.MagicalSand: color = new Color(.76f, .43f, 1f); break;
                case Cue.MagicalMend: color = new Color(.48f, 1f, .82f); break;
                case Cue.MagicalFire: color = new Color(1f, .39f, .16f); break;
                case Cue.MagicalIceWarning: case Cue.MagicalIceImpact:
                    color = new Color(.37f, .84f, 1f); break;
                case Cue.MagicalAutoImpact:
                    color = new Color(.67f, .49f, 1f); break;
                default: color = new Color(1f, .88f, .67f); break;
            }

            var root = new GameObject("Skill VFX - " + cue);
            root.transform.position = point;
            int amount = cue == Cue.MagicalAutoImpact ? 10 : cue == Cue.MagicalIceWarning ? 7 : area ? 24 : magic ? 19 : physical ? 15 : cue == Cue.NoviceSand ? 16 : 9;
            float height = area ? .15f : support ? 1.05f : .85f;
            float spread = Mathf.Max(.15f, area ? range : support ? .28f : .20f);
            CreateBurst(root.transform, color, height, spread, amount, magic ? .85f : .55f,
                area ? .21f : magic ? .19f : physical ? .16f : .125f);

            if (cue == Cue.PhysicalGuard || cue == Cue.PhysicalQuickening ||
                cue == Cue.MagicalMend || cue == Cue.MagicalFire ||
                cue == Cue.NoviceHeal || cue == Cue.NoviceHaste)
                CreateRing(root.transform, color, .55f, .11f, .52f);

            if (cue == Cue.MagicalIceWarning)
                CreateRing(root.transform, color, range, .09f, 1f, true);
            else if (cue == Cue.MagicalIceImpact || cue == Cue.PhysicalArrowArea)
                CreateRing(root.transform, color, range, .08f, cue == Cue.PhysicalArrowArea ? .72f : .32f, true);
            else if (cue == Cue.PhysicalHeavy)
                CreateRing(root.transform, color, .63f, .085f, .32f, true);
            else if (cue == Cue.MagicalAutoImpact)
                CreateRing(root.transform, color, .26f, .045f, .21f);

            CreateSignature(root.transform, cue, color, range);
            if (Application.isPlaying)
                Object.Destroy(root, cue == Cue.MagicalIceWarning ? 1.05f : 1.9f);
        }

        // Skill-specific shapes add identity without spending later-tier spectacle.
        private static void CreateSignature(Transform root, Cue cue, Color tint, float range)
        {
            if (cue == Cue.NoviceSand)
            {
                // A little miniature golden sand-twister, not a high-tier magical cyclone.
                var points = new Vector3[24];
                for(int i = 0; i < points.Length; i++)
                {
                    float t = i / (float)(points.Length - 1);
                    float a = t * Mathf.PI * 3.8f;
                    float radius = .20f * (1f - t * .5f);
                    points[i] = new Vector3(Mathf.Cos(a) * radius, .36f + t * .84f,
                        Mathf.Sin(a) * radius);
                }
                CreateStroke(root, "Magic Sand golden spiral", points, tint, .035f, .4f, false);
            }
            else if (cue == Cue.PhysicalHeavy)
            {
                var pts = new Vector3[19];
                for (int i=0;i<pts.Length;i++)
                {
                    float angle=(-32f+196f*i/(pts.Length-1))*Mathf.Deg2Rad;
                    pts[i]=new Vector3(Mathf.Cos(angle)*.50f, .98f+Mathf.Sin(angle)*.32f, -.10f);
                }
                CreateStroke(root,"Heavy Strike slash",pts,tint,.055f,.27f,false);
            }
            else if(cue==Cue.PhysicalGuard)
            {
                CreateStroke(root,"Guard shield contour",new[]{
                    new Vector3(-.22f,1.39f,-.25f),new Vector3(0,1.46f,-.25f),
                    new Vector3(.22f,1.39f,-.25f),new Vector3(.18f,1.04f,-.25f),
                    new Vector3(0,.85f,-.25f),new Vector3(-.18f,1.04f,-.25f)
                },tint,.032f,.48f,true);
            }
            else if(cue==Cue.MagicalFire)
            {
                var pts = new Vector3[35];
                for(int i=0;i<pts.Length;i++)
                {
                    float t=i/(float)(pts.Length-1);
                    float a=t*3.7f*Mathf.PI;
                    pts[i]=new Vector3(Mathf.Cos(a)*(.30f-.19f*t),.55f+t*.9f,Mathf.Sin(a)*(.30f-.19f*t));
                }
                CreateStroke(root,"Fire Imbuement spiral",pts,tint,.027f,.52f,false);
            }
            else if(cue==Cue.MagicalIceImpact)
            {
                for(int i=0;i<6;i++)
                {
                    float a=(i/6f)*Mathf.PI*2f;
                    float r=Mathf.Max(.5f,range*.63f);
                    var direction=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                    CreateStroke(root,"Frostburst ray",new[]{
                        direction*(r*.22f)+Vector3.up*.10f,
                        direction*(r*.70f)+Vector3.up*.20f,
                        direction*r+Vector3.up*.10f
                    },tint,.045f,.42f,false);
                }
            }
        }

        private static void CreateStroke(Transform parent,string name,Vector3[] pts,Color tint,float width,float life,bool loop)
        {
            var go=new GameObject(name);
            go.transform.SetParent(parent,false);
            var line=go.AddComponent<LineRenderer>();
            line.useWorldSpace=false;
            line.loop=loop;
            line.positionCount=pts.Length;
            line.SetPositions(pts);
            line.widthMultiplier=width;
            line.sharedMaterial=RingMaterial();
            line.startColor=line.endColor=new Color(tint.r,tint.g,tint.b,.8f);
            var block=new MaterialPropertyBlock();
            block.SetColor("_BaseColor",new Color(tint.r,tint.g,tint.b,.8f));
            line.SetPropertyBlock(block);
            line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows=false;
            go.AddComponent<EarlySkillRingFade>().Configure(life);
        }

        private static void CreateBurst(Transform root, Color color, float y, float spread, int count, float life, float size)
        {
            var holder = new GameObject("soft sparks");
            holder.transform.SetParent(root, false);
            holder.transform.localPosition = Vector3.up * y;
            var particles = holder.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 1.2f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life * .7f, life * 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(.17f, .75f);
            main.startSize = new ParticleSystem.MinMaxCurve(size * .5f, size);
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 96;
            var emission = particles.emission;
            emission.enabled = false;
            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = spread;
            var colorLife = particles.colorOverLifetime;
            colorLife.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(0f, 0), new GradientAlphaKey(.9f, .15f),
                        new GradientAlphaKey(.65f, .6f), new GradientAlphaKey(0f, 1) });
            colorLife.color = gradient;
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = ParticleMaterial();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            particles.Play();
            particles.Emit(count);
        }

        private static void CreateRing(Transform root, Color color, float radius, float width, float duration, bool ground = false)
        {
            var go = new GameObject("radial cue");
            go.transform.SetParent(root, false);
            go.transform.localPosition = Vector3.up * (ground ? .07f : .92f);
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 52;
            line.widthMultiplier = width;
            line.sharedMaterial = RingMaterial();
            line.startColor = line.endColor = new Color(color.r, color.g, color.b, .72f);
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_BaseColor", new Color(color.r, color.g, color.b, .72f));
            line.SetPropertyBlock(properties);
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            for (int i = 0; i < 52; i++)
            {
                float a = i * 2f * Mathf.PI / 52f;
                line.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius));
            }
            var fade = go.AddComponent<EarlySkillRingFade>();
            fade.Configure(duration);
        }
    }

    internal sealed class EarlySkillRingFade : MonoBehaviour
    {
        private LineRenderer line;
        private MaterialPropertyBlock block;
        private float until, duration;
        public void Configure(float seconds)
        {
            duration = Mathf.Max(.08f, seconds);
            until = Time.time + duration;
            line = GetComponent<LineRenderer>();
            block = new MaterialPropertyBlock();
        }
        private void Update()
        {
            float t = Mathf.Clamp01((until - Time.time) / duration);
            if (line != null)
            {
                Color c = line.startColor;
                c.a = .75f * t;
                line.startColor = line.endColor = c;
                block.SetColor("_BaseColor", c);
                line.SetPropertyBlock(block);
            }
            if (Time.time >= until) Destroy(gameObject);
        }
    }
}
