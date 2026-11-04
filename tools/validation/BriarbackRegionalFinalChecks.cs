using System;
using System.IO;
using System.Linq;
using DiceFree.Briarback;
using DiceFree.EditorTools;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object=UnityEngine.Object;

public static class BriarbackRegionalFinalChecks
{
    public static object Run()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required.");
        ArchitectureValidation.Run();ArchitectureValidation.RunPolicySelfTests(); // Architecture includes managed-reference validation.
        var quest=BriarbackElementalQuestValidation.Validate();var fort=BriarbackNorthernStrongholdAuthoring.Validate();
        var presentations=Object.FindObjectsByType<QuilboarPresentation>(FindObjectsInactive.Include);if(presentations.Length!=33)throw new InvalidOperationException("Expected all 33 creature presentations.");
        string folder="Assets/_DiceFree/Art/Creatures/Briarback/RegionalMotion/";var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(folder+"Quilboar regional.controller");
        var states=controller.layers[0].stateMachine.states.Select(s=>s.state.name).OrderBy(n=>n).ToArray();
        if(!states.SequenceEqual(new[]{"Attack","Cast","Death","Idle","Locomotion","Slam","Throw"})||presentations.Any(p=>p.model.runtimeAnimatorController!=controller))throw new InvalidOperationException("Regional motion is not consistently installed.");
        var original=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_DiceFree/Art/Creatures/Briarback/Models/Quilboar.fbx");var copy=Object.Instantiate(original);copy.hideFlags=HideFlags.HideAndDontSave;
        var first=new Mesh();var second=new Mesh();var deformations=new System.Collections.Generic.List<string>();
        try
        {
            copy.GetComponent<Animator>().enabled=false;var skin=copy.GetComponentInChildren<SkinnedMeshRenderer>();
            foreach(var name in new[]{"Locomotion","Attack","Cast","Throw","Slam","Death"})
            {
                var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(folder+name+".anim");clip.SampleAnimation(copy,clip.length*.12f);skin.BakeMesh(first);clip.SampleAnimation(copy,clip.length*.58f);skin.BakeMesh(second);
                int moving=first.vertices.Zip(second.vertices,(a,b)=>Vector3.Distance(a,b)).Count(d=>d>.005f);if(moving<100)throw new InvalidOperationException("Frozen regional motion: "+name+" moving="+moving);deformations.Add(name+":"+moving);
            }
        }
        finally{Object.DestroyImmediate(copy);Object.DestroyImmediate(first);Object.DestroyImmediate(second);}
        return new{architecture=true,dependencyPolicy=true,managedReferences=true,quest,fort,creaturePresentations=presentations.Length,states,vertexDeformation=deformations.ToArray()};
    }
}
