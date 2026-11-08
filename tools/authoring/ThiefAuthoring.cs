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

public static class ThiefAuthoring
{
    private const string Data = "Assets/_DiceFree/Settings/Thief/";
    private const string Art = "Assets/_DiceFree/Art/Characters/QuietHands/";
    private const string Motion = "Assets/_DiceFree/Resources/PlayerMotion/TierTwo/Thief.controller";
    private const string Catalog = "Assets/_DiceFree/Settings/Progression/Class catalog.asset";
    private const string Development = "Assets/_DiceFree/Settings/Development/Priest/Development Priest catalog.asset";
    private const string Menu = "Assets/_DiceFree/Resources/MenuPresentationCatalog.asset";
    private const string Model = Art + "QuietHandsThief.fbx";
    private const string Trial = "Quiet Hands";
    private const string Ledger = "work.arborium.thief-route-ledger";
    private const string Case = "work.arborium.thief-practice-case";
    private static readonly string[] DaggerNames = { "Quiet Hands Dagger", "Quiet Hands Stiletto" };

    public static void Batch()
    {
        Directory.CreateDirectory(Data);
        AssetDatabase.Refresh();
        var controller = PriestCompleteAuthoring.PrepareArt(Model,Art,Motion,Art+"QuietHandsImport.json");
        var catalog = Load<ClassCatalog>(Catalog);
        var parent = catalog.Resolve(ThiefMentor.ParentClassId);
        var thief = Asset<ActorDefinition>("Thief class");
        var basic = Attack("Thief dagger basic","attack.thief.daggers",null,5,1.05f,true);
        basic.reach = .8f;
        basic.interval = .87f;
        basic.windup = .27f;
        thief.stableId = ThiefSkillProgression.ThiefClassId;
        thief.displayName = "Thief";
        thief.tuning = parent.tuning;
        thief.baseAttributes = new AttributeValues { vitality=12,strength=7,agility=18,intelligence=6,spirit=8 };
        thief.growth = new AttributeValues { vitality=1.5f,strength=.8f,agility=3.1f,intelligence=.6f,spirit=1 };
        thief.baseHp = 10;
        thief.moveSpeed = parent.moveSpeed;
        thief.basicAttack = basic;
        var mana = Asset<ClassResourceProfile>("Thief Mana");
        EditorUtility.CopySerialized(Load<ClassResourceProfile>("Assets/_DiceFree/Settings/Resources/Physically Blessed Mana.asset"),mana);
        mana.stableId = "resource-profile.thief.mana";
        mana.classId = thief.stableId;

        var tech = Asset<ElementDefinition>("Tech");
        tech.stableId = "element.tech";
        tech.displayName = "Tech";
        Attack("Envenom stab","skill.thief.envenom",null,0,0,true);
        Attack("Envenom poison","skill.thief.envenom-poison",Load<ElementDefinition>("Assets/_DiceFree/Settings/Combat/Elements/Nature.asset"),2,.10f);
        Attack("Fan of Knives","skill.thief.fan-of-knives",null,8,.45f);
        Attack("Smoke Bomb","skill.thief.smoke-bomb",tech,.5f,.02f);
        var icons = Enumerable.Range(0,6).Select(i=>Icon((ThiefSkill)i)).ToArray();
        var daggers = new[] { Dagger(0,icons[1]), Dagger(1,icons[3]) };
        var quest = AuthorQuest();
        var edge = Asset<AdvancementDefinition>("Quiet Hands advancement");
        edge.stableId = "advance.physically-blessed.thief";
        edge.displayName = "Thief";
        edge.sourceClass = parent;
        edge.targetClass = thief;
        edge.requiredLevel = 30;
        edge.requiredQuestId = quest.stableId;
        AddClass(catalog,thief);
        AddClass(Load<ClassCatalog>(Development),thief);
        foreach (var data in new Object[] { thief,basic,mana,tech,quest,edge }) EditorUtility.SetDirty(data);
        AddMenu(thief,controller,daggers);
        AssetDatabase.SaveAssets();

        foreach (string sceneName in new[] { "Cornberg","DevelopmentRoom" })
        {
            var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/"+sceneName+".unity");
            var advancement=Object.FindObjectsByType<AdvancementController>(FindObjectsInactive.Include).Single();
            var player=advancement.GetComponent<CombatActor>();
            var classes=Load<ClassCatalog>(sceneName=="Cornberg"?Catalog:Development);
            var promotion=Load<AdvancementDefinition>(Data+"Quiet Hands advancement.asset");
            advancement.Configure(classes,advancement.Definitions.Append(promotion).ToArray());
            player.GetComponent<ManifestationPersistence>().ConfigureClassCatalog(classes);
            var resources=player.GetComponent<ActorResourceController>();
            resources.Configure(resources.Profiles.Append(Load<ClassResourceProfile>(Data+"Thief Mana.asset")).ToArray());
            player.gameObject.AddComponent<ThiefSkillCaster>().Configure(
                Load<AttackDefinition>(Data+"Envenom stab.asset"),Load<AttackDefinition>(Data+"Envenom poison.asset"),
                Load<AttackDefinition>(Data+"Fan of Knives.asset"),Load<AttackDefinition>(Data+"Smoke Bomb.asset"));
            player.gameObject.AddComponent<ThiefAbilityProvider>().Configure(Enumerable.Range(0,6)
                .Select(i=>Load<Texture2D>(Data+ThiefSkillProgression.Name((ThiefSkill)i)+" icon.asset")).ToArray());
            daggers = DaggerNames.Select(n=>Load<ItemDefinition>(Data+n+".asset")).ToArray();
            AddInventory(player.GetComponent<CarriedInventory>(),daggers);
            AddVisual(player,Load<RuntimeAnimatorController>(Motion));
            if (sceneName=="Cornberg") AddTrial(player,Load<QuestDefinition>(Data+Trial+".asset"),promotion,daggers);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save "+sceneName);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("QUIET_HANDS_THIEF_AUTHOR_OK");
    }

    private static void AddClass(ClassCatalog catalog,ActorDefinition thief)
    {
        if (catalog.Resolve(thief.stableId)!=null) throw new InvalidOperationException("Thief class already installed.");
        catalog.Configure(catalog.Classes.Append(thief).ToArray());
        EditorUtility.SetDirty(catalog);
    }

    private static AttackDefinition Attack(string title,string id,ElementDefinition element,float flat,float agility,bool accuracy=false)
    {
        var attack=Asset<AttackDefinition>(title);
        attack.stableId=id;
        attack.displayName=title;
        attack.channel=DamageChannel.Physical;
        attack.element=element;
        attack.requiresAccuracy=accuracy;
        attack.scaling=AttributeScaling.Weighted;
        attack.weights=new AttributeValues { agility=agility };
        attack.baseDamage=flat;
        attack.coefficient=1;
        EditorUtility.SetDirty(attack);
        return attack;
    }

    private static QuestDefinition AuthorQuest()
    {
        var quest=Asset<QuestDefinition>(Trial);
        quest.stableId=ThiefMentor.QuestId;
        quest.title=Trial;
        quest.requiredClassId=ThiefMentor.ParentClassId;
        quest.requiredLevel=30;
        quest.offer="The Guild pays me to see where its locks and routes fail. This is a sanctioned test. Copy the marked tally near the market and retrieve the wax seal from my practice case. Leave everything else as you found it.";
        quest.activeDialogue="Follow the marked sites through the market lanes. Copy the coded route tally and open Rook's practice case for its wax seal, then report back.";
        quest.locationHint="Both marked sites are in Arborium's market lanes near Rook.";
        quest.turnInHint="Return to Rook in the Arborium market lanes.";
        quest.stages=new[] { new QuestObjective { kind=ObjectiveKind.All,count=2,alternatives=new[] {
            new QuestLeaf { kind=ObjectiveKind.Interact,objectiveId="route-tally",contentId=Ledger,instruction="Copy the coded route tally" },
            new QuestLeaf { kind=ObjectiveKind.Interact,objectiveId="practice-seal",contentId=Case,instruction="Pick Rook's practice lock and recover the seal" }
        } } };
        return quest;
    }

    private static void AddTrial(CombatActor player,QuestDefinition trial,AdvancementDefinition edge,ItemDefinition[] daggers)
    {
        var journal=player.GetComponent<QuestJournal>();
        journal.Configure(journal.Definitions.Append(trial).ToArray());
        var trader=Object.FindObjectsByType<ItemShop>(FindObjectsInactive.Include).Single(shop=>shop.DisplayName=="Arborium market trader");
        trader.Configure(trader.Offers.Concat(daggers.Select((item,i)=>new ItemShop.Offer {item=item,goldPrice=i==0?22:18})).ToArray());
        EditorUtility.SetDirty(trader);
        var sites=new GameObject("Quiet Hands - Arborium Thief trial").transform;
        var navigation=NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            Physics.SyncTransforms();
            Vector3 center=trader.ApproachPosition;
            var mentor=Node(sites,"Rook - Thief mentor",OpenPoint(center+new Vector3(-7,0,3)));
            mentor.gameObject.layer=11;
            var capsule=mentor.gameObject.AddComponent<CapsuleCollider>();
            capsule.radius=.43f;capsule.height=1.8f;capsule.center=Vector3.up*.9f;
            AttachModel(mentor,Load<RuntimeAnimatorController>(Motion),false);
            var talk=mentor.gameObject.AddComponent<ThiefMentor>();
            talk.Configure(trial,edge);
            talk.ConfigureName("Rook, Guild Locksmith");
            var stand=Node(mentor,"Rook approach",OpenPoint(mentor.position+Vector3.back*1.4f));
            talk.ConfigureApproach(stand);
            mentor.gameObject.AddComponent<InteractionPrompt>().Configure(talk,player);

            AddSite(sites,player,trial,OpenPoint(center+new Vector3(7,0,-7)),"Coded route tally",Ledger,
                "route-tally","Copying the route tally",false);
            AddSite(sites,player,trial,OpenPoint(center+new Vector3(-15,0,-11)),"Rook's practice case",Case,
                "practice-seal","Picking the practice lock",true);
        }
        finally { navigation.Remove(); }
    }

    private static void AddSite(Transform sites,CombatActor player,QuestDefinition trial,Vector3 point,
        string name,string id,string objective,string verb,bool chest)
    {
        var root=Node(sites,name,point);
        root.gameObject.layer=11;
        var box=root.gameObject.AddComponent<BoxCollider>();
        box.center=Vector3.up*.38f;box.size=new Vector3(.91f,.76f,.72f);
        var target=root.gameObject.AddComponent<TimedInteractionTarget>();
        target.Configure(id,verb,chest?3.6f:2.2f);
        target.ConfigureName(name);
        target.ConfigureApproach(Node(root,"Clear approach",OpenPoint(point+Vector3.back*1.35f)));
        var gate=root.gameObject.AddComponent<QuestWorkGate>();
        gate.Configure(trial,objective);

        var leather=Material("Market case stained leather",new Color(.24f,.15f,.115f));
        var copper=Material("Quiet Hands aged copper",new Color(.66f,.43f,.24f));
        var vellum=Material("Quiet Hands copied vellum",new Color(.86f,.74f,.52f));
        var wax=Material("Quiet Hands marked wax",new Color(.31f,.58f,.57f));
        Piece(root,"Oak stand",new Vector3(0,.25f,0),new Vector3(.68f,.49f,.54f),leather);
        var before=Node(root,chest?"Locked case":"Folded tally",point);
        var after=Node(root,chest?"Opened case":"Unfolded tally",point);
        if (chest)
        {
            Piece(root,"Copper corners",new Vector3(0,.49f,.02f),new Vector3(.73f,.10f,.59f),copper);
            Piece(before,"Closed leather lid",new Vector3(0,.59f,0),new Vector3(.72f,.17f,.55f),leather);
            Piece(before,"Practice lock",new Vector3(0,.55f,-.29f),new Vector3(.13f,.14f,.052f),copper);
            var lid=Piece(after,"Open leather lid",new Vector3(0,.73f,.22f),new Vector3(.72f,.09f,.54f),leather);
            lid.localRotation=Quaternion.Euler(-55,0,0);
            Piece(after,"Seal imprint",new Vector3(0,.565f,-.08f),new Vector3(.16f,.025f,.14f),wax);
        }
        else
        {
            Piece(before,"Uncopied route notes",new Vector3(0,.52f,0),new Vector3(.33f,.05f,.24f),vellum);
            Piece(after,"Unfolded route notes",new Vector3(0,.53f,0),new Vector3(.53f,.05f,.34f),vellum);
            for (int i=0;i<4;i++) Piece(after,"Route tally stroke",new Vector3(-.13f+i*.09f,.562f,.02f),new Vector3(.016f,.014f,.17f),wax);
        }
        after.gameObject.SetActive(false);
        var beacon=Node(root,"Quiet Hands teal marker",point+Vector3.up*1.8f);
        Piece(beacon,"Marker diamond",Vector3.zero,Vector3.one*.21f,wax).localRotation=Quaternion.Euler(25,45,45);
        beacon.gameObject.SetActive(false);
        root.gameObject.AddComponent<GuildWorkSitePresentation>().Configure(player,gate,before.gameObject,after.gameObject,beacon.gameObject);
    }

    private static Vector3 OpenPoint(Vector3 wanted)
    {
        for (int ring=0;ring<=6;ring++)
            for(int direction=0;direction<(ring==0?1:8);direction++)
            {
                Vector3 p=wanted+Quaternion.Euler(0,direction*45,0)*Vector3.forward*ring*.8f;
                if (NavMesh.SamplePosition(p,out var hit,.8f,1) && Mathf.Abs(hit.position.y-wanted.y)<1 &&
                    !Physics.CheckCapsule(hit.position+Vector3.up*.32f,hit.position+Vector3.up*1.34f,.30f,1<<9))
                    return hit.position;
            }
        throw new InvalidOperationException("No walkable market-lane point near "+wanted);
    }

    private static void AddVisual(CombatActor player,RuntimeAnimatorController motion)
    {
        var visual=new GameObject("Quiet Hands Thief").transform;
        visual.SetParent(player.transform,false);
        var animator=AttachModel(visual,motion,false);
        visual.gameObject.AddComponent<ThiefStealthPresentation>();
        visual.gameObject.SetActive(false);
        var bindings=new SerializedObject(player.GetComponent<ClassPresentationSwitcher>()).FindProperty("bindings");
        int index=bindings.arraySize;
        bindings.InsertArrayElementAtIndex(index);
        var added=bindings.GetArrayElementAtIndex(index);
        added.FindPropertyRelative("classId").stringValue=ThiefSkillProgression.ThiefClassId;
        added.FindPropertyRelative("visualRoot").objectReferenceValue=visual;
        added.FindPropertyRelative("animator").objectReferenceValue=animator;
        bindings.serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddMenu(ActorDefinition thief,RuntimeAnimatorController motion,ItemDefinition[] daggers)
    {
        var catalog=Load<MenuPresentationCatalog>(Menu);
        var root=new GameObject("Thief menu presentation");
        try
        {
            root.AddComponent<ActorStats>().Configure(thief);
            root.AddComponent<CarriedInventory>().Configure(catalog.items.Concat(daggers).ToArray());
            root.AddComponent<Equipment>();
            root.AddComponent<EquipmentPresentation>();
            var visual=new GameObject("Class visual").transform;
            visual.SetParent(root.transform,false);
            AttachModel(visual,motion,true);
            var prefab=PrefabUtility.SaveAsPrefabAsset(root,Data+"Thief menu presentation.prefab");
            catalog.forms=catalog.forms.Append(new MenuPresentationCatalog.Form {classId=thief.stableId,tier=2,preview=prefab}).ToArray();
            catalog.items=catalog.items.Concat(daggers).ToArray();
            EditorUtility.SetDirty(catalog);
        }
        finally { Object.DestroyImmediate(root); }
    }

    private static Animator AttachModel(Transform parent,RuntimeAnimatorController motion,bool portrait)
    {
        var instance=(GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(Model),parent);
        var animator=instance.GetComponent<Animator>();
        animator.runtimeAnimatorController=motion;
        animator.applyRootMotion=false;
        var skins=instance.GetComponentsInChildren<SkinnedMeshRenderer>();
        if(animator.avatar==null||!animator.avatar.isValid||skins.Length!=3)
            throw new InvalidOperationException("Expected the Thief body and two independently skinned daggers.");
        if (portrait) foreach (var skin in skins) skin.updateWhenOffscreen=true;
        return animator;
    }

    private static void AddInventory(CarriedInventory inventory,ItemDefinition[] daggers)
    {
        var definitions=new SerializedObject(inventory).FindProperty("definitions");
        foreach (var dagger in daggers)
        {
            int index=definitions.arraySize;
            definitions.InsertArrayElementAtIndex(index);
            definitions.GetArrayElementAtIndex(index).objectReferenceValue=dagger;
        }
        definitions.serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static ItemDefinition Dagger(int side,Texture2D texture)
    {
        var item=Asset<ItemDefinition>(DaggerNames[side]);
        item.stableId=side==0?"item.arborium.quiet-hands-dagger":"item.arborium.quiet-hands-stiletto";
        item.displayName=DaggerNames[side];
        item.sourceId="shop.arborium.market";
        item.slot=side==0?EquipmentSlot.MainHand:EquipmentSlot.OffHand;
        item.handsRequired=1;
        item.itemLevel=1;
        item.allowedClassIds=new[] { ThiefSkillProgression.ThiefClassId };
        item.flavor=side==0?"Rook's broad blade for a straight cut.":"A slim companion blade for the other hand.";
        item.stats=new EquipmentStats { attributes=new AttributeValues {agility=side==0?3:2}, basicAttackFlatBonus=side==0?1:0 };
        var sprite=AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture)).OfType<Sprite>().FirstOrDefault();
        if(sprite==null)
        {
            sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));
            sprite.name=item.displayName;
            AssetDatabase.AddObjectToAsset(sprite,texture);
        }
        item.icon=sprite;
        EditorUtility.SetDirty(item);
        return item;
    }

    private static Texture2D Icon(ThiefSkill skill)
    {
        string name=ThiefSkillProgression.Name(skill);
        string path=Data+name+" icon.asset";
        const int size=72;
        var icon=new Texture2D(size,size,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Bilinear};
        var color=skill switch {
            ThiefSkill.Stealth=>new Color(.63f,.78f,.92f), ThiefSkill.Envenom=>new Color(.65f,.93f,.34f),
            ThiefSkill.KeenEdge=>new Color(.99f,.69f,.33f), ThiefSkill.FanOfKnives=>new Color(.69f,.89f,.97f),
            ThiefSkill.SmokeBomb=>new Color(.73f,.78f,.82f), _=>new Color(.85f,.66f,.34f) };
        var pixels=new Color[size*size];
        for(int y=0;y<size;y++) for(int x=0;x<size;x++)
        {
            Vector2 point=new((x+.5f)/size*2-1,(y+.5f)/size*2-1);
            float radius=point.magnitude, angle=Mathf.Atan2(point.y,point.x);
            float Ring(float r,float w)=>1-Mathf.SmoothStep(0,1,Mathf.Abs(radius-r)/w);
            float Blade(float rotation)
            {
                var v=(Vector2)(Quaternion.Euler(0,0,rotation)*point);
                float body=1-Mathf.SmoothStep(0,1,Mathf.Abs(v.x)/Mathf.Max(.02f,.19f-.13f*v.y));
                return Mathf.Abs(v.y)<.66f?body:0;
            }
            float shape=skill switch
            {
                ThiefSkill.Stealth => Ring(.62f,.054f)*Mathf.Abs(Mathf.Sin(angle*2))+
                    Mathf.Max(0,1-Mathf.Abs(Mathf.Sqrt(point.x*point.x+point.y*point.y*4)-.55f)*16),
                ThiefSkill.Envenom => Blade(37),
                ThiefSkill.KeenEdge => Mathf.Max(0,1-radius*(2+9*Mathf.Abs(Mathf.Sin(angle*2)))),
                ThiefSkill.FanOfKnives => Mathf.Max(Blade(32),Blade(-32)),
                ThiefSkill.SmokeBomb => Mathf.Clamp01((1-radius)*Mathf.PerlinNoise(point.x*4+7,point.y*4+3)*2)+Ring(.63f,.035f)*.35f,
                _ => Mathf.Max(Ring(.55f,.095f),Mathf.Max(0,1-(Mathf.Abs(point.x)+Mathf.Abs(point.y))*2.9f))
            };
            pixels[x+y*size]=Color.Lerp(new Color(.042f,.061f,.077f),Color.Lerp(color,Color.white,.15f),Mathf.Clamp01(shape));
        }
        icon.SetPixels(pixels);
        icon.Apply();
        AssetDatabase.CreateAsset(icon,path);
        return icon;
    }

    private static Transform Node(Transform parent,string name,Vector3 point)
    {
        var node=new GameObject(name).transform;
        node.SetParent(parent,false);
        node.position=point;
        return node;
    }

    private static Transform Piece(Transform parent,string name,Vector3 position,Vector3 size,Material material)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.name=name;
        go.transform.SetParent(parent,false);
        go.transform.localPosition=position;
        go.transform.localScale=size;
        go.GetComponent<MeshRenderer>().sharedMaterial=material;
        return go.transform;
    }

    private static Material Material(string name,Color color)
    {
        var path=Data+name+".mat";
        var existing=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(existing!=null)return existing;
        var material=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color};
        AssetDatabase.CreateAsset(material,path);
        return material;
    }

    private static T Asset<T>(string name) where T:ScriptableObject
    {
        string path=Data+name+".asset";
        var current=AssetDatabase.LoadAssetAtPath<T>(path);
        if(current!=null)return current;
        var result=ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(result,path);
        return result;
    }

    private static T Load<T>(string path) where T:Object=>
        AssetDatabase.LoadAssetAtPath<T>(path)??throw new InvalidOperationException("Missing "+path);
}
