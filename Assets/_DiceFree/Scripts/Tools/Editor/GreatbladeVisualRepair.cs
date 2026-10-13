using System;
using System.IO;
using System.Linq;
using DiceFree.Visuals;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace DiceFree.EditorTools
{
    /// <summary>Repairs derived local art; vendor models and materials stay intact.</summary>
    public static class GreatbladeVisualRepair
    {
        public const string Local="Assets/_DiceFree/Resources/GreatbladeLicensed";
        private const string Source="Assets/AllStarCharacterLibrary/Models/Characters/";
        public const string SwordName="Oversized two-handed Greatblade sword";
        public const string MainGrip="Right palm grip";
        public const string LowerGrip="Left palm grip";
        private static readonly Vector3 Palm=GreatbladeTwoHandGrip.HandPalm;
        // PP_Sword_0363 handle occupies raw mesh Y [-.343, -.140].
        // Its pivot is in the blade, so use measured leather grip anchors.
        private static readonly Vector3 RightGrip=new Vector3(-.00174f,-.185f,0);
        private static readonly Vector3 LeftGrip=new Vector3(-.00174f,-.295f,0);

        private static AnimationClip Clip(string path)=>
            AssetDatabase.LoadAllAssetsAtPath("Assets/Blink/Art/Animations/"+path+".fbx")
                .OfType<AnimationClip>().FirstOrDefault(x=>
                    !x.name.StartsWith("__preview") &&
                    !x.name.Equals("tpose",StringComparison.OrdinalIgnoreCase));

        private static void SaveDerived(Object generated,string path)
        {
            var existing=AssetDatabase.LoadAssetAtPath<Object>(path);
            if(existing==null)AssetDatabase.CreateAsset(generated,path);
            else
            {
                EditorUtility.CopySerialized(generated,existing);
                EditorUtility.SetDirty(existing);
                AssetDatabase.SaveAssetIfDirty(existing);
                Object.DestroyImmediate(generated);
            }
        }

        private static bool CarryCurve(EditorCurveBinding binding)
        {
            if(binding.type!=typeof(Animator) || binding.path!="")return false;
            string p=binding.propertyName;
            return new[]{"Left Arm","Right Arm","Left Forearm","Right Forearm",
                "Left Shoulder","Right Shoulder","Left Hand","Right Hand",
                "LeftHand.","RightHand."}.Any(p.StartsWith);
        }

        private static AnimationClip ReadyClip(AnimationClip swing)
        {
            var ready=Object.Instantiate(swing);
            ready.name="Greatblade armed ready";
            float poseTime=swing.length*.03f;
            foreach(var binding in AnimationUtility.GetCurveBindings(swing))
            {
                float value=AnimationUtility.GetEditorCurve(swing,binding).Evaluate(poseTime);
                AnimationUtility.SetEditorCurve(ready,binding,
                    AnimationCurve.Constant(0,1.5f,value));
            }
            var settings=AnimationUtility.GetAnimationClipSettings(ready);
            settings.startTime=0;settings.stopTime=1.5f;settings.loopTime=true;
            AnimationUtility.SetAnimationClipSettings(ready,settings);
            SaveDerived(ready,Local+"/GreatbladeReady.anim");
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(Local+"/GreatbladeReady.anim");
        }

        private static void AuthorGuard(GameObject prefab,
            AnimatorController controller,AnimationClip ready)
        {
            var scene=EditorSceneManager.NewPreviewScene();
            HumanPoseHandler handler=null;
            try
            {
                var model=Object.Instantiate(prefab);
                SceneManager.MoveGameObjectToScene(model,scene);
                var animator=model.GetComponent<Animator>();
                animator.avatar=AssetDatabase.LoadAssetAtPath<Avatar>(
                    GreatbladeHumanoidRig.AvatarPath);
                animator.runtimeAnimatorController=controller;
                animator.Rebind();animator.Play("Greatblade two-hand ready",0,0);
                animator.Update(0);
                var rh=animator.GetBoneTransform(HumanBodyBones.RightHand);
                var lh=animator.GetBoneTransform(HumanBodyBones.LeftHand);
                Vector3 initialAxis=(rh.TransformPoint(Palm)-lh.TransformPoint(Palm)).normalized;
                var initialSword=Quaternion.LookRotation(
                    Vector3.ProjectOnPlane(Vector3.forward,initialAxis).normalized,initialAxis);
                var swordInRight=Quaternion.Inverse(rh.rotation)*initialSword;
                var leftInSword=Quaternion.Inverse(initialSword)*lh.rotation;
                Vector3 axis=new Vector3(.42f,.89f,.17f).normalized;
                var aimed=Quaternion.LookRotation(
                    Vector3.ProjectOnPlane(Vector3.forward,axis).normalized,axis);
                Quaternion rightRotation=aimed*Quaternion.Inverse(swordInRight);
                Quaternion leftRotation=aimed*leftInSword;
                Vector3 rightPalm=new Vector3(.12f,1.21f,.30f);
                var sword=model.GetComponentsInChildren<Transform>(true).First(t=>t.name==SwordName);
                float spacing=(RightGrip.y-LeftGrip.y)*2.75f/
                    sword.GetComponent<MeshFilter>().sharedMesh.bounds.size.y;
                Vector3 leftPalm=rightPalm-axis*spacing;
                GreatbladeTwoHandGrip.SolveArm(
                    animator.GetBoneTransform(HumanBodyBones.RightUpperArm),
                    animator.GetBoneTransform(HumanBodyBones.RightLowerArm),rh,
                    rightPalm-rightRotation*Palm,rightRotation,Vector3.right);
                GreatbladeTwoHandGrip.SolveArm(
                    animator.GetBoneTransform(HumanBodyBones.LeftUpperArm),
                    animator.GetBoneTransform(HumanBodyBones.LeftLowerArm),lh,
                    leftPalm-leftRotation*Palm,leftRotation,Vector3.left);
                handler=new HumanPoseHandler(animator.avatar,model.transform);
                var pose=new HumanPose();
                handler.GetHumanPose(ref pose);
                var muscles=HumanTrait.MuscleName;
                foreach(var binding in AnimationUtility.GetCurveBindings(ready).Where(CarryCurve))
                {
                    int index=Array.IndexOf(muscles,binding.propertyName);
                    if(index>=0)
                        AnimationUtility.SetEditorCurve(ready,binding,
                            AnimationCurve.Constant(0,1.5f,pose.muscles[index]));
                }
                EditorUtility.SetDirty(ready);
                AssetDatabase.SaveAssetIfDirty(ready);
            }
            finally
            {
                if(handler!=null)handler.Dispose();
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static void CloseFingers(AnimationClip clip)
        {
            foreach(string side in new[]{"Left","Right"})
                foreach(string finger in new[]{"Thumb","Index","Middle","Ring","Little"})
                {
                    for(int joint=1;joint<=3;joint++)
                    {
                        var binding=EditorCurveBinding.FloatCurve("",typeof(Animator),
                            side+"Hand."+finger+"."+joint+" Stretched");
                        float curl=finger=="Thumb"?-.35f:(joint==2?-.65f:-.45f);
                        AnimationUtility.SetEditorCurve(clip,binding,
                            AnimationCurve.Constant(0,clip.length,curl));
                    }
                    AnimationUtility.SetEditorCurve(clip,
                        EditorCurveBinding.FloatCurve("",typeof(Animator),
                            side+"Hand."+finger+".Spread"),
                        AnimationCurve.Constant(0,clip.length,0));
                }
        }

        private static AnimationClip GrippedMotion(AnimationClip source,string name)
        {
            var motion=Object.Instantiate(source);
            motion.name=name;
            CloseFingers(motion);
            string path=Local+"/"+name+".anim";
            SaveDerived(motion,path);
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        }

        private static AnimationClip CarryWalk(AnimationClip walk,AnimationClip ready)
        {
            var carry=Object.Instantiate(walk);
            carry.name="Greatblade two-handed carry walk";
            foreach(var binding in AnimationUtility.GetCurveBindings(ready).Where(CarryCurve))
            {
                float value=AnimationUtility.GetEditorCurve(ready,binding).Evaluate(0);
                AnimationUtility.SetEditorCurve(carry,binding,
                    AnimationCurve.Constant(0,walk.length,value));
            }
            var settings=AnimationUtility.GetAnimationClipSettings(carry);
            settings.loopTime=true;
            AnimationUtility.SetAnimationClipSettings(carry,settings);
            SaveDerived(carry,Local+"/GreatbladeCarryWalk.anim");
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(Local+"/GreatbladeCarryWalk.anim");
        }

        private static Transform Anchor(Transform parent,string name,Vector3 point)
        {
            var anchor=parent.Find(name);
            if(anchor==null){anchor=new GameObject(name).transform;anchor.SetParent(parent,false);}
            anchor.localPosition=point;
            anchor.localRotation=Quaternion.identity;
            anchor.localScale=Vector3.one;
            return anchor;
        }

        [CliCommand("dicefree.greatblade.visuals.repair",
            "Restore Greatblade textures, T-pose retargeting, one body/weapon, and measured two-hand grips on local derived art.")]
        public static object Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before repairing art");
            string[] inputs={Source+"A03.png",Source+"A03N.png"};
            foreach(string input in inputs)
            {
                if(!File.Exists(input))throw new InvalidOperationException(
                    "Restore the missing input from your licensed Female Warrior Princess pack: "+input);
                AssetDatabase.ImportAsset(input,ImportAssetOptions.ForceSynchronousImport);
            }
            var swing=Clip("Combat/MeeleeAttack_TwoHanded");
            var walk=Clip("Movement/WalkForward");
            var spin=Clip("Combat/SpinToWin_TwoHanded");
            if(swing==null || walk==null || spin==null || !swing.isHumanMotion || !walk.isHumanMotion || !spin.isHumanMotion)
                throw new InvalidOperationException("Imported Humanoid swing/walk clips required");
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(Local+"/GreatbladePrototype.controller");
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Local+"/GreatbladeHero.prefab");
            if(controller==null || prefab==null)
                throw new InvalidOperationException("Run prototype install before visual repair");
            GreatbladeHumanoidRig.Install();
            var ready=ReadyClip(swing);
            var machine=controller.layers[0].stateMachine;
            machine.states.First(x=>x.state.name=="Greatblade two-hand ready").state.motion=ready;
            AuthorGuard(prefab,controller,ready);
            CloseFingers(ready);
            EditorUtility.SetDirty(ready);
            AssetDatabase.SaveAssetIfDirty(ready);
            var carry=CarryWalk(walk,ready);
            machine.states.First(x=>x.state.name=="Two-handed swing").state.motion=
                GrippedMotion(swing,"GreatbladeSwing");
            machine.states.First(x=>x.state.name=="Spin Dash").state.motion=
                GrippedMotion(spin,"GreatbladeSpin");
            machine.states.First(x=>x.state.name=="Greatblade walk").state.motion=carry;
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssetIfDirty(controller);

            var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.name="Greatblade textured body";
            material.SetColor("_BaseColor",Color.white);
            material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(inputs[0]));
            material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(inputs[1]));
            material.SetFloat("_BumpScale",.8f);
            material.EnableKeyword("_NORMALMAP");
            material.SetFloat("_Metallic",.08f);
            material.SetFloat("_Smoothness",.25f);
            SaveDerived(material,Local+"/GreatbladeBody.mat");
            material=AssetDatabase.LoadAssetAtPath<Material>(Local+"/GreatbladeBody.mat");

            var model=PrefabUtility.LoadPrefabContents(Local+"/GreatbladeHero.prefab");
            try
            {
                var animator=model.GetComponent<Animator>();
                animator.avatar=AssetDatabase.LoadAssetAtPath<Avatar>(GreatbladeHumanoidRig.AvatarPath);
                animator.runtimeAnimatorController=controller;
                animator.applyRootMotion=false;
                animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                foreach(var sourceWeapon in model.GetComponentsInChildren<Renderer>(true)
                    .Where(r=>r.name=="Longsword"))
                    sourceWeapon.enabled=false;
                foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    skin.enabled=skin.name!="A03_LOD" && skin.name!="Longsword";
                    if(skin.enabled)
                        skin.sharedMaterials=Enumerable.Repeat(material,skin.sharedMaterials.Length).ToArray();
                    skin.updateWhenOffscreen=true;
                }
                animator.Rebind();
                animator.Play("Greatblade two-hand ready",0,0);
                animator.Update(0);
                var right=animator.GetBoneTransform(HumanBodyBones.RightHand);
                var left=animator.GetBoneTransform(HumanBodyBones.LeftHand);
                var sword=model.GetComponentsInChildren<Transform>(true).First(t=>t.name==SwordName);
                sword.SetParent(right,false);
                var mesh=sword.GetComponent<MeshFilter>();
                if(mesh==null || mesh.sharedMesh==null ||
                    mesh.sharedMesh.bounds.size.y<=mesh.sharedMesh.bounds.size.x)
                    throw new InvalidOperationException("Sword geometry changed; remeasure its handle first");
                float scale=2.75f/mesh.sharedMesh.bounds.size.y;
                // Keep the grip girth near the source size while exaggerating length.
                sword.localScale=new Vector3(1f,scale,1f);
                Vector3 rp=right.TransformPoint(Palm),lp=left.TransformPoint(Palm);
                Vector3 blade=(rp-lp).normalized;
                if((rp-lp).sqrMagnitude<.001f)
                    throw new InvalidOperationException("Ready animation has no distinct two-hand grips");
                Vector3 normal=Vector3.ProjectOnPlane(model.transform.forward,blade).normalized;
                if(normal.sqrMagnitude<.01f)
                    normal=Vector3.ProjectOnPlane(model.transform.right,blade).normalized;
                sword.rotation=Quaternion.LookRotation(normal,blade);
                sword.localPosition=Palm-sword.localRotation*Vector3.Scale(RightGrip,sword.localScale);
                Anchor(sword,MainGrip,RightGrip);
                var support=Anchor(sword,LowerGrip,LeftGrip);
                Anchor(sword,"Blade tip",new Vector3(0,mesh.sharedMesh.bounds.max.y,0));
                var grip=model.GetComponent<GreatbladeTwoHandGrip>();
                if(grip==null)grip=model.AddComponent<GreatbladeTwoHandGrip>();
                grip.Configure(sword,sword.Find(MainGrip),support,Quaternion.Inverse(sword.rotation)*left.rotation);
                grip.ApplyPose();
                PrefabUtility.SaveAsPrefabAsset(model,Local+"/GreatbladeHero.prefab");
                return new{success=true,textured=true,oneBody=true,oneSword=true,
                    bladeLength=2.75f,gripSpacing=(RightGrip.y-LeftGrip.y)*scale,
                    mappedFingers=30,raisedGuard=true,carryUpperBody=true,
                    supportHandError=Vector3.Distance(grip.SupportPalm,support.position),
                    vendorSourceUnchanged=true};
            }
            finally{PrefabUtility.UnloadPrefabContents(model);}
        }
    }
}
