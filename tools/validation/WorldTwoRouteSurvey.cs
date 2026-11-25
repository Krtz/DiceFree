using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class WorldTwoRouteSurvey
{
    public static void Batch()
    {
        Directory.CreateDirectory("Logs/WorldTwoLibrary");
        var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/WorldTwoArrival.unity");
        var nodes=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
        string PathOf(Transform t)=>t.parent==null?t.name:PathOf(t.parent)+"/"+t.name;
        var routes=nodes.Where(t=>t.GetComponent<MeshFilter>()!=null&&(t.name.IndexOf("road",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("trail",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("street",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("avenue",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("lane",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("bridge",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("paved",StringComparison.OrdinalIgnoreCase)>=0||t.name.Contains("circuit"))).ToArray();
        File.WriteAllLines("Logs/WorldTwoRouteSurvey.txt",routes.Select(t=>PathOf(t)+" | speed="+(t.GetComponent<TravelSurface>()?.Definition?.speedBonusPercent.ToString()??"MISSING")+" | pos="+(t.position-AshfallReach.Origin)+" | mesh="+t.GetComponent<MeshFilter>().sharedMesh.name));
        var candidates=new[]{
            "Assets/the_wood_cabin/Prefabs/THE_WOOD_CABIN_LOD0.prefab",
            "Assets/Low-Poly Medieval Market/Prefabs/section_06.prefab",
            "Assets/Low-Poly Medieval Market/Prefabs/stand_01.prefab",
            "Assets/Low-Poly Medieval Market/Prefabs/stand_02.prefab",
            "Assets/Low-Poly Medieval Market/Prefabs/table_01.prefab",
            "Assets/_DiceFree/Art/World/VisualBatch01/Models/bridge_01.fbx",
            "Assets/_DiceFree/Art/World/VisualBatch01/Models/bridge_02.fbx",
            "Assets/_DiceFree/Art/World/CornbergBatch/Trees/Tree_Willow.fbx",
            "Assets/_DiceFree/Art/World/CornbergBatch/Trees/Tree_Birch.fbx",
            "Assets/_DiceFree/Art/World/VisualBatch01/Models/anvil_02.fbx"
        };
        var log=new List<string>();
        foreach(var path in candidates)
        {
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(asset==null){log.Add("MISSING "+path);continue;}
            var go=Object.Instantiate(asset);go.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);go.transform.localScale=Vector3.one;
            foreach(var lod in go.GetComponentsInChildren<LODGroup>())lod.ForceLOD(0); var mats=new List<Material>();
            foreach(var r in go.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(source=>{
                var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));if(source!=null){m.color=source.HasProperty("_BaseColor")?source.GetColor("_BaseColor"):source.HasProperty("_Color")?source.color:Color.white;var map=source.HasProperty("_BaseMap")?source.GetTexture("_BaseMap"):source.HasProperty("_MainTex")?source.GetTexture("_MainTex"):null;m.SetTexture("_BaseMap",map);}m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",m.color*.5f);mats.Add(m);return m;}).ToArray();
            var renderers=go.GetComponentsInChildren<Renderer>();var b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);
            var preview=new PreviewRenderUtility();Texture2D tex=null;var old=RenderTexture.active;
            try
            {
                preview.AddSingleGO(go);var cam=preview.camera;cam.orthographic=true;cam.orthographicSize=Mathf.Max(b.extents.y,b.extents.magnitude*.7f);cam.nearClipPlane=.01f;cam.farClipPlane=2000;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new(.1f,.12f,.14f);cam.transform.position=b.center+new Vector3(1,.75f,-1).normalized*b.size.magnitude*2;cam.transform.LookAt(b.center);
                preview.lights[0].intensity=1.3f;preview.lights[0].transform.rotation=Quaternion.Euler(30,-30,0);preview.lights[1].intensity=.9f;preview.ambientColor=new(.4f,.4f,.4f);preview.BeginPreview(new Rect(0,0,600,600),GUIStyle.none);preview.Render(true);RenderTexture.active=(RenderTexture)preview.EndPreview();tex=new Texture2D(600,600,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,600,600),0,0);tex.Apply();File.WriteAllBytes("Logs/WorldTwoLibrary/"+asset.name+".png",tex.EncodeToPNG());
                log.Add(path+" | bounds="+b.size+" | renderers="+renderers.Length+" | missingMeshes="+go.GetComponentsInChildren<MeshFilter>().Count(f=>f.sharedMesh==null)+" | enabled="+renderers.Count(r=>r.enabled));
            }
            finally{RenderTexture.active=old;preview.Cleanup();if(go!=null)Object.DestroyImmediate(go);if(tex!=null)Object.DestroyImmediate(tex);foreach(var m in mats)Object.DestroyImmediate(m);}
        }
        File.WriteAllLines("Logs/WorldTwoLibrary/candidates.txt",log);Debug.Log("DICEFREE_WORLD_TWO_ROUTE_SURVEY_OK");
    }
}
