using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Ironpine;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DiceFree.EditorTools
{
    internal static class IronpineCreatureArtAuthoring
    {
        private const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string Art = "Assets/_DiceFree/Art/Creatures/Ironpine";
        private const string AssetRoot = "Assets/_DiceFree/Settings/Ironpine";
        private const string Group = "Ironpine wildlife v1.4";
        private static readonly string[] WolfStates =
            {"Idle","Walk","Run","Bite","Howl","Pounce","Sniff","Scratch","Hurt","Death"};
        private static readonly string[] RatStates =
            {"Idle","Walk","Run","Bite","QuillShot","Scurry","Forage","Scratch","Hurt","Death"};

        private static void Check(bool ok, string why)
        {
            if (!ok) throw new InvalidOperationException("IRONPINE CREATURE ART: " + why);
        }
        private static T[] All<T>(Scene s) where T: Component =>
            s.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<T>(true)).ToArray();

        private static string Species(WildlifeKind kind) =>
            kind == WildlifeKind.Quillrat ? "Quillrat" :
            kind == WildlifeKind.AlphaWolf ? "AlphaWolf" : "Wolf";

        private static GameObject ImportModel(string species)
        {
            string file = Art + "/" + species + ".fbx";
            var importer = AssetImporter.GetAtPath(file) as ModelImporter;
            Check(importer != null, "Missing original Blender FBX " + file);
            if (importer.animationType != ModelImporterAnimationType.Generic ||
                !importer.importAnimation || importer.materialImportMode != ModelImporterMaterialImportMode.ImportStandard)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.importAnimation = true;
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                importer.SaveAndReimport();
            }
            // Imported FBX action clips are non-looping by default in Unity.
            // Author locomotion/idle loop metadata at the ModelImporter boundary,
            // not on transient AnimationClip subassets.
            var authoredClips = importer.clipAnimations;
            bool hadNoOverrides = authoredClips == null || authoredClips.Length == 0;
            if (hadNoOverrides) authoredClips = importer.defaultClipAnimations;
            bool changedClips = hadNoOverrides;
            foreach (var authoredClip in authoredClips)
            {
                bool repeat = authoredClip.name.EndsWith("|Idle", StringComparison.OrdinalIgnoreCase) ||
                    authoredClip.name.EndsWith("|Walk", StringComparison.OrdinalIgnoreCase) ||
                    authoredClip.name.EndsWith("|Run", StringComparison.OrdinalIgnoreCase);
                if (repeat && !authoredClip.loopTime)
                {
                    authoredClip.loopTime = true;
                    changedClips = true;
                }
            }
            if (changedClips)
            {
                importer.clipAnimations = authoredClips;
                importer.SaveAndReimport();
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(file);
            Check(prefab != null, "Unity cannot load " + file);
            return prefab;
        }
        private static AnimatorController Controller(string species)
        {
            var path = AssetRoot + "/" + species + "_Animator.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller != null) return controller;
            var expected = species == "Quillrat" ? RatStates : WolfStates;
            var pathFbx = Art + "/" + species + ".fbx";
            var all = AssetDatabase.LoadAllAssetsAtPath(pathFbx)
                .OfType<AnimationClip>().Where(x => !x.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            Debug.Log("IRONPINE_ART_CLIPS " + species + ": " + string.Join(", ", all.Select(c=>c.name)));
            controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            var machine = controller.layers[0].stateMachine;
            foreach (var name in expected)
            {
                var clip = all.FirstOrDefault(x => x.name.Equals(species+"|"+name, StringComparison.OrdinalIgnoreCase))
                    ?? all.FirstOrDefault(x => x.name.EndsWith("|"+name, StringComparison.OrdinalIgnoreCase))
                    ?? all.FirstOrDefault(x => x.name.EndsWith("_"+name, StringComparison.OrdinalIgnoreCase))
                    ?? all.FirstOrDefault(x => x.name.Equals(name,StringComparison.OrdinalIgnoreCase));
                Check(clip != null, "Missing " + species + " / " + name + " animation in FBX");
                var state = machine.AddState(name);
                state.motion = clip;
                if (name == "Idle") machine.defaultState = state;
                if (name is "Idle" or "Walk" or "Run")
                {
                    var settings = AnimationUtility.GetAnimationClipSettings(clip);
                    settings.loopTime = true;
                    AnimationUtility.SetAnimationClipSettings(clip, settings);
                }
            }
            EditorUtility.SetDirty(controller);
            return controller;
        }
        private static Material ColorMaterial(string key, Color color)
        {
            string path = AssetRoot + "/Art_" + key + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if(existing != null) return existing;
            var shader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Check(shader!=null,"URP shader missing");
            var mat = new Material(shader){name="Ironpine "+key};
            if(mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor",color);
            mat.color=color;
            mat.SetFloat("_Smoothness",.11f);
            AssetDatabase.CreateAsset(mat,path);
            return mat;
        }
        private static Color ColorFor(string material, WildlifeKind kind, int variant)
        {
            string lower=material.ToLowerInvariant();
            if(kind==WildlifeKind.Quillrat)
            {
                // Moss / Amber / Slate, each has independent body, cheek and quill color.
                Color body=variant==0?new Color(.37f,.55f,.24f):
                    variant==1?new Color(.67f,.39f,.22f):
                    new Color(.36f,.47f,.62f);
                if(lower.Contains("quillband")) return variant==2?new Color(.22f,.29f,.39f):new Color(.34f,.26f,.16f);
                if(lower.Contains("goldquill")) return variant==0?new Color(.86f,.82f,.41f):
                    variant==1?new Color(.95f,.72f,.32f):new Color(.68f,.78f,.87f);
                if(lower.Contains("ratbelly")) return variant==0?new Color(.73f,.80f,.51f):
                    variant==1?new Color(.84f,.67f,.46f):new Color(.59f,.68f,.76f);
                if(lower.Contains("ratshadow")) return body*.67f;
                if(lower.Contains("ratface")) return Color.Lerp(body, Color.white,.20f);
                if(lower.Contains("rateye")) return new Color(.94f,.82f,.30f);
                if(lower.Contains("ratear")) return new Color(.67f,.35f,.33f);
                if(lower.Contains("ratnose")) return new Color(.25f,.19f,.18f);
                return body;
            }
            Color fur=kind==WildlifeKind.GreyWolf?new Color(.47f,.49f,.51f):
                kind==WildlifeKind.DarkWolf?new Color(.21f,.24f,.29f):
                new Color(.16f,.18f,.21f);
            if(lower.Contains("alphamane")) return new Color(.76f,.73f,.62f);
            if(lower.Contains("alphascar")) return new Color(.94f,.87f,.68f);
            if(lower.Contains("noseandpupil")) return new Color(.12f,.13f,.15f);
            if(lower.Contains("goldeneye")) return kind==WildlifeKind.AlphaWolf?
                new Color(.96f,.49f,.18f):new Color(.92f,.73f,.29f);
            if(lower.Contains("innerear")) return new Color(.54f,.32f,.32f);
            if(lower.Contains("wolfchin")) return kind==WildlifeKind.AlphaWolf?
                new Color(.66f,.66f,.62f):new Color(.76f,.74f,.67f);
            if(lower.Contains("wolfshadow")) return fur*.66f;
            if(lower.Contains("wolfface")) return Color.Lerp(fur,Color.white,.16f);
            return fur;
        }
        private static void ApplyPalette(Transform model, WildlifeKind kind, int variant)
        {
            foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var materials=renderer.sharedMaterials;

                for(int i=0;i<materials.Length;i++)
                {
                    var original=materials[i];
                    string name=original!=null ? original.name : "WolfCoat";
                    string key=kind+"_"+variant+"_"+name.Replace(' ','_');
                    materials[i]=ColorMaterial(key,ColorFor(name,kind,variant));
                }
                renderer.sharedMaterials=materials;
                renderer.updateWhenOffscreen = true;
            }
        }
        [CliCommand("dicefree.ironpine-art.install","Replace 22 Ironpine blockout primitives with fully rigged original Blender creature models and clips.")]
        public static object Install()
        {
            Check(!EditorApplication.isPlayingOrWillChangePlaymode,"Stop play mode");
            var previous=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var animals=All<IronpineWildlife>(scene);
                Check(animals.Length==22,"Expected 22 wildlife, found "+animals.Length);
                var prefabs=new Dictionary<string,GameObject>();
                var controllers=new Dictionary<string,AnimatorController>();
                foreach (var species in new[]{"Wolf","AlphaWolf","Quillrat"})
                {
                    prefabs[species]=ImportModel(species);
                    controllers[species]=Controller(species);
                }
                int upgraded=0, skinned=0;
                foreach (var animal in animals)
                {
                    var kind=animal.Kind;
                    if(animal.GetComponent<IronpineCreatureAudio>()==null)
                        animal.gameObject.AddComponent<IronpineCreatureAudio>();
                    var species=Species(kind);
                    var actor=animal.transform;
                    var serialized=new SerializedObject(animal);
                    var visual=serialized.FindProperty("model");
                    var old=visual.objectReferenceValue as Transform;
                    Check(old!=null,"Missing existing model on "+animal.name);
                    if (old.name.StartsWith("Ironpine rigged ", StringComparison.Ordinal))
                    { upgraded++; continue; }

                    var model=(GameObject)PrefabUtility.InstantiatePrefab(prefabs[species],scene);
                    Check(model!=null,"Unable to instantiate "+species);
                    model.transform.SetParent(actor,false);
                    model.name="Ironpine rigged "+species;
                    model.transform.localPosition=Vector3.zero;
                    model.transform.localRotation=Quaternion.identity;
                    model.transform.localScale=Vector3.one*(kind==WildlifeKind.AlphaWolf?1.48f:
                        kind==WildlifeKind.DarkWolf?1.10f:1f);
                    int variant=(int)(upgraded%3);
                    ApplyPalette(model.transform,kind,variant);
                    var renderer=model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    Check(renderer.Length>=3,"Model missing skin/bones "+species);
                    skinned+=renderer.Length;
                    var animator=model.GetComponent<Animator>();
                    if (animator == null) animator=model.AddComponent<Animator>();
                    Check(animator != null,"Animator component creation failed");
                    animator.runtimeAnimatorController=controllers[species];
                    animator.applyRootMotion=false;
                    animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                    var presenter=animal.GetComponent<IronpineCreaturePresentation>() ??
                        animal.gameObject.AddComponent<IronpineCreaturePresentation>();
                    presenter.Configure(animator);
                    visual.objectReferenceValue=model.transform;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    Object.DestroyImmediate(old.gameObject);
                    upgraded++;
                }
                EditorSceneManager.MarkSceneDirty(scene);
                Check(EditorSceneManager.SaveScene(scene),"Couldn't save Cornberg art");
                AssetDatabase.SaveAssets();
                return new{success=true,models=3,animals=upgraded,skinnedMeshes=skinned};
            }
            finally {if(previous.Length>0)EditorSceneManager.RestoreSceneManagerSetup(previous);}
        }

        [CliCommand("dicefree.ironpine-art.validate","Verify all 22 wildlife have proper rigged/skinned Unity model, animator and authored animation states.")]
        public static object Validate()
        {
            var previous=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var creatures=All<IronpineWildlife>(scene);
                Check(creatures.Length==22,"22 creature count");
                int count=0;
                foreach(var animal in creatures)
                {
                    var presenter=animal.GetComponent<IronpineCreaturePresentation>();
                    Check(presenter!=null && presenter.ModelAnimator!=null,"No presenter on "+animal.name);
                    var model=presenter.ModelAnimator;
                    Check(model.runtimeAnimatorController!=null,"Missing clips on "+animal.name);
                    Check(model.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length>=3,
                        "Rig lacks skinned meshes "+animal.name);
                    var controller=model.runtimeAnimatorController as AnimatorController;
                    Check(controller!=null && controller.layers[0].stateMachine.states.Length>=10,
                        "Missing 10 authored clips "+animal.name);
                    foreach(var pair in controller.layers[0].stateMachine.states)
                    {
                        if (pair.state.name == "Idle" || pair.state.name == "Walk" || pair.state.name == "Run")
                        {
                            var clip=pair.state.motion as AnimationClip;
                            Check(clip != null && clip.isLooping, "Animation doesn't loop "+pair.state.name+" on "+animal.name);
                        }
                    }
                    count++;
                }
                return new{success=true,rigged=count,clipsPerSpecies=10};
            }
            finally {if(previous.Length>0)EditorSceneManager.RestoreSceneManagerSetup(previous);}
        }
    }
}
