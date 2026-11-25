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
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

public static class MarksmanAuthoring
{
    public const string Data="Assets/_DiceFree/Settings/Marksman/";
    const string Parent="Assets/_DiceFree/Settings/Ranger/";
    static ActorDefinition actor,targetDefinition;static ClassResourceProfile mana;static QuestDefinition quest;static AdvancementDefinition edge;static GameObject form;static Texture2D[] icons;static ItemDefinition bow;
    static readonly string[] Settlements={"Cornberg","Trimery","Arborium","Cinderwatch","Willowford","Anvilreach"};
    static readonly List<string> placed=new();
    public static void Batch()
    {
        Directory.CreateDirectory(Data);AssetDatabase.Refresh();placed.Clear();
        AuthorData();AuthorForm();AuthorMenu();MigrateManaAndCompatibility();
        PlayerScene("Cornberg",true);PlayerScene("DevelopmentRoom",false);WorldTwo();
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");
        Directory.CreateDirectory("Logs");File.WriteAllLines("Logs/MarksmanPlacements.txt",placed);Debug.Log("DICEFREE_MARKSMAN_AUTHOR_OK");
    }
    static T Load<T>(string path) where T:Object=>AssetDatabase.LoadAssetAtPath<T>(path)??throw new InvalidOperationException("Missing "+path);
    static T Asset<T>(string name) where T:ScriptableObject
    {string path=Data+name+".asset";var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old!=null)return old;var value=ScriptableObject.CreateInstance<T>();value.name=name;AssetDatabase.CreateAsset(value,path);return value;}
    static T Add<T>(Component root) where T:Component=>root.GetComponent<T>()??root.gameObject.AddComponent<T>();
    static void AuthorData()
    {
        var parent=Load<ActorDefinition>(Parent+"Ranger class.asset");var basic=Asset<AttackDefinition>("Marksman bow basic");EditorUtility.CopySerialized(parent.basicAttack,basic);basic.name="Marksman bow basic";basic.stableId="attack.marksman.bow";basic.displayName="Marksman's Arrow";basic.baseDamage=8;basic.coefficient=1;basic.weights=new AttributeValues{agility=1};basic.reach=11;
        actor=Asset<ActorDefinition>("Marksman class");EditorUtility.CopySerialized(parent,actor);actor.name="Marksman class";actor.stableId=MarksmanProgression.Id;actor.displayName="Marksman";actor.baseAttributes=new AttributeValues{vitality=19,strength=10,agility=34,intelligence=9,spirit=11};actor.growth=new AttributeValues{vitality=2,strength=.9f,agility=6,intelligence=1,spirit=1.2f};actor.baseHp=20;actor.basicAttack=basic;actor.moveSpeed=5;actor.overrideVitalityHp=true;actor.hpPerVitality=15;actor.overrideVitalityRegeneration=true;actor.regenerationPerVitality=.1f;
        mana=Asset<ClassResourceProfile>("Marksman Mana");EditorUtility.CopySerialized(Load<ClassResourceProfile>(Parent+"Ranger Mana.asset"),mana);mana.name="Marksman Mana";mana.classId=actor.stableId;mana.stableId="resource-profile.marksman.mana";mana.baseMaximum=180;
        quest=Asset<QuestDefinition>("The Farwatch Pilgrimage");quest.stableId=MarksmanArcheryTarget.QuestId;quest.title=quest.name;quest.requiredClassId="class.ranger";quest.requiredLevel=60;quest.offer="The whole world is your shooting range. I have placed one target in each town and city: Cornberg, Trimery, Arborium, Cinderwatch, Willowford and Anvilreach. Visit both worlds and strike each target with an ordinary bow shot. Your journal will keep the list. Return to me when all six are struck.";quest.activeDialogue="Six settlements, two worlds. Check your journal for the places you still need. The targets stand in the open near the roads; this is a journey, not a hunt for a hidden speck.";quest.turnInDialogue="You have learned to see the world beyond a single hunting ground. Take the path of the Marksman.";quest.locationHint="Aelwen waits at the woodland edge south of Willowford in Steamroot Vale.";quest.turnInHint="Return to Aelwen, the Farwatch, south of Willowford.";
        quest.stages=new[]{new QuestObjective{kind=ObjectiveKind.All,objectiveId="settlement-tour",count=6,instruction="Shoot one archery target in every town or city across both worlds",alternatives=Settlements.Select(s=>new QuestLeaf{kind=ObjectiveKind.Interact,objectiveId=s.ToLowerInvariant(),contentId="target.marksman."+s.ToLowerInvariant(),count=1,instruction=s}).ToArray()}};
        edge=Asset<AdvancementDefinition>("Marksman advancement");edge.stableId="advance.ranger.marksman";edge.displayName="Marksman";edge.sourceClass=parent;edge.targetClass=actor;edge.requiredLevel=60;edge.requiredQuestId=quest.stableId;
        targetDefinition=Asset<ActorDefinition>("Archery target");EditorUtility.CopySerialized(parent,targetDefinition);targetDefinition.name="Archery target";targetDefinition.stableId="quest-target.marksman";targetDefinition.displayName="Archery Target";targetDefinition.familyId="quest-target";targetDefinition.baseAttributes=default;targetDefinition.growth=default;targetDefinition.baseHp=1000;targetDefinition.basicAttack=null;targetDefinition.experienceReward=0;targetDefinition.overrideVitalityRegeneration=true;targetDefinition.regenerationPerVitality=0;
        foreach(var name in new[]{"Aimed Shot","Arrow Rain","Power Shot"}){var hit=Asset<AttackDefinition>(name);hit.stableId="skill.marksman."+name.ToLowerInvariant().Replace(' ','-');hit.displayName=name;hit.baseDamage=0;hit.weights=default;hit.coefficient=0;hit.channel=DamageChannel.Physical;hit.requiresAccuracy=false;EditorUtility.SetDirty(hit);}
        icons=Enumerable.Range(0,6).Select(Icon).ToArray();bow=Load<ItemDefinition>(Parent+"Ironpine Yew Longbow.asset");
        foreach(string path in new[]{"Assets/_DiceFree/Settings/Progression/Class catalog.asset","Assets/_DiceFree/Settings/Development/Priest/Development Priest catalog.asset"}){var catalog=Load<ClassCatalog>(path);catalog.Configure(catalog.Classes.Where(c=>c.stableId!=actor.stableId).Append(actor).ToArray());EditorUtility.SetDirty(catalog);}
        var ranger=Load<AttackDefinition>(Parent+"Ranger bow basic.asset");ranger.weights.agility=1;EditorUtility.SetDirty(ranger);
        foreach(var value in new Object[]{actor,basic,mana,quest,edge,targetDefinition})EditorUtility.SetDirty(value);
    }
    static void MigrateManaAndCompatibility()
    {
        var audit=new List<string>();foreach(string guid in AssetDatabase.FindAssets("t:ClassResourceProfile",new[]{"Assets/_DiceFree"})){
            string path=AssetDatabase.GUIDToAssetPath(guid);if(path.Contains("/Recovery/"))continue;var p=Load<ClassResourceProfile>(path);if(p.resource?.stableId!=ResourceIds.Mana)continue;
            audit.Add(path+" | class="+p.classId+" | base="+p.baseMaximum+" | old max="+p.maximumPerIntelligence+"INT + "+p.maximumPerSpirit+"SPR | old regen="+p.baseRegenPerSecond+" + "+p.regenPerIntelligence+"INT + "+p.regenPerSpirit+"SPR");
            p.maximumPerIntelligence=5;p.maximumPerSpirit=0;p.baseRegenPerSecond=4;p.regenPerIntelligence=0;p.regenPerSpirit=.1f;EditorUtility.SetDirty(p);
        }
        foreach(string guid in AssetDatabase.FindAssets("t:ItemDefinition",new[]{"Assets/_DiceFree"})){
            var item=Load<ItemDefinition>(AssetDatabase.GUIDToAssetPath(guid));if(item.allowedClassIds?.Contains("class.ranger")==true&&!item.allowedClassIds.Contains(MarksmanProgression.Id)){item.allowedClassIds=item.allowedClassIds.Append(MarksmanProgression.Id).ToArray();EditorUtility.SetDirty(item);}
        }
        Directory.CreateDirectory("Logs");File.WriteAllLines("Logs/MarksmanManaBefore.txt",audit);
    }
    static void AuthorForm()
    {
        var root=new GameObject("Farwatch Marksman");try{
            var source=Load<GameObject>("Assets/_DiceFree/Art/Characters/IronpineRanger/IronpineRanger.fbx");var body=(GameObject)PrefabUtility.InstantiatePrefab(source,root.transform);
            var animator=body.GetComponent<Animator>();if(animator.avatar==null||!animator.avatar.isValid)throw new InvalidOperationException("Wood Elf rig invalid.");animator.runtimeAnimatorController=Load<RuntimeAnimatorController>("Assets/_DiceFree/Resources/PlayerMotion/TierTwo/Ranger.controller");animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            var maps=new Dictionary<Material,Material>();foreach(var renderer in body.GetComponentsInChildren<Renderer>(true))renderer.sharedMaterials=renderer.sharedMaterials.Select(original=>{
                if(maps.TryGetValue(original,out var old))return old;string path=Data+"Farwatch "+original.name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(original);AssetDatabase.CreateAsset(mat,path);}var key=original.name.ToLowerInvariant();
                if(key.Contains("overcoat"))mat.color=new(.78f,.75f,.57f);else if(key.Contains("quilted"))mat.color=new(.065f,.21f,.16f);else if(key.Contains("lining"))mat.color=new(.15f,.29f,.22f);else if(key.Contains("enamel"))mat.color=new(.08f,.30f,.20f);else if(key.Contains("hair"))mat.color=new(.74f,.72f,.56f);else if(key.Contains("brass")||key.Contains("copper"))mat.color=new(.64f,.43f,.16f);EditorUtility.SetDirty(mat);maps[original]=mat;return mat;
            }).ToArray();
            var skins=body.GetComponentsInChildren<SkinnedMeshRenderer>(true);foreach(var skin in skins)skin.updateWhenOffscreen=true;
            root.AddComponent<RangerBowPresentation>().Configure(skins.Single(s=>s.name.Contains("Shortbow")).gameObject,skins.Single(s=>s.name.Contains("Longbow")).gameObject);
            form=PrefabUtility.SaveAsPrefabAsset(root,Data+"Farwatch Marksman.prefab");
        }finally{Object.DestroyImmediate(root);}
    }
    static void AuthorMenu()
    {
        var menu=Load<MenuPresentationCatalog>("Assets/_DiceFree/Resources/MenuPresentationCatalog.asset");var root=new GameObject("Marksman menu presentation");try{
            root.AddComponent<ActorStats>().Configure(actor);root.AddComponent<CarriedInventory>().Configure(menu.items);root.AddComponent<Equipment>();var visual=(GameObject)PrefabUtility.InstantiatePrefab(form,root.transform);var preview=PrefabUtility.SaveAsPrefabAsset(root,Data+"Marksman menu.prefab");menu.forms=menu.forms.Where(f=>f.classId!=actor.stableId).Append(new MenuPresentationCatalog.Form{classId=actor.stableId,tier=3,preview=preview}).ToArray();EditorUtility.SetDirty(menu);
        }finally{Object.DestroyImmediate(root);}
    }
    static void PlayerScene(string name,bool targets)
    {
        var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/"+name+".unity");var controller=Object.FindObjectsByType<AdvancementController>(FindObjectsInactive.Include).Single();var player=controller.GetComponent<CombatActor>();var catalog=Load<ClassCatalog>(name=="Cornberg"?"Assets/_DiceFree/Settings/Progression/Class catalog.asset":"Assets/_DiceFree/Settings/Development/Priest/Development Priest catalog.asset");
        controller.Configure(catalog,controller.Definitions.Where(e=>e.stableId!=edge.stableId).Append(edge).ToArray());player.GetComponent<ManifestationPersistence>().ConfigureClassCatalog(catalog);Add<MarksmanProgression>(player);Add<MarksmanCaster>(player).Configure(Load<AttackDefinition>(Data+"Aimed Shot.asset"),Load<AttackDefinition>(Data+"Arrow Rain.asset"),Load<AttackDefinition>(Data+"Power Shot.asset"));Add<MarksmanAbilityProvider>(player).Configure(icons);
        var resources=player.GetComponent<ActorResourceController>();resources.Configure(resources.Profiles.Where(p=>p.classId!=actor.stableId).Append(mana).ToArray());var journal=player.GetComponent<QuestJournal>();journal.Configure(journal.Definitions.Where(q=>q.stableId!=quest.stableId).Append(quest).ToArray());
        var switcher=new SerializedObject(player.GetComponent<ClassPresentationSwitcher>());var bindings=switcher.FindProperty("bindings");int index=Enumerable.Range(0,bindings.arraySize).Where(i=>bindings.GetArrayElementAtIndex(i).FindPropertyRelative("classId").stringValue==actor.stableId).DefaultIfEmpty(-1).First();if(index<0){index=bindings.arraySize;bindings.InsertArrayElementAtIndex(index);}else if(bindings.GetArrayElementAtIndex(index).FindPropertyRelative("visualRoot").objectReferenceValue is Transform old)Object.DestroyImmediate(old.gameObject);
        var model=(GameObject)PrefabUtility.InstantiatePrefab(form,player.transform);model.SetActive(false);var binding=bindings.GetArrayElementAtIndex(index);binding.FindPropertyRelative("classId").stringValue=actor.stableId;binding.FindPropertyRelative("visualRoot").objectReferenceValue=model.transform;binding.FindPropertyRelative("animator").objectReferenceValue=model.GetComponentInChildren<Animator>(true);switcher.ApplyModifiedPropertiesWithoutUndo();
        if(targets){var nav=Object.FindObjectsByType<WorldNavigation>(FindObjectsInactive.Include).Single(n=>n.gameObject.scene==scene);var instance=NavMesh.AddNavMeshData(nav.Data);try{Physics.SyncTransforms();Target("Cornberg",new(13,0,15));Target("Trimery",new(248,0,172));Target("Arborium",new(101,0,321));}finally{instance.Remove();}}
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    static Vector3 Open(Vector3 wanted)
    {
        for(int i=0;i<80;i++){var p=wanted+new Vector3(Mathf.Cos(i*2.4f),0,Mathf.Sin(i*2.4f))*i*.12f;if(!Physics.Raycast(p+Vector3.up*60,Vector3.down,out var floor,150,1<<8)||Physics.CheckSphere(floor.point+Vector3.up*.9f,.85f,1<<9,QueryTriggerInteraction.Ignore))continue;if(NavMesh.SamplePosition(floor.point,out var hit,.7f,1))return hit.position;}
        throw new InvalidOperationException("No open position near "+wanted);
    }
    static void WorldTwo()
    {
        var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/WorldTwoArrival.unity");var nav=Object.FindObjectsByType<WorldNavigation>(FindObjectsInactive.Include).Single(n=>n.gameObject.scene==scene);var instance=NavMesh.AddNavMeshData(nav.Data);try{
            Physics.SyncTransforms();Vector3 P(float x,float z)=>AshfallReach.Origin+new Vector3(x,0,z);Target("Cinderwatch",P(13,-40));Target("Willowford",P(-145,-65));Target("Anvilreach",P(84,-211));
            var existing=GameObject.Find("Aelwen, the Farwatch");if(existing!=null)Object.DestroyImmediate(existing);var root=new GameObject("Aelwen, the Farwatch"){layer=11};root.transform.position=Open(P(-130,-92));var model=(GameObject)PrefabUtility.InstantiatePrefab(form,root.transform);model.transform.rotation=Quaternion.Euler(0,35,0);var pose=model.GetComponent<RangerBowPresentation>();Object.DestroyImmediate(pose);foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>(true)){if(skin.name.Contains("Shortbow"))skin.gameObject.SetActive(false);if(skin.name.Contains("Longbow"))skin.gameObject.SetActive(true);}
            var capsule=root.AddComponent<CapsuleCollider>();capsule.center=Vector3.up*.9f;capsule.height=1.8f;capsule.radius=.4f;var approach=new GameObject("Aelwen approach").transform;approach.SetParent(root.transform,false);approach.position=Open(root.transform.position+Vector3.back*1.8f);var teacher=root.AddComponent<MarksmanMentor>();teacher.Configure(quest,edge,new[]{bow});teacher.ConfigureApproach(approach);placed.Add("Teacher Aelwen | "+root.transform.position);
        }finally{instance.Remove();}EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    static Material Paint(string name,Color tint)
    {string path=Data+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};AssetDatabase.CreateAsset(m,path);}m.color=tint;EditorUtility.SetDirty(m);return m;}
    static void Target(string settlement,Vector3 wanted)
    {
        var old=GameObject.Find("Marksman target - "+settlement);if(old!=null)Object.DestroyImmediate(old);var root=new GameObject("Marksman target - "+settlement){layer=11};root.transform.position=Open(wanted);root.SetActive(false);
        root.AddComponent<ActorStats>().Configure(targetDefinition,1);root.AddComponent<NavMeshAgent>();root.AddComponent<TraversalMotor>().enabled=false;root.AddComponent<Health>();root.AddComponent<CombatActor>().Configure(1,.45f);root.GetComponent<CombatActor>().enabled=false;
        var collider=root.AddComponent<CapsuleCollider>();collider.height=2;collider.radius=.65f;collider.center=Vector3.up;collider.enabled=false;
        var visual=new GameObject("Quest target presentation");visual.transform.SetParent(root.transform,false);
        void Part(string name,Vector3 position,Vector3 size,Material material,PrimitiveType type=PrimitiveType.Cube){var go=GameObject.CreatePrimitive(type);Object.DestroyImmediate(go.GetComponent<Collider>());go.name=name;go.transform.SetParent(visual.transform,false);go.transform.localPosition=position;go.transform.localScale=size;if(type==PrimitiveType.Cylinder)go.transform.localRotation=Quaternion.Euler(90,0,0);go.GetComponent<Renderer>().sharedMaterial=material;go.layer=2;}
        var oak=Paint("Target oak",new(.31f,.19f,.09f));Part("Left target leg",new(-.42f,.55f,0),new(.13f,1.1f,.14f),oak);Part("Right target leg",new(.42f,.55f,0),new(.13f,1.1f,.14f),oak);
        var colors=new[]{new Color(.73f,.58f,.30f),new Color(.90f,.86f,.67f),new Color(.58f,.12f,.08f)};
        for(int side=-1;side<=1;side+=2)for(int ring=0;ring<3;ring++)Part("Target ring "+side+" "+ring,new(0,1.3f,side*(.04f+ring*.012f)),new(1.35f-ring*.4f,.035f,1.35f-ring*.4f),Paint("Archery ring "+ring,colors[ring]),PrimitiveType.Cylinder);
        root.AddComponent<MarksmanArcheryTarget>().Configure(settlement.ToLowerInvariant(),settlement,visual);visual.SetActive(false);root.SetActive(true);placed.Add(settlement+" target | "+root.transform.position);
    }
    static Texture2D Icon(int index)
    {
        string path=Data+MarksmanProgression.Name(index)+" icon.asset";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(texture==null){texture=new Texture2D(96,96,TextureFormat.RGBA32,false);AssetDatabase.CreateAsset(texture,path);}var colors=new Color[96*96];
        for(int y=0;y<96;y++)for(int x=0;x<96;x++){
            var p=new Vector2((x-47.5f)/48,(y-47.5f)/48);float Line(Vector2 a,Vector2 b)=>Vector2.Distance(p,a+(b-a)*Mathf.Clamp01(Vector2.Dot(p-a,b-a)/(b-a).sqrMagnitude));
            float arrow=Mathf.Min(Line(new(-.5f,-.5f),new(.5f,.5f)),Mathf.Min(Line(new(.05f,.5f),new(.5f,.5f)),Line(new(.5f,.05f),new(.5f,.5f))));float motif=index switch{1=>Mathf.Min(Mathf.Abs(p.x+.35f),Mathf.Abs(p.x-.35f))+Mathf.Max(0,Mathf.Abs(p.y)-.6f),2 or 4=>Mathf.Abs(p.magnitude-.55f),3=>Mathf.Abs(p.x-p.y*.5f)-.12f,_=>arrow};float ink=1-Mathf.SmoothStep(.02f,.065f,Mathf.Min(arrow,Mathf.Abs(motif)));colors[y*96+x]=Color.Lerp(new(.025f,.05f,.04f),index==1?new(.45f,.8f,1):index==5?new(.85f,1,.5f):new(.95f,.77f,.32f),ink);
        }texture.SetPixels(colors);texture.Apply();EditorUtility.SetDirty(texture);return texture;
    }
}
