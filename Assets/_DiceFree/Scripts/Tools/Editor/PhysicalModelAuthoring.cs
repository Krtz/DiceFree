using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.UI;
using DiceFree.Visuals;
using DiceFree.Characters;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;

namespace DiceFree.EditorTools
{
    public static class PhysicalModelAuthoring
    {
        public const string Source = "Assets/Blink/Art/Characters/Stylized/Humans/Prefabs_Humans/HumanMale_Character_Free.prefab";
        public const string Local = "Assets/_DiceFree/Resources/PhysicalLicensed";
        [CliCommand("dicefree.physical.model.author", "Adapt the owned Blink starter-armour human without changing class data or vendor originals.")]
        public static object Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            if (setup.Any(s => SceneManager.GetSceneByPath(s.path).isDirty)) throw new InvalidOperationException("Save open scene changes first.");
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Source);
            if (source == null) throw new InvalidOperationException("Owned Blink source pack is not installed.");
            if (!AssetDatabase.IsValidFolder(Local)) AssetDatabase.CreateFolder("Assets/_DiceFree/Resources", "PhysicalLicensed");
            if (!AssetDatabase.IsValidFolder(Local+"/Materials")) AssetDatabase.CreateFolder(Local,"Materials");
            var fallback = AssetDatabase.LoadAssetAtPath<GameObject>(PhysicalBlessedAuthoring.PhysicalPrefabPath);
            var controller = fallback.GetComponentInChildren<Animator>(true).runtimeAnimatorController;
            var root = UnityEngine.Object.Instantiate(source);
            try
            {
                root.name = "PhysicalHero";
                root.transform.localPosition = Vector3.zero;
                root.transform.localRotation = Quaternion.identity;
                var animator = root.GetComponentInChildren<Animator>(true);
                if (animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman) throw new InvalidOperationException("Blink source must retain a valid Humanoid avatar.");
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    if (t.name == "Starter_Helmet") t.gameObject.SetActive(false);
                var materials = new Dictionary<Material,Material>();
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    var remapped = renderer.sharedMaterials.Select(original => {
                        if(original == null) return null;
                        if(materials.TryGetValue(original, out var existing)) return existing;
                        string path=Local+"/Materials/"+original.name.Replace("/","_")+".mat";
                        var converted=AssetDatabase.LoadAssetAtPath<Material>(path);
                        if(converted==null){converted=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(converted,path);}
                        converted.SetTexture("_BaseMap",original.HasProperty("_BaseMap")?original.GetTexture("_BaseMap"):original.mainTexture);
                        converted.SetColor("_BaseColor",original.HasProperty("_BaseColor")?original.GetColor("_BaseColor"):original.HasProperty("_Color")?original.color:Color.white);
                        converted.SetFloat("_Smoothness",.2f);EditorUtility.SetDirty(converted);materials.Add(original,converted);return converted;
                    }).ToArray();
                    renderer.sharedMaterials=remapped;
                }
                foreach(var col in root.GetComponentsInChildren<Collider>(true))UnityEngine.Object.DestroyImmediate(col);
                PrefabUtility.SaveAsPrefabAsset(root,Local+"/PhysicalHero.prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            var contents=PrefabUtility.LoadPrefabContents(PhysicalBlessedAuthoring.PhysicalPrefabPath);
            try { if(contents.GetComponent<PhysicalBlessedVisual>()==null)contents.AddComponent<PhysicalBlessedVisual>(); PrefabUtility.SaveAsPrefabAsset(contents,PhysicalBlessedAuthoring.PhysicalPrefabPath); }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            try
            {
                foreach(string path in new[]{"Assets/_DiceFree/Scenes/Cornberg.unity","Assets/_DiceFree/Scenes/DevelopmentRoom.unity"})
                {
                    var scene=EditorSceneManager.OpenScene(path);
                    foreach(var switcher in UnityEngine.Object.FindObjectsByType<ClassPresentationSwitcher>(FindObjectsInactive.Include))
                    {
                        var physical=switcher.Resolve(PhysicalBlessedAuthoring.ClassId);
                        if(physical==null)continue;
                        if(physical.GetComponent<PhysicalBlessedVisual>()==null)physical.gameObject.AddComponent<PhysicalBlessedVisual>();
                        var so=new SerializedObject(switcher);var bindings=so.FindProperty("bindings");
                        for(int i=0;i<bindings.arraySize;i++){var b=bindings.GetArrayElementAtIndex(i);if(b.FindPropertyRelative("classId").stringValue==PhysicalBlessedAuthoring.ClassId)b.FindPropertyRelative("animator").objectReferenceValue=null;}
                        so.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(switcher);
                    }
                    EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                }
                AssetDatabase.SaveAssets();
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            return new { success=true, source=Source, derived=Local+"/PhysicalHero.prefab", fallback=PhysicalBlessedAuthoring.PhysicalPrefabPath };
        }
    }
}
