using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.AI;
using DiceFree.Briarback;
using DiceFree.Combat;
using DiceFree.EditorTools;
using DiceFree.Items;
using DiceFree.Trimery;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public static partial class ArboriumApproachFix
{
    public const string RidgeName = "Arborium approach - eastward mountain ridge";
    private const string Folder = "Assets/_DiceFree/Art/World/ArboriumRidge";
    private const string PortraitPath = "Assets/_DiceFree/Art/Characters/Dawnwarden/Priest menu presentation.prefab";
    private const float OriginalWestX = 191;

    public static void Batch() => Debug.Log(Run());

    public static void BatchPortrait()
    {
        InstallPriestPortrait();
        AssetDatabase.SaveAssets();
    }

    public static object Run()
    {
        var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
        if (GameObject.Find(RidgeName) != null) throw new InvalidOperationException("The ridge has already been shifted.");
        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();
        var region = GameObject.Find(BriarbackAuthoring.RootName).transform;
        var oldRow = region.GetComponentsInChildren<Transform>(true)
            .Where(t=>t.name=="Wooded cliff boundary" && Mathf.Abs(t.position.x-OriginalWestX)<.1f)
            .OrderBy(t=>t.position.z).ToArray();
        if (oldRow.Length != 30) throw new InvalidOperationException("The original western rock row changed.");

        var rock = FilterBatch(region,"Weathered boundary rock",54,oldRow);
        var strata = FilterBatch(region,"Broken stone strata",36,oldRow);
        foreach (var piece in oldRow) piece.gameObject.SetActive(false);

        var root = new GameObject(RidgeName).transform;
        var south = new[]
        {
            P(217.2f,192.5f), P(218.4f,207), P(220,229), P(219,251),
            P(220.4f,273), P(218.7f,295), P(217.8f,309), P(220.1f,332), P(219,349), P(218,365)
        };
        var north = new[] { P(219,389.8f), P(219.5f,397.3f) };
        AddRidge(root,"Briarback foothills",south,rock.GetComponent<MeshRenderer>().sharedMaterial,
            strata.GetComponent<MeshRenderer>().sharedMaterial,219);
        AddRidge(root,"Northern pass shoulder",north,rock.GetComponent<MeshRenderer>().sharedMaterial,
            strata.GetComponent<MeshRenderer>().sharedMaterial,355);
        var relocated = ClearTrimeryNorthRoad();
        var shiftedCamp = MoveWesternCamp(region);
        var scoutingTrail = ShiftScoutingTrail(region);
        InstallPriestPortrait();

        var route = Object.FindAnyObjectByType<OgreRoadwardenRoute>();
        var serialized = new SerializedObject(route).FindProperty("wreckage");
        var wreck = Enumerable.Range(0,serialized.arraySize)
            .Select(i=>(GameObject)serialized.GetArrayElementAtIndex(i).objectReferenceValue)
            .Where(go=>go.activeSelf).ToArray();
        // Runtime quest obstacles must carve the clear road and leave no permanent hole after repair.
        try
        {
            foreach (var go in wreck) go.SetActive(false);
            CornbergSceneBuilder.Rebake();
            ValidateRoutes();
        }
        finally
        {
            foreach (var go in wreck) go.SetActive(true);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        File.WriteAllText("Logs/ArboriumApproachChanges.json",Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            hidden = oldRow.Select(t=>Id(t.gameObject)).ToArray(),
            moved = relocated.Concat(shiftedCamp.transforms).Select(Id).ToArray(),
            homes = shiftedCamp.homes.Select(Id).ToArray(),
            patrol = Id(shiftedCamp.patrol),
            meshes = new[] {rock,strata}.Concat(scoutingTrail).Select(Id).ToArray()
        }));
        return new { replacedCliffPieces = oldRow.Length, relocatedRoadsideBanks = relocated.Length, priestTier = 2, ridgePass = true };
    }

    private static string Id(Object obj) => GlobalObjectId.GetGlobalObjectIdSlow(obj).targetObjectId.ToString();
    private static Vector3 P(float x,float z) => new(x,0,z);

    private static MeshFilter FilterBatch(Transform region,string name,int stride,Transform[] row)
    {
        var filter = region.GetComponentsInChildren<MeshFilter>().Single(f=>f.name==name);
        var source = filter.sharedMesh;
        var vertices = source.vertices;
        if (vertices.Length%stride != 0 || !source.triangles.SequenceEqual(Enumerable.Range(0,vertices.Length)))
            throw new InvalidOperationException("Unexpected rock batch topology for "+name);
        var remove = new HashSet<int>();
        for(int start=0;start<vertices.Length;start+=stride)
        {
            var center = Vector3.zero;
            for(int i=start;i<start+stride;i++) center += filter.transform.TransformPoint(vertices[i]);
            center /= stride;
            if (Mathf.Abs(center.x-OriginalWestX)<.17f && row.Any(t=>Mathf.Abs(t.position.z-center.z)<.17f))
                remove.Add(start/stride);
        }
        if (remove.Count<24 || remove.Count>30) throw new InvalidOperationException("Failed to identify the original rock row in "+name+": "+remove.Count);
        var kept = Enumerable.Range(0,vertices.Length).Where(i=>!remove.Contains(i/stride)).ToArray();
        var mesh = NewMesh(name+" away from old approach");
        mesh.vertices = kept.Select(i=>vertices[i]).ToArray();
        var normals = source.normals;
        mesh.normals = kept.Select(i=>normals[i]).ToArray();
        var uv = source.uv;
        if(uv.Length==vertices.Length) mesh.uv = kept.Select(i=>uv[i]).ToArray();
        mesh.triangles = Enumerable.Range(0,kept.Length).ToArray();
        mesh.RecalculateBounds();
        EditorUtility.SetDirty(mesh);
        filter.sharedMesh = mesh;
        return filter;
    }

    private static Mesh NewMesh(string name)
    {
        string path=Folder+"/"+name+".asset";
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(mesh==null){mesh=new Mesh{name=name};AssetDatabase.CreateAsset(mesh,path);}
        mesh.Clear();
        mesh.indexFormat=IndexFormat.UInt32;
        return mesh;
    }

    private static Vector3 Ground(Vector3 point)
    {
        if(!Physics.Raycast(point+Vector3.up*20,Vector3.down,out var hit,40,1<<8,QueryTriggerInteraction.Ignore))
            throw new InvalidOperationException("The moved ridge needs ground at "+point);
        return hit.point;
    }

    private static Transform[] ClearTrimeryNorthRoad()
    {
        var banks = GameObject.Find("Wooded banks flanking the highway").transform;
        var tail = banks.Cast<Transform>().Where(t=>t.position.x>223).OrderBy(t=>t.position.x).ToArray();
        if(tail.Length!=7) throw new InvalidOperationException("Trimery's highway bank layout changed.");
        for(int i=0;i<tail.Length;i++) tail[i].position = Ground(P(224.7f+i%2*1.15f,207+i*5.6f));
        return tail;
    }

    private static void InstallPriestPortrait()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<MenuPresentationCatalog>("Assets/_DiceFree/Resources/MenuPresentationCatalog.asset");
        var classes = AssetDatabase.LoadAssetAtPath<DiceFree.Progression.ClassCatalog>("Assets/_DiceFree/Settings/Progression/Class catalog.asset");
        var priest = classes.Resolve("class.priest");
        if(priest==null) throw new InvalidOperationException("The Priest class definition is missing.");
        var preview = new GameObject("Priest menu presentation");
        try
        {
            preview.AddComponent<ActorStats>().Configure(priest);
            preview.AddComponent<CarriedInventory>().Configure(catalog.items);
            preview.AddComponent<Equipment>();
            preview.AddComponent<EquipmentPresentation>();
            var visual = new GameObject("Class visual");
            visual.transform.SetParent(preview.transform,false);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_DiceFree/Art/Characters/Dawnwarden/Dawnwarden.fbx");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model,visual.transform);
            // Keep the imported rig's centimeter-scale idle pose inside the portrait camera.
            instance.transform.localScale *= .01f;
            var animator = instance.GetComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/_DiceFree/Resources/PlayerMotion/TierTwo/Priest.controller");
            animator.applyRootMotion = false;
            var skins = instance.GetComponentsInChildren<SkinnedMeshRenderer>();
            if(animator.avatar==null||!animator.avatar.isValid||skins.Length<2)
                throw new InvalidOperationException("The saved Dawnwarden model cannot be framed.");
            foreach(var skin in skins) skin.updateWhenOffscreen=true;
            var prefab = PrefabUtility.SaveAsPrefabAsset(preview,PortraitPath);
            if(prefab==null) throw new InvalidOperationException("Could not save the Priest preview.");
            catalog.forms = catalog.forms.Where(form=>form.classId!="class.priest").Append(
                new MenuPresentationCatalog.Form {classId="class.priest",tier=2,preview=prefab}).ToArray();
            EditorUtility.SetDirty(catalog);
        }
        finally { Object.DestroyImmediate(preview); }
    }
}

public static partial class ArboriumApproachFix
{
    private static void AddRidge(Transform parent,string name,Vector3[] spine,Material stone,Material seams,int seed)
    {
        var random = new System.Random(seed);
        var stations = new List<Vector3>{spine[0]};
        for(int i=1;i<spine.Length;i++)
        {
            int sections = Mathf.CeilToInt(Vector3.Distance(spine[i-1],spine[i])/3.8f);
            for(int j=1;j<=sections;j++) stations.Add(Vector3.Lerp(spine[i-1],spine[i],(float)j/sections));
        }

        float[] across = {-4.3f,-3.4f,-1.8f,.2f,2.15f,3.95f,5.7f};
        float[] heights = {-.25f,1.33f,3.45f,7.9f,5.15f,2.2f,-.25f};
        var sectionPoints = new Vector3[stations.Count,across.Length];
        for(int i=0;i<stations.Count;i++)
        {
            var center = stations[i];
            float fade = i==0 || i==stations.Count-1 ? .60f : .91f + (float)random.NextDouble()*.32f;
            for(int j=0;j<across.Length;j++)
            {
                var point = center+Vector3.right*(across[j]+((float)random.NextDouble()-.5f)*.82f);
                float scale = j==0||j==across.Length-1 ? 1 : fade*(.82f+(float)random.NextDouble()*.36f);
                sectionPoints[i,j] = Ground(point)+Vector3.up*heights[j]*scale;
            }
        }

        var vertices = new List<Vector3>();
        var rock = new List<int>();
        var layers = new List<int>();
        void Face(Vector3 a,Vector3 b,Vector3 c,List<int> indices)
        {
            int start = vertices.Count;
            vertices.AddRange(new[]{a,b,c});
            indices.AddRange(new[]{start,start+1,start+2});
        }
        for(int i=0;i<stations.Count-1;i++)
        for(int j=0;j<across.Length-1;j++)
        {
            var a = sectionPoints[i,j];
            var b = sectionPoints[i,j+1];
            var c = sectionPoints[i+1,j];
            var d = sectionPoints[i+1,j+1];
            var material = j is 1 or 4 && (i+j)%3!=1 ? layers : rock;
            if((i+j)%2==0) {Face(a,c,b,material); Face(b,c,d,rock);}
            else {Face(a,c,d,rock); Face(a,d,b,material);}
        }

        foreach(int end in new[]{0,stations.Count-1})
        for(int j=0;j<across.Length-1;j++)
        {
            var a=sectionPoints[end,j];var b=sectionPoints[end,j+1];
            var lowA=Ground(a)-Vector3.up*.28f;var lowB=Ground(b)-Vector3.up*.28f;
            if(end==0) {Face(lowA,a,b,rock);Face(lowA,b,lowB,rock);}
            else {Face(lowA,b,a,rock);Face(lowA,lowB,b,rock);}
        }

        var mesh = NewMesh(name);
        mesh.SetVertices(vertices);
        mesh.subMeshCount=2;
        mesh.SetTriangles(rock,0);
        mesh.SetTriangles(layers,1);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.SetPreBakeCollisionMesh(false,true);
        EditorUtility.SetDirty(mesh);
        var obj = new GameObject(name);
        obj.transform.SetParent(parent,false);
        obj.layer=9;
        obj.AddComponent<MeshFilter>().sharedMesh=mesh;
        obj.AddComponent<MeshRenderer>().sharedMaterials=new[]{stone,seams};
        obj.AddComponent<MeshCollider>().sharedMesh=mesh;
    }

    private static void ValidateRoutes()
    {
        var nav = NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            Vector3 At(float x,float z)
            {
                if(!NavMesh.SamplePosition(Ground(P(x,z)),out var hit,1.4f,1))
                    throw new InvalidOperationException($"No navigation at ({x}, {z}).");
                return hit.position;
            }
            float Distance(Vector3 a,Vector3 b)
            {
                var path = new NavMeshPath();
                if(!NavMesh.CalculatePath(a,b,1,path)||path.status!=NavMeshPathStatus.PathComplete)
                    throw new InvalidOperationException($"No connection from {a} to {b}.");
                return path.corners.Zip(path.corners.Skip(1),Vector3.Distance).Sum();
            }
            var eastGate = At(184,305);
            var across = At(231,305);
            float gateDetour = Distance(eastGate,across);
            if(!NavMesh.Raycast(eastGate,across,out _,1)||gateDetour<105)
                throw new InvalidOperationException("Players can still cross directly between Briarback and the east gate.");
            float trimeryNorth = Distance(At(248,192),At(248,222));
            if(trimeryNorth>40)
                throw new InvalidOperationException("The Trimery north trail has a detour at the gate: "+trimeryNorth);
            float pass = Distance(At(229,378),At(204,383));
            if(pass>37)
                throw new InvalidOperationException("The northern pass is blocked: "+pass);
            Distance(At(204,383),At(98,377));
            float repaired = Distance(At(217,180),eastGate);
            if(repaired>155)
                throw new InvalidOperationException("The later repaired highway was cut off by the new ridge: "+repaired);
            Debug.Log($"Arborium routes verified. East gate detour {gateDetour:0.0}m, Trimery north {trimeryNorth:0.0}m, pass {pass:0.0}m, repaired highway {repaired:0.0}m.");
        }
        finally { nav.Remove(); }
    }
}

public static partial class ArboriumApproachFix
{
    private static (Transform[] transforms,AggroBehaviour[] homes,QuilboarRoadPatrol patrol) MoveWesternCamp(Transform region)
    {
        var camp=region.Find("Western spear camp");
        var patrol=region.GetComponentsInChildren<QuilboarRoadPatrol>().Single(value=>value.name=="Western road patrol");
        var actors=region.GetComponentsInChildren<CombatActor>().Where(actor=>
            actor.Faction!=0 && actor.transform.position.x<218 && actor.transform.position.z>286 &&
            actor.transform.position.z<329 && !actor.transform.IsChildOf(patrol.transform)).ToArray();
        if(camp==null||actors.Length<3) throw new InvalidOperationException("The western camp or its quillboars are missing.");
        var shift=Vector3.right*39;
        camp.position += shift + Vector3.up*(Ground(P(249,309)).y-Ground(P(210,309)).y);
        var groupShift=shift + Vector3.up*(Ground(patrol.transform.position+shift).y-patrol.transform.position.y);
        patrol.transform.position += groupShift;
        var homes = new List<AggroBehaviour>();
        foreach(var actor in actors)
        {
            actor.transform.position=Ground(actor.transform.position+shift);
            var aggro=actor.GetComponent<AggroBehaviour>();
            if(aggro==null)continue;
            aggro.ConfigureHomePosition(actor.transform.position);
            homes.Add(aggro);
        }
        foreach(var member in patrol.Members)
        {
            var aggro=member.GetComponent<AggroBehaviour>();
            aggro.ConfigureHomePosition(patrol.transform.position);
            homes.Add(aggro);
        }
        patrol.Configure(patrol.Members,patrol.RouteTargets.Select(target=>Ground(target+shift)).ToArray(),
            patrol.MinimumStopSeconds,patrol.MaximumStopSeconds);
        return (actors.Select(actor=>actor.transform).Concat(new[]{camp,patrol.transform}).ToArray(),homes.ToArray(),patrol);
    }

    private static MeshFilter[] ShiftScoutingTrail(Transform region)
    {
        var tracks=region.GetComponentsInChildren<MeshFilter>().Where(f=>
            f.name is "Briarback western scouting loop" or "Briarback western scouting loop worn surface").ToArray();
        if(tracks.Length!=2) throw new InvalidOperationException("The west scouting track and its surface must move together.");
        foreach(var filter in tracks)
        {
            var source=filter.sharedMesh;
            var mesh=NewMesh(filter.name+" along eastern foothill");
            var vertices=source.vertices;
            for(int i=0;i<vertices.Length;i++)
            {
                var old=filter.transform.TransformPoint(vertices[i]);
                float z=old.z;
                float shift=z<271 ? Mathf.SmoothStep(0,14,Mathf.InverseLerp(264,271,z)) :
                    z<295 ? Mathf.Lerp(14,24,Mathf.InverseLerp(271,295,z)) :
                    Mathf.SmoothStep(24,0,Mathf.InverseLerp(295,303.5f,z));
                if(shift<.01f) continue;
                var moved=old+Vector3.right*shift;
                moved.y+=Ground(moved).y-Ground(old).y;
                vertices[i]=filter.transform.InverseTransformPoint(moved);
            }
            mesh.vertices=vertices;
            if(source.uv.Length==vertices.Length)mesh.uv=source.uv;
            mesh.triangles=source.triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            filter.sharedMesh=mesh;
        }
        return tracks;
    }
}
