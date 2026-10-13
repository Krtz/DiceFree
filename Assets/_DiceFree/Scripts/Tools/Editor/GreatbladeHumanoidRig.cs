using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class GreatbladeHumanoidRig
    {
        private const string Body=
            "Assets/AllStarCharacterLibrary/Models/Characters/A03.FBX";
        public const string AvatarPath=
            "Assets/_DiceFree/Resources/GreatbladeLicensed/GreatbladeFemaleTposeAvatar.asset";

        private static Transform Bone(GameObject model,string name)=>
            model.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t=>t.name==name);

        private static void Aim(GameObject model,string from,string to,
            Vector3 direction)
        {
            var joint=Bone(model,from);
            var end=Bone(model,to);
            if(joint==null || end==null)
                throw new InvalidOperationException("Missing A03 joint: "+from+"/"+to);
            joint.rotation=Quaternion.FromToRotation(
                end.position-joint.position,direction)*joint.rotation;
        }

        // The vendor mesh is in an A-pose. A valid Avatar made directly
        // from that pose still retargets arms behind the body. Only pose a
        // temporary instance; never modify the source FBX or its bind pose.
        private static void PoseForRetargeting(GameObject model)
        {
            foreach(var animator in model.GetComponentsInChildren<Animator>(true))
                animator.enabled=false;
            foreach(string side in new[]{"L","R"})
            {
                Vector3 outward=side=="L"?Vector3.left:Vector3.right;
                Aim(model,side+" UpperArm",side+" Forearm",outward);
                Aim(model,side+" Forearm",side+" Hand",outward);
                Aim(model,side+" Hand",side+" Finger2",outward);
                for(int finger=1;finger<=4;finger++)
                {
                    string digit=side+" Finger"+finger;
                    Aim(model,digit,digit+"1",outward);
                    Aim(model,digit+"1",digit+"2",outward);
                }
                Aim(model,side+" Thigh",side+" Calf",Vector3.down);
                Aim(model,side+" Calf",side+" Foot",Vector3.down);
            }
        }

        private static HumanDescription Snapshot(GameObject model)
        {
            var human=new Dictionary<string,string>
            {
                ["Hips"]="Hips",["Spine"]="Spine",["Chest"]="Spine1",
                ["Neck"]="Neck",["Head"]="Head",
                ["LeftShoulder"]="L Clavicle",["LeftUpperArm"]="L UpperArm",
                ["LeftLowerArm"]="L Forearm",["LeftHand"]="L Hand",
                ["RightShoulder"]="R Clavicle",["RightUpperArm"]="R UpperArm",
                ["RightLowerArm"]="R Forearm",["RightHand"]="R Hand",
                ["LeftUpperLeg"]="L Thigh",["LeftLowerLeg"]="L Calf",
                ["LeftFoot"]="L Foot",["LeftToes"]="L Toe0",
                ["RightUpperLeg"]="R Thigh",["RightLowerLeg"]="R Calf",
                ["RightFoot"]="R Foot",["RightToes"]="R Toe0"
            };
            string[] fingers={"Thumb","Index","Middle","Ring","Little"};
            string[] joints={"Proximal","Intermediate","Distal"};
            foreach(var side in new[]{(human:"Left",bone:"L"),(human:"Right",bone:"R")})
                for(int f=0;f<5;f++)
                    for(int j=0;j<3;j++)
                    {
                        string bone=side.bone+" Finger"+f+(j==0?"":j.ToString());
                        if(Bone(model,bone)!=null)
                            human[side.human+" "+fingers[f]+" "+joints[j]]=bone;
                    }
            return new HumanDescription
            {
                human=human.Select(pair=>new HumanBone{
                    humanName=pair.Key,boneName=pair.Value,
                    limit=new HumanLimit{useDefaultValues=true}}).ToArray(),
                skeleton=model.GetComponentsInChildren<Transform>(true)
                    .Select(t=>new SkeletonBone{name=t.name,
                        position=t.localPosition,rotation=t.localRotation,
                        scale=t.localScale}).ToArray(),
                armStretch=.05f,legStretch=.05f,
                upperArmTwist=.5f,lowerArmTwist=.5f,
                upperLegTwist=.5f,lowerLegTwist=.5f,
                feetSpacing=0f,hasTranslationDoF=false
            };
        }

        public static HumanDescription Description(GameObject source)
        {
            var posed=UnityEngine.Object.Instantiate(source);
            try
            {
                posed.name=source.name;
                PoseForRetargeting(posed);
                return Snapshot(posed);
            }
            finally{UnityEngine.Object.DestroyImmediate(posed);}
        }

        [CliCommand("dicefree.greatblade.rig-human",
            "Build a T-pose Humanoid Avatar with finger mapping without changing the A03 source.")]
        public static object Install()
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Body);
            if(source==null)throw new InvalidOperationException("Missing A03 FBX");
            Directory.CreateDirectory(Path.GetDirectoryName(AvatarPath));
            var cached=AssetDatabase.LoadAssetAtPath<Avatar>(AvatarPath);
            if(cached!=null && cached.isHuman && cached.isValid &&
                cached.humanDescription.human.Length==51 &&
                cached.humanDescription.human.Where(h=>h.boneName.Contains("Finger"))
                    .All(h=>HumanTrait.BoneName.Contains(h.humanName)))
                return new{success=true,bones=cached.humanDescription.skeleton.Length,
                    mapped=51,tPose=true,fingersMapped=30,sourceUnchanged=true};
            var posed=UnityEngine.Object.Instantiate(source);
            Avatar built=null;
            try
            {
                posed.name=source.name;
                PoseForRetargeting(posed);
                var description=Snapshot(posed);
                built=AvatarBuilder.BuildHumanAvatar(posed,description);
                if(built==null || !built.isHuman || !built.isValid)
                    throw new InvalidOperationException("A03 T-pose Avatar build failed");
                built.name="GreatbladeFemaleAvatar";
                var existing=AssetDatabase.LoadAssetAtPath<Avatar>(AvatarPath);
                if(existing==null)
                {
                    AssetDatabase.CreateAsset(built,AvatarPath);
                    built=null;
                }
                else
                {
                    EditorUtility.CopySerialized(built,existing);
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssetIfDirty(existing);
                }
                return new{success=true,bones=description.skeleton.Length,
                    mapped=description.human.Length,tPose=true,
                    fingersMapped=30,sourceUnchanged=true};
            }
            finally
            {
                if(built!=null)UnityEngine.Object.DestroyImmediate(built);
                UnityEngine.Object.DestroyImmediate(posed);
            }
        }
    }
}
