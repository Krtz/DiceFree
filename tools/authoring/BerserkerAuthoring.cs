using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
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
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

public static class BerserkerAuthoring
{
    public const string Data="Assets/_DiceFree/Settings/Berserker/";
    public const string Art="Assets/_DiceFree/Art/Characters/Berserker/";
    const string Parent="Assets/_DiceFree/Settings/Skills/Greatblade/";
    const string Motion="Assets/_DiceFree/Resources/PlayerMotion/TierThree/Berserker.controller";
    static ActorDefinition actor;static ClassResourceProfile rage;static QuestDefinition quest;
    static AdvancementDefinition edge;static GameObject form;static ItemDefinition axe;static Texture2D[] icons;
    public static void Batch()
    {
        Directory.CreateDirectory(Data);Directory.CreateDirectory(Path.GetDirectoryName(Motion));AssetDatabase.Refresh();
        AuthorRig();AuthorData();AuthorForm();AuthorMenu();ExpandEquipment();
        PlayerScene("Cornberg",true);PlayerScene("DevelopmentRoom",false);
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");
        Debug.Log("DICEFREE_BERSERKER_AUTHOR_OK");
    }
    static T Load<T>(string path) where T:Object => AssetDatabase.LoadAssetAtPath<T>(path)??throw new InvalidOperationException("Missing "+path);
    static T Asset<T>(string name) where T:ScriptableObject
    {
        string path=Data+name+".asset";var value=AssetDatabase.LoadAssetAtPath<T>(path);
        if(value==null){value=ScriptableObject.CreateInstance<T>();value.name=name;AssetDatabase.CreateAsset(value,path);}return value;
    }
    static T Add<T>(Component root) where T:Component => root.GetComponent<T>()??root.gameObject.AddComponent<T>();
    static void AuthorData()
    {
        var parent=Load<ActorDefinition>(Parent+"Greatblade class.asset");
        var basic=Asset<AttackDefinition>("Berserker great weapon basic");EditorUtility.CopySerialized(parent.basicAttack,basic);
        basic.name="Berserker great weapon basic";basic.stableId="attack.berserker.great-weapon";basic.displayName="Great Weapon Strike";
        basic.baseDamage=0;basic.coefficient=1;basic.weights=new AttributeValues{strength=1};basic.minimumDamageMultiplier=basic.maximumDamageMultiplier=1;
        actor=Asset<ActorDefinition>("Berserker class");EditorUtility.CopySerialized(parent,actor);
        actor.name="Berserker class";actor.stableId=BerserkerProgression.Id;actor.displayName="Berserker";
        actor.baseAttributes=new AttributeValues{vitality=25,strength=34,agility=10,intelligence=5,spirit=7};
        actor.growth=new AttributeValues{vitality=3,strength=6,agility=1.2f,intelligence=.5f,spirit=.8f};
        actor.baseHp=10;actor.basicAttack=basic;actor.basicAttackMinimumOffset=actor.basicAttackMaximumOffset=0;
        actor.overrideVitalityHp=true;actor.hpPerVitality=15;actor.overrideVitalityRegeneration=true;actor.regenerationPerVitality=.1f;
        var resource=Asset<ResourceDefinition>("Rage");resource.stableId=BerserkerCaster.RageId;resource.displayName="Rage";
        rage=Asset<ClassResourceProfile>("Berserker Rage");rage.stableId="resource-profile.berserker.rage";rage.classId=actor.stableId;rage.resource=resource;
        rage.baseMaximum=100;rage.maximumPerIntelligence=rage.maximumPerSpirit=0;rage.baseRegenPerSecond=rage.regenPerIntelligence=rage.regenPerSpirit=0;
        rage.initialFraction=0;rage.loadPolicy=ResourceRetentionPolicy.ResetOnLoad;
        foreach(var name in new[]{"Wide Arc","Spin Dash","Rage Breaker"})
        {
            var hit=Asset<AttackDefinition>(name);hit.stableId="skill.berserker."+name.ToLowerInvariant().Replace(' ','-');hit.displayName=name;
            hit.channel=DamageChannel.Physical;hit.element=null;hit.baseDamage=0;hit.coefficient=1;hit.scaling=AttributeScaling.Weighted;
            hit.weights=new AttributeValues{strength=1};hit.requiresAccuracy=name!="Rage Breaker";EditorUtility.SetDirty(hit);
        }
        quest=Asset<QuestDefinition>("Trial of Fury");quest.stableId=BerserkerMentor.QuestId;quest.title=quest.name;
        quest.requiredClassId="class.greatblade";quest.requiredLevel=60;
        quest.offer="Rage is a tool. Let it choose your battles and you will soon have none left to fight. Enter the Crimson Crucible in Crimson Mire. Defeat its first two guardians, then bring down Bristlebreaker in the third cell. You may leave through the normal exit after that victory; your trial will be remembered. Return here when it is done.";
        quest.activeDialogue="Bristlebreaker guards the third cell of the Crimson Crucible. The slime and wolf must fall before its gate opens. You do not need the final treasury to prove your control.";
        quest.turnInDialogue="You faced the beast without becoming its echo. Take up your axe. Let anger serve your hand, never guide it.";
        quest.locationHint="Gorren Ashscar camps beside the north road from Trimery, near the Quillboar frontier.";
        quest.turnInHint="Return to Gorren Ashscar at his camp north of Trimery.";
        quest.stages=new[]{new QuestObjective{kind=ObjectiveKind.Kill,objectiveId="bristlebreaker",contentId=BerserkerMentor.BossId,dungeonBossOnly=true,count=1,
            instruction="Defeat Bristlebreaker in cell 3 of the Crimson Crucible; party participation counts"}};
        edge=Asset<AdvancementDefinition>("Berserker advancement");edge.stableId="advance.greatblade.berserker";edge.displayName="Berserker";
        edge.sourceClass=parent;edge.targetClass=actor;edge.requiredLevel=60;edge.requiredQuestId=quest.stableId;
        axe=Asset<ItemDefinition>("Ashscar Greataxe");axe.stableId="item.berserker.ashscar-greataxe";axe.displayName="Ashscar Greataxe";
        axe.slot=EquipmentSlot.MainHand;axe.handsRequired=2;axe.greatWeaponFamily=GreatWeaponFamily.TwoHandedAxe;axe.allowedClassIds=new[]{actor.stableId,"class.greatblade"};
        axe.itemLevel=1;axe.bound=true;axe.canSell=false;axe.sourceId="teacher.greatblade.berserker";
        axe.flavor="An axe carried through many battles. The edge is new. The lesson is older.";
        axe.stats=new EquipmentStats{basicAttackFlatBonus=3};
        icons=Enumerable.Range(0,5).Select(Icon).ToArray();
        foreach(var oldIcon in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(axe)).OfType<Sprite>().Where(s=>s!=axe.icon).ToArray())Object.DestroyImmediate(oldIcon,true);
        if(axe.icon==null){axe.icon=Sprite.Create(icons[0],new Rect(0,0,96,96),new Vector2(.5f,.5f),96);AssetDatabase.AddObjectToAsset(axe.icon,axe);axe.icon.name="Ashscar Greataxe icon";}
        foreach(string path in new[]{"Assets/_DiceFree/Settings/Progression/Class catalog.asset","Assets/_DiceFree/Settings/Development/Priest/Development Priest catalog.asset"})
        {var catalog=Load<ClassCatalog>(path);catalog.Configure(catalog.Classes.Where(c=>c.stableId!=actor.stableId).Append(actor).ToArray());EditorUtility.SetDirty(catalog);}
        foreach(var value in new Object[]{actor,basic,resource,rage,quest,edge,axe})EditorUtility.SetDirty(value);
    }
    static void AuthorRig()
    {
        string path=Art+"AshscarOrc.fbx";
        var model=Load<GameObject>(path);var importer=(ModelImporter)AssetImporter.GetAtPath(path);
        var map=new Dictionary<string,string>{
            ["Hips"]="Hips",["Spine"]="Waist",["Chest"]="Mid",["UpperChest"]="Chest",["Neck"]="Neck",["Head"]="Head",
            ["LeftShoulder"]="Lclavicle",["LeftUpperArm"]="Lshoulder",["LeftLowerArm"]="Lelbow",["LeftHand"]="Lwrist",
            ["RightShoulder"]="Rclavicle",["RightUpperArm"]="Rshoulder",["RightLowerArm"]="Relbow",["RightHand"]="Rwrist",
            ["LeftUpperLeg"]="LThigh",["LeftLowerLeg"]="LKnee",["LeftFoot"]="LAnkle",["LeftToes"]="Lfoot",
            ["RightUpperLeg"]="Rthigh",["RightLowerLeg"]="Rknee",["RightFoot"]="Rankle",["RightToes"]="Rfoot"
        };
        foreach(var side in new[]{"Left","Right"})foreach(var finger in new[]{"Thumb","Index","Middle","Ring","Little"})for(int i=1;i<=3;i++)
            map[side+finger+(i==1?"Proximal":i==2?"Intermediate":"Distal")]=(side=="Left"?"L":"R")+(finger=="Little"?"pinky":finger.ToLowerInvariant())+i;
        var description=new HumanDescription{
            human=map.Select(p=>new HumanBone{humanName=p.Key,boneName=p.Value,limit=new HumanLimit{useDefaultValues=true}}).ToArray(),
            skeleton=model.GetComponentsInChildren<Transform>(true).Select(t=>new SkeletonBone{name=t.name,position=t.localPosition,rotation=t.localRotation,scale=t.localScale}).ToArray(),
            upperArmTwist=.5f,lowerArmTwist=.5f,upperLegTwist=.5f,lowerLegTwist=.5f,armStretch=.05f,legStretch=.05f,feetSpacing=0,hasTranslationDoF=false
        };
        importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
        importer.humanDescription=description;importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;
        importer.importNormals=ModelImporterNormals.Import;importer.importTangents=ModelImporterTangents.CalculateMikk;importer.preserveHierarchy=true;
        foreach(var pair in new[]{("Orc_Armour","Orc_Armour.jpg"),("Orc_Body","Orc_Body.jpg"),("Orc_Head","Orc_Head.jpg")})
        {
            var material=Paint(pair.Item1,Color.white);material.SetTexture("_BaseMap",Load<Texture2D>(Art+pair.Item2));material.SetFloat("_Smoothness",.18f);
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),pair.Item1),material);
        }
        importer.SaveAndReimport();model=Load<GameObject>(path);
        var avatar=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().Single();
        if(!avatar.isValid||!avatar.isHuman)throw new InvalidOperationException("Orc humanoid avatar is invalid.");
        var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(Motion)??AnimatorController.CreateAnimatorControllerAtPath(Motion);
        var stateMachine=controller.layers[0].stateMachine;
        var native=Load<RuntimeAnimatorController>("Assets/_DiceFree/Resources/PlayerMotion/TierTwo/Greatblade.controller");
        AnimationClip Native(string name)=>native.animationClips.FirstOrDefault(c=>c.name==name)??native.animationClips[0];
        AnimationClip Clip(string file,string fallback)=>AssetDatabase.LoadAllAssetsAtPath("Assets/Blink/Art/Animations/"+file+".fbx").OfType<AnimationClip>().FirstOrDefault(c=>!c.name.StartsWith("__")&&!c.name.Equals("tpose",StringComparison.OrdinalIgnoreCase))??Native(fallback);
        var states=new Dictionary<string,AnimationClip>{
            ["Idle"]=Clip("Combat/IdleCombat","Physical Idle"),["Locomotion"]=Clip("Movement/WalkForward","Physical Locomotion"),
            ["Attack"]=Clip("Combat/MeeleeAttack_TwoHanded","Physical Attack"),["ArmedAttack"]=Clip("Combat/MeeleeAttack_TwoHanded","Physical ArmedAttack"),
            ["Heavy"]=Clip("Combat/MeeleeAttack_TwoHanded","Physical Attack"),["Spin Dash"]=Clip("Combat/SpinAttack_TwoHanded","Physical Attack"),
            ["Guard"]=Clip("Combat/Buff","Physical Idle"),["Hit"]=Clip("Combat/TakeDamage","Physical Hit"),["Death"]=Clip("Combat/Death","Physical Death")
        };
        foreach(var pair in states){var state=stateMachine.states.FirstOrDefault(s=>s.state.name==pair.Key).state??stateMachine.AddState(pair.Key);state.motion=pair.Value;if(pair.Key=="Idle")stateMachine.defaultState=state;}
        var layers=controller.layers;layers[0].iKPass=true;controller.layers=layers;EditorUtility.SetDirty(controller);
        File.WriteAllText("Logs/BerserkerRig.txt",$"Humanoid valid={avatar.isValid}; human={avatar.isHuman}; model={path}; clips="+string.Join(",",states.Select(p=>p.Key+"="+p.Value.name)));
    }
    static void AuthorForm()
    {
        var root=new GameObject("Ashscar Berserker");
        try
        {
            var model=(GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(Art+"AshscarOrc.fbx"),root.transform);
            var animator=model.GetComponent<Animator>()??model.AddComponent<Animator>();animator.runtimeAnimatorController=Load<RuntimeAnimatorController>(Motion);
            animator.avatar=AssetDatabase.LoadAllAssetsAtPath(Art+"AshscarOrc.fbx").OfType<Avatar>().Single();
            animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=true;
            var shaft=new GameObject("Two-handed weapon grip").transform;shaft.SetParent(model.transform,false);
            var axeRoot=Group(shaft,"Greataxe",Vector3.zero);var swordRoot=Group(shaft,"Greatsword",Vector3.zero);
            var wood=Paint("Ashscar haft",new(.20f,.105f,.045f));var steel=Paint("Ashscar steel",new(.38f,.46f,.49f));var edgeMat=Paint("Ashscar edge",new(.75f,.81f,.78f));var leather=Paint("Ashscar red leather",new(.38f,.075f,.045f));var brass=Paint("Ashscar bronze",new(.56f,.34f,.12f));
            Part(axeRoot,"Long ash haft",new(0,.53f,0),new(.065f,1.55f,.065f),wood,PrimitiveType.Cylinder);
            Part(axeRoot,"Wrapped lower grip",new(0,.13f,0),new(.082f,.38f,.082f),leather,PrimitiveType.Cylinder);
            Part(axeRoot,"Bronze pommel",new(0,-.21f,0),new(.12f,.07f,.12f),brass,PrimitiveType.Sphere);
            for(int side=-1;side<=1;side+=2)Blade(axeRoot,side,steel,edgeMat);
            Part(axeRoot,"Central axe socket",new(0,.94f,0),new(.15f,.38f,.16f),brass);
            Part(swordRoot,"Leather sword grip",new(0,.14f,0),new(.065f,.40f,.065f),leather,PrimitiveType.Cylinder);
            Part(swordRoot,"Greatsword guard",new(0,.42f,0),new(.52f,.075f,.085f),brass);
            Part(swordRoot,"Steel blade",new(0,.99f,0),new(.15f,1.10f,.045f),edgeMat);
            swordRoot.gameObject.SetActive(false);
            model.AddComponent<BerserkerWeaponPresentation>().Configure(shaft,axeRoot.gameObject,swordRoot.gameObject);
            foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=2;
            form=PrefabUtility.SaveAsPrefabAsset(root,Data+"Ashscar Berserker.prefab");
        }finally{Object.DestroyImmediate(root);}
    }
    static void AuthorMenu()
    {
        var menu=Load<MenuPresentationCatalog>("Assets/_DiceFree/Resources/MenuPresentationCatalog.asset");var root=new GameObject("Berserker menu presentation");
        try{
            root.AddComponent<ActorStats>().Configure(actor);root.AddComponent<CarriedInventory>().Configure(menu.items.Append(axe).Distinct().ToArray());root.AddComponent<Equipment>();
            PrefabUtility.InstantiatePrefab(form,root.transform);var preview=PrefabUtility.SaveAsPrefabAsset(root,Data+"Berserker menu.prefab");
            menu.forms=menu.forms.Where(f=>f.classId!=actor.stableId).Append(new MenuPresentationCatalog.Form{classId=actor.stableId,tier=3,preview=preview}).ToArray();
            menu.items=menu.items.Where(i=>i!=axe).Append(axe).ToArray();EditorUtility.SetDirty(menu);
        }finally{Object.DestroyImmediate(root);}
    }
    static void ExpandEquipment()
    {
        foreach(var guid in AssetDatabase.FindAssets("t:ItemDefinition",new[]{"Assets/_DiceFree/Settings","Assets/_DiceFree/Resources/DiceFree"}))
        {
            var item=Load<ItemDefinition>(AssetDatabase.GUIDToAssetPath(guid));if(item==axe)continue;
            bool sword=item.slot==EquipmentSlot.MainHand&&item.handsRequired==2&&
                ((item.displayName??"").IndexOf("sword",StringComparison.OrdinalIgnoreCase)>=0||(item.stableId??"").IndexOf("sword",StringComparison.OrdinalIgnoreCase)>=0);
            if(sword){item.greatWeaponFamily=GreatWeaponFamily.TwoHandedSword;EditorUtility.SetDirty(item);}
            bool parentItem=item.allowedClassIds?.Contains("class.greatblade")==true;
            if(!parentItem)continue;
            if(item.slot==EquipmentSlot.MainHand)
            {
                if(item.handsRequired!=2)continue;
                // Existing Greatblade mainhands are authored greatswords.
                item.greatWeaponFamily=GreatWeaponFamily.TwoHandedSword;
            }
            if(item.slot==EquipmentSlot.OffHand)continue;
            item.allowedClassIds=item.allowedClassIds.Append(actor.stableId).Distinct().ToArray();EditorUtility.SetDirty(item);
        }
    }
    static void PlayerScene(string name,bool camp)
    {
        var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/"+name+".unity");var controller=Object.FindObjectsByType<AdvancementController>(FindObjectsInactive.Include).Single();var player=controller.GetComponent<CombatActor>();
        var catalog=Load<ClassCatalog>(name=="Cornberg"?"Assets/_DiceFree/Settings/Progression/Class catalog.asset":"Assets/_DiceFree/Settings/Development/Priest/Development Priest catalog.asset");
        controller.Configure(catalog,controller.Definitions.Where(e=>e.stableId!=edge.stableId).Append(edge).ToArray());player.GetComponent<ManifestationPersistence>().ConfigureClassCatalog(catalog);
        Add<BerserkerProgression>(player);Add<BerserkerCaster>(player).Configure(Load<AttackDefinition>(Data+"Wide Arc.asset"),Load<AttackDefinition>(Data+"Spin Dash.asset"),Load<AttackDefinition>(Data+"Rage Breaker.asset"));Add<BerserkerAbilityProvider>(player).Configure(icons);
        var resources=player.GetComponent<ActorResourceController>();resources.Configure(resources.Profiles.Where(p=>p.classId!=actor.stableId).Append(rage).ToArray());
        var journal=player.GetComponent<QuestJournal>();journal.Configure(journal.Definitions.Where(q=>q.stableId!=quest.stableId).Append(quest).ToArray());
        var inventory=player.GetComponent<CarriedInventory>();inventory.Configure(inventory.Definitions.Where(i=>i!=axe).Append(axe).ToArray());
        var switcher=new SerializedObject(player.GetComponent<ClassPresentationSwitcher>());var bindings=switcher.FindProperty("bindings");
        int index=Enumerable.Range(0,bindings.arraySize).Where(i=>bindings.GetArrayElementAtIndex(i).FindPropertyRelative("classId").stringValue==actor.stableId).DefaultIfEmpty(-1).First();
        if(index<0){index=bindings.arraySize;bindings.InsertArrayElementAtIndex(index);}else if(bindings.GetArrayElementAtIndex(index).FindPropertyRelative("visualRoot").objectReferenceValue is Transform old)Object.DestroyImmediate(old.gameObject);
        var visual=(GameObject)PrefabUtility.InstantiatePrefab(form,player.transform);visual.SetActive(false);var binding=bindings.GetArrayElementAtIndex(index);
        binding.FindPropertyRelative("classId").stringValue=actor.stableId;binding.FindPropertyRelative("visualRoot").objectReferenceValue=visual.transform;binding.FindPropertyRelative("animator").objectReferenceValue=visual.GetComponentInChildren<Animator>(true);switcher.ApplyModifiedPropertiesWithoutUndo();
        if(camp)
        {
            var nav=Object.FindObjectsByType<WorldNavigation>(FindObjectsInactive.Include).Single(n=>n.gameObject.scene==scene);var instance=NavMesh.AddNavMeshData(nav.Data);
            try{Physics.SyncTransforms();Camp();}finally{instance.Remove();}
        }
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    static Vector3 Open(Vector3 wanted)
    {
        for(int i=0;i<100;i++)
        {
            var p=wanted+new Vector3(Mathf.Cos(i*2.4f),0,Mathf.Sin(i*2.4f))*i*.1f;
            if(!Physics.Raycast(p+Vector3.up*80,Vector3.down,out var floor,160,1<<8)||Physics.CheckSphere(floor.point+Vector3.up,.8f,1<<9,QueryTriggerInteraction.Ignore))continue;
            if(NavMesh.SamplePosition(floor.point,out var hit,.8f,1))return hit.position;
        }
        throw new InvalidOperationException("No reachable camp position near "+wanted);
    }
    static void Camp()
    {
        var old=GameObject.Find("Gorren Ashscar - Berserker camp");if(old!=null)Object.DestroyImmediate(old);
        var camp=new GameObject("Gorren Ashscar - Berserker camp");camp.transform.position=Open(new Vector3(245,0,238));
        var roads=Object.FindObjectsByType<TravelSurface>(FindObjectsInactive.Exclude);
        for(int i=0;i<25;i++)
        {
            var point=Open(new Vector3(245-i*.7f,0,238));
            if(roads.Any(road=>road.Contains(point)||road.Contains(point+Vector3.right*3.2f)||road.Contains(point+Vector3.back*3.2f)))continue;
            camp.transform.position=point;break;
        }
        var npc=Group(camp.transform,"Gorren Ashscar",Vector3.zero).gameObject;npc.layer=11;
        var body=(GameObject)PrefabUtility.InstantiatePrefab(form,npc.transform);body.transform.rotation=Quaternion.Euler(0,200,0);
        foreach(var renderer in body.GetComponentsInChildren<SkinnedMeshRenderer>(true))renderer.sharedMaterials=renderer.sharedMaterials.Select(original=>{
            string path=Data+"Mentor "+original.name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(original){name="Mentor "+original.name};AssetDatabase.CreateAsset(material,path);}
            material.color=original.name.Contains("Body")?new(.76f,.81f,.78f):original.name.Contains("Head")?new(.86f,.87f,.82f):new(.68f,.72f,.70f);EditorUtility.SetDirty(material);return material;
        }).ToArray();
        var capsule=npc.AddComponent<CapsuleCollider>();capsule.center=Vector3.up;capsule.height=2.1f;capsule.radius=.45f;
        var approach=Group(npc.transform,"Gorren approach",Vector3.zero);approach.position=Open(npc.transform.position+Vector3.back*1.9f);
        var mentor=npc.AddComponent<BerserkerMentor>();mentor.Configure(quest,edge,new[]{axe});mentor.ConfigureApproach(approach);
        var stone=Paint("Camp charcoal stones",new(.20f,.19f,.16f));var ember=Paint("Banked camp embers",new(.56f,.15f,.025f));var cloth=Paint("Gorren red canvas",new(.32f,.09f,.055f));
        for(int i=0;i<10;i++){float a=i*Mathf.PI/5;Part(camp.transform,"Hearth stone",new Vector3(2+Mathf.Cos(a)*.6f,.12f,1+Mathf.Sin(a)*.6f),new(.30f,.25f,.26f),stone,PrimitiveType.Sphere);}
        Part(camp.transform,"Banked embers",new(2,.04f,1),new(.82f,.08f,.82f),ember,PrimitiveType.Sphere);
        Part(camp.transform,"Canvas bedroll",new(-1.8f,.12f,1.7f),new(.75f,.24f,1.8f),cloth);
        Part(camp.transform,"Travel chest",new(-1.4f,.28f,-1.4f),new(.9f,.55f,.55f),Paint("Camp worn wood",new(.23f,.14f,.07f)));
        var light=Group(camp.transform,"Warm banked-fire glow",new(2,.5f,1)).gameObject.AddComponent<Light>();light.type=LightType.Point;light.color=new(1,.40f,.08f);light.range=5;light.intensity=1.1f;
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/BerserkerPlacement.txt",$"Gorren Ashscar north of Trimery: {npc.transform.position}; approach {approach.position}");
    }
    static Transform Group(Transform parent,string name,Vector3 position){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;}
    static void Part(Transform parent,string name,Vector3 point,Vector3 size,Material material,PrimitiveType type=PrimitiveType.Cube)
    {var go=GameObject.CreatePrimitive(type);go.name=name;Object.DestroyImmediate(go.GetComponent<Collider>());go.transform.SetParent(parent,false);go.transform.localPosition=point;if(type==PrimitiveType.Cylinder)size.y*=.5f;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;go.layer=2;}
    static Material Paint(string name,Color tint)
    {string path=Data+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};AssetDatabase.CreateAsset(material,path);}material.color=tint;EditorUtility.SetDirty(material);return material;}
    static void Blade(Transform parent,int side,Material steel,Material edge)
    {
        var outline=new[]{new Vector2(.07f,.75f),new Vector2(.30f,.64f),new Vector2(.59f,.73f),new Vector2(.64f,1.12f),new Vector2(.37f,1.23f),new Vector2(.07f,1.09f)};
        var vertices=outline.Select(p=>new Vector3(p.x*side,p.y,-.045f)).Concat(outline.Select(p=>new Vector3(p.x*side,p.y,.045f))).ToArray();
        var triangles=new List<int>();for(int i=1;i<5;i++){triangles.AddRange(new[]{0,i+1,i,6,6+i,6+i+1});}
        for(int i=0;i<6;i++){int j=(i+1)%6;triangles.AddRange(new[]{i,j,j+6,i,j+6,i+6});}
        if(side<0)for(int i=0;i<triangles.Count;i+=3){int t=triangles[i];triangles[i]=triangles[i+1];triangles[i+1]=t;}
        var mesh=new Mesh{name="Ashscar axe blade "+side};mesh.vertices=vertices;mesh.triangles=triangles.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();
        string path=Data+"Ashscar blade "+side+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);mesh=old;}
        var go=Group(parent,"Swept steel axe blade",Vector3.zero).gameObject;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=steel;
        Part(parent,"Polished cutting edge",new(side*.60f,.93f,0),new(.055f,.37f,.12f),edge);
    }
    static Texture2D Icon(int i)
    {
        string path=Data+BerserkerProgression.Name(i)+" icon.asset";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if(texture==null){texture=new Texture2D(96,96,TextureFormat.RGBA32,false);AssetDatabase.CreateAsset(texture,path);}var pixels=new Color[96*96];
        for(int y=0;y<96;y++)for(int x=0;x<96;x++)
        {
            var p=new Vector2((x-47.5f)/48,(y-47.5f)/48);float line=Mathf.Abs(p.x-p.y*.35f);
            float blade=Mathf.Max(Mathf.Abs(p.x)-.5f,Mathf.Abs(p.y-.25f)-.18f);
            float shape=i==1?Mathf.Abs(p.magnitude-.53f):i==3?Mathf.Abs(p.magnitude-.42f):i==4?Mathf.Abs(p.magnitude-.56f):Mathf.Min(line,Mathf.Abs(blade));
            float ink=1-Mathf.SmoothStep(.025f,.085f,shape);pixels[y*96+x]=Color.Lerp(new(.075f,.026f,.018f),i==4?new(.73f,.46f,.18f):i==3?new(.96f,.28f,.08f):new(.90f,.72f,.42f),ink);
        }
        texture.SetPixels(pixels);texture.Apply();EditorUtility.SetDirty(texture);return texture;
    }
}
