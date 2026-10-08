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
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;
using static DiceFree.EditorTools.CornbergEconomyAuthoring;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad] internal static class CornbergEconomyValidation
    {
        const string Key="DiceFree.EconomyTest.";
        static IEnumerator flow;
        static double deadline;
        static ManifestationPersistence persistence;
        static CombatActor player;
        static CarriedInventory inventory;
        static Equipment equipment;
        static GoldWallet wallet;
        static ItemShop shop;
        static int checks;
        static CornbergEconomyValidation() {EditorApplication.playModeStateChanged+=OnPlay;}
        [CliCommand("dicefree.economy.playtest","Isolated-save merchant purchase, exact damage, visibility and reload integration test.")]
        public static object Start()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Requires idle Edit Mode.");
            for(int i=0;i<SceneManager.sceneCount;i++)Require(!SceneManager.GetSceneAt(i).isDirty,"Save scene changes first.");
            string root=Path.Combine(Path.GetTempPath(),"DiceFree-Economy-"+Guid.NewGuid().ToString("N"));
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
            SessionState.SetString(Key+"root",root);SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"error","");SessionState.SetBool(Key+"active",true);
            EditorSceneManager.OpenScene(ScenePath);EditorApplication.EnterPlaymode();return Status();
        }
        [CliCommand("dicefree.economy.status","Read isolated economy integration result.")]
        public static object Status()=>new {status=SessionState.GetString(Key+"status","idle"),success=SessionState.GetString(Key+"status","")=="passed",marker=SessionState.GetString(Key+"status","")=="passed"?"CORNBERG_ECONOMY_PLAYTEST_OK":"",checks=SessionState.GetInt(Key+"checks",0),error=SessionState.GetString(Key+"error",""),temporarySaveRoot=SessionState.GetString(Key+"root","")};
        static void OnPlay(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Key+"active",false))return;
            checks=0;flow=Flow();deadline=EditorApplication.timeSinceStartup+90;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;
        }
        static void Tick() {try {Require(EditorApplication.timeSinceStartup<deadline,"Economy test timeout.");if(!flow.MoveNext())Finish(null);}catch(Exception e){Finish(e.ToString());}}
        static void Log(string message,string stack,LogType type)
        {if(type!=LogType.Error&&type!=LogType.Exception&&type!=LogType.Assert)return;if(message.Contains("ArgumentOutOfRangeException")&&stack.Contains("SearchDatabase"))return;Finish(message+"\n"+stack);}
        static void Finish(string error)
        {EditorApplication.update-=Tick;Application.logMessageReceived-=Log;SessionState.SetBool(Key+"active",false);SessionState.SetString(Key+"status",error==null?"passed":"failed");SessionState.SetString(Key+"error",error??"");SessionState.SetInt(Key+"checks",checks);if(error==null)Debug.Log("CORNBERG_ECONOMY_PLAYTEST_OK checks="+checks);EditorApplication.ExitPlaymode();}
        static void Check(bool condition,string message) {Require(condition,message);checks++;}
        static void Near(float actual,float expected,string label)=>Check(Mathf.Abs(actual-expected)<.0001f,label+": "+actual+" != "+expected);
        static IEnumerator Bind()
        {
            do {yield return null;persistence=UnityEngine.Object.FindAnyObjectByType<ManifestationPersistence>();}while(persistence==null||!persistence.Ready);
            player=persistence.GetComponent<CombatActor>();inventory=player.GetComponent<CarriedInventory>();equipment=player.GetComponent<Equipment>();wallet=player.GetComponent<GoldWallet>();shop=UnityEngine.Object.FindAnyObjectByType<ItemShop>();
            foreach(var ai in UnityEngine.Object.FindObjectsByType<AggroBehaviour>())ai.enabled=false;
        }
        static IEnumerator Flow()
        {
            for(var b=Bind();b.MoveNext();)yield return null;
            Check(inventory.Items.Length==0 && wallet.Gold==0,"Fresh isolated ownership/currency.");
            Check(player.Motor.Teleport(shop.ApproachPosition+Vector3.back*1.5f),"Merchant NavMesh approach.");
            var interactor=player.GetComponent<Interactor>();
            Check(shop.CanInteract(player),"Merchant in reachable interaction range.");
            var collider=shop.GetComponent<Collider>();
            var ray=new Ray(collider.bounds.center+Vector3.back*4,Vector3.forward);
            Check(interactor.ContextInteract(ray),"Right-click ray interaction failed.");
            for(int i=0;i<5;i++)yield return null;
            Check(interactor.Active==shop,"Interactor did not open shop.");
            Check(inventory.Items.Length==0&&wallet.Gold==0,"Opening shop granted something.");
            string message;
            Check(!shop.Purchase(player,0,out message)&&message=="Not enough gold."&&inventory.Items.Length==0&&wallet.Gold==0,"Insufficient purchase mutated ownership/gold.");
            var offer=shop.Offers[0];int price=offer.goldPrice;
            wallet.Grant(price);
            bool reentrant=true;
            Action callback=()=>{reentrant=shop.Purchase(player,0,out _);};wallet.Changed+=callback;
            Check(shop.Purchase(player,0,out message),"Funded purchase failed.");wallet.Changed-=callback;
            Check(!reentrant&&wallet.Gold==0&&inventory.Items.Length==1,"Reentrant purchase or incorrect payment.");
            Check(!shop.Purchase(player,0,out message)&&inventory.Items.Length==1,"Repeated unfunded purchase granted item.");
            string instance=inventory.Items.Single().instanceId;
            var stats=player.Stats;var attack=stats.Definition.basicAttack;
            var spell=UnityEngine.Object.Instantiate(attack);spell.stableId="test.spell";spell.channel=DamageChannel.Magical;
            stats.SetBasicAttackDamagePercentContribution("test.scaling",50);
            float min=DamageResolver.Calculate(stats,stats,attack,rawMultiplier:attack.minimumDamageMultiplier).baseRaw;
            float max=DamageResolver.Calculate(stats,stats,attack,rawMultiplier:attack.maximumDamageMultiplier).baseRaw;
            float magic=DamageResolver.Calculate(stats,stats,spell).baseRaw;
            Check(equipment.Equip(instance,EquipmentSlot.MainHand),"Dagger equip failed.");
            Near(DamageResolver.Calculate(stats,stats,attack,rawMultiplier:attack.minimumDamageMultiplier).baseRaw,min+1,"Scaled minimum +1");
            Near(DamageResolver.Calculate(stats,stats,attack,rawMultiplier:attack.maximumDamageMultiplier).baseRaw,max+1,"Scaled maximum +1");
            Near(DamageResolver.Calculate(stats,stats,spell).baseRaw,magic,"Spell unchanged");
            var binding=player.GetComponent<EquipmentPresentation>().Bindings.Single(b=>b.definition.stableId==Id);
            Check(binding.visuals.Single().activeInHierarchy && binding.visuals.Single().GetComponentsInChildren<Renderer>().Length==4,"Equipped 3D dagger invisible.");
            CaptureEquipped();
            Check(equipment.Unequip(EquipmentSlot.MainHand.ToString()),"Unequip failed.");
            Check(!binding.visuals.Single().activeSelf,"Unequipped dagger still visible.");
            Near(DamageResolver.Calculate(stats,stats,attack,rawMultiplier:attack.minimumDamageMultiplier).baseRaw,min,"Unequip restores damage");
            stats.RemoveBasicAttackDamagePercentContribution("test.scaling");UnityEngine.Object.Destroy(spell);
            Check(equipment.Equip(instance,EquipmentSlot.MainHand),"Re-equip failed.");
            wallet.Grant(price*2);Check(shop.Purchase(player,0,out message),"Intentional re-buy failed.");
            Check(inventory.Items.Length==2&&wallet.Gold==price&&inventory.Items.Select(i=>i.instanceId).Distinct().Count()==2,"Re-buy ownership/payment incorrect.");
            interactor.Cancel();Check(interactor.Active==null,"Close failed.");
            Check(persistence.Flush(),"Isolated save flush failed.");
            EditorSceneManager.LoadSceneInPlayMode(ScenePath,new LoadSceneParameters(LoadSceneMode.Single));
            for(var b=Bind();b.MoveNext();)yield return null;
            Check(inventory.Items.Length==2&&wallet.Gold==price&&inventory.Find(instance)!=null,"Reload duplicated/lost purchases.");
            Check(equipment.Slots.Single().instanceId==instance&&inventory.Resolve(Id)!=null,"Equipped persisted item unresolved.");
            Check(player.GetComponent<EquipmentPresentation>().Bindings.Single(b=>b.definition.stableId==Id).visuals.Single().activeInHierarchy,"Reload presentation missing.");
            // Same persistent Cornberg hero travels through additive dungeon maps; no replacement/grant.
            foreach(string name in new[]{"SlimeDungeon","DungeonRewardRoom"})
            {
                var load=SceneManager.LoadSceneAsync(name,LoadSceneMode.Additive);while(!load.isDone)yield return null;
                Check(inventory.Resolve(Id)!=null&&inventory.Items.Length==2&&equipment.IsResolved(equipment.Slots.Single()),name+" catalog/ownership lost.");
                Check(UnityEngine.Object.FindObjectsByType<ItemShop>(FindObjectsInactive.Include,FindObjectsSortMode.None).All(s=>s.gameObject.scene.name=="Cornberg"),"Shop created in dungeon.");
                var unload=SceneManager.UnloadSceneAsync(name);while(!unload.isDone)yield return null;
            }
            Check(persistence.Flush(),"Final isolated save flush failed.");
        }
        static void CaptureEquipped()
        {
            var go=new GameObject("Economy validation camera");var camera=go.AddComponent<Camera>();
            RenderTexture target=null;Texture2D image=null;var prior=RenderTexture.active;
            try
            {
                camera.enabled=false;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.12f,.16f,.18f);
                camera.transform.position=player.transform.position+new Vector3(2.5f,2.1f,-3);
                camera.transform.LookAt(player.transform.position+Vector3.up*1.05f);camera.fieldOfView=32;
                target=new RenderTexture(1280,960,24);camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                image=new Texture2D(1280,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,960),0,0);image.Apply();
                Directory.CreateDirectory("Logs");File.WriteAllBytes("Logs/CornbergEconomyEquipped.png",image.EncodeToPNG());
            }
            finally {RenderTexture.active=prior;if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}if(image!=null)UnityEngine.Object.Destroy(image);UnityEngine.Object.Destroy(go);}
        }
    }
}
