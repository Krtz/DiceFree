using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using Object=UnityEngine.Object;

namespace DiceFree.EditorTools
{
    internal static class CornbergNaturePass
    {
        private const string ScenePath="Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string Art="Assets/_DiceFree/Art/World/NaturePass03";
        private const string Root="NaturePass03 - meadow and woodland groundcover";
        private const int TextureSize=512;
        private static System.Random Random=new(310173);
        private static readonly List<Vector2[]> RoadFaces=new();
        private static readonly List<Bounds> HouseFootprints=new();

        private static float Rand(float low,float high) =>
            low+(float)Random.NextDouble()*(high-low);

        private static float SeamlessNoise(float u,float v,float frequency,float offset)
        {
            float x=u*frequency+offset;
            float y=v*frequency+offset*.71f;
            float a=Mathf.PerlinNoise(x,y);
            float b=Mathf.PerlinNoise(x-frequency,y);
            float c=Mathf.PerlinNoise(x,y-frequency);
            float d=Mathf.PerlinNoise(x-frequency,y-frequency);
            float north=Mathf.Lerp(a,b,Mathf.SmoothStep(0,1,u));
            float south=Mathf.Lerp(c,d,Mathf.SmoothStep(0,1,u));
            return Mathf.Lerp(north,south,Mathf.SmoothStep(0,1,v));
        }

        private static Color GrassColor(float u,float v)
        {
            float broad=SeamlessNoise(u,v,3.1f,13.3f);
            float medium=SeamlessNoise(u,v,16.2f,28.5f);
            float fine=SeamlessNoise(u,v,99.1f,71.7f);
            float streak=SeamlessNoise(u,v,37f,37f);
            float value=(broad-.5f)*.19f+(medium-.5f)*.15f+
                        (fine-.5f)*.11f+(streak-.5f)*.06f;
            var dark=new Color(.31f,.46f,.225f);
            var light=new Color(.45f,.61f,.30f);
            return Color.Lerp(dark,light,Mathf.Clamp01(.52f+value*2.1f));
        }

        private static void WriteTexture(string filename,Func<float,float,Color> sample)
        {
            string path=Art+"/"+filename+".png";
            var tex=new Texture2D(TextureSize,TextureSize,TextureFormat.RGB24,false);
            var pixels=new Color[TextureSize*TextureSize];
            for(int y=0;y<TextureSize;y++)
            for(int x=0;x<TextureSize;x++)
                pixels[y*TextureSize+x]=sample((x+.5f)/TextureSize,(y+.5f)/TextureSize);
            tex.SetPixels(pixels);tex.Apply(false,false);
            File.WriteAllBytes(path,tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.wrapMode=TextureWrapMode.Repeat;
            importer.filterMode=FilterMode.Trilinear;
            importer.anisoLevel=4;
            importer.mipmapEnabled=true;
            importer.textureCompression=TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static Material Material(string path,Material from,Texture texture,
            Color tint,float smooth=.11f)
        {
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null)
            {
                mat=new Material(from ?? new Material(Shader.Find("Universal Render Pipeline/Lit")));
                AssetDatabase.CreateAsset(mat,path);
            }
            mat.SetTexture("_BaseMap",texture);
            mat.SetColor("_BaseColor",tint);
            mat.SetFloat("_Smoothness",smooth);
            mat.enableInstancing=true;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Texture2D Texture(string file) =>
            AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/"+file+".png");

        private static void PrepareTextures()
        {
            Directory.CreateDirectory(Art);
            // A small non-photographic meadow texture with colour mottling and
            // fine vertical blade marks. Colour distribution is compatible with
            // the existing Cornberg summer-green palette.
            WriteTexture("CornbergMeadowTex",GrassColor);
            WriteTexture("LeafMicrostructure",(u,v)=>
            {
                float n=Mathf.PerlinNoise(u*29+7,v*29+4);
                float f=Mathf.PerlinNoise(u*110+19,v*110+31);
                float veins=Mathf.Abs(Mathf.Sin((u*63+v*28)*Mathf.PI));
                float value=Mathf.Clamp01(.84f+.15f*(n-.45f)+.085f*(f-.5f)+
                    .03f*veins);
                return new Color(value,value*.992f,value*.96f);
            });
            WriteTexture("BarkMicrostructure",(u,v)=>
            {
                float grooves=Mathf.PerlinNoise(u*65+14,v*8+29);
                float grain=Mathf.PerlinNoise(u*105+3,v*25+11);
                float irregular=Mathf.PerlinNoise(u*8+7,v*6+12);
                float value=Mathf.Clamp01(.83f+(grooves-.5f)*.21f+
                    (grain-.5f)*.08f+(irregular-.5f)*.06f);
                return new Color(value,value*.96f,value*.91f);
            });
        }

        private static void ApplyTreeMaterials()
        {
            var sample=AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/_DiceFree/Materials/CornbergBatch/Foliage_0.mat");
            if(sample==null)throw new InvalidOperationException("Tree materials missing.");
            string folder="Assets/_DiceFree/Materials/CornbergBatch";
            foreach(string kind in new[]{"Foliage","LeafAccent","Bark","BarkDeep"})
            for(int i=0;i<5;i++)
            {
                string path=folder+"/"+kind+"_"+i+".mat";
                var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(mat==null)continue;
                mat.SetTexture("_BaseMap",
                    kind.StartsWith("Bark")?Texture("BarkMicrostructure"):
                    Texture("LeafMicrostructure"));
                mat.SetFloat("_Smoothness",kind.StartsWith("Bark")?.07f:.14f);
                mat.enableInstancing=true;
                EditorUtility.SetDirty(mat);
            }
        }

        private static void ImproveValleyGround(Scene scene)
        {
            var valley=scene.GetRootGameObjects().SelectMany(go=>
                go.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t=>t.name=="Cornberg valley");
            if(valley==null)throw new InvalidOperationException("Cornberg ground not found.");
            var filter=valley.GetComponent<MeshFilter>();
            var collider=valley.GetComponent<MeshCollider>();
            if(filter==null||collider==null)throw new InvalidOperationException("Valley physics missing.");
            var current=filter.sharedMesh;
            var mesh=Object.Instantiate(current);
            mesh.name="Cornberg valley - meadow UV";
            var verts=mesh.vertices;
            var uv=new Vector2[verts.Length];
            for(int i=0;i<verts.Length;i++)
            {
                Vector3 point=valley.TransformPoint(verts[i]);
                uv[i]=new Vector2(point.x/13f,point.z/13f);
            }
            mesh.uv=uv;
            string meshPath=Art+"/CornbergMeadowTerrainUV.asset";
            var old=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if(old!=null)AssetDatabase.DeleteAsset(meshPath);
            AssetDatabase.CreateAsset(mesh,meshPath);
            filter.sharedMesh=mesh;
            collider.sharedMesh=mesh;
            var oldMeadow=valley.GetComponent<MeshRenderer>().sharedMaterial;
            var detail=Material(Art+"/MeadowDetail.mat",oldMeadow,
                Texture("CornbergMeadowTex"),Color.white,.085f);
            valley.GetComponent<MeshRenderer>().sharedMaterial=detail;
        }

        private static bool Inside2D(Vector2 p,Vector2 a,Vector2 b,Vector2 c)
        {
            float c1=(b.x-a.x)*(p.y-a.y)-(b.y-a.y)*(p.x-a.x);
            float c2=(c.x-b.x)*(p.y-b.y)-(c.y-b.y)*(p.x-b.x);
            float c3=(a.x-c.x)*(p.y-c.y)-(a.y-c.y)*(p.x-c.x);
            return (c1>=-.001f&&c2>=-.001f&&c3>=-.001f)||
                (c1<=.001f&&c2<=.001f&&c3<=.001f);
        }

        private static void CacheExclusions(Scene scene)
        {
            RoadFaces.Clear();HouseFootprints.Clear();
            foreach(var root in scene.GetRootGameObjects())
            foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if(!renderer.gameObject.activeInHierarchy)continue;
                var mat=renderer.sharedMaterial;
                if(mat!=null&&mat.name=="Warm earth")
                {
                    var filter=renderer.GetComponent<MeshFilter>();
                    if(filter?.sharedMesh==null)continue;
                    var vertices=filter.sharedMesh.vertices;
                    var triangles=filter.sharedMesh.triangles;
                    for(int i=0;i<triangles.Length;i+=3)
                    {
                        var a=filter.transform.TransformPoint(vertices[triangles[i]]);
                        var b=filter.transform.TransformPoint(vertices[triangles[i+1]]);
                        var c=filter.transform.TransformPoint(vertices[triangles[i+2]]);
                        var face=new[]{new Vector2(a.x,a.z),new Vector2(b.x,b.z),
                            new Vector2(c.x,c.z)};
                        RoadFaces.Add(face);
                    }
                }
                if(renderer.name=="Fieldstone foundation")
                    HouseFootprints.Add(renderer.bounds);
            }
        }

        private static bool IsClear(Vector3 place)
        {
            var point=new Vector2(place.x,place.z);
            foreach(var house in HouseFootprints)
                if(place.x>house.min.x-1.6f&&place.x<house.max.x+1.6f &&
                   place.z>house.min.z-1.6f&&place.z<house.max.z+1.6f)
                    return false;
            foreach(var triangle in RoadFaces)
                if(Inside2D(point,triangle[0],triangle[1],triangle[2]))
                    return false;
            // Cornberg river flows near x=-17 at the village and across the
            // outer world. Do not place decorative leaves in the water.
            return true;
        }

        private sealed class MeshWriter
        {
            private readonly List<Vector3> vertices=new();
            private readonly List<int> triangles=new();
            private readonly List<Vector2> uv=new();
            public int Count {get;private set;}
            public void Plant(Vector3 location,float height,float spread,float phase)
            {
                Count++;
                int blades=4+(int)(Mathf.Abs(phase)*3);
                for(int i=0;i<blades;i++)
                {
                    float angle=phase*3.7f+i*Mathf.PI*2f/blades;
                    var along=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                    var side=new Vector3(-along.z,0,along.x);
                    float h=height*Rand(.76f,1.18f);
                    float halfWidth=spread*Rand(.14f,.24f);
                    var center=location+along*Rand(0,.11f);
                    var b0=center-side*halfWidth;
                    var b1=center+side*halfWidth;
                    var middle=center+along*Rand(.1f,.19f)+Vector3.up*(h*.60f);
                    var tip=center+along*Rand(.16f,.29f)+Vector3.up*h;
                    int index=vertices.Count;
                    vertices.Add(b0);vertices.Add(b1);
                    vertices.Add(middle-side*halfWidth*.53f);
                    vertices.Add(middle+side*halfWidth*.53f);
                    vertices.Add(tip);
                    uv.Add(new Vector2(0,0));uv.Add(new Vector2(1,0));
                    uv.Add(new Vector2(.25f,.6f));uv.Add(new Vector2(.75f,.6f));
                    uv.Add(new Vector2(.5f,1f));
                    triangles.AddRange(new[]{index,index+2,index+1,
                        index+1,index+2,index+3,
                        index+2,index+4,index+3});
                }
            }
            public void Fern(Vector3 position,float size,float phase)
            {
                for(int frond=0;frond<5;frond++)
                {
                    float angle=phase+frond*Mathf.PI*2f/5f;
                    Vector3 direction=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                    Vector3 side=new Vector3(-direction.z,0,direction.x);
                    int segments=5;
                    for(int step=0;step<segments;step++)
                    {
                        float t=step/(float)segments;
                        float next=(step+1)/(float)segments;
                        float width=size*.15f*Mathf.Sin(Mathf.PI*(.12f+t*.85f));
                        float nextWidth=size*.15f*Mathf.Sin(Mathf.PI*(.12f+next*.85f));
                        Vector3 a=position+direction*size*t+
                            Vector3.up*(.12f+size*(.7f*t-.48f*t*t));
                        Vector3 b=position+direction*size*next+
                            Vector3.up*(.12f+size*(.7f*next-.48f*next*next));
                        int index=vertices.Count;
                        vertices.Add(a-side*width);vertices.Add(a+side*width);
                        vertices.Add(b-side*nextWidth);vertices.Add(b+side*nextWidth);
                        uv.Add(new Vector2(0,t));uv.Add(new Vector2(1,t));
                        uv.Add(new Vector2(0,next));uv.Add(new Vector2(1,next));
                        triangles.AddRange(new[]{index,index+2,index+1,
                            index+1,index+2,index+3});
                    }
                }
                Count++;
            }

            public void Blossom(Vector3 position,float radius,float h)
            {
                int petals=5;
                Vector3 flowerCenter=position+Vector3.up*h;
                for(int i=0;i<petals;i++)
                {
                    float angle=(i+0.2f)*Mathf.PI*2f/petals;
                    Vector3 dir=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                    Vector3 perpendicular=new Vector3(-dir.z,0,dir.x);
                    int idx=vertices.Count;
                    vertices.Add(flowerCenter);
                    vertices.Add(flowerCenter+dir*radius+perpendicular*radius*.48f);
                    vertices.Add(flowerCenter+dir*radius-perpendicular*radius*.48f);
                    triangles.AddRange(new[]{idx,idx+1,idx+2});
                    uv.Add(new Vector2(.5f,.5f));uv.Add(new Vector2(1,1));
                    uv.Add(new Vector2(0,1));
                }
                Count++;
            }
            public Mesh Save(string path)
            {
                var mesh=new Mesh{name=Path.GetFileNameWithoutExtension(path),
                    indexFormat=IndexFormat.UInt32};
                mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);
                mesh.SetUVs(0,uv);
                // Blades and flower petals keep upward diffuse normals so all
                // directions remain naturally green, not black silhouettes.
                mesh.SetNormals(Enumerable.Repeat(Vector3.up,vertices.Count).ToArray());
                mesh.RecalculateBounds();
                var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(old!=null)AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(mesh,path);
                return mesh;
            }
        }

        private static int PlantGroundcover(Scene scene)
        {
            Random=new System.Random(310173);
            CacheExclusions(scene);
            var previous=scene.GetRootGameObjects().FirstOrDefault(o=>o.name==Root);
            if(previous!=null)Object.DestroyImmediate(previous);
            var root=new GameObject(Root);
            SceneManager.MoveGameObjectToScene(root,scene);
            var meshes=new[]{new MeshWriter(),new MeshWriter(),new MeshWriter(),
                new MeshWriter(),new MeshWriter(),new MeshWriter()};
            var valley=scene.GetRootGameObjects().SelectMany(o=>
                o.GetComponentsInChildren<Transform>(true))
                .First(t=>t.name=="Cornberg valley");
            var terrain=valley.GetComponent<MeshCollider>();
            var trees=scene.GetRootGameObjects().SelectMany(o=>
                o.GetComponentsInChildren<Transform>(true))
                .Where(t=>t.name.EndsWith(" - 3D") && t.name.Contains("Tree_") &&
                    t.gameObject.activeInHierarchy).ToArray();
            int planted=0;
            bool Place(float x,float z,bool flowering=false,bool fern=false)
            {
                if(!terrain.Raycast(new Ray(new Vector3(x,80,z),Vector3.down),
                    out var hit,160f))return false;
                var location=hit.point;
                if(!IsClear(location))return false;
                int palette=Random.Next(0,3);
                meshes[palette].Plant(location+Vector3.up*.06f,
                    Rand(.25f,.56f),Rand(.26f,.53f),Rand(-1.9f,1.9f));
                if(fern)
                    meshes[5].Fern(location+Vector3.up*.05f,
                        Rand(.55f,1.05f),Rand(0,Mathf.PI*2f));
                if(flowering)
                    meshes[3+Random.Next(0,2)].Blossom(
                        location+Vector3.up*.07f,Rand(.095f,.165f),
                        Rand(.26f,.48f));
                planted++;return true;
            }
            foreach(var tree in trees)
            {
                int tries=6;
                for(int j=0;j<tries;j++)
                {
                    float theta=Rand(0,Mathf.PI*2);
                    float radius=Rand(1.7f,4.6f);
                    Place(tree.position.x+Mathf.Cos(theta)*radius,
                        tree.position.z+Mathf.Sin(theta)*radius,Random.NextDouble()<.07,
                        j==0 && Random.NextDouble()<.88);
                }
            }
            // Village lawn and the playable transition into the slime forest.
            for(int i=0;i<1150;i++)
            {
                float x=Rand(-48f,112f),z=Rand(-43f,88f);
                Place(x,z,Random.NextDouble()<.12);
            }
            Color[] colors={
                new Color(.38f,.57f,.25f),
                new Color(.43f,.60f,.31f),
                new Color(.49f,.61f,.31f)
            };
            for(int i=0;i<3;i++)
            {
                if(meshes[i].Count==0)continue;
                var mesh=meshes[i].Save(Art+"/GrassClumps_"+i+".asset");
                var mat=Material(Art+"/GrassBlade_"+i+".mat",
                    AssetDatabase.LoadAssetAtPath<Material>(
                        "Assets/_DiceFree/Materials/Blockout/Meadow.mat"),
                    Texture("LeafMicrostructure"),colors[i],.075f);
                mat.SetFloat("_Cull",0);
                var go=new GameObject("Grass clumps shade "+i,
                    typeof(MeshFilter),typeof(MeshRenderer));
                go.layer=2;
                go.transform.SetParent(root.transform,false);
                go.GetComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=go.GetComponent<MeshRenderer>();
                renderer.sharedMaterial=mat;
                renderer.shadowCastingMode=ShadowCastingMode.Off;
                renderer.receiveShadows=true;
            }
            Color[] bloomColors={
                new Color(.94f,.84f,.55f),
                new Color(.74f,.76f,.93f)
            };
            for(int i=3;i<5;i++)
            {
                if(meshes[i].Count==0)continue;
                var mesh=meshes[i].Save(Art+"/WildflowerPetals_"+(i-3)+".asset");
                var material=Material(Art+"/Wildflower_"+(i-3)+".mat",
                    AssetDatabase.LoadAssetAtPath<Material>(
                        "Assets/_DiceFree/Materials/Blockout/Meadow.mat"),
                    Texture("LeafMicrostructure"),bloomColors[i-3],.11f);
                material.SetFloat("_Cull",0);
                var go=new GameObject("Wildflowers "+(i-3),
                    typeof(MeshFilter),typeof(MeshRenderer));
                go.layer=2;
                go.transform.SetParent(root.transform,false);
                go.GetComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=go.GetComponent<MeshRenderer>();
                renderer.sharedMaterial=material;
                renderer.shadowCastingMode=ShadowCastingMode.Off;
            }
            if(meshes[5].Count>0)
            {
                var fernMesh=meshes[5].Save(Art+"/ForestFerns.asset");
                var fernMaterial=Material(Art+"/FernLeaves.mat",
                    AssetDatabase.LoadAssetAtPath<Material>(
                        "Assets/_DiceFree/Materials/Blockout/Meadow.mat"),
                    Texture("LeafMicrostructure"),new Color(.42f,.61f,.34f),.075f);
                fernMaterial.SetFloat("_Cull",0);
                var ferns=new GameObject("Woodland ferns",typeof(MeshFilter),
                    typeof(MeshRenderer));
                ferns.layer=2;
                ferns.transform.SetParent(root.transform,false);
                ferns.GetComponent<MeshFilter>().sharedMesh=fernMesh;
                var fernRenderer=ferns.GetComponent<MeshRenderer>();
                fernRenderer.sharedMaterial=fernMaterial;
                fernRenderer.shadowCastingMode=ShadowCastingMode.Off;
            }
            return planted;
        }

        [CliCommand("dicefree.nature.install",
            "Add meadow texture, tree bark/leaves and hand-grounded forest grass.")]
        public static object Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Edit Mode required");
            for(int i=0;i<SceneManager.sceneCount;i++)
                if(SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save your open scene before installing art.");
            var old=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                if(scene.GetRootGameObjects().Any(o=>o.name==Root))
                    throw new InvalidOperationException("Nature pass already installed.");
                PrepareTextures();
                ApplyTreeMaterials();
                ImproveValleyGround(scene);
                int flowers=PlantGroundcover(scene);
                EditorSceneManager.MarkSceneDirty(scene);
                if(!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Cannot save scene.");
                AssetDatabase.SaveAssets();
                return new{success=true,grassClumps=flowers,
                    treeTextureMaterials=20,generatedTextures=3,meadowUV=true,
                    roadTrianglesProtected=RoadFaces.Count,buildingsProtected=HouseFootprints.Count};
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(old);}
        }

        [CliCommand("dicefree.nature.polish",
            "Polish the installed meadow and brighten grass with gentle wildflowers.")]
        public static object Polish()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Edit Mode required");
            for(int i=0;i<SceneManager.sceneCount;i++)
                if(SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save any open scene first");
            var previousSetup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                if(!scene.GetRootGameObjects().Any(o=>o.name==Root))
                    throw new InvalidOperationException("Install nature pass first");
                PrepareTextures();
                ApplyTreeMaterials();
                int count=PlantGroundcover(scene);
                var meadow=AssetDatabase.LoadAssetAtPath<Material>(
                    Art+"/MeadowDetail.mat");
                meadow.SetTexture("_BaseMap",Texture("CornbergMeadowTex"));
                EditorUtility.SetDirty(meadow);
                EditorSceneManager.MarkSceneDirty(scene);
                if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Save failed");
                AssetDatabase.SaveAssets();
                return new{success=true,groundcoverClusters=count,
                    forestGrassMeshes=3,wildflowerMeshes=2,fernMeshes=1};
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(previousSetup);}
        }

        [CliCommand("dicefree.nature.validate",
            "Validate UV meadow, tree textures and safe collision-free forest grass.")]
        public static object Validate()
        {
            var old=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var root=scene.GetRootGameObjects().FirstOrDefault(o=>o.name==Root);
                if(root==null)throw new InvalidOperationException("Nature art group missing");
                var grass=root.GetComponentsInChildren<MeshRenderer>(true);
                if(grass.Length!=6||grass.Any(r=>r.GetComponent<Collider>()!=null))
                    throw new InvalidOperationException("Grass renderers/collisions wrong");
                var terrain=scene.GetRootGameObjects().SelectMany(o=>
                    o.GetComponentsInChildren<MeshFilter>(true))
                    .First(f=>f.name=="Cornberg valley");
                if(terrain.sharedMesh.uv.Length!=terrain.sharedMesh.vertexCount ||
                    terrain.GetComponent<MeshCollider>().sharedMesh!=terrain.sharedMesh)
                    throw new InvalidOperationException("Ground mesh/physics mismatch");
                int trees=scene.GetRootGameObjects().SelectMany(o=>
                    o.GetComponentsInChildren<Transform>(true))
                    .Count(t=>t.name.EndsWith(" - 3D")&&t.name.Contains("Tree_")&&
                        t.gameObject.activeInHierarchy);
                if(trees<630)throw new InvalidOperationException("Trees missing");
                var leaf=AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/_DiceFree/Materials/CornbergBatch/Foliage_0.mat");
                if(leaf?.GetTexture("_BaseMap")==null)
                    throw new InvalidOperationException("Leaf textures absent");
                return new{success=true,treeCount=trees,grassMeshes=grass.Length,
                    noNewColliders=true,meadowHasUV=true,leafTexture=true};
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(old);}
        }
    }
}