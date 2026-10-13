using System;
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
        private const string Local=
            "Assets/_DiceFree/Resources/GreatbladeLicensed/GreatbladeFemaleAvatar.asset";

        public static HumanDescription Description(GameObject model)
        {
            var human=new Dictionary<string,string>
            {
                ["Hips"]="Hips",
                ["Spine"]="Spine",
                ["Chest"]="Spine1",
                ["Neck"]="Neck",
                ["Head"]="Head",
                ["LeftShoulder"]="L Clavicle",
                ["LeftUpperArm"]="L UpperArm",
                ["LeftLowerArm"]="L Forearm",
                ["LeftHand"]="L Hand",
                ["RightShoulder"]="R Clavicle",
                ["RightUpperArm"]="R UpperArm",
                ["RightLowerArm"]="R Forearm",
                ["RightHand"]="R Hand",
                ["LeftUpperLeg"]="L Thigh",
                ["LeftLowerLeg"]="L Calf",
                ["LeftFoot"]="L Foot",
                ["LeftToes"]="L Toe0",
                ["RightUpperLeg"]="R Thigh",
                ["RightLowerLeg"]="R Calf",
                ["RightFoot"]="R Foot",
                ["RightToes"]="R Toe0"
            };
            var humanBones=human.Select(pair=>new HumanBone{
                humanName=pair.Key,boneName=pair.Value,
                limit=new HumanLimit{useDefaultValues=true}
            }).ToArray();
            var skeleton=model.GetComponentsInChildren<Transform>(true)
                .Select(t=>new SkeletonBone{
                    name=t.name,
                    position=t.localPosition,
                    rotation=t.localRotation,
                    scale=t.localScale
                }).ToArray();
            return new HumanDescription{
                human=humanBones,skeleton=skeleton,
                armStretch=.05f,legStretch=.05f,
                upperArmTwist=.5f,lowerArmTwist=.5f,
                upperLegTwist=.5f,lowerLegTwist=.5f,
                feetSpacing=0f,hasTranslationDoF=false};
        }

        [CliCommand("dicefree.greatblade.rig-human",
            "Build mapped Humanoid Avatar for A03 female rig from actual imported skeleton.")]
        public static object Install()
        {
            var body=AssetDatabase.LoadAssetAtPath<GameObject>(Body);
            if(body==null)throw new InvalidOperationException("Missing A03 FBX");
            var h=Description(body);
            Avatar built=AvatarBuilder.BuildHumanAvatar(body,h);
            bool valid=built!=null&&built.isHuman&&built.isValid;
            if(!valid)
                return new{success=false,
                    stage="AvatarBuilder",
                    bones=h.skeleton.Length,
                    mapped=h.human.Select(x=>x.humanName+"="+x.boneName).ToArray(),
                    avatarExists=built!=null,
                    human=built!=null && built.isHuman,
                    valid=built!=null && built.isValid};
            var previous=AssetDatabase.LoadAssetAtPath<Avatar>(Local);
            if(previous==null)
            {
                AssetDatabase.CreateAsset(built,Local);
                AssetDatabase.SaveAssets();
            }
            else UnityEngine.Object.DestroyImmediate(built);
            var asset=AssetDatabase.LoadAssetAtPath<Avatar>(Local);
            return new{success=asset!=null&&asset.isHuman&&asset.isValid,
                bones=h.skeleton.Length,mapped=h.human.Length,
                human=asset!=null&&asset.isHuman,
                valid=asset!=null&&asset.isValid};
        }
    }
}
