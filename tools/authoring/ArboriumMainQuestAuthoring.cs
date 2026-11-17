using System;
using System.IO;
using System.Linq;
using DiceFree.Combat;
using DiceFree.EditorTools;
using DiceFree.Quests;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public static class ArboriumMainQuestAuthoring
{
    private const string Settings = "Assets/_DiceFree/Settings/Arborium";
    private const string Art = "Assets/_DiceFree/Art/World/";
    private const string Investigation = "quest.arborium.routes-under-orders";
    private const string Summons = "quest.arborium.verdant-summons";
    private const string Ledger = "work.arborium.reedscale-watch-ledger";
    private const string Dispatch = "work.arborium.reedscale-sealed-dispatch";
    private const string Clerk = "npc.arborium.council-clerk";
    private const string Audience = "conversation.arborium.verdant-summons";

    public static void Batch()
    {
        var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
        if (GameObject.Find("Arborium dragon investigation") != null)
            throw new InvalidOperationException("The Guild investigation is already in Cornberg.");
        var journal = Object.FindObjectsByType<QuestJournal>().Single();
        var steward = GameObject.Find("Guild Steward - timber commissions").GetComponent<QuestGiver>();
        var root = Place(null, "Arborium dragon investigation", Vector3.zero);
        var player = journal.GetComponent<CombatActor>();
        var routes = Quest("Routes Under Orders", Investigation, 35, 5200,
            new[] { "quest.trimery.road-to-arborium", "quest.arborium.reedscale-lookout" });
        routes.offer = "The lookout report names the Ancient Road Dragon. We need to know what it is asking of the Reedscale. Follow the Eastwatch trail toward Briarback and search the two highland camps. Bring back the watch ledger and the sealed dispatch, and defeat three Reedscale defenders.";
        routes.activeDialogue = "The first hide shelter is on the north side of the trail out of Eastwatch. The second camp is farther east, on the south side. Read the ledger and recover the dispatch in either order. Bring the evidence back to me in Arborium.";
        routes.locationHint = "Follow the highland trail east from Eastwatch. Search the north camp for the ledger and the south camp for the dispatch.";
        routes.turnInHint = "Return to the Guild Steward in Arborium.";
        routes.turnInDialogue = "The ledger assigns the Reedscale to watch the mountain path northwest of Arborium. The dispatch names Quillboar and wolf companies and sets the same signal for their march. The Council needs to see both records. I'll send our recommendation with them.";
        routes.stages = new[]
        {
            new QuestObjective
            {
                kind = ObjectiveKind.All, count = 5,
                instruction = "Recover the two records and defeat three Reedscale defenders",
                alternatives = new[]
                {
                    EvidenceObjective(Ledger, "Read the north camp's watch ledger"),
                    EvidenceObjective(Dispatch, "Recover the south camp's sealed dispatch"),
                    new QuestLeaf { kind = ObjectiveKind.Kill, objectiveId = "defeat-reedscale-defenders",
                        familyId = "reedscale.lizardman", count = 3, creditMode = QuestCreditMode.NearbyParty,
                        instruction = "Defeat Reedscale defenders" }
                }
            }
        };

        var summons = Quest("A Verdant Summons", Summons, 35, 1500, new[] { Investigation });
        summons.offer = "The Verdant Council has read your report and formally summoned you. Present yourself to the Council Clerk outside the Mage Observatory in Arborium's mage district.";
        summons.activeDialogue = "Take the summons to the Council Clerk outside the Mage Observatory, west of the World Tree.";
        summons.locationHint = "Meet the Council Clerk in the courtyard north of the Mage Observatory, west of the World Tree.";
        summons.completeOnObjectives = true;
        summons.stages = new[]
        {
            new QuestObjective { kind = ObjectiveKind.TalkTo, contentId = Clerk, conversationId = Audience,
                count = 1, instruction = "Present the summons to the Council Clerk at the Mage Observatory" }
        };

        var nav = NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            var north = AddEvidence(root, player, routes, Ledger, "Reedscale watch ledger",
                "Reading the watch ledger", new Vector3(81, 10.4f, 433.5f), new Vector3(80, 10.4f, 432), 43);
            var south = AddEvidence(root, player, routes, Dispatch, "Reedscale sealed dispatch",
                "Recovering the sealed dispatch", new Vector3(129, 9.5f, 425), new Vector3(129, 9.5f, 426.8f), -30);
            var clerk = AddClerk(root, player, summons);
            RequirePath(steward.ApproachPosition, north.ApproachPosition);
            RequirePath(north.ApproachPosition, south.ApproachPosition);
            RequirePath(steward.ApproachPosition, clerk.ApproachPosition);
        }
        finally { nav.Remove(); }
        journal.Configure(journal.Definitions.Concat(new[] { routes, summons }).ToArray());
        steward.ConfigureAdditional(steward.Offers.Skip(1).Concat(new[] { routes, summons }).ToArray());
        foreach (var item in new Object[] { journal, steward, routes, summons }) EditorUtility.SetDirty(item);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the Arborium story chapter.");
        AssetDatabase.SaveAssets();
        File.WriteAllLines("Logs/ArboriumMainQuestSceneIds.txt", new Object[] { journal, steward }
            .Select(item => GlobalObjectId.GetGlobalObjectIdSlow(item).targetObjectId.ToString()));
        Debug.Log("DICEFREE_ARBORIUM_MAIN_QUEST_AUTHORING_OK");
    }

    private static QuestDefinition Quest(string title, string id, int level, int xp, string[] prerequisites)
    {
        var path = Settings + "/" + title + ".asset";
        var quest = AssetDatabase.LoadAssetAtPath<QuestDefinition>(path);
        if (quest == null)
        {
            quest = ScriptableObject.CreateInstance<QuestDefinition>();
            AssetDatabase.CreateAsset(quest, path);
        }
        quest.name = quest.title = title;
        quest.stableId = id;
        quest.requiredLevel = level;
        quest.rewardXp = xp;
        quest.completedQuestIds = prerequisites;
        return quest;
    }

    private static QuestLeaf EvidenceObjective(string id, string label) =>
        new() { kind = ObjectiveKind.Interact, objectiveId = id, contentId = id, count = 1, instruction = label };

    private static TimedInteractionTarget AddEvidence(Transform root, CombatActor player, QuestDefinition quest,
        string id, string name, string verb, Vector3 position, Vector3 approach, float yaw)
    {
        var table = Place(root, name, Nav(position));
        table.rotation = Quaternion.Euler(0, yaw, 0);
        table.gameObject.layer = 11;
        var oak = MaterialAt("ReedscaleLookout/Hewn larch.mat");
        var metal = MaterialAt("ReedscaleLookout/Hammered bronze.mat");
        var parchment = MaterialAt("ReedscaleLookout/Bone and parchment.mat");
        var leather = MaterialAt("ReedscaleLookout/Boiled leather.mat");
        var cloth = MaterialAt("ReedscaleLookout/Reedscale indigo cloth.mat");
        var glow = MaterialAt("ReedscaleLookout/Guild objective light.mat");
        Piece(table, "Larch table", PrimitiveType.Cube, new(0,.80f,0), new(1.22f,.12f,.83f), oak);
        foreach (int side in new[] { -1, 1 })
            Piece(table, "Table trestle", PrimitiveType.Cube, new(side*.40f,.39f,0), new(.13f,.78f,.64f), oak);
        Piece(table, "Indigo runner", PrimitiveType.Cube, new(0,.87f,0), new(.77f,.02f,.65f), cloth);
        Piece(table, "Hide record backing", PrimitiveType.Cube, new(-.06f,.901f,.01f), new(.57f,.047f,.41f), leather);
        Piece(table, "Parchment record", PrimitiveType.Cube, new(-.06f,.931f,.01f), new(.53f,.025f,.37f), parchment);
        if (id == Ledger)
        {
            for (int line = 0; line < 4; line++)
                Piece(table, "Ledger ink", PrimitiveType.Cube, new(-.09f,.947f,.12f-line*.072f), new(.35f,.006f,.018f), leather);
        }
        else
        {
            Piece(table, "Dispatch strap", PrimitiveType.Cube, new(-.06f,.95f,.02f), new(.054f,.013f,.34f), leather);
            Piece(table, "Stamped seal", PrimitiveType.Cylinder, new(-.06f,.964f,.05f), new(.115f,.013f,.115f), metal);
        }
        var target = table.gameObject.AddComponent<TimedInteractionTarget>();
        target.ConfigureName(name);
        target.Configure(id, verb, 2);
        var walkTo = Place(root, name + " approach", Nav(approach));
        target.ConfigureApproach(walkTo);
        var collider = table.gameObject.AddComponent<BoxCollider>();
        collider.center = new(0,.49f,0);
        collider.size = new(1.23f,.98f,.83f);
        var gate = table.gameObject.AddComponent<QuestWorkGate>();
        gate.Configure(quest,id);
        var beacon = Place(table, "Investigation marker", new(0,1.8f,0));
        Piece(beacon,"Guild diamond",PrimitiveType.Cube,Vector3.zero,Vector3.one*.22f,glow)
            .transform.localRotation = Quaternion.Euler(35,0,45);
        beacon.gameObject.SetActive(false);
        table.gameObject.AddComponent<GuildWorkSitePresentation>().Configure(player,gate,null,null,beacon.gameObject);
        return target;
    }

    private static ConversationTarget AddClerk(Transform root, CombatActor player, QuestDefinition summons)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Art+"CornbergBatch/NPCs/NPC_Alchemist.fbx");
        if (prefab == null) throw new IOException("Missing Council Clerk model.");
        var at = Nav(new Vector3(46,0,306.3f));
        var clerk = Place(root, "Verdant Council Clerk", at);
        clerk.rotation = Quaternion.Euler(0, 190, 0);
        clerk.gameObject.layer = 11;
        var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab,clerk);
        PrefabUtility.UnpackPrefabInstance(model,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        var renderers = model.GetComponentsInChildren<Renderer>();
        Bounds BoundsOf()
        {
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }
        model.transform.localScale *= 2.27f / BoundsOf().size.y;
        var bounds = BoundsOf();
        model.transform.position += new Vector3(at.x-bounds.center.x,at.y-bounds.min.y,at.z-bounds.center.z);
        foreach (var collider in model.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(collider);
        var capsule = clerk.gameObject.AddComponent<CapsuleCollider>();
        capsule.height = 2.35f;
        capsule.radius = .51f;
        capsule.center = Vector3.up * 1.15f;
        var conversation = clerk.gameObject.AddComponent<ConversationTarget>();
        conversation.ConfigureName("Verdant Council Clerk");
        conversation.Configure(Clerk, Audience, "The Council has entered your testimony into the record. Your summons is accepted. The Reedscale orders give us a mountain approach northwest of Arborium. The Council will need your help finding a way through that pass to the Ancient Road Dragon.");
        conversation.ConfigureApproach(Place(root,"Council Clerk approach",Nav(new Vector3(45,0,304.7f))));
        clerk.gameObject.AddComponent<QuestConversationGate>().Configure(summons,player,null);
        clerk.gameObject.AddComponent<QuestWorldMarker>();
        return conversation;
    }

    private static Material MaterialAt(string path) =>
        AssetDatabase.LoadAssetAtPath<Material>(Art+path) ?? throw new IOException("Missing material "+path);

    private static Transform Place(Transform parent, string name, Vector3 at)
    {
        var item = new GameObject(name).transform;
        item.SetParent(parent,false);
        item.localPosition = at;
        return item;
    }

    private static GameObject Piece(Transform parent,string name,PrimitiveType shape,Vector3 at,Vector3 size,Material art)
    {
        var item = GameObject.CreatePrimitive(shape);
        item.name = name;
        item.layer = 2;
        item.transform.SetParent(parent,false);
        item.transform.localPosition = at;
        item.transform.localScale = size;
        Object.DestroyImmediate(item.GetComponent<Collider>());
        item.GetComponent<Renderer>().sharedMaterial = art;
        return item;
    }

    private static Vector3 Nav(Vector3 at)
    {
        if (!NavMesh.SamplePosition(at,out var hit,1.5f,1))
            throw new InvalidOperationException("Missing quest navigation near "+at);
        return hit.position;
    }

    private static void RequirePath(Vector3 start, Vector3 finish)
    {
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(start,finish,1,path) || path.status != NavMeshPathStatus.PathComplete)
            throw new InvalidOperationException($"No complete quest path from {start} to {finish}.");
    }
}
