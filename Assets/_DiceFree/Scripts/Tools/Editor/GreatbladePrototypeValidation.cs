using System;
using System.Linq;
using DiceFree.Progression;
using DiceFree.Visuals;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
    public static class GreatbladePrototypeValidation
    {
        private static void Require(bool ok,string error)
        {
            if(!ok)throw new InvalidOperationException("GREATBLADE PROTOTYPE: "+error);
        }
        [CliCommand("dicefree.greatblade.prototype.validate",
            "Verify Greatblade humanoid model, spin/attack animation, sword and staggered passive/skill progression gates.")]
        public static object Validate()
        {
            var art=AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_DiceFree/Resources/GreatbladeLicensed/GreatbladeHero.prefab");
            Require(art!=null,"Shared licensed visual prefab missing");
            var animator=art.GetComponent<Animator>();
            Require(animator!=null && animator.avatar!=null &&
                    animator.avatar.isValid && animator.avatar.isHuman,
                    "Female warrior must have valid Humanoid Avatar");
            var sword=art.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t=>t.name=="Oversized two-handed Greatblade sword");
            Require(sword!=null && sword.parent!=null &&
                    sword.parent.name=="R Hand",
                    "Sword must attach to actual hand socket");
            var controller=animator.runtimeAnimatorController as AnimatorController;
            Require(controller!=null,
                "Shared Greatblade visual lacks prototype animation controller");
            var states=controller.layers[0].stateMachine.states
                .Select(x=>x.state).ToArray();
            foreach(string required in new[]{
                "Greatblade two-hand ready","Greatblade walk",
                "Two-handed swing","Spin Dash"})
                Require(states.Any(x=>x.name==required && x.motion!=null),
                    "Missing playable animation state "+required);
            foreach(var motion in states)
                if(motion.motion is AnimationClip clip)
                    Require(clip.isHumanMotion,
                        "Animation must be Humanoid to retarget: "+clip.name);

            var prior=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(
                    "Assets/_DiceFree/Scenes/GreatbladePrototype.unity",
                    OpenSceneMode.Single);
                var previews=scene.GetRootGameObjects()
                    .SelectMany(x=>x.GetComponentsInChildren<GreatbladePrototypeVisual>(true))
                    .ToArray();
                Require(previews.Length==2,
                    "Prototype should contain player and swordswoman, using same base.");
                Require(previews.Any(x=>x.name.Contains("retired",
                        StringComparison.OrdinalIgnoreCase)),
                    "Retired swordswoman missing from shared preview");
                Require(previews.All(x=>GreatbladePrototypeVisual.ResourcePath==
                    "GreatbladeLicensed/GreatbladeHero"),
                    "Player/NPC model base diverged.");
            }
            finally{if(prior.Length>0)
                EditorSceneManager.RestoreSceneManagerSetup(prior);}

            var r=GreatbladeProgressionRules.Skills;
            Require(r.Length==6,"Expected four actives and two distinct passives");
            Require(r.Count(x=>x.passive)==2,
                "Mastery and Heavy Rhythm must be separate");
            foreach(var kind in new[]{
                GreatbladeProgressionRules.Kind.WideArc,
                GreatbladeProgressionRules.Kind.SpinDash,
                GreatbladeProgressionRules.Kind.GreatswordMastery,
                GreatbladeProgressionRules.Kind.HeavyRhythm})
                Require(GreatbladeProgressionRules.CanRankUp(kind,1,0),
                    "Required level-1 skill missing: "+kind);
            Require(!GreatbladeProgressionRules.CanRankUp(
                    GreatbladeProgressionRules.Kind.OverheadBreaker,5,0) &&
                GreatbladeProgressionRules.CanRankUp(
                    GreatbladeProgressionRules.Kind.OverheadBreaker,6,0),
                "Finisher must first unlock at level six");
            Require(!GreatbladeProgressionRules.CanRankUp(
                    GreatbladeProgressionRules.Kind.SpinDash,8,1) &&
                GreatbladeProgressionRules.CanRankUp(
                    GreatbladeProgressionRules.Kind.SpinDash,9,1),
                "Spin Dash uneven rank interval broken");
            Require(!GreatbladeProgressionRules.IsUltimateSlotAllowed(2) &&
                !GreatbladeProgressionRules.IsUltimateSlotAllowed(3) &&
                GreatbladeProgressionRules.IsUltimateSlotAllowed(4),
                "Ultimate slot must be exclusive to Tier IV+");
            Require(!GreatbladeProgressionRules.GetsMasteryCleaveHaste(1) &&
                GreatbladeProgressionRules.GetsMasteryCleaveHaste(2) &&
                !GreatbladeProgressionRules.GetsMasteryWideArcWidth(2) &&
                GreatbladeProgressionRules.GetsMasteryWideArcWidth(3) &&
                !GreatbladeProgressionRules.GetsMasteryFinisherProtection(3) &&
                GreatbladeProgressionRules.GetsMasteryFinisherProtection(4),
                "Progressively evolving Mastery milestones wrong.");
            Require(GreatbladeProgressionRules.MomentumGainFactor(3)>
                    GreatbladeProgressionRules.MomentumGainFactor(2) &&
                GreatbladeProgressionRules.MomentumDecayFactor(3)<
                    GreatbladeProgressionRules.MomentumDecayFactor(2),
                "Heavy Rhythm must increase gain and reduce decay by rank.");
            Require(r.All(x=>x.slot<8),
                "No Tier II skills permitted in reserved slot 9.");

            Debug.Log("DICEFREE_GREATBLADE_VISUAL_AND_PROGRESS_OK");
            return new{success=true,
                marker="DICEFREE_GREATBLADE_VISUAL_AND_PROGRESS_OK",
                playerAndMentorSameModel=true,
                femaleHumanoidAvatar=true,animationStates=states.Length,
                swordInHand=true,passives=2,
                level1Skills=4,breakerFirstLevel=6,
                ultimateTierMinimum=4};
        }
    }
}
