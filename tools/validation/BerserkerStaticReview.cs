using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Items;
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
public static class BerserkerStaticReview
{
    [Serializable]public sealed class Report{public string status="passed";public int checks;public string[] passed;public string[] motions;}
    public static void Batch()
    {
        var checks=new List<string>();void Need(bool test,string name){if(!test)throw new Exception(name);checks.Add(name);}
        T Load<T>(string p)where T:Object=>AssetDatabase.LoadAssetAtPath<T>(p)??throw new Exception("Missing "+p);
        var catalog=Load<ClassCatalog>("Assets/_DiceFree/Settings/Progression/Class catalog.asset");var actor=catalog.Resolve(BerserkerProgression.Id);
        Need(actor!=null,"Berserker registered in main catalog");
        Need(new[]{35,30,30,25,30}.SequenceEqual(Enumerable.Range(0,5).Select(BerserkerProgression.MaxRank)),"Five approved caps total 150");
        Need(new[]{1,3,6,1,8}.SequenceEqual(Enumerable.Range(0,5).Select(i=>BerserkerProgression.RequiredLevel(i,1))),"Approved staggered first unlocks");
        for(int l=1;l<=150;l++)Need(Enumerable.Range(0,5).Sum(i=>Enumerable.Range(1,BerserkerProgression.MaxRank(i)).Count(r=>BerserkerProgression.RequiredLevel(i,r)<=l))>=l,"Spendability at level "+l);
        for(int i=0;i<5;i++)for(int r=2;r<=BerserkerProgression.MaxRank(i);r++)Need(BerserkerProgression.RequiredLevel(i,r)>BerserkerProgression.RequiredLevel(i,r-1),$"Strict rank gates {i}/{r}");
        Need(actor.baseAttributes.vitality==25&&actor.baseAttributes.strength==34&&actor.baseAttributes.agility==10&&actor.baseAttributes.intelligence==5&&actor.baseAttributes.spirit==7,"Approved level-one attributes");
        Need(actor.growth.vitality==3&&actor.growth.strength==6&&actor.growth.agility==1.2f&&actor.growth.intelligence==.5f&&actor.growth.spirit==.8f,"Approved growth");
        Need(Mathf.Approximately(actor.basicAttack.RawDamage(actor.baseAttributes),34),"Level-one core auto damage 34");
        Need(Mathf.Approximately(actor.baseHp+actor.baseAttributes.vitality*actor.VitalityHpCoefficient,385),"Level-one baseline maximum HP 385");
        foreach(var c in catalog.Classes)Need(c.VitalityHpCoefficient==15,"Global 15 HP/VIT: "+c.stableId);
        var rage=Load<ClassResourceProfile>(BerserkerAuthoring.Data+"Berserker Rage.asset");
        Need(rage.Maximum(actor.baseAttributes)==100&&rage.Regeneration(actor.baseAttributes)==0&&rage.initialFraction==0&&rage.loadPolicy==ResourceRetentionPolicy.ResetOnLoad,"Rage cap, zero generation outside events and reset-on-load");
        for(int r=0;r<=25;r++){Need(BerserkerCaster.RagePerBasic(r)==(r==0?1:r<=10?2:r<=20?3:4),"Basic Rage rank "+r);Need(Mathf.Approximately(BerserkerCaster.DecayPerSecond(r),5-4.9f*r/25),"Fractional decay rank "+r);}
        Need(Mathf.Approximately(BerserkerCaster.ShieldFraction(1),.05f)&&Mathf.Approximately(BerserkerCaster.ShieldFraction(30),.35f),"Coffee shield endpoint fractions");
        Need(new[]{7f,15f,12f,0f,25f}.SequenceEqual(Enumerable.Range(0,5).Select(BerserkerCaster.Cooldown)),"Final approved cooldowns");
        Need(new[]{10,0,30,0,25}.SequenceEqual(Enumerable.Range(0,5).Select(BerserkerCaster.Cost)),"Final fixed Rage costs");
        for(int i=0;i<3;i++){var f=BerserkerCaster.Formula(i,BerserkerProgression.MaxRank(i));Need(Mathf.Approximately(f.x,new[]{148f,95,257}[i])&&Mathf.Approximately(f.y,new[]{5.2f,3.52f,7.14f}[i]),"Offensive formula cap "+i);}
        var quest=Load<QuestDefinition>(BerserkerAuthoring.Data+"Trial of Fury.asset");
        Need(quest.requiredClassId=="class.greatblade"&&quest.requiredLevel==60&&quest.stages.Length==1&&quest.stages[0].dungeonBossOnly&&quest.stages[0].contentId==BerserkerMentor.BossId&&!quest.repeatable,"Bristlebreaker-only level-60 advancement contract");
        var edge=Load<AdvancementDefinition>(BerserkerAuthoring.Data+"Berserker advancement.asset");Need(edge.targetClass==actor&&edge.sourceClass.stableId=="class.greatblade"&&edge.requiredQuestId==quest.stableId,"Separate Greatblade advancement edge");
        var axe=Load<ItemDefinition>(BerserkerAuthoring.Data+"Ashscar Greataxe.asset");Need(ItemDefinition.IsGreatWeapon(axe)&&axe.greatWeaponFamily==GreatWeaponFamily.TwoHandedAxe,"Starter is an eligible two-handed axe");
        Need(AssetDatabase.FindAssets("t:ItemDefinition",new[]{"Assets/_DiceFree/Settings","Assets/_DiceFree/Resources/DiceFree"}).Select(g=>Load<ItemDefinition>(AssetDatabase.GUIDToAssetPath(g))).Any(i=>i.greatWeaponFamily==GreatWeaponFamily.TwoHandedSword&&(i.allowedClassIds.Length==0||i.allowedClassIds.Contains(BerserkerProgression.Id))),"Existing universal two-handed swords remain eligible");
        var avatar=AssetDatabase.LoadAllAssetsAtPath(BerserkerAuthoring.Art+"AshscarOrc.fbx").OfType<Avatar>().Single();Need(avatar.isValid&&avatar.isHuman,"Native orc avatar valid and humanoid");
        Need(Load<GameObject>(BerserkerAuthoring.Data+"Ashscar Berserker.prefab").GetComponentInChildren<Animator>(true).avatar==avatar,"Class prefab animator actually references the humanoid avatar");
        var controller=Load<RuntimeAnimatorController>("Assets/_DiceFree/Resources/PlayerMotion/TierThree/Berserker.controller");
        var motions=controller.animationClips.Select(c=>$"{c.name}: human={c.humanMotion}, duration={c.length}").ToArray();
        Need(controller.animationClips.Length>=3,"Combat, idle and locomotion clips present");
        var menu=Load<MenuPresentationCatalog>("Assets/_DiceFree/Resources/MenuPresentationCatalog.asset");Need(menu.Resolve(actor.stableId)?.tier==3,"Tier III menu form registered");
        foreach(var name in new[]{"Cornberg","DevelopmentRoom"})
        {
            var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/"+name+".unity");var player=Object.FindObjectsByType<BerserkerProgression>(FindObjectsInactive.Include).Single();
            Need(player.GetComponent<BerserkerCaster>()!=null&&player.GetComponent<BerserkerAbilityProvider>()!=null,"Class runtime registered: "+name);
            Need(player.GetComponent<QuestJournal>().Definitions.Contains(quest),"Quest journal registered: "+name);
            Need(player.GetComponent<ActorResourceController>().Profiles.Contains(rage),"Rage profile registered: "+name);
            if(name=="Cornberg")
            {
                var teacher=Object.FindObjectsByType<BerserkerMentor>(FindObjectsInactive.Include).Single();
                Need(teacher.Offers.Contains(quest),"Teacher marker offers correct quest");
                var nav=Object.FindObjectsByType<WorldNavigation>(FindObjectsInactive.Include).Single(n=>n.gameObject.scene==scene);var instance=NavMesh.AddNavMeshData(nav.Data);
                try{var path=new NavMeshPath();Need(NavMesh.SamplePosition(new Vector3(248,0,172),out var start,10,1)&&NavMesh.CalculatePath(start.position,teacher.ApproachPosition,1,path)&&path.status==NavMeshPathStatus.PathComplete,"Mentor approach connected to Trimery");}
                finally{instance.Remove();}
            }
        }
        EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/BerserkerStaticReview.json",JsonUtility.ToJson(new Report{checks=checks.Count,passed=checks.ToArray(),motions=motions},true));Debug.Log("DICEFREE_BERSERKER_STATIC_OK "+checks.Count);
    }
}
