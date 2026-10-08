using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace DiceFree.EditorTools
{
    /// <summary>
    /// Additive art/geography authoring for the existing Cornberg scene. Does not recreate
    /// the scene, delete quest objects, change player saves, or touch Slime Dungeon interior.
    /// </summary>
    public static class CornbergArtBatchInstall
    {
        private const string Root = "Assets/_DiceFree";
        private const string Models = Root + "/Art/World/CornbergBatch/";
        private const string Materials = Root + "/Materials/CornbergBatch/";
        private static readonly string[] TreeNames = {
            "Tree_Pine", "Tree_Spruce", "Tree_Oak", "Tree_Birch", "Tree_Maple",
            "Tree_Willow", "Tree_Twisted", "Tree_Aspen", "Tree_Cypress"
        };
        private static readonly Dictionary<string, GameObject> Templates = new();
        private static readonly Dictionary<string, Material> SharedMaterials = new();

        [MenuItem("DiceFree/World/Install Cornberg 3D art batch")]
        [CliCommand("dicefree.cornberg.art-install",
            "Apply Blender asset batch and expand northeast slime forest without regenerating Cornberg.",
            Tags = new[] { "art", "world", "cornberg" })]
        public static object Install()
        {
            Templates.Clear(); SharedMaterials.Clear();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            if (!scene.IsValid()) throw new InvalidOperationException("Cornberg scene could not be opened.");
            if (GameObject.Find("Cornberg 3D Art Batch") != null)
                throw new InvalidOperationException("This art batch is already installed; refusing to duplicate it.");

            string[] probes = { "Trees/Tree_Oak", "NPCs/NPC_Farmer", "Slimes/Slime_Crop",
                "Mountain/CornbergAdvancementMountain" };
            foreach (string item in probes) RequireModel(item);
            EnsureFolder(Materials.TrimEnd('/'));

            var root = new GameObject("Cornberg 3D Art Batch").transform;
            var oldTrees = UnityEngine.Object.FindObjectsByType<Transform>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(t => t.name == "Woodland tree")
                .ToArray();

            int treesReplaced = ReplaceTrees(oldTrees);
            ExpandGround();
            int newTrees = ExpandForest(root);
            int slimeVisuals = ReplaceSlimeVisuals();
            int extraSlimes = AddPopulation(root);
            int npcs = AddNpcArt(root);
            AddMountain(root);
            var player = UnityEngine.Object.FindAnyObjectByType<TraversalInput>();
            if (player != null && player.GetComponent<MountainTrialTraveller>() == null)
                player.gameObject.AddComponent<MountainTrialTraveller>();
            CornbergSceneBuilder.Rebake();

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("Could not save Cornberg art batch.");
            AssetDatabase.SaveAssets();
            Debug.Log($"DICEFREE_CORNBERG_ART_INSTALLED: replaced {treesReplaced} trees; " +
                $"added {newTrees} trees, {extraSlimes} slimes, {npcs} NPC models; " +
                $"replaced {slimeVisuals} slime visuals; mountain gate added.");
            return new { success = true, marker = "DICEFREE_CORNBERG_ART_INSTALLED",
                treesReplaced, newTrees, slimeVisuals, extraSlimes, npcs };
        }

        private static GameObject RequireModel(string name)
        {
            if (Templates.TryGetValue(name, out var template)) return template;
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Models + name + ".fbx");
            if (asset == null)
                throw new InvalidOperationException("Blender FBX was not imported: " + name);
            Templates.Add(name, asset);
            return asset;
        }

        private static GameObject PlaceModel(string name, Transform parent, Vector3 worldPosition,
            float scale, int palette = 0, Quaternion? rotation = null)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(RequireModel(name));
            instance.name = name + " - 3D";
            instance.transform.SetParent(parent, true);
            instance.transform.SetPositionAndRotation(worldPosition, rotation ?? Quaternion.identity);
            instance.transform.localScale = Vector3.one * scale;
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                string[] names = renderer.sharedMaterials.Select(m => m == null ? "" : m.name).ToArray();
                var materials = names.Select(n => MaterialFor(n, palette)).ToArray();
                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = name.StartsWith("Slimes/") ?
                    ShadowCastingMode.Off : ShadowCastingMode.On;
                renderer.receiveShadows = !name.StartsWith("Slimes/");
            }
            SetLayerRecursive(instance, 2); // Visual only, original physics remain authoritative.
            return instance;
        }

        private static Material MaterialFor(string exportedName, int palette)
        {
            string key = exportedName.Replace(" (Instance)", "").Replace("DF_", "");
            if (key.EndsWith("_mesh", StringComparison.Ordinal))
                key = key.Substring(0, key.Length - 5);
            string paletteKey = key + "_" + palette;
            if (SharedMaterials.TryGetValue(paletteKey, out var cached)) return cached;
            EnsureFolder(Materials.TrimEnd('/'));
            string safe = paletteKey.Replace(' ', '_').Replace(':', '_');
            string path = Materials + safe + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) throw new InvalidOperationException("URP Lit shader missing.");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            Color col = key switch {
                "Gel" => new Color(.29f,.75f,.40f,.38f),
                "GelInner" => new Color(.60f,.95f,.74f,.50f),
                "Eyes" => new Color(.05f,.07f,.08f,1),
                "Glow" or "SealedLight" => new Color(1f,.81f,.26f,1),
                "Bark" => new Color(.30f,.19f,.10f,1),
                "BarkDeep" => new Color(.15f,.11f,.09f,1),
                "Foliage" => (palette % 5) switch {
                    0 => new Color(.22f,.46f,.18f),
                    1 => new Color(.37f,.55f,.19f),
                    2 => new Color(.22f,.38f,.32f),
                    3 => new Color(.52f,.39f,.19f),
                    _ => new Color(.33f,.50f,.15f) },
                "LeafAccent" => (palette % 5) switch {
                    0 => new Color(.37f,.61f,.25f),
                    1 => new Color(.64f,.69f,.22f),
                    2 => new Color(.24f,.55f,.44f),
                    3 => new Color(.73f,.47f,.22f),
                    _ => new Color(.51f,.72f,.29f) },
                "Skin" => new Color(.76f,.54f,.40f),
                "SkinHighlight" => new Color(.91f,.68f,.53f),
                "Hair" => new Color(.28f,.16f,.10f),
                "ClothPrimary" => (palette % 5) switch {
                    0 => new Color(.32f,.46f,.25f),
                    1 => new Color(.65f,.37f,.22f),
                    2 => new Color(.25f,.34f,.58f),
                    3 => new Color(.42f,.31f,.55f),
                    _ => new Color(.36f,.38f,.40f) },
                "ClothTrim" => new Color(.81f,.75f,.58f),
                "Leather" => new Color(.30f,.18f,.11f),
                "Metal" => new Color(.60f,.63f,.64f),
                "Bronze" => new Color(.68f,.46f,.19f),
                "Rock" => new Color(.38f,.40f,.42f),
                "DeepRock" => new Color(.21f,.23f,.26f),
                "Ledge" => new Color(.49f,.49f,.45f),
                "FloatingCrystals" => new Color(.68f,.82f,.91f),
                "LeafBits" or "Moss" => new Color(.32f,.53f,.22f),
                "Potion" => new Color(.14f,.73f,.53f,.72f),
                _ => new Color(.62f,.60f,.55f)
            };
            material.SetColor("_BaseColor", col);
            material.SetFloat("_Smoothness",
                key == "Gel" || key == "GelInner" ? .86f : .22f);
            bool translucent = key == "Gel" || key == "GelInner" || key == "Potion";
            if (translucent)
            {
                material.SetFloat("_Surface", 1);
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)RenderQueue.Transparent;
            }
            else
            {
                material.SetFloat("_Surface", 0);
                material.SetFloat("_ZWrite", 1);
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = -1;
            }
            if (key == "Glow" || key == "SealedLight" || (key == "Eyes" && palette >= 5))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", col * 1.6f);
            }
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            SharedMaterials[paletteKey] = material;
            return material;
        }

        private static void SetLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                SetLayerRecursive(child.gameObject, layer);
        }

        private static int ReplaceTrees(IEnumerable<Transform> roots)
        {
            int n = 0;
            foreach (var root in roots)
            {
                var trunk = root.Find("Trunk");
                if (trunk == null) continue;
                float size = trunk.lossyScale.y / .3f;
                Vector3 ground = trunk.position - Vector3.up * size * .3f;
                int hash = Mathf.Abs(Mathf.RoundToInt(ground.x * 17f + ground.z * 31f));
                int variety = hash % TreeNames.Length;
                int palette = (hash / 7) % 5;
                string model = "Trees/" + TreeNames[variety];
                float baseHeight = 4.5f;
                float scale = Mathf.Clamp(size / baseHeight, 1.0f, 3f);
                PlaceModel(model, root, ground, scale, palette,
                    Quaternion.Euler(0, hash % 360, 0));
                foreach (var renderer in root.GetComponentsInChildren<Renderer>())
                {
                    if (renderer.gameObject.name.EndsWith(" - 3D")) continue;
                    if (renderer.gameObject.name is "Trunk" or "Canopy" or "Sunward crown")
                        renderer.enabled = false;
                }
                n++;
            }
            return n;
        }

        private static void ExpandGround()
        {
            GameObject ground = GameObject.Find("Cornberg valley");
            if (ground == null) throw new InvalidOperationException("Existing Cornberg valley missing.");
            const int cols = 160, rows = 122;
            var vertices = new Vector3[(cols + 1) * (rows + 1)];
            var triangles = new int[cols * rows * 6];
            for (int z = 0; z <= rows; z++)
            for (int x = 0; x <= cols; x++)
                vertices[z*(cols+1)+x] = CornbergLandscape.Ground(-66 + x*2f,-54 + z*2f);
            int t = 0;
            for (int z = 0; z < rows; z++)
            for (int x = 0; x < cols; x++)
            {
                int i=z*(cols+1)+x;
                triangles[t++]=i; triangles[t++]=i+cols+1; triangles[t++]=i+1;
                triangles[t++]=i+1; triangles[t++]=i+cols+1; triangles[t++]=i+cols+2;
            }
            string meshPath = Models + "CornbergExpandedValley.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh == null)
            {
                mesh = new Mesh { name = "Cornberg Expanded Valley" };
                AssetDatabase.CreateAsset(mesh,meshPath);
            }
            mesh.Clear(); mesh.vertices=vertices;mesh.triangles=triangles;
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            ground.GetComponent<MeshFilter>().sharedMesh=mesh;
            ground.GetComponent<MeshCollider>().sharedMesh=mesh;
            EditorUtility.SetDirty(mesh);
        }

        private static int ExpandForest(Transform root)
        {
            var newForest = new GameObject("Expanded northeast slime woodland").transform;
            newForest.SetParent(root, false);
            var rand = new System.Random(7110);
            int count=0;
            // Existing village buildings remain untouched. New play area extends beyond
            // old 134/82 bounds and is accessible through open southeast woodland paths.
            for (float x=132;x<=236;x+=9f)
            for (float z=18;z<=168;z+=9f)
            {
                Vector2 p = new(x+(float)rand.NextDouble()*4f,z+(float)rand.NextDouble()*4f);
                if (Mathf.Abs(p.x-170)<8 && p.y>50 && p.y<155) continue; // visible forest travel lane
                if (Mathf.Abs(p.x-136)<10 && p.y<85) continue; // accessible join with old clearing
                if (rand.NextDouble()<.26) continue; // irregular openings for encounters
                int hash = Mathf.Abs(Mathf.RoundToInt(p.x*19+p.y*29));
                var model = "Trees/" + TreeNames[hash%TreeNames.Length];
                var pos = CornbergLandscape.Ground(p.x,p.y);
                var tree=PlaceModel(model,newForest,pos,.85f+(float)rand.NextDouble()*.7f,
                    (hash/7)%5,Quaternion.Euler(0,hash%360,0));
                var trunkCollider = tree.AddComponent<CapsuleCollider>();
                trunkCollider.radius=.35f;trunkCollider.height=2.4f;trunkCollider.center=Vector3.up*1.2f;
                tree.layer=9; count++;
            }
            var naturalBoundary = GameObject.Find("07 - Natural boundary - expand northeast from here");
            if (naturalBoundary != null)
            {
                // Old north/east boundary was a temporary stop before world expansion.
                foreach (Transform child in naturalBoundary.transform)
                    if (child.position.x > 126f || child.position.z > 72f)
                        child.gameObject.SetActive(false);
            }
            return count;
        }

        private static int ReplaceSlimeVisuals()
        {
            int count=0;
            foreach (var stats in UnityEngine.Object.FindObjectsByType<ActorStats>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (stats.Definition == null || !stats.Definition.stableId.StartsWith("enemy.")) continue;
                string id=stats.Definition.stableId;
                if (!id.Contains("slime", StringComparison.OrdinalIgnoreCase)) continue;
                string model=id.Contains("crop")?"Slime_Crop":
                    id.Contains("named")?"Slime_Elite":
                    id.Contains("forest")?"Slime_Forest":"Slime_Road";
                var body=stats.transform.Find("Slime body");
                foreach (var renderer in stats.GetComponentsInChildren<Renderer>())
                    if (renderer.gameObject.name is "Slime body" or "Left eye" or "Right eye")
                        renderer.enabled=false;
                // Keep existing body and physics/feedback hierarchy intact.
                PlaceModel("Slimes/"+model,stats.transform,
                    stats.transform.position,1.0f,model=="Slime_Elite"?5:0);
                count++;
            }
            return count;
        }

        private static int AddPopulation(Transform root)
        {
            var template=UnityEngine.Object.FindObjectsByType<ActorStats>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(s=>s.Definition!=null && s.Definition.stableId=="enemy.road-slime");
            if (template==null) throw new InvalidOperationException("Existing road-slime actor missing.");
            var crop=UnityEngine.Object.FindObjectsByType<ActorStats>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(s=>s.Definition!=null && s.Definition.stableId=="enemy.crop-slime");
            if(crop==null) throw new InvalidOperationException("Existing crop-slime actor missing.");

            var actors=new GameObject("Additional live slimes").transform;
            actors.SetParent(root,false);
            int count=0;
            foreach(var p in new[]{new Vector2(44,15),new Vector2(49,18),
                                   new Vector2(43,-13),new Vector2(49,-8)})
            {
                var o=UnityEngine.Object.Instantiate(crop.gameObject);
                o.name="Crop Slime - field "+(count+1);
                o.transform.SetParent(actors,true);
                o.transform.position=CornbergLandscape.Ground(p.x,p.y);
                AttachSlimeVisual(o,"Slime_Crop");
                count++;
            }
            var random=new System.Random(4711);
            for(int i=0;i<23;i++)
            {
                float x=126+(float)random.NextDouble()*106;
                float z=22+(float)random.NextDouble()*140;
                if (Mathf.Abs(x-170)<4 && z>55) x+=9;
                var o=UnityEngine.Object.Instantiate(template.gameObject);
                o.name="Woodland Road Slime - "+(i+1);
                o.transform.SetParent(actors,true);
                o.transform.position=CornbergLandscape.Ground(x,z);
                AttachSlimeVisual(o,i%5==0?"Slime_Forest": "Slime_Road");
                count++;
            }
            return count;
        }

        private static void AttachSlimeVisual(GameObject actor,string kind)
        {
            // Only destroy the root of each previously attached art prefab. Destroying
            // descendants while iterating every Transform leaves destroyed references.
            var oldVisuals = actor.transform.Cast<Transform>()
                .Where(value => value != null &&
                    (value.name.StartsWith("Slimes/", StringComparison.Ordinal) ||
                     value.name.StartsWith("Slime_", StringComparison.Ordinal)))
                .ToArray();
            foreach (var oldVisual in oldVisuals)
                UnityEngine.Object.DestroyImmediate(oldVisual.gameObject);
            foreach (var r in actor.GetComponentsInChildren<Renderer>())
                if (r.gameObject.name is "Slime body" or "Left eye" or "Right eye")r.enabled=false;
            PlaceModel("Slimes/"+kind,actor.transform,actor.transform.position,1f);
        }

        private static int AddNpcArt(Transform root)
        {
            var people=new GameObject("NPC role models - visual kit").transform;
            people.SetParent(root,false);
            var locations = new (string, Vector2, int)[] {
                ("NPC_Farmer",new Vector2(31,17),0),
                ("NPC_Farmer",new Vector2(32,-16),4),
                ("NPC_Merchant",new Vector2(17,-18),1),
                ("NPC_Banker",new Vector2(13,12),2),
                ("NPC_Alchemist",new Vector2(-3,-19),3),
                ("NPC_Blacksmith",new Vector2(-5,10),4),
                ("NPC_Villager",new Vector2(3,7),1),
                ("NPC_Guard",new Vector2(-18,-7),4),
                ("NPC_Herbalist",new Vector2(20,27),0)
            };
            foreach (var (kind, coords, palette) in locations)
                PlaceModel("NPCs/"+kind,people,
                    CornbergLandscape.Ground(coords.x,coords.y),
                    1f,palette,Quaternion.Euler(0,100,0));
            return locations.Length;
        }

        private static void AddMountain(Transform root)
        {
            var old=GameObject.Find("03 - Southwest emergence mountain");
            if(old==null)throw new InvalidOperationException("Existing mountain landmark missing.");
            foreach(var renderer in old.GetComponentsInChildren<Renderer>())
                renderer.enabled=false;
            var mountain = PlaceModel("Mountain/CornbergAdvancementMountain",root,
                new Vector3(-55, CornbergLandscape.Height(-55,-47),-47),
                1.6f,0,Quaternion.Euler(0,180,0));
            mountain.name="Novice Advancement Mountain - 3D gate";
            var gate=mountain.AddComponent<DiceFree.World.MountainAdvancementGate>();
            gate.Configure("NoviceMountainTrial",10);
        }

        private static void EnsureFolder(string path)
        {
            string[] parts=path.Split('/');
            string current=parts[0];
            for(int i=1;i<parts.Length;i++)
            {
                string next=current+"/"+parts[i];
                if(!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current,parts[i]);
                current=next;
            }
        }
    }
}
