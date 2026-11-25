using UnityEngine;

namespace DiceFree.World
{
    // One clock owner remains WorldDayNightCycle. The foundry shares its night.
    public sealed class FoundryAtmosphere : MonoBehaviour
    {
        [SerializeField] Light[] streetLights;
        [SerializeField] Light[] hearthLights;
        WorldDayNightCycle clock;
        float nextLookup;
        public void Configure(Light[] streets,Light[] hearths){streetLights=streets;hearthLights=hearths;}
        void Update()
        {
            if(clock==null&&Time.unscaledTime>=nextLookup){nextLookup=Time.unscaledTime+1;clock=FindAnyObjectByType<WorldDayNightCycle>();}
            bool night=clock!=null&&clock.MoonActive;
            foreach(var light in streetLights)if(light!=null)light.enabled=night;
            for(int i=0;i<hearthLights.Length;i++)if(hearthLights[i]!=null)
                hearthLights[i].intensity=3.0f+Mathf.PerlinNoise(Time.time*2.4f,i*13.7f)*1.3f;
        }
    }
}
