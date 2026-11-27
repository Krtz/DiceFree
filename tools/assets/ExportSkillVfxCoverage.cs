using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DiceFree.Combat;

var directory="T:/TEMP/DiceFree-AssetCatalog/2026-11-27";
string Q(string s)=>"\""+(s??"").Replace("\\","\\\\").Replace("\"","\\\"").Replace("\n"," ").Replace("\r"," ")+"\"";
var library=Resources.Load<AssetPackVfxLibrary>("AssetLibraryLicensed/VisualEffects");if(library==null||library.entries.Length!=29)throw new Exception("Missing native VFX library");
var rows=new List<string>();
void Row(string id,string name,string source){if(string.IsNullOrEmpty(id)||string.IsNullOrEmpty(name))return;string theme=AssetPackVfx.Theme(id,name);var entry=library.Find("cast."+theme);if(entry==null)throw new Exception("Missing cast binding: "+id+" "+name+" theme "+theme);rows.Add("{\"classId\":"+Q(id)+",\"skill\":"+Q(name)+",\"castKey\":"+Q(entry.key)+",\"prefab\":"+Q(AssetDatabase.GetAssetPath(entry.prefab))+",\"nameEvidence\":"+Q(source)+"}");}
var types=AppDomain.CurrentDomain.GetAssemblies().Where(a=>a.GetName().Name.StartsWith("DiceFree")).SelectMany(a=>a.GetTypes()).Where(t=>t.Namespace!=null&&t.Namespace.StartsWith("DiceFree")&&(t.Name.EndsWith("Progression")||t.Name.EndsWith("ProgressionRules")||t.Name.EndsWith("SpellRules")));
foreach(var type in types){
 var flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.FlattenHierarchy;
 string id=type.GetFields(flags).Where(f=>f.IsLiteral&&f.FieldType==typeof(string)).Select(f=>f.GetRawConstantValue() as string).FirstOrDefault(s=>s!=null&&s.StartsWith("class."));
 if(type.Name=="PriestSpellRules")id="class.priest";if(type.Name=="GreatbladeProgressionRules"){foreach(var value in Enum.GetValues(type.GetNestedType("Kind")))Row("class.greatblade",System.Text.RegularExpressions.Regex.Replace(value.ToString(),"(?<=[a-z])([A-Z])"," $1"),type.Name);continue;}if(type.Name=="MageProgressionRules")id="class.mage";
 if(type.Name=="PsionEvolutionProgression"){var m=type.GetMethod("Name",flags);for(int i=0;i<5;i++){Row("class.psionic-knight",(string)m.Invoke(null,new object[]{true,i}),type.Name);Row("class.mirage-mage",(string)m.Invoke(null,new object[]{false,i}),type.Name);}continue;}
 if(id==null)continue;
 var method=type.GetMethods(flags).FirstOrDefault(m=>m.Name=="Name"&&m.ReturnType==typeof(string)&&m.GetParameters().Length==1);if(method==null)continue;
 var arg=method.GetParameters()[0].ParameterType;
 if(arg.IsEnum){foreach(var value in Enum.GetValues(arg))Row(id,(string)method.Invoke(null,new[]{value}),type.Name);}
 else if(arg==typeof(int)){for(int i=0;i<16;i++){var n=(string)method.Invoke(null,new object[]{i});if(string.IsNullOrEmpty(n))break;Row(id,n,type.Name);}}
}
foreach(var spec in new[]{("NoviceSkillDefinition","class.novice"),("PhysicalSkillDefinition","class.physically-blessed-novice"),("MagicalSkillDefinition","class.magically-touched-novice")})foreach(var guid in AssetDatabase.FindAssets("t:"+spec.Item1)){
 var obj=AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));var name=obj.GetType().GetField("displayName").GetValue(obj) as string;Row(spec.Item2,name,spec.Item1);
}
File.WriteAllText(directory+"/skill-vfx-coverage.json","["+string.Join(",",rows.Distinct())+"]");
var dependencies=AssetDatabase.GetDependencies("Assets/_DiceFree/Resources/AssetLibraryLicensed/VisualEffects.asset",true).Where(p=>p.StartsWith("Assets/")).ToArray();File.WriteAllText(directory+"/vfx-dependencies.json","["+string.Join(",",dependencies.Select(Q))+"]");
var all=AssetDatabase.GetAllAssetPaths();var roots=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).Concat(all.Where(p=>p.Contains("/Resources/")&&!AssetDatabase.IsValidFolder(p))).ToArray();File.WriteAllText(directory+"/runtime-dependencies.json","["+string.Join(",",AssetDatabase.GetDependencies(roots,true).Select(Q))+"]");
return new{skills=rows.Distinct().Count(),nativeEntries=library.entries.Length,dependencyFiles=dependencies.Length,dirty=UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty};