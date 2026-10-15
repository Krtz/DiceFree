using System;
using System.IO;
using System.Linq;
using DiceFree.UI;
using UnityEditor;
using UnityEngine;
using Unity.Pipeline.Commands;
namespace DiceFree.EditorTools
{
 public static class CreditsAuthoring
 {
  [Serializable] sealed class Data { public CreditBook.Entry[] entries; }
  static void Folder(string path) {
   if(AssetDatabase.IsValidFolder(path))return;
   string parent=Path.GetDirectoryName(path).Replace('\\','/');
   Folder(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(path));
  }
  [CliCommand("dicefree.credits.author","Build runtime credits from the in-use asset provenance manifest.")]
  public static object Run() {
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode.");
   const string folder="Assets/_DiceFree/Resources/Credits";
   Folder(folder+"/Notices");
   var data=JsonUtility.FromJson<Data>(File.ReadAllText("SourceArt/Credits/credit_manifest.json"));
   if(data?.entries==null||data.entries.Length<2)throw new InvalidOperationException("Missing credit entries.");
   if(data.entries.Select(e=>e.title).Distinct().Count()!=data.entries.Length)throw new InvalidOperationException("Duplicate credits.");
   foreach(var e in data.entries)
    foreach(string url in new[]{e.sourceUrl,e.licenseUrl})
     if(!Uri.TryCreate(url,UriKind.Absolute,out var uri)||uri.Scheme!="https")throw new InvalidOperationException("Unsafe credit URL: "+url);
   const string notice=folder+"/Notices/ShaderCompatibility.txt";
   File.Copy("Assets/_DiceFree/Shaders/Compatibility/LICENSE.md",notice,true);
   AssetDatabase.ImportAsset(notice,ImportAssetOptions.ForceSynchronousImport);
   string path=folder+"/CreditBook.asset";
   var book=AssetDatabase.LoadAssetAtPath<CreditBook>(path);
   if(book==null){book=ScriptableObject.CreateInstance<CreditBook>();AssetDatabase.CreateAsset(book,path);}
   book.entries=data.entries;book.notices=new[]{AssetDatabase.LoadAssetAtPath<TextAsset>(notice)};
   EditorUtility.SetDirty(book);AssetDatabase.SaveAssets();
   return new{marker="DICEFREE_CREDITS_AUTHORED",entries=book.entries.Length,notices=book.notices.Length};
  }
 }
}
