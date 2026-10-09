using System;
using System.IO;
using System.Linq;
using DiceFree.World;
using DiceFree.Dungeons;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
    internal static class WorldActorMotionPreview
    {
        const string Source="Assets/_DiceFree/Scenes/Cornberg.unity";
        const string Preview="Assets/_DiceFree/Art/Validation/Previews/VillageAndSlimeMotion_Unity.png";

        [CliCommand("dicefree.motion.capture", "Render a baseline and an animated-pose lineup using real Cornberg NPC and slime models.")]
        public static object Capture()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit mode only");
            for(int i=0;i<SceneManager.sceneCount;i++)
                if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Refusing dirty scene");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            RenderTexture target=null,previous=RenderTexture.active;
            Texture2D image=null;
            Camera camera=null;
            try
            {
                var source=EditorSceneManager.OpenScene(Source,OpenSceneMode.Single);
                var all=source.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
                var farmers=all.Select(t=>t.GetComponent<VillageCharacterGestures>())
                    .Where(n=>n!=null).ToArray();
                var monsters=all.Select(t=>t.GetComponent<SlimeVisualMotion>())
                    .Where(n=>n!=null).ToArray();
                GameObject Npc(VillageCharacterGestures.Personality type) =>
                    farmers.First(n=>n.Role==type).gameObject;
                GameObject Slime(string type) =>
                    monsters.First(n=>n.name.Contains(type)).gameObject;
                GameObject[] subjects={
                    Npc(VillageCharacterGestures.Personality.Farmer),
                    Npc(VillageCharacterGestures.Personality.Blacksmith),
                    Npc(VillageCharacterGestures.Personality.Alchemist),
                    Slime("Local Road Slime"),
                    Slime("Elite Forest Slime")
                };
                var stage=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
                var set=SceneManager.GetSceneByPath(stage.path);
                // Stage only contains clones. Gameplay/Cornberg scenes stay unchanged.
                for(int row=0;row<2;row++)
                for(int col=0;col<subjects.Length;col++)
                {
                    var copy=UnityEngine.Object.Instantiate(subjects[col]);
                    copy.name="MotionPreview_"+row+"_"+col;
                    SceneManager.MoveGameObjectToScene(copy,stage);
                    copy.transform.position=new Vector3((col-2)*2.6f,0,row==0?2f:-2f);
                    foreach(var item in copy.GetComponentsInChildren<Transform>(true))
                        item.gameObject.layer=31;
                    copy.transform.rotation=Quaternion.identity;
                    var npc=copy.GetComponent<VillageVisualIdle>();
                    var gesture=copy.GetComponent<VillageCharacterGestures>();
                    var slime=copy.GetComponent<SlimeVisualMotion>();
                    if(npc!=null)npc.SamplePose(row==0?1.2f:2.47f,row!=0, .4f);
                    if(gesture!=null)gesture.Sample(row==0?1.2f:2.47f,row!=0, .25f);
                    if(slime!=null)
                    {
                        slime.Sample(row==0?1.2f:2.47f,row==0?0:2.8f,.1f,
                            row==0?"Idle":col==3?"Wind-up":"Recovery",true);
                        if(row==1)slime.Sample(2.66f,2.4f,.1f,"Recovery",true);
                    }
                }

                var camGo=new GameObject("Motion studio camera");
                SceneManager.MoveGameObjectToScene(camGo,stage);
                camera=camGo.AddComponent<Camera>();
                camera.orthographic=true;camera.orthographicSize=5.05f;
                camera.cullingMask=1<<31;
                camera.clearFlags=CameraClearFlags.SolidColor;
                camera.backgroundColor=new Color(.125f,.16f,.21f);
                camera.transform.position=new Vector3(3.8f,10f,-16f);
                camera.transform.LookAt(new Vector3(0,.9f,0));
                var key=new GameObject("Preview key").AddComponent<Light>();
                SceneManager.MoveGameObjectToScene(key.gameObject,stage);
                key.type=LightType.Directional;key.intensity=1.4f;key.shadows=LightShadows.None;
                key.transform.rotation=Quaternion.Euler(45,-30,0);
                target=new RenderTexture(1440,1024,24);
                camera.targetTexture=target;
                camera.Render();
                RenderTexture.active=target;
                image=new Texture2D(1440,1024,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,1440,1024),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(Directory.GetParent(Application.dataPath).FullName,Preview),image.EncodeToPNG());
                AssetDatabase.ImportAsset(Preview,ImportAssetOptions.ForceUpdate);
                return new{success=true,realSubjects=5,poses=10,file=Preview,
                    layout="row1 idle; row2 active, Farmer/Blacksmith/Alchemist/Road/Elite"};
            }
            finally
            {
                if(camera!=null)camera.targetTexture=null;
                RenderTexture.active=previous;
                if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}
                if(image!=null)UnityEngine.Object.DestroyImmediate(image);
                EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }
    }
}
