using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.EditorTools;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Skills;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class PriestEscortAuthoring
{
    private const string Settings = "Assets/_DiceFree/Settings/PriestEscort";
    private const string Preview = "Assets/_DiceFree/Settings/Development/Priest/";
    private const string ScenePath = "Assets/_DiceFree/Scenes/PriestEscort.unity";
    private const string CornbergPath = "Assets/_DiceFree/Scenes/Cornberg.unity";
    private const string Batch = "Assets/_DiceFree/Art/World/CornbergBatch/";
    private const string Blockout = "Assets/_DiceFree/Materials/Blockout/";
    private const string ForestFloor = "Assets/_DiceFree/Settings/WolfSurvival/Moonlit forest floor.mat";
    private const string PackedEarth = "Assets/_DiceFree/Settings/WolfSurvival/Trampled wolf trails.mat";
    private const float InspectorPace = 1.16f;
    private const float GuardPace = 4.2f;
    private const float GuardDamage = 31;
    private const float GuardInterval = 1.25f;
    private const float GuardReach = 1.18f;
    private const string TurnInHint = "Return to the Trimery healer to finish Priest advancement.";
    private static readonly Vector3 Origin = new(2400, 0, 2400);
    private static readonly float[] WaypointDepths = { 18, 44, 69, 94, 120, 146, 171, 200, 226 };

    public static object Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty))
            throw new InvalidOperationException("Clean Edit Mode required.");
        var setup = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            var world = EditorSceneManager.OpenScene(CornbergPath);
            var player = InScene<TraversalInput>(world).Single().GetComponent<CombatActor>();
            var healer = InScene<HealerInteraction>(world).Single(x => x.name == TrimeryHealerAuthoring.Name);
            if (healer.GetComponent<PriestMentor>() != null) throw new InvalidOperationException("Priest mentor already authored.");
            Directory.CreateDirectory("Logs");
            if (!File.Exists("Logs/Cornberg-before-priest-escort.unity"))
                File.Copy(CornbergPath, "Logs/Cornberg-before-priest-escort.unity");
            if (!AssetDatabase.IsValidFolder(Settings)) AssetDatabase.CreateFolder("Assets/_DiceFree/Settings", "PriestEscort");
            var catalog = Load<ClassCatalog>(AdvancementAuthoring.CatalogPath);
            var priest = Load<ActorDefinition>(Preview + "Priest preview.asset");
            var magical = catalog.Resolve(PriestMentor.ParentClassId);
            var arrival = WorldReturnPoints(world, player.transform.position, healer.ApproachPosition);
            var trial = Asset<QuestDefinition>("Cornberg Road Inspection");
            trial.stableId = PriestMentor.QuestId;
            trial.title = "The Cornberg Road Inspection";
            trial.requiredLevel = 30;
            trial.requiredClassId = PriestMentor.ParentClassId;
            trial.offer = "To become a Priest, escort our road inspector to Cornberg. The west road was broken deliberately. We want to know whether the Cornberg road is clear. Two guards will fight alongside you. Keep the inspector alive.";
            trial.activeDialogue = "Meet the inspector here to depart. Follow the party and heal them when wildlife attacks. If the inspector falls, we can send another escort free of charge.";
            trial.locationHint = "Depart from the Trimery healer. Escort the inspector to Cornberg, then return to the healer.";
            trial.turnInHint = TurnInHint;
            trial.stages = new[] { new QuestObjective { kind = ObjectiveKind.Escort, contentId = PriestEscortSite.RouteId,
                count = 1, instruction = "Bring the inspector safely to Cornberg" } };
            EditorUtility.SetDirty(trial);

            var edge = Asset<AdvancementDefinition>("Magically Touched to Priest");
            edge.stableId = "advancement.magically-touched.priest";
            edge.displayName = "Become a Priest";
            edge.sourceClass = magical;
            edge.targetClass = priest;
            edge.requiredLevel = 30;
            edge.requiredQuestId = trial.stableId;
            EditorUtility.SetDirty(edge);

            BuildInstance(magical.tuning);
            SceneManager.SetActiveScene(world);
            catalog.Configure(catalog.Classes.Append(priest).Distinct().ToArray());
            EditorUtility.SetDirty(catalog);
            var advancement = player.GetComponent<AdvancementController>();
            advancement.Configure(catalog, advancement.Definitions.Append(edge).ToArray());
            player.GetComponent<ManifestationPersistence>().ConfigureClassCatalog(catalog);
            var journal = player.GetComponent<QuestJournal>();
            journal.Configure(journal.Definitions.Append(trial).ToArray());
            InstallPriestKit(player.gameObject);
            var mentor = healer.gameObject.AddComponent<PriestMentor>();
            var context = healer.gameObject.AddComponent<ContextualNpc>();
            var approach = new GameObject("Priest mentor approach").transform;
            approach.SetParent(healer.transform);
            approach.position = healer.ApproachPosition;
            mentor.ConfigureName("Trimery Healer – Priest advancement");
            mentor.ConfigureApproach(approach);
            mentor.Configure(trial, healer, arrival);
            context.ConfigureName("Trimery Healer");
            context.ConfigureApproach(approach);
            context.Configure("npc.trimery.healer", mentor, healer);
            healer.GetComponent<InteractionPrompt>().Configure(context, player);
            EditorBuildSettings.scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath)
                .Append(new EditorBuildSettingsScene(ScenePath, true)).ToArray();
            EditorSceneManager.MarkSceneDirty(world);
            if (!EditorSceneManager.SaveScene(world)) throw new InvalidOperationException("Cornberg could not save.");
            AssetDatabase.SaveAssets();
            return new { quest = trial.stableId, scene = ScenePath, encounters = 4, waypoints = WaypointDepths.Length,
                returnToTrimery = approach.position.ToString(), cornbergArrival = arrival.ToString(), classes = catalog.Classes.Count };
        }
        finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
    }

    private static Vector3 WorldReturnPoints(Scene world, Vector3 noviceSpawn, Vector3 departure)
    {
        var data = InScene<WorldNavigation>(world).First(nav => nav.Data != null).Data;
        var registered = NavMesh.AddNavMeshData(data);
        try
        {
            if (!NavMesh.SamplePosition(departure, out _, 2, 1))
                throw new InvalidOperationException("Trimery healer approach is off the world navigation.");
            if (!NavMesh.SamplePosition(noviceSpawn + new Vector3(12, 0, 12), out var hit, 10, 1))
                throw new InvalidOperationException("No safe arrival point near Cornberg.");
            return hit.position;
        }
        finally { registered.Remove(); }
    }

    private static void InstallPriestKit(GameObject player)
    {
        var resources = player.GetComponent<ActorResourceController>();
        resources.Configure(resources.Profiles.Concat(new[] { Load<ClassResourceProfile>(Preview + "Priest Mana.asset"),
            Load<ClassResourceProfile>(Preview + "Priest Grace.asset") }).ToArray());
        player.AddComponent<PriestSkillProgression>();
        player.AddComponent<PriestSkillCaster>().Configure(Load<AttackDefinition>(Preview + "Holy Light.asset"));
        player.AddComponent<PriestAbilityProvider>();
        var presentation = new SerializedObject(player.GetComponent<ClassPresentationSwitcher>());
        var bindings = presentation.FindProperty("bindings");
        var caster = Enumerable.Range(0, bindings.arraySize).Select(bindings.GetArrayElementAtIndex)
            .Single(x => x.FindPropertyRelative("classId").stringValue == PriestMentor.ParentClassId);
        var root = caster.FindPropertyRelative("visualRoot").objectReferenceValue;
        var animator = caster.FindPropertyRelative("animator").objectReferenceValue;
        int index = bindings.arraySize;
        bindings.InsertArrayElementAtIndex(index);
        var priest = bindings.GetArrayElementAtIndex(index);
        priest.FindPropertyRelative("classId").stringValue = PriestSkillProgression.PriestClassId;
        priest.FindPropertyRelative("visualRoot").objectReferenceValue = root;
        priest.FindPropertyRelative("animator").objectReferenceValue = animator;
        presentation.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BuildInstance(CombatTuning tuning)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        var root = new GameObject("Private Trimery to Cornberg road").transform;
        root.position = Origin;
        var site = root.gameObject.AddComponent<PriestEscortSite>();
        var floor = Strip("Rolling forest floor", root, -38, 272, 65, 0, Load<Material>(ForestFloor));
        floor.layer = 8;
        var floorMesh = floor.GetComponent<MeshFilter>().sharedMesh;
        floorMesh.SetPreBakeCollisionMesh(false, true);
        EditorUtility.SetDirty(floorMesh);
        floor.AddComponent<MeshCollider>().sharedMesh = floorMesh;
        Strip("Packed road to Cornberg", root, -16, 249, 4.5f, .05f, Load<Material>(PackedEarth));
        DressRoad(root);
        DressRoadside(root);
        site.route = WaypointDepths.Select(depth => Origin + Ground(Centre(depth), depth)).ToArray();
        site.playerSpawn = Origin + Ground(Centre(0), -3);
        site.mapBounds = new Rect(Origin.x - 61, Origin.z - 35, 122, 303);
        var guardAttack = Strike("Escort guard sword", "attack.priest-escort.guard", GuardDamage, GuardInterval, GuardReach);
        var wolfAttack = Strike("Escort wolf bite", "attack.priest-escort.wolf", 24, 1.68f, .8f);
        var slimeAttack = Strike("Escort slime bump", "attack.priest-escort.slime", 27, 1.82f, .85f);
        var inspector = Stats("Road Inspector", "npc.priest-escort.inspector", tuning, null, 410, InspectorPace);
        var guard = Stats("Escort Guard", "npc.priest-escort.guard", tuning, guardAttack, 365, GuardPace);
        var wolf = Stats("Roadside Wolf", "enemy.priest-escort.wolf", tuning, wolfAttack, 195, 4.2f);
        var slime = Stats("Road Slime", "enemy.priest-escort.slime", tuning, slimeAttack, 230, 3.25f);
        site.inspector = Actor(root, inspector, Batch + "NPCs/NPC_Herbalist.fbx", Ground(Centre(0), 0), 0, .44f,
            Load<Material>(Blockout + "Limewash.mat"));
        site.guards = new[] {
            Actor(root, guard, Batch + "NPCs/NPC_Guard.fbx", Ground(Centre(-1)-1.85f, -1), 0, .46f, Load<Material>(Blockout + "Timber.mat")),
            Actor(root, guard, Batch + "NPCs/NPC_Guard.fbx", Ground(Centre(-1)+1.85f, -1), 0, .46f, Load<Material>(Blockout + "Timber.mat"))
        };
        var waveWaypoints = new[] { 1, 3, 5, 7 };
        site.encounters = waveWaypoints.Select((waypoint, index) =>
        {
            int count = index == 0 ? 2 : index == 3 ? 4 : 3;
            var group = new GameObject("Road ambush " + (index + 1)).transform;
            group.SetParent(root, false);
            var hostiles = new CombatActor[count];
            for (int i = 0; i < count; i++)
            {
                float depth = WaypointDepths[waypoint] + 5.5f + i % 2 * 2;
                float x = Centre(depth) + (i % 2 == 0 ? -1 : 1) * (5.2f + i / 2 * 2);
                bool isWolf = index == 0 || index == 3 && i % 2 == 0 || index == 2 && i == 0;
                hostiles[i] = Actor(group, isWolf ? wolf : slime, isWolf ? "Assets/_DiceFree/Art/Creatures/Ironpine/Wolf.fbx" :
                    Batch + "Slimes/Slime_Road.fbx", Ground(x, depth), 1, .5f,
                    Load<Material>(Blockout + (isWolf ? "Fieldstone.mat" : "Crop Slime green.mat")));
                hostiles[i].gameObject.SetActive(false);
            }
            return new PriestEscortSite.Encounter { waypoint = waypoint, hostiles = hostiles };
        }).ToArray();

        Physics.SyncTransforms();
        var sources = new List<NavMeshBuildSource>();
        var bounds = new Bounds(Origin + new Vector3(0, 3, 115), new Vector3(134, 30, 324));
        NavMeshBuilder.CollectSources(bounds, 1 << 8, NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), sources);
        var data = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(0), sources, bounds, Vector3.zero, Quaternion.identity);
        if (data == null) throw new InvalidOperationException("Escort navigation did not bake.");
        string path = Settings + "/Escort NavMesh.asset";
        var existing = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
        if (existing == null) AssetDatabase.CreateAsset(data, path);
        else { EditorUtility.CopySerialized(data, existing); Object.DestroyImmediate(data); data = existing; }
        root.gameObject.AddComponent<WorldNavigation>().Configure(data);
        var nav = NavMesh.AddNavMeshData(data);
        try
        {
            var points = site.route.Append(site.playerSpawn).Concat(site.guards.Select(x => x.transform.position))
                .Append(site.inspector.transform.position).Concat(site.encounters.SelectMany(x => x.hostiles.Select(enemy => enemy.transform.position)));
            foreach (var point in points)
                if (!NavMesh.SamplePosition(point, out _, .7f, 1))
                    throw new InvalidOperationException("Escort route point is off navigation: " + point);
        }
        finally { nav.Remove(); }
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Escort scene did not save.");
        EditorSceneManager.CloseScene(scene, true);
    }

    public static object TuneExisting()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty))
            throw new InvalidOperationException("Clean Edit Mode required.");
        var setup = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var root = scene.GetRootGameObjects().Single(x => x.GetComponent<PriestEscortSite>() != null).transform;
            root.Find("Rolling forest floor").GetComponent<Renderer>().sharedMaterial = Load<Material>(ForestFloor);
            root.Find("Packed road to Cornberg").GetComponent<Renderer>().sharedMaterial = Load<Material>(PackedEarth);
            DressRoadside(root);
            var inspector = Load<ActorDefinition>(Settings + "/Road Inspector.asset");
            inspector.moveSpeed = InspectorPace;
            var guard = Load<ActorDefinition>(Settings + "/Escort Guard.asset");
            guard.moveSpeed = GuardPace;
            var strike = guard.basicAttack;
            strike.baseDamage = GuardDamage;
            strike.interval = GuardInterval;
            strike.reach = GuardReach;
            var quest = Load<QuestDefinition>(Settings + "/Cornberg Road Inspection.asset");
            quest.turnInHint = TurnInHint;
            foreach (var asset in new Object[] { inspector, guard, strike, quest }) EditorUtility.SetDirty(asset);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Escort polish did not save.");
            AssetDatabase.SaveAssets();
            return new { inspectorSpeed = inspector.moveSpeed, guardSpeed = guard.moveSpeed, guardDamage = strike.baseDamage };
        }
        finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
    }

    private static CombatActor Actor(Transform parent, ActorDefinition stats, string model, Vector3 local,
        int faction, float radius, Material fallback)
    {
        var root = new GameObject(stats.displayName);
        root.layer = 10;
        root.transform.SetParent(parent, false);
        root.transform.localPosition = local;
        var capsule = root.AddComponent<CapsuleCollider>();
        capsule.height = faction == 0 ? 2f : 1.1f;
        capsule.center = Vector3.up * capsule.height * .5f;
        capsule.radius = radius;
        var agent = root.AddComponent<NavMeshAgent>();
        agent.enabled = false;
        agent.areaMask = 1;
        agent.height = capsule.height;
        agent.radius = radius;
        root.AddComponent<TraversalMotor>();
        root.AddComponent<ActorStats>().Configure(stats, faction == 0 ? 30 : 20);
        root.AddComponent<Health>();
        var actor = root.AddComponent<CombatActor>();
        actor.Configure(faction, radius);
        root.AddComponent<CombatStatusController>();
        if (stats.basicAttack != null) root.AddComponent<BasicAttack>();
        var visual = Model(parent: root.transform, path: model, position: Vector3.zero, scale: faction == 0 ? 1.18f : 1.05f, fallback: fallback);
        root.AddComponent<ActorFeedback>().Configure(visual.transform);
        return actor;
    }

    private static ActorDefinition Stats(string name, string id, CombatTuning tuning, AttackDefinition attack, float hp, float speed)
    {
        var stats = Asset<ActorDefinition>(name);
        stats.stableId = id;
        stats.displayName = name;
        stats.tuning = tuning;
        stats.basicAttack = attack;
        stats.baseHp = hp;
        stats.moveSpeed = speed;
        stats.baseAttributes = new AttributeValues(0);
        stats.growth = new AttributeValues(0);
        stats.experienceReward = 0;
        EditorUtility.SetDirty(stats);
        return stats;
    }

    private static AttackDefinition Strike(string name, string id, float damage, float interval, float range)
    {
        var attack = Asset<AttackDefinition>(name);
        attack.stableId = id;
        attack.displayName = name;
        attack.baseDamage = damage;
        attack.coefficient = 0;
        attack.requiresAccuracy = false;
        attack.interval = interval;
        attack.windup = .4f;
        attack.reach = range;
        EditorUtility.SetDirty(attack);
        return attack;
    }

    private static float Centre(float z) => 8 * Mathf.Sin(z * .035f) + 5 * Mathf.Sin(z * .016f);
    private static Vector3 Ground(float x, float z)
    {
        float offRoad = Mathf.SmoothStep(0, 1, Mathf.Clamp01((Mathf.Abs(x - Centre(z)) - 4) / 17));
        float rolling = .58f * Mathf.Sin(z * .029f) + 1.2f * offRoad * Mathf.PerlinNoise(x * .042f + 14, z * .031f + 7);
        return new Vector3(x, rolling, z);
    }

    private static GameObject Strip(string name, Transform parent, float start, float end, float halfWidth, float lift, Material material)
    {
        const int columns = 24;
        int rows = Mathf.CeilToInt((end - start) / 2);
        var vertices = new List<Vector3>();
        var uv = new List<Vector2>();
        var indices = new List<int>();
        for (int z = 0; z <= rows; z++)
        {
            float depth = Mathf.Lerp(start, end, z / (float)rows);
            for (int x = 0; x <= columns; x++)
            {
                float width = halfWidth < 10 ? halfWidth + .45f * Mathf.Sin(depth * .22f) : halfWidth;
                var point = Ground(Centre(depth) + Mathf.Lerp(-width, width, x / (float)columns), depth);
                vertices.Add(point + Vector3.up * lift);
                uv.Add(new Vector2(point.x, point.z) / 5);
                if (z == rows || x == columns) continue;
                int a = z * (columns + 1) + x;
                int b = a + columns + 1;
                indices.AddRange(new[] { a, b, a + 1, a + 1, b, b + 1 });
            }
        }
        var mesh = new Mesh { name = name };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uv);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        string path = Settings + "/" + name + ".asset";
        var saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (saved == null) AssetDatabase.CreateAsset(mesh, path);
        else { EditorUtility.CopySerialized(mesh, saved); Object.DestroyImmediate(mesh); mesh = saved; }
        var root = new GameObject(name);
        root.layer = 2;
        root.transform.SetParent(parent, false);
        root.AddComponent<MeshFilter>().sharedMesh = mesh;
        root.AddComponent<MeshRenderer>().sharedMaterial = material;
        return root;
    }

    private static void DressRoad(Transform root)
    {
        var foliage = Load<Material>(Blockout + "Sunlit foliage.mat");
        var wood = Load<Material>(Blockout + "Timber.mat");
        var stone = Load<Material>(Blockout + "Fieldstone.mat");
        var random = new System.Random(411);
        for (int i = 0; i < 116; i++)
        {
            float z = -29 + i % 29 * 10.3f + (float)random.NextDouble() * 4.5f;
            float x = Centre(z) + (i / 29 % 2 == 0 ? -1 : 1) * (13 + i / 58 * 15 + (float)random.NextDouble() * 9);
            var model = Model(root, Batch + "Trees/Tree_" + (i % 4 == 0 ? "Spruce" : "Pine") + ".fbx", Ground(x, z),
                .9f + (float)random.NextDouble() * .7f, foliage);
            model.transform.localRotation = Quaternion.Euler(0, (float)random.NextDouble() * 360, 0);
            model.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(18);
        }
        for (int i = 0; i < 26; i++)
        {
            float z = -11 + i * 10 + (float)random.NextDouble() * 4;
            float x = Centre(z) + (i % 2 == 0 ? 1 : -1) * (6.4f + i % 4 * .9f);
            var boulder = Shape(root, "Weathered roadside stone", PrimitiveType.Sphere, Ground(x, z) + Vector3.up * .2f,
                new Vector3(1.35f + i % 4 * .38f, .65f + i % 3 * .25f, 1.2f), stone);
            boulder.transform.localRotation = Quaternion.Euler(8, i * 37, 13);
        }
        Sign(root, "TRIMERY", -6, wood);
        Sign(root, "CORNBERG", 226, wood);
        foreach (float depth in new[] { 33f, 80f, 112f, 158f, 210f })
        {
            Vector3 position = Ground(Centre(depth) + 5.6f, depth);
            Shape(root, "Roadside marker", PrimitiveType.Cube, position + Vector3.up * .55f, new Vector3(.31f, 1.1f, .31f), wood);
            Shape(root, "Stone marker cap", PrimitiveType.Sphere, position + Vector3.up * 1.07f, new Vector3(.38f, .24f, .38f), stone);
        }
        var sun = new GameObject("Afternoon light").AddComponent<Light>();
        sun.transform.SetParent(root, false);
        sun.type = LightType.Directional;
        sun.color = new Color(1f, .91f, .72f);
        sun.intensity = .45f;
        sun.transform.rotation = Quaternion.Euler(46, -40, 0);
    }

    private static void DressRoadside(Transform root)
    {
        if (root.Find("Roadside timber and undergrowth") != null) return;
        var detail = new GameObject("Roadside timber and undergrowth").transform;
        detail.SetParent(root, false);
        var wood = Load<Material>(Blockout + "Timber.mat");
        var moss = Load<Material>(Blockout + "Sunlit foliage.mat");
        const string props = "Assets/_DiceFree/Art/World/VisualBatch01/Models/";
        for (int i = 0; i < 16; i++)
        {
            float z = 11 + i * 13.25f;
            float direction = i % 2 == 0 ? -1 : 1;
            float x = Centre(z) + direction * (8.2f + 2.7f * Mathf.Sin(i * 2.4f));
            var stump = Model(detail, props + "stump_0" + (i % 3 + 1) + ".fbx", Ground(x, z), 1.15f, wood);
            stump.transform.localRotation = Quaternion.Euler(0, i * 41, 0);
            if (i % 3 == 0)
            {
                var log = Model(detail, props + "log_02.fbx", Ground(x + direction * 2, z + 1.8f), 1.35f, wood);
                log.transform.localRotation = Quaternion.Euler(0, i * 37, 0);
            }
            for (int j = 0; j < 5; j++)
            {
                float xx = x + Mathf.Sin(j * 4.6f) * 2.6f;
                float zz = z + Mathf.Cos(j * 4.6f) * 3.1f;
                Shape(detail, "Roadside ground cover", PrimitiveType.Sphere, Ground(xx, zz) + Vector3.up * .12f,
                    new Vector3(.52f + .13f * j, .22f + .09f * (j % 3), .58f), moss);
            }
        }
    }

    private static void Sign(Transform root, string title, float depth, Material wood)
    {
        Vector3 p = Ground(Centre(depth) - 5.8f, depth);
        Shape(root, title + " sign post", PrimitiveType.Cube, p + Vector3.up, new Vector3(.23f, 2, .23f), wood);
        var panel = Shape(root, title + " sign board", PrimitiveType.Cube, p + Vector3.up * 1.95f,
            new Vector3(3.25f, .55f, .2f), wood);
        panel.transform.localRotation = Quaternion.Euler(0, 37, 0);
        var label = new GameObject(title + " lettering").AddComponent<TextMesh>();
        label.transform.SetParent(panel.transform, false);
        label.transform.localPosition = new Vector3(0, 0, -.51f);
        label.transform.localScale = new Vector3(1 / 3.25f, 1 / .55f, 1 / .2f);
        label.text = title;
        label.fontSize = 42;
        label.characterSize = .036f;
        label.anchor = TextAnchor.MiddleCenter;
        label.color = new Color(.98f, .9f, .69f);
    }

    private static GameObject Shape(Transform parent, string name, PrimitiveType kind, Vector3 position, Vector3 scale, Material material)
    {
        var part = GameObject.CreatePrimitive(kind);
        part.name = name;
        part.layer = 2;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(part.GetComponent<Collider>());
        return part;
    }

    private static GameObject Model(Transform parent, string path, Vector3 position, float scale, Material fallback)
    {
        var model = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(path));
        model.transform.SetParent(parent, false);
        model.transform.localPosition = position;
        model.transform.localScale *= scale;
        foreach (var part in model.GetComponentsInChildren<Transform>(true)) part.gameObject.layer = 2;
        foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
            renderer.sharedMaterials = renderer.sharedMaterials.Select(material =>
            {
                string key = material == null ? "" : material.name.Replace("DF_", "").Replace("_mesh", "");
                return AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Materials/CornbergBatch/" + key + "_0.mat") ?? fallback;
            }).ToArray();
        return model;
    }

    private static T Load<T>(string path) where T : Object =>
        AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidOperationException("Missing asset: " + path);

    private static T Asset<T>(string name) where T : ScriptableObject
    {
        string path = Settings + "/" + name + ".asset";
        var value = AssetDatabase.LoadAssetAtPath<T>(path);
        if (value != null) return value;
        value = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(value, path);
        return value;
    }

    private static IEnumerable<T> InScene<T>(Scene scene) where T : Component =>
        scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true));
}
