using System;
using System.IO;
using System.Linq;
using DiceFree.Dungeons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
public static class DungeonTreasuryAuthoring
{
 const string Folder="Assets/_DiceFree/Art/World/DungeonTreasury";
 static Transform root;
 static GameObject Part(string name,PrimitiveType shape,Vector3 p,Vector3 size,Material material)
 {var go=GameObject.CreatePrimitive(shape);go.name=name;go.layer=2;go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
 public static object Author()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().isDirty||AssetDatabase.IsValidFolder(Folder))throw new InvalidOperationException("Clean unauthored Edit Mode required");
  const string path="Assets/_DiceFree/Scenes/DungeonRewardRoom.unity";if(File.Exists("Logs/DungeonRewardRoom-before-WolfRewards.unity"))throw new InvalidOperationException("Preserve previous recovery");File.Copy(path,"Logs/DungeonRewardRoom-before-WolfRewards.unity");var previous=SceneManager.GetActiveScene();var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
  try
  {
   SceneManager.SetActiveScene(scene);AssetDatabase.CreateFolder("Assets/_DiceFree/Art/World","DungeonTreasury");var all=scene.GetRootGameObjects();var original=all.Single(g=>g.name=="Reusable neutral reward room");original.transform.Find("Neutral floor").GetComponent<Renderer>().enabled=false;foreach(var text in original.GetComponentsInChildren<TextMesh>())text.GetComponent<Renderer>().enabled=false;
   root=new GameObject("Dungeon treasury - furnished stone hall").transform;root.position=SlimeDungeonRun.RewardOrigin;
   Material M(string title,string source,Color tint,float tiles=1){var m=new Material(AssetDatabase.LoadAssetAtPath<Material>(source)){name=title,color=tint};m.mainTextureScale=Vector2.one*tiles;AssetDatabase.CreateAsset(m,Folder+"/"+title+".mat");return m;}
   string palette="Assets/_DiceFree/Art/World/ArboriumArrival/";var stone=M("Treasury granite",palette+"radial granite paving.mat",new Color(.45f,.51f,.57f),8);var walls=M("Treasury carved stone",palette+"weathered limestone.mat",new Color(.48f,.51f,.56f),4);var bronze=M("Treasury bronze",palette+"old gilded bronze.mat",new Color(.72f,.48f,.19f));var cloth=M("Treasury blue runner",palette+"guild blue cloth.mat",new Color(.09f,.19f,.3f));var wood=M("Treasury supply oak",palette+"oiled oak.mat",new Color(.3f,.19f,.12f));var backdrop=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name="Treasury dark backdrop",color=new Color(.045f,.065f,.09f)};AssetDatabase.CreateAsset(backdrop,Folder+"/Treasury dark backdrop.mat");
   Part("Distant dark floor",PrimitiveType.Cube,new Vector3(0,-.4f,0),new Vector3(130,.1f,130),backdrop);Part("Fitted stone floor",PrimitiveType.Cube,new Vector3(0,.012f,0),new Vector3(24,.02f,24),stone);
   Part("Blue treasure runner",PrimitiveType.Cube,new Vector3(0,.035f,2),new Vector3(8,.025f,8),cloth);foreach(float side in new[]{-1f,1f}){Part("Runner bronze edge",PrimitiveType.Cube,new Vector3(side*4,.055f,2),new Vector3(.09f,.015f,8),bronze);Part("Runner end",PrimitiveType.Cube,new Vector3(0,.055f,2+side*4),new Vector3(8,.015f,.09f),bronze);}
   foreach(float side in new[]{-1f,1f})
   {
    Part("Side wall",PrimitiveType.Cube,new Vector3(side*12.3f,1.8f,0),new Vector3(.5f,3.6f,25),walls);Part("End wall",PrimitiveType.Cube,new Vector3(0,1.8f,side*12.3f),new Vector3(25,3.6f,.5f),walls);
    Part("Side stone cornice",PrimitiveType.Cube,new Vector3(side*12.3f,3.7f,0),new Vector3(.8f,.25f,25),bronze);Part("End stone cornice",PrimitiveType.Cube,new Vector3(0,3.7f,side*12.3f),new Vector3(25,.25f,.8f),bronze);
    foreach(float z in new[]{-10f,10f})
    {
     Part("Carved column base",PrimitiveType.Cube,new Vector3(side*10.7f,.22f,z),new Vector3(1.35f,.44f,1.35f),bronze);Part("Treasury stone column",PrimitiveType.Cylinder,new Vector3(side*10.7f,2.15f,z),new Vector3(.9f,1.95f,.9f),walls);Part("Column crown",PrimitiveType.Cube,new Vector3(side*10.7f,4.25f,z),new Vector3(1.4f,.35f,1.4f),bronze);
     var lamp=Part("Lantern glass",PrimitiveType.Sphere,new Vector3(side*10.7f,4.65f,z),Vector3.one*.65f,AssetDatabase.LoadAssetAtPath<Material>(palette+"practical enchanted glass.mat"));var light=lamp.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.74f,.43f);light.intensity=3;light.range=14;light.shadows=LightShadows.None;light.cullingMask=~(1<<31);
    }
    foreach(float z in new[]{-6f,-3f,7f}){Part("Sealed storage crate",PrimitiveType.Cube,new Vector3(side*10.4f,.75f,z),new Vector3(1.6f,1.5f,1.7f),wood);foreach(float offset in new[]{-.5f,.5f})Part("Storage crate binding",PrimitiveType.Cube,new Vector3(side*10.4f+offset,.75f,z),new Vector3(.09f,1.55f,1.75f),bronze);}
   }
   AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Treasury save failed");return new{originalCollisionPreserved=true,navigationUnchanged=true,decorativeProps=root.childCount};
  }
  finally{EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(previous);}
 }
}
