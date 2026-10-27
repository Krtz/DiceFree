using System;
using System.Linq;
using DiceFree.Core;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DiceFree.EditorTools
{
    /// <summary>Creates a small, navigable logging edge east of Trimery.</summary>
    public static class TrimeryForestEdgeAuthoring
    {
        const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        const string RootName = "Eastern timber harvest edge v1";
        const string PalisadeName = "Believable log palisade with three guarded openings";
        const string GateName = "EAST - Forester's cart gate";

        static T[] All<T>(Scene scene) where T : Component => scene.GetRootGameObjects()
            .SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();
        static void Need(bool ok, string message)
        {
            if (!ok) throw new InvalidOperationException("TRIMERY FOREST EDGE: " + message);
        }

        [CliCommand("dicefree.trimery.forest-edge.install",
            "Open Trimery's east logging gate and add a stump clearing with forest farther out.")]
        public static object Install()
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "Stop Play Mode first.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                Need(!SceneManager.GetSceneAt(i).isDirty, "Save modified scenes first.");

            var prior = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                var town = All<Transform>(scene).SingleOrDefault(t =>
                    t.name == TrimeryVillageAuthoring.RootName);
                Need(town != null, "Trimery is missing from Cornberg.");
                var existing = town.Find(RootName);

                var palisade = town.Find(PalisadeName);
                Need(palisade != null, "Trimery east palisade is missing.");
                var eastPosts = palisade.Cast<Transform>().Where(t =>
                    t.gameObject.activeSelf &&
                    t.name == "Trimery pointed timber palisade" &&
                    t.position.x > 275f && Mathf.Abs(t.position.z - 180f) <= 5.2f).ToArray();
                if (existing == null)
                    Need(eastPosts.Length >= 6, "Could not identify the east wall section for the cart opening.");

                var materials = "Assets/_DiceFree/Materials/Blockout/";
                var timber = AssetDatabase.LoadAssetAtPath<Material>(materials + "Timber.mat");
                var leaf = AssetDatabase.LoadAssetAtPath<Material>(materials + "Forest.mat");
                var leafLight = AssetDatabase.LoadAssetAtPath<Material>(materials + "Sunlit foliage.mat");
                var path = AssetDatabase.LoadAssetAtPath<Material>(materials + "Warm earth.mat");
                Need(timber != null && leaf != null && leafLight != null && path != null,
                    "Expected existing Trimery fallback materials are missing.");
                BlockoutShapes.Timber = timber;
                BlockoutShapes.Leaf = leaf;
                BlockoutShapes.LeafLight = leafLight;
                BlockoutShapes.Path = path;

                var edge = existing;
                bool refresh = edge != null;
                if (edge == null)
                {
                    edge = new GameObject(RootName).transform;
                    Undo.RegisterCreatedObjectUndo(edge.gameObject, "Create Trimery forest edge");
                    edge.SetParent(town, false);

                    foreach (var post in eastPosts)
                    {
                        Undo.RecordObject(post.gameObject, "Open Trimery logging gate");
                        post.gameObject.SetActive(false);
                        EditorUtility.SetDirty(post.gameObject);
                    }
                    // Remove only the short brace pieces at the new opening.
                    foreach (var brace in palisade.Cast<Transform>().Where(t =>
                        t.gameObject.activeSelf && t.name == "Iron-banded wall brace" &&
                        t.position.x > 275f && Mathf.Abs(t.position.z - 180f) <= 2.8f))
                    {
                        Undo.RecordObject(brace.gameObject, "Open Trimery logging gate");
                        brace.gameObject.SetActive(false);
                        EditorUtility.SetDirty(brace.gameObject);
                    }

                    var gate = BlockoutShapes.Group(GateName, edge);
                    for (int side = -1; side <= 1; side += 2)
                    {
                        float z = 180f + side * 5.1f;
                        var ground = CornbergLandscape.Ground(276f, z);
                        BlockoutShapes.Shape("Timber gate support", PrimitiveType.Cylinder,
                            ground + Vector3.up * 2.05f, new Vector3(.46f, 2.05f, .46f),
                            timber, gate, false);
                    }
                    var lintel = CornbergLandscape.Ground(276f, 180f);
                    BlockoutShapes.Box("Timber gate lintel",
                        lintel + Vector3.up * 4.45f, new Vector3(.7f, .35f, 11f), timber, gate, false);
                    BlockoutShapes.Label("EAST - FORESTER'S ROAD",
                        lintel + Vector3.up * 5.35f, gate);

                    var track = BlockoutShapes.Group("Open timber cart track", edge);
                    CornbergLandscape.Trail("Eastern timber cart trail",
                        new[] { new Vector2(273, 180), new Vector2(285, 180),
                            new Vector2(301, 180) }, 5.2f, track);
                }
                else
                {
                    Need(edge.Find(GateName) != null && edge.Find("Open timber cart track") != null,
                        "Existing logging gate/track is incomplete; refusing a partial refresh.");
                    foreach (var generated in new[] { "Recently cut stumps", "Forest beyond the cut" })
                    {
                        var priorGroup = edge.Find(generated);
                        if (priorGroup != null) Undo.DestroyObjectImmediate(priorGroup.gameObject);
                    }
                }

                var stumps = BlockoutShapes.Group("Recently cut stumps", edge);
                var stumpPositions = new[] {
                    new Vector2(279,158), new Vector2(283,162), new Vector2(288,158), new Vector2(291,163),
                    new Vector2(280,167), new Vector2(285,169), new Vector2(290,166),
                    new Vector2(281,174), new Vector2(287,174), new Vector2(280,186),
                    new Vector2(285,186), new Vector2(291,187), new Vector2(279,194),
                    new Vector2(284,197), new Vector2(290,193), new Vector2(281,202),
                    new Vector2(287,204), new Vector2(292,200), new Vector2(288,207),
                    new Vector2(277,198), new Vector2(291,158), new Vector2(283,191),
                    new Vector2(276,169), new Vector2(289,201)
                };
                foreach (var point in stumpPositions)
                    CutStump(point, stumps, timber, path);
                LogPieces(new Vector2(289,177), stumps, timber, path);
                LogPieces(new Vector2(289,184), stumps, timber, path);

                var forest = BlockoutShapes.Group("Forest beyond the cut", edge);
                // Irregular groups sit beyond the clear-cut band. Keep the center
                // corridor open so carts can continue east into the timber source.
                var trees = new[] {
                    new Vector2(298,152), new Vector2(304,158), new Vector2(312,151), new Vector2(320,157),
                    new Vector2(299,164), new Vector2(307,169), new Vector2(315,163), new Vector2(324,170),
                    new Vector2(298,191), new Vector2(306,197), new Vector2(314,190), new Vector2(322,197),
                    new Vector2(300,204), new Vector2(308,211), new Vector2(317,204), new Vector2(326,211),
                    new Vector2(303,147), new Vector2(318,146), new Vector2(327,163), new Vector2(302,216),
                    new Vector2(311,218), new Vector2(320,216), new Vector2(328,202), new Vector2(313,199)
                };
                int treeIndex = 0;
                foreach (var point in trees)
                {
                    var ground = CornbergLandscape.Ground(point.x, point.y);
                    float height = 7.2f + Mathf.Abs(Mathf.Sin(point.x * 2.17f + point.y * .73f)) * 2.4f;
                    var before = forest.childCount;
                    BlockoutShapes.Tree(ground, height, forest, true);
                    var tree = forest.GetChild(before);
                    tree.name = "Woodland tree - logging source " + (++treeIndex).ToString("D2");
                    var renderers = tree.GetComponentsInChildren<Renderer>(true);
                    var adapter = Undo.AddComponent<DetailedTreeVisual>(tree.gameObject);
                    adapter.Configure(treeIndex % 4, renderers,
                        tree.InverseTransformPoint(ground), height);
                    var occluder = Undo.AddComponent<TreeCameraOccluder>(tree.gameObject);
                    EditorUtility.SetDirty(adapter);
                    EditorUtility.SetDirty(occluder);
                    var obstacle = Undo.AddComponent<NavMeshObstacle>(tree.gameObject);
                    obstacle.shape = NavMeshObstacleShape.Capsule;
                    obstacle.center = Vector3.up * .95f;
                    obstacle.radius = .55f;
                    obstacle.height = 1.9f;
                    obstacle.carving = true;
                    EditorUtility.SetDirty(obstacle);
                }

                EditorSceneManager.MarkSceneDirty(scene);
                Need(EditorSceneManager.SaveScene(scene), "Could not save the east forest extension.");
                CornbergSceneBuilder.Rebake();
                AssetDatabase.SaveAssets();
                return new { success = true, marker = "TRIMERY_FOREST_EDGE_INSTALLED",
                    refreshed = refresh,
                    eastWallPostsOpened = eastPosts.Length, gateWidth = 10.2f,
                    stumps = stumpPositions.Length, felledLogClusters = 2,
                    forestTrees = treeIndex, navMeshRebaked = true };
            }
            finally { if (prior.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(prior); }
        }

        [CliCommand("dicefree.trimery.forest-edge.validate",
            "Verify the new east logging gate, clear-cut band, forest belt and walkable cart route.")]
        public static object Validate()
        {
            var prior = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                var town = All<Transform>(scene).SingleOrDefault(t =>
                    t.name == TrimeryVillageAuthoring.RootName);
                Need(town != null, "Trimery is missing.");
                var edge = town.Find(RootName);
                Need(edge != null, "East logging edge has not been installed.");
                var gate = edge.Find(GateName);
                var stumps = edge.Find("Recently cut stumps");
                var forest = edge.Find("Forest beyond the cut");
                Need(gate != null && stumps != null && stumps.Cast<Transform>().Count(t => t.name == "Cut tree stump") >= 24,
                    "Logging gate or stump clearing is incomplete.");
                Need(forest != null && forest.childCount >= 24,
                    "Forest belt beyond the stump clearing is incomplete.");
                var palisade = town.Find(PalisadeName);
                Need(palisade != null && !palisade.Cast<Transform>().Any(t =>
                    t.name == "Trimery pointed timber palisade" &&
                    t.position.x > 275f && Mathf.Abs(t.position.z - 180f) <= 5.2f &&
                    t.gameObject.activeSelf), "East cart opening is still blocked by palisade posts.");
                var navigation = All<WorldNavigation>(scene).FirstOrDefault();
                Need(navigation != null && navigation.Data != null, "World NavMesh data missing.");
                var instance = NavMesh.AddNavMeshData(navigation.Data);
                try
                {
                    Need(Route(new Vector2(248,183), new Vector2(278,180)),
                        "Town cannot route through the east cart gate.");
                    Need(Route(new Vector2(278,180), new Vector2(288,180)),
                        "Cart track to the cut clearing is blocked.");
                    Need(Route(new Vector2(288,180), new Vector2(298,180)),
                        "Cart track cannot reach the forest edge.");
                }
                finally { instance.Remove(); }
                return new { success = true, marker = "TRIMERY_FOREST_EDGE_VALID",
                    stumps = stumps.childCount, trees = forest.childCount,
                    eastGateOpen = true, cartRoutes = 3 };
            }
            finally { if (prior.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(prior); }
        }

        static bool Route(Vector2 from, Vector2 to)
        {
            if (!NavMesh.SamplePosition(CornbergLandscape.Ground(from.x, from.y),
                    out var start, 4f, NavMesh.AllAreas) ||
                !NavMesh.SamplePosition(CornbergLandscape.Ground(to.x, to.y),
                    out var end, 4f, NavMesh.AllAreas)) return false;
            var path = new NavMeshPath();
            return NavMesh.CalculatePath(start.position, end.position,
                NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete;
        }

        static void CutStump(Vector2 point, Transform parent, Material bark, Material cut)
        {
            var ground = CornbergLandscape.Ground(point.x, point.y);
            var stump = BlockoutShapes.Group("Cut tree stump", parent);
            float height = .42f + Mathf.Abs(Mathf.Sin(point.x * 3.17f + point.y)) * .22f;
            float radius = .34f + Mathf.Abs(Mathf.Cos(point.x + point.y * 2.1f)) * .13f;
            BlockoutShapes.Shape("Dark bark", PrimitiveType.Cylinder,
                ground + Vector3.up * height * .5f,
                new Vector3(radius, height * .5f, radius), bark, stump, false);
            BlockoutShapes.Shape("Fresh sawn top", PrimitiveType.Cylinder,
                ground + Vector3.up * (height + .015f),
                new Vector3(radius * .94f, .025f, radius * .94f), cut, stump, false);
        }

        static void LogPieces(Vector2 origin, Transform parent, Material bark, Material cut)
        {
            var group = BlockoutShapes.Group("Felled timber left for hauling", parent);
            for (int i = 0; i < 3; i++)
            {
                var point = new Vector2(origin.x + i * .72f, origin.y + i * .18f);
                var ground = CornbergLandscape.Ground(point.x, point.y);
                var log = BlockoutShapes.Shape("Felled tree trunk", PrimitiveType.Cylinder,
                    ground + Vector3.up * .23f, new Vector3(.25f, .23f, .25f), bark,
                    group, false);
                log.transform.rotation = Quaternion.Euler(0, 0, 90);
                BlockoutShapes.Shape("Cut log end", PrimitiveType.Cylinder,
                    ground + new Vector3(-.48f, .23f, 0),
                    new Vector3(.255f, .025f, .255f), cut, group, false)
                    .transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }
    }
}
