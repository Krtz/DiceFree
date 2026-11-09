using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Items;
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

public static class DefenderCompleteAuthoring
{
    private const string Data = "Assets/_DiceFree/Settings/Defender/";
    private const string Art = "Assets/_DiceFree/Art/Characters/TrimeryDefender/";
    private const string Motion = "Assets/_DiceFree/Resources/PlayerMotion/TierTwo/Defender.controller";
    private const string Catalog = "Assets/_DiceFree/Settings/Progression/Class catalog.asset";
    private const string Development = "Assets/_DiceFree/Settings/Development/Priest/Development Priest catalog.asset";
    private const string Menu = "Assets/_DiceFree/Resources/MenuPresentationCatalog.asset";
    private const string Scene = "Assets/_DiceFree/Scenes/DefenderGate.unity";
    private const string Model = Art + "TrimeryDefender.fbx";
    private const string Blockout = "Assets/_DiceFree/Materials/Blockout/";
    private const string WorldArt = "Assets/_DiceFree/Art/World/CornbergBatch/";
    private const string ClassId = DefenderSkillProgression.DefenderClassId;
    private const string Sword = "Trimery Watch Sword";
    private const string Shield = "Trimery Watch Shield";
    private static readonly Vector3 Yard = new(2760, 0, 2630);

    public static void Batch()
    {
        var controller = PriestCompleteAuthoring.PrepareArt(Model, Art, Motion, Art + "TrimeryDefenderImport.json");
        var defender = Load<ActorDefinition>(Data + "Defender class.asset");
        var trial = AuthorQuest();
        var items = new[] { Gear(false), Gear(true) };
        var classes = Load<ClassCatalog>(Catalog);
        classes.Configure(classes.Classes.Where(value => value.stableId != ClassId).Append(defender).ToArray());
        EditorUtility.SetDirty(classes);
        AddMenu(defender, controller, items);
        AssetDatabase.SaveAssets();
        BuildGate();
        AssetDatabase.SaveAssets();

        foreach (string sceneName in new[] { "Cornberg", "DevelopmentRoom" })
        {
            bool world = sceneName == "Cornberg";
            var scene = EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/" + sceneName + ".unity");
            var advancement = Object.FindObjectsByType<AdvancementController>(FindObjectsInactive.Include).Single();
            var player = advancement.GetComponent<CombatActor>();
            classes = Load<ClassCatalog>(world ? Catalog : Development);
            var edge = Load<AdvancementDefinition>(Data + "Trimery Gate advancement.asset");
            advancement.Configure(classes, advancement.Definitions.Where(value => value != edge).Append(edge).ToArray());
            player.GetComponent<ManifestationPersistence>().ConfigureClassCatalog(classes);
            var resources = player.GetComponent<ActorResourceController>();
            var resolve = Load<ClassResourceProfile>(Data + "Defender Resolve.asset");
            resources.Configure(resources.Profiles.Where(value => value != resolve).Append(resolve).ToArray());
            var caster = player.GetComponent<DefenderSkillCaster>() ?? player.gameObject.AddComponent<DefenderSkillCaster>();
            caster.Configure(Load<AttackDefinition>(Data + "Defender Shield Bash.asset"), Load<AttackDefinition>(Data + "Defender Shield Sweep.asset"));
            var provider = player.GetComponent<DefenderAbilityProvider>() ?? player.gameObject.AddComponent<DefenderAbilityProvider>();
            provider.Configure(Enumerable.Range(0, 6).Select(i => Load<Texture2D>(Data + DefenderSkillProgression.Name((DefenderSkill)i) + " icon.asset")).ToArray());
            controller = Load<UnityEditor.Animations.AnimatorController>(Motion);
            items = new[] { Load<ItemDefinition>(Data + Sword + ".asset"), Load<ItemDefinition>(Data + Shield + ".asset") };
            AddInventory(player.GetComponent<CarriedInventory>(), items);
            AddVisual(player, controller);

            if (world) AddMentor(player, Load<QuestDefinition>(Data + "Hold the North Gate.asset"), edge, controller, items);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save " + sceneName);
        }

        EditorBuildSettings.scenes = EditorBuildSettings.scenes.Where(s => s.path != Scene)
            .Append(new EditorBuildSettingsScene(Scene, true)).ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("DICEFREE_DEFENDER_COMPLETE_AUTHOR_OK");
    }

    private static QuestDefinition AuthorQuest()
    {
        var trial = Asset<QuestDefinition>("Hold the North Gate");
        trial.stableId = DefenderMentor.QuestId;
        trial.title = "Hold the North Gate";
        trial.requiredClassId = DefenderMentor.ParentId;
        trial.requiredLevel = 30;
        trial.offer = "Briarback raiders have been following the timber crews home. Two workers are still in the north gate yard. Take your post with the watch and keep them alive until the last raiders fall.";
        trial.activeDialogue = "I can send you to the gate yard when you're ready. Watch both lanes, draw attackers away from the workers, and keep at least one guard on their feet.";
        trial.locationHint = "Take your post with Veteran Mara by Trimery's north gate.";
        trial.turnInHint = "Report to Veteran Mara at Trimery's north gate.";
        trial.stages = new[] { new QuestObjective { kind = ObjectiveKind.Escort, contentId = DefenderGateSite.TrialId,
            objectiveId = "hold-the-yard", count = 1, instruction = "Hold three waves and protect both workers and a guard" } };
        EditorUtility.SetDirty(trial);
        return trial;
    }

    private static void BuildGate()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("Trimery north gate yard").transform;
        root.position = Yard;
        var tuning = Load<ActorDefinition>(Data + "Defender class.asset").tuning;
        var site = root.gameObject.AddComponent<DefenderGateSite>();
        site.playerSpawn = Yard + new Vector3(0, .03f, -1);
        site.mapBounds = new Rect(Yard.x - 29, Yard.z - 26, 58, 56);

        var meadow = Load<Material>(Blockout + "Meadow.mat");
        var road = Load<Material>("Assets/_DiceFree/Settings/WolfSurvival/Trampled wolf trails.mat");
        var stone = Load<Material>(Blockout + "Fieldstone.mat");
        var timber = Load<Material>(Blockout + "Timber.mat");
        var roof = Load<Material>(Blockout + "Weathered roof.mat");
        var brass = Load<Material>(Data + "Gate brass.mat");
        var blue = Load<Material>(Data + "Trimery blue oak.mat");
        var foliage = Load<Material>(Blockout + "Sunlit foliage.mat");
        var shade = Load<Material>(Blockout + "Forest.mat");

        Piece(root, "Grass yard", new Vector3(0,-.28f,2), new Vector3(60,.56f,60), meadow, 8)
            .GetComponent<Renderer>().enabled = false;
        Surface(root, "Textured stockade grass", new Vector3(0,.006f,2), new Vector2(94,96), 5,
            GroundMaterial("North gate turf", meadow, "Assets/_DiceFree/Settings/WolfSurvival/Forest duff.png", new Color(.63f,.72f,.49f)));
        Surface(root, "Worn north road", new Vector3(0,.020f,4), new Vector2(5.7f,70), 4,
            GroundMaterial("North gate road", road, null, new Color(.71f,.65f,.49f)));
        for (int i=0; i<30; i++)
        {
            float z = -16 + i * 1.48f;
            for (int sign=-1; sign<=1; sign+=2)
                Piece(root, "Road edge setts", new Vector3(sign*(2.87f+.10f*Mathf.Sin(i*1.9f)),.044f,z),
                    new Vector3(.32f,.11f,.66f), stone).localRotation = Quaternion.Euler(0,i*11%17-8,0);
        }
        foreach (float sign in new[] { -1f, 1f })
        {
            Piece(root, "Stone gate shoulder", new Vector3(sign*13.3f,1.45f,-12), new Vector3(20,2.9f,1.1f), stone, 9);
            Piece(root, "Cap beam", new Vector3(sign*13.3f,2.94f,-12), new Vector3(20,.26f,1.3f), timber);
            for (int i=0;i<12;i++)
            {
                float x=sign*(4.6f+i*1.59f);
                Piece(root,"Stockade facing",new Vector3(x,1.30f,-11.40f),new Vector3(.18f,2.5f,.24f),timber);
                if(i%2==0)
                    Piece(root,"Dressed masonry foot",new Vector3(x,.27f,-11.32f),new Vector3(1.56f,.48f,.37f),stone);
            }
            Piece(root, "Watch tower masonry", new Vector3(sign*4.55f,2.15f,-12), new Vector3(2.45f,4.3f,2.65f), stone, 9);
            Piece(root, "Watch tower timber ring", new Vector3(sign*4.55f,4.26f,-12), new Vector3(2.65f,.26f,2.85f), timber);
            Piece(root, "Watch tower west roof", new Vector3(sign*4.55f-.68f,4.76f,-12), new Vector3(1.76f,.19f,3.13f), roof)
                .localRotation = Quaternion.Euler(0,0,29);
            Piece(root, "Watch tower east roof", new Vector3(sign*4.55f+.68f,4.76f,-12), new Vector3(1.76f,.19f,3.13f), roof)
                .localRotation = Quaternion.Euler(0,0,-29);
            Piece(root, "Watch pennant", new Vector3(sign*4.55f,2.95f,-10.60f), new Vector3(.79f,1.58f,.044f), blue);
            Piece(root, "Gate lamp bracket", new Vector3(sign*3.25f,2.73f,-10.92f), new Vector3(.21f,.21f,.59f), timber);
            Piece(root, "Amber watch lantern", new Vector3(sign*3.25f,2.51f,-10.66f), new Vector3(.21f,.31f,.21f),
                Load<Material>(Blockout + "Well light.mat"));
            Piece(root, "Pennant lintel", new Vector3(sign*4.55f,3.30f,-10.56f), new Vector3(.56f,.09f,.043f), brass);
            for (int i=0; i<3; i++)
                Piece(root, "Pennant post", new Vector3(sign*4.55f + (i-1)*.16f,3.02f,-10.56f), new Vector3(.07f,.47f,.043f), brass);
            for (int i=0; i<15; i++)
            {
                float z = -10 + i * 2.1f;
                Piece(root, "Stockade post", new Vector3(sign*22.3f,1.25f,z), new Vector3(.19f,1.25f,.19f), timber,
                    9, PrimitiveType.Cylinder);
                if (i % 2 == 0)
                    Piece(root, "Stockade rail", new Vector3(sign*22.3f,1.14f,z+1.05f), new Vector3(.23f,.24f,4.2f), timber,9);
            }
        }
        Piece(root, "Gate crossbeam", new Vector3(0,3.6f,-12), new Vector3(7.6f,.40f,.66f), timber);
        Piece(root, "Gate cap", new Vector3(0,3.88f,-12), new Vector3(7.9f,.17f,.97f), roof);
        var random = new System.Random(119);
        for (int i = 0; i < 52; i++)
        {
            float x = (i % 2 == 0 ? -1 : 1) * (15 + (float)random.NextDouble()*17);
            float z = -24 + (float)random.NextDouble()*58;
            if (Mathf.Abs(z + 12) < 3) continue;
            string pine = i % 3 == 0 ? "Tree_Spruce.fbx" : "Tree_Pine.fbx";
            var tree = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(WorldArt+"Trees/"+pine),root);
            tree.name = "Trimery tree line";
            tree.transform.localPosition = new Vector3(x,0,z);
            tree.transform.localRotation = Quaternion.Euler(0,(float)random.NextDouble()*360,0);
            tree.transform.localScale = Vector3.one * (.87f + (float)random.NextDouble()*.47f);
            foreach (var collider in tree.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            if (Mathf.Abs(x) < 26)
                Piece(root,"Tree footing",new Vector3(x,.9f,z),new Vector3(.31f,.9f,.31f),timber,9,PrimitiveType.Cylinder)
                    .GetComponent<Renderer>().enabled=false;
        }
        for (int i = 0; i < 72; i++)
        {
            float x = (i % 2 == 0 ? -1 : 1) * (4.1f + (float)random.NextDouble()*24);
            float z = -20 + (float)random.NextDouble()*52;
            if (Mathf.Abs(z + 12) < 1.7f) continue;
            float size = .18f + (float)random.NextDouble()*.29f;
            var tint=i%4==0?shade:foliage;
            for(int j=0;j<3;j++)
                Piece(root,"Grass blades",new Vector3(x+(j-1)*.15f,size*.36f,z+(j%2)*.12f),
                    new Vector3(.06f,size,.09f),tint).localRotation=Quaternion.Euler(0,i*31%360,(j-1)*27);
        }
        for (int i=0;i<8;i++)
        {
            float sign = i % 2 == 0 ? -1 : 1;
            float z = -6.3f + i / 2 * .57f;
            Piece(root, "Timber load", new Vector3(sign*7.05f,.39f,z), new Vector3(1.1f,.78f,.49f), timber, 9);
            Piece(root, "Stacked board ends", new Vector3(sign*7.05f,.79f,z), new Vector3(1.19f,.08f,.53f), brass);
        }

        var worker = Stats("Gate yard worker", "trimery.watch.worker", tuning, null, 465, 3.8f);
        var guard = Stats("Trimery watch guard", "trimery.watch.guard", tuning,
            Strike("Watch sword", "attack.trimery.watch-guard", 25, 1.17f), 615, 4.2f);
        var raider = Stats("Briarback gate raider", "trimery.watch.raider", tuning,
            Strike("Raider cleaver", "attack.trimery.watch-raider", 27, 1.36f), 155, 4.3f);
        var scout = Stats("Briarback outrunner", "trimery.watch.outrunner", tuning,
            Strike("Outrunner axe", "attack.trimery.watch-outrunner", 19, 1.12f), 107, 4.9f);
        var breaker = Stats("Briarback gatebreaker", "trimery.watch.gatebreaker", tuning,
            Strike("Gatebreaker maul", "attack.trimery.watch-gatebreaker", 37, 1.62f), 236, 3.8f);

        site.workers = new[] {
            Actor(root, worker, new Vector3(-4,0,-7.4f), 0, WorldArt+"NPCs/NPC_Herbalist.fbx", 1.12f),
            Actor(root, worker, new Vector3(4,0,-7.4f), 0, WorldArt+"NPCs/NPC_Herbalist.fbx", 1.15f)
        };
        site.workers[0].name = "Tamsin - timber worker";
        site.workers[1].name = "Edric - timber worker";
        site.guards = new[] {
            Actor(root, guard, new Vector3(-2.5f,0,-4.6f), 0, WorldArt+"NPCs/NPC_Guard.fbx", 1.17f),
            Actor(root, guard, new Vector3(2.5f,0,-4.6f), 0, WorldArt+"NPCs/NPC_Guard.fbx", 1.17f)
        };
        site.waves = new[] {
            Wave(root, "Scouts on the north road", new[] { scout, raider, scout }, new[] { -3.5f, .2f, 4f }, 15),
            Wave(root, "Raiders split across both lanes", new[] { raider, scout, raider, scout }, new[] { -9f, -4f, 4f, 9f }, 16),
            Wave(root, "Gatebreaker leads the last assault", new[] { scout, raider, breaker, raider, scout },
                new[] { -11f, -5.7f, 0, 5.7f, 11f }, 17)
        };

        Physics.SyncTransforms();
        var sources = new List<NavMeshBuildSource>();
        var bounds = new Bounds(Yard + new Vector3(0,1,2),new Vector3(59,10,59));
        NavMeshBuilder.CollectSources(bounds, (1<<8)|(1<<9), NavMeshCollectGeometry.PhysicsColliders, 0,
            new List<NavMeshBuildMarkup>(), sources);
        var data = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(0),sources,bounds,Vector3.zero,Quaternion.identity);
        if (data == null) throw new InvalidOperationException("The gate yard navigation did not bake.");
        const string navPath = Data + "Trimery gate navigation.asset";
        var saved = AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
        if (saved == null) AssetDatabase.CreateAsset(data, navPath);
        else { EditorUtility.CopySerialized(data, saved); Object.DestroyImmediate(data); data = saved; }
        root.gameObject.AddComponent<WorldNavigation>().Configure(data);
        var nav = NavMesh.AddNavMeshData(data);
        try
        {
            foreach (var actor in site.workers.Concat(site.guards).Concat(site.waves.SelectMany(wave => wave.raiders)))
            {
                var path = new NavMeshPath();
                if (!NavMesh.SamplePosition(actor.transform.position, out var ground, .75f, 1) ||
                    !NavMesh.CalculatePath(site.playerSpawn, ground.position, 1, path) || path.status != NavMeshPathStatus.PathComplete)
                    throw new InvalidOperationException("Unreachable gate placement " + actor.transform.position);
                actor.transform.position = ground.position;
            }
        }
        finally { nav.Remove(); }
        if (!EditorSceneManager.SaveScene(scene, Scene)) throw new InvalidOperationException("The Defender instance could not save.");
    }

    private static DefenderGateSite.Wave Wave(Transform root, string warning, ActorDefinition[] definitions, float[] x, float z)
    {
        const string boar = "Assets/_DiceFree/Art/Creatures/Briarback/Models/Quilboar.fbx";
        var wave = new DefenderGateSite.Wave { warning = warning, raiders = new CombatActor[definitions.Length] };
        for (int i = 0; i < definitions.Length; i++)
        {
            var stats = definitions[i];
            float scale = stats.stableId.EndsWith("gatebreaker", StringComparison.Ordinal) ? 1.34f : 1.04f;
            var actor = Actor(root, stats, new Vector3(x[i],0,z+i%2*2), 1, boar, scale);
            actor.transform.localRotation = Quaternion.Euler(0,180,0);
            actor.gameObject.SetActive(false);
            wave.raiders[i] = actor;
        }
        return wave;
    }

    private static CombatActor Actor(Transform parent, ActorDefinition stats, Vector3 local, int faction, string model, float scale)
    {
        var root = new GameObject(stats.displayName);
        root.layer = 10;
        root.transform.SetParent(parent, false);
        root.transform.localPosition = local;
        var capsule = root.AddComponent<CapsuleCollider>();
        capsule.height = 1.75f * scale;
        capsule.center = Vector3.up * capsule.height * .5f;
        capsule.radius = .37f;
        var agent = root.AddComponent<NavMeshAgent>();
        agent.enabled = false;
        agent.areaMask = 1;
        agent.height = capsule.height;
        agent.radius = capsule.radius;
        root.AddComponent<TraversalMotor>();
        root.AddComponent<ActorStats>().Configure(stats, faction == 0 ? 30 : 24);
        root.AddComponent<Health>();
        var actor = root.AddComponent<CombatActor>();
        actor.Configure(faction, capsule.radius);
        root.AddComponent<CombatStatusController>();
        if (stats.basicAttack != null) root.AddComponent<BasicAttack>();
        var visual = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(model), root.transform);
        visual.transform.localScale = Vector3.one * scale;
        root.AddComponent<ActorFeedback>().Configure(visual.transform);
        if (faction == 1)
        {
            var animator = visual.GetComponentInChildren<Animator>();
            animator.runtimeAnimatorController = Load<RuntimeAnimatorController>(
                "Assets/_DiceFree/Art/Creatures/Briarback/RegionalMotion/Quilboar regional.controller");
            animator.applyRootMotion = false;
            root.AddComponent<QuilboarPresentation>().model = animator;
        }
        return actor;
    }

    private static Material GroundMaterial(string name, Material source, string texture, Color tint)
    {
        string path = Data + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(source) { name = name };
            AssetDatabase.CreateAsset(material,path);
        }
        if (texture != null) material.SetTexture("_BaseMap",Load<Texture2D>(texture));
        material.SetColor("_BaseColor",tint);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void Surface(Transform root,string name,Vector3 center,Vector2 size,float tile,Material material)
    {
        var gameObject=new GameObject(name);
        gameObject.transform.SetParent(root,false);
        gameObject.transform.localPosition=center;
        var vertices=new[]{new Vector3(-size.x/2,0,-size.y/2),new Vector3(-size.x/2,0,size.y/2),
            new Vector3(size.x/2,0,size.y/2),new Vector3(size.x/2,0,-size.y/2)};
        var source=new Mesh { name=name, vertices=vertices, triangles=new[]{0,1,2,0,2,3},
            uv=vertices.Select(point=>new Vector2(point.x,point.z)/tile).ToArray() };
        source.RecalculateNormals();
        source.RecalculateBounds();
        string path=Data+name+".asset";
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh == null){AssetDatabase.CreateAsset(source,path);mesh=source;}
        else {EditorUtility.CopySerialized(source,mesh);Object.DestroyImmediate(source);}
        gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
        gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
    }

    private static Transform Piece(Transform root, string name, Vector3 point, Vector3 size, Material material,
        int layer = 2, PrimitiveType type = PrimitiveType.Cube)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.layer = layer;
        if (layer != 8 && layer != 9) Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(root, false);
        go.transform.localPosition = point;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go.transform;
    }

    private static ActorDefinition Stats(string name, string id, CombatTuning tuning, AttackDefinition attack, float hp, float speed)
    {
        var actor = Asset<ActorDefinition>(name);
        actor.displayName = name;
        actor.stableId = id;
        actor.tuning = tuning;
        actor.basicAttack = attack;
        actor.baseHp = hp;
        actor.moveSpeed = speed;
        actor.baseAttributes = new AttributeValues(0);
        actor.growth = new AttributeValues(0);
        actor.tags = new[] { "no-loot" };
        EditorUtility.SetDirty(actor);
        return actor;
    }

    private static AttackDefinition Strike(string name, string id, float damage, float interval)
    {
        var attack = Asset<AttackDefinition>(name);
        attack.stableId = id;
        attack.displayName = name;
        attack.channel = DamageChannel.Physical;
        attack.baseDamage = damage;
        attack.coefficient = 0;
        attack.reach = .95f;
        attack.interval = interval;
        attack.windup = .45f;
        EditorUtility.SetDirty(attack);
        return attack;
    }

    private static void AddMentor(CombatActor player, QuestDefinition trial, AdvancementDefinition edge,
        RuntimeAnimatorController controller, ItemDefinition[] items)
    {
        var journal = player.GetComponent<QuestJournal>();
        journal.Configure(journal.Definitions.Where(value => value != trial).Append(trial).ToArray());
        var shop = Object.FindObjectsByType<ItemShop>(FindObjectsInactive.Include).Single(value => value.DisplayName == "Trimery lumber trader");
        shop.Configure(shop.Offers.Where(offer => !items.Contains(offer.item))
            .Concat(items.Select((item,i) => new ItemShop.Offer { item = item, goldPrice = i == 0 ? 29 : 26 })).ToArray());
        EditorUtility.SetDirty(shop);
        var watch = GameObject.Find("Trimery North gate watch 5").transform;
        var nav = NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            Physics.SyncTransforms();
            var existing = GameObject.Find("Veteran Mara - Defender mentor");
            if (existing != null) Object.DestroyImmediate(existing);
            var mentor = new GameObject("Veteran Mara - Defender mentor");
            mentor.layer = 11;
            mentor.transform.position = OpenPoint(watch.position + new Vector3(-3.2f,0,-1.2f));
            mentor.transform.rotation = Quaternion.Euler(0,165,0);
            var collider = mentor.AddComponent<CapsuleCollider>();
            collider.radius = .42f;
            collider.height = 1.9f;
            collider.center = Vector3.up * .95f;
            AttachModel(mentor.transform, controller, false);
            var talk = mentor.AddComponent<DefenderMentor>();
            talk.ConfigureName("Veteran Mara, North Gate Watch");
            talk.Configure(trial, edge);
            var approach = new GameObject("North gate watch approach").transform;
            approach.SetParent(mentor.transform, false);
            approach.position = OpenPoint(mentor.transform.position + Vector3.back * 1.45f);
            talk.ConfigureApproach(approach);
            mentor.AddComponent<InteractionPrompt>().Configure(talk, player);
            Debug.Log($"DICEFREE_DEFENDER_MENTOR {mentor.transform.position} APPROACH {approach.position}");
        }
        finally { nav.Remove(); }
    }

    private static Vector3 OpenPoint(Vector3 wanted)
    {
        for (int ring=0; ring<=6; ring++)
            for (int i=0; i<(ring==0?1:8); i++)
            {
                var point = wanted + Quaternion.Euler(0,i*45,0)*Vector3.forward*ring*.8f;
                if (NavMesh.SamplePosition(point,out var hit,.8f,1) && Mathf.Abs(hit.position.y-wanted.y)<1 &&
                    !Physics.CheckCapsule(hit.position+Vector3.up*.32f,hit.position+Vector3.up*1.36f,.30f,1<<9))
                    return hit.position;
            }
        throw new InvalidOperationException("No open point beside the Trimery watch at " + wanted);
    }

    private static void AddVisual(CombatActor player, RuntimeAnimatorController motion)
    {
        var switcher = new SerializedObject(player.GetComponent<ClassPresentationSwitcher>());
        var bindings = switcher.FindProperty("bindings");
        int index = Enumerable.Range(0, bindings.arraySize).Where(i =>
            bindings.GetArrayElementAtIndex(i).FindPropertyRelative("classId").stringValue == ClassId)
            .DefaultIfEmpty(-1).First();
        if (index < 0)
        {
            index = bindings.arraySize;
            bindings.InsertArrayElementAtIndex(index);
        }
        else if (bindings.GetArrayElementAtIndex(index).FindPropertyRelative("visualRoot").objectReferenceValue is Transform old)
            Object.DestroyImmediate(old.gameObject);
        var root = new GameObject("Trimery Defender").transform;
        root.SetParent(player.transform, false);
        var animator = AttachModel(root, motion, false);
        root.gameObject.SetActive(false);
        var entry = bindings.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("classId").stringValue = ClassId;
        entry.FindPropertyRelative("visualRoot").objectReferenceValue = root;
        entry.FindPropertyRelative("animator").objectReferenceValue = animator;
        switcher.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddMenu(ActorDefinition defender, RuntimeAnimatorController motion, ItemDefinition[] items)
    {
        var catalog = Load<MenuPresentationCatalog>(Menu);
        var root = new GameObject("Defender menu presentation");
        try
        {
            root.AddComponent<ActorStats>().Configure(defender);
            root.AddComponent<CarriedInventory>().Configure(catalog.items.Except(items).Concat(items).ToArray());
            root.AddComponent<Equipment>();
            root.AddComponent<EquipmentPresentation>();
            var visual = new GameObject("Class visual").transform;
            visual.SetParent(root.transform, false);
            AttachModel(visual, motion, true);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, Data + "Defender menu presentation.prefab");
            catalog.forms = catalog.forms.Where(form => form.classId != ClassId)
                .Append(new MenuPresentationCatalog.Form { classId = ClassId, tier = 2, preview = prefab }).ToArray();
            catalog.items = catalog.items.Except(items).Concat(items).ToArray();
            EditorUtility.SetDirty(catalog);
        }
        finally { Object.DestroyImmediate(root); }
    }

    private static Animator AttachModel(Transform parent, RuntimeAnimatorController motion, bool portrait)
    {
        var model = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(Model), parent);
        var animator = model.GetComponent<Animator>();
        animator.runtimeAnimatorController = motion;
        animator.applyRootMotion = false;
        var meshes = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (animator.avatar == null || !animator.avatar.isValid || meshes.Length != 3)
            throw new InvalidOperationException("Expected the Defender armor and independent sword and shield meshes.");
        parent.gameObject.AddComponent<DefenderGearPresentation>().Configure(
            meshes.Single(mesh => mesh.name.Contains("Gate Sword")).gameObject,
            meshes.Single(mesh => mesh.name.Contains("Gate Shield")).gameObject);
        if (portrait) foreach (var mesh in meshes) mesh.updateWhenOffscreen = true;
        return animator;
    }

    private static ItemDefinition Gear(bool shield)
    {
        var item = Asset<ItemDefinition>(shield ? Shield : Sword);
        item.stableId = shield ? DefenderGearPresentation.ShieldId : DefenderGearPresentation.SwordId;
        item.displayName = item.name;
        item.sourceId = "shop.trimery.lumber";
        item.slot = shield ? EquipmentSlot.OffHand : EquipmentSlot.MainHand;
        item.handsRequired = 1;
        item.itemLevel = 1;
        item.allowedClassIds = new[] { ClassId };
        item.flavor = shield ? "Blue enamel carries the three pillars of the North Gate watch." :
            "A stout blade issued to the North Gate watch for close work in the stockade.";
        item.stats = shield ? new EquipmentStats { physicalDefense = 5, magicalDefense = 3, attributes = new AttributeValues { vitality = 2 } } :
            new EquipmentStats { basicAttackFlatBonus = 4, attributes = new AttributeValues { strength = 2 } };
        var texture = Load<Texture2D>(Data + (shield ? "Shield Bash" : "Shield Sweep") + " icon.asset");
        var sprite = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture)).OfType<Sprite>().FirstOrDefault();
        if (sprite == null)
        {
            sprite = Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));
            sprite.name = item.displayName;
            AssetDatabase.AddObjectToAsset(sprite,texture);
        }
        item.icon = sprite;
        EditorUtility.SetDirty(item);
        return item;
    }

    private static void AddInventory(CarriedInventory inventory, ItemDefinition[] items)
    {
        var serialized = new SerializedObject(inventory);
        var definitions = serialized.FindProperty("definitions");
        foreach (var item in items)
        {
            if (Enumerable.Range(0,definitions.arraySize).Any(i => definitions.GetArrayElementAtIndex(i).objectReferenceValue == item)) continue;
            int index = definitions.arraySize;
            definitions.InsertArrayElementAtIndex(index);
            definitions.GetArrayElementAtIndex(index).objectReferenceValue = item;
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static T Asset<T>(string name) where T : ScriptableObject
    {
        string path = Data + name + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;
        var value = ScriptableObject.CreateInstance<T>();
        value.name = name;
        AssetDatabase.CreateAsset(value,path);
        return value;
    }

    private static T Load<T>(string path) where T : Object =>
        AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidOperationException("Missing " + path);
}