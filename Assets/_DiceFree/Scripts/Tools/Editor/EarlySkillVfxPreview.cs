using System;
using System.IO;
using System.Linq;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using DiceFree.Combat;

namespace DiceFree.EditorTools
{
    internal static class EarlySkillVfxPreview
    {
        const string Preview = "Assets/_DiceFree/Art/Validation/Previews/StarterSkillsVfx_Unity.png";

        [CliCommand("dicefree.visual.skills.capture", "Capture 12 castable early skill VFX in a clean studio scene.")]
        public static object Capture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode");
            for (int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
                if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Unsaved scene");
            var previousSetup = EditorSceneManager.GetSceneManagerSetup();
            RenderTexture target=null, previous=RenderTexture.active;
            Texture2D bitmap=null;
            Camera camera=null;
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.transform.localScale = new Vector3(2,1,1.6f);
                floor.transform.position=Vector3.zero;
                UnityEngine.Object.DestroyImmediate(floor.GetComponent<Collider>());
                var shader=Shader.Find("Universal Render Pipeline/Lit");
                var floorMat=new Material(shader);
                floorMat.SetColor("_BaseColor",new Color(.095f,.13f,.18f));
                floor.GetComponent<Renderer>().sharedMaterial=floorMat;
                var all=new[]
                {
                    EarlySkillVfx.Cue.NoviceStrike,EarlySkillVfx.Cue.NoviceSand,EarlySkillVfx.Cue.NoviceHaste,EarlySkillVfx.Cue.NoviceHeal,
                    EarlySkillVfx.Cue.PhysicalHeavy,EarlySkillVfx.Cue.PhysicalGuard,EarlySkillVfx.Cue.PhysicalQuickening,EarlySkillVfx.Cue.PhysicalArrowArea,
                    EarlySkillVfx.Cue.MagicalSand,EarlySkillVfx.Cue.MagicalMend,EarlySkillVfx.Cue.MagicalFire,EarlySkillVfx.Cue.MagicalIceImpact
                };
                for(int i=0;i<all.Length;i++)
                    EarlySkillVfx.EditorPreview(all[i],new Vector3((i%4-1.5f)*2.6f,0f,(1-i/4)*3.0f),1.25f);

                foreach(var fx in UnityEngine.Object.FindObjectsByType<ParticleSystem>())
                    fx.Simulate(.22f,true,false,true);

                var lamp=new GameObject("Studio key").AddComponent<Light>();
                lamp.type=LightType.Directional;lamp.intensity=1.25f;lamp.shadows=LightShadows.None;
                lamp.transform.rotation=Quaternion.Euler(55,-35,0);
                camera=new GameObject("Studio camera").AddComponent<Camera>();
                camera.orthographic=true;camera.orthographicSize=6.45f;
                camera.backgroundColor=new Color(.075f,.095f,.14f);
                camera.clearFlags=CameraClearFlags.SolidColor;
                camera.transform.position=new Vector3(6f,11f,-11.5f);
                camera.transform.LookAt(new Vector3(0,0.45f,0));
                target=new RenderTexture(1440,1000,24);
                camera.targetTexture=target;camera.Render();
                RenderTexture.active=target;
                bitmap=new Texture2D(1440,1000,TextureFormat.RGB24,false);
                bitmap.ReadPixels(new Rect(0,0,1440,1000),0,0);bitmap.Apply();
                File.WriteAllBytes(Path.Combine(Directory.GetParent(Application.dataPath).FullName,Preview),bitmap.EncodeToPNG());
                AssetDatabase.ImportAsset(Preview,ImportAssetOptions.ForceUpdate);
                return new{success=true,cues=12,particles=UnityEngine.Object.FindObjectsByType<ParticleSystem>().Sum(p=>p.particleCount),file=Preview};
            }
            finally
            {
                if(camera!=null)camera.targetTexture=null;
                RenderTexture.active=previous;
                if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}
                if(bitmap!=null)UnityEngine.Object.DestroyImmediate(bitmap);
                EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }
        }
    }
}
