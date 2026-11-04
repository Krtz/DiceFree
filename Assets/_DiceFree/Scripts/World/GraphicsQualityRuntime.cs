using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DiceFree.World
{
    public enum VisualQuality { Low, Medium, High, Ultra }

    /// <summary>Per-session URP copy. Never edits the project's shared pipeline assets.</summary>
    public sealed class GraphicsQualityRuntime : MonoBehaviour
    {
        [Serializable] sealed class Preferences { public int version=1;public VisualQuality preset=VisualQuality.High; }
        static GraphicsQualityRuntime current;
        UniversalRenderPipelineAsset owned;
        RenderPipelineAsset previous;
        public static VisualQuality Preset => current!=null?current.selected:VisualQuality.High;
        public static UniversalRenderPipelineAsset Pipeline => current!=null?current.owned:null;
        VisualQuality selected=VisualQuality.High;
        public static event Action Changed;
        public static string SettingsPath
        {
            get
            {
                var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-diceFreeSaveRoot");
                if(index>=0&&index+1<args.Length&&Path.IsPathFullyQualified(args[index+1]))return Path.Combine(args[index+1],"Settings","graphics-quality.json");
#if UNITY_EDITOR
                string path=Environment.GetEnvironmentVariable("DICEFREE_EDITOR_SETTINGS_ROOT");
                if(!string.IsNullOrEmpty(path))return Path.Combine(path,"graphics-quality.json");
                if(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DICEFREE_EDITOR_SAVE_ROOT"))||Environment.GetEnvironmentVariable("DICEFREE_DISABLE_PERSISTENCE")=="1")return null;
#endif
                return Path.Combine(Application.persistentDataPath,"Settings","graphics-quality.json");
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSession(){current=null;Changed=null;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install(){if(current!=null)return;var go=new GameObject("DiceFree graphics quality");DontDestroyOnLoad(go);go.AddComponent<GraphicsQualityRuntime>();}
        void Awake()
        {
            if(current!=null){Destroy(gameObject);return;}current=this;
            previous=QualitySettings.renderPipeline;
            var source=(previous!=null?previous:GraphicsSettings.defaultRenderPipeline) as UniversalRenderPipelineAsset;
            if(source==null){enabled=false;return;}
            owned=Instantiate(source);owned.name="DiceFree session quality (runtime copy)";QualitySettings.renderPipeline=owned;
            Reload();
        }
        public static void Select(VisualQuality preset,bool persist=true)
        {
            if(!Enum.IsDefined(typeof(VisualQuality),preset))throw new ArgumentOutOfRangeException(nameof(preset));
            if(current==null)Install();if(current==null||current.owned==null)return;
            current.selected=preset;current.Apply();
            string path=SettingsPath;if(persist&&path!=null){Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,JsonUtility.ToJson(new Preferences{preset=preset},true));}
            Changed?.Invoke();
        }
        public static void Reload()
        {
            if(current==null)return;var preset=VisualQuality.High;var path=SettingsPath;
            if(path!=null&&File.Exists(path))
            {
                try{var data=JsonUtility.FromJson<Preferences>(File.ReadAllText(path));if(data!=null&&data.version==1&&Enum.IsDefined(typeof(VisualQuality),data.preset))preset=data.preset;}
                catch(IOException){ }catch(ArgumentException){ }
            }
            Select(preset,false);
        }
        void Apply()
        {
            int p=(int)selected;
            owned.renderScale=new[]{.8f,1f,1f,1.15f}[p];
            owned.shadowDistance=new[]{35f,60f,90f,120f}[p];
            owned.mainLightShadowmapResolution=new[]{512,1024,2048,4096}[p];
            owned.msaaSampleCount=new[]{1,2,2,4}[p];
        }
        void OnDestroy()
        {
            if(current!=this)return;
            if(QualitySettings.renderPipeline==owned)QualitySettings.renderPipeline=previous;
            if(owned!=null)Destroy(owned);current=null;
        }
    }
}
