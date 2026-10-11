using System;
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
    internal static class IronpineRigMotionValidation
    {
        [CliCommand("dicefree.ironpine-art.motion-validate",
            "Prove imported wolf, alpha and quillrat animations drive actual bones and deform skinned mesh vertices.")]
        public static object Validate()
        {
            var previous=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity",OpenSceneMode.Single);
                var wildlife=scene.GetRootGameObjects()
                    .SelectMany(r=>r.GetComponentsInChildren<IronpineWildlife>(true)).ToArray();
                string[] species={"Wolf","AlphaWolf","Quillrat"};
                int checkedRigs=0, changedVertices=0;
                foreach(string id in species)
                {
                    var chosen=wildlife.First(a=>
                        (id=="Wolf" && a.Kind==WildlifeKind.GreyWolf) ||
                        (id=="AlphaWolf" && a.Kind==WildlifeKind.AlphaWolf) ||
                        (id=="Quillrat" && a.Kind==WildlifeKind.Quillrat));
                    var animator=chosen.GetComponent<IronpineCreaturePresentation>().ModelAnimator;
                    var controller=animator.runtimeAnimatorController as AnimatorController;
                    Check(controller!=null,"missing animator controller "+id);
                    var locomotion=controller.layers[0].stateMachine.states
                        .FirstOrDefault(s=>s.state.name=="Walk").state?.motion as AnimationClip;
                    Check(locomotion!=null,"missing Walk animation "+id);
                    var limb=animator.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="FrontLUpper");
                    Check(limb!=null,"front leg bone missing "+id);
                    var skin=animator.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                        .FirstOrDefault(s=>s.bones.Any(b=>b!=null && b.name=="FrontLUpper"));
                    Check(skin!=null,"no skinned mesh assigned front leg "+id);
                    locomotion.SampleAnimation(animator.gameObject,.03f);
                    var first=limb.localRotation;
                    var meshA=new Mesh(); var meshB=new Mesh();
                    skin.BakeMesh(meshA);
                    locomotion.SampleAnimation(animator.gameObject,.31f);
                    var second=limb.localRotation;
                    skin.BakeMesh(meshB);
                    Check(Quaternion.Angle(first,second)>.8f,
                        "Walk animation doesn't move leg bone "+id);
                    Check(meshA.vertexCount==meshB.vertexCount && meshA.vertexCount>40,
                        "missing deformable mesh "+id);
                    var a=meshA.vertices;
                    var b=meshB.vertices;
                    int moving=0;
                    for(int i=0;i<a.Length;i++)
                        if((a[i]-b[i]).sqrMagnitude>.00002f)moving++;
                    Check(moving>8,"Walk animation fails to deform skinned geometry "+id+
                        " moving="+moving);
                    changedVertices+=moving;
                    Object.DestroyImmediate(meshA);Object.DestroyImmediate(meshB);
                    checkedRigs++;
                    Debug.Log("IRONPINE_RIG_MOTION_OK "+id+" movingVertices="+moving+
                              " boneDeltaDegrees="+Quaternion.Angle(first,second));
                }
                return new {success=true,rigCount=checkedRigs,changedVertices=changedVertices};
            }
            finally {if(previous.Length>0)EditorSceneManager.RestoreSceneManagerSetup(previous);}
        }
        private static void Check(bool ok,string detail)
        {
            if(!ok)throw new InvalidOperationException("IRONPINE RIG MOTION: "+detail);
        }
    }
}
