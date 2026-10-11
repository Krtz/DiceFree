using System;
using System.IO;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
 internal static class IronpineCapture
 {
  [CliCommand("dicefree.ironpine.capture","Capture actual northeast forest road and Village 2 future site.")]
  public static object Capture()
  {
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode");
   for(int i=0;i<SceneManager.sceneCount;i++)
    if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save open scene first");
   var setup=EditorSceneManager.GetSceneManagerSetup();
   try
   {
    var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity",OpenSceneMode.Single);
    var views=new[]{
     ("PineRoad",new Vector3(186,0,102),46f),
     ("Village2Approach",new Vector3(219,0,141),28f),
     ("PineEntrance",new Vector3(150,0,51),32f),
     ("RoadOnly",new Vector3(186,0,102),43f)
    };
    var output=new System.Collections.Generic.List<string>();
    foreach(var view in views)
    {
     var o=new GameObject("Ironpine preview camera");
     SceneManager.MoveGameObjectToScene(o,scene);
     var camera=o.AddComponent<Camera>();
     camera.orthographic=true;
     camera.orthographicSize=view.Item3*.63f;
     camera.transform.position=view.Item2+new Vector3(36,45,-39);
     camera.transform.LookAt(view.Item2);
     camera.clearFlags=CameraClearFlags.Skybox;
     var isolateRoad=view.Item1=="RoadOnly";
     Transform road=null;
     if(isolateRoad)
     {
         road=scene.GetRootGameObjects()
             .SelectMany(g=>g.GetComponentsInChildren<Transform>(true))
             .First(t=>t.name=="Winding pine forest road");
         road.gameObject.layer=28;
         camera.cullingMask=1<<28;
         camera.clearFlags=CameraClearFlags.SolidColor;
         camera.backgroundColor=Color.black;
     }
     if(view.Item1=="PineRoad" && Environment.GetEnvironmentVariable(
         "DICEFREE_IRONPINE_ISOLATE")=="1")
     {
         camera.cullingMask=1<<2;
         camera.clearFlags=CameraClearFlags.SolidColor;
         camera.backgroundColor=Color.black;
     }
     var rt=new RenderTexture(1280,720,24);
     camera.targetTexture=rt;camera.Render();
     var old=RenderTexture.active;RenderTexture.active=rt;
     var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
     image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
     RenderTexture.active=old;
     string file=Path.Combine(Directory.GetParent(Application.dataPath).FullName,
      "VisualBaselines/Ironpine"+view.Item1+".png");
     Directory.CreateDirectory(Path.GetDirectoryName(file));
     File.WriteAllBytes(file,image.EncodeToPNG());
     output.Add(file);
     camera.targetTexture=null;rt.Release();
     UnityEngine.Object.DestroyImmediate(rt);
     UnityEngine.Object.DestroyImmediate(image);
     UnityEngine.Object.DestroyImmediate(o);
     if(road!=null)road.gameObject.layer=2;
    }
    return new{success=true,files=output};
   }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
  }
 }
}