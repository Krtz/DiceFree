using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DiceFree.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class FireMageStaffGripReview
{
    [Serializable]public sealed class Report{public string status="passed";public int samples;public float minimumHeadAndHatClearance;public string[] motions;}
    private static List<Vector3> Clip(List<Vector3> polygon,float height,bool above)
    {
        var result=new List<Vector3>();
        for(int i=0;i<polygon.Count;i++)
        {
            var a=polygon[i];var b=polygon[(i+1)%polygon.Count];bool ia=above?a.y>=height:a.y<=height;bool ib=above?b.y>=height:b.y<=height;
            if(ia)result.Add(a);if(ia!=ib)result.Add(Vector3.LerpUnclamped(a,b,(height-a.y)/(b.y-a.y)));
        }
        return result;
    }
    public static void Batch()
    {
        var root=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(FireMageAuthoring.Data+"Fire Mage form.prefab"));
        float minimum=float.PositiveInfinity;int samples=0;var states=new[]{"Idle","Locomotion","Cast","Guard","Attack","ArmedAttack","Hit"};
        try
        {
            var animator=root.GetComponentInChildren<Animator>();var presentation=animator.GetComponent<FireMageStaffPresentation>();
            var staff=(Transform)new SerializedObject(presentation).FindProperty("staff").objectReferenceValue;
            var skin=root.GetComponentInChildren<SkinnedMeshRenderer>();var head=animator.GetBoneTransform(HumanBodyBones.Head);int hi=Array.IndexOf(skin.bones,head);
            var vertices=skin.sharedMesh.vertices;var weights=skin.sharedMesh.boneWeights;var bind=skin.sharedMesh.bindposes;
            var headVertices=vertices.Where((v,i)=>{var w=weights[i];return (w.boneIndex0==hi?w.weight0:0)+(w.boneIndex1==hi?w.weight1:0)+(w.boneIndex2==hi?w.weight2:0)+(w.boneIndex3==hi?w.weight3:0)>.999f;}).Select(v=>bind[hi].MultiplyPoint3x4(v)).ToArray();
            if(headVertices.Length<300)throw new Exception("Head and hat geometry missing from clearance review");
            var staffMeshes=staff.GetComponentsInChildren<MeshFilter>();
            var meshVertices=staffMeshes.Select(f=>f.sharedMesh.vertices).ToArray();var triangles=staffMeshes.Select(f=>f.sharedMesh.triangles).ToArray();
            foreach(float yaw in new[]{0f,90f,180f,270f})foreach(string state in states)for(int frame=0;frame<20;frame++)
            {
                root.transform.rotation=Quaternion.Euler(0,yaw,0);animator.Rebind();animator.Play(state,0,frame/20f);animator.Update(0);
                var upper=animator.GetBoneTransform(HumanBodyBones.RightUpperArm);var lower=animator.GetBoneTransform(HumanBodyBones.RightLowerArm);var hand=animator.GetBoneTransform(HumanBodyBones.RightHand);var left=animator.GetBoneTransform(HumanBodyBones.LeftHand);
                float first=Vector3.Distance(upper.position,lower.position),second=Vector3.Distance(lower.position,hand.position);var leftPosition=left.position;var leftRotation=left.rotation;
                presentation.ApplyPortraitPose();
                if(Vector3.Distance(staff.position,hand.position)>.0001f)throw new Exception("Detached staff in "+state);
                if(Mathf.Abs(first-Vector3.Distance(upper.position,lower.position))>.0001f||Mathf.Abs(second-Vector3.Distance(lower.position,hand.position))>.0001f)throw new Exception("Holding pose stretches arm in "+state);
                if(Vector3.Distance(leftPosition,left.position)>.0001f||Quaternion.Angle(leftRotation,left.rotation)>.01f)throw new Exception("Casting hand altered in "+state);
                var lastHand=hand.position;var lastStaff=staff.rotation;presentation.ApplyPortraitPose();
                if(Vector3.Distance(lastHand,hand.position)>.0001f||Quaternion.Angle(lastStaff,staff.rotation)>.01f)throw new Exception("Portrait holding pose accumulates drift in "+state);
                var headPoints=headVertices.Select(v=>root.transform.InverseTransformPoint(head.TransformPoint(v))).ToArray();float low=headPoints.Min(v=>v.y),high=headPoints.Max(v=>v.y),edge=headPoints.Max(v=>v.x);float closest=float.PositiveInfinity;
                for(int m=0;m<staffMeshes.Length;m++)
                {
                    var posed=meshVertices[m].Select(v=>root.transform.InverseTransformPoint(staffMeshes[m].transform.TransformPoint(v))).ToArray();var indices=triangles[m];
                    for(int i=0;i<indices.Length;i+=3)
                    {
                        var polygon=Clip(new List<Vector3>{posed[indices[i]],posed[indices[i+1]],posed[indices[i+2]]},low,true);polygon=Clip(polygon,high,false);
                        foreach(var p in polygon)closest=Mathf.Min(closest,p.x-edge);
                    }
                }
                if(float.IsInfinity(closest)||closest<.025f)throw new Exception("Staff touches head/hat envelope: "+state+" frame "+frame+" yaw "+yaw+" clearance "+closest);
                minimum=Mathf.Min(minimum,closest);samples++;
            }
            animator.Rebind();animator.Play("Death",0,.7f);animator.Update(0);presentation.ApplyPortraitPose();
            if(Quaternion.Angle(staff.rotation,animator.GetBoneTransform(HumanBodyBones.RightHand).rotation)>.01f)throw new Exception("Death pose does not release upright holding constraint");
        }
        finally{Object.DestroyImmediate(root);}
        EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");Directory.CreateDirectory("Logs");File.WriteAllText("Logs/FireMageStaffGripReview.json",JsonUtility.ToJson(new Report{samples=samples,minimumHeadAndHatClearance=minimum,motions=states},true));Debug.Log("DICEFREE_FIRE_MAGE_STAFF_GRIP_OK "+samples+" poses, clearance "+minimum);
    }
}