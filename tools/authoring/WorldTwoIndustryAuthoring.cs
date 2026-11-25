using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Banking;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Items;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

public static partial class AshfallReachExpansionAuthoring
{
    const string Industry="Assets/_DiceFree/Art/World/Anvilreach/";
    const string IndustryGroup="Anvilreach and the southern biomes";
    static Material masonry,copper,coal,fen,glass,ember;
    static readonly List<Vector2> industryChecks=new();
    static readonly List<Light> industryHearths=new();
    static Transform industryRoot,city;
    static ItemShop.Offer[] industryStock;

    public static void BuildIndustry()
    {
        Directory.CreateDirectory(Industry);AssetDatabase.Refresh();LoadMaterials();LoadRiverTraderStock();industryStock=riverTraderStock;
        var scene=EditorSceneManager.OpenScene(ScenePath);var world=scene.GetRootGameObjects().Single(g=>g.GetComponent<WorldNavigation>()!=null).transform;
        if(world.Find(IndustryGroup)!=null)throw new InvalidOperationException("Industry pass already present; preserve the authored city instead of duplicating it.");
        industryRoot=Group(world,IndustryGroup,Vector3.zero);industryChecks.Clear();industryHearths.Clear();
        masonry=IndustryTint("Warm cut limestone",new(.40f,.39f,.34f));copper=IndustryTint("Weathered copper",new(.23f,.40f,.34f));
        coal=IndustryTint("Coke and iron",new(.075f,.085f,.10f));fen=IndustryTint("Deepmoss jade",new(.055f,.27f,.23f));
        glass=IndustryTint("Volcanic glass",new(.12f,.14f,.19f));ember=IndustryTint("Live foundry coals",new(1,.23f,.035f),true);
        random=new System.Random(11252026);
        int cleared=ClearIndustrySpace(world);
        RecolourIndustryTerrain(world);
        city=Group(industryRoot,"Anvilreach - city of the hearth",Vector3.zero);
        IndustryStreet("Anvilreach crown trade road",AshfallReach.AnvilreachRoad,3.1f);
        IndustryStreet("Anvilreach northern trade road",AshfallReach.AnvilreachNorthRoad,3.1f);
        IndustryStreet("Anvilreach foundry avenue",new[]{new Vector2(43,-205),new(92,-205),new(152,-205)},3.1f);
        IndustryStreet("Anvilreach artisans lane",new[]{new Vector2(53,-175),new(132,-175),new(132,-249),new(54,-249),new(54,-175)},2.1f);
        BuildIndustryWalls();BuildIndustryBuildings();BuildFoundry();BuildMine();BuildIndustryServices(world);DressIndustryBiomes();
        foreach(var p in new[]{new Vector2(40,-199),new(62,-199),new(83,-199),new(104,-199),new(126,-199),new(148,-199),new(86,-151),new(86,-176),new(86,-233),new(86,-267),new(58,-245),new(128,-245)})Lantern(city,Ground(p));
        city.gameObject.AddComponent<FoundryAtmosphere>().Configure(city.GetComponentsInChildren<Light>().Except(industryHearths).ToArray(),industryHearths.ToArray());
        Waypost(city,"ANVILREACH",Ground(38,-198),180,4);Waypost(industryRoot,"COPPERVEIN HIGHLANDS",Ground(49,-120),170,3.5f);
        Waypost(industryRoot,"GLASSWIND BARRENS",Ground(27,-460),150,3.5f);Waypost(industryRoot,"DEEPMOSS FEN",Ground(-136,-389),160,3.5f);
        var nav=IndustryNavigation(world);world.GetComponent<WorldNavigation>().Configure(nav);var instance=NavMesh.AddNavMeshData(nav);
        int relocated=0;
        try
        {
            var start=IndustryNav(AshfallReach.Cinderwatch);
            foreach(var actor in world.GetComponentsInChildren<CombatActor>(true))
            {
                var p=new Vector2(actor.transform.position.x-AshfallReach.Origin.x,actor.transform.position.z-AshfallReach.Origin.z);
                if(actor.GetComponent<AggroBehaviour>()==null||!new Rect(10,-307,180,195).Contains(p))continue;
                var at=IndustryNav(new Vector2(-10-(relocated%3)*7,-151-(relocated/3)*15));actor.transform.position=at;actor.GetComponent<AggroBehaviour>().ConfigureHomePosition(at);relocated++;
            }
            foreach(var route in new[]{AshfallReach.AnvilreachRoad,AshfallReach.AnvilreachNorthRoad,AshfallReach.CrownPath,AshfallReach.SouthernCircuit,AshfallReach.WillowfordRoad})
                foreach(var point in route)IndustryPath(start,point);
            foreach(var point in industryChecks)IndustryPath(start,point);
            foreach(var target in world.GetComponentsInChildren<InteractionTarget>(true))
                if(target.name is "Basalt Needle survey case" or "Ember Crown survey case" or "Expedition Warden Ivara" or "Envoy Tamsin")
                    IndustryPath(start,new(target.ApproachPosition.x-AshfallReach.Origin.x,target.ApproachPosition.z-AshfallReach.Origin.z));
        }
        finally{instance.Remove();}
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new IOException("City scene save failed");
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/WorldTwoIndustryAuthoring.json",JsonUtility.ToJson(new IndustryReport{buildings=16,newBiomes=3,checkedApproaches=industryChecks.Count,clearedClutter=cleared,relocatedPatrols=relocated,healing=125,anchor="anchor.anvilreach"},true));
        Debug.Log("DICEFREE_WORLD_TWO_INDUSTRY_OK");
    }
    [Serializable]sealed class IndustryReport{public int buildings,newBiomes,checkedApproaches,clearedClutter,relocatedPatrols,healing;public string anchor;}
    public static void RefineIndustry()
    {
        LoadMaterials();var scene=EditorSceneManager.OpenScene(ScenePath);var world=scene.GetRootGameObjects().Single(g=>g.GetComponent<WorldNavigation>()!=null).transform;
        industryRoot=world.Find(IndustryGroup);city=industryRoot.Find("Anvilreach - city of the hearth");
        for(int i=0;i<5;i++){city.Find("Anvilreach artisan "+(i+1)).localPosition=Ground(58+i*15,-246);city.Find("Anvilreach artisan "+(i+1)+" approach").localPosition=Ground(58+i*15,-249);}
        foreach(var t in industryRoot.Cast<Transform>())
        {
            if(!t.name.StartsWith("Library - ")||t.name.Contains("rock_")||t.name.Contains("pine_")||t.name.Contains("mushroom_"))continue;
            if(t.Find("Industry prop footprint")!=null)continue;
            var rr=t.GetComponentsInChildren<Renderer>();var bounds=rr[0].bounds;foreach(var r in rr)bounds.Encapsulate(r.bounds);
            float size=Mathf.Min(bounds.size.x,Mathf.Min(bounds.size.z,1.8f));Solid(t,"Industry prop footprint",new(0,.5f,0),new(size,1,size));
        }
        var nav=IndustryNavigation(world);world.GetComponent<WorldNavigation>().Configure(nav);var instance=NavMesh.AddNavMeshData(nav);
        try
        {
            var start=IndustryNav(AshfallReach.Cinderwatch);
            foreach(var npc in city.GetComponentsInChildren<InteractionTarget>())IndustryPath(start,new(npc.ApproachPosition.x-AshfallReach.Origin.x,npc.ApproachPosition.z-AshfallReach.Origin.z));
            foreach(var road in new[]{AshfallReach.AnvilreachRoad,AshfallReach.AnvilreachNorthRoad,AshfallReach.CrownPath,AshfallReach.SouthernCircuit})foreach(var p in road)IndustryPath(start,p);
        }
        finally{instance.Remove();}
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Debug.Log("DICEFREE_INDUSTRY_REFINEMENT_OK");
    }
    public static void PolishIndustry()
    {
        LoadMaterials();var scene=EditorSceneManager.OpenScene(ScenePath);var world=scene.GetRootGameObjects().Single(g=>g.GetComponent<WorldNavigation>()!=null).transform;
        industryRoot=world.Find(IndustryGroup);city=industryRoot.Find("Anvilreach - city of the hearth");
        masonry=Load<Material>(Industry+"Warm cut limestone.mat");copper=Load<Material>(Industry+"Weathered copper.mat");coal=Load<Material>(Industry+"Coke and iron.mat");ember=Load<Material>(Industry+"Live foundry coals.mat");
        if(city.Find("Anvilreach paved exchange")!=null)throw new Exception("City polish already authored");
        var paving=IndustryPavingMaterial();IndustryPaving("Anvilreach paved exchange",new Rect(76,-221,32,33),paving);
        IndustryPaving("Anvilreach paved main avenue",new Rect(31,-209,137,8),paving);IndustryPaving("Anvilreach paved guild road",new Rect(88,-281,8,144),paving);
        foreach(var h in new[]{(44f,-164f),(64f,-165f),(110f,-163f),(126f,-163f),(41f,-238f),(155f,-252f),(125f,-267f),(156f,-213f),(67f,-220f),(105f,-220f)})
            IndustryHouse("Anvilreach courtyard workshop "+h.Item1+" "+h.Item2,new(h.Item1,h.Item2),8,6,0);
        var cloth=IndustryTint("Indigo market canvas",new(.18f,.27f,.39f));
        foreach(var p in new[]{new Vector2(73,-193),new(73,-214),new(107,-193)})
        {
            var stall=Group(city,"Metalworkers market stall",Ground(p));
            foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})Part(stall,"Timber stall upright",new(x*2.3f,1.6f,z*1.4f),new(.17f,3.3f,.17f),oak);
            Part(stall,"Canvas awning",new(0,3.2f,0),new(5.3f,.13f,3.5f),cloth,rotation:Quaternion.Euler(0,0,7));
            Part(stall,"Sales counter",new(0,.9f,0),new(4.3f,1,1.4f),cedar);Solid(stall,"Market counter footprint",new(0,.8f,0),new(4.3f,1.6f,1.4f));
            for(int i=0;i<6;i++)Part(stall,"Stacked copper ingot",new(-1.5f+i*.6f,1.48f,0),new(.45f,.2f,.65f),i%2==0?bronze:coal);
            IndustryProp("barrel_01",p+new Vector2(3.1f,0),1.2f);
        }
        foreach(var f in city.Cast<Transform>().Where(t=>t.name.StartsWith("Working blast furnace")))
        {
            f.Find("Glowing open hearth").localPosition=new(0,1.3f,-2.45f);f.Find("Foundry firelight").localPosition=new(0,1.4f,-3);
            foreach(int side in new[]{-1,1})Part(f,"Firebrick arch pier",new(side*1.55f,1.35f,-2.5f),new(.6f,2.9f,.6f),masonry);
            Part(f,"Firebrick hearth lintel",new(0,2.8f,-2.5f),new(3.7f,.5f,.6f),masonry);
            for(int i=0;i<3;i++)Part(f,"Riveted iron binding ring",new(0,1+i*1.8f,0),new(5,.12f,5),copper,PrimitiveType.Cylinder);
        }
        for(int i=0;i<3;i++)
        {
            var bench=Group(city,"Smiths outdoor workbench",Ground(116+i*12,-216));Part(bench,"Wood worktop",new(0,1,0),new(3,.2f,1.3f),oak);
            foreach(int side in new[]{-1,1})Part(bench,"Iron trestle",new(side*1.1f,.5f,0),new(.2f,1,1.1f),coal);Solid(bench,"Workbench collision",new(0,.55f,0),new(3,1.1f,1.3f));
            for(int j=0;j<3;j++)Part(bench,"Finished iron bar",new(-.8f+j*.7f,1.17f,0),new(.4f,.12f,.8f),copper);
        }
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);RefineIndustry();Debug.Log("DICEFREE_INDUSTRY_POLISH_OK");
    }
    static Material IndustryPavingMaterial()
    {
        const int n=256;var tex=new Texture2D(n,n,TextureFormat.RGB24,false);
        for(int y=0;y<n;y++)for(int x=0;x<n;x++){int row=y/32;float dx=(x+(row%2)*16)%32,dy=y%32;float grout=Mathf.Min(Mathf.Min(dx,31-dx),Mathf.Min(dy,31-dy));float noise=Mathf.PerlinNoise(x*.16f,y*.16f);var c=Color.Lerp(new Color(.12f,.14f,.14f),new Color(.37f,.39f,.37f)*(.85f+noise*.25f),Mathf.SmoothStep(0,1,grout/3));tex.SetPixel(x,y,c);}tex.Apply();string path=Industry+"Weathered city cobbles.png";File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);var material=new Material(slate){name="Weathered city cobbles",color=Color.white};material.SetTexture("_BaseMap",Load<Texture2D>(path));return Save(material,Industry+material.name+".mat");
    }
    static void IndustryPaving(string name,Rect rect,Material material)
    {
        int nx=Mathf.CeilToInt(rect.width),nz=Mathf.CeilToInt(rect.height);var v=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();
        for(int z=0;z<=nz;z++)for(int x=0;x<=nx;x++){var p=rect.position+new Vector2(x/(float)nx*rect.width,z/(float)nz*rect.height);v.Add(Ground(p)+Vector3.up*.18f);uv.Add(p*.35f);if(x<nx&&z<nz){int i=z*(nx+1)+x;indices.AddRange(new[]{i,i+nx+1,i+1,i+1,i+nx+1,i+nx+2});}}
        var mesh=new Mesh{name=name};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();Surface(city,name,Save(mesh,Industry+name+".asset"),material);
    }
    public static void FinishIndustry()
    {
        var scene=EditorSceneManager.OpenScene(ScenePath);var world=scene.GetRootGameObjects().Single(g=>g.GetComponent<WorldNavigation>()!=null).transform;
        var group=world.Find(IndustryGroup);int removed=0;
        var forestMaterials=new Dictionary<Material,Material>();
        foreach(var t in world.GetComponentsInChildren<Transform>(true).ToArray())
        {
            if(t==null||t.IsChildOf(group))continue;
            var p=new Vector2(t.position.x-AshfallReach.Origin.x,t.position.z-AshfallReach.Origin.z);
            if(t.name=="Fractured basalt"&&AshfallReach.AnvilreachBounds.Contains(p)){Object.DestroyImmediate(t.gameObject);removed++;continue;}
            if(t.GetComponent<TreeCameraOccluder>()==null)continue;
            if(AshfallReach.GlasswindWeight(p)>.5f){Object.DestroyImmediate(t.gameObject);continue;}
            if(AshfallReach.DeepmossWeight(p)<.5f)continue;
            foreach(var renderer in t.GetComponentsInChildren<Renderer>())
            {
                var materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {
                    var source=materials[i];if(source==null)continue;
                    if(!forestMaterials.TryGetValue(source,out var tinted))
                    {tinted=new Material(source){name="Deepmoss forest material "+forestMaterials.Count};tinted.color=Color.Lerp(source.color,new Color(.10f,.26f,.22f),.65f);tinted=Save(tinted,Industry+tinted.name+".mat");forestMaterials.Add(source,tinted);}
                    materials[i]=tinted;
                }
                renderer.sharedMaterials=materials;
            }
        }
        foreach(var t in group.Cast<Transform>().Where(t=>t.name.StartsWith("Library - anvil")))foreach(var r in t.GetComponentsInChildren<Renderer>())r.sharedMaterial=Load<Material>(Industry+"Coke and iron.mat");
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Directory.CreateDirectory("Logs");File.WriteAllText("Logs/WorldTwoIndustryFinal.json","{\"buildings\":26,\"biomes\":3,\"furnaces\":3,\"stalls\":3,\"removedDecorativeRoadRocks\":"+removed+"}");
        RefineIndustry(); // Removed old grove trunks must also leave navigation.
        WorldFacePreviewAuthoring.Batch();Debug.Log("DICEFREE_INDUSTRY_FINISHED_OK");
    }
    static Material IndustryTint(string name,Color color,bool emissive=false)
    {var m=new Material(slate){name=name,color=color};if(emissive){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*3);}return Save(m,Industry+name+".mat");}
    public static void RepairIndustryPalette()
    {
        LoadMaterials();var scene=EditorSceneManager.OpenScene(ScenePath);var world=scene.GetRootGameObjects().Single(g=>g.GetComponent<WorldNavigation>()!=null).transform;
        RecolourIndustryTerrain(world);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Debug.Log("DICEFREE_INDUSTRY_PALETTE_REPAIRED_OK");
    }
    static void IndustryStreet(string name,IReadOnlyList<Vector2> route,float halfWidth)
    {
        Road(industryRoot,name,route,halfWidth);var node=industryRoot.Find(name);node.gameObject.AddComponent<TravelSurface>().Configure("travel.anvilreach."+name,
            Load<TravelSurfaceDefinition>("Assets/_DiceFree/Settings/Travel/Cornberg dirt road.asset"),node.GetComponent<MeshFilter>(),new Bounds(Vector3.zero,Vector3.one*10000));
    }
    static int ClearIndustrySpace(Transform world)
    {
        int count=0;var clearance=new Rect(20,-293,162,168);
        foreach(var t in world.GetComponentsInChildren<Transform>(true).ToArray())
        {
            if(t==null||t.IsChildOf(industryRoot)||t.GetComponentInParent<CombatActor>()!=null)continue;
            bool tree=t.GetComponent<TreeCameraOccluder>()!=null || PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)&&PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject).Contains("/Trees/");
            bool rock=t.name is "Fractured ridge stone" or "Slate ridge";
            if(!tree&&!rock)continue;
            var p=new Vector2(t.position.x-AshfallReach.Origin.x,t.position.z-AshfallReach.Origin.z);
            bool road=new[]{AshfallReach.AnvilreachRoad,AshfallReach.AnvilreachNorthRoad}.Any(r=>DistanceToRoad(p,r)<7);
            if(clearance.Contains(p)||road){Object.DestroyImmediate(t.gameObject);count++;}
        }
        return count;
    }
    static void RecolourIndustryTerrain(Transform world)
    {
        var renderer=world.Find("Slate ridges and alpine heath").GetComponent<MeshRenderer>();var old=renderer.sharedMaterial;
        // The original palette is a native Texture2D .asset, not PNG bytes.
        // Copy via the GPU so non-readable library textures work as well.
        var original=Load<Texture2D>(Output+"Heath fading into volcanic soil.asset");
        var source=new Texture2D(original.width,original.height,TextureFormat.RGBA32,false);
        var copy=RenderTexture.GetTemporary(original.width,original.height,0,RenderTextureFormat.ARGB32);var active=RenderTexture.active;
        try{Graphics.Blit(original,copy);RenderTexture.active=copy;source.ReadPixels(new Rect(0,0,original.width,original.height),0,0);source.Apply();}
        finally{RenderTexture.active=active;RenderTexture.ReleaseTemporary(copy);}
        const int size=2048;var texture=new Texture2D(size,size,TextureFormat.RGB24,false);var pixels=new Color32[size*size];
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {
            var p=AshfallReach.LocalBounds.position+new Vector2((x+.5f)/size*AshfallReach.FaceWidth,(y+.5f)/size*AshfallReach.FaceHeight);
            float noise=Mathf.PerlinNoise(p.x*.11f+71,p.y*.11f+38),grain=.83f+noise*.24f;
            var c=source.GetPixelBilinear((x+.5f)/size,(y+.5f)/size);
            c=Color.Lerp(c,new Color(.40f,.235f,.12f)*grain,AshfallReach.CopperveinWeight(p)*.85f);
            c=Color.Lerp(c,new Color(.42f,.43f,.39f)*grain,AshfallReach.GlasswindWeight(p)*.88f);
            c=Color.Lerp(c,new Color(.085f,.235f,.18f)*grain,AshfallReach.DeepmossWeight(p)*.9f);
            if(AshfallReach.AnvilreachBounds.Contains(p))c=Color.Lerp(c,new Color(.32f,.29f,.245f)*grain,.65f);
            pixels[y*size+x]=c;
        }
        texture.SetPixels32(pixels);texture.Apply();string path=Industry+"World Two industrial biomes.png";File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);Object.DestroyImmediate(source);
        AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.wrapMode=TextureWrapMode.Clamp;importer.maxTextureSize=2048;importer.SaveAndReimport();
        var material=new Material(old){name="World Two five biomes"};material.SetTexture("_BaseMap",Load<Texture2D>(path));renderer.sharedMaterial=Save(material,Industry+material.name+".mat");
    }
    static void IndustryWall(Vector2 a,Vector2 b)
    {
        var d=b-a;int n=Mathf.CeilToInt(d.magnitude/5);float yaw=Mathf.Atan2(d.x,d.y)*Mathf.Rad2Deg;
        for(int i=0;i<n;i++)
        {
            var p=Vector2.Lerp(a,b,(i+.5f)/n);var wall=Group(city,"Anvilreach masonry curtain",Ground(p));wall.localRotation=Quaternion.Euler(0,yaw,0);
            Part(wall,"Basalt lower wall",new(0,1.8f,0),new(1.7f,4.6f,d.magnitude/n+.1f),masonry);Solid(wall,"City perimeter collision",new(0,1.8f,0),new(1.7f,4.6f,d.magnitude/n+.1f));
            Part(wall,"Copper coping",new(0,4.17f,0),new(2,.25f,d.magnitude/n+.15f),copper);
            for(int j=0;j<3;j++)Part(wall,"Stone battlement",new(0,4.55f,(j-1)*1.6f),new(1.9f,.65f,.9f),masonry);
        }
    }
    static void BuildIndustryWalls()
    {
        IndustryWall(new(30,-136),new(86,-136));IndustryWall(new(98,-136),new(170,-136));IndustryWall(new(170,-136),new(170,-282));
        IndustryWall(new(30,-136),new(30,-200));IndustryWall(new(30,-210),new(30,-282));IndustryWall(new(30,-282),new(86,-282));IndustryWall(new(98,-282),new(170,-282));
        foreach(var p in new[]{new Vector2(30,-136),new(170,-136),new(30,-282),new(170,-282)})
        {var tower=Group(city,"Copper-roofed watchtower",Ground(p));Part(tower,"Octagonal tower",new(0,3,0),new(7,3.5f,7),masonry,PrimitiveType.Cylinder);Solid(tower,"Tower footprint",new(0,3,0),new(6.5f,7,6.5f));Part(tower,"Copper crown",new(0,6.8f,0),new(8,.5f,8),copper,PrimitiveType.Cylinder);}
        foreach(var gate in new[]{(new Vector2(30,-205),90f),(new Vector2(92,-136),0f),(new Vector2(92,-282),0f)})
        {var g=Group(city,"Anvilreach open gate",Ground(gate.Item1));g.localRotation=Quaternion.Euler(0,gate.Item2,0);Part(g,"Riveted lintel",new(0,5.4f,0),new(13,1.3f,2.4f),coal);foreach(int side in new[]{-1,1})Part(g,"Copper gate badge",new(side*4.8f,5.4f,-1.25f),new(.8f,.8f,.2f),copper,rotation:Quaternion.Euler(0,0,45));industryChecks.Add(gate.Item1);}
    }
    static void IndustryHouse(string name,Vector2 p,float w,float d,float yaw,bool tall=false)
    {
        Cabin(city,name,p.x,p.y,w,d,yaw,copper);var house=city.Find(name);if(tall)house.localScale=new(1,1.45f,1);
        foreach(var r in house.GetComponentsInChildren<Renderer>())
        {if(r.name is "Cedar walls" or "Lapped front boards" or "Lapped rear boards")r.sharedMaterial=masonry;if(r.name=="Stone chimney")r.sharedMaterial=coal;}
    }
    static void BuildIndustryBuildings()
    {
        IndustryHouse("Miners Guildhall",new(65,-239),18,12,0,true);IndustryHouse("Copper Kettle inn",new(65,-151),17,11,180,true);
        IndustryHouse("Exchange and counting house",new(116,-187),15,10,180,true);IndustryHouse("Anvilreach foundry hall",new(145,-235),20,13,90,true);
        IndustryHouse("Metalworkers cooperative",new(60,-186),14,10,180);IndustryHouse("Ore assayers office",new(146,-150),13,9,180);
        IndustryHouse("Smiths supply hall",new(117,-151),12,9,180);IndustryHouse("Grain and coal store",new(148,-265),14,11,0);
        IndustryHouse("Miners bunkhouse west",new(46,-265),12,9,0);IndustryHouse("Miners bunkhouse east",new(68,-266),12,9,0);
        IndustryHouse("Apprentices house",new(111,-267),12,9,0);IndustryHouse("Healers hall",new(111,-233),11,10,0);
        IndustryHouse("Toolmaker workshop",new(45,-222),10,9,90);IndustryHouse("Cartwright workshop",new(151,-186),12,9,180);
        IndustryHouse("Watch barracks",new(46,-151),10,9,180);IndustryHouse("Stonecutters lodge",new(45,-186),10,9,180);
    }
    static Transform IndustryProp(string model,Vector2 p,float size,float yaw=0,Material tint=null)
    {
        var root=Group(industryRoot,"Library - "+model,Ground(p));root.localRotation=Quaternion.Euler(0,yaw,0);
        var go=(GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(Art+"VisualBatch01/Models/"+model+".fbx"),root);PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        foreach(var c in go.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);
        var rr=go.GetComponentsInChildren<Renderer>();var bounds=rr[0].bounds;foreach(var r in rr)bounds.Encapsulate(r.bounds);go.transform.localScale*=size/Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));bounds=rr[0].bounds;foreach(var r in rr){bounds.Encapsulate(r.bounds);if(tint!=null)r.sharedMaterial=tint;}
        go.transform.position+=new Vector3(root.position.x-bounds.center.x,root.position.y-bounds.min.y,root.position.z-bounds.center.z);foreach(var t in go.GetComponentsInChildren<Transform>())t.gameObject.layer=2;
        return root;
    }
    static void BuildFoundry()
    {
        for(int i=0;i<3;i++)
        {
            var p=new Vector2(126+i*12,-220);var furnace=Group(city,"Working blast furnace "+i,Ground(p));
            Part(furnace,"Iron furnace shell",new(0,2.8f,0),new(4.8f,3,4.8f),coal,PrimitiveType.Cylinder);Solid(furnace,"Furnace footprint",new(0,2.8f,0),new(4.8f,6,4.8f));
            Part(furnace,"Masonry chimney",new(0,8,0),new(2.1f,7,2.1f),masonry);Part(furnace,"Copper chimney rim",new(0,11.6f,0),new(2.6f,.4f,2.6f),copper);
            Part(furnace,"Glowing open hearth",new(0,1.3f,2.45f),new(2.6f,2,.1f),ember);var fire=Group(furnace,"Foundry firelight",new(0,1.4f,3)).gameObject.AddComponent<Light>();fire.type=LightType.Point;fire.color=new(1,.32f,.06f);fire.range=12;fire.shadows=LightShadows.None;industryHearths.Add(fire);
            var smoke=Group(furnace,"Chimney smoke",new(0,11.8f,0)).gameObject.AddComponent<ParticleSystem>();var main=smoke.main;main.duration=12;main.loop=true;main.startLifetime=6;main.startSpeed=1.3f;main.startSize=2.6f;main.startColor=new Color(.20f,.20f,.21f,.25f);main.maxParticles=45;main.simulationSpace=ParticleSystemSimulationSpace.World;var emission=smoke.emission;emission.rateOverTime=4;var velocity=smoke.velocityOverLifetime;velocity.enabled=true;velocity.x=.5f;velocity.y=1.1f;velocity.z=.25f;var shape=smoke.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=12;shape.radius=.5f;
            smoke.GetComponent<ParticleSystemRenderer>().sharedMaterial=IndustrySmoke();
            IndustryProp(i%2==0?"anvil_01":"anvil_02",new(124+i*12,-212),2.0f,90);
            for(int j=0;j<4;j++)IndustryProp("crate_01",new(126+i*9+j*1.4f,-239),1.1f,j*12);
        }
        for(int i=0;i<9;i++)IndustryProp("cart_01",new(112+(i%3)*12,-252-(i/3)*4),2.9f,85);
        for(int i=0;i<18;i++)Rock(city,Ground(117+i%6*1.3f,-257-i/6*1.3f)+Vector3.up*.3f,new(1.3f,.65f,1.3f),i%3==0?bronze:coal);
    }
    static Material IndustrySmoke()
    {
        var path=Industry+"Soft chimney smoke.mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(path);if(existing!=null)return existing;
        var tex=new Texture2D(64,64,TextureFormat.RGBA32,false);for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=Vector2.Distance(new(x,y),new(31.5f,31.5f))/31.5f;tex.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-d),2)));}tex.Apply();string image=Industry+"Soft smoke.png";File.WriteAllBytes(image,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(image);
        var m=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));m.SetTexture("_BaseMap",Load<Texture2D>(image));m.SetFloat("_Surface",1);m.SetFloat("_SrcBlend",5);m.SetFloat("_DstBlend",10);m.SetFloat("_ZWrite",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3000;m.SetOverrideTag("RenderType","Transparent");return Save(m,path);
    }
    static void BuildMine()
    {
        for(int i=0;i<15;i++){float a=i*.42f;var p=new Vector2(160+Mathf.Sin(a)*7,-161+Mathf.Cos(a)*9);Rock(city,Ground(p)+Vector3.up*2,new(7,Range(5,10),7),i%3==0?bronze:warmRock);}
        var entrance=Group(city,"Coppervein mine gallery",Ground(146,-165));
        foreach(int side in new[]{-1,1}){Part(entrance,"Mine support",new(side*2.4f,2.0f,0),new(.6f,4.5f,.7f),oak);Solid(entrance,"Mine support collision",new(side*2.4f,2,0),new(.6f,4.5f,.7f));}
        Part(entrance,"Heavy mine lintel",new(0,4.1f,0),new(6,.7f,.9f),oak);Part(entrance,"Dark gallery roof",new(0,4.4f,2.3f),new(6,.6f,5),coal);
        for(int i=0;i<14;i++){var p=Ground(146,-166-i*.5f);Part(city,"Ore tram sleeper",p+Vector3.up*.06f,new(2.8f,.12f,.2f),oak);foreach(int side in new[]{-1,1})Part(city,"Ore tram rail",p+new Vector3(side*.85f,.16f,0),new(.1f,.1f,.55f),coal);}
        IndustryProp("cart_02",new(146,-169),2.7f);industryChecks.Add(new(146,-173));
        for(int i=0;i<8;i++)IndustryProp("barrel_02",new(136+(i%4)*1.4f,-158-(i/4)*1.6f),1.2f);
    }
    static GameObject IndustryResident(string name,string id,string model,Vector2 p,Vector2 approach,string text)
    {industryChecks.Add(approach);return Resident(city,name,id,model,p,approach,180,text,"npc.anvilreach.");}
    static void BuildIndustryServices(Transform world)
    {
        var smith=IndustryResident("Master smith Branna","branna","Farmer",new(119,-210),new(119,-207),"Anvilreach lives by its hammers. Ore comes down from Coppervein, the furnaces refine it, and our smiths turn it into useful work. The cooperative trades finished supplies; the miners' guild stands in the southwest quarter.");
        var shop=smith.AddComponent<ItemShop>();shop.Configure(industryStock);shop.ConfigureName("Anvilreach smiths cooperative");shop.ConfigureApproach(city.Find("Master smith Branna approach"));var context=smith.AddComponent<ContextualNpc>();context.Configure("npc.anvilreach.branna",shop,smith.GetComponent<ConversationTarget>());context.ConfigureName("Master smith Branna");context.ConfigureApproach(city.Find("Master smith Branna approach"));
        var banker=IndustryResident("Anvilreach Banker","banker","Villager",new(114,-195),new(114,-198),"Your Echo's bank is shared wherever you travel. The exchange keeps your possessions safe while you work the hills.");Object.DestroyImmediate(banker.GetComponent<ConversationTarget>());var bank=banker.AddComponent<BankInteraction>();bank.ConfigureName("Anvilreach Banker");bank.ConfigureApproach(city.Find("Anvilreach Banker approach"));
        IndustryResident("Mine foreman Dorrin","foreman","Farmer",new(140,-170),new(140,-173),"Coppervein's galleries supply the city's foundry. Keep the tram rails clear. The dark woodland to the southwest is Deepmoss Fen; pale ash and volcanic glass mark Glasswind beyond the southern road.");
        IndustryResident("Anvilreach Innkeeper","innkeeper","Herbalist",new(70,-161),new(70,-164),"The Copper Kettle welcomes travellers. The northern gate leads toward Cinderwatch; the western gate joins the Crown road. Our spring and resurrection dice are beside the central avenue.");
        for(int i=0;i<5;i++)IndustryResident("Anvilreach artisan "+(i+1),"artisan."+i,i%2==0?"Villager":"Farmer",new(58+i*15,-246),new(58+i*15,-249),"The city trades copper, tools, masonry and finished iron. There is always another wagon to unload.");
        var spring=Object.Instantiate(world.GetComponentsInChildren<HealingArea>().First(h=>h.name=="Cinderwatch spring").gameObject,city);spring.name="Anvilreach healing spring";spring.transform.localPosition=Ground(83,-211);spring.GetComponent<HealerInteraction>().ConfigureName("Drink from the Anvilreach spring");spring.GetComponent<HealerInteraction>().ConfigureApproach(Group(city,"Anvilreach spring approach",Ground(80,-211)));industryChecks.Add(new(80,-211));
        var source=world.GetComponentsInChildren<Transform>().Single(t=>t.name=="Willowford resurrection dice");var die=Object.Instantiate(source.gameObject,city);die.name="Anvilreach resurrection dice";var anchor=die.GetComponentInChildren<ResurrectionAnchor>();die.transform.position+=AshfallReach.Origin+Ground(101,-211)-anchor.transform.position;var visual=die.GetComponentInChildren<ResurrectionDiceVisual>();anchor.name="Anvilreach resurrection anchor";anchor.Configure("anchor.anvilreach",visual.WorldPip.transform,4.5f);foreach(var label in die.GetComponentsInChildren<TextMesh>())label.text="Anvilreach";industryChecks.Add(new(101,-211));
    }
    static bool IndustryDressingSafe(Vector2 p)
    {
        if(!AshfallReach.LocalBounds.Contains(p)||p.y>-95||new Rect(15,-300,170,180).Contains(p))return false;
        if(Mathf.Abs(p.x-AshfallReach.RiverX(p.y))<11||Vector2.Distance(p,AshfallReach.EmberCrown)<72||Vector2.Distance(p,AshfallReach.BasaltNeedle)<38)return false;
        return new[]{AshfallReach.CrownPath,AshfallReach.SouthernCircuit,AshfallReach.WesternTrail,AshfallReach.AnvilreachRoad,AshfallReach.AnvilreachNorthRoad}.All(r=>DistanceToRoad(p,r)>6);
    }
    static void DressIndustryBiomes()
    {
        var mushroom=IndustryTint("Luminous fen caps",new(.17f,.63f,.52f),true);
        for(int i=0;i<500;i++)
        {
            var p=new Vector2(Range(-230,178),Range(-566,-100));if(!IndustryDressingSafe(p))continue;
            if(AshfallReach.CopperveinWeight(p)>.6f)
            {IndustryProp("rock_0"+(i%5+1),p,Range(2,5),i*137.5f,i%4==0?bronze:warmRock);if(i%3==0)Rock(industryRoot,Ground(p)+Vector3.up*.6f,new(1.4f,1.4f,.35f),bronze);}
            else if(AshfallReach.GlasswindWeight(p)>.6f)
            {var shard=IndustryProp("rock_0"+(i%5+1),p,Range(1.4f,3.8f),i*47,glass);shard.localScale=new(.65f,Range(1.8f,3.5f),.65f);}
            else if(AshfallReach.DeepmossWeight(p)>.6f)
            {
                var tree=IndustryProp("pine_0"+(i%4+1),p,Range(6,10),i*71,fen);tree.gameObject.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(9);
                for(int j=0;j<3;j++)IndustryProp("mushroom_0"+(j%4+1),p+new Vector2(1.4f+j*.5f,1.1f),Range(.65f,1.25f),j*31,mushroom);
            }
        }
        // Small dark pools sit away from routes; they are solid wet ground, not
        // invisible navigation holes or a substitute for the continuing river.
        foreach(var p in new[]{new Vector2(-121,-407),new(-107,-471),new(-201,-459)})
            for(int j=0;j<4;j++){var at=p+new Vector2(j*1.6f,Mathf.Sin(j)*2);Part(industryRoot,"Deepmoss dark pool",Ground(at)+Vector3.up*.07f,new(5,.05f,3.7f),fen,PrimitiveType.Cylinder);}
    }
    static NavMeshData IndustryNavigation(Transform world)
    {
        Physics.SyncTransforms();var sources=new List<NavMeshBuildSource>();var blockers=new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(world,1<<8,NavMeshCollectGeometry.PhysicsColliders,0,new List<NavMeshBuildMarkup>(),sources);NavMeshBuilder.CollectSources(world,1<<9,NavMeshCollectGeometry.PhysicsColliders,1,new List<NavMeshBuildMarkup>(),blockers);sources.AddRange(blockers);
        var settings=NavMesh.GetSettingsByID(0);settings.overrideVoxelSize=true;settings.voxelSize=.25f;settings.agentSlope=40;settings.agentClimb=.4f;
        var r=AshfallReach.Bounds;var data=NavMeshBuilder.BuildNavMeshData(settings,sources,new Bounds(new(r.center.x,45,r.center.y),new(r.width,140,r.height)),Vector3.zero,Quaternion.identity);
        if(data==null)throw new Exception("Industry navigation failed");return Save(data,Industry+"Anvilreach and five biomes navigation.asset");
    }
    static Vector3 IndustryNav(Vector2 p){var at=AshfallReach.Origin+Ground(p);if(!NavMesh.SamplePosition(at,out var hit,3,1))throw new Exception("Missing industry navigation at "+p);return hit.position;}
    static void IndustryPath(Vector3 from,Vector2 p){var path=new NavMeshPath();if(!NavMesh.CalculatePath(from,IndustryNav(p),1,path)||path.status!=NavMeshPathStatus.PathComplete)throw new Exception("Unreachable industry destination "+p);}
}
