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
    var monsters=roots.Where(t=>t.GetComponent<DiceFree.Combat.CombatActor>()!=null&&t.GetComponent<DiceFree.AI.EnemyVariant>()!=null).Take(12).Select(t=>new{
      name=t.name, children=t.GetComponentsInChildren<Renderer>(true).Take(12).Select(r=>r.name).ToArray(),
      visual=t.Find("Presentation")==null?"none":t.Find("Presentation").name,
      motion=t.GetComponent<DiceFree.Dungeons.SlimeVisualMotion>()!=null
    }).ToArray();
    var dungeon=roots.Where(t=>t.GetComponent<DiceFree.Dungeons.SlimeVisualMotion>()!=null).Take(5).Select(t=>t.name).ToArray();
    return new{scene=name,npcCount=roots.Count(t=>t.GetComponent<DiceFree.World.VillageVisualIdle>()!=null),slimeCount=roots.Count(t=>t.GetComponent<DiceFree.Dungeons.SlimeVisualMotion>()!=null),npcs,monsters,dungeon};
   }).ToArray();
  }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
 }
 }
}
