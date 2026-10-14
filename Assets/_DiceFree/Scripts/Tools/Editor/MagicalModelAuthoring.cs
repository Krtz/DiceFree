using System;
using System.Linq;
using DiceFree.UI;
using DiceFree.Visuals;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;

namespace DiceFree.EditorTools
{
    public static class MagicalModelAuthoring
    {
        public const string Source="Assets/Polytope Studio/Lowpoly_Characters/Sources/Modular_Armors/Meshes/PT_Lowpoly_Armors_Male_Moduar_Free.fbx";
        public const string StaffSource="Assets/AdventureForge/PreFab/Metalstaff01/512/Metalstaff01.prefab";
        public const string Local="Assets/_DiceFree/Resources/MagicalLicensed";
        static Material Mat(string name,Color color,Texture texture=null,float metal=0)
        {
            string path=Local+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetTexture("_BaseMap",texture);m.SetFloat("_Smoothness",.2f);m.SetFloat("_Metallic",metal);
            EditorUtility.SetDirty(m);return m;
        }
        static Bounds VisibleBounds(GameObject root)
        {
            var mesh=new Mesh();var b=new Bounds();bool first=true;
            try{foreach(var r in root.GetComponentsInChildren<Renderer>().Where(r=>r.enabled)){
                Bounds bounds;
                if(r is SkinnedMeshRenderer skin){skin.BakeMesh(mesh);bounds=mesh.bounds;}else{var filter=r.GetComponent<MeshFilter>();if(filter==null)continue;bounds=filter.sharedMesh.bounds;}
                for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2){
                    var point=r.transform.TransformPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3(x,y,z)));
                    if(first){b=new Bounds(point,Vector3.zero);first=false;}else b.Encapsulate(point);
                }
            }}finally{UnityEngine.Object.DestroyImmediate(mesh);}return b;
        }
        [CliCommand("dicefree.magical.model.author","Build a smaller cloth caster with the owned Polytope human and Adventure Forge staff.")]
        public static object Run()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            if(setup.Any(s=>SceneManager.GetSceneByPath(s.path).isDirty))throw new InvalidOperationException("Save scene changes first.");
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Source);var staffSource=AssetDatabase.LoadAssetAtPath<GameObject>(StaffSource);
            if(source==null||staffSource==null)throw new InvalidOperationException("Verified local source assets are missing.");
            if(!AssetDatabase.IsValidFolder(Local))AssetDatabase.CreateFolder("Assets/_DiceFree/Resources","MagicalLicensed");
            if(!AssetDatabase.IsValidFolder(Local+"/Materials"))AssetDatabase.CreateFolder(Local,"Materials");
            var fallback=AssetDatabase.LoadAssetAtPath<GameObject>(MagicalTouchedAuthoring.MagicalPrefabPath);
            var controller=fallback.GetComponentInChildren<Animator>(true).runtimeAnimatorController;
            var root=UnityEngine.Object.Instantiate(source);
            float height=0,width=0;
            try
            {
                root.name="MagicalHero";root.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
                var a=root.GetComponentInChildren<Animator>(true);
                if(a==null||a.avatar==null||!a.avatar.isValid||!a.avatar.isHuman)throw new InvalidOperationException("Polytope Humanoid avatar invalid.");
                a.runtimeAnimatorController=controller;a.applyRootMotion=false;a.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                string[] chosen={"cloth_00_body","cloth_00_legs","cloth_00_gauntlets","cloth_00_boots","head_01","hair_20","01_A_cape"};
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Lowpoly_Characters/Sources/Modular_Armors/Textures/PT_Armors_Base_Texture.png");
                var cloth=Mat("Caster indigo cloth",new Color(.25f,.30f,.59f),texture);
                var skin=Mat("Caster skin",new Color(.87f,.68f,.51f),texture);
                var hair=Mat("Caster dark hair",new Color(.17f,.12f,.10f),texture);
                var boots=Mat("Caster worn boots",new Color(.25f,.20f,.18f),texture);
                var cape=Mat("Caster violet cape",new Color(.39f,.22f,.50f),texture);
                foreach(var r in root.GetComponentsInChildren<SkinnedMeshRenderer>(true)){
                    r.enabled=chosen.Any(s=>r.name=="PT_Male_Armor_"+s);
                    if(!r.enabled)continue;
                    var material=r.name.Contains("head_")?skin:r.name.Contains("hair_")?hair:r.name.Contains("boots")?boots:r.name.Contains("cape")?cape:cloth;
                    r.sharedMaterials=Enumerable.Repeat(material,r.sharedMaterials.Length).ToArray();
                }
                a.Rebind();a.Play("Base Layer.Idle",0,0);a.Update(0);
                var bounds=VisibleBounds(root);
                // Source model remains untouched. Narrower x/z and shorter y are baked into this derived form.
                float desiredLocalHeight=1.85f/fallback.transform.localScale.y;
                float scale=desiredLocalHeight/bounds.size.y;root.transform.localScale=new Vector3(scale*.83f,scale,scale*.88f);
                bounds=VisibleBounds(root);height=bounds.size.y*fallback.transform.localScale.y;width=bounds.size.x*fallback.transform.localScale.x;
                var staff=new GameObject("Caster Staff");staff.transform.SetParent(root.transform,false);
                var art=UnityEngine.Object.Instantiate(staffSource,staff.transform);art.name="Modest metal staff";
                // This legacy prop's demo behaviours are not part of gameplay presentation.
                foreach(var t in art.GetComponentsInChildren<Transform>(true))GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                foreach(var behaviour in art.GetComponentsInChildren<MonoBehaviour>(true))UnityEngine.Object.DestroyImmediate(behaviour);
                foreach(var anim in art.GetComponentsInChildren<Animator>(true))UnityEngine.Object.DestroyImmediate(anim);
                foreach(var col in art.GetComponentsInChildren<Collider>(true))UnityEngine.Object.DestroyImmediate(col);
                foreach(var r in art.GetComponentsInChildren<Renderer>(true)){
                    r.sharedMaterials=r.sharedMaterials.Select(m=>{
                        string n=m!=null?m.name:"Staff";
                        var t=m!=null&&m.HasProperty("_MainTex")?m.GetTexture("_MainTex"):null;
                        return Mat("Staff "+n.Replace("/","_"),n.IndexOf("Gem",StringComparison.OrdinalIgnoreCase)>=0?new Color(.50f,.65f,1):new Color(.60f,.63f,.68f),t,.2f);
                    }).ToArray();
                }
                var sb=VisibleBounds(art);float staffHeight=1.5f/fallback.transform.localScale.y;
                art.transform.localScale*=staffHeight/sb.size.y;
                sb=VisibleBounds(art);var hand=a.GetBoneTransform(HumanBodyBones.RightHand);
                // Keep a vertical shaft in the authored idle pose, with its middle gripped by the hand.
                staff.transform.position=hand.position+Vector3.up*.18f/fallback.transform.localScale.y;
                art.transform.position+=staff.transform.position-VisibleBounds(art).center;
                staff.AddComponent<PlainGearAttachment>().Configure(hand);
                foreach(var col in root.GetComponentsInChildren<Collider>(true))UnityEngine.Object.DestroyImmediate(col);
                PrefabUtility.SaveAsPrefabAsset(root,Local+"/MagicalHero.prefab");
            }finally{UnityEngine.Object.DestroyImmediate(root);}
            var contents=PrefabUtility.LoadPrefabContents(MagicalTouchedAuthoring.MagicalPrefabPath);
            try{if(contents.GetComponent<MagicallyTouchedVisual>()==null)contents.AddComponent<MagicallyTouchedVisual>();PrefabUtility.SaveAsPrefabAsset(contents,MagicalTouchedAuthoring.MagicalPrefabPath);}
            finally{PrefabUtility.UnloadPrefabContents(contents);}
            try{
                foreach(var path in new[]{"Assets/_DiceFree/Scenes/Cornberg.unity","Assets/_DiceFree/Scenes/DevelopmentRoom.unity"}){
                    var scene=EditorSceneManager.OpenScene(path);
                    foreach(var switcher in UnityEngine.Object.FindObjectsByType<ClassPresentationSwitcher>(FindObjectsInactive.Include)){
                        var magical=switcher.Resolve(MagicalTouchedAuthoring.ClassId);if(magical==null)continue;
                        if(magical.GetComponent<MagicallyTouchedVisual>()==null)magical.gameObject.AddComponent<MagicallyTouchedVisual>();
                        var so=new SerializedObject(switcher);var bindings=so.FindProperty("bindings");
                        for(int i=0;i<bindings.arraySize;i++){var b=bindings.GetArrayElementAtIndex(i);if(b.FindPropertyRelative("classId").stringValue==MagicalTouchedAuthoring.ClassId)b.FindPropertyRelative("animator").objectReferenceValue=null;}
                        so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(switcher);
                    }
                    EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                }AssetDatabase.SaveAssets();
            }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            return new{success=true,source=Source,derived=Local+"/MagicalHero.prefab",bodyHeight=height,bodyWidth=width,marker="DICEFREE_MAGICAL_MODEL_AUTHORED"};
        }
    }
}
