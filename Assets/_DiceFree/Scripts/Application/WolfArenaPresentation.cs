using System.Collections.Generic;
using DiceFree.World;
using UnityEngine;
using UnityEngine.Rendering;
namespace DiceFree.Dungeons
{
    // An additive arena owns its temporary lighting and restores the overworld on every exit.
    public sealed class WolfArenaPresentation : MonoBehaviour
    {
        readonly Dictionary<Light,bool> lights=new();
        WorldDayNightCycle clock;bool clockWasEnabled,initialized,fog;
        AmbientMode ambientMode;Color sky,equator,ground,fogColor;Light sun;
        static Material warning;
        public static Material WarningMaterial
        {
            get{if(warning==null){warning=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name="Wolf trail warning",color=new Color(.48f,.80f,1f)};}return warning;}
        }
        void OnEnable()
        {
            if(!Application.isPlaying)return;
            clock=FindAnyObjectByType<WorldDayNightCycle>();if(clock!=null){clockWasEnabled=clock.enabled;clock.enabled=false;}
            ambientMode=RenderSettings.ambientMode;sky=RenderSettings.ambientSkyColor;equator=RenderSettings.ambientEquatorColor;ground=RenderSettings.ambientGroundColor;fog=RenderSettings.fog;fogColor=RenderSettings.fogColor;sun=RenderSettings.sun;
            foreach(var light in FindObjectsByType<Light>())if(light.type==LightType.Directional)
            {
                if(light.gameObject.scene==gameObject.scene)light.cullingMask&=~(1<<31);
                else if((light.cullingMask&(1<<8))!=0){lights[light]=light.enabled;light.enabled=false;}
            }
            initialized=true;Apply();
        }
        void LateUpdate(){if(initialized)Apply();}
        void Apply()
        {
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.27f,.36f,.53f);RenderSettings.ambientEquatorColor=new Color(.18f,.25f,.34f);RenderSettings.ambientGroundColor=new Color(.12f,.17f,.23f);RenderSettings.fog=false;RenderSettings.fogColor=new Color(.07f,.12f,.2f);
        }
        void OnDisable()
        {
            if(!initialized)return;initialized=false;
            foreach(var pair in lights)if(pair.Key!=null)pair.Key.enabled=pair.Value;lights.Clear();
            RenderSettings.ambientMode=ambientMode;RenderSettings.ambientSkyColor=sky;RenderSettings.ambientEquatorColor=equator;RenderSettings.ambientGroundColor=ground;RenderSettings.fog=fog;RenderSettings.fogColor=fogColor;RenderSettings.sun=sun;
            if(clock!=null){clock.enabled=clockWasEnabled;if(clockWasEnabled)clock.RefreshLighting();}
        }
    }
}
