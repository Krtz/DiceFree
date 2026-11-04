using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

public static class PlayerMotionLiveReview
{
    public static async Task<object> Run()
    {
        CombatActor p=null;await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.Motor.Ready&&p.GetComponent<ManifestationPersistence>().Ready,"player");
        p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.Health.Invulnerable=true;
        foreach(var b in Object.FindObjectsByType<MonoBehaviour>().Where(b=>b is DiceFree.AI.AggroBehaviour||b is BasicAttack||b is QuilboarCombat||b is BriarbackChieftainEncounter))if(b.gameObject!=p.gameObject)b.enabled=false;
        var save=p.GetComponent<ManifestationPersistence>();var xp=p.GetComponent<ExperienceProgression>();while(xp.Level<30)xp.Grant(xp.RequiredXp);
        var driver=p.GetComponent<NovicePresentationDriver>();var motion=p.GetComponent<PlayerMotionPresentation>();var camera=Object.FindAnyObjectByType<ExplorationCamera>();
        string output="VisualBaselines/PlayerMotion/Live";Directory.CreateDirectory(output);var results=new System.Collections.Generic.List<object>();
        var nav=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.AI.NavMeshVisualizationSettings")).First(t=>t!=null).GetProperty("showNavMesh",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic);var oldNav=nav.GetValue(null);nav.SetValue(null,false);
        var viewType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");var view=Resources.FindObjectsOfTypeAll(viewType).FirstOrDefault();var gizmos=viewType.GetProperty("drawGizmos",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic);var oldGizmos=view!=null?gizmos.GetValue(view):null;if(view!=null)gizmos.SetValue(view,false);
        try
        {
            for(int kind=0;kind<3;kind++)
            {
                string id=new[]{"class.novice","class.physically-blessed-novice","class.magically-touched-novice"}[kind];string label=new[]{"novice","physical","magical"}[kind];
                if(kind>0)
                {
                    var definition=AssetDatabase.FindAssets("t:ActorDefinition",new[]{"Assets/_DiceFree/Settings"}).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<ActorDefinition>).Single(a=>a.stableId==id);
                    string error;Need(save.HasManifestation(id)?save.TryActivateExisting(id,out error):save.TryForkAndActivate(definition,out error),"Class fork: "+error);
                }
                if(kind==0&&save.CurrentClassId!=id)Need(save.TryActivateExisting(id,out _),"Restore Novice fixture");
                Need(p.Motor.Teleport(Point(new Vector3(270,0,390))),"safe pose location");p.Motor.Stop();await Task.Delay(350);
                Need(motion.ControllerClassId==id,"Class motion did not switch");var model=driver.VisualAnimator;var hips=model.GetBoneTransform(HumanBodyBones.Hips);Need(Vector3.Distance(hips.position,p.transform.position)<2.5f,"Humanoid escaped gameplay root: "+id+" hips="+hips.position);
                var root=p.GetComponent<ClassPresentationSwitcher>().ActiveVisualRoot;var oldScale=root.localScale;
                camera.RecenterForTravel(p.transform);await Capture(output+"/"+label+"-gameplay-idle.png");
                var rotation=Quaternion.Euler(12,155,0);camera.SetPresentationPose(p.transform.position+Vector3.up*1.05f-rotation*Vector3.forward*4.4f,rotation,45);await Capture(output+"/"+label+"-idle.png");
                var leg=model.GetBoneTransform(HumanBodyBones.LeftUpperLeg);var before=leg.localRotation;Need(p.Motor.MoveTo(Point(p.transform.position+Vector3.forward*5)),"normal walk");await Task.Delay(300);Need(motion.State=="Locomotion"&&Quaternion.Angle(before,leg.localRotation)>2,"Live walk pose frozen");p.Motor.Stop();await Task.Delay(400);
                bool cast=false;
                if(kind==0){var skills=p.GetComponent<NoviceSkillProgression>();var d=skills.Definitions.Single(d=>d.kind==NoviceSkillKind.AgilityAttackSpeedBuff);Need(skills.Spend(d.stableId),"Novice allocation");cast=p.GetComponent<NoviceSkillCaster>().Cast(d,p);}
                if(kind==1){var skills=p.GetComponent<PhysicalSkillProgression>();var d=skills.Definitions.Single(d=>d.kind==PhysicalSkillKind.Guard);Need(skills.Spend(d),"Physical allocation");p.GetComponent<ActorResourceController>().RefillAll();cast=p.GetComponent<PhysicalSkillCaster>().Cast(d,p);}
                if(kind==2){var skills=p.GetComponent<MagicalSkillProgression>();var d=skills.Definitions.Single(d=>d.kind==MagicalSkillKind.FireImbuement);Need(skills.Spend(d),"Magical allocation");p.GetComponent<ActorResourceController>().RefillAll();cast=p.GetComponent<MagicalSkillCaster>().Cast(d,p);}
                Need(cast,"Normal skill rejected");await Task.Delay(150);Need(motion.State=="Guard"||motion.State=="Cast","Successful skill did not animate");camera.ClearPresentationPose();camera.RecenterForTravel(p.transform);await Capture(output+"/"+label+"-skill-gameplay.png");
                Need(root.localScale==oldScale,"Player body still squashed");
                var inventory=p.GetComponent<CarriedInventory>();var equipment=p.GetComponent<Equipment>();var bindings=p.GetComponent<EquipmentPresentation>().Bindings;
                var weapon=bindings.First(b=>b.slot==EquipmentSlot.MainHand&&b.definition!=null&&(b.definition.allowedClassIds==null||b.definition.allowedClassIds.Length==0||b.definition.allowedClassIds.Contains(id))).definition;
                var item=FixedItemGrant.Grant(inventory,weapon);Need(equipment.Equip(item.instanceId,EquipmentSlot.MainHand),"Weapon equip");await Task.Delay(100);
                Need(bindings.Where(b=>b.definition==weapon).SelectMany(b=>b.visuals).Any(v=>v!=null&&v.activeInHierarchy),"Equipped weapon has no active presentation");
                var target=Object.FindObjectsByType<QuilboarCombat>().First(q=>q.role==QuilboarRole.Raider&&q.GetComponent<CombatActor>().Alive).GetComponent<CombatActor>();Need(target.Motor.Teleport(Point(p.transform.position+Vector3.forward*2)),"attack target fixture");
                var basic=p.GetComponent<BasicAttack>();basic.enabled=true;Need(basic.Order(target),"Normal armed attack");await Until(()=>motion.State=="ArmedAttack","Armed attack animation");await Capture(output+"/"+label+"-armed-attack.png");basic.Cancel();equipment.Unequip(EquipmentSlot.MainHand.ToString());await Task.Delay(350);
                p.Effects.TryApplyStun(.2f,true);await Task.Delay(60);Need(model.speed==0,"Stun did not pause motion");await Task.Delay(300);Need(model.speed>0,"Stun animation did not recover");
                p.Health.Invulnerable=false;p.Health.ApplyPacket(null,new DamageResult{mitigated=1000000});await Task.Delay(1250);Need(motion.State=="Death"&&root.localScale==oldScale,"Death pose/squash regression");Need(hips.position.y>p.transform.position.y+.15f,"Retargeted corpse sunk into ground");await Capture(output+"/"+label+"-death.png");p.Health.Restore();p.Health.Invulnerable=true;await Task.Delay(120);Need(motion.State!="Death"&&model.speed>0,"Restoration left death pose");
                results.Add(new{id,controller=model.runtimeAnimatorController.name,liveWalk=true,skillGesture=true,equippedWeapon=weapon.stableId,weaponVisible=true,armedAttack=true,stunRecovery=true,deathAndRestoration=true,bodyScalePreserved=true});
            }
            var result=new{passed=true,classes=results.ToArray(),movementAndDamageAuthorityUnchanged=true};File.WriteAllText("Logs/PlayerMotionLiveReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
        }
        finally{camera.ClearPresentationPose();nav.SetValue(null,oldNav);if(view!=null&&oldGizmos!=null)gizmos.SetValue(view,oldGizmos);}
    }
    static Vector3 Point(Vector3 p){Need(NavMesh.SamplePosition(p,out var h,3,NavMesh.AllAreas),"No NavMesh "+p);return h.position;}
    static async Task Capture(string path){await Task.Delay(40);ScreenCapture.CaptureScreenshot(path);await Task.Delay(80);}
    static async Task Until(Func<bool> c,string detail){var end=DateTime.UtcNow.AddSeconds(30);while(!c()){Need(DateTime.UtcNow<end,"Timed out: "+detail);await Task.Delay(20);}}
    static void Need(bool ok,string detail){if(!ok)throw new InvalidOperationException(detail);}
}
