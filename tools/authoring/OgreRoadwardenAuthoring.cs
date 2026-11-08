using System;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.EditorTools;
using DiceFree.Quests;
using DiceFree.Trimery;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static partial class OgreRoadwardenAuthoring
{
    private const string Folder = "Assets/_DiceFree/Settings/Trimery/Roadwarden";
    private const string SurveyId = "quest.arborium.road-survey";
    private const string EnemyId = "enemy.ogre-roadwarden";
    private const string FamilyId = "ogre.roadwarden";
    private const string RootName = "Trimery highway - Roadwarden and Guild repair";
    private static readonly Vector3 Passage = new(203.5f, 0, 193.5f);
    private static readonly Vector3 Camp = new(201.5f, 0, 206.5f);

    public static object Author()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Use a saved scene in Edit Mode.");

        var scene = SceneManager.GetActiveScene();
        if (scene.path != CornbergSceneBuilder.ScenePath)
            scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
        var journal = Object.FindObjectsByType<QuestJournal>().Single(j => j.Definitions.Any(q => q.stableId == SurveyId));
        var steward = Object.FindObjectsByType<QuestGiver>().Single(g => g.Offers.Any(q => q != null && q.stableId == SurveyId));
        var wreck = Find("Broken western trade road (no Roadwarden yet)");
        var fractured = Find("Western highway - fractured setts and split trunks");
        var sign = Find("Western highway warning sign");
        var laneTree = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Single(t =>
            t.name == "Woodland tree - logging source 21" && t.GetComponentsInChildren<Collider>(true).Any(c =>
                c.gameObject.layer == 9 && Mathf.Abs(c.transform.position.x - 203.75f) < .3f &&
                Mathf.Abs(c.transform.position.z - 191.10f) < .3f)).gameObject;
        var old = GameObject.Find(RootName);
        if (old != null) Object.DestroyImmediate(old);

        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();
        Palette();
        var quest = CreateQuest();
        journal.Configure(journal.Definitions.Where(q => q != quest).Append(quest).ToArray());
        steward.ConfigureAdditional(steward.Offers.Skip(1).Where(q => q != quest).Append(quest).ToArray());
        EditorUtility.SetDirty(steward);
        EditorUtility.SetDirty(journal);

        var root = new GameObject(RootName).transform;
        var cliffs = Node(root, "Wooded banks flanking the highway");
        BuildBanks(cliffs);
        var trails = Node(root, "Guild trail from Arborium to the wreck");
        LayRoad(trails, new[]
        {
            P(184,0,304.7f), P(193,0,298), P(198,0,284), P(203,0,276), P(202,0,261),
            P(211,0,233), P(211,0,218), P(206,0,211), Camp, P(201,0,200), P(202.5f,0,197),
        }, false);
        AddWaymarker(trails, "Trimery trail waypost", P(195,0,301), 36, false);
        AddWaymarker(trails, "Guild contract waypost", P(212.8f,0,216), 88, false);
        BuildCamp(trails);

        var jam = Node(root, "Ogre barricade - felled pine and broken roadbed", Grounded(Passage));
        BuildJam(jam);
        var open = Node(root, "Reopened highway - dressed stone and Guild lamps");
        LayRoad(open, new[] { P(202.5f,0,197), Passage, P(205,0,188), P(208,0,183.5f), P(214,0,180), P(220,0,180) }, true);
        AddWaymarker(open, "Road reopened waypost - Arborium side", P(206.7f,0,197), 10, true);
        AddWaymarker(open, "Road reopened waypost - Trimery side", P(213.8f,0,177.8f), 85, true);
        AddLamp(open, P(200.8f,0,194.5f), 35);
        AddLamp(open, P(208.1f,0,188.8f), 140);

        var boss = CreateBoss(root);
        var repairPoint = Node(root, "Guild road crew - clear the barricade", Grounded(P(205.5f,0,199.1f)));
        var crew = Node(repairPoint, "Repair crew, winch and handcart");
        BuildCrew(crew);
        var approach = Node(repairPoint, "Crew approach", P(-.35f,0,1.45f));
        var repair = repairPoint.gameObject.AddComponent<TimedInteractionTarget>();
        repair.ConfigureName("Guild Road Crew");
        repair.ConfigureApproach(approach);
        repair.Configure(OgreRoadwardenRoute.RepairId, "Clearing the highway", 4.2f);
        var clickable = repairPoint.gameObject.AddComponent<BoxCollider>();
        repairPoint.gameObject.layer = 11;
        clickable.center = P(.1f,.87f,-.3f);
        clickable.size = P(3.0f,1.74f,1.65f);
        var route = repairPoint.gameObject.AddComponent<OgreRoadwardenRoute>();
        var closed = new[] { wreck, fractured, sign, laneTree, jam.gameObject };
        route.Configure(journal, boss, closed, crew.gameObject, open.gameObject);
        boss.gameObject.SetActive(false);
        crew.gameObject.SetActive(false);
        clickable.enabled = false;
        open.gameObject.SetActive(false);

        // Rebuild with the permanent banks and clear road. The live wreck's obstacle
        // carves only while the quest still owns the closed state.
        foreach (var part in closed) part.SetActive(false);
        CornbergSceneBuilder.Rebake();
        foreach (var part in closed) part.SetActive(true);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return new { quest = quest.stableId, boss = boss.name, root = root.name, scene = scene.path };
    }

    private static GameObject Find(string name) => Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Single(t => t.name == name).gameObject;

    private static T Asset<T>(string name) where T : ScriptableObject
    {
        string path = Folder + "/" + name + ".asset";
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static QuestDefinition CreateQuest()
    {
        var quest = Asset<QuestDefinition>("The Roadwarden's Order");
        quest.stableId = OgreRoadwardenRoute.QuestId;
        quest.title = "The Roadwarden's Order";
        quest.offer = "The survey confirms it. That road was torn up from the Arborium side. Follow the old track south from our East Gate and deal with the Ogre who still holds it. The Guild crew will wait until you give them the all-clear.";
        quest.activeDialogue = "Take Arborium's East Gate and follow the old track south. Watch the Ogre's hammer. Once he's down, lend the Guild road crew a hand at the barricade.";
        quest.locationHint = "Arborium East Gate, then south along the old Trimery highway.";
        quest.turnInHint = "The Guild road crew is waiting on the Arborium side of the barricade.";
        quest.completedQuestIds = new[] { SurveyId };
        quest.rewardXp = 6000;
        quest.completeOnObjectives = true;
        quest.stages = new[]
        {
            new QuestObjective
            {
                kind = ObjectiveKind.Kill, contentId = EnemyId, familyId = FamilyId, count = 1,
                instruction = "Defeat the Ogre Roadwarden on the Arborium side of the highway"
            },
            new QuestObjective
            {
                kind = ObjectiveKind.Interact, contentId = OgreRoadwardenRoute.RepairId, count = 1,
                instruction = "Help the Guild crew reopen the road. The Ogre's unsigned order says to keep it closed."
            }
        };
        EditorUtility.SetDirty(quest);
        return quest;
    }

    private static AttackDefinition Attack(string title, string id, float damage, float windup, float interval, float reach, bool accuracy)
    {
        var attack = Asset<AttackDefinition>(title);
        attack.stableId = "attack.roadwarden." + id;
        attack.displayName = title;
        attack.channel = DamageChannel.Physical;
        attack.requiresAccuracy = accuracy;
        attack.baseDamage = damage;
        attack.coefficient = 0;
        attack.minimumDamageMultiplier = attack.maximumDamageMultiplier = 1;
        attack.windup = windup;
        attack.interval = interval;
        attack.reach = reach;
        EditorUtility.SetDirty(attack);
        return attack;
    }

    private static CombatActor CreateBoss(Transform root)
    {
        var definition = Asset<ActorDefinition>("Ogre Roadwarden");
        definition.stableId = EnemyId;
        definition.displayName = "Ogre Roadwarden";
        definition.familyId = FamilyId;
        definition.tags = new[] { "ogre", "elite", "trimery" };
        definition.tuning = AssetDatabase.LoadAssetAtPath<CombatTuning>("Assets/_DiceFree/Settings/Combat/Cornberg provisional tuning.asset");
        definition.baseAttributes = new AttributeValues { strength = 18, vitality = 18, agility = 14 };
        definition.growth = default;
        definition.baseHp = 1190;
        definition.overrideVitalityHp = definition.overrideVitalityRegeneration = true;
        definition.hpPerVitality = definition.regenerationPerVitality = 0;
        definition.physicalDefense = 15;
        definition.magicalDefense = 12;
        definition.moveSpeed = 3.15f;
        definition.basicAttack = Attack("Iron hammer", "hammer", 43, .85f, 2.3f, 1.45f, true);
        EditorUtility.SetDirty(definition);

        var boss = Node(root, definition.displayName, Grounded(Camp + P(-1.2f,0,-.8f)));
        boss.gameObject.SetActive(false);
        boss.gameObject.layer = 10;
        var agent = boss.gameObject.AddComponent<NavMeshAgent>();
        agent.enabled = false;
        agent.radius = 1.05f;
        agent.height = 3.85f;
        agent.areaMask = 1;
        var body = boss.gameObject.AddComponent<CapsuleCollider>();
        body.center = Vector3.up * 1.93f;
        body.radius = 1.02f;
        body.height = 3.85f;
        boss.gameObject.AddComponent<ActorStats>().Configure(definition, 32);
        var actor = boss.gameObject.AddComponent<CombatActor>();
        actor.Configure(1, 1.05f, FamilyId);
        boss.gameObject.AddComponent<DefeatReporter>();
        boss.gameObject.AddComponent<CombatStatusController>().ConfigureControl(.45f, false);
        var aggro = boss.gameObject.AddComponent<AggroBehaviour>();
        aggro.Configure(13, 24);
        aggro.ConfigureHomePosition(boss.position);
        aggro.ConfigureAutoAggro(new AutoAggroPolicy { alwaysAutoAggro = true });
        boss.gameObject.AddComponent<ActorFeedback>();

        var rig = Node(boss, "Handcrafted Ogre rig");
        SculptOgre(rig, out var hammerArm, out var freeArm);
        var marker = Node(boss, "Terrain-following hazard boundary").gameObject.AddComponent<LineRenderer>();
        marker.sharedMaterial = markerMat;
        marker.useWorldSpace = true;
        marker.alignment = LineAlignment.View;
        marker.numCornerVertices = 3;
        marker.shadowCastingMode = ShadowCastingMode.Off;
        marker.receiveShadows = false;
        marker.enabled = false;
        var fight = boss.gameObject.AddComponent<OgreRoadwardenEncounter>();
        fight.Configure(Attack("Hammerfall", "hammerfall", 134, 1.55f, 1.55f, 9, false),
            Attack("Roadbreak", "roadbreak", 106, 1.8f, 1.8f, 4.8f, false), rig, hammerArm, freeArm, marker);
        return actor;
    }
}


public static partial class OgreRoadwardenAuthoring
{
    private static Material skin, skinLight, hide, wrap, iron, ironLight, brass, linen, burgundy, guild;
    private static Material stone, shale, moss, bark, pine, pineShade, earth, pavers, amber, markerMat;
    private static Mesh bevel;

    private static void Palette()
    {
        skin = Mat("Ogre · slate olive skin", "60705A");
        skinLight = Mat("Ogre · lit olive skin", "7A8563");
        hide = Mat("Brown tanned hide", "51352B");
        wrap = Mat("Hemp binding", "927552");
        iron = Mat("Blackened forged iron", "3E464A", .46f);
        ironLight = Mat("Worn iron edges", "849095", .42f);
        brass = Mat("Hammered brass", "B78B54", .33f);
        linen = Mat("Bone and coarse linen", "D6B883");
        burgundy = Mat("Ogre · oxblood wool", "8E4939");
        guild = Mat("Guild · indigo wool", "536B88");
        stone = Mat("Warm grey road granite", "827970");
        shale = Mat("Shadowed slate", "625F5B");
        moss = Mat("Sage moss", "687649");
        bark = Mat("Split pine bark", "584736");
        pine = Mat("Pine needles · sunlit", "526854");
        pineShade = Mat("Pine needles · deep green", "354A40");
        earth = Mat("Weathered highway gravel", "A09077");
        pavers = Mat("Restored limestone setts", "A49B89");
        amber = Mat("Lantern amber", "FFD68A", 0, true);
        markerMat = Mat("Attack warning", "FFFFFF", 0, true);
        bevel = BevelMesh();
    }

    private static Material Mat(string name, string hex, float metallic = 0, bool unlit = false)
    {
        string path = Folder + "/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        ColorUtility.TryParseHtmlString("#" + hex, out var color);
        mat.SetColor("_BaseColor", color);
        if (!unlit)
        {
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", metallic > 0 ? .33f : .12f);
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Vector3 P(float x, float y, float z) => new(x, y, z);

    private static Transform Node(Transform parent, string name, Vector3 position = default, Quaternion rotation = default)
    {
        var node = new GameObject(name).transform;
        node.SetParent(parent, false);
        node.localPosition = position;
        node.localRotation = rotation == default ? Quaternion.identity : rotation;
        return node;
    }

    private static Vector3 Grounded(Vector3 point)
    {
        if (!Physics.Raycast(P(point.x,35,point.z), Vector3.down, out var hit, 68, 1 << 8, QueryTriggerInteraction.Ignore))
            throw new InvalidOperationException("No ground at " + point);
        point.y += hit.point.y;
        return point;
    }

    private static Transform Rock(Transform root, string label, Vector3 point, Vector3 size, Material mat,
        Vector3 rotation = default)
    {
        var obj = Node(root, label, point, Quaternion.Euler(rotation));
        obj.localScale = size;
        obj.gameObject.AddComponent<MeshFilter>().sharedMesh = bevel;
        obj.gameObject.AddComponent<MeshRenderer>().sharedMaterial = mat;
        return obj;
    }

    private static Transform Shape(Transform root, PrimitiveType type, string label, Vector3 point, Vector3 size,
        Material mat, Vector3 rotation = default)
    {
        var obj = GameObject.CreatePrimitive(type);
        obj.name = label;
        Object.DestroyImmediate(obj.GetComponent<Collider>());
        obj.transform.SetParent(root, false);
        obj.transform.localPosition = point;
        obj.transform.localScale = size;
        obj.transform.localRotation = Quaternion.Euler(rotation);
        obj.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return obj.transform;
    }

    private static Transform Oval(Transform root, string name, Vector3 point, Vector3 size, Material mat, Vector3 rotation = default) =>
        Shape(root, PrimitiveType.Sphere, name, point, size, mat, rotation);

    private static Transform Rod(Transform root, string name, Vector3 start, Vector3 end, float width, Material mat)
    {
        var rod = Shape(root, PrimitiveType.Cylinder, name, (start + end) / 2f,
            P(width, Vector3.Distance(start,end) / 2f, width), mat);
        rod.localRotation = Quaternion.FromToRotation(Vector3.up, (end - start).normalized);
        return rod;
    }

    private static Mesh BevelMesh()
    {
        var mesh = MeshAsset("Roadwarden cut stone");
        float[] heights = { -.5f, -.37f, .34f, .5f };
        float[] scales = { .68f, 1, .96f, .70f };
        Vector2[] ring =
        {
            new(-.31f,-.5f), new(.31f,-.5f), new(.5f,-.31f), new(.5f,.31f),
            new(.31f,.5f), new(-.31f,.5f), new(-.5f,.31f), new(-.5f,-.31f)
        };
        var vertices = new System.Collections.Generic.List<Vector3>();
        var triangles = new System.Collections.Generic.List<int>();
        Vector3 Point(int row, int column) => P(ring[column].x * scales[row], heights[row], ring[column].y * scales[row]);
        void Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int n = vertices.Count;
            vertices.AddRange(new[] { a, b, c, d });
            triangles.AddRange(new[] { n, n+1, n+2, n, n+2, n+3 });
        }
        for (int i = 0; i < ring.Length; i++)
        {
            int next = (i + 1) % ring.Length;
            for (int row = 0; row < heights.Length - 1; row++)
                Face(Point(row,i), Point(row+1,i), Point(row+1,next), Point(row,next));
            Face(P(0,heights[0],0), Point(0,i), Point(0,next), P(0,heights[0],0));
            Face(P(0,heights[3],0), Point(3,next), Point(3,i), P(0,heights[3],0));
        }
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        EditorUtility.SetDirty(mesh);
        return mesh;
    }

    private static void SculptOgre(Transform rig, out Transform hammerArm, out Transform freeArm)
    {
        Oval(rig, "Broad lower torso", P(0,1.78f,0), P(1.82f,1.48f,1.13f), hide);
        Oval(rig, "Ogre chest and abdomen", P(0,2.37f,.09f), P(2.06f,1.63f,1.31f), skin);
        Oval(rig, "Massive neck", P(0,3.06f,.12f), P(.89f,.94f,.82f), skin);
        Oval(rig, "Oilskin back cloak", P(0,2.24f,-.44f), P(1.76f,1.62f,.52f), burgundy);
        Oval(rig, "Leather war belt", P(0,1.66f,.04f), P(1.97f,.41f,1.26f), hide);
        Rock(rig, "Guild chain broken into a belt clasp", P(0,1.68f,.72f), P(.52f,.43f,.22f), brass, P(0,0,12));
        Rock(rig, "Belt clasp dark inset", P(0,1.68f,.855f), P(.28f,.22f,.08f), iron);
        Rock(rig, "Oxblood tabard", P(.03f,1.13f,.49f), P(.57f,1.05f,.18f), burgundy, P(-6,4,4));
        for (int side = -1; side <= 1; side += 2)
        {
            Oval(rig, "Leather breeches", P(side*.51f,1.10f,0), P(.74f,1.15f,.88f), hide, P(0,0,side*8));
            Oval(rig, "Iron greave", P(side*.62f,.53f,.11f), P(.64f,.74f,.68f), iron);
            Rock(rig, "Greave edge", P(side*.63f,.58f,.43f), P(.46f,.41f,.12f), ironLight, P(0,0,side*6));
            Rock(rig, "Heavy split-leather boot", P(side*.63f,.18f,.24f), P(.83f,.37f,1.08f), hide, P(0,side*13,0));
            Rock(rig, "Steel toecap", P(side*.69f,.21f,.60f), P(.69f,.30f,.48f), iron, P(0,side*13,0));
            Rod(rig, "Chest harness", P(side*.65f,2.87f,.48f), P(-side*.40f,1.77f,.64f), .135f, hide);
        }

        var head = Node(rig, "Heavy brows, crooked jaw and tusks", P(.01f,3.16f,.22f));
        Oval(head, "Ogre cranium", P(0,.39f,-.03f), P(1.03f,1.05f,.89f), skin);
        Oval(head, "Broad chin", P(0,.08f,.29f), P(.92f,.57f,.73f), skinLight);
        Oval(head, "Broken nose", P(0,.37f,.47f), P(.32f,.29f,.27f), skinLight);
        Rock(head, "Mouth shadow", P(0,.10f,.636f), P(.55f,.10f,.10f), hide, P(0,0,-5));
        for (int side = -1; side <= 1; side += 2)
        {
            Oval(head, "Ragged ear", P(side*.55f,.33f,-.04f), P(.29f,.38f,.24f), skinLight, P(0,0,side*-23));
            Oval(head, "Deep eye socket", P(side*.25f,.45f,.364f), P(.29f,.21f,.17f), shale);
            Oval(head, "Ochre eye", P(side*.22f,.44f,.44f), P(.135f,.076f,.062f), amber);
            Rock(head, "Stern angular brow", P(side*.25f,.54f,.422f), P(.37f,.18f,.16f), skinLight, P(0,0,side*16));
            Oval(head, "Curved bone tusk", P(side*.31f,.14f,.59f), P(.13f,side == 1 ? .43f : .29f,.14f),
                linen, P(18,0,side*-17));
        }
        Oval(head, "Dark crown and topknot", P(0,.72f,-.24f), P(.90f,.48f,.69f), hide);
        Oval(head, "Tied topknot", P(.06f,.92f,-.37f), P(.34f,.44f,.33f), hide, P(-21,0,-10));
        Rock(head, "Topknot brass clasp", P(.06f,.84f,-.33f), P(.32f,.12f,.26f), brass);
        Rod(head, "Broken ear ring", P(-.57f,.14f,.03f), P(-.58f,.01f,.11f), .125f, brass);

        hammerArm = OgreArm(rig, 1);
        freeArm = OgreArm(rig, -1);
        var hammer = Node(hammerArm, "Roadwarden · bridge-breaker hammer", P(.03f,-1.38f,.29f), Quaternion.Euler(74,9,-6));
        Rod(hammer, "Ashwood hammer haft", P(0,-.66f,0), P(0,1.31f,0), .18f, bark);
        for (int i = 0; i < 7; i++)
            Rod(hammer, "Spiral grip binding", P(0,-.42f+i*.073f,0), P(.014f,-.38f+i*.073f,0), .225f, wrap);
        Rock(hammer, "Pitted iron hammer head", P(0,1.28f,0), P(.70f,.72f,1.20f), iron);
        Rock(hammer, "Worn striking face", P(0,1.28f,.64f), P(.66f,.68f,.27f), ironLight);
        Rock(hammer, "Opposing split counterweight", P(0,1.30f,-.55f), P(.54f,.54f,.35f), ironLight);
        Rock(hammer, "Hammer cheek strap", P(.356f,1.28f,0), P(.08f,.62f,.30f), brass);
        Rock(hammer, "Hammer cheek strap", P(-.356f,1.28f,0), P(.08f,.62f,.30f), brass);
        Rock(hammer, "Haft pommel", P(0,-.66f,0), P(.25f,.24f,.24f), ironLight);
    }

    private static Transform OgreArm(Transform rig, int side)
    {
        var shoulder = Node(rig, side > 0 ? "Hammer shoulder" : "Free shoulder", P(side*.97f,2.82f,.02f));
        Oval(shoulder, "Shoulder muscle", P(side*.13f,-.26f,0), P(.91f,1.02f,.82f), skin);
        Rock(shoulder, "Asymmetric steel shoulder plates", P(side*.16f,.03f,-.04f),
            P(side > 0 ? 1.02f : .80f,.48f,.90f), side > 0 ? iron : hide, P(4,side*17,side*-19));
        Rock(shoulder, "Scored pauldron trim", P(side*.33f,.12f,.14f), P(.68f,.27f,.69f), side > 0 ? ironLight : wrap,
            P(4,side*17,side*-19));
        Oval(shoulder, "Heavy forearm", P(side*.22f,-.87f,.14f), P(.64f,.94f,.67f), skinLight, P(-19,0,side*5));
        Oval(shoulder, "Leather forearm wrap", P(side*.22f,-1.01f,.21f), P(.67f,.50f,.69f), hide, P(-19,0,side*5));
        Rock(shoulder, "Forearm brass guard", P(side*.34f,-.97f,.49f), P(.43f,.48f,.15f), brass, P(-15,0,side*5));
        Oval(shoulder, "Ogre fist", P(side*.20f,-1.36f,.32f), P(.55f,.51f,.58f), skin);
        for (int finger = 0; finger < 3; finger++)
            Oval(shoulder, "Finger knuckle", P(side*(.04f+finger*.14f),-1.41f,.54f), P(.16f,.22f,.21f), skinLight);
        return shoulder;
    }
}


public static partial class OgreRoadwardenAuthoring
{
    private static Mesh MeshAsset(string name)
    {
        string path = Folder + "/" + name + ".asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh != null) return mesh;
        mesh = new Mesh { name = name };
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    private static void LayRoad(Transform root, Vector3[] route, bool restored)
    {
        const float Step = 1.12f;
        float halfWidth = restored ? 2.32f : 1.88f;
        var samples = new System.Collections.Generic.List<Vector3> { route[0] };
        for (int index = 1; index < route.Length; index++)
        {
            var previous = route[index-1];
            int segments = Mathf.CeilToInt(Vector3.Distance(previous, route[index]) / Step);
            for (int i = 1; i <= segments; i++) samples.Add(Vector3.Lerp(previous, route[index], (float)i/segments));
        }

        var random = new System.Random(restored ? 391 : 825);
        var points = new System.Collections.Generic.List<Vector3>();
        var triangles = new System.Collections.Generic.List<int>();
        var setts = new System.Collections.Generic.List<CombineInstance>();
        const int Columns = 5;
        for (int i = 0; i < samples.Count; i++)
        {
            var center = samples[i];
            var ahead = samples[Mathf.Min(i+1,samples.Count-1)] - samples[Mathf.Max(i-1,0)];
            var right = Vector3.Cross(Vector3.up, ahead.normalized);
            float width = halfWidth + .14f * Mathf.Sin(i * 1.41f);
            for (int j = 0; j < Columns; j++)
            {
                var point = center + right * (Mathf.Lerp(-width, width, j / (Columns-1f)));
                point = Grounded(point + Vector3.up * .037f);
                points.Add(point);
                if (i == 0 || j == 0) continue;
                int n = i * Columns + j;
                triangles.AddRange(new[] { n-Columns-1, n-1, n, n-Columns-1, n, n-Columns });
            }

            for (int j = -2; j <= 2; j++)
            {
                if (!restored && random.NextDouble() > (j == -2 || j == 2 ? .64 : .12)) continue;
                var point = Grounded(center + right * (j * width * .42f + (float)random.NextDouble() * .09f)
                    + ahead.normalized * (j % 2 != 0 ? .28f : -.13f) + Vector3.up * (restored ? .069f : .079f));
                setts.Add(new CombineInstance
                {
                    mesh = bevel,
                    transform = Matrix4x4.TRS(point, Quaternion.LookRotation(ahead) * Quaternion.Euler(0,random.Next(-9,10),0),
                        P(restored ? .71f + random.Next(0,17)*.01f : .37f + random.Next(0,33)*.01f,
                            restored ? .145f : .15f, restored ? .72f + random.Next(0,19)*.01f : .29f + random.Next(0,28)*.01f))
                });
            }
        }

        string phase = restored ? "repaired" : "approach";
        var mesh = MeshAsset("Roadwarden " + phase + " graded roadbed");
        mesh.Clear();
        mesh.SetVertices(points);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        EditorUtility.SetDirty(mesh);
        var roadbed = DrawMesh(root, "Compacted roadbed", mesh, earth);
        if (restored)
        {
            var dirtRoad = AssetDatabase.LoadAssetAtPath<TravelSurfaceDefinition>("Assets/_DiceFree/Settings/Travel/Cornberg dirt road.asset");
            var bounds = mesh.bounds;
            bounds.Expand(new Vector3(.01f, 2f, .01f));
            roadbed.gameObject.AddComponent<TravelSurface>().Configure("surface-area.trimery.reopened-highway",
                dirtRoad, roadbed, bounds);
        }
        var stoneMesh = MeshAsset("Roadwarden " + phase + " dressed setts");
        stoneMesh.Clear();
        stoneMesh.indexFormat = IndexFormat.UInt32;
        stoneMesh.CombineMeshes(setts.ToArray());
        EditorUtility.SetDirty(stoneMesh);
        DrawMesh(root, restored ? "Staggered rows of new limestone setts" : "Remnants of the old causeway",
            stoneMesh, restored ? pavers : stone);
    }

    private static MeshFilter DrawMesh(Transform parent, string name, Mesh mesh, Material mat)
    {
        var obj = Node(parent, name);
        var filter = obj.gameObject.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        obj.gameObject.AddComponent<MeshRenderer>().sharedMaterial = mat;
        return filter;
    }

    private static void BuildBanks(Transform root)
    {
        Vector3[] west = { P(194.2f,0,193.7f), P(198.3f,0,193.8f) };
        Vector3[] east =
        {
            P(208.7f,0,193.6f), P(213.9f,0,192.8f), P(219.3f,0,192.4f),
            P(224.4f,0,193.4f), P(228.7f,0,196.4f), P(233.4f,0,198.6f),
            P(238.2f,0,200.2f), P(243.1f,0,202.9f), P(248.1f,0,205.1f), P(251.3f,0,207.7f),
        };
        var random = new System.Random(4391);
        void Bank(Vector3[] line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                var anchor = Node(root, "Pine, shale and retaining stones", Grounded(line[i]));
                Vector3 direction = line[Mathf.Min(line.Length-1,i+1)] - line[Mathf.Max(0,i-1)];
                anchor.localRotation = Quaternion.LookRotation(Vector3.Cross(direction.normalized, Vector3.up));
                var solid = anchor.gameObject.AddComponent<BoxCollider>();
                anchor.gameObject.layer = 9;
                solid.center = P(0,.66f,0);
                solid.size = P(i == line.Length-1 ? 5.0f : 6.05f,2.3f,2.05f);
                for (int j = -1; j <= 1; j++)
                {
                    Rock(anchor, "Layered granite outcrop", P(j*1.72f,.55f + random.Next(0,36)*.01f,0),
                        P(2.43f,1.84f + random.Next(0,88)*.01f,2.57f), j == 0 ? stone : shale, P(j*12,random.Next(-22,23),j*13));
                    Rock(anchor, "Weathered moss seam", P(j*1.61f,1.23f,.53f),
                        P(1.70f,.32f,.97f), moss, P(-11,random.Next(-20,21),j*9));
                    Rock(anchor, "Loose scree", P(j*1.47f,.12f,-1.46f),
                        P(.40f + random.Next(0,45)*.01f,.41f,.60f), stone, P(0,random.Next(360),0));
                }

                if (i % 3 != 1) Pine(anchor, P(.37f,.31f,.42f), 4.3f + random.Next(0,170)*.01f);
            }
        }
        Bank(west);
        Bank(east);
    }

    private static void Pine(Transform root, Vector3 position, float height)
    {
        Rod(root, "Pine trunk", position, position + Vector3.up * (height*.87f), height*.075f, bark);
        for (int i = 0; i < 3; i++)
        {
            float size = height * (.54f - .11f * i);
            Rock(root, "Staggered evergreen boughs", position + Vector3.up * (height*(.52f+.16f*i)),
                P(size,size*1.05f,size), i == 1 ? pine : pineShade, P(10,i*27,-6));
        }
        Rock(root, "Pine crown", position + Vector3.up * (height*.95f), P(height*.19f,height*.37f,height*.19f), pine, P(7,42,0));
    }

    private static void BuildJam(Transform root)
    {
        root.gameObject.layer = 9;
        var collider = root.gameObject.AddComponent<BoxCollider>();
        collider.center = P(0,.75f,0);
        collider.size = P(8.1f,2.1f,2.9f);
        var obstacle = root.gameObject.AddComponent<NavMeshObstacle>();
        obstacle.center = collider.center;
        obstacle.size = collider.size;
        obstacle.carving = true;
        obstacle.carveOnlyStationary = false;
        for (int i = 0; i < 6; i++)
        {
            var point = P(-3.57f+i*1.39f,.42f + (i%3)*.18f,(i%2==0 ? -.39f : .57f));
            Rock(root, "Uprooted paving slab", point, P(1.92f,1.28f,2.23f), i%2 == 0 ? stone : shale, P(16-i*5,17*i,9+i*9));
            Rock(root, "Spilled roadside rubble", point + P(.23f,-.25f,1.13f), P(.73f,.52f,.91f), stone, P(i*11,i*21,5));
        }
        Rod(root, "Felled pine laid across the highway", P(-4.12f,1.06f,-.75f), P(4.14f,1.53f,.42f), .61f, bark);
        Rod(root, "Exposed timber endgrain", P(-4.17f,1.057f,-.76f), P(-4.10f,1.062f,-.75f), .55f, wrap);
        Rod(root, "Splintered branch", P(-2.5f,1.22f,-.51f), P(-2.91f,2.21f,-1.73f), .23f, bark);
        Rod(root, "Splintered branch", P(1.91f,1.39f,.11f), P(1.21f,2.24f,1.48f), .22f, bark);
        Rod(root, "Snapped tie beam", P(-3.23f,.44f,1.14f), P(.89f,.67f,1.56f), .29f, wrap);
    }

    private static void AddWaymarker(Transform root, string name, Vector3 point, float yaw, bool restored)
    {
        var node = Node(root, name, Grounded(point), Quaternion.Euler(0,yaw,0));
        Rock(node, "Granite milestone footing", P(0,.27f,0), P(.90f,.59f,.76f), stone);
        Rock(node, "Old trail post", P(0,1.13f,0), P(.23f,2.2f,.24f), bark);
        Rock(node, "Guild waypost header", P(0,1.93f,0), P(1.49f,.39f,.23f), restored ? guild : hide, P(0,0,-5));
        Rock(node, "Waypost brass shield", P(-.47f,1.96f,-.132f), P(.26f,.25f,.07f), brass);
        var label = Node(node, "Trimery road lettering", P(.07f,1.93f,-.132f), Quaternion.Euler(0,0,-5)).gameObject.AddComponent<TextMesh>();
        label.text = restored ? "ROAD OPEN" : "TRIMERY";
        label.color = new Color(.94f,.86f,.68f);
        label.anchor = TextAnchor.MiddleCenter;
        label.fontSize = 42;
        label.characterSize = .034f;
        label.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
    }

    private static void AddLamp(Transform root, Vector3 point, float yaw)
    {
        var lamp = Node(root, "Guild lantern on a new oak post", Grounded(point), Quaternion.Euler(0,yaw,0));
        Rock(lamp, "Square limestone footing", P(0,.24f,0), P(.94f,.55f,.85f), pavers);
        Rock(lamp, "Oiled timber post", P(0,1.49f,0), P(.25f,2.76f,.25f), bark);
        Rod(lamp, "Forge hook", P(0,2.58f,0), P(.55f,2.65f,0), .09f, iron);
        Rod(lamp, "Suspension", P(.55f,2.65f,0), P(.55f,2.39f,0), .06f, iron);
        Rock(lamp, "Amber lantern", P(.55f,2.15f,0), P(.38f,.53f,.39f), amber);
        Rock(lamp, "Lantern peaked roof", P(.55f,2.46f,0), P(.54f,.24f,.52f), iron);
        Rock(lamp, "Lantern iron base", P(.55f,1.91f,0), P(.46f,.16f,.44f), iron);
    }

    private static void BuildCamp(Transform root)
    {
        var camp = Node(root, "Roadwarden's broken wagon camp", Grounded(P(195.9f,0,203.8f)), Quaternion.Euler(0,33,0));
        Rock(camp, "Wagon bed", P(0,.43f,0), P(2.49f,.29f,2.16f), bark, P(5,2,-12));
        for (int i = -1; i <= 1; i += 2)
        {
            Rod(camp, "Splintered wheel axle", P(i*.53f,.38f,-1.1f), P(i*.67f,.19f,-2.2f), .17f, bark);
            Rock(camp, "Torn wagon side", P(i*.91f,.67f,.11f), P(.20f,.63f,1.94f), hide, P(i*18,0,i*28));
            Rock(camp, "Wagon box iron corner", P(i*.98f,.61f,.79f), P(.26f,.39f,.19f), iron);
        }
        Wheel(camp, P(-1.12f,.59f,.08f), .62f);
        Rock(camp, "Split grain chest", P(.97f,.47f,1.8f), P(1.03f,.73f,.82f), bark, P(0,23,15));
        Rock(camp, "Grain chest iron band", P(.94f,.68f,1.83f), P(.20f,.24f,.88f), iron, P(0,23,15));
        Rock(camp, "Sealed parchment weight", P(.28f,.67f,.47f), P(.54f,.07f,.32f), linen, P(1,17,-12));
        Rock(camp, "Wax seal without an insignia", P(.31f,.718f,.42f), P(.11f,.025f,.12f), burgundy);
        var fire = Node(root, "Low campfire and abandoned supplies", Grounded(P(198.2f,0,207.8f)));
        for (int i = 0; i < 7; i++)
        {
            float a = i * Mathf.PI * 2 / 7;
            Rock(fire, "Stone hearth", P(Mathf.Cos(a)*.57f,.14f,Mathf.Sin(a)*.57f), P(.44f,.32f,.36f), stone, P(5,i*37,0));
        }
        Rod(fire, "Blackened kindling", P(-.37f,.11f,-.29f), P(.37f,.11f,.21f), .16f, hide);
        Rod(fire, "Embers", P(-.29f,.17f,.21f), P(.29f,.17f,-.21f), .088f, amber);
    }

    private static void Wheel(Transform root, Vector3 center, float radius)
    {
        for (int i = 0; i < 8; i++)
        {
            float a = i * Mathf.PI / 4;
            float b = (i+1) * Mathf.PI / 4;
            Vector3 edge = center + P(0,Mathf.Cos(a)*radius,Mathf.Sin(a)*radius);
            Rod(root, "Iron-rimmed wheel", edge, center + P(0,Mathf.Cos(b)*radius,Mathf.Sin(b)*radius), .099f, iron);
            Rod(root, "Oak wheel spoke", center, edge, .062f, wrap);
        }
        Rod(root, "Axle cap", center-P(.17f,0,0), center+P(.17f,0,0), .20f, iron);
    }

    private static void BuildCrew(Transform root)
    {
        var foreman = Node(root, "Guild foreman with leather gloves", P(-.39f,0,.17f), Quaternion.Euler(0,-25,0));
        Oval(foreman, "Indigo work coat", P(0,1.09f,0), P(.65f,.83f,.43f), guild);
        Rock(foreman, "Leather work apron", P(0,.96f,.225f), P(.50f,.68f,.15f), hide);
        Oval(foreman, "Foreman's face", P(0,1.61f,.03f), P(.30f,.40f,.32f), linen);
        Oval(foreman, "Leather brimmed hat", P(0,1.77f,.0f), P(.53f,.10f,.50f), hide);
        Oval(foreman, "Hat crown", P(0,1.83f,0), P(.34f,.23f,.34f), hide);
        for (int side = -1; side <= 1; side += 2)
        {
            Rod(foreman, "Linen sleeve", P(side*.29f,1.33f,0), P(side*.36f,.88f,.16f), .22f, linen);
            Oval(foreman, "Leather glove", P(side*.36f,.86f,.19f), P(.19f,.23f,.19f), hide);
            Rod(foreman, "Work trousers", P(side*.16f,.84f,0), P(side*.19f,.17f,0), .25f, shale);
            Rock(foreman, "Work boot", P(side*.19f,.10f,.065f), P(.28f,.22f,.37f), hide);
        }
        var cart = Node(root, "Guild winch cart", P(.90f,0,-.50f), Quaternion.Euler(0,26,0));
        Rock(cart, "Winch cart bed", P(0,.48f,0), P(1.14f,.20f,1.11f), bark);
        Wheel(cart, P(-.57f,.37f,0), .36f);
        Wheel(cart, P(.57f,.37f,0), .36f);
        Rod(cart, "Winch axle", P(-.42f,.85f,0), P(.42f,.85f,0), .17f, iron);
        Rod(cart, "Coiled hemp rope", P(-.23f,.85f,0), P(.24f,.85f,0), .43f, wrap);
        Rod(cart, "Winch handle", P(.45f,.84f,0), P(.45f,1.11f,.27f), .08f, iron);
        Rock(cart, "Spare road setts", P(0,.66f,-.36f), P(.59f,.23f,.42f), pavers, P(0,-8,0));
    }
}
