using System;
using System.Collections.Generic;
using DiceFree.World;
using System.Linq;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
namespace DiceFree.EditorTools
{
    internal static class CornbergSouthernBridgeAlignment
    {
        private const string ScenePath="Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string MeshPath="Assets/_DiceFree/Settings/Ironpine/Southern bridge aligned road.asset";
        private static readonly Vector2[] West={
            new(-45,-33),new(-36,-29),new(-30,-22),
            new(-26,-15),new(-26,-9),new(-23.59f,-9)
        };
        private static readonly Vector2[] East={
            new(-14.61f,-9),new(-12,-9),new(0,0),
            new(25,0),new(54,6),new(72,18),new(95,34),new(117,48)
        };
        private static void Check(bool value,string detail)
        {
            if(!value)throw new InvalidOperationException("SOUTH BRIDGE ALIGNMENT: "+detail);
        }
        private static IEnumerable<T> All<T>(Scene scene) where T:Component=>
            scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true));
        private static Vector3 Ground(Vector2 p,bool west)
        {
            var pos=CornbergLandscape.Ground(p.x,p.y)+Vector3.up*.11f;
            if(west && p.x>=-28f)
            {
                float t=Mathf.InverseLerp(-28,-23.59f,p.x);
                pos.y=Mathf.Lerp(pos.y,.215f,t);
            }
            if(!west && p.x<=-8f)
            {
                float t=Mathf.InverseLerp(-14.61f,-8f,p.x);
                pos.y=Mathf.Lerp(.215f,pos.y,t);
            }
            return pos;
        }
        private static Mesh BuildRoad()
        {
            var verts=new List<Vector3>();
            var triangles=new List<int>();
            var uv=new List<Vector2>();
            void AddStrip(Vector2[] route,bool west)
            {
                float cumulative=0;
                for(int segment=1;segment<route.Length;segment++)
                {
                    var a=route[segment-1];var b=route[segment];
                    var direction=(b-a).normalized;
                    var normal=new Vector2(-direction.y,direction.x)*2.30f;
                    int sections=Mathf.CeilToInt(Vector2.Distance(a,b)*1.75f);
                    for(int n=0;n<sections;n++)
                    {
                        float t0=n/(float)sections,t1=(n+1f)/sections;
                        var p0=Vector2.Lerp(a,b,t0);
                        var p1=Vector2.Lerp(a,b,t1);
                        int start=verts.Count;
                        verts.Add(Ground(p0-normal,west));verts.Add(Ground(p0+normal,west));
                        verts.Add(Ground(p1-normal,west));verts.Add(Ground(p1+normal,west));
                        triangles.Add(start);triangles.Add(start+1);triangles.Add(start+2);
                        triangles.Add(start+2);triangles.Add(start+1);triangles.Add(start+3);
                        float v0=cumulative+Vector2.Distance(a,p0);
                        float v1=cumulative+Vector2.Distance(a,p1);
                        uv.Add(new Vector2(0,v0*.23f));uv.Add(new Vector2(1,v0*.23f));
                        uv.Add(new Vector2(0,v1*.23f));uv.Add(new Vector2(1,v1*.23f));
                    }
                    cumulative+=Vector2.Distance(a,b);
                }
            }
            AddStrip(West,true);
            AddStrip(East,false);
            var mesh=new Mesh{name="Southern bridge connected mountain road"};
            mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetTriangles(triangles,0);
            mesh.SetUVs(0,uv);
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            return mesh;
        }
        private static (Scene scene,MeshFilter filter,Transform deck) Open()
        {
            var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var road=All<MeshFilter>(scene).Single(m=>m.name=="Mountain to east road");
            var deck=All<Transform>(scene).FirstOrDefault(t=>t.name=="Timber footbridge" &&
                Vector3.Distance(t.position,new Vector3(-19.1f,.02f,-9))<2f);
            Check(deck!=null,"southern bridge missing");
            Check(Mathf.Abs(deck.position.z+9)<.06f &&
                Mathf.Abs(deck.lossyScale.x-9)<.06f,"unexpected bridge geometry");
            return (scene,road,deck);
        }
        [CliCommand("dicefree.cornberg.southern-bridge.align",
            "Align the southern road to both footbridge entrances, removing diagonal submerged/broken road over deck.")]
        public static object Install()
        {
            Check(!EditorApplication.isPlayingOrWillChangePlaymode,"Exit play mode");
            var prior=EditorSceneManager.GetSceneManagerSetup();
            try{
                var (scene,road,deck)=Open();
                var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
                var built=BuildRoad();
                if(mesh==null){AssetDatabase.CreateAsset(built,MeshPath);mesh=built;}
                else {
                    EditorUtility.CopySerialized(built,mesh);
                    UnityEngine.Object.DestroyImmediate(built);
                    EditorUtility.SetDirty(mesh);
                }
                road.sharedMesh=mesh;
                var collider=road.GetComponent<MeshCollider>();
                if(collider!=null)collider.sharedMesh=mesh;
                EditorUtility.SetDirty(road);
                EditorSceneManager.MarkSceneDirty(scene);
                Check(EditorSceneManager.SaveScene(scene),"scene save failed");
                AssetDatabase.SaveAssets();
                return new{success=true,roadTriangles=mesh.triangles.Length/3,
                    westExit=West.Last().ToString(),eastEntry=East.First().ToString(),
                    deck=deck.position.ToString("F2")};
            }finally{if(prior.Length>0)EditorSceneManager.RestoreSceneManagerSetup(prior);}
        }
        [CliCommand("dicefree.cornberg.southern-bridge.validate",
            "Validate repaired southern road terminates on both deck edges and retains north/east route.")]
        public static object Validate()
        {
            var prior=EditorSceneManager.GetSceneManagerSetup();
            try{
                var (scene,road,deck)=Open();
                Check(road.sharedMesh==AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath),
                    "corrected road mesh not installed");
                var mesh=road.sharedMesh;
                var vertices=mesh.vertices.Select(road.transform.TransformPoint).ToArray();
                bool WestContact=vertices.Any(v=>Mathf.Abs(v.x+23.59f)<.15f &&
                    Mathf.Abs(v.z+9f)<2.45f && Mathf.Abs(v.y-.215f)<.06f);
                bool EastContact=vertices.Any(v=>Mathf.Abs(v.x+14.61f)<.15f &&
                    Mathf.Abs(v.z+9f)<2.45f && Mathf.Abs(v.y-.215f)<.06f);
                Check(WestContact && EastContact,"path ends don't meet bridge ends at deck height");
                Check(!vertices.Any(v=>v.x>-23.5f && v.x<-14.7f && Mathf.Abs(v.z+9)<2.6f),
                    "road covers bridge planks instead of stopping at deck");
                Check(vertices.Any(v=>v.x>100),"rest of road was truncated");
                var navigation=All<WorldNavigation>(scene).FirstOrDefault();
                Check(navigation!=null && navigation.Data!=null,"Cornberg baked NavMesh missing");
                var handle=NavMesh.AddNavMeshData(navigation.Data);
                bool crossing=false;
                try
                {
                    if(NavMesh.SamplePosition(new Vector3(-26,0,-9),out var bankWest,5f,NavMesh.AllAreas) &&
                       NavMesh.SamplePosition(new Vector3(-12,0,-9),out var bankEast,5f,NavMesh.AllAreas))
                    {
                        var path=new NavMeshPath();
                        crossing=NavMesh.CalculatePath(bankWest.position,bankEast.position,
                            NavMesh.AllAreas,path) && path.status==NavMeshPathStatus.PathComplete;
                    }
                } finally {if(handle.valid)handle.Remove();}
                Check(crossing,"NavMesh is not traversable across the corrected bridge");
                return new{success=true,roadTriangles=mesh.triangles.Length/3,
                    bridgeWest=WestContact,bridgeEast=EastContact,
                    bridgeVisible=true,navMeshCrossing=crossing};
            }finally{if(prior.Length>0)EditorSceneManager.RestoreSceneManagerSetup(prior);}
        }
    }
}
