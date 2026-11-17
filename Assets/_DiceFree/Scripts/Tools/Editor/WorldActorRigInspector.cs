using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Pipeline.Commands;

namespace DiceFree.EditorTools {
 internal static class WorldActorRigInspector {
 [CliCommand("dicefree.motion.inspect-world", "Inspect authored NPCs and monster visual children safely.")]
 public static object Inspect() {
  var setup=EditorSceneManager.GetSceneManagerSetup();
  try {
   var scenes=new[]{"Cornberg","SlimeDungeon"};
   return scenes.Select(name=>{
    var s=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/"+name+".unity",OpenSceneMode.Single);
    var roots=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
    var npcs=roots.Where(t=>t.GetComponent<DiceFree.World.VillageVisualIdle>()!=null).Take(12).Select(t=>new{
      name=t.name,
      visual=t.GetComponent<DiceFree.World.VillageVisualIdle>().Visual==null?"null":t.GetComponent<DiceFree.World.VillageVisualIdle>().Visual.name,
      children=t.GetComponentsInChildren<Renderer>(true).Take(16).Select(r=>r.name).ToArray(),
      geometries=t.GetComponentsInChildren<MeshFilter>(true).Take(9).Select(m=>new{
        name=m.name,verts=m.sharedMesh==null?0:m.sharedMesh.vertexCount,
        readable=m.sharedMesh!=null&&m.sharedMesh.isReadable,
        bounds=m.sharedMesh==null?"none":m.sharedMesh.bounds.ToString(),
        position=m.transform.localPosition.ToString(),scale=m.transform.localScale.ToString()
      }).ToArray(),
      wander=t.GetComponent<DiceFree.World.VillageHomeWander>()!=null
    }).ToArray();
    var monsters=roots.Where(t=>t.GetComponent<DiceFree.Combat.CombatActor>()!=null&&
      (t.GetComponent<DiceFree.AI.EnemyVariant>()!=null||
       t.GetComponent<DiceFree.Ironpine.IronpineWildlife>()!=null||
       t.GetComponent<DiceFree.Briarback.QuilboarCombat>()!=null||
       t.GetComponent<DiceFree.Dungeons.SlimeVisualMotion>()!=null)).Select(t=>{
      var renderers=t.GetComponentsInChildren<Renderer>(true);
      var visual=t.Find("Presentation")??t.Find("Model");
      int visibleMeshes=renderers.Count(r=>r.enabled&&r.gameObject.activeInHierarchy&&
       (r is SkinnedMeshRenderer skin?skin.sharedMesh!=null:
        r is MeshRenderer? r.GetComponent<MeshFilter>()?.sharedMesh!=null:true));
      return new{
       name=t.name,
       kind=t.GetComponent<DiceFree.Ironpine.IronpineWildlife>()!=null?"Ironpine wildlife":
        t.GetComponent<DiceFree.Briarback.QuilboarCombat>()!=null?"Briarback quilboar":
        t.GetComponent<DiceFree.Dungeons.SlimeVisualMotion>()!=null?"Dungeon slime":"Cornberg enemy",
       active=t.gameObject.activeInHierarchy,
       visual=visual!=null?visual.name:
        t.GetComponentInChildren<Animator>(true)?.transform.name??"implicit mesh children",
       rendererCount=renderers.Length,
       enabledRendererCount=renderers.Count(r=>r.enabled&&r.gameObject.activeInHierarchy),
       visibleMeshCount=visibleMeshes,
       renderers=renderers.Take(12).Select(r=>new{name=r.name,active=r.gameObject.activeInHierarchy,
        enabled=r.enabled,mesh=r is SkinnedMeshRenderer skin?(skin.sharedMesh==null?"missing":skin.sharedMesh.name):
         r is MeshRenderer? r.GetComponent<MeshFilter>()?.sharedMesh?.name??"missing":"other"}).ToArray(),
       animator=t.GetComponentInChildren<Animator>(true)==null?"missing":
        t.GetComponentInChildren<Animator>(true).runtimeAnimatorController==null?"no controller":"ready",
       motion=t.GetComponent<DiceFree.Dungeons.SlimeVisualMotion>()!=null
      };
    }).ToArray();
    var dungeon=roots.Where(t=>t.GetComponent<DiceFree.Dungeons.SlimeVisualMotion>()!=null).Take(5).Select(t=>t.name).ToArray();
    return new{
     scene=name,
     npcCount=roots.Count(t=>t.GetComponent<DiceFree.World.VillageVisualIdle>()!=null),
     slimeCount=roots.Count(t=>t.GetComponent<DiceFree.Dungeons.SlimeVisualMotion>()!=null),
     monsterCount=monsters.Length,
     activeMonsterCount=monsters.Count(m=>m.active),
     activeByKind=monsters.Where(m=>m.active).GroupBy(m=>m.kind)
      .Select(group=>new{kind=group.Key,count=group.Count()}).ToArray(),
     potentialVisualFailures=monsters.Where(m=>m.active&&
      (m.enabledRendererCount==0||m.visibleMeshCount==0)).Select(m=>new{
       m.name,m.kind,m.visual,m.rendererCount,m.enabledRendererCount,m.visibleMeshCount,m.renderers
      }).ToArray()
    };
   }).ToArray();
   }finally{if(setup.Length>0)EditorSceneManager.RestoreSceneManagerSetup(setup);}
 }
 }
}
