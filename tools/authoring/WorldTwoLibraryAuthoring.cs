using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Core;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class WorldTwoLibraryAuthoring
{
    public const string Output="Assets/_DiceFree/Art/World/WorldTwoLibraryPass";
    const string Batch="Assets/_DiceFree/Art/World/VisualBatch01/Models/";
    const string Market="Assets/Low-Poly Medieval Market/Prefabs/";
    const string Marker="Library replacement";
    static readonly Dictionary<Object,Object> copies=new();
    static readonly List<string> replacements=new();
    public static void Apply()
    {
        Directory.CreateDirectory(Output);AssetDatabase.Refresh();copies.Clear();replacements.Clear();
        var scene=EditorSceneManager.OpenScene(WorldTwoRouteAuthoring.ScenePath);
        var world=scene.GetRootGameObjects().Single(g=>g.GetComponent<WorldNavigation>()!=null).transform;
        var city=world.Find("Anvilreach and the southern biomes/Anvilreach - city of the hearth");
        var stalls=city.Cast<Transform>().Where(t=>t.name=="Metalworkers market stall").ToArray();
        for(int i=0;i<stalls.Length;i++)
        {
            var stall=stalls[i];if(stall.Find(Marker)!=null)continue;
            foreach(var r in stall.GetComponentsInChildren<MeshRenderer>())if(r.name is "Sales counter" or "Stacked copper ingot")r.enabled=false;
            var display=Portable(Market+(i%2==0?"stand_01.prefab":"stand_02.prefab"),i%2==0?"Smiths blade display":"Bowyers display");
            Place(display,stall,new Bounds(new(0,1.05f,0),new(4.3f,2.1f,1.4f)));replacements.Add("Anvilreach market counter "+(i+1));
        }
        var benches=city.Cast<Transform>().Where(t=>t.name=="Smiths outdoor workbench").ToArray();
        for(int i=0;i<benches.Length;i++)
        {
            var bench=benches[i];if(bench.Find(Marker)!=null)continue;
            foreach(var r in bench.GetComponentsInChildren<MeshRenderer>())r.enabled=false;
            var display=Portable(Market+(i==1?"stand_02.prefab":"stand_01.prefab"),i==1?"Bowyers display":"Smiths blade display");
            Place(display,bench,new Bounds(new(0,.85f,0),new(3,1.7f,1.3f)));replacements.Add("Anvilreach stocked smiths bench "+(i+1));
        }
        // Preserve every original trunk collider and its baked navigation footprint.
        int trees=0;
        foreach(var occluder in world.GetComponentsInChildren<TreeCameraOccluder>(true))
        {
            var t=occluder.transform;if(t.name!="Southern Steamroot grove"||t.Find(Marker)!=null)continue;
            var p=new Vector2(t.position.x-AshfallReach.Origin.x,t.position.z-AshfallReach.Origin.z);
            bool riverside=Mathf.Abs(p.x-AshfallReach.RiverX(p.y))<33;
            bool fen=AshfallReach.DeepmossWeight(p)>.5f;
            if(!riverside&&!fen)continue;
            // A mix of drooping willows, silver birches and existing oaks.
            if(!riverside&&trees%3==2){trees++;continue;}
            var rr=t.GetComponentsInChildren<Renderer>();if(rr.Length==0)continue;
            var bounds=LocalBounds(t,rr);foreach(var r in rr)r.enabled=false;
            string path=fen?Batch+"tree_04.fbx":"Assets/_DiceFree/Art/World/CornbergBatch/Trees/"+(trees%3==0?"Tree_Birch.fbx":"Tree_Willow.fbx");
            var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            if(model==null)throw new InvalidOperationException(path);
            Place(model,t,bounds);replacements.Add((fen?"Deepmoss rooted tree":"Steamroot river tree")+" "+trees);trees++;
        }
        // Replace repeated round boulders, retaining the exact original transform,
        // solid colliders and bounds. The two pip landmarks are intentionally excluded.
        int rocks=0;
        foreach(var filter in world.GetComponentsInChildren<MeshFilter>(true).ToArray())
        {
            if(filter.name is not ("Fractured ridge stone" or "Fractured basalt"))continue;
            if(filter.transform.Find(Marker)!=null||filter.GetComponent<MeshRenderer>() is not {} old||!old.enabled)continue;
            var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Batch+(rocks%2==0?"rock_04.fbx":"rock_05.fbx")));
            foreach(var r in model.GetComponentsInChildren<MeshRenderer>())r.sharedMaterials=Enumerable.Repeat(old.sharedMaterial,r.sharedMaterials.Length).ToArray();
            Place(model,filter.transform,filter.sharedMesh.bounds);old.enabled=false;rocks++;replacements.Add("Faceted ridge rock "+rocks);
        }
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/WorldTwoLibraryPass.json",JsonUtility.ToJson(new Report{marketCounters=stalls.Length,workbenches=benches.Length,trees=replacements.Count(s=>s.Contains("tree")),rocks=rocks,replacements=replacements.ToArray()},true));
        Debug.Log("DICEFREE_WORLD_TWO_LIBRARY_PASS_OK");
    }
    static Bounds LocalBounds(Transform root,IEnumerable<Renderer> renderers)
    {
        bool first=true;var bounds=new Bounds();foreach(var r in renderers){var b=r.bounds;for(int i=0;i<8;i++){var v=root.InverseTransformPoint(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));if(first){bounds=new(v,Vector3.zero);first=false;}else bounds.Encapsulate(v);}}return bounds;
    }
    static void Place(GameObject model,Transform parent,Bounds target)
    {
        model.name=Marker;model.transform.SetParent(parent,false);model.transform.SetLocalPositionAndRotation(Vector3.zero,Quaternion.identity);model.transform.localScale=Vector3.one;
        var rr=model.GetComponentsInChildren<Renderer>();var b=LocalBounds(parent,rr);
        if(b.size.sqrMagnitude<.001f)throw new InvalidOperationException("Empty replacement for "+parent.name);
        model.transform.localScale=new(target.size.x/Mathf.Max(.01f,b.size.x),target.size.y/Mathf.Max(.01f,b.size.y),target.size.z/Mathf.Max(.01f,b.size.z));
        b=LocalBounds(parent,rr);model.transform.localPosition+=target.center-b.center;
        foreach(var c in model.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);
        foreach(var t in model.GetComponentsInChildren<Transform>())t.gameObject.layer=2;
    }
    static string Key(Object source)
    {
        if(!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source,out string guid,out long local))throw new InvalidOperationException("Untracked library dependency: "+source.name);
        return guid+"-"+local.ToString().Replace('-','n');
    }
    static T Save<T>(T candidate,string path) where T:Object
    {
        if(AssetDatabase.LoadAssetAtPath<T>(path) is {} existing){Object.DestroyImmediate(candidate);return existing;}
        AssetDatabase.CreateAsset(candidate,path);return candidate;
    }
    static Material Material(Material source)
    {
        if(copies.TryGetValue(source,out var cached))return (Material)cached;
        var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=source.name};
        m.color=source.HasProperty("_BaseColor")?source.GetColor("_BaseColor"):source.HasProperty("_Color")?source.color:Color.white;
        m.SetFloat("_Smoothness",.22f);
        var texture=source.HasProperty("_BaseMap")?source.GetTexture("_BaseMap"):source.HasProperty("_MainTex")?source.GetTexture("_MainTex"):null;
        if(texture!=null)m.SetTexture("_BaseMap",Texture(texture));
        var result=Save(m,Output+"/Material-"+Key(source)+".mat");copies[source]=result;return result;
    }
    static Texture2D Texture(Texture source)
    {
        if(copies.TryGetValue(source,out var cached))return (Texture2D)cached;
        string path=Output+"/Texture-"+Key(source)+".png";
        if(!File.Exists(path))
        {
            var rt=RenderTexture.GetTemporary(source.width,source.height,0);var old=RenderTexture.active;Texture2D image=null;
            try{Graphics.Blit(source,rt);RenderTexture.active=rt;image=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}
            finally{RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);if(image!=null)Object.DestroyImmediate(image);}
            AssetDatabase.ImportAsset(path);
        }
        var result=AssetDatabase.LoadAssetAtPath<Texture2D>(path);copies[source]=result;return result;
    }
    static GameObject Portable(string path,string name)
    {
        var result=new GameObject(name);var original=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        original.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);original.transform.localScale=Vector3.one;
        try
        {
            foreach(var filter in original.GetComponentsInChildren<MeshFilter>())
            {
                if(filter.sharedMesh==null)throw new InvalidOperationException("Missing library mesh: "+path);
                var renderer=filter.GetComponent<MeshRenderer>();if(renderer==null)continue;
                var go=new GameObject(filter.name);go.transform.SetParent(result.transform,false);go.transform.SetPositionAndRotation(filter.transform.position,filter.transform.rotation);go.transform.localScale=filter.transform.lossyScale;
                if(!copies.TryGetValue(filter.sharedMesh,out var mesh)){mesh=Save(Object.Instantiate(filter.sharedMesh),Output+"/Mesh-"+Key(filter.sharedMesh)+".asset");copies[filter.sharedMesh]=mesh;}
                go.AddComponent<MeshFilter>().sharedMesh=(Mesh)mesh;go.AddComponent<MeshRenderer>().sharedMaterials=renderer.sharedMaterials.Select(Material).ToArray();
            }
            return result;
        }
        finally{Object.DestroyImmediate(original);}
    }
    [Serializable]sealed class Report{public int marketCounters,workbenches,trees,rocks;public string[] replacements;}
}
