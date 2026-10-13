using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.Pipeline.Commands;

namespace DiceFree.EditorTools
{
    public static class GreatbladeArtSurvey
    {
        [CliCommand("dicefree.greatblade.art-survey",
            "Check locally licensed Greatblade base models, greatswords and two-handed animation sources.")]
        public static object Inspect()
        {
            var paths=new[]{
                "Assets/AllStarCharacterLibrary/Models/Characters/A03.FBX",
                "Assets/Blink/Art/Characters/Stylized/Humans/Prefabs_Humans/HumanMale_Character_Free.prefab",
                "Assets/Blink/Art/Animations/Combat/MeeleeAttack_TwoHanded.fbx",
                "Assets/Blink/Art/Animations/Combat/SpinAttack_TwoHanded.fbx",
                "Assets/Blink/Art/Animations/Combat/SpinToWin_TwoHanded.fbx",
                "Assets/Blink/Art/Animations/Movement/Idle.fbx",
                "Assets/Blink/Art/Animations/Movement/WalkForward.fbx",
                "Assets/PurePoly/Free_Swords/Prefabs/PP_Sword_0177.prefab",
                "Assets/PurePoly/Free_Swords/Prefabs/PP_Sword_0363.prefab",
                "Assets/PurePoly/Free_Swords/Prefabs/PP_Sword_1039.prefab"
            };
            var models=paths.Select(path=>{
                var obj=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var clips=AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<AnimationClip>()
                    .Where(c=>!c.name.StartsWith("__preview"))
                    .Select(c=>c.name).ToArray();
                var anim=obj==null?null:obj.GetComponentInChildren<Animator>(true);
                return new{
                    path,found=obj!=null,
                    mesh=obj==null?0:obj.GetComponentsInChildren<Renderer>(true).Length,
                    skinned=obj==null?0:obj.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length,
                    avatar=anim!=null && anim.avatar!=null && anim.avatar.isHuman,
                    clips
                };
            }).ToArray();
            foreach(var model in models)
                Debug.Log("GREATBLADE_ASSET "+model.path+
                    " found="+model.found+" skinned="+model.skinned+
                    " human="+model.avatar+" animations="+
                    string.Join(",",model.clips));
            var candidate=AssetDatabase.LoadAssetAtPath<GameObject>(paths[0]);
            var boneNames=candidate.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .SelectMany(renderer=>renderer.bones)
                .Where(bone=>bone!=null)
                .Select(bone=>bone.name)
                .Distinct().Take(100).ToArray();
            return new{success=true,models,femaleBoneNames=boneNames};
        }
    }
}
