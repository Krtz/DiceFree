using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using static DiceFree.EditorTools.BlockoutShapes;

namespace DiceFree.EditorTools
{
    /// <summary>
    /// Adds the first northern mountain-stronghold blockout as an isolated child
    /// of the user-authored Briarback region. It never invokes the broad region
    /// installer and it does not author boss, quest, or combat behavior.
    /// </summary>
    public static class BriarbackNorthernStrongholdAuthoring
    {
        private const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string RegionRootName = "14 - Briarback quilboar frontier";
        private const string GroupName = "Briarback northern mountain stronghold - additive";
        private const string GroundMeshPath = "Assets/_DiceFree/Art/Blockout/Briarback northern extension ground.asset";
        private const string RoadMeshPath = "Assets/_DiceFree/Art/Blockout/Briarback north approach.asset";
        private const string UpperTerraceMeshPath = "Assets/_DiceFree/Art/Blockout/Briarback upper stronghold terrace.asset";
        private const string RampMeshPath = "Assets/_DiceFree/Art/Blockout/Briarback stronghold ascent ramp.asset";
        private const string BridgeMeshPath = "Assets/_DiceFree/Art/Blockout/Briarback stronghold overpass.asset";
        private const string BridgeRampMeshPath = "Assets/_DiceFree/Art/Blockout/Briarback stronghold bridge ramps.asset";
        private const string NavBackupPath = "Assets/_DiceFree/Settings/Recovery/CornbergNavMesh-before-briarback-stronghold.asset";
        private const string GroundBackupPath = "Assets/_DiceFree/Settings/Recovery/Briarback-north-ground-before-seam-repair.asset";
        private const string ExistingGroundMeshPath = "Assets/_DiceFree/Settings/Briarback/Briarback ground.asset";
        private const string ExistingGroundBackupPath = "Assets/_DiceFree/Settings/Recovery/Briarback-ground-before-northern-expansion.asset";
        private const string OpenArenaTerraceMeshPath = "Assets/_DiceFree/Art/Blockout/Briarback upper open arena terrace.asset";
        private const string OpenArenaSceneBackupPath = "Assets/_DiceFree/Settings/Recovery/Cornberg-before-chieftain-open-arena-20261028.unity";
        private const string OpenArenaMeshBackupPath = "Assets/_DiceFree/Settings/Recovery/Briarback-upper-terrace-before-open-arena-20261028.asset";
        private const string BoundarySceneBackupPath = "Assets/_DiceFree/Settings/Recovery/Cornberg-before-north-boundary-relocation.unity";
        private const string ApproachSceneBackupPath = "Assets/_DiceFree/Settings/Recovery/Cornberg-before-north-approach-opening.unity";
        private const string UnderpassSceneBackupPath = "Assets/_DiceFree/Settings/Recovery/Cornberg-before-underpass-ramp-repair.unity";
        private const string AscentMeshBackupPath = "Assets/_DiceFree/Settings/Recovery/Briarback-stronghold-ascent-before-underpass-repair.asset";
        private const string AscentCutSceneBackupPath = "Assets/_DiceFree/Settings/Recovery/Cornberg-before-ascent-cut-opening.unity";
        private const float West = 190f;
        private const float East = 346f;
        private const float South = 376f;
        private const float North = 538f;
        private const float UpperFloor = 5.2f;

        private static readonly Vector2[] Approach =
        {
            new Vector2(270, 369), new Vector2(272, 390), new Vector2(270, 412),
            new Vector2(270, 431)
        };

        private static void Need(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("BRIARBACK NORTHERN STRONGHOLD: " + message);
        }

        private static Transform RegionRoot(Scene scene)
        {
            var matches = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Where(value => value.name == RegionRootName).ToArray();
            Need(matches.Length == 1, "expected exactly one existing Briarback root; no scene was changed.");
            return matches[0];
        }

        private static Material Material(string name)
        {
            var value = AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Materials/Blockout/" + name + ".mat");
            Need(value != null, "the existing " + name + " blockout material is missing.");
            return value;
        }

        private static Mesh CreateGround(float south, float north)
        {
            const int columns = 78;
            var rows = Mathf.RoundToInt((north - south) / 2f);
            var vertices = new Vector3[(columns + 1) * (rows + 1)];
            var triangles = new int[columns * rows * 6];
            for (var z = 0; z <= rows; z++)
            for (var x = 0; x <= columns; x++)
                vertices[z * (columns + 1) + x] = CornbergLandscape.Ground(West + x * 2, south + z * 2);

            var index = 0;
            for (var z = 0; z < rows; z++)
            for (var x = 0; x < columns; x++)
            {
                var cell = z * (columns + 1) + x;
                triangles[index++] = cell;
                triangles[index++] = cell + columns + 1;
                triangles[index++] = cell + 1;
                triangles[index++] = cell + 1;
                triangles[index++] = cell + columns + 1;
                triangles[index++] = cell + columns + 2;
            }

            var mesh = new Mesh { name = "Briarback northern extension ground" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateExtensionGround() => CreateGround(South, North);

        private static Mesh CreateApproachMesh()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (var segment = 1; segment < Approach.Length; segment++)
            {
                var delta = Approach[segment] - Approach[segment - 1];
                var side = new Vector2(-delta.y, delta.x).normalized * 2.4f;
                var steps = Mathf.CeilToInt(delta.magnitude / 1.5f);
                for (var step = 0; step < steps; step++)
                {
                    var a = Vector2.Lerp(Approach[segment - 1], Approach[segment], step / (float)steps);
                    var b = Vector2.Lerp(Approach[segment - 1], Approach[segment], (step + 1f) / steps);
                    var i = vertices.Count;
                    vertices.Add(CornbergLandscape.Ground(a.x - side.x, a.y - side.y) + Vector3.up * 0.06f);
                    vertices.Add(CornbergLandscape.Ground(a.x + side.x, a.y + side.y) + Vector3.up * 0.06f);
                    vertices.Add(CornbergLandscape.Ground(b.x - side.x, b.y - side.y) + Vector3.up * 0.06f);
                    vertices.Add(CornbergLandscape.Ground(b.x + side.x, b.y + side.y) + Vector3.up * 0.06f);
                    triangles.AddRange(new[] { i, i + 1, i + 2, i + 2, i + 1, i + 3 });
                }
            }
            var mesh = new Mesh { name = "Briarback north approach" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // The same mesh builders are used for the upper terrace, sloped ramps,
        // and raised bridge. All tops are real colliding geometry for the
        // standard NavMesh bake; the bridge's underside remains open to traffic.
        private static Mesh BuildPrism(string name, Vector3 startLeft, Vector3 startRight,
            Vector3 endLeft, Vector3 endRight, float thickness)
        {
            var vertices = new[]
            {
                startLeft, startRight, endLeft, endRight,
                startLeft - Vector3.up * thickness, startRight - Vector3.up * thickness,
                endLeft - Vector3.up * thickness, endRight - Vector3.up * thickness
            };
            var triangles = new[]
            {
                0, 2, 1, 2, 3, 1, // top faces up on flat or sloped walkways
                4, 5, 6, 6, 5, 7, // underside
                0, 4, 1, 1, 4, 5,
                2, 3, 6, 3, 7, 6,
                0, 2, 4, 2, 6, 4,
                1, 5, 3, 3, 5, 7
            };
            var mesh = new Mesh { name = name };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildRamp(string name, Vector3 lowCenter, Vector3 highCenter,
            float width, float thickness)
        {
            var horizontal = new Vector3(highCenter.x - lowCenter.x, 0, highCenter.z - lowCenter.z).normalized;
            var side = Vector3.Cross(Vector3.up, horizontal) * (width * 0.5f);
            return BuildPrism(name, lowCenter - side, lowCenter + side, highCenter - side, highCenter + side, thickness);
        }

        private static Mesh BuildTerrace(string name, Vector2 min, Vector2 max, float elevation, float thickness)
        {
            var a = CornbergLandscape.Ground(min.x, min.y) + Vector3.up * elevation;
            var b = CornbergLandscape.Ground(max.x, min.y) + Vector3.up * elevation;
            var c = CornbergLandscape.Ground(min.x, max.y) + Vector3.up * elevation;
            var d = CornbergLandscape.Ground(max.x, max.y) + Vector3.up * elevation;
            return BuildPrism(name, a, b, c, d, thickness);
        }

        private static Mesh SaveMeshAsset(string path, Mesh mesh)
        {
            Need(AssetDatabase.LoadAssetAtPath<Mesh>(path) == null, "mesh asset already exists: " + path);
            AssetDatabase.CreateAsset(mesh, path);
            var saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            Need(saved != null && saved.vertexCount > 0, "Unity could not save mesh asset: " + path);
            return saved;
        }

        private static GameObject MeshObject(string name, Mesh mesh, Material material, Transform parent, bool collider)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            Undo.RegisterCreatedObjectUndo(go, "Create Briarback northern stronghold blockout");
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            go.layer = collider ? 8 : 2;
            if (collider) go.AddComponent<MeshCollider>().sharedMesh = mesh;
            return go;
        }

        private static GameObject Box(string name, Vector3 position, Vector3 size, Material material, Transform parent)
        {
            var value = Shape(name, PrimitiveType.Cube, position, size, material, parent, true, 9);
            Undo.RegisterCreatedObjectUndo(value, "Create Briarback northern stronghold blockout");
            return value;
        }

        private static GameObject Log(string name, Vector3 position, float height, float diameter, Material material, Transform parent, bool solid)
        {
            var value = Shape(name, PrimitiveType.Cylinder, position, new Vector3(diameter, height * 0.5f, diameter), material, parent, solid, 9);
            if (solid) Undo.RegisterCreatedObjectUndo(value, "Create Briarback northern stronghold blockout");
            return value;
        }

        private static void WallRun(string name, Vector2 start, Vector2 end, Transform parent, Material timber, bool spikes, float floorOffset = 0)
        {
            var distance = Vector2.Distance(start, end);
            var count = Mathf.FloorToInt(distance / 2f);
            var direction = (end - start).normalized;
            for (var i = 0; i <= count; i++)
            {
                var point = start + direction * Mathf.Min(i * 2f, distance);
                var ground = CornbergLandscape.Ground(point.x, point.y) + Vector3.up * floorOffset;
                Log(name + " - timber palisade", ground + Vector3.up * 2.4f, 4.8f, 0.52f, timber, parent, true);
                if (spikes && i % 2 == 0)
                    Log(name + " - sharpened crown", ground + Vector3.up * 5.05f, 1.0f, 0.2f, timber, parent, false);
            }
        }

        private static void Tower(string name, Vector2 position, Transform parent, Material timber, Material stone, float floorOffset = 0)
        {
            var ground = CornbergLandscape.Ground(position.x, position.y) + Vector3.up * floorOffset;
            Box(name + " - fieldstone foot", ground + Vector3.up * 0.65f, new Vector3(7.6f, 1.3f, 7.6f), stone, parent);
            for (var side = -1; side <= 1; side += 2)
            {
                Box(name + " - timber leg", ground + new Vector3(side * 2.7f, 3.3f, -2.7f), new Vector3(0.42f, 5.6f, 0.42f), timber, parent);
                Box(name + " - timber leg", ground + new Vector3(side * 2.7f, 3.3f, 2.7f), new Vector3(0.42f, 5.6f, 0.42f), timber, parent);
            }
            Box(name + " - raised fighting deck", ground + Vector3.up * 6.0f, new Vector3(7.1f, 0.35f, 7.1f), timber, parent);
            Box(name + " - crenel parapet", ground + Vector3.up * 7.0f, new Vector3(7.3f, 1.6f, 0.38f), timber, parent);
        }

        private static void Rock(Vector3 ground, Vector3 scale, Material stone, Transform parent, string name)
        {
            var rock = Shape(name, PrimitiveType.Sphere, ground + Vector3.up * scale.y * 0.38f, scale, stone, parent, true, 9);
            rock.transform.rotation = Quaternion.Euler(0, (ground.x * 13 + ground.z * 7) % 360, 0);
            Undo.RegisterCreatedObjectUndo(rock, "Create Briarback northern stronghold blockout");
        }

        private static object Preflight(Scene scene)
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            Need(scene.path == ScenePath, "the saved Cornberg scene must be active; no other scene will be loaded.");
            Need(!scene.isDirty, "save the user's current scene edits before authoring; no scene was changed.");
            var root = RegionRoot(scene);
            var group = root.Find(GroupName);
            Need(group == null, group != null ? "the additive stronghold already exists." : "");
            Need(AssetDatabase.LoadAssetAtPath<Mesh>(GroundMeshPath) == null,
                "the dedicated ground mesh path is already occupied; refusing to overwrite it.");
            Need(AssetDatabase.LoadAssetAtPath<Mesh>(RoadMeshPath) == null,
                "the dedicated approach mesh path is already occupied; refusing to overwrite it.");
            Need(AssetDatabase.LoadAssetAtPath<Mesh>(UpperTerraceMeshPath) == null &&
                 AssetDatabase.LoadAssetAtPath<Mesh>(RampMeshPath) == null &&
                 AssetDatabase.LoadAssetAtPath<Mesh>(BridgeMeshPath) == null &&
                 AssetDatabase.LoadAssetAtPath<Mesh>(BridgeRampMeshPath) == null,
                "one of the dedicated multilevel geometry paths is already occupied; refusing to overwrite it.");
            var bounds = root.GetComponent<OverworldMapBounds>();
            Need(bounds != null && Mathf.Abs(bounds.Bounds.yMax - 406f) < 0.1f,
                "the saved map bounds differ from the expected Briarback baseline; inspect before extending them.");
            Need(AssetDatabase.LoadAssetAtPath<NavMeshData>("Assets/_DiceFree/Settings/CornbergNavMesh.asset") != null,
                "the current saved NavMesh asset is missing.");
            return new { root = root.name, expectedExistingTerrainNorth = 398f, newTerrainNorth = North, oldMapNorth = bounds.Bounds.yMax, newMapNorth = 545f };
        }

        [CliCommand("dicefree.briarback.north.preview", "Preflight additive northern ground and open stronghold blockout without changing the scene.")]
        public static object Preview()
        {
            var scene = SceneManager.GetActiveScene();
            var baseline = Preflight(scene);
            return new
            {
                marker = "DICEFRE_BRIARBACK_NORTH_PREFLIGHT_OK",
                baseline,
                sceneDirty = scene.isDirty,
                createdObjects = 0,
                preservesExistingScene = true,
                fortGateOpen = true,
                noBossOrQuestAuthored = true
            };
        }

        [CliCommand("dicefree.briarback.north.author", "Extend Briarback north and add the first cliff-backed timber stronghold blockout through Unity Editor APIs.")]
        public static object Author()
        {
            var scene = SceneManager.GetActiveScene();
            var preflight = Preflight(scene);
            var root = RegionRoot(scene);
            var navAssetPath = "Assets/_DiceFree/Settings/CornbergNavMesh.asset";
            var navAsset = AssetDatabase.LoadAssetAtPath<NavMeshData>(navAssetPath);
            var navigation = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<WorldNavigation>(true)).SingleOrDefault();
            Need(navigation != null && navigation.Data == navAsset,
                "the scene's live WorldNavigation must reference the expected saved NavMesh asset.");

            var recoveryDirectory = System.IO.Path.GetDirectoryName(NavBackupPath).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(recoveryDirectory))
            {
                var parent = "Assets/_DiceFree/Settings";
                AssetDatabase.CreateFolder(parent, "Recovery");
            }
            Need(AssetDatabase.LoadAssetAtPath<NavMeshData>(NavBackupPath) == null,
                "the NavMesh recovery asset already exists; refusing to overwrite a backup.");
            Need(AssetDatabase.CopyAsset(navAssetPath, NavBackupPath), "could not create the pre-bake NavMesh recovery copy.");

            var previousBounds = root.GetComponent<OverworldMapBounds>().Bounds;
            GameObject group = null;
            try
            {
                var timber = Material("Timber");
                var stone = Material("Fieldstone");
                var meadow = Material("Meadow");
                var path = Material("Warm earth");
                group = new GameObject(GroupName);
                Undo.RegisterCreatedObjectUndo(group, "Extend Briarback north with mountain stronghold");
                group.transform.SetParent(root, false);

                var groundMesh = CreateExtensionGround();
                AssetDatabase.CreateAsset(groundMesh, GroundMeshPath);
                groundMesh = AssetDatabase.LoadAssetAtPath<Mesh>(GroundMeshPath);
                Need(groundMesh != null, "could not create the dedicated extension ground asset.");
                MeshObject("Continuous walkable northern extension", groundMesh, meadow, group.transform, true);

                var approachMesh = CreateApproachMesh();
                approachMesh = SaveMeshAsset(RoadMeshPath, approachMesh);
                MeshObject("North approach to the open stronghold gate", approachMesh, path, group.transform, false);

                // The lower compound and road stay at valley grade.
                WallRun("West lower palisade", new Vector2(220, 438), new Vector2(220, 469), group.transform, timber, true);
                WallRun("East lower palisade", new Vector2(316, 438), new Vector2(316, 469), group.transform, timber, true);
                WallRun("South palisade - west gate wing", new Vector2(220, 438), new Vector2(257, 438), group.transform, timber, true);
                WallRun("South palisade - east gate wing", new Vector2(279, 438), new Vector2(316, 438), group.transform, timber, true);
                Tower("West gate tower", new Vector2(258, 438), group.transform, timber, stone);
                Tower("East gate tower", new Vector2(278, 438), group.transform, timber, stone);

                // A 5.2m upper shelf is formed as a colliding solid mesh; a
                // narrow, sub-20-degree stone ramp passes through the cliff lip.
                var terrace = SaveMeshAsset(UpperTerraceMeshPath,
                    BuildTerrace("Briarback upper stronghold terrace", new Vector2(234, 474), new Vector2(306, 502), UpperFloor, 0.9f));
                MeshObject("Raised upper stronghold terrace", terrace, stone, group.transform, true);
                for (var x = 220; x < 262; x += 6)
                    Box("Natural cliff face below upper terrace", CornbergLandscape.Ground(x + 3, 472) + Vector3.up * 2.6f,
                        new Vector3(6.2f, 5.2f, 0.9f), stone, group.transform);
                for (var x = 278; x < 316; x += 6)
                {
                    if (Mathf.Abs(x + 3 - 293f) < 0.1f) continue;
                    Box("Natural cliff face below upper terrace", CornbergLandscape.Ground(x + 3, 472) + Vector3.up * 2.6f,
                        new Vector3(6.2f, 5.2f, 0.9f), stone, group.transform);
                }
                WallRun("West upper palisade", new Vector2(234, 474), new Vector2(234, 497), group.transform, timber, true, UpperFloor);
                WallRun("East upper palisade", new Vector2(306, 474), new Vector2(306, 497), group.transform, timber, true, UpperFloor);
                WallRun("North upper palisade", new Vector2(234, 497), new Vector2(306, 497), group.transform, timber, true, UpperFloor);
                Tower("Northwest upper watchtower", new Vector2(234, 497), group.transform, timber, stone, UpperFloor);
                Tower("Northeast upper watchtower", new Vector2(306, 497), group.transform, timber, stone, UpperFloor);

                var ascent = SaveMeshAsset(RampMeshPath, BuildRamp("Briarback stronghold ascent ramp",
                    CornbergLandscape.Ground(294, 460) + Vector3.up * UpperFloor,
                    CornbergLandscape.Ground(294, 475.5f) + Vector3.up * UpperFloor, 4.4f, 0.45f));
                MeshObject("Walkable ramp through the cliff cut", ascent, path, group.transform, true);

                // A raised EW timber bridge creates a real over/under route:
                // its colliding deck is 4.65m above the lower road and two
                // 15-degree ramps give access to the upper level.
                var bridgeStart = CornbergLandscape.Ground(242, 460) + Vector3.up * UpperFloor;
                var bridgeEnd = CornbergLandscape.Ground(294, 460) + Vector3.up * UpperFloor;
                var bridge = SaveMeshAsset(BridgeMeshPath,
                    BuildRamp("Briarback stronghold overpass", bridgeStart, bridgeEnd, 5.2f, 0.55f));
                MeshObject("Raised timber bridge deck over lower road", bridge, timber, group.transform, true);
                var ramps = new Mesh
                {
                    name = "Briarback stronghold bridge ramps"
                };
                var westRamp = BuildRamp("west bridge ramp", CornbergLandscape.Ground(242, 441), bridgeStart, 4.2f, 0.45f);
                var eastRamp = BuildRamp("east bridge ramp", bridgeEnd, CornbergLandscape.Ground(294, 441), 4.2f, 0.45f);
                // Store both ramp meshes as sub-assets in one dedicated file;
                // each stays a separate collider and preserves its slope.
                AssetDatabase.CreateAsset(ramps, BridgeRampMeshPath);
                AssetDatabase.AddObjectToAsset(westRamp, ramps);
                AssetDatabase.AddObjectToAsset(eastRamp, ramps);
                ramps.name = "Briarback stronghold bridge ramps";
                AssetDatabase.SaveAssets();
                Need(AssetDatabase.LoadAllAssetsAtPath(BridgeRampMeshPath).OfType<Mesh>().Count() == 3,
                    "Unity did not persist both bridge ramp mesh sub-assets.");
                MeshObject("West ramp to upper timber bridge", westRamp, path, group.transform, true);
                MeshObject("East ramp to upper timber bridge", eastRamp, path, group.transform, true);
                Box("West bridge support pier", CornbergLandscape.Ground(242, 460) + Vector3.up * 2.3f,
                    new Vector3(2.2f, 4.6f, 2.2f), stone, group.transform);
                Box("East bridge support pier", CornbergLandscape.Ground(294, 460) + Vector3.up * 2.3f,
                    new Vector3(2.2f, 4.6f, 2.2f), stone, group.transform);
                Box("Bridge north parapet", bridgeStart + Vector3.forward * 2.45f + Vector3.up * 0.8f,
                    new Vector3(52, 1.6f, 0.3f), timber, group.transform);
                Box("Bridge south parapet", bridgeStart - Vector3.forward * 2.45f + Vector3.up * 0.8f,
                    new Vector3(52, 1.6f, 0.3f), timber, group.transform);

                // Natural cliff spine closes the northern and side approaches while leaving the road and gate usable.
                for (var i = 0; i < 9; i++)
                {
                    var x = 203 + i * 17.5f;
                    Rock(CornbergLandscape.Ground(x, 510), new Vector3(20, 20 + (i % 3) * 3, 19), stone, group.transform, "Natural northern cliff spine");
                }
                for (var i = 0; i < 5; i++)
                {
                    var z = 450 + i * 11f;
                    Rock(CornbergLandscape.Ground(202, z), new Vector3(13, 14 + i % 2 * 3, 18), stone, group.transform, "West mountain shoulder");
                    Rock(CornbergLandscape.Ground(334, z), new Vector3(13, 14 + i % 2 * 3, 18), stone, group.transform, "East mountain shoulder");
                }

                // Blockout longhouse marks the destination without adding an unapproved boss or encounter logic.
                Box("Chieftain longhouse - blockout foundation", CornbergLandscape.Ground(268, 490) + Vector3.up * (UpperFloor + 0.45f),
                    new Vector3(24, 0.9f, 17), stone, group.transform);
                Box("Chieftain longhouse - timber hall", CornbergLandscape.Ground(268, 490) + Vector3.up * (UpperFloor + 3.7f),
                    new Vector3(19, 6.5f, 13), timber, group.transform);
                Box("Chieftain longhouse - roof blockout", CornbergLandscape.Ground(268, 490) + Vector3.up * (UpperFloor + 7.5f),
                    new Vector3(21, 1.2f, 15), stone, group.transform);

                // The ridge encloses the rear; low non-blocking trees make the expanded north edge read as mountain forest.
                for (var i = 0; i < 18; i++)
                {
                    var x = 198 + i * 8.2f;
                    var z = 525 + (i % 2) * 5f;
                    var ground = CornbergLandscape.Ground(x, z);
                    Log("Northern ridge tree trunk", ground + Vector3.up * 3.4f, 6.8f, 0.65f, timber, group.transform, true);
                    Shape("Northern ridge tree canopy", PrimitiveType.Sphere, ground + Vector3.up * 7.2f,
                        new Vector3(6, 5.8f, 6), Material("Forest"), group.transform, false, 2);
                }

                var mapBounds = root.GetComponent<OverworldMapBounds>();
                Undo.RecordObject(mapBounds, "Extend Briarback exploration bounds north");
                mapBounds.Configure(new Rect(previousBounds.xMin, previousBounds.yMin, previousBounds.width, 615f));
                EditorUtility.SetDirty(mapBounds);

                AssetDatabase.SaveAssets();
                EditorSceneManager.MarkSceneDirty(scene);
                Need(EditorSceneManager.SaveScene(scene), "Unity could not save the additive northern stronghold.");
                return new
                {
                    marker = "DICEFRE_BRIARBACK_NORTH_BLOCKOUT_AUTHORED",
                    scene = ScenePath,
                    previousMapNorth = previousBounds.yMax,
                    newMapNorth = mapBounds.Bounds.yMax,
                    terrain = new { west = West, east = East, south = South, north = North, hasMeshCollider = true },
                    compound = new { west = 220, east = 316, lowerSouth = 438, upperNorth = 497, gateWidth = 22, openGate = true },
                    elevations = new { lower = 0, upper = UpperFloor, bridge = UpperFloor, bridgeUnderpassClearance = 4.65f, upperRampWidth = 4.4f },
                    cliffBacked = true,
                    upperTerraceAndRamp = true,
                    overUnderTimberBridge = true,
                    bossLogicAdded = false,
                    questLogicAdded = false,
                    navigationRequiresRebake = true,
                    navmeshRecoveryCopy = NavBackupPath,
                    preflight
                };
            }
            catch
            {
                var mapBounds = root.GetComponent<OverworldMapBounds>();
                mapBounds.Configure(previousBounds);
                EditorUtility.SetDirty(mapBounds);
                if (group != null) Undo.DestroyObjectImmediate(group);
                foreach (var path in new[] { GroundMeshPath, RoadMeshPath, UpperTerraceMeshPath, RampMeshPath, BridgeMeshPath, BridgeRampMeshPath })
                    if (AssetDatabase.LoadMainAssetAtPath(path) != null) AssetDatabase.DeleteAsset(path);
                AssetDatabase.DeleteAsset(NavBackupPath);
                AssetDatabase.Refresh();
                throw;
            }
        }

        [CliCommand("dicefree.briarback.north.bake", "Rebuild only the existing saved World 1 NavMesh after the northern extension, using the scene's authored Walkable and WorldSolid layers.")]
        public static object BakeNavigation()
        {
            var scene = SceneManager.GetActiveScene();
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            Need(scene.path == ScenePath && !scene.isDirty, "save and activate the completed Cornberg blockout first.");
            Need(RegionRoot(scene).Find(GroupName) != null, "the additive northern stronghold is missing.");
            Need(AssetDatabase.LoadAssetAtPath<NavMeshData>(NavBackupPath) != null,
                "the dedicated pre-bake NavMesh recovery copy is missing; refusing to alter navigation.");
            CornbergSceneBuilder.Rebake();
            var current = AssetDatabase.LoadAssetAtPath<NavMeshData>("Assets/_DiceFree/Settings/CornbergNavMesh.asset");
            Need(current != null, "the existing World 1 NavMesh asset is missing after the scoped bake.");
            return new
            {
                marker = "DICEFRE_BRIARBACK_NORTH_NAVMESH_BAKED",
                baker = "CornbergSceneBuilder.Rebake (Walkable layer 8 + WorldSolid obstacle layer 9)",
                bakedBounds = new { west = -85, east = 353, south = -70, north = 545 },
                existingNavMeshAssetIdentityPreserved = AssetDatabase.AssetPathToGUID("Assets/_DiceFree/Settings/CornbergNavMesh.asset"),
                recoveryCopy = NavBackupPath
            };
        }

        [CliCommand("dicefree.briarback.north.repair-seam", "Extend only the new north ground 22m south to overlap the old terrain, preserving a recovery copy first.")]
        public static object RepairGroundSeam()
        {
            var scene = SceneManager.GetActiveScene();
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            Need(scene.path == ScenePath && !scene.isDirty, "save and activate the completed Cornberg blockout first.");
            var group = RegionRoot(scene).Find(GroupName);
            Need(group != null, "the additive northern stronghold is missing.");
            var groundObject = group.Find("Continuous walkable northern extension");
            Need(groundObject != null, "the extension ground is missing.");
            var meshFilter = groundObject.GetComponent<MeshFilter>();
            var collider = groundObject.GetComponent<MeshCollider>();
            var mesh = meshFilter != null ? meshFilter.sharedMesh : null;
            Need(mesh != null && collider != null, "the ground mesh or collider is missing.");
            var recoveryDirectory = System.IO.Path.GetDirectoryName(GroundBackupPath).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(recoveryDirectory)) AssetDatabase.CreateFolder("Assets/_DiceFree/Settings", "Recovery");
            var groundBackup = AssetDatabase.LoadAssetAtPath<Mesh>(GroundBackupPath);
            if (groundBackup == null)
                Need(AssetDatabase.CopyAsset(GroundMeshPath, GroundBackupPath), "could not create the north-ground recovery copy.");

            var expanded = CreateExtensionGround();
            Undo.RegisterCompleteObjectUndo(mesh, "Overlap Briarback north ground seam");
            mesh.Clear();
            mesh.indexFormat = expanded.indexFormat;
            mesh.vertices = expanded.vertices;
            mesh.triangles = expanded.triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            collider.sharedMesh = null;
            collider.sharedMesh = mesh;
            EditorUtility.SetDirty(mesh);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            Need(EditorSceneManager.SaveScene(scene), "Unity could not save the ground seam repair.");
            return new
            {
                marker = "DICEFRE_BRIARBACK_NORTH_GROUND_SEAM_OVERLAP_REPAIRED",
                oldTerrainNorth = 398,
                newExtensionSouth = South,
                overlapMeters = 398f - South,
                exactHeightContinuity = true,
                recoveryCopy = GroundBackupPath
            };
        }

        [CliCommand("dicefree.briarback.north.inspect-sources", "Read the actual layer-8 NavMesh sources and world bounds at the Briarback extension seam.")]
        public static object InspectSources()
        {
            var scene = SceneManager.GetActiveScene();
            Need(scene.path == ScenePath, "activate the saved Cornberg scene first.");
            var group = RegionRoot(scene).Find(GroupName);
            Need(group != null, "the additive northern stronghold is missing.");
            var ground = RegionRoot(scene).Find("Continuous Briarback walkable terrain");
            Need(ground != null, "the unified Briarback terrain is missing.");
            var collider = ground.GetComponent<MeshCollider>();
            var mesh = ground.GetComponent<MeshFilter>().sharedMesh;
            Physics.SyncTransforms();
            var bakeBounds = new Bounds(new Vector3(134, 18, 237.5f), new Vector3(438, 100, 615));
            var sources = new List<NavMeshBuildSource>();
            NavMeshBuilder.CollectSources(bakeBounds, 1 << 8, NavMeshCollectGeometry.PhysicsColliders, 0,
                new List<NavMeshBuildMarkup>(), sources);
            var collected = sources.Where(source => source.sourceObject == mesh || source.component == collider).ToArray();
            var navigation = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<WorldNavigation>(true)).Single();
            var nav = NavMesh.AddNavMeshData(navigation.Data);
            try
            {
            var seam = Enumerable.Range(391, 18).Where(z => z % 2 == 0).Select(z =>
                {
                    var point = CornbergLandscape.Ground(270, z);
                    var found = NavMesh.SamplePosition(point, out var hit, 2f, NavMesh.AllAreas);
                    return new { z, found, position = found ? hit.position.ToString("F2") : "none" };
                }).ToArray();
                return new
                {
                    marker = "DICEFRE_BRIARBACK_NORTH_NAV_SOURCES_INSPECTED",
                    layer = ground.gameObject.layer,
                    colliderEnabled = collider.enabled,
                    colliderWorldBoundsCenter = collider.bounds.center.ToString("F2"),
                    colliderWorldBoundsSize = collider.bounds.size.ToString("F2"),
                    meshLocalBoundsCenter = mesh.bounds.center.ToString("F2"),
                    meshLocalBoundsSize = mesh.bounds.size.ToString("F2"),
                    meshVertexCount = mesh.vertexCount,
                    objectWorldPosition = ground.position.ToString("F2"),
                    parentWorldPosition = group.position.ToString("F2"),
                    includedInBakeBounds = bakeBounds.Intersects(collider.bounds),
                    totalLayer8Sources = sources.Count,
                    exactGroundSourceCount = collected.Length,
                    sourceBoundsNorth = collected.Select(source =>
                    {
                        if (source.sourceObject is Mesh sourceMesh)
                            return source.transform.MultiplyPoint3x4(new Vector3(sourceMesh.bounds.center.x, sourceMesh.bounds.center.y, sourceMesh.bounds.max.z)).z;
                        return float.NaN;
                    }).ToArray(),
                    navMeshTriangulationMaxZ = NavMesh.CalculateTriangulation().vertices.Length > 0
                        ? NavMesh.CalculateTriangulation().vertices.Max(vertex => vertex.z)
                        : float.NaN,
                    matchingSources = collected.Select(source => new
                    {
                        sourceObject = source.sourceObject != null ? source.sourceObject.name : "null",
                        component = source.component != null ? source.component.name : "null",
                        shape = source.shape.ToString(),
                        area = source.area,
                        position = source.transform.MultiplyPoint3x4(Vector3.zero).ToString("F2")
                    }).ToArray(),
                    seamSamples = seam
                };
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        [CliCommand("dicefree.briarback.north.relocate-boundary", "Move only the confirmed obsolete Briarback North edge to the far side of the new extension, with a scene recovery copy, then rebake navigation.")]
        public static object RelocateBoundary()
        {
            var scene = SceneManager.GetActiveScene();
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            Need(scene.path == ScenePath && !scene.isDirty, "save and activate the completed Cornberg blockout first.");
            var all = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            var boundary = all.SingleOrDefault(t => t.name == "North edge" && t.parent != null && t.parent.name == "Physical outer edge");
            Need(boundary != null, "the exact authored Physical outer edge/North edge object was not found.");
            var obstacle = boundary.GetComponent<NavMeshObstacle>();
            Need(obstacle != null, "the North edge no longer has its expected NavMeshObstacle.");
            Need(Mathf.Abs(boundary.position.z - 400f) < 0.25f && obstacle.size.x >= 150f,
                "the identified obstacle is not the old z=400 northern wall; refusing to move it.");
            Need(AssetDatabase.LoadMainAssetAtPath(BoundarySceneBackupPath) == null,
                "a pre-relocation scene backup already exists; refusing to overwrite it.");
            var folder = System.IO.Path.GetDirectoryName(BoundarySceneBackupPath).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/_DiceFree/Settings", "Recovery");
            Need(EditorSceneManager.SaveScene(scene, BoundarySceneBackupPath, true), "could not create the pre-relocation scene recovery copy.");

            Undo.RecordObjects(new UnityEngine.Object[] { boundary, obstacle }, "Move obsolete northern Briarback boundary");
            boundary.position = new Vector3(268f, 3.6f, North + 7f);
            obstacle.size = new Vector3(160f, 8f, 3.5f);
            EditorUtility.SetDirty(boundary);
            EditorUtility.SetDirty(obstacle);
            Physics.SyncTransforms();
            CornbergSceneBuilder.Rebake();
            Need(EditorSceneManager.SaveScene(scene), "could not save the relocated northern boundary.");
            AssetDatabase.SaveAssets();
            return new
            {
                marker = "DICEFRE_BRIARBACK_NORTH_BOUNDARY_RELOCATED",
                hierarchy = "Physical outer edge/North edge",
                priorCenterZ = 400f,
                newCenterZ = North + 7f,
                navObstacleSize = obstacle.size.ToString("F2"),
                sceneRecoveryCopy = BoundarySceneBackupPath,
                navMeshBakedAfterRelocation = true
            };
        }

        [CliCommand("dicefree.briarback.north.inspect-boundaries", "List scene NavMesh obstacles and colliders near the old northern frontier boundary.")]
        public static object InspectBoundaries()
        {
            var scene = SceneManager.GetActiveScene();
            Need(scene.path == ScenePath, "the Cornberg scene must be active.");
            var query = new Bounds(new Vector3(270f, 1f, 400f), new Vector3(180f, 12f, 24f));
            string Hierarchy(Transform value)
            {
                var names = new Stack<string>();
                for (var t = value; t != null; t = t.parent) names.Push(t.name);
                return string.Join("/", names);
            }
            var colliders = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Collider>(true))
                .Where(collider => collider.enabled && collider.bounds.Intersects(query))
                .Where(collider => collider.name == "North edge" ||
                                   (collider.name == "Northern wooded ridge" && Mathf.Abs(collider.bounds.center.x - 270f) <= 20f))
                .ToArray();
            var obstacleDescriptions = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<NavMeshObstacle>(true))
                .Where(obstacle => obstacle.name == "North edge")
                .Where(obstacle =>
                {
                    var center = obstacle.transform.position;
                    var worldSize = Vector3.Scale(obstacle.size, obstacle.transform.lossyScale);
                    var extents = worldSize * 0.5f;
                    return obstacle.enabled &&
                           Mathf.Abs(center.x - query.center.x) <= extents.x + query.extents.x &&
                           Mathf.Abs(center.y - query.center.y) <= extents.y + query.extents.y &&
                           Mathf.Abs(center.z - query.center.z) <= extents.z + query.extents.z;
                })
                .Select(obstacle => new
                {
                    hierarchy = Hierarchy(obstacle.transform),
                    center = obstacle.transform.position.ToString("F2"),
                    worldSize = Vector3.Scale(obstacle.size, obstacle.transform.lossyScale).ToString("F2"),
                    obstacleSize = obstacle.size.ToString("F2"),
                    shape = obstacle.shape.ToString(),
                    carving = obstacle.carving
                }).ToArray();
            return new
            {
                marker = "DICEFRE_BRIARBACK_NORTH_BOUNDARIES_INSPECTED",
                query = query.ToString(),
                colliders = colliders.Select(collider => new
                {
                    hierarchy = Hierarchy(collider.transform),
                    type = collider.GetType().Name,
                    layer = collider.gameObject.layer,
                    boundsCenter = collider.bounds.center.ToString("F2"),
                    boundsSize = collider.bounds.size.ToString("F2")
                }).ToArray(),
                obstacles = obstacleDescriptions
            };
        }

        [CliCommand("dicefree.briarback.north.inspect-underpass", "List the exact colliders overlapping the player-sized lower-route capsule beneath the bridge.")]
        public static object InspectUnderpass()
        {
            var scene = SceneManager.GetActiveScene();
            Need(scene.path == ScenePath, "the Cornberg scene must be active.");
            var ground = CornbergLandscape.Ground(270, 460);
            var bottom = ground + Vector3.up * 0.55f;
            var top = ground + Vector3.up * 2.1f;
            var hits = Physics.OverlapCapsule(bottom, top, 0.32f, Physics.AllLayers,
                    QueryTriggerInteraction.Ignore)
                .Where(collider => collider != null && collider.enabled && collider.gameObject.activeInHierarchy)
                .Select(collider => new
                {
                    hierarchy = string.Join("/", Ancestors(collider.transform)),
                    type = collider.GetType().Name,
                    layer = LayerMask.LayerToName(collider.gameObject.layer),
                    boundsCenter = collider.bounds.center.ToString("F2"),
                    boundsSize = collider.bounds.size.ToString("F2")
                }).ToArray();
            return new
            {
                marker = "DICEFRE_BRIARBACK_UNDERPASS_COLLIDERS_INSPECTED",
                sampleGround = ground.ToString("F2"),
                capsuleBottom = bottom.ToString("F2"),
                capsuleTop = top.ToString("F2"),
                radius = 0.32f,
                hitCount = hits.Length,
                hits
            };
        }

        [CliCommand("dicefree.briarback.north.inspect-levels", "Inspect baked NavMesh positions across the lower road, bridge, and upper courtyard.")]
        public static object InspectLevels()
        {
            var scene = SceneManager.GetActiveScene();
            Need(scene.path == ScenePath, "the Cornberg scene must be active.");
            var owner = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<WorldNavigation>(true)).Single();
            var instance = NavMesh.AddNavMeshData(owner.Data);
            try
            {
                var points = new[]
                {
                    (label: "lower underpass", point: CornbergLandscape.Ground(270, 460)),
                    (label: "east bridge ramp foot", point: CornbergLandscape.Ground(294, 441)),
                    (label: "bridge deck center", point: CornbergLandscape.Ground(270, 460) + Vector3.up * UpperFloor),
                    (label: "east bridge landing", point: CornbergLandscape.Ground(294, 460) + Vector3.up * UpperFloor),
                    (label: "upper ascent mid", point: CornbergLandscape.Ground(294, 468) + Vector3.up * UpperFloor),
                    (label: "upper courtyard", point: CornbergLandscape.Ground(270, 478) + Vector3.up * UpperFloor)
                };
                var samples = points.Select(entry =>
                {
                    var found = NavMesh.SamplePosition(entry.point, out var hit, 1.25f, NavMesh.AllAreas);
                    return new { label = entry.label, query = entry.point.ToString("F2"), found, sample = found ? hit.position.ToString("F2") : "none" };
                }).ToArray();
                var routePairs = new[]
                {
                    (label: "underpass to east ramp foot", from: points[0].point, to: points[1].point),
                    (label: "east ramp foot to bridge", from: points[1].point, to: points[2].point),
                    (label: "bridge to ascent midpoint", from: points[3].point, to: points[4].point),
                    (label: "ascent midpoint to upper court", from: points[4].point, to: points[5].point),
                    (label: "east bridge landing to upper court", from: points[3].point, to: points[5].point),
                    (label: "bridge to upper courtyard", from: points[2].point, to: points[5].point)
                };
                var routes = routePairs.Select(route =>
                {
                    var from = NavMesh.SamplePosition(route.from, out var fromHit, 2f, NavMesh.AllAreas) ? fromHit.position : route.from;
                    var to = NavMesh.SamplePosition(route.to, out var toHit, 2f, NavMesh.AllAreas) ? toHit.position : route.to;
                    var path = new NavMeshPath();
                    var calculated = NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path);
                    return new { label = route.label, calculated, status = path.status.ToString(), corners = string.Join(" -> ", path.corners.Select(value => value.ToString("F1"))) };
                }).ToArray();
                var triangulation = NavMesh.CalculateTriangulation();
                var upperVertices = triangulation.vertices.Where(vertex => vertex.y > 3f && vertex.x >= 230f && vertex.x <= 310f && vertex.z >= 438f && vertex.z <= 505f)
                    .Select(vertex => vertex.ToString("F2")).Take(40).ToArray();
                return new
                {
                    marker = "DICEFRE_BRIARBACK_LEVEL_NAVMESH_INSPECTED",
                    sampleCount = samples.Length,
                    samples,
                    routes,
                    upperNavVertexCountInStrongholdBounds = triangulation.vertices.Count(vertex => vertex.y > 3f && vertex.x >= 230f && vertex.x <= 310f && vertex.z >= 438f && vertex.z <= 505f),
                    upperNavVertices = upperVertices
                };
            }
            finally { if (instance.valid) instance.Remove(); }
        }

        [CliCommand("dicefree.briarback.north.repair-underpass-ramp", "Relocate the upper ascent to the eastern bridge landing, preserving the lower route and mesh before saving.")]
        public static object RepairUnderpassRamp()
        {
            var scene = SceneManager.GetActiveScene();
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            Need(scene.path == ScenePath && !scene.isDirty, "save the Cornberg scene and activate it before changing the ascent.");
            var region = RegionRoot(scene);
            var group = region.Find(GroupName);
            Need(group != null, "the additive northern stronghold is missing.");
            var rampObject = group.Find("Walkable ramp through the cliff cut");
            Need(rampObject != null, "the current cliff-cut ramp is missing.");
            var filter = rampObject.GetComponent<MeshFilter>();
            var collider = rampObject.GetComponent<MeshCollider>();
            Need(filter != null && collider != null && filter.sharedMesh != null && collider.sharedMesh == filter.sharedMesh,
                "the current ascent must have one shared render and collision mesh.");
            Need(Mathf.Abs(filter.sharedMesh.bounds.center.x - 270f) < 2f && Mathf.Abs(filter.sharedMesh.bounds.center.z - 463f) < 3f,
                "the ascent mesh does not match the known ramp obstructing the lower underpass; refusing to replace it.");
            Need(AssetDatabase.LoadMainAssetAtPath(UnderpassSceneBackupPath) == null &&
                 AssetDatabase.LoadMainAssetAtPath(AscentMeshBackupPath) == null,
                "a pre-repair recovery copy already exists; refusing to overwrite it.");
            var folder = System.IO.Path.GetDirectoryName(UnderpassSceneBackupPath).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/_DiceFree/Settings", "Recovery");
            Need(EditorSceneManager.SaveScene(scene, UnderpassSceneBackupPath, true), "could not create the pre-repair scene recovery copy.");
            var original = UnityEngine.Object.Instantiate(filter.sharedMesh);
            original.name = "Briarback stronghold ascent before underpass repair";
            AssetDatabase.CreateAsset(original, AscentMeshBackupPath);

            var repaired = BuildRamp("Briarback stronghold ascent ramp",
                CornbergLandscape.Ground(294, 460) + Vector3.up * UpperFloor,
                CornbergLandscape.Ground(294, 475.5f) + Vector3.up * UpperFloor, 4.4f, 0.45f);
            Undo.RecordObject(filter.sharedMesh, "Move Briarback ascent to bridge landing");
            Undo.RecordObject(collider, "Move Briarback ascent collision to bridge landing");
            filter.sharedMesh.vertices = repaired.vertices;
            filter.sharedMesh.triangles = repaired.triangles;
            filter.sharedMesh.RecalculateNormals();
            filter.sharedMesh.RecalculateBounds();
            UnityEngine.Object.DestroyImmediate(repaired);
            collider.sharedMesh = null;
            collider.sharedMesh = filter.sharedMesh;
            EditorUtility.SetDirty(filter.sharedMesh);
            EditorUtility.SetDirty(collider);
            Physics.SyncTransforms();
            CornbergSceneBuilder.Rebake();
            Need(EditorSceneManager.SaveScene(scene), "could not save the repaired lower and upper route connection.");
            AssetDatabase.SaveAssets();
            return new
            {
                marker = "DICEFRE_BRIARBACK_UNDERPASS_RAMP_REPAIRED",
                cause = "the previous northbound ascent ramp crossed the lower road and intersected the player capsule",
                newLowerLanding = (CornbergLandscape.Ground(294, 460) + Vector3.up * UpperFloor).ToString("F2"),
                newUpperLanding = (CornbergLandscape.Ground(294, 475.5f) + Vector3.up * UpperFloor).ToString("F2"),
                sceneRecoveryCopy = UnderpassSceneBackupPath,
                meshRecoveryCopy = AscentMeshBackupPath,
                navigationRebaked = true
            };
        }

        [CliCommand("dicefree.briarback.north.open-ascent-cut", "Open only the verified cliff-wall segment blocking the upper ramp at the east bridge landing, then rebake navigation.")]
        public static object OpenAscentCut()
        {
            var scene = SceneManager.GetActiveScene();
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            Need(scene.path == ScenePath && !scene.isDirty, "save the Cornberg scene and activate it before opening the ramp cut.");
            Need(AssetDatabase.LoadMainAssetAtPath(UnderpassSceneBackupPath) != null,
                "the scene and ramp recovery copies must exist before opening the cut.");
            Need(AssetDatabase.LoadMainAssetAtPath(AscentCutSceneBackupPath) == null,
                "the pre-cut recovery copy already exists; refusing to overwrite it.");
            var group = RegionRoot(scene).Find(GroupName);
            Need(group != null, "the additive northern stronghold is missing.");
            var ramp = group.Find("Walkable ramp through the cliff cut");
            Need(ramp != null && Mathf.Abs(ramp.GetComponent<MeshCollider>().bounds.center.x - 294f) < 2f,
                "the eastern bridge landing ramp is not in its expected position.");
            var walls = group.GetComponentsInChildren<Transform>(true)
                .Where(value => value.name == "Natural cliff face below upper terrace" &&
                                Mathf.Abs(value.position.x - 293f) < 0.1f && Mathf.Abs(value.position.z - 472f) < 0.1f)
                .ToArray();
            Need(walls.Length == 1 && walls[0].GetComponent<Collider>() != null,
                "the exact x=293 cliff segment was not found; refusing to alter adjacent terrain.");
            Need(EditorSceneManager.SaveScene(scene, AscentCutSceneBackupPath, true),
                "could not create the pre-cut scene recovery copy.");
            var center = walls[0].position.ToString("F2");
            Undo.DestroyObjectImmediate(walls[0].gameObject);
            Physics.SyncTransforms();
            CornbergSceneBuilder.Rebake();
            Need(EditorSceneManager.SaveScene(scene), "could not save the opened eastern ascent cut.");
            AssetDatabase.SaveAssets();
            return new
            {
                marker = "DICEFRE_BRIARBACK_ASCENT_CUT_OPENED",
                removedCliffSegmentCenter = center,
                neighboringCliffSegmentsPreserved = true,
                sceneRecoveryCopy = AscentCutSceneBackupPath,
                navigationRebaked = true
            };
        }

        private static IEnumerable<string> Ancestors(Transform value)
        {
            var names = new Stack<string>();
            for (var current = value; current != null; current = current.parent)
                names.Push(current.name);
            return names;
        }

        [CliCommand("dicefree.briarback.north.open-approach", "Create a recoverable road gap through only the three central authored ridge rocks, then rebake navigation.")]
        public static object OpenApproach()
        {
            var scene = SceneManager.GetActiveScene();
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            Need(scene.path == ScenePath, "activate the completed Cornberg blockout first.");
            Need(System.IO.File.Exists(BoundarySceneBackupPath), "the pre-boundary scene recovery copy is missing; refusing to continue.");
            var region = RegionRoot(scene);
            var stronghold = region.Find(GroupName);
            Need(stronghold != null, "the additive northern stronghold is missing.");
            var boundary = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .SingleOrDefault(t => t.name == "North edge" && t.parent != null && t.parent.name == "Physical outer edge");
            Need(boundary != null && boundary.position.z >= North + 5f,
                "move the confirmed old z=400 physical boundary first; refusing to alter the ridge before that.");
            var ridgeRoot = region.Find("Briarback thornwood, rocks and camp clearings");
            Need(ridgeRoot != null, "the original Briarback woodland blockout is missing.");
            var ridge = ridgeRoot.GetComponentsInChildren<Transform>(true)
                .Where(t => t.name == "Northern wooded ridge" && Mathf.Abs(t.position.z - 397f) < 1f)
                .OrderBy(t => Mathf.Abs(t.position.x - 270f)).Take(3).ToArray();
            Need(ridge.Length == 3 && ridge.Select(t => t.position.x).OrderBy(x => x)
                    .Zip(new[] { 266f, 271f, 276f }, (actual, expected) => Mathf.Abs(actual - expected) < 0.1f).All(value => value),
                "the expected three central z=397 ridge rocks are not present; refusing to alter the scene.");
            Need(AssetDatabase.LoadMainAssetAtPath(ApproachSceneBackupPath) == null,
                "the approach scene recovery copy already exists; refusing to overwrite it.");
            var folder = System.IO.Path.GetDirectoryName(ApproachSceneBackupPath).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/_DiceFree/Settings", "Recovery");
            Need(EditorSceneManager.SaveScene(scene, ApproachSceneBackupPath, true), "could not create the pre-edit scene recovery copy.");

            var removedCenters = ridge.Select(t => t.position.ToString("F2")).ToArray();
            foreach (var rock in ridge)
                Undo.DestroyObjectImmediate(rock.gameObject);
            Physics.SyncTransforms();
            CornbergSceneBuilder.Rebake();
            Need(EditorSceneManager.SaveScene(scene), "could not save the northern approach opening.");
            AssetDatabase.SaveAssets();
            return new
            {
                marker = "DICEFRE_BRIARBACK_NORTH_APPROACH_OPENED",
                removedRidgeRockCenters = removedCenters,
                physicalNorthBoundaryZ = boundary.position.z,
                sceneRecoveryCopy = ApproachSceneBackupPath,
                navigationRebaked = true
            };
        }

        [CliCommand("dicefree.briarback.north.extend-continuous-ground", "Extend the original Briarback ground mesh in place, with an original-mesh recovery copy, eliminating the two-source seam.")]
        public static object ExtendContinuousGround()
        {
            var scene = SceneManager.GetActiveScene();
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            Need(scene.path == ScenePath && !scene.isDirty, "save and activate the completed Cornberg blockout first.");
            var region = RegionRoot(scene);
            var group = region.Find(GroupName);
            Need(group != null, "the additive northern stronghold is missing.");
            var baseTerrain = region.Find("Continuous Briarback walkable terrain");
            Need(baseTerrain != null, "the original continuous Briarback terrain object is missing.");
            var filter = baseTerrain.GetComponent<MeshFilter>();
            var collider = baseTerrain.GetComponent<MeshCollider>();
            var mesh = filter != null ? filter.sharedMesh : null;
            Need(mesh != null && collider != null, "the original ground mesh or collider is missing.");
            Need(AssetDatabase.GetAssetPath(mesh) == ExistingGroundMeshPath,
                "the original ground mesh asset path differs from the expected Briarback-owned asset; no changes were made.");
            Need(mesh.bounds.max.z < 400f, "the original ground is already extended; refusing to overwrite it again.");
            var recoveryDirectory = System.IO.Path.GetDirectoryName(ExistingGroundBackupPath).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(recoveryDirectory)) AssetDatabase.CreateFolder("Assets/_DiceFree/Settings", "Recovery");
            Need(AssetDatabase.LoadAssetAtPath<Mesh>(ExistingGroundBackupPath) == null,
                "the original-ground recovery copy already exists; refusing to overwrite it.");
            Need(AssetDatabase.CopyAsset(ExistingGroundMeshPath, ExistingGroundBackupPath),
                "could not create the original Briarback ground recovery copy.");

            var fullGround = CreateGround(246f, North);
            Undo.RegisterCompleteObjectUndo(mesh, "Extend Briarback continuous terrain north");
            mesh.Clear();
            mesh.indexFormat = fullGround.indexFormat;
            mesh.vertices = fullGround.vertices;
            mesh.triangles = fullGround.triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            collider.sharedMesh = null;
            collider.sharedMesh = mesh;
            EditorUtility.SetDirty(mesh);

            // The original surface now covers the extension. Removing our
            // redundant overlapping child avoids duplicate renderer/collider
            // sources in both Physics and the NavMesh bake.
            var extension = group.Find("Continuous walkable northern extension");
            Need(extension != null, "the temporary additive ground child is missing.");
            Undo.DestroyObjectImmediate(extension.gameObject);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            Need(EditorSceneManager.SaveScene(scene), "Unity could not save the single continuous ground mesh.");
            return new
            {
                marker = "DICEFRE_BRIARBACK_NORTH_SINGLE_GROUND_MESH_READY",
                originalGroundAsset = ExistingGroundMeshPath,
                originalGroundRecoveryCopy = ExistingGroundBackupPath,
                originalGroundNorth = 398,
                continuousGroundNorth = North,
                extensionGroundSourceRemoved = true,
                exactHeightContinuity = true
            };
        }

        [CliCommand("dicefree.briarback.north.arena.preview", "Preflight an outdoor 25m Chieftain clearing and move the longhouse out of its footprint; no scene changes.")]
        public static object PreviewChieftainArena()
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            var scene = SceneManager.GetActiveScene();
            Need(scene.path == ScenePath && !scene.isDirty, "save and activate the completed Cornberg blockout first.");
            var region = RegionRoot(scene);
            var group = region.Find(GroupName);
            Need(group != null, "the additive northern stronghold is missing.");
            var foundation = group.Find("Chieftain longhouse - blockout foundation");
            var hall = group.Find("Chieftain longhouse - timber hall");
            var roof = group.Find("Chieftain longhouse - roof blockout");
            Need(foundation != null && hall != null && roof != null, "the exact longhouse blockout trio is required; no scene was changed.");
            Need(Vector3.Distance(foundation.position, CornbergLandscape.Ground(268, 490) + Vector3.up * (UpperFloor + .45f)) < .2f,
                "the longhouse has already moved; refusing to infer a new source placement.");

            var houseParts = new[] { foundation, hall, roof };
            var destinationGround = CornbergLandscape.Ground(248, 483);
            var boxes = new[]
            {
                (position: destinationGround + Vector3.up * (UpperFloor + .45f), size: new Vector3(24, .9f, 17)),
                (position: destinationGround + Vector3.up * (UpperFloor + 3.7f), size: new Vector3(19, 6.5f, 13)),
                (position: destinationGround + Vector3.up * (UpperFloor + 7.5f), size: new Vector3(21, 1.2f, 15))
            };
            var blockers = boxes.SelectMany(box => Physics.OverlapBox(box.position, box.size * .5f,
                    Quaternion.identity, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                .Where(collider => collider != null && collider.enabled && collider.gameObject.activeInHierarchy &&
                    collider.transform != group.Find("Raised upper stronghold terrace") &&
                    !houseParts.Any(part => collider.transform == part || collider.transform.IsChildOf(part)))
                .Select(collider => collider.name).Distinct().ToArray();
            Need(blockers.Length == 0, "the proposed west-side hall position overlaps existing collision: " + string.Join(", ", blockers.Take(12)));
            Need(AssetDatabase.LoadAssetAtPath<Mesh>(OpenArenaTerraceMeshPath) == null,
                "the open-arena terrace asset path is already occupied; no scene was changed.");
            Need(AssetDatabase.LoadAssetAtPath<Mesh>(OpenArenaMeshBackupPath) == null &&
                 AssetDatabase.LoadMainAssetAtPath(OpenArenaSceneBackupPath) == null,
                "the arena recovery path already exists; inspect it before proceeding.");

            var center = CornbergLandscape.Ground(282, 484) + Vector3.up * UpperFloor;
            return new
            {
                marker = "DICEFRE_BRIARBACK_CHIEFTAIN_ARENA_PREFLIGHT_OK",
                arenaCenter = center.ToString("F2"),
                clearDimensions = new { width = 25f, depth = 23f },
                longhouseDestination = destinationGround.ToString("F2"),
                destinationColliders = blockers,
                unchangedRampsAndBridge = true,
                recoveryCreated = false,
                navMeshChanged = false
            };
        }

        [CliCommand("dicefree.briarback.north.arena.open", "Move the longhouse to the upper terrace's west edge and expand the raised terrace for a clear outdoor Chieftain arena; save recovery copies first.")]
        public static object OpenChieftainArena()
        {
            var preflight = PreviewChieftainArena();
            var scene = SceneManager.GetActiveScene();
            var group = RegionRoot(scene).Find(GroupName);
            var foundation = group.Find("Chieftain longhouse - blockout foundation");
            var hall = group.Find("Chieftain longhouse - timber hall");
            var roof = group.Find("Chieftain longhouse - roof blockout");
            var oldTerrace = AssetDatabase.LoadAssetAtPath<Mesh>(UpperTerraceMeshPath);
            var filter = group.Find("Raised upper stronghold terrace")?.GetComponent<MeshFilter>();
            var collider = group.Find("Raised upper stronghold terrace")?.GetComponent<MeshCollider>();
            Need(oldTerrace != null && filter != null && collider != null && filter.sharedMesh == oldTerrace && collider.sharedMesh == oldTerrace,
                "the authored terrace filter/collider no longer reference the expected original mesh.");
            Need(AssetDatabase.CopyAsset(UpperTerraceMeshPath, OpenArenaMeshBackupPath), "could not preserve the original terrace mesh.");
            Need(EditorSceneManager.SaveScene(scene, OpenArenaSceneBackupPath, true), "could not preserve the pre-arena Cornberg scene.");

            var newTerrace = BuildTerrace("Briarback upper open arena terrace", new Vector2(234, 470),
                new Vector2(306, 502), UpperFloor, .9f);
            AssetDatabase.CreateAsset(newTerrace, OpenArenaTerraceMeshPath);
            newTerrace = AssetDatabase.LoadAssetAtPath<Mesh>(OpenArenaTerraceMeshPath);
            Need(newTerrace != null && newTerrace.vertexCount > 0, "could not save the widened upper terrace mesh.");
            Undo.RecordObject(filter, "Extend Chieftain arena terrace south");
            Undo.RecordObject(collider, "Extend Chieftain arena terrace collision south");
            filter.sharedMesh = newTerrace;
            collider.sharedMesh = null;
            collider.sharedMesh = newTerrace;
            var destinationGround = CornbergLandscape.Ground(248, 483);
            var parts = new[]
            {
                (target: foundation, position: destinationGround + Vector3.up * (UpperFloor + .45f)),
                (target: hall, position: destinationGround + Vector3.up * (UpperFloor + 3.7f)),
                (target: roof, position: destinationGround + Vector3.up * (UpperFloor + 7.5f))
            };
            foreach (var part in parts)
            {
                Undo.RecordObject(part.target, "Move Chieftain longhouse clear of outdoor arena");
                part.target.position = part.position;
                EditorUtility.SetDirty(part.target);
            }
            EditorUtility.SetDirty(filter);
            EditorUtility.SetDirty(collider);
            AssetDatabase.SaveAssets();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(scene);
            Need(EditorSceneManager.SaveScene(scene), "could not save the open upper arena blockout.");
            return new
            {
                marker = "DICEFRE_BRIARBACK_CHIEFTAIN_ARENA_OPENED",
                arenaCenter = CornbergLandscape.Ground(282, 484).ToString("F2"),
                clearDimensions = new { width = 25f, depth = 23f },
                longhouseDestination = destinationGround.ToString("F2"),
                originalLonghousePreserved = true,
                originalTerraceMesh = OpenArenaMeshBackupPath,
                sceneRecoveryCopy = OpenArenaSceneBackupPath,
                newTerraceMesh = OpenArenaTerraceMeshPath,
                navMeshNeedsRebake = true,
                preflight = preflight
            };
        }

        [CliCommand("dicefree.briarback.north.validate", "Validate the saved northern terrain, accessible fort gate, cliff-backed blockout, bounds, and navigation connection.")]
        public static object Validate()
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            var scene = SceneManager.GetActiveScene();
            Need(scene.path == ScenePath && !scene.isDirty, "the saved Cornberg scene must be active and clean.");
            var root = RegionRoot(scene);
            var group = root.Find(GroupName);
            Need(group != null, "the additive northern blockout is missing.");
            var ground = group.Find("Continuous walkable northern extension");
            var originalTerrain = root.Find("Continuous Briarback walkable terrain");
            Need(ground == null, "the redundant extension collider still overlaps the continuous terrain source.");
            Need(originalTerrain != null && originalTerrain.GetComponent<MeshCollider>() != null &&
                 originalTerrain.GetComponent<MeshCollider>().bounds.max.z >= North - 1f,
                "the original continuous terrain collider does not reach the new northern edge.");
            var road = group.Find("North approach to the open stronghold gate");
            Need(road != null && road.GetComponent<Collider>() == null, "the visual approach must not add collision over the ground.");
            var foundation = group.Find("Chieftain longhouse - blockout foundation");
            var hall = group.Find("Chieftain longhouse - timber hall");
            var roof = group.Find("Chieftain longhouse - roof blockout");
            Need(foundation != null && hall != null && roof != null, "the longhouse blockout is missing.");
            var expectedHouseGround = CornbergLandscape.Ground(248, 483);
            Need(Vector3.Distance(foundation.position, expectedHouseGround + Vector3.up * (UpperFloor + .45f)) < .2f &&
                 Vector3.Distance(hall.position, expectedHouseGround + Vector3.up * (UpperFloor + 3.7f)) < .2f &&
                 Vector3.Distance(roof.position, expectedHouseGround + Vector3.up * (UpperFloor + 7.5f)) < .2f,
                "the Chieftain longhouse must sit clear at the west side of the highest terrace.");
            var terrace = group.Find("Raised upper stronghold terrace");
            Need(terrace?.GetComponent<MeshCollider>() != null &&
                 terrace.GetComponent<MeshFilter>().sharedMesh == AssetDatabase.LoadAssetAtPath<Mesh>(OpenArenaTerraceMeshPath) &&
                 terrace.GetComponent<MeshCollider>().sharedMesh == AssetDatabase.LoadAssetAtPath<Mesh>(OpenArenaTerraceMeshPath),
                "the raised upper arena terrace must use its recoverable widened walkable mesh.");
            var upperAscent = group.Find("Walkable ramp through the cliff cut");
            Need(upperAscent?.GetComponent<MeshCollider>() != null, "the upper ascent ramp is missing its collider.");
            Need(Mathf.Abs(upperAscent.GetComponent<MeshCollider>().bounds.center.x - 294f) < 2f,
                "the upper ascent must enter from the eastern bridge landing so it does not cross the lower road.");
            var cliffSegments = group.GetComponentsInChildren<Transform>(true)
                .Where(value => value.name == "Natural cliff face below upper terrace").ToArray();
            Need(cliffSegments.Any(value => Mathf.Abs(value.position.x - 287f) < 0.1f) &&
                 cliffSegments.Any(value => Mathf.Abs(value.position.x - 299f) < 0.1f) &&
                 !cliffSegments.Any(value => Mathf.Abs(value.position.x - 293f) < 0.1f),
                "the eastern ascent needs one clear cliff cut while keeping both neighboring cliff segments.");
            Need(group.Find("Raised timber bridge deck over lower road")?.GetComponent<MeshCollider>() != null, "the raised bridge deck is missing its collider.");
            Need(group.Find("West ramp to upper timber bridge")?.GetComponent<MeshCollider>() != null &&
                 group.Find("East ramp to upper timber bridge")?.GetComponent<MeshCollider>() != null,
                "both walkable bridge ramps must be present.");
            var fortColliderCount = group.GetComponentsInChildren<Collider>(true).Length;
            Need(fortColliderCount >= 190, "the defensive palisade and cliff collision is incomplete; found " + fortColliderCount + " colliders.");

            var navOwner = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<WorldNavigation>(true)).SingleOrDefault();
            Need(navOwner != null && navOwner.Data != null, "the saved World 1 NavMesh is missing.");
            var nav = NavMesh.AddNavMeshData(navOwner.Data);
            try
            {
                Vector3 Sample(Vector3 location, string label, float radius = 2.5f)
                {
                    Need(NavMesh.SamplePosition(location, out var hit, radius, NavMesh.AllAreas), "no NavMesh at " + label + " near " + location);
                    return hit.position;
                }

                var lowStart = Sample(CornbergLandscape.Ground(270, 369), "existing loop handoff");
                var gate = Sample(CornbergLandscape.Ground(270, 431), "lower open gate");
                var underpass = Sample(CornbergLandscape.Ground(270, 460), "lower road under bridge");
                var bridgeLow = Sample(CornbergLandscape.Ground(242, 441), "west bridge-ramp foot");
                var bridgeDeck = Sample(CornbergLandscape.Ground(270, 460) + Vector3.up * UpperFloor, "upper bridge deck", 1.25f);
                var upperRampFoot = Sample(CornbergLandscape.Ground(294, 460) + Vector3.up * UpperFloor, "upper-terrace ramp foot at eastern bridge landing", 1.25f);
                var upperCourt = Sample(CornbergLandscape.Ground(270, 478) + Vector3.up * UpperFloor, "upper stronghold courtyard", 1.25f);
                var paths = 0;
                var routes = new[]
                {
                    ("lower approach and upper ascent", new[] { lowStart, gate, underpass, upperCourt }),
                    ("overpass and bridge ramps", new[] { bridgeLow, bridgeDeck, Sample(CornbergLandscape.Ground(294, 441), "east bridge-ramp foot") }),
                    ("east bridge landing ascent to upper court", new[] { upperRampFoot, upperCourt })
                };
                foreach (var route in routes)
                {
                    for (var i = 1; i < route.Item2.Length; i++)
                    {
                        var path = new NavMeshPath();
                        var calculated = NavMesh.CalculatePath(route.Item2[i - 1], route.Item2[i], NavMesh.AllAreas, path);
                        Need(calculated && path.status == NavMeshPathStatus.PathComplete,
                            route.Item1 + " is disconnected at segment " + (i - 1) + " from " + route.Item2[i - 1] + " to " + route.Item2[i] +
                            "; status=" + path.status + "; corners=" + string.Join(" -> ", path.corners.Select(p => p.ToString("F1"))));
                        paths++;
                    }
                }
                var arenaCells = 0;
                var arenaCenter = CornbergLandscape.Ground(282, 484) + Vector3.up * UpperFloor;
                for (var x = 270f; x <= 294f; x += 2f)
                for (var z = 473f; z <= 495f; z += 2f)
                {
                    var requested = CornbergLandscape.Ground(x, z) + Vector3.up * UpperFloor;
                    Need(NavMesh.SamplePosition(requested, out var arenaPoint, 1.25f, NavMesh.AllAreas),
                        "the 25m outdoor arena footprint has no walkable NavMesh at " + new Vector2(x, z));
                    Need(!Physics.CheckCapsule(arenaPoint.position + Vector3.up * .48f,
                            arenaPoint.position + Vector3.up * 1.42f, .34f, Physics.AllLayers, QueryTriggerInteraction.Ignore),
                        "a player-sized capsule is obstructed inside the Chieftain's outdoor arena at " + new Vector2(x, z));
                    arenaCells++;
                }
                var gateSweep = Physics.CapsuleCastAll(CornbergLandscape.Ground(270, 432) + Vector3.up * 0.5f,
                    CornbergLandscape.Ground(270, 432) + Vector3.up * 1.7f, 0.34f, Vector3.forward, 10f,
                    Physics.AllLayers, QueryTriggerInteraction.Ignore).Where(hit => hit.collider != null && hit.collider.enabled && hit.collider.gameObject.activeInHierarchy).ToArray();
                Need(gateSweep.Length == 0, "a solid collider blocks the open southern gate approach.");
                Need(!Physics.CheckCapsule(CornbergLandscape.Ground(270, 460) + Vector3.up * 0.55f,
                        CornbergLandscape.Ground(270, 460) + Vector3.up * 2.1f, 0.32f,
                        Physics.AllLayers, QueryTriggerInteraction.Ignore),
                    "the lower route does not have clear player-sized headroom beneath the bridge.");
                return new
                {
                    marker = "DICEFRE_BRIARBACK_NORTH_BLOCKOUT_VALID",
                    terrainCollider = true,
                    fortColliderCount,
                    completeMultilevelNavMeshSegments = paths,
                    sampledUpperBridgeY = bridgeDeck.y,
                    sampledUpperCourtyardY = upperCourt.y,
                    lowerRoadHasUnderpassClearance = true,
                    gateWidth = 22,
                    gateCollisionFree = true,
                    chieftainArena = new { center = arenaCenter.ToString("F2"), width = 25f, depth = 23f, clearNavMeshCells = arenaCells },
                    longhouseOutsideArena = true,
                    mapNorth = root.GetComponent<OverworldMapBounds>().Bounds.yMax,
                    recoveryCopyExists = AssetDatabase.LoadAssetAtPath<NavMeshData>(NavBackupPath) != null,
                    encounterLogicDeferred = true
                };
            }
            finally { if (nav.valid) nav.Remove(); }
        }
    }
}
