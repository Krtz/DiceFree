using System;
using System.Linq;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
    /// <summary>
    /// Adds the approved circular warband road as a separate visual mesh. The
    /// existing user-authored Briarback root, actors, camps, and quest data are
    /// left intact. All route nodes are preflighted against the saved NavMesh.
    /// </summary>
    public static class BriarbackLoopRoadAuthoring
    {
        private const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string RegionRootName = "14 - Briarback quilboar frontier";
        private const string GroupName = "Briarback circular road - additive";
        private const string RoadName = "Briarback circular warband road";
        private const string MeshPath = "Assets/_DiceFree/Art/Blockout/Briarback circular warband road.asset";

        // The loop skirts the existing camps and returns to the established
        // watchpost approach; points lie inside the already authored frontier.
        private static readonly Vector2[] Loop =
        {
            new(248, 263), new(278, 260), new(307, 275), new(324, 296),
            new(327, 320), new(316, 344), new(297, 361), new(270, 369),
            new(245, 365), new(223, 351), new(208, 333), new(204, 311),
            new(211, 289), new(228, 271), new(248, 263)
        };

        private static void Need(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("BRIARBACK LOOP ROAD: " + message);
        }

        private static Transform RegionRoot()
        {
            var root = GameObject.Find(RegionRootName);
            Need(root != null, "the existing Briarback root is missing; no scene was changed.");
            return root.transform;
        }

        private static NavMeshDataInstance AddNavigation(Scene scene)
        {
            var navigation = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<WorldNavigation>(true))
                .SingleOrDefault();
            Need(navigation != null && navigation.Data != null,
                "the saved World 1 NavMesh is missing; no scene was changed.");
            return NavMesh.AddNavMeshData(navigation.Data);
        }

        private static Material RoadMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/_DiceFree/Art/Creatures/Briarback/Materials/Briarback worn trail.mat");
            Need(material != null, "the existing Briarback trail palette material is missing.");
            return material;
        }

        private static Vector3[] SampleLoop()
        {
            var sampled = new Vector3[Loop.Length];
            for (var i = 0; i < Loop.Length; i++)
            {
                var ground = CornbergLandscape.Ground(Loop[i].x, Loop[i].y);
                Need(NavMesh.SamplePosition(ground, out var hit, 3f, NavMesh.AllAreas),
                    "loop node " + i + " is not on the saved NavMesh: " + Loop[i]);
                sampled[i] = hit.position;
            }

            for (var i = 1; i < sampled.Length; i++)
            {
                var path = new NavMeshPath();
                Need(NavMesh.CalculatePath(sampled[i - 1], sampled[i], NavMesh.AllAreas, path) &&
                     path.status == NavMeshPathStatus.PathComplete,
                    "the NavMesh is disconnected between loop nodes " + (i - 1) + " and " + i + ".");
            }
            return sampled;
        }

        [CliCommand("dicefree.briarback.loop.preview",
            "Preflight the additive circular Briarback road against saved scene and NavMesh state.")]
        public static object Preview()
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            Need(SceneManager.GetActiveScene().path == ScenePath,
                "open the saved Cornberg scene first; no other scene will be loaded.");
            Need(!SceneManager.GetActiveScene().isDirty,
                "save the current scene edits before previewing the route.");
            var nav = AddNavigation(SceneManager.GetActiveScene());
            try
            {
                RegionRoot();
                var points = SampleLoop();
                return new
                {
                    marker = "DICEFRE_BRIARBACK_LOOP_PREFLIGHT_OK",
                    routeNodes = points.Length,
                    points = points.Select(p => p.ToString("F2")).ToArray(),
                    existingRegionPreserved = true
                };
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        [CliCommand("dicefree.briarback.loop.inspect-west-collision",
            "List actual colliders intersecting the failed western Briarback route segment; does not change the scene.")]
        public static object InspectWestCollision()
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            Need(SceneManager.GetActiveScene().path == ScenePath,
                "open the saved Cornberg scene first.");
            var nav = AddNavigation(SceneManager.GetActiveScene());
            try
            {
                var fromGround = CornbergLandscape.Ground(236.39f, 313.75f);
                var toGround = CornbergLandscape.Ground(232f, 304f);
                var from = fromGround + Vector3.up * 0.5f;
                var to = toGround + Vector3.up * 0.5f;
                var delta = to - from;
                var hits = Physics.CapsuleCastAll(from, from + Vector3.up * 1.3f,
                        0.34f, delta.normalized, delta.magnitude,
                        Physics.AllLayers, QueryTriggerInteraction.Ignore)
                    .Where(hit => hit.collider != null && hit.collider.enabled &&
                                  hit.collider.gameObject.activeInHierarchy)
                    .OrderBy(hit => hit.distance)
                    .Select(hit => new
                    {
                        collider = hit.collider.name,
                        hierarchy = Hierarchy(hit.collider.transform),
                        type = hit.collider.GetType().Name,
                        point = hit.point.ToString("F2"),
                        distance = hit.distance,
                        layer = LayerMask.LayerToName(hit.collider.gameObject.layer)
                    }).ToArray();
                var nearby = Physics.OverlapSphere(toGround + Vector3.up * 0.5f, 1.5f,
                        Physics.AllLayers, QueryTriggerInteraction.Ignore)
                    .Where(collider => collider != null && collider.enabled &&
                                       collider.gameObject.activeInHierarchy)
                    .Select(collider => new
                    {
                        collider = collider.name,
                        hierarchy = Hierarchy(collider.transform),
                        type = collider.GetType().Name,
                        position = collider.transform.position.ToString("F2"),
                        layer = LayerMask.LayerToName(collider.gameObject.layer)
                    }).ToArray();
                return new
                {
                    marker = "DICEFRE_BRIARBACK_WEST_COLLISION_INSPECTED",
                    from = fromGround.ToString("F2"),
                    to = toGround.ToString("F2"),
                    castHits = hits,
                    destinationNearby = nearby
                };
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        private static string Hierarchy(Transform value)
        {
            var names = value.name;
            while (value.parent != null)
            {
                value = value.parent;
                names = value.name + "/" + names;
            }
            return names;
        }

        [CliCommand("dicefree.briarback.loop.author",
            "Add the preflighted Briarback circular road to the existing region without rebuilding it.")]
        public static object Author()
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            Need(SceneManager.GetActiveScene().path == ScenePath,
                "open the saved Cornberg scene first; no other scene will be loaded.");
            var scene = SceneManager.GetActiveScene();
            Need(!scene.isDirty, "save the current scene edits before authoring.");
            var region = RegionRoot();
            var existing = region.Find(GroupName);
            if (existing != null)
                return RepairExisting(existing, scene);
            Need(AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath) == null,
                "a mesh already exists at the dedicated road path without its scene group; refusing to overwrite it.");

            var nav = AddNavigation(scene);
            GameObject group = null;
            bool meshCreated = false;
            try
            {
                var points = SampleLoop();
                var pathMaterial = RoadMaterial();
                group = new GameObject(GroupName);
                Undo.RegisterCreatedObjectUndo(group, "Add Briarback circular road");
                group.transform.SetParent(region, false);
                var mesh = new Mesh { name = RoadName };
                AssetDatabase.CreateAsset(mesh, MeshPath);
                meshCreated = true;
                // CornbergLandscape.Trail uses its established terrain heights,
                // width, material, and non-colliding presentation convention.
                CornbergLandscape.Trail(RoadName, Loop, 4.2f, group.transform);
                var roadObject = group.transform.Find(RoadName);
                Need(roadObject != null && roadObject.GetComponent<MeshRenderer>() != null,
                    "Unity did not create the road renderer.");
                roadObject.GetComponent<MeshRenderer>().sharedMaterial = pathMaterial;
                EditorUtility.SetDirty(roadObject.GetComponent<MeshRenderer>());
                var built = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
                Need(built != null && built.vertexCount > 0,
                    "Unity did not create the expected road mesh.");
                EditorUtility.SetDirty(built);
                AssetDatabase.SaveAssets();
                EditorSceneManager.MarkSceneDirty(scene);
                Need(EditorSceneManager.SaveScene(scene), "Unity could not save the additive road.");
                return new
                {
                    marker = "DICEFRE_BRIARBACK_LOOP_AUTHORED",
                    routeNodes = points.Length,
                    roadWidth = 4.2f,
                    meshVertices = built.vertexCount,
                    existingRegionPreserved = true,
                    otherScenesChanged = false,
                    savedScene = ScenePath
                };
            }
            catch
            {
                if (group != null) Undo.DestroyObjectImmediate(group);
                if (meshCreated) AssetDatabase.DeleteAsset(MeshPath);
                AssetDatabase.Refresh();
                throw;
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        [CliCommand("dicefree.briarback.loop.validate",
            "Verify the additive Briarback circular road mesh and its complete NavMesh loop.")]
        public static object Validate()
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            var previous = EditorSceneManager.GetSceneManagerSetup();
            NavMeshDataInstance nav = default;
            try
            {
                var scene = SceneManager.GetActiveScene();
                if (scene.path != ScenePath)
                    scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                var region = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .SingleOrDefault(value => value.name == RegionRootName);
                Need(region != null, "the existing Briarback root is missing.");
                var group = region.Find(GroupName);
                Need(group != null, "the additive circular road has not been authored.");
                var road = group.Find(RoadName);
                Need(road != null && road.GetComponent<MeshFilter>()?.sharedMesh != null,
                    "the road mesh is missing.");
                Need(road.GetComponent<MeshRenderer>()?.sharedMaterial == RoadMaterial(),
                    "the road is not using the existing Briarback trail palette material.");
                Need(road.GetComponent<Collider>() == null,
                    "the visual road must not add a collision surface over the walkable terrain.");
                nav = AddNavigation(scene);
                var points = SampleLoop();
                return new
                {
                    marker = "DICEFRE_BRIARBACK_LOOP_VALID",
                    routeNodes = points.Length,
                    completeNavMeshSegments = points.Length - 1,
                    meshVertices = road.GetComponent<MeshFilter>().sharedMesh.vertexCount,
                    hasCollision = false,
                    existingRegionPreserved = true
                };
            }
            finally
            {
                if (nav.valid) nav.Remove();
                EditorSceneManager.RestoreSceneManagerSetup(previous);
            }
        }

        private static object RepairExisting(Transform group, Scene scene)
        {
            Need(group.Find(RoadName) != null,
                "the existing additive road group is incomplete; refusing to repair or replace it.");
            var road = group.Find(RoadName);
            var renderer = road.GetComponent<MeshRenderer>();
            Need(renderer != null && road.GetComponent<MeshFilter>()?.sharedMesh != null,
                "the existing additive road renderer or mesh is missing; refusing to rebuild it.");
            var material = RoadMaterial();
            bool repaired = renderer.sharedMaterial != material;
            if (repaired)
            {
                Undo.RecordObject(renderer, "Match Briarback circular road palette");
                renderer.sharedMaterial = material;
                EditorUtility.SetDirty(renderer);
            }
            var nav = AddNavigation(scene);
            try
            {
                var points = SampleLoop();
                var mesh = road.GetComponent<MeshFilter>()?.sharedMesh;
                Need(mesh != null && mesh.vertexCount > 0, "the existing road mesh is empty.");
                if (repaired)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    Need(EditorSceneManager.SaveScene(scene), "Unity could not save the road palette repair.");
                }
                return new
                {
                    marker = repaired ? "DICEFRE_BRIARBACK_LOOP_PALETTE_REPAIRED" : "DICEFRE_BRIARBACK_LOOP_ALREADY_VALID",
                    routeNodes = points.Length,
                    meshVertices = mesh.vertexCount,
                    palette = material.name,
                    existingRegionPreserved = true
                };
            }
            finally { if (nav.valid) nav.Remove(); }
        }
    }
}
