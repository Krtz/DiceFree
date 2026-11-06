using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.UI;
using DiceFree.Visuals;
using UnityEngine;
using Object=UnityEngine.Object;

public static class WolfEquipmentMotionReview
{
    static void Need(bool ok,string text){if(!ok)throw new InvalidOperationException(text);}
    static async Task Frame()
    {
        var ready=new TaskCompletionSource<bool>();
        void Done(UnityEngine.Rendering.ScriptableRenderContext c,List<Camera> cameras)=>ready.TrySetResult(true);
        UnityEngine.Rendering.RenderPipelineManager.endContextRendering+=Done;
        try{Need(await Task.WhenAny(ready.Task,Task.Delay(5000))==ready.Task,"Rendered motion frame");}
        finally{UnityEngine.Rendering.RenderPipelineManager.endContextRendering-=Done;}
    }
    public static async Task<object> Run()
    {
        var p=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();
        var save=p.GetComponent<ManifestationPersistence>();
        var catalog=(ClassCatalog)typeof(ManifestationPersistence).GetField("classCatalog",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(save);
        var target=CombatActor.All.First(a=>a.Stats.Definition.stableId=="briarback.raider");
        foreach(var actor in CombatActor.All.Where(a=>a!=p))
        {
            if(actor.GetComponent<AggroBehaviour>() is Behaviour ai)ai.enabled=false;
            if(actor.GetComponent<BasicAttack>() is Behaviour attack)attack.enabled=false;
            if(actor.Motor!=null&&actor.Motor.Ready)actor.Motor.Stop();
        }
        p.Health.Invulnerable=true;target.Health.Invulnerable=true;
        var basic=p.GetComponent<BasicAttack>();var rows=new List<object>();
        try
        {
            foreach(var fixture in new[]{("class.physically-blessed-novice","packhunters-fang"),("class.magically-touched-novice","moonwell-staff"),("class.mage","starfall-wand")})
            {
                Need(save.CurrentClassId==fixture.Item1||save.TryActivateExisting(fixture.Item1,out _)||save.TryForkAndActivate(catalog.Resolve(fixture.Item1),out _),"Activate motion fixture");
                await Task.Delay(300);
                Need(p.Motor.TeleportAcrossMaps(new Vector3(270,0,415)),"Player motion position");
                Need(target.Motor.TeleportAcrossMaps(p.transform.position+Vector3.forward*(p.Radius+target.Radius+.20f)),"Target motion position");
                var inventory=p.GetComponent<CarriedInventory>();var equipment=p.GetComponent<Equipment>();string id=WolfRewardEquipmentVisuals.Prefix+fixture.Item2;
                var owned=inventory.Items.First(i=>i.definitionId==id);Need(equipment.Equip(owned.instanceId,EquipmentSlot.MainHand),"Equip motion weapon");
                var root=p.GetComponent<ClassPresentationSwitcher>().ActiveVisualRoot;
                var model=ClassPresentationSwitcher.FindVisualAnimator(root);
                var visual=p.GetComponent<EquipmentPresentation>().Bindings.Single(b=>b.definition?.stableId==id).visuals.Single(v=>v!=null&&v.transform.IsChildOf(root));
                await Task.Delay(150);await Frame();
                var before=visual.transform.rotation;var start=p.transform.position;
                Need(p.Motor.MoveTo(start+Vector3.right*3),"Walk order");await Task.Delay(250);await Frame();
                Need(p.GetComponent<PlayerMotionPresentation>().State=="Locomotion","Locomotion played with weapon");
                Need(Vector3.Distance(visual.transform.position,model.GetBoneTransform(HumanBodyBones.RightHand).position)<.003f,"Walking hand attachment");p.Motor.Stop();
                Need(p.Motor.TeleportAcrossMaps(start),"Return to target");int hits=basic.Hits,misses=basic.Misses;
                basic.enabled=true;Need(basic.Order(target),"Actual basic attack order");
                float maxRotation=0,maxError=0,maxSupport=0;bool windup=false;int samples=0;
                var deadline=DateTime.UtcNow.AddSeconds(10);
                while((basic.Hits+basic.Misses==hits+misses||samples<12)&&DateTime.UtcNow<deadline)
                {
                    await Task.Delay(40);await Frame();samples++;windup|=basic.State=="Wind-up";
                    maxRotation=Mathf.Max(maxRotation,Quaternion.Angle(before,visual.transform.rotation));
                    maxError=Mathf.Max(maxError,Vector3.Distance(visual.transform.position,model.GetBoneTransform(HumanBodyBones.RightHand).position));
                    if(equipment.UsesTwoHandedStance)
                    {
                        float error=Vector3.Distance(model.GetBoneTransform(HumanBodyBones.LeftHand).position,visual.transform.TransformPoint(Vector3.down*.22f));maxSupport=Mathf.Max(maxSupport,error);
                    }
                }
                Need(basic.Hits+basic.Misses>hits+misses&&windup,"Actual attack progressed through wind-up and resolution");
                rows.Add(new{classId=fixture.Item1,item=id,walk=true,attack=true,samples,maxDominantError=maxError,maxSupportError=maxSupport,rotationDegrees=maxRotation});
                File.WriteAllText("Logs/WolfEquipmentMotionReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{status="running",rows},Newtonsoft.Json.Formatting.Indented));
                Need(maxError<.003f&&maxSupport<.012f,"Animated grip errors "+maxError+" / "+maxSupport);
                if(!equipment.UsesTwoHandedStance)Need(maxRotation>5,"One-hand weapon rotates with attack wrist");
                basic.Cancel();basic.enabled=false;
            }
            var result=new{status="passed",rows};File.WriteAllText("Logs/WolfEquipmentMotionReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
        }
        finally{basic.Cancel();basic.enabled=false;}
    }
}
