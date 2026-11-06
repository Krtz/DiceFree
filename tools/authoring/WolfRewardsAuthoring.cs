using System;
using System.IO;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Dungeons;
using DiceFree.Items;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
public static class WolfRewardsAuthoring
{
 const string Items="Assets/_DiceFree/Resources/DiceFree/DungeonRewards/Wolf";
 const string Art="Assets/_DiceFree/Art/ItemIcons/WolfRewards";
 static Material wood,bone,silver,gold,hide,blue,green,purple;
 static void Folder(string path){if(AssetDatabase.IsValidFolder(path))return;Folder(Path.GetDirectoryName(path).Replace('\\','/'));AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\','/'),Path.GetFileName(path));}
 static ItemDefinition Item(string id,string title,EquipmentSlot slot,EquipmentStats stats,string[] classes,int hands=1,bool rare=false)
 {
  var item=ScriptableObject.CreateInstance<ItemDefinition>();item.name=title;item.stableId="item.wolf-survival."+id;item.displayName=title;item.slot=slot;item.itemLevel=20;item.handsRequired=hands;item.allowedClassIds=classes;item.sourceId="dungeon.ironpine.wolf-survival";item.rarityId=rare?"rarity.rare":"rarity.uncommon";item.stats=stats;
  item.flavor=title+" — recovered from the moonlit clearing.";AssetDatabase.CreateAsset(item,Items+"/"+title.Replace("'","")+".asset");return item;
 }
 public static object Author()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||AssetDatabase.IsValidFolder(Items))throw new InvalidOperationException("Clean, unauthored Edit Mode required");
  const string tuningPath="Assets/_DiceFree/Settings/WolfSurvival/Wolf Survival.asset";
  if(File.Exists("Logs/Wolf-Survival-before-rewards.asset"))throw new InvalidOperationException("Preserve existing tuning backup");File.Copy(tuningPath,"Logs/Wolf-Survival-before-rewards.asset");
  Folder(Items);Folder(Art);
  var ring=Item("moonfang-signet","Moonfang Signet",EquipmentSlot.Ring,new EquipmentStats{attributes=new AttributeValues(2),physicalDefense=2,magicalDefense=2,regeneration=1},Array.Empty<string>(),rare:true);
  var chest=Item("nightpelt-vest","Nightpelt Vest",EquipmentSlot.Chest,new EquipmentStats{attributes=new AttributeValues{vitality=6,strength=2},physicalDefense=6,magicalDefense=2},Array.Empty<string>());
  var healer=Item("moonwell-staff","Moonwell Staff",EquipmentSlot.MainHand,new EquipmentStats{attributes=new AttributeValues{spirit=8,vitality=2},regeneration=.4f},new[]{"class.magically-touched-novice","class.priest"},2);
  var mage=Item("starfall-wand","Starfall Wand",EquipmentSlot.MainHand,new EquipmentStats{attributes=new AttributeValues{intelligence=7,vitality=2},basicAttackFlatBonus=3},new[]{"class.magically-touched-novice","class.mage"});
  var thief=Item("packhunters-fang","Packhunter's Fang",EquipmentSlot.MainHand,new EquipmentStats{attributes=new AttributeValues{agility=7,strength=2},attackSpeedPercent=6,basicAttackFlatBonus=3},new[]{"class.physically-blessed-novice","class.thief"});
  var acolyte=Item("duskwake-wand","Duskwake Wand",EquipmentSlot.MainHand,new EquipmentStats{attributes=new AttributeValues{intelligence=5,spirit=4},basicAttackFlatBonus=2},new[]{"class.magically-touched-novice","class.acolyte"});
  var pool=ScriptableObject.CreateInstance<DungeonRewardPool>();pool.stableId="loot.wolf-survival";pool.rare=ring;pool.items=new[]{chest,healer,mage,thief,acolyte};AssetDatabase.CreateAsset(pool,"Assets/_DiceFree/Settings/WolfSurvival/Wolf rewards.asset");
  var tuning=AssetDatabase.LoadAssetAtPath<WolfDungeonTuning>(tuningPath);tuning.rewards=pool;var slime=AssetDatabase.LoadAssetAtPath<SlimeDungeonTuning>("Assets/_DiceFree/Settings/Dungeons/Slime playtest tuning.asset");tuning.bonusXp=slime.bonusXp;tuning.bonusGold=slime.bonusGold;EditorUtility.SetDirty(tuning);
  RenderIcons(new[]{ring,chest,healer,mage,thief,acolyte});
  AssetDatabase.SaveAssets();return new{items=6,rare=ring.displayName,normalItems=pool.items.Length,bonusXp=tuning.bonusXp,bonusGold=tuning.bonusGold,icons=6};
 }
 public static object RebuildIcons()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required");
  var ids=new[]{"moonfang-signet","nightpelt-vest","moonwell-staff","starfall-wand","packhunters-fang","duskwake-wand"};
  var items=AssetDatabase.FindAssets("t:ItemDefinition",new[]{Items}).Select(g=>AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();RenderIcons(ids.Select(id=>items.Single(i=>i.stableId=="item.wolf-survival."+id)).ToArray());AssetDatabase.SaveAssets();return new{icons=6};
 }
 static void RenderIcons(ItemDefinition[] items)
 {
  // Selected illustration art is authoritative. Keep procedural geometry as a fallback,
  // but never overwrite the curated set during a later data-authoring pass.
  if(File.Exists("docs/assets/wolf-dungeon-icon-art.json"))
  {
   foreach(var item in items)
   {
    string path=Art+"/"+item.stableId+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
    var icon=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(icon==null)throw new InvalidOperationException("Selected wolf illustration missing: "+path);
    if(item.icon!=icon){item.icon=icon;EditorUtility.SetDirty(item);}
   }
   return;
  }
  bool asyncShaders=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
  try{MakeMaterials();int index=0;foreach(var item in items){Icon(item,index++);EditorUtility.SetDirty(item);}}
  finally{foreach(var m in new[]{wood,bone,silver,gold,hide,blue,green,purple})if(m!=null)Object.DestroyImmediate(m);ShaderUtil.allowAsyncCompilation=asyncShaders;}
 }
 static void MakeMaterials()
 {
  Material M(Color c,bool glow=false){var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=c};m.SetFloat("_Smoothness",.35f);m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*(glow?.8f:.12f));return m;}
  wood=M(new Color(.18f,.105f,.08f));bone=M(new Color(.91f,.83f,.64f));silver=M(new Color(.7f,.83f,.94f));gold=M(new Color(.8f,.48f,.15f));hide=M(new Color(.19f,.28f,.36f));blue=M(new Color(.12f,.6f,1),true);green=M(new Color(.18f,.85f,.55f),true);purple=M(new Color(.65f,.2f,.94f),true);
 }
 static GameObject Shape(Transform parent,PrimitiveType type,Vector3 p,Vector3 size,Material mat)
 {var go=GameObject.CreatePrimitive(type);Object.DestroyImmediate(go.GetComponent<Collider>());go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=mat;return go;}
 static void Ring(Transform parent,Vector3 center,float radius,float tube,Material mat)
 {
  var v=new System.Collections.Generic.List<Vector3>();var t=new System.Collections.Generic.List<int>();const int n=48,m=10;
  for(int a=0;a<=n;a++)for(int b=0;b<=m;b++){float u=a*2*Mathf.PI/n,w=b*2*Mathf.PI/m;v.Add(center+new Vector3(Mathf.Cos(u)*(radius+tube*Mathf.Cos(w)),Mathf.Sin(u)*(radius+tube*Mathf.Cos(w)),tube*Mathf.Sin(w)));if(a<n&&b<m){int i=a*(m+1)+b;t.AddRange(new[]{i,i+m+1,i+1,i+1,i+m+1,i+m+2});}}
  var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();var go=new GameObject("Original ring");go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mat;
 }
 static void Model(Transform root,int index)
 {
  void S(PrimitiveType type,Vector3 p,Vector3 size,Material mat)=>Shape(root,type,p,size,mat);
  if(index==0){Ring(root,Vector3.zero,.65f,.12f,silver);S(PrimitiveType.Cube,new Vector3(0,.67f,0),new Vector3(.5f,.35f,.25f),gold);S(PrimitiveType.Sphere,new Vector3(0,.75f,-.12f),new Vector3(.4f,.38f,.28f),blue);return;}
  if(index==1){S(PrimitiveType.Cube,Vector3.zero,new Vector3(1.1f,1.25f,.35f),hide);foreach(float side in new[]{-1f,1f}){S(PrimitiveType.Sphere,new Vector3(side*.57f,.6f,0),new Vector3(.55f,.4f,.48f),bone);S(PrimitiveType.Cube,new Vector3(side*.27f,-.15f,-.22f),new Vector3(.34f,.85f,.06f),wood);}S(PrimitiveType.Cube,new Vector3(0,-.32f,-.25f),new Vector3(1.12f,.12f,.1f),gold);Ring(root,new Vector3(0,-.32f,-.34f),.12f,.035f,silver);return;}
  if(index==4)
  {
   var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-.18f,-.1f,0),new Vector3(.18f,-.1f,0),new Vector3(0,1.25f,0),new Vector3(0,.2f,-.1f),new Vector3(0,.2f,.1f)};mesh.triangles=new[]{0,3,2,3,1,2,0,1,3,0,2,4,4,2,1,0,4,1};mesh.RecalculateNormals();var blade=new GameObject("Original fang blade");blade.transform.SetParent(root,false);blade.AddComponent<MeshFilter>().sharedMesh=mesh;blade.AddComponent<MeshRenderer>().sharedMaterial=silver;
   S(PrimitiveType.Cylinder,new Vector3(0,-.52f,0),new Vector3(.22f,.4f,.22f),wood);S(PrimitiveType.Cube,new Vector3(0,-.16f,0),new Vector3(.65f,.12f,.2f),gold);S(PrimitiveType.Sphere,new Vector3(0,-.95f,0),Vector3.one*.25f,bone);return;
  }
  bool staff=index==2;float length=staff?2.3f:1.6f;S(PrimitiveType.Cylinder,new Vector3(0,-.2f,0),new Vector3(.12f,length/2,.12f),wood);
  for(int k=0;k<4;k++)S(PrimitiveType.Cylinder,new Vector3(0,-.65f+k*.17f,0),new Vector3(.145f,.028f,.145f),staff?bone:gold);
  float top=length/2-.05f;
  if(staff){Ring(root,new Vector3(0,top+.22f,0),.32f,.07f,silver);S(PrimitiveType.Sphere,new Vector3(0,top+.22f,0),Vector3.one*.34f,green);foreach(float side in new[]{-1f,1f})S(PrimitiveType.Sphere,new Vector3(side*.2f,top-.2f,0),new Vector3(.3f,.15f,.12f),green);}
  else{S(PrimitiveType.Sphere,new Vector3(0,top+.13f,0),new Vector3(.32f,.57f,.32f),index==3?blue:purple);foreach(float side in new[]{-1f,1f}){var claw=Shape(root,PrimitiveType.Cube,new Vector3(side*.19f,top+.04f,0),new Vector3(.09f,.48f,.09f),bone);claw.transform.localRotation=Quaternion.Euler(0,0,side*20);}}
 }
 static void Icon(ItemDefinition item,int index)
 {
  var preview=new PreviewRenderUtility();var old=RenderTexture.active;bool asyncShaders=ShaderUtil.allowAsyncCompilation;Texture2D image=null;GameObject root=null;
  try
  {
   ShaderUtil.allowAsyncCompilation=false;root=new GameObject("Original "+item.displayName+" icon source");Model(root.transform,index);root.transform.rotation=Quaternion.Euler(0,-18,index==0?10:index==1?0:-22);preview.AddSingleGO(root);
   var bounds=new Bounds(root.transform.position,Vector3.zero);foreach(var r in root.GetComponentsInChildren<Renderer>())bounds.Encapsulate(r.bounds);var camera=preview.camera;camera.orthographic=true;camera.orthographicSize=Mathf.Max(bounds.extents.x,bounds.extents.y)*1.2f;camera.nearClipPlane=.01f;camera.farClipPlane=20;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.055f,.085f,.13f,1);camera.transform.position=bounds.center+new Vector3(0,0,-6);camera.transform.rotation=Quaternion.identity;
   preview.lights[0].intensity=2.7f;preview.lights[0].transform.rotation=Quaternion.Euler(25,-25,0);preview.lights[1].intensity=1.4f;preview.lights[1].transform.rotation=Quaternion.Euler(-10,35,0);preview.ambientColor=Color.gray;
   preview.BeginPreview(new Rect(0,0,512,512),GUIStyle.none);preview.Render(true);RenderTexture.active=(RenderTexture)preview.EndPreview();image=new Texture2D(512,512,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,512,512),0,0);image.Apply();string path=Art+"/"+item.stableId+".png";File.WriteAllBytes(path,image.EncodeToPNG());AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();item.icon=AssetDatabase.LoadAssetAtPath<Sprite>(path);
  }
  finally{if(root!=null)foreach(var mf in root.GetComponentsInChildren<MeshFilter>())if(!AssetDatabase.Contains(mf.sharedMesh)&&mf.sharedMesh.name=="")Object.DestroyImmediate(mf.sharedMesh);RenderTexture.active=old;ShaderUtil.allowAsyncCompilation=asyncShaders;preview.Cleanup();if(image!=null)Object.DestroyImmediate(image);}
 }
}
