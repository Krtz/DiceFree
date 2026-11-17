using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Dungeons;
using DiceFree.Items;
using DiceFree.Quests;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public static class AncientRoadDragonFightAuthoring
{
    const string Arena = "Assets/_DiceFree/Scenes/AncientRoadDragonLair.unity";
    const string World = "Assets/_DiceFree/Scenes/Cornberg.unity";
    const string Data = "Assets/_DiceFree/Settings/AncientRoadDragon/Encounter";
    const string Items = "Assets/_DiceFree/Resources/DiceFree/DungeonRewards/AncientRoadDragon";
    const string Story = "Assets/_DiceFree/Settings/Arborium/";
    const string Elements = "Assets/_DiceFree/Settings/Combat/Elements/";

    public static void Batch()
    {
        Directory.CreateDirectory(Data);
        Directory.CreateDirectory(Items);
        AssetDatabase.Refresh();

        var arena = EditorSceneManager.OpenScene(Arena);
        var owner = GameObject.Find("Ancient Road Dragon - arena preview");
        var originalBlock = GameObject.Find("Roost ground clearance");
        if (owner == null || originalBlock == null) throw new InvalidOperationException("Expected the existing dragon preview.");
        Object.DestroyImmediate(originalBlock);
        owner.name = "Ancient Road Dragon";
        owner.layer = 10;
        InstallActor(owner);
        RebuildNavigation(arena.GetRootGameObjects().Single(r => r.GetComponent<WorldNavigation>() != null), owner.transform.position);
        EditorSceneManager.MarkSceneDirty(arena);
        if (!EditorSceneManager.SaveScene(arena)) throw new IOException("Could not save the dragon encounter.");

        var world = EditorSceneManager.OpenScene(World);
        InstallStory();
        EditorSceneManager.MarkSceneDirty(world);
        if (!EditorSceneManager.SaveScene(world)) throw new IOException("Could not save the Council quest.");
        AssetDatabase.SaveAssets();
    }

    static void InstallActor(GameObject owner)
    {
        var bite = Attack("Rending Bite", "bite", 112, DamageChannel.Physical);
        var breath = Attack("Verdant Breath", "breath", 275, DamageChannel.Magical, "Nature");
        var tail = Attack("Tail Sweep", "tail", 206, DamageChannel.Physical);
        var stonefall = Attack("Stonefall", "stonefall", 232, DamageChannel.Magical, "Earth");
        var stats = Object.Instantiate(Load<ActorDefinition>(
            "Assets/_DiceFree/Settings/Briarback/FourfoldHollow/Hollow Hierophant stats.asset"));
        stats.stableId = AncientRoadDragonLairPortal.BossId;
        stats.displayName = "Ancient Road Dragon";
        stats.familyId = "dragon.ancient-road";
        stats.tags = new[] { "ancient-road-dragon", "dungeon-boss" };
        stats.baseHp = 11400;
        stats.experienceReward = 0;
        stats.overrideVitalityRegeneration = true;
        stats.regenerationPerVitality = 0;
        stats.physicalDefense = 25;
        stats.magicalDefense = 25;
        stats.basicAttack = bite;
        stats.resistances = Array.Empty<ElementResistance>();
        AssetDatabase.CreateAsset(stats, Data + "/Ancient Road Dragon stats.asset");

        var nav = owner.AddComponent<NavMeshAgent>();
        nav.enabled = false;
        nav.updateRotation = false;
        nav.radius = 2.2f;
        nav.height = 4.4f;
        nav.areaMask = 1;
        owner.AddComponent<TraversalMotor>();
        owner.AddComponent<ActorStats>().Configure(stats, 55);
        owner.AddComponent<Health>();
        owner.AddComponent<CombatActor>().Configure(1,2.2f);
        owner.AddComponent<CombatStatusController>();
        var hitbox = owner.AddComponent<CapsuleCollider>();
        hitbox.radius = 2.2f;
        hitbox.height = 4.5f;
        hitbox.center = Vector3.up * 2.2f;

        var model = owner.transform.Find("Perched dragon, authored idle");
        var bones = model.GetComponentInChildren<SkinnedMeshRenderer>().bones.Where(b => b != null).ToArray();
        Transform Bone(string term) => bones.FirstOrDefault(b => b.name.IndexOf(term,StringComparison.OrdinalIgnoreCase) >= 0);
        var neck = Bone("neck");
        var jaw = Bone("lower_lip");
        var tailJoint = Bone("tail");
        File.WriteAllText("Logs/AncientRoadDragonBones.txt",
            "Attack joints: "+neck?.name+", "+jaw?.name+", "+tailJoint?.name+"\n"+
            string.Join("\n",bones.Select(b=>b.name)));
        var warning = Load<Material>("Assets/_DiceFree/Resources/BriarbackFx/UnlitTransparent.mat");
        owner.AddComponent<AncientRoadDragonEncounter>().Configure(model,neck,jaw,tailJoint,
            bite,breath,tail,stonefall,warning,CreateRewards());
    }

    static AttackDefinition Attack(string title, string id, float damage, DamageChannel channel, string element = null)
    {
        var attack=ScriptableObject.CreateInstance<AttackDefinition>();
        attack.displayName=title;
        attack.stableId="attack.ancient-road-dragon."+id;
        attack.baseDamage=damage;
        attack.coefficient=0;
        attack.channel=channel;
        attack.requiresAccuracy=false;
        if (element != null) attack.element=Load<ElementDefinition>(Elements + element + ".asset");
        AssetDatabase.CreateAsset(attack, Data+"/"+title+".asset");
        return attack;
    }

    static void InstallStory()
    {
        var journal = Object.FindObjectsByType<QuestJournal>().Single();
        var clerk = GameObject.Find("Verdant Council Clerk");
        var summons = Load<QuestDefinition>(Story + "A Verdant Summons.asset");
        var existing = clerk.GetComponent<ConversationTarget>();
        var quest = ScriptableObject.CreateInstance<QuestDefinition>();
        quest.stableId = AncientRoadDragonLairPortal.QuestId;
        quest.title = "The Ancient Road Dragon";
        quest.requiredLevel = 35;
        quest.completedQuestIds = new[]{summons.stableId};
        quest.offer = "The Reedscale watch the old mountain road for their master. Follow the trail northwest of Arborium into the Ancient Road Dragon’s lair. End his command over the factions and report back to the Council.";
        quest.activeDialogue = "Follow the old mountain road northwest of Arborium. The lair entrance is marked by green lamps. Return here after the Ancient Road Dragon has fallen.";
        quest.locationHint = "Take the mountain path northwest of Arborium, above the Verdant Wilds.";
        quest.turnInHint = "Return to the Verdant Council Clerk outside the Mage Observatory.";
        quest.turnInDialogue = "With the Ancient Road Dragon dead, Arborium has time to recover. I have sent word to Ironpine. Follow the trail south of the main road. A small fire will clear the pines blocking the pass to World 2. Find out there why the dragon came to Arborium.";
        quest.stages = new[] { new QuestObjective { kind=ObjectiveKind.Kill, count=1, dungeonBossOnly=true,
            contentId=AncientRoadDragonLairPortal.BossId, instruction="Defeat the Ancient Road Dragon in his lair" } };
        quest.rewardXp=10500;
        AssetDatabase.CreateAsset(quest, Story + "The Ancient Road Dragon.asset");

        var gate = clerk.GetComponent<QuestConversationGate>();
        gate.LimitToActiveQuest();
        var title = existing.DisplayName;
        var approach = clerk.transform.parent.GetComponentsInChildren<Transform>(true)
            .Single(t => t.name=="Council Clerk approach");
        var giver = clerk.AddComponent<QuestGiver>();
        giver.ConfigureName(title);
        giver.ConfigureApproach(approach);
        giver.Configure(quest);
        giver.ConfigureAmbient(true);
        var context = clerk.AddComponent<ContextualNpc>();
        context.ConfigureName(title);
        context.ConfigureApproach(approach);
        context.Configure(existing.NpcId, existing, giver);
        journal.Configure(journal.Definitions.Append(quest).ToArray());
        EditorUtility.SetDirty(journal);
        EditorUtility.SetDirty(gate);
        EditorUtility.SetDirty(existing);
        File.WriteAllLines("Logs/AncientRoadDragonStoryIds.txt",
            new Object[]{journal,clerk,existing,gate}.Select(item=>GlobalObjectId.GetGlobalObjectIdSlow(item).targetObjectId.ToString()));
    }

    static DungeonRewardPool CreateRewards()
    {
        var pool=ScriptableObject.CreateInstance<DungeonRewardPool>();
        pool.stableId="loot.ancient-road-dragon";
        pool.items=new[]
        {
            Reward("Roadwarden's Dragon Signet","signet",EquipmentSlot.Ring,"rarity.uncommon",
                new EquipmentStats{attributes=new AttributeValues(7),physicalDefense=7,magicalDefense=8},
                "A trail star set into a polished scale."),
            Reward("Jadescale Pendant","jadescale",EquipmentSlot.Amulet,"rarity.uncommon",
                new EquipmentStats{attributes=new AttributeValues{vitality=13,intelligence=13,spirit=9},magicalDefense=11},
                "Light catches between the veins of the green scale.")
        };
        pool.rare=Reward("Ancient Road Mantle","mantle",EquipmentSlot.Shoulders,"rarity.rare",
            new EquipmentStats{attributes=new AttributeValues(10),physicalDefense=13,magicalDefense=13,regeneration=9},
            "Green plates sewn over a worn traveller's mantle.");
        AssetDatabase.CreateAsset(pool,Data+"/Ancient Road Dragon treasure.asset");
        return pool;
    }

    static ItemDefinition Reward(string title,string id,EquipmentSlot slot,string rarity,EquipmentStats stats,string flavor)
    {
        var item=ScriptableObject.CreateInstance<ItemDefinition>();
        item.stableId="item.ancient-road-dragon."+id;
        item.displayName=title;
        item.itemLevel=55;
        item.slot=slot;
        item.rarityId=rarity;
        item.sourceId=AncientRoadDragonLairPortal.BossId;
        item.stats=stats;
        item.flavor=flavor;
        item.icon=MakeIcon(id);
        AssetDatabase.CreateAsset(item,Items+"/"+title+".asset");
        return item;
    }

    static Sprite MakeIcon(string id)
    {
        const int size=80;
        var texture=new Texture2D(size,size,TextureFormat.RGB24,false);
        for (int y=0;y<size;y++)
        for (int x=0;x<size;x++)
        {
            float u=(x-39.5f)/36,v=(y-39.5f)/36;
            float r=new Vector2(u,v).magnitude;
            var color=Color.Lerp(new Color(.025f,.045f,.036f),new Color(.13f,.25f,.19f),Mathf.Clamp01(1-r));
            if (id=="signet" && Mathf.Abs(new Vector2(u,(v+.12f)*1.26f).magnitude-.49f)<.075f ||
                id=="jadescale" && Mathf.Abs(new Vector2(u,(v-.61f)*1.08f).magnitude-.39f)<.04f && v>.18f)
                color=Color.Lerp(new Color(.4f,.24f,.08f),new Color(.94f,.77f,.36f),Mathf.Clamp01(.5f+v*.8f));
            float xCenter=id=="mantle"?Mathf.Abs(u)-.40f:u;
            float yCenter=id=="signet"?v-.29f:v+.1f;
            float outline=Mathf.Abs(xCenter)*1.53f+Mathf.Abs(yCenter)*.71f;
            if (id=="mantle" && Mathf.Abs(u)<.73f && v<.53f-Mathf.Abs(u)*.5f && v>-.58f)
                color=Color.Lerp(new Color(.09f,.125f,.093f),new Color(.58f,.45f,.23f),Mathf.Clamp01(.4f+v));
            if (outline<.33f)
            {
                float vein = Mathf.Pow(Mathf.Abs(Mathf.Sin(yCenter*31+xCenter*34)), 9) *.18f;
                color=Color.Lerp(new Color(.075f,.34f,.19f),new Color(.73f,.93f,.30f),
                    Mathf.Clamp01(.39f+yCenter*.92f-xCenter*.75f+vein));
                if (outline>.29f) color=Color.Lerp(color,new Color(.91f,.74f,.39f),.58f);
            }
            if (Mathf.Max(Mathf.Abs(u),Mathf.Abs(v)) is >1.01f and <1.065f)
                color=new Color(.42f,.32f,.14f);
            texture.SetPixel(x,y,color);
        }
        texture.Apply();
        string path=Data+"/"+id+"-icon.png";
        File.WriteAllBytes(path,texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var imp=(TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType=TextureImporterType.Sprite;
        imp.spriteImportMode=SpriteImportMode.Single;
        imp.mipmapEnabled=false;
        imp.SaveAndReimport();
        return Load<Sprite>(path);
    }

    static void RebuildNavigation(GameObject root, Vector3 at)
    {
        var origin = AncientRoadDragonLairPortal.Origin;
        var bounds = new Bounds(origin+Vector3.up*4,new Vector3(92,26,103));
        Physics.SyncTransforms();
        var sources = new List<NavMeshBuildSource>();
        var blockers = root.GetComponentsInChildren<Collider>().Where(c=>c.gameObject.layer==9)
            .Select(c=>new NavMeshBuildMarkup{root=c.transform,overrideArea=true,area=1}).ToList();
        NavMeshBuilder.CollectSources(bounds,(1<<8)|(1<<9),NavMeshCollectGeometry.PhysicsColliders,0,blockers,sources);
        var settings=NavMesh.GetSettingsByID(0);
        settings.agentRadius=.55f;
        settings.agentHeight=1.85f;
        settings.agentClimb=.4f;
        settings.overrideVoxelSize=true;
        settings.voxelSize=.16f;
        var fresh=NavMeshBuilder.BuildNavMeshData(settings,sources,bounds,Vector3.zero,Quaternion.identity);
        if (fresh==null) throw new InvalidOperationException("Dragon fight navigation could not be baked.");
        var temp = NavMesh.AddNavMeshData(fresh);
        try
        {
            var path = new NavMeshPath();
            if (!NavMesh.SamplePosition(origin+AncientRoadDragonLairPortal.ArrivalOffset,out var arrival,1,1) ||
                !NavMesh.SamplePosition(at,out var roost,.5f,1) ||
                !NavMesh.CalculatePath(arrival.position,roost.position,1,path) ||
                path.status!=NavMeshPathStatus.PathComplete)
                throw new InvalidOperationException("The dragon is not reachable from the entrance.");
            var original=root.GetComponent<WorldNavigation>().Data;
            EditorUtility.CopySerialized(fresh,original);
            EditorUtility.SetDirty(original);
        }
        finally { temp.Remove(); Object.DestroyImmediate(fresh); }
    }

    static T Load<T>(string path) where T:Object =>
        AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new IOException("Required source is missing: "+path);
}

