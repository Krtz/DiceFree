using System;
using System.Linq;
using System.Collections.Generic;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.Visuals;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace DiceFree.EditorTools
{
    public static class GreatbladePlayableAuthoring
    {
        public const string Folder="Assets/_DiceFree/Settings/Skills/Greatblade";
        public const string ClassPath=Folder+"/Greatblade class.asset";
        public const string TuningPath=Folder+"/Greatblade combat tuning.asset";
        public const string EdgePath=Folder+"/Physical to Greatblade.asset";
        [CliCommand("dicefree.greatblade.playable.author","Author playable Tier II Greatblade, skills/resource/advancement and Cornberg/development-room composition.")]
        public static object Run()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
            if(EditorSceneManager.GetSceneManagerSetup().Any(s=>SceneManager.GetSceneByPath(s.path).isDirty))throw new InvalidOperationException("Save open scene edits first.");
            var previous=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/_DiceFree/Settings/Skills","Greatblade");
                var physical=AssetDatabase.LoadAssetAtPath<ActorDefinition>(AdvancementAuthoring.PhysicalPath);
                var definition=Asset<ActorDefinition>(ClassPath);
                EditorUtility.CopySerialized(physical,definition);definition.name="Greatblade class";
                definition.stableId=GreatbladeProgressionRules.ClassId;definition.displayName="Greatblade";
                definition.baseAttributes=new AttributeValues{vitality=14,strength=18,agility=9,intelligence=5,spirit=5};
                definition.growth=new AttributeValues{vitality=2,strength=3,agility=1,intelligence=.5f,spirit=.5f};
                definition.basicAttack=Attack("Greatsword basic",2,1.5f,.35f,1.3f);
                definition.basicAttack.requiresAccuracy=true;definition.basicAttack.minimumDamageMultiplier=.9f;definition.basicAttack.maximumDamageMultiplier=1.1f;
                EditorUtility.SetDirty(definition.basicAttack);EditorUtility.SetDirty(definition);
                var tuning=Asset<GreatbladeCombatTuning>(TuningPath);
                tuning.wideArc=Attack("Wide Arc",1.8f,1,.35f,3);
                tuning.spinDash=Attack("Spin Dash",1.2f,1,0,1.8f);
                tuning.overheadBreaker=Attack("Overhead Breaker",2.5f,1,.55f,1.2f);
                EditorUtility.SetDirty(tuning);
                var resource=Asset<ResourceDefinition>(Folder+"/Momentum.asset");resource.stableId=GreatbladeSkillCaster.MomentumId;resource.displayName="Momentum";EditorUtility.SetDirty(resource);
                var profile=Asset<ClassResourceProfile>(Folder+"/Greatblade Momentum.asset");
                profile.stableId="resource-profile.greatblade.momentum";profile.classId=definition.stableId;profile.resource=resource;
                profile.baseMaximum=100;profile.baseRegenPerSecond=0;profile.initialFraction=0;profile.loadPolicy=ResourceRetentionPolicy.ResetOnLoad;EditorUtility.SetDirty(profile);
                var quest=Asset<QuestDefinition>(Folder+"/Swordswoman Slime Trial.asset");
                quest.stableId=GreatbladeMentor.QuestId;quest.title="The Weight of a Greatblade";
                quest.requiredClassId=physical.stableId;quest.requiredLevel=30;
                quest.offer="Strength alone does not make a Greatblade. Enter the Slime Dungeon and defeat its normal Slime Boss. Return to me, and I will teach you to carry a greatsword.";
                quest.activeDialogue="Your training awaits in the Slime Dungeon. Defeat the normal Slime Boss, then return to the retired swordswoman in Cornberg.";
                quest.locationHint="Slime Dungeon: defeat the normal Slime Boss, then return to Cornberg's retired swordswoman.";
                quest.stages=new[]{new QuestObjective{kind=ObjectiveKind.Kill,objectiveId="kill-normal-slime-boss",contentId="boss.slime",dungeonBossOnly=true,count=1,creditMode=QuestCreditMode.NearbyParty,creditRadius=50,instruction="Defeat the normal Slime Boss"}};
                quest.rewardXp=0;quest.reward=null;quest.acceptOnTalk=false;quest.completeOnObjectives=false;quest.repeatable=false;
                EditorUtility.SetDirty(quest);
                var edge=Asset<AdvancementDefinition>(EdgePath);edge.stableId="advancement.physical.greatblade";edge.displayName="Become a Greatblade";
                edge.requiredQuestId=quest.stableId;
                edge.sourceClass=physical;edge.targetClass=definition;edge.requiredLevel=30;EditorUtility.SetDirty(edge);
                var catalog=AssetDatabase.LoadAssetAtPath<ClassCatalog>(AdvancementAuthoring.CatalogPath);
                catalog.Configure(catalog.Classes.Where(x=>x.stableId!=definition.stableId).Concat(new[]{definition}).ToArray());EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();
                int players=0;
                foreach(string path in new[]{"Assets/_DiceFree/Scenes/Cornberg.unity","Assets/_DiceFree/Scenes/DevelopmentRoom.unity"})
                {
                    var scene=EditorSceneManager.OpenScene(path);
                    catalog=AssetDatabase.LoadAssetAtPath<ClassCatalog>(AdvancementAuthoring.CatalogPath);
                    definition=AssetDatabase.LoadAssetAtPath<ActorDefinition>(ClassPath);
                    edge=AssetDatabase.LoadAssetAtPath<AdvancementDefinition>(EdgePath);
                    tuning=AssetDatabase.LoadAssetAtPath<GreatbladeCombatTuning>(TuningPath);
                    profile=AssetDatabase.LoadAssetAtPath<ClassResourceProfile>(Folder+"/Greatblade Momentum.asset");
                    quest=AssetDatabase.LoadAssetAtPath<QuestDefinition>(Folder+"/Swordswoman Slime Trial.asset");
                    foreach(var persistence in scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<ManifestationPersistence>(true)))
                    {
                        var go=persistence.gameObject;
                        var journal=go.GetComponent<QuestJournal>();
                        journal.Configure(journal.Definitions.Where(x=>x.stableId!=quest.stableId).Concat(new[]{quest}).ToArray());EditorUtility.SetDirty(journal);
                        persistence.ConfigureClassCatalog(catalog);EditorUtility.SetDirty(persistence);
                        var controller=go.GetComponent<AdvancementController>();
                        controller.Configure(catalog,controller.Definitions.Where(x=>x.stableId!=edge.stableId).Concat(new[]{edge}).ToArray());EditorUtility.SetDirty(controller);
                        Ensure<GreatbladeSkillProgression>(go);
                        var caster=Ensure<GreatbladeSkillCaster>(go);caster.Configure(tuning);EditorUtility.SetDirty(caster);
                        Ensure<GreatbladePresentation>(go);
                        var resources=go.GetComponent<ActorResourceController>();
                        resources.Configure(resources.Profiles.Where(x=>x.classId!=definition.stableId).Concat(new[]{profile}).ToArray());EditorUtility.SetDirty(resources);
                        var switcher=go.GetComponent<ClassPresentationSwitcher>();
                        var serialized=new SerializedObject(switcher);var existing=serialized.FindProperty("bindings");
                        var bindings=new List<ClassPresentationSwitcher.Binding>();
                        for(int i=0;i<existing.arraySize;i++)
                        {
                            var b=existing.GetArrayElementAtIndex(i);
                            string id=b.FindPropertyRelative("classId").stringValue;
                            if(id==definition.stableId)continue;
                            bindings.Add(new ClassPresentationSwitcher.Binding{classId=id,visualRoot=b.FindPropertyRelative("visualRoot").objectReferenceValue as Transform,animator=b.FindPropertyRelative("animator").objectReferenceValue as Animator});
                        }
                        var visual=go.transform.Find("GreatbladePresentation");
                        if(visual==null){visual=new GameObject("GreatbladePresentation").transform;visual.SetParent(go.transform,false);visual.gameObject.AddComponent<GreatbladePrototypeVisual>();}
                        visual.gameObject.SetActive(false);
                        bindings.Add(new ClassPresentationSwitcher.Binding{classId=definition.stableId,visualRoot=visual});
                        switcher.Configure(bindings.ToArray());EditorUtility.SetDirty(switcher);
                        string authoredClass=go.GetComponent<ActorStats>().Definition.stableId;
                        foreach(var binding in bindings)binding.visualRoot.gameObject.SetActive(binding.classId==authoredClass);
                        foreach(var state in go.GetComponents<GreatbladeSkillProgression>())EditorUtility.SetDirty(state);
                        EditorUtility.SetDirty(go.GetComponent<GreatbladePresentation>());
                        players++;
                    }
                    if(path.Contains("Cornberg"))
                    {
                        var farmer=GameObject.Find("Former swordswoman - farmer");
                        if(farmer==null)throw new InvalidOperationException("Retired swordswoman not found.");
                        var mentor=Ensure<GreatbladeMentor>(farmer);mentor.ConfigureName("Greatblade training");mentor.Configure(quest);
                        var context=Ensure<ContextualNpc>(farmer);
                        var data=new SerializedObject(context);var actions=data.FindProperty("actions");var keep=new List<InteractionTarget>();
                        for(int i=0;i<actions.arraySize;i++){var target=actions.GetArrayElementAtIndex(i).objectReferenceValue as InteractionTarget;if(target!=null&&target!=mentor)keep.Add(target);}
                        if(keep.Count==0)keep.AddRange(farmer.GetComponents<InteractionTarget>().Where(x=>x!=context&&x!=mentor));
                        context.Configure(string.IsNullOrWhiteSpace(context.NpcId)?"npc.cornberg.swordswoman":context.NpcId,new[]{(InteractionTarget)mentor}.Concat(keep).ToArray());
                        if(farmer.transform.Find("Greatblade mentor shared base")==null)
                        {
                            var art=new GameObject("Greatblade mentor shared base");art.transform.SetParent(farmer.transform,false);
                            Vector3 scale=farmer.transform.lossyScale;art.transform.localScale=new Vector3(1/scale.x,1/scale.y,1/scale.z);
                            art.AddComponent<GreatbladePrototypeVisual>();
                        }
                        var sharedBase=farmer.transform.Find("Greatblade mentor shared base");
                        if(Physics.Raycast(farmer.transform.position+Vector3.up*15,Vector3.down,out var ground,30,1<<8,QueryTriggerInteraction.Ignore))
                            sharedBase.position=ground.point;
                        var renderer=farmer.GetComponent<Renderer>();if(renderer!=null)renderer.enabled=false;
                        EditorUtility.SetDirty(mentor);EditorUtility.SetDirty(context);
                    }
                    EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                }
                AssetDatabase.SaveAssets();
                return new{marker="DICEFREE_GREATBLADE_PLAYABLE_AUTHORED",players,skills=6,advancementLevel=30,momentumCap=100};
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(previous);}
        }
        static T Ensure<T>(GameObject go)where T:Component=>go.GetComponent<T>()??go.AddComponent<T>();
        static T Asset<T>(string path)where T:ScriptableObject
        {
            var asset=AssetDatabase.LoadAssetAtPath<T>(path);if(asset!=null)return asset;
            asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,path);return asset;
        }
        static AttackDefinition Attack(string name,float coefficient,float interval,float windup,float reach)
        {
            var a=Asset<AttackDefinition>(Folder+"/"+name+" attack.asset");
            a.stableId="attack.greatblade."+name.ToLowerInvariant().Replace(' ','-');a.displayName=name;
            a.channel=DamageChannel.Physical;a.scaling=AttributeScaling.Weighted;a.weights=new AttributeValues{strength=1};
            a.baseDamage=0;a.coefficient=coefficient;a.interval=interval;a.windup=windup;a.reach=reach;a.requiresAccuracy=true;
            EditorUtility.SetDirty(a);return a;
        }
    }
}
