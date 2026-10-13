using System;
using System.IO;
using System.Linq;
using DiceFree.Visuals;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

namespace DiceFree.EditorTools
{
    public static class GreatbladePrototypeAuthoring
    {
        private const string Female=
            "Assets/AllStarCharacterLibrary/Models/Characters/A03.FBX";
        private const string Sword=
            "Assets/PurePoly/Free_Swords/Prefabs/PP_Sword_0363.prefab";
        private const string Local="Assets/_DiceFree/Resources/GreatbladeLicensed";
        private const string Preview="Assets/_DiceFree/Scenes/GreatbladePrototype.unity";

        private static AnimationClip Clip(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(
                "Assets/Blink/Art/Animations/"+path+".fbx")
                .OfType<AnimationClip>()
                .FirstOrDefault(x=>!x.name.Equals("tpose",
                    StringComparison.OrdinalIgnoreCase)&&
                    !x.name.StartsWith("__preview"));
        }
        private static Transform Bone(Transform root,string name)
        {
            if(root.name==name)return root;
            foreach(Transform child in root)
            {
                var bone=Bone(child,name);
                if(bone!=null)return bone;
            }
            return null;
        }
        [CliCommand("dicefree.greatblade.prototype.install",
            "Prepare shared female Greatblade licensed model, 2H sword animations and portable player/NPC preview scene.")]
        public static object Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before authoring.");
            var importer=AssetImporter.GetAtPath(Female) as ModelImporter;
            if(importer==null)throw new InvalidOperationException(
                "Female Warrior Princess source model not imported locally");
            if(importer.animationType!=ModelImporterAnimationType.Human)
            {
                importer.animationType=ModelImporterAnimationType.Human;
                importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
                importer.SaveAndReimport();
            }
            var female=AssetDatabase.LoadAssetAtPath<GameObject>(Female);
            var avatar=AssetDatabase.LoadAssetAtPath<Avatar>(
                Local+"/GreatbladeFemaleAvatar.asset");
            if(avatar==null || !avatar.isHuman || !avatar.isValid)
                avatar=AssetDatabase.LoadAllAssetsAtPath(Female)
                    .OfType<Avatar>().FirstOrDefault(x=>x!=null&&x.isHuman&&x.isValid);
            if(female==null)throw new InvalidOperationException(
                "Female body source couldn't be imported");
            var sword=AssetDatabase.LoadAssetAtPath<GameObject>(Sword);
            if(sword==null)throw new InvalidOperationException(
                "Large sword asset couldn't be imported");
            Directory.CreateDirectory(Local);

            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(
                Local+"/GreatbladePrototype.controller");
            if(controller==null)
            {
                controller=AnimatorController.CreateAnimatorControllerAtPath(
                    Local+"/GreatbladePrototype.controller");
                controller.AddParameter("Speed",AnimatorControllerParameterType.Float);
                controller.AddParameter("Attack",AnimatorControllerParameterType.Trigger);
                controller.AddParameter("SpinDash",AnimatorControllerParameterType.Trigger);
                var machine=controller.layers[0].stateMachine;
                var idle=machine.AddState("Greatblade two-hand ready");
                idle.motion=Clip("Movement/Idle");
                var walk=machine.AddState("Greatblade walk");
                walk.motion=Clip("Movement/WalkForward");
                var strike=machine.AddState("Two-handed swing");
                strike.motion=Clip("Combat/MeeleeAttack_TwoHanded");
                var spin=machine.AddState("Spin Dash");
                spin.motion=Clip("Combat/SpinToWin_TwoHanded");
                machine.defaultState=idle;
                var move=idle.AddTransition(walk);
                move.hasExitTime=false;
                move.AddCondition(AnimatorConditionMode.Greater,.15f,"Speed");
                var stop=walk.AddTransition(idle);
                stop.hasExitTime=false;
                stop.AddCondition(AnimatorConditionMode.Less,.1f,"Speed");
                foreach(var state in new[]{idle,walk})
                {
                    var slash=state.AddTransition(strike);
                    slash.hasExitTime=false;
                    slash.AddCondition(AnimatorConditionMode.If,0,"Attack");
                    var dash=state.AddTransition(spin);
                    dash.hasExitTime=false;
                    dash.AddCondition(AnimatorConditionMode.If,0,"SpinDash");
                }
                foreach(var state in new[]{strike,spin})
                {
                    var back=state.AddTransition(idle);
                    back.hasExitTime=true;
                    back.exitTime=.92f;
                }
            }

            string prefabPath=Local+"/GreatbladeHero.prefab";
            if(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)==null)
            {
                var model=(GameObject)PrefabUtility.InstantiatePrefab(female);
                try
                {
                    model.name="Shared Greatblade character — player and swordswoman";
                    model.transform.position=Vector3.zero;
                    model.transform.rotation=Quaternion.identity;
                    var animator=model.GetComponent<Animator>();
                    if(animator==null)animator=model.AddComponent<Animator>();
                    if(avatar!=null)animator.avatar=avatar;
                    animator.runtimeAnimatorController=controller;
                    animator.applyRootMotion=false;

                    var hand=Bone(model.transform,"R Hand");
                    if(hand==null)throw new InvalidOperationException(
                        "Female model has no right-hand weapon socket");
                    var swordInstance=(GameObject)PrefabUtility.InstantiatePrefab(sword);
                    swordInstance.name="Oversized two-handed Greatblade sword";
                    swordInstance.transform.SetParent(hand,false);
                    swordInstance.transform.localPosition=new Vector3(
                        0,-.04f,0);
                    swordInstance.transform.localRotation=Quaternion.Euler(90,0,0);
                    var renderers=swordInstance.GetComponentsInChildren<Renderer>(true);
                    var bounds=new Bounds(Vector3.zero,Vector3.one);
                    bool found=false;
                    foreach(var mesh in renderers)
                    {
                        if(!found){bounds=mesh.bounds;found=true;}
                        else bounds.Encapsulate(mesh.bounds);
                    }
                    // Scale disproportionately large for the game's WC3
                    // hero silhouette, regardless of source prefab dimensions.
                    float largest=Mathf.Max(bounds.size.x,
                        Mathf.Max(bounds.size.y,bounds.size.z));
                    float factor=found?Mathf.Clamp(2.75f/
                        Mathf.Max(.01f,largest),.5f,7f):2.5f;
                    swordInstance.transform.localScale*=factor;
                    foreach(var col in model.GetComponentsInChildren<Collider>(true))
                        Object.DestroyImmediate(col);
                    PrefabUtility.SaveAsPrefabAsset(model,prefabPath);
                }
                finally{Object.DestroyImmediate(model);}
            }

            // The initial asset-only build may have been created before
            // manually mapping the female skeleton. Refresh controller+avatar
            // without duplicating or replacing the shared NPC/player model.
            var edited=PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var target=edited.GetComponent<Animator>();
                if(target==null)target=edited.AddComponent<Animator>();
                if(avatar!=null)target.avatar=avatar;
                target.runtimeAnimatorController=controller;
                target.applyRootMotion=false;
                PrefabUtility.SaveAsPrefabAsset(edited,prefabPath);
            }
            finally{PrefabUtility.UnloadPrefabContents(edited);}

            var previous=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var camera=new GameObject("Greatblade hero preview camera")
                    .AddComponent<Camera>();
                camera.transform.position=new Vector3(0,4,-11f);
                camera.transform.LookAt(new Vector3(0,1.5f,0));
                camera.fieldOfView=34f;
                camera.clearFlags=CameraClearFlags.SolidColor;
                camera.backgroundColor=new Color(.085f,.105f,.135f);
                var light=new GameObject("Greatblade preview sun").AddComponent<Light>();
                light.type=LightType.Directional;
                light.transform.rotation=Quaternion.Euler(40,-28,0);
                light.intensity=1.6f;
                var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name="Shared class portrait stage";
                floor.transform.localScale=new Vector3(1.1f,1,1.1f);
                var mat=AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/_DiceFree/Materials/Blockout/Meadow.mat");
                if(mat!=null)floor.GetComponent<Renderer>().sharedMaterial=mat;
                foreach(var actor in new[]{
                    (name:"Greatblade playable class prototype",x:-2f),
                    (name:"Retired swordswoman — SAME BASE",x:2f)})
                {
                    var obj=new GameObject(actor.name);
                    obj.transform.position=new Vector3(actor.x,0,0);
                    obj.AddComponent<GreatbladePrototypeVisual>();
                }
                if(!EditorSceneManager.SaveScene(scene,Preview))
                    throw new InvalidOperationException("Couldn't save prototype scene");
            }
            finally
            {
                if(previous.Length>0)
                    EditorSceneManager.RestoreSceneManagerSetup(previous);
            }
            AssetDatabase.SaveAssets();
            return new{success=true,
                scene=Preview,
                visualResource=GreatbladePrototypeVisual.ResourcePath,
                sharedBaseForNpc=true,
                avatarHumanoid=avatar!=null,
                animationStates=4,
                swordPath=Sword,
                licensedRawAssetsLocalOnly=true};
        }
    }
}
