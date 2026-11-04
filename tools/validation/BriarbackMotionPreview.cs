using System;
using System.IO;
using System.Linq;
using DiceFree.Briarback;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

public static class BriarbackMotionPreview
{
    public static object Capture()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required.");
        string output="VisualBaselines/BriarbackRegion/Motion";Directory.CreateDirectory(output);
        var root=new GameObject("Temporary original motion studio");root.hideFlags=HideFlags.HideAndDontSave;
        var camera=root.AddComponent<Camera>();camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.09f,.12f,.13f);camera.orthographic=true;camera.orthographicSize=2;
        var lightGo=new GameObject("Temporary motion studio light");lightGo.transform.SetParent(root.transform);var light=lightGo.AddComponent<Light>();light.type=LightType.Directional;light.cullingMask=1<<31;light.intensity=1.4f;light.transform.rotation=Quaternion.Euler(40,-30,0);
        var rt=new RenderTexture(900,900,24);var tex=new Texture2D(900,900,TextureFormat.RGB24,false);var old=RenderTexture.active;camera.targetTexture=rt;
        var reports=new System.Collections.Generic.List<object>();
        try
        {
            foreach(var row in new[]{("Attack",QuilboarRole.Brute,.54f),("Cast",QuilboarRole.FireShaman,.62f),("Throw",QuilboarRole.SpearThrower,.58f),("Slam",QuilboarRole.Chieftain,.58f)})
            {
                var p=Object.FindObjectsByType<QuilboarPresentation>(FindObjectsInactive.Include).First(p=>p.GetComponent<QuilboarCombat>().role==row.Item2);
                var copy=Object.Instantiate(p.model.gameObject,root.transform);copy.transform.position=new Vector3(2000,0,2000);copy.transform.rotation=Quaternion.Euler(0,-25,0);copy.transform.localScale=Vector3.one;
                foreach(var t in copy.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
                copy.GetComponent<Animator>().enabled=false;
                var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/_DiceFree/Art/Creatures/Briarback/RegionalMotion/"+row.Item1+".anim");
                clip.SampleAnimation(copy,clip.length*row.Item3);
                var bones=copy.GetComponentsInChildren<Transform>(true);var hand=bones.Single(b=>b.name=="RightHand");var head=bones.Single(b=>b.name=="Head");
                var look=copy.transform.position+Vector3.up*1.15f;camera.transform.position=look+new Vector3(4,2.5f,-5);camera.transform.LookAt(look);camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,900,900),0,0);tex.Apply();File.WriteAllBytes(output+"/"+row.Item1.ToLowerInvariant()+".png",tex.EncodeToPNG());
                reports.Add(new{state=row.Item1,clipSeconds=clip.length,rightHandAboveHead=hand.position.y-head.position.y});Object.DestroyImmediate(copy);
            }
            return reports.ToArray();
        }
        finally{RenderTexture.active=old;Object.DestroyImmediate(root);Object.DestroyImmediate(tex);rt.Release();Object.DestroyImmediate(rt);}
    }
}
