using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
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
using Object = UnityEngine.Object;

public static class PsionMirrorAuthoring
{
    private const string Data = "Assets/_DiceFree/Settings/Psion/";
    private const string Trial = Data + "Trial/";
    private const string Art = Data + "Mirrorweave/";
    private const string Gear = Data + "Gear/";
    private const string WorldCatalog = "Assets/_DiceFree/Settings/Progression/Class catalog.asset";
    private const string MenuCatalog = "Assets/_DiceFree/Resources/MenuPresentationCatalog.asset";
    private const string Cornberg = "Assets/_DiceFree/Scenes/Cornberg.unity";
    private const string Chamber = "Assets/_DiceFree/Scenes/" + PsionMirrorSite.SceneName + ".unity";
    private static readonly Vector3 ChamberCenter = new(3140,0,3420);
    private static string ClassId => PsionSkillProgression.PsionClassId;

    public static void Batch()
    {
        Directory.CreateDirectory(Trial);
        AssetDatabase.Refresh();
        var form = Load<GameObject>(Art+"Mirrorweave Psion.prefab");
        var gear = new[] { Load<ItemDefinition>(Gear+"Mirrorsteel Mindblade.asset"), Load<ItemDefinition>(Gear+"Mirrorleaf Codex.asset") };
        var psion = Load<ActorDefinition>(Data+"Psion class.asset");
        var edge = Load<AdvancementDefinition>(Data+"Psion development edge.asset");
        AuthorQuest(edge);
        var catalog = Load<ClassCatalog>(WorldCatalog);
        catalog.Configure(catalog.Classes.Where(value=>value.stableId!=ClassId).Append(psion).ToArray());
        EditorUtility.SetDirty(catalog);
        AuthorMenu(psion,form,gear);
        AssetDatabase.SaveAssets();
        AuthorChamber();
        AssetDatabase.SaveAssets();
        AuthorWorld();
        EditorBuildSettings.scenes = EditorBuildSettings.scenes.Where(scene=>scene.path!=Chamber)
            .Append(new EditorBuildSettingsScene(Chamber,true)).ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("DICEFREE_PSION_MIRROR_AUTHOR_OK");
    }

    private static QuestDefinition AuthorQuest(AdvancementDefinition edge)
    {
        var quest = Asset<QuestDefinition>("The Third Reflection");
        quest.stableId = PsionMentor.QuestId;
        quest.title = "The Third Reflection";
        quest.requiredClassId = PsionMentor.ParentId;
        quest.requiredLevel = edge.requiredLevel;
        quest.offer = "Three reflections wait beneath the Guild. Only one has a will of its own. Walk sideways and watch their turns. Two move together, while the real one follows you. Strike the one that follows and defeat it.";
        quest.activeDialogue = "The chamber is ready. Move before you strike. A hit on a false reflection ends the attempt.";
        quest.locationHint = "Speak to Ilyra in the Adventurers’ Guild reception in Arborium.";
        quest.turnInHint = "Return to Ilyra at the Adventurers’ Guild in Arborium.";
        quest.stages = new[] { new QuestObjective { kind=ObjectiveKind.SolvePuzzle, contentId=PsionMirrorSite.PuzzleId,
            objectiveId="third-reflection",count=1,instruction="Identify and defeat the real reflection in the Guild's mirror chamber" } };
        EditorUtility.SetDirty(quest);
        return quest;
    }

    private static void AuthorWorld()
    {
        var scene = EditorSceneManager.OpenScene(Cornberg);
        var catalog = Load<ClassCatalog>(WorldCatalog);
        var edge = Load<AdvancementDefinition>(Data+"Psion development edge.asset");
        var quest = Load<QuestDefinition>(Trial+"The Third Reflection.asset");
        var form = Load<GameObject>(Art+"Mirrorweave Psion.prefab");
        var gear = new[] { Load<ItemDefinition>(Gear+"Mirrorsteel Mindblade.asset"), Load<ItemDefinition>(Gear+"Mirrorleaf Codex.asset") };
        var advancement = Object.FindObjectsByType<AdvancementController>(FindObjectsInactive.Include).Single();
        advancement.Configure(catalog,advancement.Definitions.Where(value=>value!=edge).Append(edge).ToArray());
        var player = advancement.GetComponent<CombatActor>();
        player.GetComponent<ManifestationPersistence>().ConfigureClassCatalog(catalog);
        var resources = player.GetComponent<ActorResourceController>();
        var mana = Load<ClassResourceProfile>(Data+"Psion Mana.asset");
        resources.Configure(resources.Profiles.Where(value=>value!=mana).Append(mana).ToArray());
        var caster = player.GetComponent<PsionSkillCaster>() ?? player.gameObject.AddComponent<PsionSkillCaster>();
        caster.Configure(Load<AttackDefinition>(Data+"Psychic Bolt.asset"),Load<AttackDefinition>(Data+"Lightning Storm.asset"),
            Load<AttackDefinition>(Data+"Mental Image blade.asset"),Load<AttackDefinition>(Data+"Mental Image rupture.asset"),
            Load<ElementDefinition>(Data+"Metal.asset"));
        var castData = new SerializedObject(caster);
        castData.FindProperty("imageVisual").objectReferenceValue = form;
        castData.FindProperty("imageMaterial").objectReferenceValue = Load<Material>(Art+"Mirrorweave apparition.mat");
        castData.ApplyModifiedPropertiesWithoutUndo();
        var provider = player.GetComponent<PsionAbilityProvider>() ?? player.gameObject.AddComponent<PsionAbilityProvider>();
        provider.Configure(Enumerable.Range(0,PsionSkillProgression.SkillCount)
            .Select(i=>Load<Texture2D>(Data+PsionSkillProgression.Name((PsionSkill)i)+" icon.asset")).ToArray());

        var inventory = player.GetComponent<CarriedInventory>();
        inventory.Configure(inventory.Definitions.Except(gear).Concat(gear).ToArray());
        var journal = player.GetComponent<QuestJournal>();
        journal.Configure(journal.Definitions.Where(value=>value!=quest).Append(quest).ToArray());
        var switcher = player.GetComponent<ClassPresentationSwitcher>();
        var data = new SerializedObject(switcher);
        var bindings = data.FindProperty("bindings");
        int index = Enumerable.Range(0,bindings.arraySize)
            .Where(i=>bindings.GetArrayElementAtIndex(i).FindPropertyRelative("classId").stringValue==ClassId).DefaultIfEmpty(-1).First();
        if (index<0)
        {
            index=bindings.arraySize;
            bindings.InsertArrayElementAtIndex(index);
        }
        else if (bindings.GetArrayElementAtIndex(index).FindPropertyRelative("visualRoot").objectReferenceValue is Transform old)
            Object.DestroyImmediate(old.gameObject);
        var visual = (GameObject)PrefabUtility.InstantiatePrefab(form,player.transform);
        var fitted = PsionModelAuthoring.FitGear(visual,gear[0],gear[1]);
        visual.SetActive(false);
        var entry = bindings.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("classId").stringValue = ClassId;
        entry.FindPropertyRelative("visualRoot").objectReferenceValue = visual.transform;
        entry.FindPropertyRelative("animator").objectReferenceValue = visual.GetComponent<Animator>();
        data.ApplyModifiedPropertiesWithoutUndo();
        var equipment = player.GetComponent<EquipmentPresentation>();
        equipment.Configure(equipment.Bindings.Where(value=>!gear.Contains(value.definition)).Concat(fitted).ToArray());
        EditorUtility.SetDirty(equipment);

        var guild = Object.FindObjectsByType<ArboriumInterior>(FindObjectsInactive.Include)
            .Single(value=>value.BuildingName.StartsWith("Adventurers",StringComparison.Ordinal));
        var reception = guild.transform.Find("Floor 1 - oak rooms and stairwell");
        var oldMentor = reception.Find("Ilyra - Psion mentor");
        if (oldMentor != null) Object.DestroyImmediate(oldMentor.gameObject);
        var mentor = Group("Ilyra - Psion mentor",reception,Vector3.zero);
        mentor.gameObject.layer = 11;
        var nav = NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            Physics.SyncTransforms();
            mentor.SetPositionAndRotation(OpenPoint(guild.transform.TransformPoint(new Vector3(3.2f,0,2.95f))),
                guild.transform.rotation*Quaternion.Euler(0,180,0));
            var approach = Group("Ilyra conversation point",mentor,Vector3.zero);
            approach.position = OpenPoint(mentor.position+mentor.forward*1.5f);
            var collider = mentor.gameObject.AddComponent<CapsuleCollider>();
            collider.radius=.40f; collider.height=1.8f; collider.center=Vector3.up*.90f;
            PrefabUtility.InstantiatePrefab(form,mentor);
            var talk = mentor.gameObject.AddComponent<PsionMentor>();
            talk.ConfigureName("Ilyra, Keeper of Mirrors");
            talk.ConfigureApproach(approach);
            talk.Configure(quest,edge,gear);
            mentor.gameObject.AddComponent<InteractionPrompt>().Configure(talk,player);
            Debug.Log($"DICEFREE_MIRROR_MENTOR {mentor.position} APPROACH {approach.position}");
        }
        finally { nav.Remove(); }
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save Cornberg.");
    }

    private static Vector3 OpenPoint(Vector3 wanted)
    {
        for (int ring=0;ring<=3;ring++)
            for (int direction=0;direction<(ring==0?1:8);direction++)
            {
                var point = wanted+Quaternion.Euler(0,45*direction,0)*Vector3.forward*(ring*.55f);
                if (NavMesh.SamplePosition(point,out var hit,.65f,1) && Mathf.Abs(hit.position.y-wanted.y)<.75f &&
                    !Physics.CheckCapsule(hit.position+Vector3.up*.3f,hit.position+Vector3.up*1.4f,.27f,1<<9))
                    return hit.position;
            }
        throw new InvalidOperationException("No clear Guild navigation at "+wanted);
    }

    private static void AuthorMenu(ActorDefinition psion,GameObject form,ItemDefinition[] gear)
    {
        var catalog = Load<MenuPresentationCatalog>(MenuCatalog);
        catalog.items = catalog.items.Except(gear).Concat(gear).ToArray();
        var root = new GameObject("Psion menu presentation");
        try
        {
            root.AddComponent<ActorStats>().Configure(psion);
            root.AddComponent<CarriedInventory>().Configure(catalog.items);
            root.AddComponent<Equipment>();
            var visual = Group("Class visual",root.transform,Vector3.zero);
            var model = (GameObject)PrefabUtility.InstantiatePrefab(form,visual);
            foreach (var mesh in model.GetComponentsInChildren<SkinnedMeshRenderer>()) mesh.updateWhenOffscreen=true;
            root.AddComponent<EquipmentPresentation>().Configure(PsionModelAuthoring.FitGear(model,gear[0],gear[1]));
            var preview = PrefabUtility.SaveAsPrefabAsset(root,Trial+"Psion menu presentation.prefab");
            catalog.forms = catalog.forms.Where(value=>value.classId!=ClassId)
                .Append(new MenuPresentationCatalog.Form { classId=ClassId,tier=2,preview=preview }).ToArray();
            EditorUtility.SetDirty(catalog);
        }
        finally { Object.DestroyImmediate(root); }
    }

    private static void AuthorChamber()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var psion = Load<ActorDefinition>(Data+"Psion class.asset");
        var form = Load<GameObject>(Art+"Mirrorweave Psion.prefab");
        var root = new GameObject("Arborium mirror chamber").transform;
        root.position = ChamberCenter;
        var site = root.gameObject.AddComponent<PsionMirrorSite>();
        site.playerSpawn = ChamberCenter+new Vector3(0,.02f,-6);
        site.mapBounds = new Rect(ChamberCenter.x-11,ChamberCenter.z-9,22,18);
        var stone = Load<Material>("Assets/_DiceFree/Materials/Blockout/Fieldstone.mat");
        var brass = Load<Material>(Gear+"Brushed electrum.mat");
        var cloth = Load<Material>(Gear+"Midnight bookcloth.mat");
        var amethyst = Load<Material>(Gear+"Charged amethyst.mat");
        var slate = Load<Material>(Gear+"Blackened silver.mat");
        var steel = Load<Material>(Gear+"Mirrorsteel.mat");
        Block(root,"Dark chamber surround",new Vector3(0,-.58f,0),new Vector3(155,.15f,155),cloth);
        Block(root,"Dressed perimeter footing",new Vector3(0,-.39f,0),new Vector3(25.2f,.28f,21.2f),slate);
        Block(root,"Stone foundation",new Vector3(0,-.22f,0),new Vector3(24,.44f,20),slate,8);
        for(int z=-4;z<=4;z++)
            for(int x=-5;x<=5;x++)
                Block(root,"Dressed slate tile",new Vector3(x*2,.012f,z*2),new Vector3(1.965f,.024f,1.965f),
                    (x+z)%2==0?cloth:slate);
        for(int side=-1;side<=1;side+=2)
        {
            Block(root,"Side wall",new Vector3(side*11,1.4f,0),new Vector3(.65f,2.8f,18),stone,9);
            Block(root,"Side coping",new Vector3(side*11,2.84f,0),new Vector3(.87f,.20f,18),brass);
            for (int z=-7;z<=7;z+=3)
            {
                Block(root,"Wall pilaster",new Vector3(side*10.55f,1.4f,z),new Vector3(.54f,2.8f,.62f),stone);
                Block(root,"Amethyst wall lamp",new Vector3(side*10.18f,2.24f,z),new Vector3(.28f,.53f,.28f),amethyst);
            }
        }
        Block(root,"North wall",new Vector3(0,1.4f,8.9f),new Vector3(22,2.8f,.62f),stone,9);
        Block(root,"South wall",new Vector3(0,1.15f,-8.9f),new Vector3(22,2.3f,.62f),stone,9);
        for (int i=0;i<40;i++)
        {
            float a=i*9*Mathf.Deg2Rad;
            var inlay=Block(root,"Silver observation circle",new Vector3(Mathf.Sin(a)*3.65f,.034f,-2+Mathf.Cos(a)*3.65f),
                new Vector3(.54f,.019f,.085f),brass);
            inlay.localRotation=Quaternion.Euler(0,i*9,0);
        }
        for (int i=0;i<3;i++)
        {
            float x=(i-1)*4.8f;
            Block(root,"Beveled mirror base",new Vector3(x,.21f,6.86f),new Vector3(2.44f,.42f,.83f),slate,9);
            Block(root,"Violet mirror face",new Vector3(x,1.89f,7.18f),new Vector3(1.78f,2.77f,.16f),cloth);
            Block(root,"Silver reflection glass",new Vector3(x,1.89f,7.08f),new Vector3(1.47f,2.51f,.032f),steel);
            for(int side=-1;side<=1;side+=2)
                Block(root,"Gilded mirror frame",new Vector3(x+side*.88f,1.89f,6.99f),new Vector3(.12f,2.84f,.18f),brass);
            Block(root,"Mirror crown",new Vector3(x,3.31f,6.99f),new Vector3(1.87f,.14f,.18f),brass);
            Block(root,"Violet mirror gem",new Vector3(x,3.45f,6.94f),new Vector3(.24f,.24f,.24f),amethyst)
                .localRotation=Quaternion.Euler(0,15,45);
            var lamp=Group("Violet mirror light",root,new Vector3(x,3.2f,5)).gameObject.AddComponent<Light>();
            lamp.type=LightType.Point; lamp.color=new Color(.57f,.37f,1); lamp.range=7; lamp.intensity=2.2f;
        }

        var bounds = new Bounds(ChamberCenter,new Vector3(26,10,23));
        var sources = new List<NavMeshBuildSource>();
        Physics.SyncTransforms();
        NavMeshBuilder.CollectSources(bounds,(1<<8)|(1<<9),NavMeshCollectGeometry.PhysicsColliders,0,
            new List<NavMeshBuildMarkup>(),sources);
        var nav = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(0),sources,bounds,Vector3.zero,Quaternion.identity);
        if (nav == null) throw new InvalidOperationException("Mirror chamber navigation could not bake.");
        string navPath=Trial+"Mirror chamber navigation.asset";
        var previous=AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
        if (previous == null) AssetDatabase.CreateAsset(nav,navPath);
        else { EditorUtility.CopySerialized(nav,previous); Object.DestroyImmediate(nav); nav=previous; }
        root.gameObject.AddComponent<WorldNavigation>().Configure(nav);

        var strike = Asset<AttackDefinition>("Reflection strike");
        strike.stableId="attack.arborium.reflection"; strike.displayName="Reflection strike";
        strike.channel=DamageChannel.Magical; strike.baseDamage=27; strike.coefficient=0;
        strike.reach=3.35f; strike.interval=1.6f; strike.windup=.50f; strike.requiresAccuracy=false;
        EditorUtility.SetDirty(strike);
        var stats = Asset<ActorDefinition>("Guild reflection");
        stats.stableId="actor.arborium.reflection"; stats.displayName="Guild Reflection";
        stats.baseHp=440; stats.hpPerVitality=0; stats.overrideVitalityHp=true;
        stats.overrideVitalityRegeneration=true; stats.regenerationPerVitality=0;
        stats.tuning=psion.tuning; stats.moveSpeed=3.4f; stats.basicAttack=strike;
        EditorUtility.SetDirty(stats);
        site.reflections=Enumerable.Range(0,3).Select(i=>Reflection(root,form,stats,(i-1)*4.8f)).ToArray();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene,Chamber)) throw new InvalidOperationException("Could not save the mirror chamber.");
    }

    private static CombatActor Reflection(Transform parent,GameObject form,ActorDefinition definition,float x)
    {
        var unit=Group("Guild Reflection",parent,new Vector3(x,.02f,3));
        unit.rotation=Quaternion.Euler(0,180,0);
        unit.gameObject.layer=10;
        var agent=unit.gameObject.AddComponent<NavMeshAgent>();
        agent.enabled=false; agent.areaMask=1;
        var capsule=unit.gameObject.AddComponent<CapsuleCollider>();
        capsule.height=1.82f; capsule.radius=.38f; capsule.center=Vector3.up*.91f;
        unit.gameObject.AddComponent<ActorStats>().Configure(definition,30);
        var actor=unit.gameObject.AddComponent<CombatActor>();
        actor.Configure(1,.38f);
        unit.gameObject.AddComponent<CombatStatusController>();
        unit.gameObject.AddComponent<BasicAttack>();
        var body=(GameObject)PrefabUtility.InstantiatePrefab(form,unit);
        unit.gameObject.AddComponent<NovicePresentationDriver>().Configure(body.transform,body.GetComponent<Animator>());
        unit.gameObject.AddComponent<ActorFeedback>().Configure(body.transform);
        unit.gameObject.SetActive(false);
        return actor;
    }

    private static Transform Group(string name,Transform parent,Vector3 at)
    {
        var result = new GameObject(name).transform;
        result.SetParent(parent,false); result.localPosition=at;
        return result;
    }

    private static Transform Block(Transform parent,string name,Vector3 at,Vector3 size,Material material,int layer=0)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name=name; go.layer=layer;
        go.transform.SetParent(parent,false); go.transform.localPosition=at; go.transform.localScale=size;
        go.GetComponent<Renderer>().sharedMaterial=material;
        if (layer==0) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go.transform;
    }

    private static T Asset<T>(string name) where T:ScriptableObject
    {
        var path=Trial+name+".asset";
        var current=AssetDatabase.LoadAssetAtPath<T>(path);
        if (current != null) return current;
        var result=ScriptableObject.CreateInstance<T>();
        result.name=name;
        AssetDatabase.CreateAsset(result,path);
        return result;
    }

    private static T Load<T>(string path) where T:Object =>
        AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidOperationException("Missing "+path);
}
