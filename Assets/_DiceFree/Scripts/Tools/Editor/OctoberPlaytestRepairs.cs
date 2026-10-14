using System;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Dungeons;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
    public static class OctoberPlaytestRepairs
    {
        internal static void RepairOuterNavigationEdges(Transform[] all)
        {
            foreach(var transform in all.Where(t=>t.parent!=null && t.parent.name=="Physical outer edge"))
            {
                var obstacle=transform.GetComponent<UnityEngine.AI.NavMeshObstacle>();
                if(obstacle==null)continue;
                if(transform.name=="East edge")
                {
                    transform.position=new Vector3(303.75f,3.6f,189.8f);
                    obstacle.size=new Vector3(3.5f,8f,117.6f);
                }
                else if(transform.name=="North edge")
                {
                    transform.position=new Vector3(248f,3.6f,250.35f);
                    obstacle.size=new Vector3(108f,8f,3.5f);
                }
                else continue;
                EditorUtility.SetDirty(transform);EditorUtility.SetDirty(obstacle);
            }
        }
        [CliCommand("dicefree.october-playtest.author","Apply the Oct 13 damage tuning and repair Trimery clearing, navigation and revive activation.")]
        public static object Run()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Exit Play Mode first.");
            if(EditorSceneManager.GetSceneManagerSetup().Any(s=>SceneManager.GetSceneByPath(s.path).isDirty))
                throw new InvalidOperationException("An open scene has unsaved changes.");
            int attacks=0;
            foreach(string guid in AssetDatabase.FindAssets("t:AttackDefinition",new[]{"Assets/_DiceFree/Settings/Ironpine"}))
            {
                var attack=AssetDatabase.LoadAssetAtPath<AttackDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if(attack.name.StartsWith("Quillrat-",StringComparison.Ordinal))
                {attack.baseDamage=20.4f;attack.maximumDamageMultiplier=1.25f;}
                else if(attack.name.StartsWith("DarkWolf-",StringComparison.Ordinal))
                {attack.baseDamage=24.15f;attack.maximumDamageMultiplier=5f/3.5f;}
                else if(attack.name.StartsWith("AlphaWolf-",StringComparison.Ordinal))
                {attack.baseDamage=54f;attack.maximumDamageMultiplier=8f/6f;}
                else continue;
                // Existing wildlife attacks have no attribute coefficient or flat offsets.
                // Values derive from 5.1 / 6.9 / 9 baseline damage and the approved endpoint factors.
                attack.minimumDamageMultiplier=1f;
                EditorUtility.SetDirty(attack);attacks++;
            }
            if(attacks!=9)throw new InvalidOperationException("Unexpected wildlife tuning inventory: "+attacks);
            var tuning=AssetDatabase.LoadAssetAtPath<SlimeDungeonTuning>(
                "Assets/_DiceFree/Settings/Dungeons/Slime playtest tuning.asset");
            if(tuning==null)throw new InvalidOperationException("Slime tuning missing.");
            tuning.bossDamageMultiplier=10f;EditorUtility.SetDirty(tuning);
            var previous=EditorSceneManager.GetSceneManagerSetup();
            int cleared;
            try
            {
                var scene=EditorSceneManager.OpenScene(TrimeryVillageAuthoring.ScenePath);
                var all=scene.GetRootGameObjects().SelectMany(root=>root.GetComponentsInChildren<Transform>(true)).ToArray();
                cleared=TrimeryVillageAuthoring.ClearFootprint(scene);
                RepairOuterNavigationEdges(all);
                var anchor=all.Select(t=>t.GetComponent<ResurrectionAnchor>())
                    .Single(a=>a!=null&&a.StableId==TrimeryVillageAuthoring.AnchorId);
                var beacon=all.Single(t=>t.name=="Trimery D6 revive beacon");
                anchor.Configure(TrimeryVillageAuthoring.AnchorId,beacon,4.5f);
                EditorUtility.SetDirty(anchor);
                CornbergSceneBuilder.Rebake();
                if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Trimery save failed.");
                AssetDatabase.SaveAssets();
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(previous);}
            return new{marker="DICEFREE_OCTOBER_PLAYTEST_REPAIRS_AUTHORED",wildlifeAttacks=attacks,
                normalBossDamageMultiplier=tuning.bossDamageMultiplier,oldTreesCleared=cleared,
                reviveActivationRadius=4.5f};
        }
    }
}
