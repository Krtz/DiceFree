using System;
using System.Collections;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;
using static DiceFree.EditorTools.VillageArtAuthoring;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad] internal static class VillageArtValidation
    {
        const string Key="DiceFree.VillageArtTest.";
        static IEnumerator flow;
        static double deadline;
        static int checks;
        static VillageArtValidation() => EditorApplication.playModeStateChanged += OnPlay;
        [CliCommand("dicefree.village-art.playtest", "Isolated-save equipment/class/pose and ambient-life integration with camera captures.")]
        public static object Start()
        {
            EditMode();
            string root=Path.Combine(Path.GetTempPath(),"DiceFree-VillageArt-"+Guid.NewGuid().ToString("N"));
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
            SessionState.SetString(Key+"root",root); SessionState.SetString(Key+"status","running"); SessionState.SetString(Key+"error",""); SessionState.SetBool(Key+"active",true);
            EditorSceneManager.OpenScene(CornbergEconomyAuthoring.ScenePath); EditorApplication.EnterPlaymode(); return Status();
        }
        [CliCommand("dicefree.village-art.status", "Read isolated-save equipment and village life verification result.")]
        public static object Status() => new {status=SessionState.GetString(Key+"status","idle"),success=SessionState.GetString(Key+"status","")=="passed",checks=SessionState.GetInt(Key+"checks",0),error=SessionState.GetString(Key+"error",""),temporarySaveRoot=SessionState.GetString(Key+"root","")};
        static void OnPlay(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.ExitingPlayMode&&SessionState.GetBool(Key+"active",false)) Finish("Play Mode interrupted.",false);
            if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Key+"active",false))return;
            checks=0;flow=Flow();deadline=EditorApplication.timeSinceStartup+90;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;
        }
        static void Tick() {try {Require(EditorApplication.timeSinceStartup<deadline,"Village art test timeout.");if(!flow.MoveNext())Finish(null);}catch(Exception e){Finish(e.ToString());}}
        static void Log(string message,string stack,LogType type)
        {if(type!=LogType.Error&&type!=LogType.Exception&&type!=LogType.Assert)return;if(message.Contains("ArgumentOutOfRangeException")&&stack.Contains("SearchDatabase"))return;Finish(message+"\n"+stack);}
        static void Finish(string error,bool stop=true)
        {
            EditorApplication.update-=Tick;Application.logMessageReceived-=Log;SessionState.SetBool(Key+"active",false);SessionState.SetString(Key+"status",error==null?"passed":"failed");SessionState.SetString(Key+"error",error??"");SessionState.SetInt(Key+"checks",checks);
            if(error==null)Debug.Log("VILLAGE_ART_PLAYTEST_OK checks="+checks);if(stop)EditorApplication.ExitPlaymode();
        }
        static void Check(bool value,string message) {Require(value,message);checks++;}
        static IEnumerator Flow()
        {
            ManifestationPersistence persistence;
            do {yield return null;persistence=UnityEngine.Object.FindAnyObjectByType<ManifestationPersistence>();}while(persistence==null||!persistence.Ready);
            var hero=persistence.GetComponent<CombatActor>();var inv=hero.GetComponent<CarriedInventory>();var eq=hero.GetComponent<Equipment>();var pres=hero.GetComponent<EquipmentPresentation>();
            foreach(var ai in UnityEngine.Object.FindObjectsByType<AggroBehaviour>())ai.enabled=false;
            Check(inv.Items.Length==0,"Expected isolated empty inventory.");
            var definitions=Definitions();var items=definitions.Select(d=>inv.Grant(d.stableId)).ToArray();
            var oldGlove=pres.Bindings.Single(b=>b.slot==EquipmentSlot.Hands&&!Ids.Contains(b.definition.stableId));
            var oldGloveItem=inv.Grant(oldGlove.definition.stableId);
            var dagger=inv.Grant(CornbergEconomyAuthoring.Id);
            var pants=pres.Bindings.First(b=>b.slot==EquipmentSlot.Legs);var pantsItem=inv.Grant(pants.definition.stableId);
            var merchant=UnityEngine.Object.FindAnyObjectByType<ItemShop>();var merchantPosition=merchant.transform.position;var merchantRotation=merchant.transform.rotation;
            Check(hero.Motor.Teleport(merchant.ApproachPosition+Vector3.back*2),"Capture approach unavailable.");
            foreach(string classId in new[]{"class.novice","class.physically-blessed-novice","class.magically-touched-novice"})
            {
                var actorDefinition=AssetDatabase.FindAssets("t:ActorDefinition").Select(g=>AssetDatabase.LoadAssetAtPath<ActorDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(d=>d.stableId==classId);
                hero.Stats.Configure(actorDefinition);
                for(int f=0;f<5;f++)yield return null;
                Check(eq.Equip(pantsItem.instanceId,EquipmentSlot.Legs),"Older pants equip failed.");
                Check(eq.Equip(dagger.instanceId,EquipmentSlot.MainHand),"Older dagger equip failed.");
                for(int i=0;i<4;i++)
                {
                    bool allowed=definitions[i].allowedClassIds.Length==0||definitions[i].allowedClassIds.Contains(classId);
                    Check(eq.Equip(items[i].instanceId,definitions[i].slot)==allowed,"Class restriction changed.");
                    if(allowed) Check(pres.Bindings.Single(b=>b.definition==definitions[i]).visuals.Any(v=>v.activeInHierarchy),"Equipped art absent on active form.");
                }
                Check(pants.visuals.All(v=>v.activeSelf),"New gear hides older pants.");
                Check(pres.Bindings.Single(b=>b.definition.stableId==CornbergEconomyAuthoring.Id).visuals.Single().activeSelf,"New gear hides dagger.");
                Check(!oldGlove.visuals.Any(v=>v.activeSelf),"Older gloves overlap selected slimy gloves.");
                var animator=hero.GetComponent<NovicePresentationDriver>().VisualAnimator;
                foreach(var v in pres.Bindings.Single(b=>b.definition==definitions[2]).visuals.Where(v=>v.activeInHierarchy))
                {
                    var socket=v.transform.parent;var hand=socket.parent;
                    Check(hand==animator.GetBoneTransform(v.name=="LeftGlove"?HumanBodyBones.LeftHand:HumanBodyBones.RightHand),"Glove attached to wrong bone.");
                    Check(Vector3.Distance(socket.position,hand.position)<.001f,"Glove cuff misses wrist.");
                }
                hero.GetComponent<NovicePresentationDriver>().PlayAttackAnimation();
                for(int f=0;f<12;f++)yield return null;
                animator.Play("Base Layer.Idle",0,0);animator.Update(0);
                Capture(hero.transform,"Logs/VillageArt_"+classId+".png");
                Check(eq.Equip(oldGloveItem.instanceId,EquipmentSlot.Hands),"Older gloves cannot replace new gloves.");
                Check(oldGlove.visuals.All(v=>v.activeSelf)&&pres.Bindings.Single(b=>b.definition==definitions[2]).visuals.All(v=>!v.activeSelf),"Same-slot visual transition inconsistent.");
                Check(eq.Unequip("Hands"),"Glove unequip failed.");Check(oldGlove.visuals.All(v=>!v.activeSelf),"Older glove remains after unequip.");
                foreach(var slot in eq.Slots)eq.Unequip(slot.slotId);
                Check(pres.Bindings.Where(b=>Ids.Contains(b.definition.stableId)).SelectMany(b=>b.visuals).All(v=>!v.activeSelf),"Dungeon art remains after unequip.");
            }
            var idles=UnityEngine.Object.FindObjectsByType<VillageVisualIdle>(FindObjectsSortMode.None);
            foreach(var idle in idles)
            {
                idle.enabled=false; var p=idle.Visual.localPosition;var r=idle.Visual.localRotation;var s=idle.Visual.localScale;var root=idle.transform.position;
                idle.enabled=true;idle.Sample(1);var pose=idle.Visual.localRotation;idle.Sample(2);
                Check(Quaternion.Angle(pose,idle.Visual.localRotation)>.0001f,"Idle does not animate.");
                Check(idle.transform.position==root,"Idle moves NPC root.");
                idle.enabled=false;Check(Vector3.Distance(p,idle.Visual.localPosition)<.00001f&&Quaternion.Angle(r,idle.Visual.localRotation)<.001f&&s==idle.Visual.localScale,"Idle does not restore baseline.");idle.enabled=true;
            }
            var walkers=UnityEngine.Object.FindObjectsByType<VillageHomeWander>(FindObjectsSortMode.None);int moves=0;
            foreach(var walker in walkers)
            {
                var before=walker.transform.position;bool onNav=NavMesh.SamplePosition(before,out _,.65f,NavMesh.AllAreas);
                if(onNav)
                {
                    Check(walker.TryChoosePath(),"No valid small home path for "+walker.name);
                    for(int i=0;i<2400;i++)
                    {
                        var prior=walker.transform.position;walker.Step(.05f);
                        Check(Vector3.Distance(prior,walker.transform.position)<.076f,"Wander teleported.");
                        Check(Vector3.ProjectOnPlane(walker.transform.position-walker.Home,Vector3.up).magnitude<=walker.Radius+.001f,"Wander escaped home.");
                    }
                    if(Vector3.Distance(before,walker.transform.position)>.1f)moves++;
                }
                else {walker.TryChoosePath();for(int i=0;i<100;i++)walker.Step(.1f);Check(walker.transform.position==before,"Missing NavMesh must remain stationary.");}
            }
            Check(moves>0,"No villagers actually walked.");
            Check(merchant.transform.position==merchantPosition&&Quaternion.Angle(merchant.transform.rotation,merchantRotation)<.001f,"Merchant moved/rotated.");
            Check(merchant.GetComponent<VillageHomeWander>()==null&&merchant.Offers.Single(o=>o.item.stableId==CornbergEconomyAuthoring.Id).goldPrice==15,"Shop changed.");
            Check(merchant.CanInteract(hero),"Merchant no longer interactable.");
            var fallback=new GameObject("No NavMesh fallback test");fallback.transform.position=new Vector3(10000,10000,10000);var noNav=fallback.AddComponent<VillageHomeWander>();var fallbackPosition=fallback.transform.position;
            Check(!noNav.TryChoosePath(),"Unexpected remote NavMesh.");for(int i=0;i<500;i++)noNav.Step(.1f);Check(fallback.transform.position==fallbackPosition,"Fallback moved without NavMesh.");UnityEngine.Object.Destroy(fallback);
            SessionState.SetInt(Key+"walkersMoved",moves);
        }
        static void Capture(Transform hero,string path)
        {
            var go=new GameObject("Village art validation camera");var camera=go.AddComponent<Camera>();var target=hero.position+Vector3.up*1.1f;
            go.transform.position=target+new Vector3(3,2.1f,4);go.transform.LookAt(target);camera.orthographic=true;camera.orthographicSize=1.65f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.12f,.17f,.18f);
            var objects=hero.GetComponentsInChildren<Renderer>(true).Select(r=>r.gameObject).Distinct().ToArray();var layers=objects.Select(o=>o.layer).ToArray();
            foreach(var obj in objects)obj.layer=30;camera.cullingMask=1<<30;
            var rt=RenderTexture.GetTemporary(1024,1024,24);var previous=RenderTexture.active;var texture=new Texture2D(1024,1024,TextureFormat.RGB24,false);
            try {camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1024,1024),0,0);texture.Apply();Directory.CreateDirectory("Logs");File.WriteAllBytes(path,texture.EncodeToPNG());}
            finally {for(int i=0;i<objects.Length;i++)objects[i].layer=layers[i];camera.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.Destroy(texture);UnityEngine.Object.Destroy(go);}
        }
    }
}
