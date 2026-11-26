using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DiceFree.Advancement;
using DiceFree.Combat;
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
public static class FireMageStaticReview
{
    [Serializable]public sealed class Report{public string status="passed";public int checks;public string[] passed;public float coursePathLength;}
    public static void Batch()
    {
        var checks=new List<string>();void Need(bool value,string label){if(!value)throw new Exception(label);checks.Add(label);}
        T Load<T>(string name)where T:Object=>AssetDatabase.LoadAssetAtPath<T>(FireMageAuthoring.Data+name)??throw new Exception("Missing "+name);
        var actor=Load<ActorDefinition>("Fire Mage class.asset");var mana=Load<ClassResourceProfile>("Fire Mage Mana.asset");var heat=Load<ClassResourceProfile>("Fire Mage Heat.asset");
        Need(actor.stableId==FireMageProgression.Id,"Stable independent Fire Mage identity");Need(new[]{35,25,30,30,15,15}.SequenceEqual(Enumerable.Range(0,6).Select(FireMageProgression.MaxRank)),"Approved six caps sum to 150");
        for(int level=1;level<=150;level++){int available=0;for(int i=0;i<6;i++)for(int r=1;r<=FireMageProgression.MaxRank(i);r++)if(FireMageProgression.RequiredLevel(i,r)<=level)available++;Need(available>=level,"Spendability at level "+level);}
        for(int i=0;i<6;i++)for(int r=2;r<=FireMageProgression.MaxRank(i);r++)Need(FireMageProgression.RequiredLevel(i,r)>FireMageProgression.RequiredLevel(i,r-1),$"Strict gate {i}/{r}");
        var a=actor.baseAttributes;Need(a.vitality==19&&a.strength==7&&a.agility==12&&a.intelligence==33&&a.spirit==13,"Approved level-one attributes");a=actor.growth;Need(a.vitality==2&&a.strength==.75f&&a.agility==1&&a.intelligence==6&&a.spirit==1.5f,"Approved stat growth");
        Need(mana.baseMaximum==160&&mana.Maximum(actor.baseAttributes)==325&&Mathf.Abs(mana.Regeneration(actor.baseAttributes)-5.3f)<.001f,"Global Mana formula and level-one 325 / 5.3s");
        Need(heat.baseMaximum==100&&heat.initialFraction==0&&heat.loadPolicy==ResourceRetentionPolicy.ResetOnLoad,"Heat max100 resets empty");
        for(int r=1;r<=35;r++){var f=FireMageCaster.Formula(0,r);Need(Mathf.Abs(f.x-(30+8*r))<.001f&&Mathf.Abs(f.y-(.3f+.04f*r))<.001f,"Fireball raw coefficients rank "+r);}
        Need(FireMageCaster.ManaCost(0,35)==545&&FireMageCaster.ManaCost(1,25)==160&&FireMageCaster.ManaCost(2,30)==480&&FireMageCaster.ManaCost(3,30)==500,"Final Mana costs");
        Need(FireMageCaster.Cooldown(0,1)==14&&FireMageCaster.Cooldown(0,35)==7&&FireMageCaster.Cooldown(1,25)==20&&FireMageCaster.Cooldown(2,30)==10&&FireMageCaster.Cooldown(3,30)==15,"Final cooldown endpoints");
        Need(Mathf.Abs(FireMageCaster.WardEfficiency(25)-1.3f)<.001f&&FireMageCaster.WardDuration(25)==10&&FireMageCaster.WardFraction(25)==.65f,"Ward rank25 curves");
        var quest=Load<QuestDefinition>("The Ember Walk.asset");var edge=Load<AdvancementDefinition>("Fire Mage advancement.asset");Need(quest.requiredClassId=="class.mage"&&quest.requiredLevel==60&&quest.stages.Single().contentId==FireEscapeSite.TrialId,"Mage level60 solo escape objective");Need(edge.targetClass==actor&&edge.sourceClass.stableId=="class.mage"&&edge.requiredQuestId==quest.stableId,"Quest-gated parent-preserving advancement");
        foreach(string name in new[]{"Cornberg","DevelopmentRoom"})
        {EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/"+name+".unity");var player=Object.FindObjectsByType<AdvancementController>(FindObjectsInactive.Include).Single();Need(player.GetComponent<FireMageCaster>()!=null&&player.GetComponent<FireMageProgression>()!=null&&player.GetComponent<FireMageAbilityProvider>()!=null,name+" has class systems");Need(player.GetComponent<ActorResourceController>().Profiles.Count(p=>p.classId==actor.stableId)==2,name+" has Mana and Heat profiles");}
        EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/WorldTwoArrival.unity");var teacher=Object.FindAnyObjectByType<FireMageMentor>();Need(teacher!=null,"Ember Crown mentor in World2");Need((teacher.transform.position-(AshfallReach.Origin+new Vector3(AshfallReach.EmberCrown.x,teacher.transform.position.y,AshfallReach.EmberCrown.y))).magnitude<95,"Mentor at existing volcano base");
        var form=Load<GameObject>("Fire Mage form.prefab");Need(form.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length>0,"Native weighted robe model");Need(form.GetComponentInChildren<Animator>(true).runtimeAnimatorController.animationClips.Length>=6,"Native idle/walk/cast/hit/death animation coverage");Need(form.GetComponentsInChildren<Renderer>(true).All(r=>r.bounds.size.magnitude<6),"Hand attachments normalize source rig scale");
        var trial=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/EmberCrownTrial.unity");var site=Object.FindAnyObjectByType<FireEscapeSite>();Need(!site.IsLava(site.entrance)&&!site.IsLava(site.finish),"Retry and exit beacons are safe");Need(site.flames.All(f=>!f.bounds.Contains(new Vector2(site.entrance.x,site.entrance.z))),"Entrance cannot be hit by flame gates");
        Need(site.flames.All(f=>f.visual.bounds.min.y>.07f),"Warning tiles render above safe stone, not under it");
        var nav=Object.FindAnyObjectByType<WorldNavigation>();var instance=NavMesh.AddNavMeshData(nav.Data);float length=0;
        try{var path=new NavMeshPath();Need(NavMesh.CalculatePath(site.entrance,site.finish,1,path)&&path.status==NavMeshPathStatus.PathComplete,"Continuous baked route to finish");for(int i=1;i<path.corners.Length;i++)length+=Vector3.Distance(path.corners[i-1],path.corners[i]);Need(length>180,"Interlocking ridges prevent direct exit shortcut");}finally{instance.Remove();}
        Need(FireEscapeSite.FlamePhase(0,0)==0&&FireEscapeSite.FlamePhase(2.1f,0)==1&&FireEscapeSite.FlamePhase(4.1f,0)==2,"Warn / burn / safe repeat deterministically");
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/FireMageStaticReview.json",JsonUtility.ToJson(new Report{checks=checks.Count,passed=checks.ToArray(),coursePathLength=length},true));EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");Debug.Log("DICEFREE_FIRE_MAGE_STATIC_OK "+checks.Count);
    }
}
