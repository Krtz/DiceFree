using System;using System.Linq;using DiceFree.Briarback;using DiceFree.World;using DiceFree.Quests;using DiceFree.Items;using DiceFree.Combat;using DiceFree.AI;using DiceFree.Characters;using UnityEngine;using UnityEngine.AI;using UnityEditor;using UnityEditor.SceneManagement;using Unity.Pipeline.Commands;using Object=UnityEngine.Object;
namespace DiceFree.EditorTools
{
 public static class BriarbackValidation
 {
  static void Need(bool ok,string text){if(!ok)throw new InvalidOperationException(text);}
  static Vector3 Nav(Vector2 point){Need(NavMesh.SamplePosition(new Vector3(point.x,0,point.y),out var hit,1.5f,1),"No navigation at "+point);return hit.position;}
  [CliCommand("dicefree.briarback.validate","Validate region topology, thirteen creature compositions, original rig, quest and reward.")]
  public static object Run(){
   Need(!EditorApplication.isPlayingOrWillChangePlaymode,"Edit Mode only");var setup=EditorSceneManager.GetSceneManagerSetup();NavMeshDataInstance nav=default;int paths=0,moving=0;
   try{
    EditorSceneManager.OpenScene(BriarbackAuthoring.Scene);var root=GameObject.Find(BriarbackAuthoring.RootName);Need(root!=null,"Region missing");
    nav=NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
    var creatures=root.GetComponentsInChildren<QuilboarCombat>();Need(creatures.Length==13&&creatures.Count(c=>c.role==QuilboarRole.Harrowtusk)==1,"Enemy composition");
    foreach(var c in creatures){
     var actor=c.GetComponent<CombatActor>();var d=c.GetComponent<ActorStats>().Definition;
     Need(d.familyId==BriarbackAuthoring.Family&&d.basicAttack!=null&&c.GetComponent<DefeatReporter>()!=null&&c.GetComponent<OverworldRespawn>()?.Definition!=null,"Combat/credit/respawn composition");
     Need(c.GetComponent<AggroBehaviour>().AutoAggroPolicy.alwaysAutoAggro,"Quilboar must not ignore higher-level players");
     Need(c.transform.position.z-c.GetComponent<AggroBehaviour>().Leash>230,"Spawn leash reaches Trimery");
     Nav(new Vector2(c.transform.position.x,c.transform.position.z));
     Need(c.GetComponent<QuilboarPresentation>().model.runtimeAnimatorController!=null,"No creature animations");
     Need(c.GetComponentsInChildren<SkinnedMeshRenderer>().All(r=>r.sharedMesh.bindposes.Length>=20&&r.sharedMaterials.All(m=>m!=null&&m.shader.name.StartsWith("Universal Render Pipeline/"))),"Rig or URP materials");
    }
    Vector3 previous=Nav(new Vector2(250,182));
    foreach(var point in new[]{new Vector2(242,199)}.Concat(BriarbackAuthoring.Route).Concat(BriarbackAuthoring.EliteRoute)){
     var destination=Nav(point);var path=new NavMeshPath();Need(NavMesh.CalculatePath(previous,destination,1,path)&&path.status==NavMeshPathStatus.PathComplete,"Disconnected route to "+point);previous=destination;paths++;
    }
    var quest=AssetDatabase.LoadAssetAtPath<QuestDefinition>(BriarbackAuthoring.Folder+"/The Northern Trail.asset");var item=AssetDatabase.LoadAssetAtPath<ItemDefinition>(BriarbackAuthoring.Folder+"/Bristlehide Mantle.asset");
    Need(quest.stages.Length==3&&quest.stages[0].areaId==BriarbackAuthoring.WatchId&&quest.stages[1].familyId==BriarbackAuthoring.Family&&quest.stages[1].count==8&&quest.stages[2].contentId==BriarbackAuthoring.EliteId&&!quest.completeOnObjectives,"Quest sequence");
    Need(item.slot==EquipmentSlot.Back&&item.allowedClassIds.Length==0&&item.stats.attributes.vitality==3&&item.stats.physicalDefense==3&&item.stats.magicalDefense==2&&quest.reward.items.Single()==item,"Reward data");
    Need(root.GetComponent<OverworldMapBounds>().Bounds==BriarbackAuthoring.Bounds,"Map extents");
    Need(Object.FindAnyObjectByType<WorldFogOfWar>().MapMax==new Vector2(353,406),"Fog extent");
    Need(Object.FindObjectsByType<QuestGiver>().Single(g=>g.name=="Scout Runa - northern trail").Quest==quest,"Quest giver");
    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(BriarbackAuthoring.Art+"/Models/Quilboar.fbx");var model=Object.Instantiate(prefab);model.hideFlags=HideFlags.HideAndDontSave;
    try{
     var animator=model.GetComponent<Animator>();animator.enabled=false;var skin=model.GetComponentInChildren<SkinnedMeshRenderer>();var clip=AssetDatabase.LoadAllAssetsAtPath(BriarbackAuthoring.Art+"/Models/Quilboar.fbx").OfType<AnimationClip>().Single(c=>!c.name.StartsWith("__")&&c.name.EndsWith("|Locomotion"));
     var first=new Mesh();var second=new Mesh();clip.SampleAnimation(model,.12f);skin.BakeMesh(first);clip.SampleAnimation(model,.58f);skin.BakeMesh(second);
     moving=first.vertices.Zip(second.vertices,(a,b)=>Vector3.Distance(a,b)).Count(d=>d>.01f);Object.DestroyImmediate(first);Object.DestroyImmediate(second);Need(moving>100,"Creature locomotion does not deform vertices");
    }finally{Object.DestroyImmediate(model);}
    foreach(string scene in new[]{BriarbackAuthoring.Scene,"Assets/_DiceFree/Scenes/DevelopmentRoom.unity"}){
     EditorSceneManager.OpenScene(scene);var p=Object.FindAnyObjectByType<TraversalInput>(FindObjectsInactive.Include);Need(p.GetComponent<CarriedInventory>().Resolve(item.stableId)==item&&p.GetComponent<QuestJournal>().Definitions.Contains(quest)&&p.GetComponent<BriarbackMantleVisual>()!=null,"Player content catalog");
    }
    return new{marker="DICEFREE_BRIARBACK_DATA_OK",creatures=13,paths,movingVertices=moving,questStages=3,rewardSlots=1};
   }finally{if(nav.valid)nav.Remove();EditorSceneManager.RestoreSceneManagerSetup(setup);}
  }
 }
}
