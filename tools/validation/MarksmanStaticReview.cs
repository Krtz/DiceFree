using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Skills;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;
public static class MarksmanStaticReview
{
    public static void Batch()
    {
        var passed=new List<string>();void Need(bool b,string message){if(!b)throw new Exception(message);passed.Add(message);}
        var caps=new[]{30,25,20,30,20,25};Need(Enumerable.Range(0,6).All(i=>MarksmanProgression.MaxRank(i)==caps[i]),"Approved six rank caps total 150");
        for(int level=1;level<=150;level++){int total=Enumerable.Range(0,6).Sum(i=>Enumerable.Range(1,caps[i]).Count(r=>MarksmanProgression.RequiredLevel(i,r)<=level));Need(total>=level,"One spendable rank available at class level "+level);}
        Need(Enumerable.Range(0,6).All(i=>Enumerable.Range(2,caps[i]-1).All(r=>MarksmanProgression.RequiredLevel(i,r)>MarksmanProgression.RequiredLevel(i,r-1))),"All per-skill gate sequences strictly increase");
        var actor=AssetDatabase.LoadAssetAtPath<ActorDefinition>(MarksmanAuthoring.Data+"Marksman class.asset");Need(actor.baseAttributes.agility==34&&actor.growth.agility==6&&actor.baseHp==20&&actor.basicAttack.baseDamage==8&&actor.basicAttack.weights.agility==1,"Approved class and basic attack numbers");
        Need(AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/_DiceFree/Settings/Ranger/Ranger bow basic.asset").weights.agility==1,"Ranger basic AGI correction");
        var profiles=AssetDatabase.FindAssets("t:ClassResourceProfile",new[]{"Assets/_DiceFree"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>!p.Contains("/Recovery/")).Select(AssetDatabase.LoadAssetAtPath<ClassResourceProfile>).Where(p=>p.resource?.stableId==ResourceIds.Mana).ToArray();
        foreach(var profile in profiles){var a=new AttributeValues{intelligence=17,spirit=23};Need(Mathf.Approximately(profile.Maximum(a),profile.baseMaximum+85)&&Mathf.Approximately(profile.Regeneration(a),6.3f),"Unified Mana calculation: "+profile.name);Need(profile.maximumPerIntelligence==5&&profile.maximumPerSpirit==0&&profile.regenPerIntelligence==0&&profile.regenPerSpirit==.1f&&profile.baseRegenPerSecond==4,"Migrated authoring fields: "+profile.name);}
        var nonMana=ScriptableObject.CreateInstance<ClassResourceProfile>();var other=ScriptableObject.CreateInstance<ResourceDefinition>();try{other.stableId="resource.qa-other";nonMana.resource=other;nonMana.baseMaximum=10;nonMana.maximumPerIntelligence=2;Need(nonMana.Maximum(new AttributeValues{intelligence=3})==16,"Non-Mana resource formulas preserved");}finally{Object.DestroyImmediate(nonMana);Object.DestroyImmediate(other);}
        var targetIds=new List<string>();foreach(string sceneName in new[]{"Cornberg","WorldTwoArrival","DevelopmentRoom"}){
            var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/"+sceneName+".unity");var roots=scene.GetRootGameObjects();
            if(sceneName!="WorldTwoArrival"){var player=roots.SelectMany(g=>g.GetComponentsInChildren<MarksmanCaster>(true)).Single();Need(player.GetComponent<MarksmanProgression>()!=null&&player.GetComponent<DiceFree.UI.MarksmanAbilityProvider>()!=null,"Serialized class providers: "+sceneName);}
            if(sceneName=="DevelopmentRoom")continue;
            var world=roots.SelectMany(g=>g.GetComponentsInChildren<WorldNavigation>(true)).Single();var instance=NavMesh.AddNavMeshData(world.Data);Physics.SyncTransforms();
            try{
                var origin=sceneName=="Cornberg"?new Vector3(8,0,6):AshfallReach.Origin+new Vector3(5,1,-47);Need(NavMesh.SamplePosition(origin,out var start,5,1),"Navigation start: "+sceneName);
                foreach(var target in roots.SelectMany(g=>g.GetComponentsInChildren<MarksmanArcheryTarget>(true))){targetIds.Add(target.SettlementId);Need(!target.GetComponent<CombatActor>().enabled&&!target.GetComponent<Collider>().enabled,"Target hidden before acceptance: "+target.SettlementName);var path=new NavMeshPath();Need(NavMesh.CalculatePath(start.position,target.transform.position,1,path)&&path.status==NavMeshPathStatus.PathComplete,"Settlement target reachable: "+target.SettlementName);}
                if(sceneName=="WorldTwoArrival"){var teacher=roots.SelectMany(g=>g.GetComponentsInChildren<MarksmanMentor>(true)).Single();var path=new NavMeshPath();Need(NavMesh.CalculatePath(start.position,teacher.ApproachPosition,1,path)&&path.status==NavMeshPathStatus.PathComplete,"Teacher has a connected approach");}
            }finally{instance.Remove();}
        }
        Need(targetIds.Count==6&&targetIds.Distinct().Count()==6,"Six canonical towns/cities across both worlds, no biome targets");
        var menu=AssetDatabase.LoadAssetAtPath<MenuPresentationCatalog>("Assets/_DiceFree/Resources/MenuPresentationCatalog.asset");Need(menu.TierOf(MarksmanProgression.Id)==3,"Marksman is a Tier III menu form");var form=menu.Resolve(MarksmanProgression.Id).preview;Need(form.GetComponentsInChildren<SkinnedMeshRenderer>(true).All(s=>s.sharedMesh!=null)&&form.GetComponentInChildren<Animator>(true).avatar.isValid,"Native Wood Elf mesh and rig are valid");
        var quest=AssetDatabase.LoadAssetAtPath<QuestDefinition>(MarksmanAuthoring.Data+"The Farwatch Pilgrimage.asset");Need(quest.stages.Single().kind==ObjectiveKind.All&&quest.stages[0].alternatives.Length==6&&quest.requiredLevel==60,"Quest uses durable independent settlement objectives");
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/MarksmanStaticReview.json",JsonUtility.ToJson(new Result{checks=passed.Count,passed=passed.ToArray(),manaProfiles=profiles.Length},true));Debug.Log("DICEFREE_MARKSMAN_STATIC_OK");
    }
    [Serializable]sealed class Result{public int checks,manaProfiles;public string[] passed;}
}
