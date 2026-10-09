using System;
using System.Linq;
using DiceFree.Core;
using DiceFree.Dungeons;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
    public static class PlaytestFixesAuthoring
    {
        const string Path = "Assets/_DiceFree/Scenes/Cornberg.unity";
        static T[] All<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
        static void Require(bool value,string message) { if(!value)throw new InvalidOperationException(message); }
        static T Ensure<T>(GameObject obj) where T:Component {var c=obj.GetComponent<T>();return c==null?obj.AddComponent<T>():c;}
        [CliCommand("dicefree.playtest-fixes.inspect", "Read authored cave geometry and trees without changing assets.")]
        public static object Inspect()
        {
            var scene=SceneManager.GetSceneByPath(Path);Require(scene.isLoaded,"Open Cornberg first.");
            var cave=All<Transform>(scene).Single(t=>t.name=="08 - Slime dungeon entrance - exterior only");
            return new {cave=cave.position.ToString(),children=cave.GetComponentsInChildren<Transform>(true).Select(t=>new {t.name,position=t.position.ToString(),scale=t.lossyScale.ToString()}).ToArray(),trees=All<Transform>(scene).Where(t=>t.name.StartsWith("Trees/Tree_")).Take(4).Select(t=>new {t.name,position=t.position.ToString(),bounds=t.GetComponentsInChildren<Renderer>().Select(r=>r.bounds.ToString()).ToArray()}).ToArray()};
        }
        [CliCommand("dicefree.playtest-fixes.install", "Scoped idempotent cave, NPC, trunk and camera fixes. No scene rebuild.")]
        public static object Install()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Stop Play Mode first.");
            for(int i=0;i<SceneManager.sceneCount;i++)Require(!SceneManager.GetSceneAt(i).isDirty,"Save existing scene changes first.");
            var scene=SceneManager.GetSceneByPath(Path);Require(scene.isLoaded,"Open existing Cornberg first.");
            var cave=All<Transform>(scene).Single(t=>t.name=="08 - Slime dungeon entrance - exterior only");
            var portals=All<DungeonPortal>(scene);
            var source=portals.FirstOrDefault(p=>p.definition!=null&&p.tuning!=null);Require(source!=null,"Existing portal configuration missing.");
            var mouth=cave.Find("Playtest cave interaction");
            if(mouth==null)
            {
                var sealedEntrance=cave.Find("Dark sealed entrance");Require(sealedEntrance!=null,"Original cave mouth missing.");
                var b=sealedEntrance.GetComponent<Renderer>().bounds;
                var probe=new Vector3(b.center.x,0,b.min.z-2);
                var nav=All<WorldNavigation>(scene).Single();var instance=NavMesh.AddNavMeshData(nav.Data);
                try { Require(NavMesh.SamplePosition(probe,out var sample,2,NavMesh.AllAreas),"No reachable navigation at original cave mouth.");mouth=new GameObject("Playtest cave interaction").transform;mouth.SetParent(cave,false);mouth.position=sample.position; }
                finally{if(instance.valid)instance.Remove();}
            }
            var portal=Ensure<DungeonPortal>(cave.gameObject);
            portal.definition=source.definition;portal.tuning=source.tuning;portal.enabled=true;portal.ConfigureName("Slime Dungeon - enter shared staging");portal.ConfigureApproach(mouth);
            mouth.gameObject.layer=11;
            var click=Ensure<CapsuleCollider>(mouth.gameObject);click.radius=1.1f;click.height=2.5f;click.center=Vector3.up*1.25f;click.isTrigger=false;
            foreach(var duplicate in portals.Where(p=>p!=portal))
            {
                duplicate.enabled=false;foreach(var c in duplicate.GetComponents<Collider>())c.enabled=false;
                // Hide only the stale entry instruction; retain every scenery mesh and transform.
                foreach(var label in duplicate.GetComponentsInChildren<TextMesh>(true))if(label.text.Contains("enter"))label.gameObject.SetActive(false);
            }
            var obsolete=mouth.GetComponent<DungeonPortal>();if(obsolete!=null)UnityEngine.Object.DestroyImmediate(obsolete);
            click.enabled=true;
            int npcs=0,trees=0;
            foreach(var idle in All<VillageVisualIdle>(scene))
            {
                var root=idle.transform;var bounds=BoundsOf(root);var scale=root.lossyScale;
                root.gameObject.layer=11;
                var capsule=Ensure<CapsuleCollider>(root.gameObject);
                capsule.center=root.InverseTransformPoint(new Vector3(bounds.center.x,bounds.min.y+Mathf.Min(bounds.size.y,2)*.5f,bounds.center.z));
                capsule.height=Mathf.Min(bounds.size.y,2)/Mathf.Abs(scale.y);capsule.radius=Mathf.Clamp(Mathf.Min(bounds.size.x,bounds.size.z)*.3f,.25f,.45f)/Mathf.Max(Mathf.Abs(scale.x),Mathf.Abs(scale.z));capsule.isTrigger=false;capsule.enabled=true;
                var obstacle=Ensure<NavMeshObstacle>(root.gameObject);obstacle.shape=NavMeshObstacleShape.Capsule;obstacle.center=capsule.center;obstacle.radius=capsule.radius;obstacle.height=capsule.height;obstacle.carving=root.GetComponent<VillageHomeWander>()==null;
                idle.ConfigureVisibleMotion();EditorUtility.SetDirty(idle);npcs++;
            }
            foreach(var tree in All<Transform>(scene).Where(t=>t.name.StartsWith("Trees/Tree_",StringComparison.Ordinal)))
            {
                ConfigureTree(tree);trees++;
            }
            foreach(var camera in All<ExplorationCamera>(scene)){camera.ConfigureZoom(8,110,.60f);EditorUtility.SetDirty(camera);}
            foreach(var actor in All<DiceFree.Combat.CombatActor>(scene))
            {
                if(actor.GetComponent<DiceFree.Characters.TraversalInput>()!=null){Ensure<OverworldCodexCredit>(actor.gameObject);Ensure<DiceFree.UI.QuestJournalPanel>(actor.gameObject);Ensure<ReturnToStartMenu>(actor.gameObject);}
                else if(actor.GetComponent<DiceFree.Combat.ActorStats>()?.Definition?.familyId=="enemy-family.slime"||actor.name.Contains("Slime"))
                {Ensure<SlimeVisualMotion>(actor.gameObject);Ensure<DiceFree.Combat.DefeatReporter>(actor.gameObject);}
            }
            var tuning=portal.tuning;tuning.minibossFragmentReferenceHp=160;tuning.bossFragmentReferenceHp=420;
            tuning.minibossHp=800;tuning.bossHp=3000;tuning.bossFragmentHp=300;tuning.minibossScale=6.9f;tuning.miniboss.baseHp=tuning.minibossHp;tuning.boss.baseHp=tuning.bossHp;
            EditorUtility.SetDirty(tuning);EditorUtility.SetDirty(tuning.miniboss);EditorUtility.SetDirty(tuning.boss);
            var physical=AssetDatabase.LoadAssetAtPath<DiceFree.Combat.ActorDefinition>("Assets/_DiceFree/Settings/Progression/Physically Blessed Novice shell.asset");
            Require(physical!=null,"Physical class definition missing.");physical.basicAttackMinimumOffset=-3;physical.basicAttackMaximumOffset=3;EditorUtility.SetDirty(physical);
            EditorSceneManager.MarkSceneDirty(scene);Require(EditorSceneManager.SaveScene(scene),"Cornberg save failed.");
            var dungeon=SceneManager.GetSceneByPath("Assets/_DiceFree/Scenes/SlimeDungeon.unity");bool opened=!dungeon.isLoaded;int dungeonTrees=0;
            if(opened)dungeon=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/SlimeDungeon.unity",OpenSceneMode.Additive);
            try {foreach(var tree in All<Transform>(dungeon).Where(t=>t.name.StartsWith("Tree_",StringComparison.Ordinal)&&t.GetComponent<Renderer>()==null)){ConfigureTree(tree);dungeonTrees++;}
                Require(dungeonTrees>0,"Dungeon tree roots not found.");EditorSceneManager.MarkSceneDirty(dungeon);Require(EditorSceneManager.SaveScene(dungeon),"Dungeon tree save failed.");}
            finally {if(opened)EditorSceneManager.CloseScene(dungeon,true);}
            AssetDatabase.SaveAssets();return new {success=true,npcs,trees,dungeonTrees,portal=mouth.position.ToString()};
        }
        static void ConfigureTree(Transform tree)
        {
            var bounds=BoundsOf(tree);var node=tree.Find("Playtest trunk collision");
            if(node==null){node=new GameObject("Playtest trunk collision").transform;node.SetParent(tree,false);}
            // Physical trunk only; canopy fading uses renderer bounds independently.
            node.position=new Vector3(tree.position.x,bounds.min.y,tree.position.z);node.rotation=Quaternion.identity;
            var s=tree.lossyScale;node.localScale=new Vector3(1/Mathf.Abs(s.x),1/Mathf.Abs(s.y),1/Mathf.Abs(s.z));node.gameObject.layer=9;
            var trunk=Ensure<CapsuleCollider>(node.gameObject);trunk.radius=Mathf.Clamp(Mathf.Min(bounds.size.x,bounds.size.z)*.08f,.18f,.65f);trunk.height=Mathf.Clamp(bounds.size.y*.45f,1.5f,4);trunk.center=Vector3.up*trunk.height*.5f;trunk.isTrigger=false;
            foreach(var old in tree.GetComponentsInChildren<Collider>())if(old!=trunk)old.enabled=false;
            if(tree.parent!=null&&tree.parent.name=="Woodland tree")foreach(var old in tree.parent.GetComponentsInChildren<Collider>())if(old!=trunk)old.enabled=false;
            var obstacle=Ensure<NavMeshObstacle>(node.gameObject);obstacle.shape=NavMeshObstacleShape.Capsule;obstacle.center=trunk.center;obstacle.radius=trunk.radius;obstacle.height=trunk.height;obstacle.carving=true;
            Ensure<TreeCameraOccluder>(tree.gameObject);
        }
        static Bounds BoundsOf(Transform root)
        {
            var meshes=root.GetComponentsInChildren<Renderer>(true);Require(meshes.Length>0,"Missing visible geometry: "+root.name);var b=meshes[0].bounds;foreach(var r in meshes.Skip(1))b.Encapsulate(r.bounds);return b;
        }
        [CliCommand("dicefree.playtest-fixes.validate", "Validate scoped P0 scene wiring.")]
        public static object Validate()
        {
            var scene=SceneManager.GetSceneByPath(Path);Require(scene.isLoaded,"Open Cornberg.");
            var portals=All<DungeonPortal>(scene).Where(p=>p.isActiveAndEnabled).ToArray();Require(portals.Length==1&&portals[0].name=="08 - Slime dungeon entrance - exterior only","Incorrect active cave portal.");
            Require(portals[0].transform.Find("Playtest cave interaction").GetComponent<Collider>().enabled,"Portal click collider disabled.");
            foreach(var npc in All<VillageVisualIdle>(scene)){Require(npc.Visual!=null&&npc.Visual.GetComponentsInChildren<Renderer>().Length>0,"Idle has no visible geometry.");Require(npc.GetComponent<CapsuleCollider>()?.enabled==true&&npc.GetComponent<NavMeshObstacle>()!=null,"NPC collision missing.");}
            foreach(var tree in All<TreeCameraOccluder>(scene))Require(tree.transform.Find("Playtest trunk collision").GetComponent<NavMeshObstacle>().carving,"Tree navigation missing.");
            return new {success=true,marker="PLAYTEST_P0_SCENE_OK",npcs=All<VillageVisualIdle>(scene).Length,trees=All<TreeCameraOccluder>(scene).Length};
        }
    }
}
