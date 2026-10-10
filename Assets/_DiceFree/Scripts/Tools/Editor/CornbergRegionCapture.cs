using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;
namespace DiceFree.EditorTools {
internal static class CornbergRegionCapture {
[CliCommand("dicefree.visual.capture-regions","Capture bridge, southern homes and eastern forest from real Unity scene.")]
public static object Capture(){
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Exit Play Mode");
for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Unsaved scene");
var setup=EditorSceneManager.GetSceneManagerSetup();
var files=new System.Collections.Generic.List<string>();
try{
var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity",OpenSceneMode.Single);
var pairs=new[]{
 ("Bridge",new Vector3(-16,0,26),28f),
 ("South",new Vector3(13,0,-22),28f),
 ("RoadSlimes",new Vector3(94,0,15),49f),
 ("East",new Vector3(162,0,114),70f),
 ("TwoShopFronts",new Vector3(7,0,-11),24f),
 ("ForestEntry",new Vector3(84,0,34),38f)
};
foreach(var tuple in pairs){
var target=tuple.Item2;
var go=new GameObject("Temporary capture camera");
SceneManager.MoveGameObjectToScene(go,scene);
var camera=go.AddComponent<Camera>();
camera.orthographic=true;camera.orthographicSize=tuple.Item3*.63f;
camera.transform.position=target+(tuple.Item1=="TwoShopFronts" ? new Vector3(-19f,32f,34f) : new Vector3(38f,47f,-38f));
camera.transform.LookAt(target);
var rt=new RenderTexture(1280,720,24);
camera.targetTexture=rt;camera.Render();
var last=RenderTexture.active;RenderTexture.active=rt;
var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);
tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();
RenderTexture.active=last;
var path=Path.Combine(Directory.GetParent(Application.dataPath).FullName,
"VisualBaselines/Cornberg"+tuple.Item1+"After.png");
File.WriteAllBytes(path,tex.EncodeToPNG());files.Add(path);
camera.targetTexture=null;
rt.Release();UnityEngine.Object.DestroyImmediate(rt);
UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(go);
}
return new{success=true,files};
}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
}
}
}