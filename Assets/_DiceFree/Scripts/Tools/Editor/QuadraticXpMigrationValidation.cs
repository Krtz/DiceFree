using System;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;

namespace DiceFree.EditorTools
{
    /// <summary>
    /// Edit-mode, save-safe regression checks for the quadratic XP change.
    /// Never opens or changes the actual user's Echo profile.
    /// </summary>
    public static class QuadraticXpMigrationValidation
    {
        [CliCommand("dicefree.xp.quadratic.validate",
            "Check L1-200 quadratic thresholds, every-manifestation save migration and early reward data.")]
        public static object Validate()
        {
            var curve=AssetDatabase.LoadAssetAtPath<ExperienceCurve>(
                "Assets/_DiceFree/Settings/Progression/Starter XP curve.asset");
            Check(curve!=null && curve.firstThreshold==30 &&
                curve.quadraticCoefficient==3 && curve.levelLimit==200,
                "Starter curve coefficient or cap is wrong.");
            foreach(var point in new[]
            {
                (level:1, threshold:30),
                (level:2, threshold:33),
                (level:5, threshold:78),
                (level:10, threshold:273),
                (level:20, threshold:1113),
                (level:30, threshold:2553),
                (level:60, threshold:10473),
                (level:120, threshold:42513),
                (level:199, threshold:117642)
            })
                Check(curve.ToNextLevel(point.level)==point.threshold,
                    "Unexpected XP threshold at L"+point.level);

            long total=0;
            foreach(int cap in new[]{10,30,60,120,200})
            {
                // Total for a fresh level-1 manifestation to REACH cap.
                total=0;
                for(int level=1;level<cap;level++)total+=curve.ToNextLevel(level);
                long expected=cap switch
                {
                    10=>882L, 30=>24012L, 60=>201957L,
                    120=>1667547L, 200=>7827267L, _=>-1L
                };
                Check(total==expected,"Wrong cumulative XP to level "+cap);
            }

            // Test both active and inactive manifestations in a single Echo;
            // XP must be rebased for EVERY class without changing its level.
            var older=new EchoSave{schemaVersion=5};
            foreach(var record in new[]
            {
                (id:"class.novice",level:9,xp:45),
                (id:"class.physical",level:24,xp:128),
                (id:"class.magical",level:30,xp:200),
                (id:"class.highest",level:200,xp:0)
            })
            {
                var state=new ManifestationSave
                {
                    classId=record.id,level=record.level,xp=record.xp,
                    gold=500,quests=Array.Empty<QuestProgress>()
                };
                older.sections.Add(new SaveSection
                {
                    id="manifestation:"+record.id,version=5,
                    json=JsonUtility.ToJson(state)
                });
            }
            older.sections.Add(new SaveSection
            {
                id="echo:future-feature",version=7,json="{\"keep\":true}"
            });

            string originalJson=JsonUtility.ToJson(older);
            var migrated=SaveMigrations.Upgrade(older);
            Check(migrated.schemaVersion==6 &&
                JsonUtility.ToJson(older)==originalJson,
                "Migration mutated original source or failed version bump.");
            foreach(var old in older.sections.Where(x=>
                x.id.StartsWith("manifestation:",StringComparison.Ordinal)))
            {
                var updated=migrated.sections.Single(x=>x.id==old.id);
                var before=JsonUtility.FromJson<ManifestationSave>(old.json);
                var after=JsonUtility.FromJson<ManifestationSave>(updated.json);
                int expected=SaveMigrations.RebaseLegacyXp(before.level,before.xp);
                Check(updated.version==6 && after.level==before.level &&
                    after.xp==expected && after.gold==before.gold &&
                    after.classId==before.classId,
                    "Migration altered saved character identity, level, money or XP fraction.");
                if(before.level<200)
                {
                    double oldFraction=before.xp/(30.0+10*(before.level-1));
                    double newFraction=after.xp/(double)curve.ToNextLevel(after.level);
                    Check(Math.Abs(oldFraction-newFraction)<
                        1.01/curve.ToNextLevel(after.level),
                        "XP progress percentage drifted during migration.");
                }
            }
            var future=migrated.sections.Single(x=>x.id=="echo:future-feature");
            Check(future.version==7 && future.json=="{\"keep\":true}",
                "Unknown Echo-wide save data must remain byte-for-byte opaque.");
            Check(JsonUtility.ToJson(SaveMigrations.Upgrade(migrated))==
                    JsonUtility.ToJson(migrated),
                "Migration is not idempotent.");
            bool rejectsBadProgress=false;
            try{SaveMigrations.RebaseLegacyXp(20,220);}
            catch(SaveMigrationException){rejectsBadProgress=true;}
            Check(rejectsBadProgress,"Invalid pre-quadratic XP must not be silently rewritten.");

            // Report actual authored XP balance, without changing any reward.
            var quests=AssetDatabase.FindAssets("t:QuestDefinition",
                    new[]{"Assets/_DiceFree/Settings"})
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<QuestDefinition>)
                .Where(x=>x!=null).ToArray();
            var monsters=AssetDatabase.FindAssets("t:ActorDefinition",
                    new[]{"Assets/_DiceFree/Settings"})
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ActorDefinition>)
                .Where(x=>x!=null&&x.experienceReward>0).ToArray();
            foreach(var quest in quests.OrderBy(x=>x.stableId))
                Debug.Log("DICEFREE_XP_QUEST_AUDIT "+quest.stableId+
                    " reward="+quest.rewardXp);
            foreach(var monster in monsters.OrderBy(x=>x.stableId))
                Debug.Log("DICEFREE_XP_ENEMY_AUDIT "+monster.stableId+
                    " killXp="+monster.experienceReward);
            Debug.Log("DICEFREE_QUADRATIC_XP_MIGRATION_OK: coefficient 3, levels 1-200, schema v6, four class save states, opaque sections, audited rewards");
            return new{success=true,formula="30 + 3*(level-1)^2",
                schema=6,manifestationsMigrated=4,questDefinitions=quests.Length,
                enemyDefinitions=monsters.Length};
        }

        private static void Check(bool condition,string message)
        {
            if(!condition)throw new InvalidOperationException(
                "Quadratic XP validation: "+message);
        }
    }
}
