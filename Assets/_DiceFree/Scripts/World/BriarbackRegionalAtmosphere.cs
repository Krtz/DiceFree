using UnityEngine;

namespace DiceFree.World
{
    [DefaultExecutionOrder(400)]
    public sealed class BriarbackRegionalAtmosphere : MonoBehaviour
    {
        [SerializeField] WorldDayNightCycle clock;
        Transform traveler;
        float originalFogStart,originalFogEnd;
        bool originalFog;
        FogMode originalFogMode;
        public float Weight {get;private set;}
        public void Configure(WorldDayNightCycle value)=>clock=value;
        void Awake(){originalFog=RenderSettings.fog;originalFogMode=RenderSettings.fogMode;originalFogStart=RenderSettings.fogStartDistance;originalFogEnd=RenderSettings.fogEndDistance;}
        public static float Blend(Vector3 p)
        {
            float north=Mathf.SmoothStep(0,1,Mathf.InverseLerp(208,252,p.z));
            float side=Mathf.SmoothStep(0,1,Mathf.InverseLerp(182,206,p.x))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(332,355,p.x)));
            float far=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(532,552,p.z));return north*side*far;
        }
        void LateUpdate()
        {
            if(clock==null||!clock.isActiveAndEnabled||clock.IsInterior){if(Weight>0)RestoreDistances();Weight=0;return;}
            if(traveler==null)traveler=FindAnyObjectByType<WorldFogOfWar>()?.Revealer;
            float previousWeight=Weight;
            Weight=traveler!=null?Blend(traveler.position):0;
            if(Weight<=0){if(previousWeight>0)RestoreDistances();return;}
            // Day/night supplies a fresh base each Update. These are restrained regional accents.
            RenderSettings.ambientEquatorColor=Color.Lerp(RenderSettings.ambientEquatorColor,RenderSettings.ambientEquatorColor*new Color(.92f,.94f,.87f),Weight);
            RenderSettings.ambientGroundColor=Color.Lerp(RenderSettings.ambientGroundColor,RenderSettings.ambientGroundColor*new Color(.88f,.91f,.83f),Weight);
            var light=clock.CelestialLight;
            if(light!=null){light.color=Color.Lerp(light.color,light.color*new Color(1,.97f,.89f),Weight);light.intensity*=Mathf.Lerp(1,.90f,Weight);}
            float day=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.1f,.3f,WorldDayNightCycle.SunElevation(clock.Hour)));
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=Color.Lerp(RenderSettings.fogColor,Color.Lerp(new Color(.10f,.15f,.22f),new Color(.48f,.53f,.47f),day),Weight*.5f);
            RenderSettings.fogStartDistance=Mathf.Lerp(originalFogStart,62,Weight);
            RenderSettings.fogEndDistance=Mathf.Lerp(originalFogEnd,230,Weight);
        }
        void RestoreDistances(){RenderSettings.fog=originalFog;RenderSettings.fogMode=originalFogMode;RenderSettings.fogStartDistance=originalFogStart;RenderSettings.fogEndDistance=originalFogEnd;}
        void OnDisable(){Weight=0;if(clock!=null)clock.RefreshLighting();RestoreDistances();}
    }
}
