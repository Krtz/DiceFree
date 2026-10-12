using System;
using System.Linq;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DiceFree.EditorTools
{
    /// <summary>
    /// Creates optional local-only art resources from the user's licensed wolf pack.
    /// Assets/Wolf and Resources/IronpineLicensedWolf are both gitignored;
    /// no original third-party model, texture or animation is redistributed.
    /// </summary>
    public static class IronpineLicensedWolfInstaller
    {
        private const string Source="Assets/Wolf/URP/Wolf/Prefab/Wolf_URP.prefab";
        private const string Fbx="Assets/Wolf/Models/Wolf.fbx";
        private const string Root="Assets/_DiceFree/Resources/IronpineLicensedWolf";
        private static readonly (string state,string clip)[] Clips={
            ("Idle","Idle_New"),("Walk","Walking"),("Run","Running_New"),
            ("Bite","AttackR"),("Howl","WolfHowl_New"),
            ("Pounce","Jump_New"),("Sniff","sitingIdle._New"),
            ("Scratch","sits down._New"),("Hurt","GetHit_2"),("Death","dead")
        };
        private static void Require(bool valid,string error)
        {
            if(!valid)throw new InvalidOperationException("LICENSED WOLF: "+error);
        }
        private static Material Material(string name,Texture2D texture,bool alphaClip,Color color)
        {
            string path=Root+"/"+name+".mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null)
            {
                var shader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                Require(shader!=null,"Lit/Standard shader unavailable");
                mat=new Material(shader){name=name};
                AssetDatabase.CreateAsset(mat,path);
            }
            var activeShader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if(activeShader!=null && mat.shader!=activeShader)mat.shader=activeShader;
            mat.SetColor("_BaseColor",color);
            mat.SetTexture("_BaseMap",texture);
            if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",texture);
            mat.SetFloat("_Smoothness",.17f);
            if(alphaClip)
            {
                mat.SetFloat("_AlphaClip",1);
                mat.SetFloat("_Cutoff",.22f);
                mat.EnableKeyword("_ALPHATEST_ON");
                mat.renderQueue=2450;
                mat.SetOverrideTag("RenderType","TransparentCutout");
                mat.doubleSidedGI=true;
                if(mat.HasProperty("_Cull"))mat.SetFloat("_Cull",0);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }
        private static void EnsureLoopingClips()
        {
            var model=AssetImporter.GetAtPath(Fbx) as ModelImporter;
            Require(model!=null,"missing vendor FBX importer");
            var clips=model.clipAnimations;
            if(clips==null || clips.Length==0)clips=model.defaultClipAnimations;
            bool changed=false;
            foreach(var clip in clips)
            {
                bool shouldLoop=Clips.Any(pair=>
                    (pair.state=="Idle"||pair.state=="Walk"||pair.state=="Run") &&
                    clip.name.StartsWith(pair.clip,StringComparison.OrdinalIgnoreCase));
                if(shouldLoop && !clip.loopTime){clip.loopTime=true;changed=true;}
            }
            if(changed)
            {
                model.clipAnimations=clips;
                model.SaveAndReimport();
            }
        }
        [MenuItem("DiceFree/Ironpine/Refresh local realistic wolves")]
        public static void RefreshMenu()=>Install();
        [CliCommand("dicefree.ironpine.realistic-wolf.install",
            "Install the locally licensed Realistic Furry Wolf FREE model and its own animations for Ironpine.")]
        public static object Install()
        {
            Require(AssetDatabase.LoadAssetAtPath<GameObject>(Source)!=null,
                "Wolf_URP.prefab missing; unpack the user's purchased/free Asset Store pack locally");
            if(!AssetDatabase.IsValidFolder("Assets/_DiceFree/Resources"))
                AssetDatabase.CreateFolder("Assets/_DiceFree","Resources");
            if(!AssetDatabase.IsValidFolder(Root))
                AssetDatabase.CreateFolder("Assets/_DiceFree/Resources","IronpineLicensedWolf");
            EnsureLoopingClips();
            var all=AssetDatabase.LoadAllAssetsAtPath(Fbx).OfType<AnimationClip>()
                .Where(c=>!c.name.StartsWith("__preview__",StringComparison.OrdinalIgnoreCase))
                .ToArray();
            string controllerPath=Root+"/RealisticWolf.controller";
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if(controller==null)
                controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var states=controller.layers[0].stateMachine;
            foreach(var pair in Clips)
            {
                var clip=all.FirstOrDefault(c=>c.name.Equals(pair.clip,StringComparison.OrdinalIgnoreCase))
                    ??all.FirstOrDefault(c=>c.name.StartsWith(pair.clip+" (",StringComparison.OrdinalIgnoreCase));
                Require(clip!=null,"missing actual wolf pack clip "+pair.clip);
                var state=states.states.FirstOrDefault(s=>s.state.name==pair.state).state
                    ??states.AddState(pair.state);
                state.motion=clip;
                if(pair.state=="Howl")state.speed=Mathf.Max(1,clip.length/1.25f);
                if(pair.state=="Bite")state.speed=Mathf.Max(1,clip.length/.48f);
                if(pair.state=="Idle")states.defaultState=state;
            }
            EditorUtility.SetDirty(controller);
            string path=Root+"/RealisticWolf.prefab";
            if(AssetDatabase.LoadAssetAtPath<GameObject>(path)==null)
                Require(AssetDatabase.CopyAsset(Source,path),"failed to copy licensed URP wolf prefab locally");
            var wolf=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var texFolder="Assets/Wolf/Textures/Wolf_Textures/";
                var coatTex=AssetDatabase.LoadAssetAtPath<Texture2D>(texFolder+"wolf_AlbedoAlpha.png");
                var hairTex=AssetDatabase.LoadAssetAtPath<Texture2D>(texFolder+"WolfFiber_AlphaAlbedo.png");
                var furTex=AssetDatabase.LoadAssetAtPath<Texture2D>(texFolder+"WolfFur2_AlbedoAlpha.png");
                Require(coatTex!=null && hairTex!=null && furTex!=null,"wolf pack albedo/fur textures missing");
                var coat=Material("FurryWolfCoat",coatTex,false,Color.white);
                var hair=Material("FurryWolfHair",hairTex,true,Color.white);
                var shortFur=Material("FurryWolfShortFur",furTex,true,Color.white);
                var eyes=Material("FurryWolfEyes",coatTex,false,new Color(.94f,.92f,.85f));
                foreach(var renderer in wolf.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if(renderer.name.Contains("LongFur",StringComparison.OrdinalIgnoreCase))
                        renderer.sharedMaterials=new[]{hair};
                    else if(renderer.name.Contains("ShortFur",StringComparison.OrdinalIgnoreCase))
                        renderer.sharedMaterials=new[]{shortFur};
                    else if(renderer.name.Contains("wolf_low",StringComparison.OrdinalIgnoreCase))
                        renderer.sharedMaterials=new[]{coat,eyes};
                    else renderer.sharedMaterial=coat;
                }
                var animator=wolf.GetComponentInChildren<Animator>(true);
                Require(animator!=null,"vendor wolf lacks rigged Animator");
                animator.runtimeAnimatorController=controller;
                animator.applyRootMotion=false;
                animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                Require(PrefabUtility.SaveAsPrefabAsset(wolf,path)!=null,"failed to save local wolf");
            }
            finally{PrefabUtility.UnloadPrefabContents(wolf);}
            AssetDatabase.SaveAssets();
            Debug.Log("IRONPINE_LICENSED_WOLF_INSTALLED 10 original source animations, URP textured coat/fur and optional fallback");
            return new {success=true,animatedStates=Clips.Length,prefab=path};
        }
        [CliCommand("dicefree.ironpine.realistic-wolf.validate",
            "Verify optional locally licensed wolf visuals have animated states, fur and valid URP materials.")]
        public static object Validate()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/RealisticWolf.prefab");
            Require(prefab!=null,"generated wolf prefab missing");
            var animator=prefab.GetComponentInChildren<Animator>(true);
            Require(animator!=null && animator.runtimeAnimatorController!=null,"wolf controller missing");
            var controller=animator.runtimeAnimatorController as AnimatorController;
            Require(controller!=null && controller.layers[0].stateMachine.states.Length==Clips.Length,
                "missing source wolf animations");
            var skins=prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Require(skins.Length>=3,"wolf skin/LOD missing");
            Require(skins.All(s=>s.sharedMaterials.All(m=>m!=null &&
                m.shader!=null && m.shader.name!="Hidden/InternalErrorShader")),
                "invalid URP fur/coat shader");
            Require(controller.layers[0].stateMachine.states.Where(s=>
                 s.state.name is "Idle" or "Walk" or "Run")
                 .All(s=>(s.state.motion as AnimationClip)?.isLooping==true),
                 "vendor locomotion clips must loop");
            return new {success=true,skins=skins.Length,animationStates=Clips.Length,source="locally licensed"};
        }
    }
}
