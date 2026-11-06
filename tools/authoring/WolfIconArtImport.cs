using System;
using System.IO;
using System.Linq;
using DiceFree.Items;
using UnityEditor;
using UnityEngine;
public static class WolfIconArtImport
{
 public static object Run()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required for art import");
  var items=AssetDatabase.FindAssets("t:ItemDefinition",new[]{"Assets/_DiceFree/Resources/DiceFree/DungeonRewards/Wolf"}).Select(g=>AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
  if(items.Length!=6)throw new InvalidOperationException("Expected the six existing wolf items");
  var rows=items.Select(item=>
  {
   string path="Assets/_DiceFree/Art/ItemIcons/WolfRewards/"+item.stableId+".png";string guid=AssetDatabase.AssetPathToGUID(path);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
   var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.maxTextureSize=512;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
   var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite==null||sprite.texture.width!=512||sprite.texture.height!=512||item.icon!=sprite||guid!=AssetDatabase.AssetPathToGUID(path))throw new InvalidOperationException("Existing icon reference or dimensions changed: "+item.displayName);
   var decoded=new Texture2D(2,2);if(!decoded.LoadImage(File.ReadAllBytes(path)))throw new InvalidOperationException("Invalid illustration PNG");var pixels=decoded.GetPixels32();int clear=pixels.Count(p=>p.a==0),solid=pixels.Count(p=>p.a>240);UnityEngine.Object.DestroyImmediate(decoded);if(clear==0||solid==0)throw new InvalidOperationException("Expected actual transparent background and solid subject");
   return new{item=item.displayName,itemId=item.stableId,path,guid,width=sprite.texture.width,alpha=true,clearPixels=clear,solidPixels=solid};
  }).ToArray();
  File.WriteAllText("Logs/WolfIconArtImport.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{passed=true,items=rows},Newtonsoft.Json.Formatting.Indented));return new{passed=true,icons=rows.Length,preservedItemReferences=true,importSize=512};
 }
}
