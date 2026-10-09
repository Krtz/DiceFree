using System;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;
namespace DiceFree.EditorTools {
internal static class VisualSceneCapture {
 const string ScenePath="Assets/_DiceFree/Scenes/Cornberg.unity";
 [CliCommand("dicefree.visual.capture-cornberg","Capture actual Cornberg scene from a temporary isolated camera without saving any scene modifications.")]
 public static object Capture() {
 if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Exit Play Mode");
 for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save scene before visual capture");
 var prior=EditorSceneManager.GetSceneManagerSetup();RenderTexture rt=null;Texture2D image=null;GameObject cameraGo=null;
 try{
 var s=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
 var heal=s.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t=>t.name=="Cornberg magical well healing area");
 if(heal==null)throw new InvalidOperationException("Healing well anchor absent");
 var p=heal.position;
 cameraGo=new GameObject("Temporary VIS Camera");SceneManager.MoveGameObjectToScene(cameraGo,s);
 var camera=cameraGo.AddComponent<Camera>();
 camera.orthographic=true;camera.orthographicSize=12f;camera.clearFlags=CameraClearFlags.Skybox;
 camera.transform.position=p+new Vector3(18f,23f,-18f);camera.transform.LookAt(p);
 rt=new RenderTexture(1280,720,24);camera.targetTexture=rt;camera.Render();
 var old=RenderTexture.active;RenderTexture.active=rt;
 image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();RenderTexture.active=old;
 var path=System.IO.Path.GetFullPath("VisualBaselines/CornbergWellCurrent.png");
 Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));File.WriteAllBytes(path,image.EncodeToPNG());
 return new{success=true,file=path,bytes=new FileInfo(path).Length,cameraPosition=camera.transform.position.ToString(),wellPosition=p.ToString()};
 }finally{
 if(cameraGo!=null)UnityEngine.Object.DestroyImmediate(cameraGo);
 if(image!=null)UnityEngine.Object.DestroyImmediate(image);
 if(rt!=null){rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
 EditorSceneManager.RestoreSceneManagerSetup(prior);
 }
 }
}
}
