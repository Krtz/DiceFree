using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class FireMageMagicianStaticReview
{
    [Serializable]public sealed class Report{public string status="passed";public int checks;public string[] passed;public string[] motions;}
    public static void Batch()
    {
        var checks=new List<string>();void Need(bool value,string label){if(!value)throw new Exception(label);checks.Add(label);}
        string art=FireMageMagicianAuthoring.Art;var avatar=AssetDatabase.LoadAllAssetsAtPath(art+"EmberMagician.fbx").OfType<Avatar>().Single();Need(avatar.isValid&&avatar.isHuman,"Valid humanoid avatar for selected orange Magician");
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(FireMageAuthoring.Data+"Fire Mage form.prefab");Need(prefab!=null,"Stable existing Fire Mage prefab asset updated");Need(!AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefab),true).Any(p=>p.Contains("/Dawnwarden/")),"Fire Mage no longer depends on Priest geometry");
        var root=Object.Instantiate(prefab);
        var motions=new List<string>();
        try
        {
            root.SetActive(true);var animator=root.GetComponentInChildren<Animator>();animator.Rebind();animator.Update(0);
            Need(animator.avatar==avatar&&animator.isHuman,"Prefab Animator uses the authored humanoid avatar");
            Need(animator.GetComponent<FireMageStaffPresentation>()!=null,"Original staff has live and portrait bone-follow presentation");
            var skins=root.GetComponentsInChildren<SkinnedMeshRenderer>();Need(skins.Length==1,"One weighted character skin");
            foreach(var skin in skins)
            {
                var weights=skin.sharedMesh.boneWeights;Need(weights.Length>2000&&weights.All(w=>Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.001f),"Every source vertex has normalized skin weights");
                Need(skin.bones.Length>=21&&skin.bones.All(b=>b!=null),"All deform bone references resolve");
            }
            Need(root.GetComponentsInChildren<Renderer>().All(r=>r.bounds.size.magnitude<5),"Source rig scale does not enlarge meshes or staff");
            foreach(var state in new[]{"Idle","Locomotion","Cast","Guard","Attack","ArmedAttack","Hit","Death"})
            {
                Need(animator.HasState(0,Animator.StringToHash("Base Layer."+state)),"Native motion state "+state);
                animator.Play(state,0,.4f);animator.Update(0);animator.GetComponent<FireMageStaffPresentation>().ApplyPortraitPose();
                foreach(var skin in skins)
                {
                    var baked=new Mesh();skin.BakeMesh(baked);Need(baked.bounds.size.magnitude<5,"Bounded skin deformation in "+state);Object.DestroyImmediate(baked);
                }
            }
            foreach(var clip in animator.runtimeAnimatorController.animationClips.Distinct()){Need(clip.humanMotion&&clip.length>0,"Humanoid clip "+clip.name);motions.Add(clip.name+": "+clip.length+" seconds");}
            Need(animator.GetBoneTransform(HumanBodyBones.RightHand)!=null&&animator.GetBoneTransform(HumanBodyBones.LeftHand)!=null,"Both animated casting hands resolve");
            var material=skins.Single().sharedMaterial;Need(material.GetTexture("_BaseMap") is Texture2D texture&&texture.width==512&&texture.height==512,"Original 512-pixel painted atlas preserved");
        }
        finally{Object.DestroyImmediate(root);}
        foreach(string scene in new[]{"Cornberg","DevelopmentRoom"})
        {EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/"+scene+".unity");var player=Object.FindObjectsByType<AdvancementController>(FindObjectsInactive.Include).Single();Need(player.GetComponentsInChildren<FireMageStaffPresentation>(true).Length==1,scene+" contains exactly one new Fire Mage presentation");}
        EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/WorldTwoArrival.unity");var mentor=Object.FindAnyObjectByType<FireMageMentor>();Need(mentor.GetComponentInChildren<FireMageStaffPresentation>()!=null,"Maelis uses the selected orange Magician");Need(mentor.GetComponentInChildren<Animator>().avatar==avatar,"Mentor uses the same validated humanoid rig");
        File.WriteAllText("Logs/FireMageMagicianStaticReview.json",JsonUtility.ToJson(new Report{checks=checks.Count,passed=checks.ToArray(),motions=motions.ToArray()},true));EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");Debug.Log("DICEFREE_FIRE_MAGE_MAGICIAN_STATIC_OK "+checks.Count);
    }
}
