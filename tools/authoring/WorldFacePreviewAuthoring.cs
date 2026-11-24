using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class WorldFacePreviewAuthoring
{
    private const string Output = "Assets/_DiceFree/Resources/WorldFaceCrossing/";
    private const string Art = "Assets/_DiceFree/Art/World/World1Handcrafted/";
    private readonly struct Proxy
    {
        public readonly Mesh mesh;
        public readonly Material[] materials;
        public readonly Vector3 position, scale;
        public readonly Quaternion rotation;
        public Proxy(MeshFilter filter)
        {
            mesh=filter.sharedMesh;materials=filter.GetComponent<MeshRenderer>().sharedMaterials;
            position=filter.transform.position;scale=filter.transform.lossyScale;rotation=filter.transform.rotation;
        }
    }

    public static void Batch()
    {
        if(!AssetDatabase.IsValidFolder(Output.TrimEnd('/')))
            AssetDatabase.CreateFolder("Assets/_DiceFree/Resources","WorldFaceCrossing");
        var sourceRect=new Rect(-85,AshfallReach.WorldOneRiverEdgeZ,AshfallReach.FaceWidth,AshfallReach.FaceHeight);
        float riverX=-18+Mathf.Sin(-53*.065f)*2;
        float riverU=(riverX-sourceRect.xMin)/sourceRect.width;
        var targetRect=AshfallReach.LocalBounds;
        if(Mathf.Abs((AshfallReach.RiverX(35)-targetRect.xMin)/targetRect.width-riverU)>.0001f)
            throw new InvalidOperationException("The face textures disagree about the river seam.");
        var source=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
        var tree=source.GetRootGameObjects().SelectMany(root=>root.GetComponentsInChildren<MeshFilter>())
            .Where(filter=>filter.sharedMesh!=null&&filter.GetComponent<MeshRenderer>()!=null)
            .Where(filter=>
            {
                var bounds=filter.GetComponent<MeshRenderer>().bounds;
                return bounds.max.y>25 && new Vector2(bounds.center.x-98,bounds.center.z-305).sqrMagnitude<225;
            }).Select(filter=>new Proxy(filter)).ToArray();
        EnsureCapture("World 1",sourceRect,Vector3.zero);
        var target=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/WorldTwoArrival.unity",OpenSceneMode.Additive);
        SceneManager.SetActiveScene(source);
        var features=target.GetRootGameObjects().SelectMany(root=>root.GetComponentsInChildren<MeshFilter>())
            .Where(filter=>filter.GetComponent<MeshRenderer>()!=null&&
                (HasAncestor(filter.transform,"Basalt Needle")||HasAncestor(filter.transform,"Ember Crown")))
            .Select(filter=>new Proxy(filter)).ToArray();
        EnsureCapture("World 2",targetRect,AshfallReach.Origin);
        BuildCube(sourceRect,targetRect,tree,features);
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");
        Debug.Log("DICEFREE_WORLD_FACE_PREVIEWS_OK");
    }

    private static bool HasAncestor(Transform transform,string name)
    {
        for(var parent=transform.parent;parent!=null;parent=parent.parent)
            if(parent.name==name)return true;
        return false;
    }

    private static void EnsureCapture(string name,Rect bounds,Vector3 origin)
    {
        // Prefer the shipped runtime captures: Cornberg replaces legacy surfaces during startup.
        // Rebuilding the cube must not overwrite those with edit-mode debug terrain.
        if(File.Exists(Output+name+".png"))
        {
            AssetDatabase.ImportAsset(Output+name+".png",ImportAssetOptions.ForceUpdate);
            return;
        }
        var cameraObject=new GameObject("Authoring face capture");
        var camera=cameraObject.AddComponent<Camera>();
        camera.enabled=false;camera.orthographic=true;camera.orthographicSize=bounds.height*.5f;
        camera.aspect=bounds.width/bounds.height;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.18f,.26f,.17f);
        camera.nearClipPlane=.3f;camera.farClipPlane=1500;
        camera.cullingMask=~((1<<31)|(1<<29)|(1<<30));
        camera.transform.SetPositionAndRotation(origin+new Vector3(bounds.center.x,650,bounds.center.y),
            Quaternion.Euler(90,0,0));
        var texture=new RenderTexture(1536,1536,24,RenderTextureFormat.ARGB32);
        var pixels=new Texture2D(1536,1536,TextureFormat.RGB24,false);
        var active=RenderTexture.active;
        bool fog=RenderSettings.fog;
        try
        {
            RenderSettings.fog=false;
            camera.targetTexture=texture;
            camera.Render();
            RenderTexture.active=texture;
            pixels.ReadPixels(new Rect(0,0,1536,1536),0,0);
            pixels.Apply();
            File.WriteAllBytes(Output+name+".png",pixels.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture=null;RenderTexture.active=active;RenderSettings.fog=fog;
            Object.DestroyImmediate(cameraObject);texture.Release();Object.DestroyImmediate(texture);Object.DestroyImmediate(pixels);
        }
        AssetDatabase.ImportAsset(Output+name+".png",ImportAssetOptions.ForceUpdate);
        var importer=(TextureImporter)AssetImporter.GetAtPath(Output+name+".png");
        importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Trilinear;
        importer.maxTextureSize=2048;importer.mipmapEnabled=true;importer.anisoLevel=4;
        importer.SaveAndReimport();
    }

    private static void BuildCube(Rect sourceRect,Rect destinationRect,Proxy[] tree,Proxy[] features)
    {
        var root=new GameObject("Crossing cube");
        root.layer=31;
        var surface=Material("Map surface","Universal Render Pipeline/Unlit",Color.white);
        var body=Material("Dice stone","Universal Render Pipeline/Lit",new Color(.10f,.15f,.17f));
        var bronze=AssetDatabase.LoadAssetAtPath<Material>(Art+"Brass.mat");
        Part(root.transform,"Stone body",PrimitiveType.Cube,Vector3.zero,Vector3.one*18,body);
        var first=new Material(surface){name="World 1 surface"};
        first.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Output+"World 1.png"));
        first=Save(first,Output+first.name+".mat");
        var second=new Material(surface){name="World 2 surface"};
        second.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Output+"World 2.png"));
        second=Save(second,Output+second.name+".mat");
        Quad(root.transform,"The green face",new[]{new Vector3(-9,9.025f,-9),new Vector3(-9,9.025f,9),
            new Vector3(9,9.025f,-9),new Vector3(9,9.025f,9)},first);
        Quad(root.transform,"The warm river face",new[]{new Vector3(-9,-9,-9.025f),new Vector3(-9,9,-9.025f),
            new Vector3(9,-9,-9.025f),new Vector3(9,9,-9.025f)},second);
        AddFeatures(root.transform,tree,sourceRect,Vector3.zero,false);
        AddFeatures(root.transform,features,destinationRect,AshfallReach.Origin,true);
        for(int side=-1;side<=1;side+=2)
        for(int across=-1;across<=1;across+=2)
        {
            Part(root.transform,"Bronze edge",PrimitiveType.Cube,new(side*9.05f,across*9.05f,0),new(.075f,.075f,18.15f),bronze);
            Part(root.transform,"Bronze edge",PrimitiveType.Cube,new(side*9.05f,0,across*9.05f),new(.075f,18.15f,.075f),bronze);
            Part(root.transform,"Bronze edge",PrimitiveType.Cube,new(0,side*9.05f,across*9.05f),new(18.15f,.075f,.075f),bronze);
        }
        var light=new GameObject("Preview light").AddComponent<Light>();
        light.transform.SetParent(root.transform,false);
        light.type=LightType.Directional;light.color=new Color(.87f,.94f,1);light.intensity=1.1f;
        light.cullingMask=1<<31;light.shadows=LightShadows.None;
        light.transform.localRotation=Quaternion.Euler(43,35,0);
        var prefab=PrefabUtility.SaveAsPrefabAsset(root,Output+"Crossing cube.prefab");
        Object.DestroyImmediate(root);
        if(prefab==null)throw new IOException("Could not save the production crossing cube.");
    }

    private static void AddFeatures(Transform root,Proxy[] proxies,Rect bounds,Vector3 origin,bool front)
    {
        var orientation=front?Quaternion.Euler(-90,0,0):Quaternion.identity;
        var frame=new GameObject(front?"World 2 landmark frame":"World 1 landmark frame").transform;
        frame.SetParent(root,false);frame.gameObject.layer=31;
        frame.localPosition=(front?Vector3.back:Vector3.up)*9.03f;frame.localRotation=orientation;
        frame.localScale=new Vector3(18/bounds.width,18/Mathf.Max(bounds.width,bounds.height),18/bounds.height);
        foreach(var proxy in proxies)
        {
            var go=new GameObject(front?"World 2 pip landmark":"World Tree pip");
            go.layer=31;go.transform.SetParent(frame,false);
            var p=proxy.position-origin-new Vector3(bounds.center.x,0,bounds.center.y);
            go.transform.localPosition=p;
            go.transform.localRotation=proxy.rotation;
            go.transform.localScale=proxy.scale;
            go.AddComponent<MeshFilter>().sharedMesh=proxy.mesh;
            var renderer=go.AddComponent<MeshRenderer>();
            renderer.sharedMaterials=proxy.materials;
            renderer.shadowCastingMode=ShadowCastingMode.Off;
        }
    }

    private static void Quad(Transform parent,string name,Vector3[] points,Material material)
    {
        var mesh=new Mesh{name=name,vertices=points,uv=new[]{Vector2.zero,Vector2.up,Vector2.right,Vector2.one},
            triangles=new[]{0,1,2,2,1,3}};
        mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=Save(mesh,Output+name+".asset");
        var go=new GameObject(name);go.layer=31;go.transform.SetParent(parent,false);
        go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
    }

    private static void Part(Transform parent,string name,PrimitiveType type,Vector3 at,Vector3 scale,Material material)
    {
        var go=GameObject.CreatePrimitive(type);go.name=name;go.layer=31;go.transform.SetParent(parent,false);
        go.transform.localPosition=at;go.transform.localScale=scale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial=material;
    }

    private static Material Material(string name,string shader,Color color)=>
        Save(new Material(Shader.Find(shader)){name=name,color=color},Output+name+".mat");

    private static T Save<T>(T value,string path)where T:Object
    {
        var current=AssetDatabase.LoadAssetAtPath<T>(path);
        if(current==null){AssetDatabase.CreateAsset(value,path);return value;}
        EditorUtility.CopySerialized(value,current);EditorUtility.SetDirty(current);Object.DestroyImmediate(value);return current;
    }
}
