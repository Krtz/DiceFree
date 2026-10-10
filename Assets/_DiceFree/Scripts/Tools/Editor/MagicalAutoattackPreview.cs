using System;
using System.IO;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using DiceFree.Combat;

namespace DiceFree.EditorTools
{
    internal static class MagicalAutoattackPreview
    {
        const string Output="Assets/_DiceFree/Art/Validation/Previews/ArcaneAutoattackMissile_Unity.png";
        [CliCommand("dicefree.magical.missile.capture","Capture visual-only Arcane Spark missile from an isolated Unity rendering scene.")]
        public static object Capture()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode");
            for(int i=0;i<SceneManager.sceneCount;i++)
                if(SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Unsaved scene; refusing to replace");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            RenderTexture texture=null,previous=RenderTexture.active;
            Texture2D bitmap=null;
            Camera cam=null;
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var shader=Shader.Find("Universal Render Pipeline/Lit");
                var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);
                UnityEngine.Object.DestroyImmediate(floor.GetComponent<Collider>());
                floor.transform.localScale=new Vector3(1.5f,1f,1f);
                var floorMat=new Material(shader);
                floorMat.SetColor("_BaseColor",new Color(.11f,.13f,.19f));
                floor.GetComponent<Renderer>().sharedMaterial=floorMat;
                var sphere=MagicalBasicMissileVfx.EditorPreview(
                    new Vector3(-2.7f,1.2f,0),new Vector3(2.8f,1.15f,0));
                var colliderCount=sphere.GetComponentsInChildren<Collider>().Length;
                if(colliderCount!=0)throw new InvalidOperationException("Projectile VFX may not have colliders");
                var src=GameObject.CreatePrimitive(PrimitiveType.Capsule);
                src.transform.position=new Vector3(-2.8f,.8f,0);
                src.transform.localScale=new Vector3(.6f,.8f,.6f);
                UnityEngine.Object.DestroyImmediate(src.GetComponent<Collider>());
                var srcMat=new Material(shader);
                srcMat.SetColor("_BaseColor",new Color(.19f,.17f,.44f));
                src.GetComponent<Renderer>().sharedMaterial=srcMat;
                var dest=GameObject.CreatePrimitive(PrimitiveType.Capsule);
                dest.transform.position=new Vector3(2.8f,.8f,0);
                dest.transform.localScale=new Vector3(.6f,.8f,.6f);
                UnityEngine.Object.DestroyImmediate(dest.GetComponent<Collider>());
                var dstMat=new Material(shader);
                dstMat.SetColor("_BaseColor",new Color(.35f,.25f,.20f));
                dest.GetComponent<Renderer>().sharedMaterial=dstMat;
                var key=new GameObject("Preview key").AddComponent<Light>();
                key.type=LightType.Directional;key.intensity=1.35f;
                key.transform.rotation=Quaternion.Euler(45,-28,0);
                cam=new GameObject("Missile camera").AddComponent<Camera>();
                cam.orthographic=true;cam.orthographicSize=3.2f;
                cam.clearFlags=CameraClearFlags.SolidColor;
                cam.backgroundColor=new Color(.07f,.085f,.135f);
                cam.transform.position=new Vector3(1.7f,5.3f,-9.3f);
                cam.transform.LookAt(new Vector3(0,1.15f,0));
                texture=new RenderTexture(1280,720,24);
                cam.targetTexture=texture;cam.Render();
                RenderTexture.active=texture;
                bitmap=new Texture2D(1280,720,TextureFormat.RGB24,false);
                bitmap.ReadPixels(new Rect(0,0,1280,720),0,0);bitmap.Apply();
                File.WriteAllBytes(Path.Combine(Directory.GetParent(Application.dataPath).FullName,Output),bitmap.EncodeToPNG());
                AssetDatabase.ImportAsset(Output,ImportAssetOptions.ForceUpdate);
                return new{success=true,preview=Output,colliders=colliderCount};
            }
            finally
            {
                if(cam!=null)cam.targetTexture=null;
                RenderTexture.active=previous;
                if(texture!=null){texture.Release();UnityEngine.Object.DestroyImmediate(texture);}
                if(bitmap!=null)UnityEngine.Object.DestroyImmediate(bitmap);
                EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }
    }
}
