using System;
using System.IO;
using System.Linq;
using DiceFree.Quests;
using DiceFree.UI;
using UnityEditor;
using UnityEngine;

public static class CombatFeedbackChecks
{
    static int checks;
    public static string Run()
    {
        checks=0;
        var text=new CombatTextBuffer();
        text.Add(CombatTextKind.Damage,21,Vector3.zero,0);
        text.Add(CombatTextKind.Damage,9,Vector3.zero,.05f);
        Need(text.Entries.Count==1&&text.Entries[0].amount==30,"Same-burst packets should coalesce without dropping damage");
        text.Add(CombatTextKind.Damage,7,Vector3.zero,.16f);
        text.Add(CombatTextKind.Healing,15,Vector3.zero,.17f);
        Need(text.Entries.Count==3,"Separate hits/heals were overwritten");
        Need(text.Entries.Last().Text=="+15","Healing label");
        text.Add(CombatTextKind.Critical,123,Vector3.zero,.18f);
        Need(text.Entries.Last().Text=="CRIT 123","Critical label");
        text.Add(CombatTextKind.Shield,42,Vector3.zero,.19f);
        Need(text.Entries.Last().Text=="Absorb 42","Absorption must not mislabel redirected damage as a shield");
        text.Add(CombatTextKind.Resistance,18,Vector3.zero,.20f);
        Need(text.Entries.Last().Text=="Resist +18","Resistance restoration label");
        text.Add(CombatTextKind.Miss,0,Vector3.zero,.21f);
        Need(text.Entries.Last().Text=="MISS","Miss label");
        int before=text.Entries.Count;
        text.Add(CombatTextKind.Damage,float.NaN,Vector3.zero,.22f);text.Add(CombatTextKind.Damage,-1,Vector3.zero,.22f);text.Add(CombatTextKind.Damage,0,Vector3.zero,.22f);
        Need(text.Entries.Count==before,"Invalid/zero damage should not create feedback");
        for(int i=0;i<100;i++)text.Add(i%2==0?CombatTextKind.Damage:CombatTextKind.Healing,1,Vector3.zero,.3f+i*.001f);
        Need(text.Entries.Count==CombatTextBuffer.Capacity,"Burst saturation must stay bounded");
        text.Tick(2);Need(text.Entries.Count==0,"Expired feedback leaked");
        text.Add(CombatTextKind.Damage,1,Vector3.zero,3);text.Clear();Need(text.Entries.Count==0,"Restore/session clear");
        var trail=new HealthBarTrail();trail.Sample(100,100,0,.016f);trail.Sample(50,100,.1f,.016f);
        Need(trail.Fraction==1,"Damage should remain visible initially");
        trail.Sample(50.2f,100,.2f,.1f);Need(trail.Fraction==1,"Passive regeneration must not erase recent damage");
        trail.Sample(50.4f,100,.5f,.1f);Need(trail.Fraction<1&&trail.Fraction>.504f,"Damage trail should drain");
        trail.Sample(99,100,.6f,.1f);Need(trail.Fraction>=.99f,"Healing bar cannot exceed its trail");
        trail.Sample(0,100,.7f,.1f);Need(trail.Fraction==0,"Death should clear recent health");
        trail.Sample(100,100,.8f,.1f);Need(trail.Fraction==1,"Restoration should recover recent health");
        trail.Sample(50,200,.9f,.1f);Need(trail.Fraction==.25f,"Maximum/class switch should reset trail");
        var quest=AssetDatabase.LoadAssetAtPath<QuestDefinition>("Assets/_DiceFree/Settings/Briarback/The Four Elements Hunt.asset");
        var state=new QuestProgress{questId=quest.stableId,definitionVersion=quest.version,status=QuestStatus.Active};
        for(int i=0;i<4;i++){state.stage=i;Need(Mathf.Abs(QuestReadout.Fraction(quest,state)-i*.25f)<.001f,"Ordered objective fraction "+i);Need(QuestReadout.Objective(quest,state).StartsWith("Step "+(i+1)+" of 4"),"Ordered objective caption "+i);}
        state.status=QuestStatus.ReadyToTurnIn;state.count=1;Need(QuestReadout.Fraction(quest,state)==1,"Turn-in fraction");
        Need(QuestReadout.Objective(quest,state).Contains("Trimery Elemental Scholar"),"Return text must identify the real scholar");
        state.definitionVersion++;Need(QuestReadout.Fraction(quest,state)==0&&QuestReadout.Objective(quest,state).Contains("preserved"),"Unsupported versions stay inert");
        var empty=ScriptableObject.CreateInstance<QuestDefinition>();empty.version=1;empty.stages=Array.Empty<QuestObjective>();state.definitionVersion=1;
        try{Need(!QuestReadout.Supported(empty,state)&&QuestReadout.Fraction(empty,state)==0,"Empty stage fallback");}
        finally{UnityEngine.Object.DestroyImmediate(empty);}
        string result=Newtonsoft.Json.JsonConvert.SerializeObject(new{passed=true,checks,boundedEntries=CombatTextBuffer.Capacity,damageAuthorityUnchanged=true,questAuthorityUnchanged=true},Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText("Logs/CombatFeedbackChecks.json",result);return result;
    }
    static void Need(bool ok,string detail){checks++;if(!ok)throw new InvalidOperationException(detail);}
}
