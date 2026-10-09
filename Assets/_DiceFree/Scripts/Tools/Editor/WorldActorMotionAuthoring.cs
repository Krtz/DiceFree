using System;
using System.Linq;
using DiceFree.World;
using DiceFree.Dungeons;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools {
    internal static class WorldActorMotionAuthoring {
        private const string Cornberg="Assets/_DiceFree/Scenes/Cornberg.unity";

        private static VillageCharacterGestures.Personality Personality(string name) {
            if(name.Contains("Farmer")) return VillageCharacterGestures.Personality.Farmer;
            if(name.Contains("Merchant")) return VillageCharacterGestures.Personality.Merchant;
            if(name.Contains("Banker")) return VillageCharacterGestures.Personality.Banker;
            if(name.Contains("Alchemist")) return VillageCharacterGestures.Personality.Alchemist;
            if(name.Contains("Blacksmith")) return VillageCharacterGestures.Personality.Blacksmith;
            if(name.Contains("Guard")) return VillageCharacterGestures.Personality.Guard;
            if(name.Contains("Herbalist")) return VillageCharacterGestures.Personality.Herbalist;
            return VillageCharacterGestures.Personality.Villager;
        }

        private static int Seed(Transform transform) {
            var pos=transform.position;
            var text=transform.name+"|"+Mathf.RoundToInt(pos.x*100)+":"+Mathf.RoundToInt(pos.z*100);
            unchecked {int hash=17; foreach(var c in text)hash=hash*31+c;return hash;}
        }

        [CliCommand("dicefree.motion.npcs.install", "Add character-specific secondary gestures to all authored Cornberg villagers.")]
        public static object Install() {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit mode only");
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)
                throw new InvalidOperationException("Unsaved user scene, refusing to modify");
            var previous=EditorSceneManager.GetSceneManagerSetup();
            try {
                var scene=EditorSceneManager.OpenScene(Cornberg,OpenSceneMode.Single);
                var npcs=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<VillageVisualIdle>(true)).ToArray();
                if(npcs.Length<8)throw new InvalidOperationException("Unexpected village NPC count "+npcs.Length);
                int added=0, walking=0, props=0;
                foreach(var npc in npcs) {
                    var component=npc.GetComponent<VillageCharacterGestures>();
                    if(component==null){component=npc.gameObject.AddComponent<VillageCharacterGestures>();added++;}
                    component.Configure(Personality(npc.name),Seed(npc.transform));
                    if(npc.GetComponent<VillageHomeWander>()!=null)walking++;
                    if(component.HasSecondaryProp)props++;
                    EditorUtility.SetDirty(component);
                }
                if(added>0) {
                    EditorSceneManager.MarkSceneDirty(scene);
                    if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Scene save failed");
                }
                return new{success=true,npcs=npcs.Length,added,walkers=walking,secondaryProps=props,scene=Cornberg};
            }finally{EditorSceneManager.RestoreSceneManagerSetup(previous);}
        }

        [CliCommand("dicefree.motion.npcs.validate", "Validate all Cornberg NPC motions and existing slime animation components.")]
        public static object Validate() {
            var previous=EditorSceneManager.GetSceneManagerSetup();
            try {
                var scene=EditorSceneManager.OpenScene(Cornberg,OpenSceneMode.Single);
                var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
                var npcs=all.Select(t=>t.GetComponent<VillageVisualIdle>()).Where(v=>v!=null).ToArray();
                var slimes=all.Select(t=>t.GetComponent<SlimeVisualMotion>()).Where(v=>v!=null).ToArray();
                if(npcs.Length!=9)throw new InvalidOperationException("Expected 9 NPCs, found "+npcs.Length);
                if(slimes.Length<35)throw new InvalidOperationException("Expected 35+ slimes, found "+slimes.Length);
                if(npcs.Any(n=>n.GetComponent<VillageCharacterGestures>()==null))
                    throw new InvalidOperationException("Missing NPC gesture driver");
                if(npcs.Any(n=>n.Visual==null||n.Visual.name!="VillageIdleVisual"))
                    throw new InvalidOperationException("Wrong NPC visual root");
                var counts=Enum.GetValues(typeof(VillageCharacterGestures.Personality)).Cast<VillageCharacterGestures.Personality>()
                    .Select(role=>new{role=role.ToString(),count=npcs.Count(n=>n.GetComponent<VillageCharacterGestures>().Role==role)}).ToArray();
                return new{success=true,npcs=npcs.Length,slimes=slimes.Length,walkers=npcs.Count(n=>n.GetComponent<VillageHomeWander>()!=null),roles=counts};
            }finally{EditorSceneManager.RestoreSceneManagerSetup(previous);}
        }
    }
}
