using System;
using System.IO;
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
    /// Adds four clearly marked, visual-only elemental settlement blockouts
    /// inside the existing northern Briarback extension. Placement and art are
    /// provisional; this authoring does not create actors, hazards, colliders,
    /// quests, or navigation changes.
    /// </summary>
    public static class BriarbackElementalSitesAuthoring
    {
        private const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string RegionRootName = "14 - Briarback quilboar frontier";
        private const string GroupName = "Briarback elemental settlements - visual blockout - provisional";
        private const string RecoveryPath = "Assets/_DiceFree/Settings/Recovery/Cornberg-before-elemental-settlement-blockout.unity";
        private const string RefinementRecoveryPath = "Assets/_DiceFree/Settings/Recovery/Cornberg-before-elemental-settlement-visual-refinement.unity";
        private static readonly (string name, Vector2 position, string material)[] Sites =
        {
            ("Fire Shaman settlement - provisional", new Vector2(230, 378), "Warm earth"),
            ("Earth Shaman settlement - provisional", new Vector2(286, 378), "Fieldstone"),
            ("Lightning Shaman settlement - provisional", new Vector2(230, 426), "Timber"),
            ("Ice Shaman settlement - provisional", new Vector2(286, 426), "Mountain Trial floor")
        };

        private static void Need(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("BRIARBACK ELEMENTAL SITES: " + message);
        }

        private static Scene ActiveScene()
        {
            var scene = SceneManager.GetActiveScene();
            Need(scene.path == ScenePath, "open the saved Cornberg scene first; no other scene is loaded.");
            Need(!scene.isDirty, "save current scene edits before preview or authoring.");
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            return scene;
        }

        private static Transform RegionRoot(Scene scene)
        {
            var roots = scene.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<Transform>(true))
                .Where(t => t.name == RegionRootName).ToArray();
            Need(roots.Length == 1, "expected one existing Briarback root.");
            return roots[0];
        }

        private static Material Palette(string name)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Materials/Blockout/" + name + ".mat");
            Need(material != null, "existing blockout material missing: " + name);
            return material;
        }

        private static NavMeshDataInstance AddNavigation(Scene scene)
        {
            var owner = scene.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<WorldNavigation>(true))
                .SingleOrDefault();
            Need(owner != null && owner.Data != null, "saved World 1 NavMesh missing.");
            return NavMesh.AddNavMeshData(owner.Data);
        }

        private static bool IsGround(Collider collider)
        {
            var hierarchy = collider.transform;
            while (hierarchy != null)
            {
                var name = hierarchy.name;
                if (name.IndexOf("ground", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("terrain", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                hierarchy = hierarchy.parent;
            }
            return false;
        }

        private static Vector3[] Preflight(Scene scene)
        {
            RegionRoot(scene);
            var approach = CornbergLandscape.Ground(270f, 369f);
            Need(NavMesh.SamplePosition(approach, out var approachHit, 2f, NavMesh.AllAreas),
                "the existing northern approach has no saved NavMesh point.");
            var points = Sites.Select(site =>
            {
                var ground = CornbergLandscape.Ground(site.position.x, site.position.y);
                Need(NavMesh.SamplePosition(ground, out var hit, 2f, NavMesh.AllAreas),
                    site.name + " is outside navigable ground at " + ground.ToString("F1"));
                var route = new NavMeshPath();
                Need(NavMesh.CalculatePath(approachHit.position, hit.position, NavMesh.AllAreas, route) &&
                     route.status == NavMeshPathStatus.PathComplete,
                    site.name + " is disconnected from the existing northern approach.");
                var obstructions = Physics.OverlapSphere(hit.position + Vector3.up * 1f, 7f,
                        Physics.AllLayers, QueryTriggerInteraction.Ignore)
                    .Where(c => c.enabled && c.gameObject.activeInHierarchy && !IsGround(c)).ToArray();
                Need(obstructions.Length == 0, site.name + " has nearby solid blockers: " +
                     string.Join(", ", obstructions.Select(c => c.name).Distinct().Take(8)));
                return hit.position;
            }).ToArray();

            for (var i = 0; i < points.Length; i++)
            for (var j = i + 1; j < points.Length; j++)
                Need(Vector3.Distance(points[i], points[j]) >= 45f, "elemental sites overlap one another.");

            return points;
        }

        [CliCommand("dicefree.briarback.elements.probe-clearings",
            "Read-only sample of northern approach clearings with collision and NavMesh route results.")]
        public static object ProbeClearings()
        {
            var scene = ActiveScene();
            var nav = AddNavigation(scene);
            try
            {
                var entry = CornbergLandscape.Ground(270f, 369f);
                Need(NavMesh.SamplePosition(entry, out var from, 2f, NavMesh.AllAreas), "northern approach is disconnected.");
                var candidates = new System.Collections.Generic.List<object>();
                for (var z = 378; z <= 430; z += 8)
                for (var x = 222; x <= 314; x += 8)
                {
                    var ground = CornbergLandscape.Ground(x, z);
                    var onNav = NavMesh.SamplePosition(ground, out var hit, 2f, NavMesh.AllAreas);
                    var blockers = Physics.OverlapSphere(ground + Vector3.up, 7f,
                            Physics.AllLayers, QueryTriggerInteraction.Ignore)
                        .Where(c => c.enabled && c.gameObject.activeInHierarchy && !IsGround(c)).ToArray();
                    var route = new NavMeshPath();
                    var connected = onNav && NavMesh.CalculatePath(from.position, hit.position, NavMesh.AllAreas, route) &&
                                    route.status == NavMeshPathStatus.PathComplete;
                    candidates.Add(new
                    {
                        x,
                        z,
                        clear = blockers.Length == 0,
                        connected,
                        blockers = blockers.Select(c => c.name).Distinct().Take(3).ToArray()
                    });
                }
                return new { marker = "DICEFRE_BRIARBACK_CLEARING_PROBE", candidates };
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        [CliCommand("dicefree.briarback.elements.preview",
            "Check four provisional elemental-site footprints against saved ground, blockers, and NavMesh without changing the scene.")]
        public static object Preview()
        {
            var scene = ActiveScene();
            var nav = AddNavigation(scene);
            try
            {
                Need(RegionRoot(scene).Find(GroupName) == null, "provisional site blockout already exists.");
                var points = Preflight(scene);
                return new
                {
                    marker = "DICEFRE_BRIARBACK_ELEMENT_SITES_PREFLIGHT_OK",
                    siteCount = points.Length,
                    sites = Sites.Select((site, i) => new { site = site.name, position = points[i].ToString("F2") }).ToArray(),
                    collidersAdded = 0,
                    navmeshChanged = false,
                    placementProvisional = true
                };
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        private static GameObject Primitive(string name, PrimitiveType type, Vector3 position,
            Vector3 scale, Material material, Transform parent)
        {
            var go = GameObject.CreatePrimitive(type);
            Undo.RegisterCreatedObjectUndo(go, "Add Briarback elemental settlement visual blockout");
            go.name = name;
            go.layer = 2;
            go.transform.SetParent(parent, true);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            var collider = go.GetComponent<Collider>();
            if (collider != null) Undo.DestroyObjectImmediate(collider);
            return go;
        }

        private static void BuildSite(Transform group, int index, Vector3 ground)
        {
            var site = Sites[index];
            var siteRoot = new GameObject(site.name);
            Undo.RegisterCreatedObjectUndo(siteRoot, "Add Briarback elemental settlement visual blockout");
            siteRoot.transform.SetParent(group, true);
            siteRoot.transform.position = ground;
            var mat = Palette(site.material);
            var stone = Palette("Fieldstone");

            // Eight broken standing stones establish a compact tribal site.
            for (var i = 0; i < 8; i++)
            {
                var angle = i * Mathf.PI / 4f;
                var radius = 5.8f + (i % 2) * 0.6f;
                var height = 1.8f + (i % 3) * 0.45f;
                var p = ground + new Vector3(Mathf.Cos(angle) * radius, height * 0.5f,
                    Mathf.Sin(angle) * radius);
                var pillar = Primitive(site.name + " - ring stone " + (i + 1),
                    PrimitiveType.Cube, p, new Vector3(0.85f, height, 0.75f), stone, siteRoot.transform);
                pillar.transform.rotation = Quaternion.Euler(0, i * 22.5f, 7f * (i % 2 == 0 ? 1 : -1));
            }

            Primitive(site.name + " - central altar", PrimitiveType.Cylinder,
                ground + Vector3.up * 0.4f, new Vector3(3.4f, 0.8f, 3.4f), mat, siteRoot.transform);
            switch (index)
            {
                case 0: // Fire: low, contained ritual braziers; no flame or damage behavior.
                    for (var i = 0; i < 4; i++)
                    {
                        var angle = i * Mathf.PI / 2f;
                        Primitive(site.name + " - unlit brazier " + (i + 1), PrimitiveType.Cylinder,
                            ground + new Vector3(Mathf.Cos(angle) * 4.1f, 0.55f, Mathf.Sin(angle) * 4.1f),
                            new Vector3(1.15f, 0.55f, 1.15f), mat, siteRoot.transform);
                    }
                    Primitive(site.name + " - ritual pillar", PrimitiveType.Cube,
                        ground + Vector3.up * 2f, new Vector3(1f, 2.4f, 1f), mat, siteRoot.transform);
                    break;
                case 1: // Earth: a rough cairn made from overlapping blockout boulders.
                    for (var i = 0; i < 4; i++)
                    {
                        var angle = i * Mathf.PI / 2f;
                        Primitive(site.name + " - cairn boulder " + (i + 1), PrimitiveType.Sphere,
                            ground + new Vector3(Mathf.Cos(angle) * 2.1f, 0.75f, Mathf.Sin(angle) * 2.1f),
                            new Vector3(1.8f, 1.35f, 1.8f), mat, siteRoot.transform);
                    }
                    Primitive(site.name + " - cairn capstone", PrimitiveType.Cube,
                        ground + Vector3.up * 2.2f, new Vector3(1.6f, 0.7f, 1.5f), stone, siteRoot.transform);
                    break;
                case 2: // Lightning: three high conductors joined by a crossbar.
                    for (var i = 0; i < 3; i++)
                    {
                        var angle = i * Mathf.PI * 2f / 3f;
                        Primitive(site.name + " - conductor " + (i + 1), PrimitiveType.Cube,
                            ground + new Vector3(Mathf.Cos(angle) * 3.8f, 2.25f, Mathf.Sin(angle) * 3.8f),
                            new Vector3(0.45f, 4.5f, 0.45f), mat, siteRoot.transform);
                    }
                    Primitive(site.name + " - overhead crossbar", PrimitiveType.Cube,
                        ground + Vector3.up * 3.8f, new Vector3(7.2f, 0.35f, 0.35f), mat, siteRoot.transform);
                    Primitive(site.name + " - ritual pillar", PrimitiveType.Cube,
                        ground + Vector3.up * 1.65f, new Vector3(0.8f, 1.8f, 0.8f), stone, siteRoot.transform);
                    break;
                default: // Ice: tall angled shards distinguish this ring without adding a slow zone.
                    for (var i = 0; i < 4; i++)
                    {
                        var angle = i * Mathf.PI / 2f + Mathf.PI / 4f;
                        var shard = Primitive(site.name + " - frost shard " + (i + 1), PrimitiveType.Cube,
                            ground + new Vector3(Mathf.Cos(angle) * 3.3f, 1.6f, Mathf.Sin(angle) * 3.3f),
                            new Vector3(0.7f, 3.2f, 0.7f), mat, siteRoot.transform);
                        shard.transform.rotation = Quaternion.Euler(0, i * 45f, i % 2 == 0 ? 8f : -8f);
                    }
                    Primitive(site.name + " - ritual pillar", PrimitiveType.Cube,
                        ground + Vector3.up * 1.8f, new Vector3(0.8f, 2f, 0.8f), mat, siteRoot.transform);
                    break;
            }

            // Low approach stones suggest an intentional entrance without
            // placing a wall, hazard, or navigation obstacle across the route.
            for (var i = 0; i < 3; i++)
            {
                var p = ground + new Vector3(0, 0.18f, -8.5f - i * 2.4f);
                Primitive(site.name + " - approach stone " + (i + 1), PrimitiveType.Cube,
                    p, new Vector3(1.7f, 0.36f, 1.0f), stone, siteRoot.transform);
            }
        }

        [CliCommand("dicefree.briarback.elements.author",
            "Create the preflighted four-site provisional visual blockout in a separate scene group; no gameplay or collision is added.")]
        public static object Author()
        {
            var scene = ActiveScene();
            var region = RegionRoot(scene);
            Need(region.Find(GroupName) == null, "provisional site blockout already exists; no scene changed.");
            var nav = AddNavigation(scene);
            try
            {
                var points = Preflight(scene);
                var recoveryFolder = "Assets/_DiceFree/Settings/Recovery";
                Need(AssetDatabase.IsValidFolder(recoveryFolder), "scene recovery folder missing; no scene changed.");
                Need(!System.IO.File.Exists(RecoveryPath), "recovery destination already exists; no scene changed.");
                Need(EditorSceneManager.SaveScene(scene, RecoveryPath, true), "could not create the pre-edit scene recovery copy.");

                var group = new GameObject(GroupName);
                Undo.RegisterCreatedObjectUndo(group, "Add Briarback elemental settlement visual blockout");
                group.transform.SetParent(region, false);
                for (var i = 0; i < points.Length; i++) BuildSite(group.transform, i, points[i]);

                Need(EditorSceneManager.SaveScene(scene), "Unity could not save the provisional elemental sites.");
                AssetDatabase.SaveAssets();
                return new
                {
                    marker = "DICEFRE_BRIARBACK_ELEMENT_SITES_AUTHORED",
                    sites = Sites.Select((site, i) => new { site = site.name, position = points[i].ToString("F2") }).ToArray(),
                    recovery = RecoveryPath,
                    visuals = 68,
                    collidersAdded = 0,
                    navmeshChanged = false,
                    actorsQuestsHazardsAdded = false,
                    placementProvisional = true
                };
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        [CliCommand("dicefree.briarback.elements.refine",
            "Refine only the generated visual-only elemental sites, moving them to clear approach clearings and differentiating each element motif.")]
        public static object Refine()
        {
            var scene = ActiveScene();
            var region = RegionRoot(scene);
            var oldGroup = region.Find(GroupName);
            Need(oldGroup != null, "the provisional group is missing; use the author command first.");
            Need(oldGroup.GetComponentsInChildren<Collider>(true).Length == 0,
                "generated group unexpectedly contains colliders; refusing to replace it.");
            Need(oldGroup.childCount == Sites.Length, "generated group no longer matches the four-site schema.");
            Need(!System.IO.File.Exists(RefinementRecoveryPath), "refinement recovery destination already exists.");
            var nav = AddNavigation(scene);
            try
            {
                var points = Preflight(scene);
                Need(EditorSceneManager.SaveScene(scene, RefinementRecoveryPath, true),
                    "could not create the pre-refinement recovery copy.");
                Undo.DestroyObjectImmediate(oldGroup.gameObject);
                var group = new GameObject(GroupName);
                Undo.RegisterCreatedObjectUndo(group, "Refine Briarback elemental settlement visuals");
                group.transform.SetParent(region, false);
                for (var i = 0; i < points.Length; i++) BuildSite(group.transform, i, points[i]);
                Need(EditorSceneManager.SaveScene(scene), "Unity could not save the refined site blockout.");
                AssetDatabase.SaveAssets();
                return new
                {
                    marker = "DICEFRE_BRIARBACK_ELEMENT_SITES_REFINED",
                    sites = Sites.Select((site, i) => new { site = site.name, position = points[i].ToString("F2") }).ToArray(),
                    recovery = RefinementRecoveryPath,
                    collisionFree = true,
                    navmeshChanged = false,
                    placementProvisional = true
                };
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        [CliCommand("dicefree.briarback.elements.validate",
            "Verify the four provisional elemental-site visuals add no colliders and preserve sampled routes.")]
        public static object Validate()
        {
            var scene = ActiveScene();
            var region = RegionRoot(scene);
            var group = region.Find(GroupName);
            Need(group != null, "provisional site group is missing.");
            var nav = AddNavigation(scene);
            try
            {
                var points = Preflight(scene);
                var roots = group.Cast<Transform>().ToArray();
                Need(roots.Length == Sites.Length, "expected four labeled settlement site groups.");
                foreach (var site in Sites)
                {
                    var root = group.Find(site.name);
                    Need(root != null, "missing labeled site: " + site.name);
                    Need(root.GetComponentsInChildren<Collider>(true).Length == 0,
                        site.name + " introduced collision.");
                }

                var routeCount = 0;
                var approach = CornbergLandscape.Ground(270f, 369f);
                Need(NavMesh.SamplePosition(approach, out var approachHit, 2f, NavMesh.AllAreas),
                    "existing northern approach lost its NavMesh point.");
                foreach (var site in Sites)
                {
                    var target = CornbergLandscape.Ground(site.position.x, site.position.y);
                    Need(NavMesh.SamplePosition(target, out var hit, 2f, NavMesh.AllAreas),
                        "site center lost saved NavMesh coverage.");
                    var path = new NavMeshPath();
                    Need(NavMesh.CalculatePath(approachHit.position, hit.position, NavMesh.AllAreas, path) &&
                         path.status == NavMeshPathStatus.PathComplete,
                        "site is disconnected from the northern approach navigation.");
                    routeCount++;
                }

                return new
                {
                    marker = "DICEFRE_BRIARBACK_ELEMENT_SITES_VALID",
                    sites = roots.Length,
                    visualObjects = group.GetComponentsInChildren<Transform>(true).Length - 1,
                    colliders = 0,
                    connectedSiteRoutes = routeCount,
                    savedNavMeshUnchanged = true,
                    recoveryCopyExists = System.IO.File.Exists(RecoveryPath),
                    gameplayDeferred = true
                };
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        [CliCommand("dicefree.briarback.elements.capture",
            "Render each saved provisional elemental settlement from gameplay height for visual review.")]
        public static object Capture()
        {
            var scene = ActiveScene();
            Need(RegionRoot(scene).Find(GroupName) != null, "provisional site blockout is missing.");
            var output = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "VisualBaselines");
            Directory.CreateDirectory(output);
            var files = Sites.Select(site =>
            {
                var target = CornbergLandscape.Ground(site.position.x, site.position.y) + Vector3.up * 2f;
                GameObject cameraObject = null;
                RenderTexture texture = null;
                Texture2D image = null;
                try
                {
                    cameraObject = new GameObject("Temporary elemental settlement review camera");
                    SceneManager.MoveGameObjectToScene(cameraObject, scene);
                    var camera = cameraObject.AddComponent<Camera>();
                    camera.orthographic = true;
                    camera.orthographicSize = 12f;
                    camera.clearFlags = CameraClearFlags.Skybox;
                    camera.transform.position = target + new Vector3(16f, 20f, -16f);
                    camera.transform.LookAt(target);
                    texture = new RenderTexture(1280, 720, 24);
                    camera.targetTexture = texture;
                    camera.Render();
                    var previous = RenderTexture.active;
                    RenderTexture.active = texture;
                    image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                    image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                    image.Apply();
                    RenderTexture.active = previous;
                    var file = Path.Combine(output, site.name.Replace(' ', '-') + ".png");
                    File.WriteAllBytes(file, image.EncodeToPNG());
                    return new { site = site.name, path = file, bytes = new FileInfo(file).Length };
                }
                finally
                {
                    if (cameraObject != null) UnityEngine.Object.DestroyImmediate(cameraObject);
                    if (image != null) UnityEngine.Object.DestroyImmediate(image);
                    if (texture != null)
                    {
                        texture.Release();
                        UnityEngine.Object.DestroyImmediate(texture);
                    }
                }
            }).ToArray();
            return new { marker = "DICEFRE_BRIARBACK_ELEMENT_SITES_CAPTURED", files };
        }

        [CliCommand("dicefree.briarback.elements.capture-overview",
            "Render a wide view of the northern approach and all four provisional site clearings.")]
        public static object CaptureOverview()
        {
            var scene = ActiveScene();
            Need(RegionRoot(scene).Find(GroupName) != null, "provisional site blockout is missing.");
            var target = new Vector3(267f, 0f, 405f);
            var cameraObject = new GameObject("Temporary elemental overview camera");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            RenderTexture texture = null;
            Texture2D image = null;
            try
            {
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 43f;
                camera.transform.position = target + Vector3.up * 110f;
                camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                texture = new RenderTexture(1600, 900, 24);
                camera.targetTexture = texture;
                camera.Render();
                var previous = RenderTexture.active;
                RenderTexture.active = texture;
                image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
                image.Apply();
                RenderTexture.active = previous;
                var file = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                    "VisualBaselines/Briarback-Elemental-Sites-Overview.png");
                File.WriteAllBytes(file, image.EncodeToPNG());
                return new { marker = "DICEFRE_BRIARBACK_ELEMENT_SITES_OVERVIEW", path = file, bytes = new FileInfo(file).Length };
            }
            finally
            {
                if (cameraObject != null) UnityEngine.Object.DestroyImmediate(cameraObject);
                if (image != null) UnityEngine.Object.DestroyImmediate(image);
                if (texture != null)
                {
                    texture.Release();
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }
        }
    }
}
