using System;
using System.IO;
using System.Linq;
using DiceFree.Briarback;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Quests;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
public static class NovemberFeedbackAuthoring
{
 const string Folder="Assets/_DiceFree/Settings/Briarback/";
 public static object Apply()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Clean Edit Mode required.");
  var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
  if(!File.Exists("Logs/Cornberg-before-NovemberFeedback.unity"))File.Copy(scene.path,"Logs/Cornberg-before-NovemberFeedback.unity",false);
  var orb=AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/_DiceFree/Settings/Dungeons/Slime Orb.asset");orb.allowedClassIds=new[]{"class.magically-touched-novice","class.mage"};EditorUtility.SetDirty(orb);
  foreach(var name in new[]{"Ember Bolt","Frost Snare","Arc Lightning","Quicksand","Half-Life"}){var attack=AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/_DiceFree/Settings/Skills/Mage/"+name+" attack.asset");attack.baseDamage=20;EditorUtility.SetDirty(attack);}
  var bounty=AssetDatabase.LoadAssetAtPath<QuestDefinition>(Folder+"Another 20 Quillboars.asset");
  bounty.title="Another 15 Quillboars";bounty.version=2;bounty.stages[0].count=15;bounty.stages[0].instruction="Defeat 15 Quillboars";
  bounty.offer="Trimery needs the Briarback warband kept off the road. Defeat fifteen Quillboars, then return here for your bounty.";
  bounty.activeDialogue="Fifteen down. Return when the bounty is complete and I will pay you.";
  bounty.locationHint="Return to the Trimery bounty hunter after defeating 15 Quillboars.";
  bounty.rewardXp=20*AssetDatabase.LoadAssetAtPath<ActorDefinition>(Folder+"Raider-L25 stats.asset").experienceReward;EditorUtility.SetDirty(bounty);
  var hunt=AssetDatabase.LoadAssetAtPath<QuestDefinition>(Folder+"The Four Elements Hunt.asset");hunt.rewardXp=2500;EditorUtility.SetDirty(hunt);
  var data=AssetDatabase.LoadAssetAtPath<TravelSurfaceDefinition>("Assets/_DiceFree/Settings/Travel/Cornberg dirt road.asset");
  var root=GameObject.Find("14 - Briarback quilboar frontier");int roads=0;
  foreach(var mesh in root.GetComponentsInChildren<MeshFilter>(true).Where(m=>new[]{"Briarback northern detour","Briarback western scouting loop","Harrowtusk eastern camp trail","Briarback circular warband road","North approach to the open stronghold gate"}.Contains(m.name))){
   var surface=mesh.GetComponent<TravelSurface>()??mesh.gameObject.AddComponent<TravelSurface>();surface.Configure("surface-area.briarback."+mesh.name.ToLowerInvariant().Replace(' ','-'),data,mesh,new Bounds(Vector3.zero,Vector3.one*10000));roads++;
  }
  if(roads!=5)throw new InvalidOperationException("Expected all five authored road footprints.");
  var cornberg=Object.FindObjectsByType<ResurrectionAnchor>(FindObjectsInactive.Include).Single(a=>a.StableId=="anchor.cornberg");
  var die=Object.FindObjectsByType<ResurrectionDiceVisual>(FindObjectsInactive.Include).Single(d=>d.Anchor==cornberg);
  cornberg.Configure(cornberg.StableId,die.transform,4.5f);
  InstallMantleIcon();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
  return new{roads,roadBonusPercent=data.speedBonusPercent,bountyKills=15,bountyXp=bounty.rewardXp,fourElementsXp=hunt.rewardXp,mageBaseDamage=20};
 }
 public static string InstallMantleIcon()
 {
  const string path="Assets/_DiceFree/Art/ItemIcons/item.briarback.bristlehide-mantle.png";
  var preview=new PreviewRenderUtility();var oldTarget=RenderTexture.active;bool oldAsync=ShaderUtil.allowAsyncCompilation;
  var mesh=BriarbackMantleVisual.CreateMesh();Material hide=null,bone=null;Texture2D image=null;
  try{
   ShaderUtil.allowAsyncCompilation=false;
   hide=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=new Color(.37f,.25f,.13f)};hide.SetFloat("_Cull",0);hide.EnableKeyword("_EMISSION");hide.SetColor("_EmissionColor",hide.color*.4f);
   bone=new Material(hide){color=new Color(.85f,.74f,.5f)};bone.SetColor("_EmissionColor",bone.color*.3f);
   var model=new GameObject("Bristlehide Mantle icon source");model.AddComponent<MeshFilter>().sharedMesh=mesh;model.AddComponent<MeshRenderer>().sharedMaterial=hide;
   preview.AddSingleGO(model);
   for(int i=0;i<5;i++){var stud=GameObject.CreatePrimitive(PrimitiveType.Capsule);stud.transform.SetParent(model.transform,false);stud.transform.localPosition=new Vector3((i-2)*.09f,.16f,-.06f);stud.transform.localScale=new Vector3(.035f,.10f,.035f);stud.transform.localRotation=Quaternion.Euler(35,0,(i-2)*-12);Object.DestroyImmediate(stud.GetComponent<Collider>());stud.GetComponent<Renderer>().sharedMaterial=bone;}
   var c=preview.camera;c.orthographic=true;c.orthographicSize=.5f;c.nearClipPlane=.01f;c.farClipPlane=10;c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=Color.clear;
   c.transform.rotation=Quaternion.Euler(-8,165,-12);c.transform.position=new Vector3(0,-.13f,0)-c.transform.forward*3;
   preview.lights[0].intensity=3;preview.lights[0].transform.rotation=Quaternion.Euler(30,155,0);preview.lights[1].intensity=1.5f;preview.lights[1].transform.rotation=Quaternion.Euler(20,230,0);preview.ambientColor=Color.gray;
   preview.BeginPreview(new Rect(0,0,256,256),GUIStyle.none);preview.Render(true);RenderTexture.active=(RenderTexture)preview.EndPreview();
   image=new Texture2D(256,256,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
  }finally{RenderTexture.active=oldTarget;ShaderUtil.allowAsyncCompilation=oldAsync;preview.Cleanup();Object.DestroyImmediate(mesh);if(image!=null)Object.DestroyImmediate(image);if(hide!=null)Object.DestroyImmediate(hide);if(bone!=null)Object.DestroyImmediate(bone);}
  AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
  var item=AssetDatabase.LoadAssetAtPath<ItemDefinition>(Folder+"Bristlehide Mantle.asset");item.icon=AssetDatabase.LoadAssetAtPath<Sprite>(path);EditorUtility.SetDirty(item);AssetDatabase.SaveAssetIfDirty(item);return path;
 }
}
