using System;using System.IO;using System.Linq;using System.Collections.Generic;
using DiceFree.Advancement;using DiceFree.Combat;using DiceFree.Persistence;using DiceFree.Progression;using DiceFree.Skills;using DiceFree.Quests;using DiceFree.UI;using DiceFree.Visuals;using DiceFree.World;
using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Unity.Pipeline.Commands;using Object=UnityEngine.Object;
namespace DiceFree.EditorTools
{
 public static class MageAuthoring
 {
  public const string Folder="Assets/_DiceFree/Settings/Skills/Mage",ClassPath=Folder+"/Mage class.asset",TuningPath=Folder+"/Mage tuning.asset",QuestPath=Folder+"/A Mind for Magic.asset",EdgePath=Folder+"/Magical to Mage.asset";
  static void DirectoryAsset(string path){if(AssetDatabase.IsValidFolder(path))return;string parent=Path.GetDirectoryName(path).Replace('\\','/');DirectoryAsset(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(path));}
  static T Asset<T>(string path)where T:ScriptableObject{var a=AssetDatabase.LoadAssetAtPath<T>(path);if(a!=null)return a;a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,path);return a;}
  static T Ensure<T>(GameObject go)where T:Component=>go.GetComponent<T>()??go.AddComponent<T>();
  [CliCommand("dicefree.mage.validate","Validate Mage data, six elements, eight abilities, quest and both player scene compositions.")]
  public static object Validate(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode only.");
   void Need(bool ok,string text){if(!ok)throw new InvalidOperationException(text);}
   var definition=AssetDatabase.LoadAssetAtPath<ActorDefinition>(ClassPath);
   var tuning=AssetDatabase.LoadAssetAtPath<MageCombatTuning>(TuningPath);
   var quest=AssetDatabase.LoadAssetAtPath<QuestDefinition>(QuestPath);
   var edge=AssetDatabase.LoadAssetAtPath<AdvancementDefinition>(EdgePath);
   var profile=AssetDatabase.LoadAssetAtPath<ClassResourceProfile>(Folder+"/Mage Mana.asset");
   Need(definition!=null&&tuning!=null&&quest!=null&&edge!=null&&profile!=null,"Missing Mage data.");
   Need(definition.stableId==MageProgressionRules.ClassId&&definition.basicAttack.weights.intelligence==1&&definition.basicAttack.weights.strength==0&&definition.basicAttack.weights.spirit==0,"Mage must scale attacks with INT.");
   Need(tuning.attacks.Length==6&&tuning.attacks.All(a=>a!=null&&a.element!=null&&!a.requiresAccuracy&&a.weights.intelligence==1)&&tuning.attacks.Select(a=>a.element.stableId).Distinct().Count()==6,"Six distinct elemental attacks.");
   Need(MageProgressionRules.Skills.Length==8&&MageProgressionRules.Skills.Count(r=>r.passive)==2,"Flexible eight-ability Mage kit.");
   Need(tuning.decayDelay==8&&tuning.decayInterval==1,"Weaving timing.");
   Need(MageProgressionRules.Cooldown(MageProgressionRules.Kind.EmberBolt)>0&&MageProgressionRules.Cooldown(MageProgressionRules.Kind.ArcLightning)>MageProgressionRules.Cooldown(MageProgressionRules.Kind.EmberBolt),"Cooldown identity.");
   Need(profile.classId==definition.stableId&&profile.maximumPerIntelligence==10&&profile.maximumPerSpirit==0,"Mage Mana scaling.");
   Need(edge.targetClass==definition&&edge.requiredLevel==30&&edge.requiredQuestId==quest.stableId&&quest.requiredClassId==edge.sourceClass.stableId,"Advancement gate.");
   Need(quest.stages.Length==1&&quest.stages[0].kind==ObjectiveKind.SolvePuzzle&&quest.stages[0].contentId=="puzzle.slime-tree"&&!quest.completeOnObjectives,"Puzzle objective / return turn-in.");
   var setup=EditorSceneManager.GetSceneManagerSetup();int players=0;
   try{
    foreach(string path in new[]{"Assets/_DiceFree/Scenes/Cornberg.unity","Assets/_DiceFree/Scenes/DevelopmentRoom.unity"}){
     var scene=EditorSceneManager.OpenScene(path);
     foreach(var p in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<ManifestationPersistence>(true))){
      Need(p.GetComponent<MageSkillProgression>()!=null&&p.GetComponent<MageSkillCaster>()?.Tuning==tuning&&p.GetComponent<MagePresentation>()!=null,"Mage runtime composition.");
      Need(p.GetComponent<ClassPresentationSwitcher>().Resolve(definition.stableId)?.GetComponent<MageOutfit>()!=null,"Mage visual binding.");
      Need(p.GetComponent<QuestJournal>().Definitions.Contains(quest)&&p.GetComponent<ActorResourceController>().Profiles.Any(r=>r.classId==definition.stableId),"Mage quest/resource composition.");players++;
     }
     if(path.Contains("Cornberg"))Need(Object.FindObjectsByType<MageMentor>().Single().TrainingQuest==quest,"Brewer mentor.");
    }
   }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
   return new{marker="DICEFREE_MAGE_DATA_OK",players,abilities=8,actives=6,elements=6,advancementLevel=30,visual="Original placeholder"};
  }
  [CliCommand("dicefree.mage.author","Author Tier II Mage, eight abilities, puzzle advancement and both player scenes.")]
  public static object Run(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode.");
   var setup=EditorSceneManager.GetSceneManagerSetup();int players=0;
   try{
    DirectoryAsset(Folder);
    var parent=AssetDatabase.LoadAssetAtPath<ActorDefinition>(AdvancementAuthoring.MagicalPath);
    var definition=Asset<ActorDefinition>(ClassPath);EditorUtility.CopySerialized(parent,definition);
    definition.name="Mage class";definition.stableId=MageProgressionRules.ClassId;definition.displayName="Mage";
    definition.baseAttributes=new AttributeValues{vitality=11,strength=5,agility=8,intelligence=18,spirit=9};
    definition.growth=new AttributeValues{vitality=1.5f,strength=.5f,agility=.75f,intelligence=3,spirit=1.5f};
    var basic=Asset<AttackDefinition>(Folder+"/Mage wand basic.asset");EditorUtility.CopySerialized(parent.basicAttack,basic);
    basic.stableId="attack.mage.wand";basic.displayName="Wand Spark";basic.weights=new AttributeValues{intelligence=1};basic.scaling=AttributeScaling.Weighted;basic.channel=DamageChannel.Magical;
    basic.baseDamage=1;basic.coefficient=1.2f;basic.interval=1.5f;basic.reach=12;basic.windup=.25f;basic.element=null;definition.basicAttack=basic;
    var tuning=Asset<MageCombatTuning>(TuningPath);tuning.attacks=new AttackDefinition[6];tuning.range=18;tuning.chainRange=6;tuning.quicksandRadius=3.5f;tuning.decayDelay=8;tuning.decayInterval=1;
    string[] elements={"Fire","Ice","Electric","Earth","Radiation","Math"};
    foreach(var rule in MageProgressionRules.Skills.Where(r=>!r.passive)){
     int i=(int)rule.kind;string name=MageProgressionRules.Name(rule.kind);
     var element=Asset<ElementDefinition>(MagicalTouchedAuthoring.ElementFolder+"/"+elements[i]+".asset");element.stableId="element."+elements[i].ToLowerInvariant();element.displayName=elements[i];EditorUtility.SetDirty(element);
     var attack=Asset<AttackDefinition>(Folder+"/"+name+" attack.asset");attack.stableId="attack.mage."+rule.kind.ToString().ToLowerInvariant();attack.displayName=name;attack.element=element;attack.channel=DamageChannel.Magical;
     attack.scaling=AttributeScaling.Weighted;attack.weights=new AttributeValues{intelligence=1};attack.baseDamage=15;attack.coefficient=MageProgressionRules.Coefficient(rule.kind,1);attack.requiresAccuracy=false;attack.reach=18;
     tuning.attacks[i]=attack;EditorUtility.SetDirty(attack);
    }
    var profile=Asset<ClassResourceProfile>(Folder+"/Mage Mana.asset");
    EditorUtility.CopySerialized(AssetDatabase.LoadAssetAtPath<ClassResourceProfile>(MagicalTouchedAuthoring.MagicalManaPath),profile);
    profile.stableId="resource-profile.mage.mana";profile.classId=definition.stableId;profile.baseMaximum=80;profile.maximumPerIntelligence=10;profile.maximumPerSpirit=0;profile.baseRegenPerSecond=2;profile.regenPerIntelligence=.04f;profile.regenPerSpirit=0;profile.initialFraction=1;
    var quest=Asset<QuestDefinition>(QuestPath);quest.stableId=MageMentor.QuestId;quest.title="A Mind for Magic";quest.requiredClassId=parent.stableId;quest.requiredLevel=30;
    quest.offer="Magic asks more than power. Enter the Slime Dungeon and solve the enchanted tree puzzle. Return to me afterwards, and I will teach you the Mage's craft.";
    quest.activeDialogue="Solve the five-ring tree puzzle in the Slime Dungeon, then return to the retired mage at Cornberg's brewery.";
    quest.locationHint="Slime Dungeon tree puzzle; return to Cornberg's brewer.";
    quest.stages=new[]{new QuestObjective{kind=ObjectiveKind.SolvePuzzle,objectiveId="solve-slime-tree",contentId="puzzle.slime-tree",count=1,instruction="Solve the Slime Dungeon tree puzzle"}};
    quest.rewardXp=0;quest.reward=null;quest.acceptOnTalk=false;quest.completeOnObjectives=false;quest.repeatable=false;
    var edge=Asset<AdvancementDefinition>(EdgePath);edge.stableId="advancement.magical.mage";edge.displayName="Become a Mage";edge.sourceClass=parent;edge.targetClass=definition;edge.requiredLevel=30;edge.requiredQuestId=quest.stableId;
    var catalog=AssetDatabase.LoadAssetAtPath<ClassCatalog>(AdvancementAuthoring.CatalogPath);
    catalog.Configure(catalog.Classes.Where(c=>c.stableId!=definition.stableId).Append(definition).ToArray());
    foreach(var asset in new Object[]{definition,basic,tuning,profile,quest,edge,catalog})EditorUtility.SetDirty(asset);
    AssetDatabase.SaveAssets();
    foreach(string path in new[]{"Assets/_DiceFree/Scenes/Cornberg.unity","Assets/_DiceFree/Scenes/DevelopmentRoom.unity"}){
     var scene=EditorSceneManager.OpenScene(path);catalog=AssetDatabase.LoadAssetAtPath<ClassCatalog>(AdvancementAuthoring.CatalogPath);definition=AssetDatabase.LoadAssetAtPath<ActorDefinition>(ClassPath);
     edge=AssetDatabase.LoadAssetAtPath<AdvancementDefinition>(EdgePath);profile=AssetDatabase.LoadAssetAtPath<ClassResourceProfile>(Folder+"/Mage Mana.asset");quest=AssetDatabase.LoadAssetAtPath<QuestDefinition>(QuestPath);tuning=AssetDatabase.LoadAssetAtPath<MageCombatTuning>(TuningPath);
     foreach(var persistence in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<ManifestationPersistence>(true))){
      var go=persistence.gameObject;var journal=go.GetComponent<QuestJournal>();journal.Configure(journal.Definitions.Where(q=>q.stableId!=quest.stableId).Append(quest).ToArray());EditorUtility.SetDirty(journal);
      persistence.ConfigureClassCatalog(catalog);EditorUtility.SetDirty(persistence);
      var controller=go.GetComponent<AdvancementController>();controller.Configure(catalog,controller.Definitions.Where(d=>d.stableId!=edge.stableId).Append(edge).ToArray());EditorUtility.SetDirty(controller);
      Ensure<MageSkillProgression>(go);Ensure<MagePresentation>(go);var caster=Ensure<MageSkillCaster>(go);caster.Configure(tuning);EditorUtility.SetDirty(caster);
      var resources=go.GetComponent<ActorResourceController>();resources.Configure(resources.Profiles.Where(p=>p.classId!=definition.stableId).Append(profile).ToArray());EditorUtility.SetDirty(resources);
      var switcher=go.GetComponent<ClassPresentationSwitcher>();var data=new SerializedObject(switcher);var array=data.FindProperty("bindings");var bindings=new List<ClassPresentationSwitcher.Binding>();
      for(int i=0;i<array.arraySize;i++){var b=array.GetArrayElementAtIndex(i);string id=b.FindPropertyRelative("classId").stringValue;if(id==definition.stableId)continue;bindings.Add(new ClassPresentationSwitcher.Binding{classId=id,visualRoot=b.FindPropertyRelative("visualRoot").objectReferenceValue as Transform,animator=b.FindPropertyRelative("animator").objectReferenceValue as Animator});}
      var visual=go.transform.Find("MagePresentation");
      if(visual==null){
       var source=switcher.Resolve(AdvancementAuthoring.MagicalId);
       visual=Object.Instantiate(source.gameObject,go.transform).transform;visual.name="MagePresentation";visual.gameObject.SetActive(false);
       if(PrefabUtility.IsPartOfPrefabInstance(visual.gameObject))PrefabUtility.UnpackPrefabInstance(visual.gameObject,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
       var originals=source.GetComponentsInChildren<Transform>(true);var copies=visual.GetComponentsInChildren<Transform>(true);
       var mapping=new Dictionary<GameObject,GameObject>();for(int i=0;i<originals.Length;i++)mapping[originals[i].gameObject]=copies[i].gameObject;
       var equipment=go.GetComponent<EquipmentPresentation>();
       foreach(var b in equipment.Bindings){b.visuals=b.visuals.Concat(b.visuals.Where(g=>g!=null&&mapping.ContainsKey(g)).Select(g=>mapping[g])).ToArray();b.baselineVisuals=b.baselineVisuals.Concat(b.baselineVisuals.Where(g=>g!=null&&mapping.ContainsKey(g)).Select(g=>mapping[g])).ToArray();}
       EditorUtility.SetDirty(equipment);
      }
      Ensure<MageOutfit>(visual.gameObject);bindings.Add(new ClassPresentationSwitcher.Binding{classId=definition.stableId,visualRoot=visual});
      switcher.Configure(bindings.ToArray());EditorUtility.SetDirty(switcher);
      foreach(var b in bindings)b.visualRoot.gameObject.SetActive(b.classId==go.GetComponent<ActorStats>().Definition.stableId);
      players++;
     }
     if(path.Contains("Cornberg")){
      var npc=GameObject.Find("NPCs/NPC_Alchemist - 3D");if(npc==null)throw new InvalidOperationException("Brewer NPC missing.");
      var mentor=Ensure<MageMentor>(npc);mentor.Configure(quest);mentor.ConfigureName("Mage training");
      var context=Ensure<ContextualNpc>(npc);var so=new SerializedObject(context);var actions=so.FindProperty("actions");var keep=new List<InteractionTarget>();
      for(int i=0;i<actions.arraySize;i++){var a=actions.GetArrayElementAtIndex(i).objectReferenceValue as InteractionTarget;if(a!=null&&a!=mentor)keep.Add(a);}
      if(keep.Count==0)keep.AddRange(npc.GetComponents<InteractionTarget>().Where(a=>a!=context&&a!=mentor));
      context.Configure(string.IsNullOrWhiteSpace(context.NpcId)?"npc.cornberg.brewer":context.NpcId,new[]{(InteractionTarget)mentor}.Concat(keep).ToArray());
      EditorUtility.SetDirty(mentor);EditorUtility.SetDirty(context);
     }
     EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    AssetDatabase.SaveAssets();return new{marker="DICEFREE_MAGE_AUTHORED",players,classes=catalog.Classes.Count,abilities=8,quest=MageMentor.QuestId,visual="Original placeholder; reference pack not installed"};
   }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
  }
 }
}
