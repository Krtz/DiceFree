using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Skills;
using Unity.Pipeline.Commands;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class ClassSkillRankValidation
    {
        [Serializable] private sealed class Envelope { public string checksum, payload; }
        private static int checks;

        private static void Need(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
            checks++;
        }

        [CliCommand("dicefree.skills.rank-migration.validate", "Validate rank-zero Greatblade/Mage schedules, ceilings, and the safe schema-6 rank reset.")]
        public static object Run()
        {
            checks = 0;
            ValidateSchedules();
            ValidateMageCeilings();
            string root = Path.Combine(Path.GetTempPath(), "DiceFree-ClassSkillRank-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            ValidateMigration(root);
            var result = new { success = true, validation = "class-skill-rank-migration", finalMarker = "DICEFREE_CLASS_SKILL_RANK_MIGRATION_OK", checks, saveRoot = root };
            Debug.Log("DICEFREE_CLASS_SKILL_RANK_MIGRATION_OK checks=" + checks + " saveRoot=" + root);
            return result;
        }

        private static void ValidateSchedules()
        {
            Need(GreatbladeProgressionRules.Skills.Sum(rule => rule.rankLevels.Length) == GreatbladeProgressionRules.TotalRanks, "Greatblade cap sum changed.");
            Need(MageProgressionRules.Skills.Sum(rule => rule.rankLevels.Length) == MageProgressionRules.TotalRanks, "Mage cap sum changed.");
            foreach (var rule in GreatbladeProgressionRules.Skills)
            {
                Need(GreatbladeSkillProgression.StarterRank(rule.kind) == 0, "Greatblade begins with a free rank: " + rule.kind);
                for (int i = 1; i < rule.rankLevels.Length; i++) Need(rule.rankLevels[i] > rule.rankLevels[i - 1], "Greatblade gates must strictly increase: " + rule.kind);
            }
            foreach (var rule in MageProgressionRules.Skills)
            {
                Need(MageSkillProgression.StarterRank(rule.kind) == 0, "Mage begins with a free rank: " + rule.kind);
                for (int i = 1; i < rule.rankLevels.Length; i++) Need(rule.rankLevels[i] > rule.rankLevels[i - 1], "Mage gates must strictly increase: " + rule.kind);
            }
            CheckGates(GreatbladeProgressionRules.For(GreatbladeProgressionRules.Kind.WideArc).rankLevels, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20);
            CheckGates(GreatbladeProgressionRules.For(GreatbladeProgressionRules.Kind.SpinDash).rankLevels, 1, 6, 11, 16, 21, 26, 31, 36, 41, 46, 51, 56, 61, 66, 71, 76);
            CheckGates(GreatbladeProgressionRules.For(GreatbladeProgressionRules.Kind.OverheadBreaker).rankLevels, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23);
            CheckGates(GreatbladeProgressionRules.For(GreatbladeProgressionRules.Kind.BattleCry).rankLevels, 18, 26, 34, 42, 50, 58, 66, 74);
            CheckGates(GreatbladeProgressionRules.For(GreatbladeProgressionRules.Kind.GreatswordMastery).rankLevels, 1, 11, 21, 31, 41, 51, 61, 71);
            CheckGates(GreatbladeProgressionRules.For(GreatbladeProgressionRules.Kind.HeavyRhythm).rankLevels, 1, 12, 23, 34, 45, 56, 67, 78);
            CheckGates(MageProgressionRules.For(MageProgressionRules.Kind.EmberBolt).rankLevels, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18);
            CheckGates(MageProgressionRules.For(MageProgressionRules.Kind.FrostSnare).rankLevels, 1, 8, 15, 22, 29, 36, 43, 50, 57, 64, 71, 78);
            CheckGates(MageProgressionRules.For(MageProgressionRules.Kind.ArcLightning).rankLevels, 6, 12, 18, 24, 30, 36, 42, 48, 54, 60, 66, 72);
            CheckGates(MageProgressionRules.For(MageProgressionRules.Kind.Quicksand).rankLevels, 12, 19, 26, 33, 40, 47, 54, 61, 68, 75);
            CheckGates(MageProgressionRules.For(MageProgressionRules.Kind.HalfLife).rankLevels, 18, 22, 26, 30, 34, 38, 42, 46);
            CheckGates(MageProgressionRules.For(MageProgressionRules.Kind.MathBarrier).rankLevels, 6, 12, 18, 24, 30);
            CheckGates(MageProgressionRules.For(MageProgressionRules.Kind.ElementalWeaving).rankLevels, 1, 12, 23, 34, 45, 56, 67, 78);
            CheckGates(MageProgressionRules.For(MageProgressionRules.Kind.ManaAttunement).rankLevels, 1, 14, 27, 40, 53, 66, 79);
            for (int level = 1; level <= 78; level++)
                Need(GreatbladeProgressionRules.Skills.Sum(rule => rule.AvailableRank(level)) >= level, "Greatblade cannot spend one point at level " + level);
            for (int level = 1; level <= 80; level++)
                Need(MageProgressionRules.Skills.Sum(rule => rule.AvailableRank(level)) >= level, "Mage cannot spend one point at level " + level);
            Need(GreatbladeProgressionRules.AllocationBudget(200) == 78 && MageProgressionRules.AllocationBudget(200) == 80, "High-level class allocation did not cap at authored total ranks.");
            Need(GreatbladeProgressionRules.Skills.Sum(rule => rule.AvailableRank(78)) == 78, "Greatblade ranks are not all available by level 78.");
            Need(MageProgressionRules.Skills.Sum(rule => rule.AvailableRank(79)) == 80, "Mage ranks are not all available by level 79.");
            Need(GreatbladeProgressionRules.GetsMasteryCleaveHaste(3) && !GreatbladeProgressionRules.GetsMasteryCleaveHaste(2), "Mastery haste milestone must be rank 3.");
            Need(GreatbladeProgressionRules.GetsMasteryWideArcWidth(5) && !GreatbladeProgressionRules.GetsMasteryWideArcWidth(4), "Mastery width milestone must be rank 5.");
            Need(GreatbladeProgressionRules.GetsMasteryFinisherProtection(7) && !GreatbladeProgressionRules.GetsMasteryFinisherProtection(6), "Mastery shield milestone must be rank 7.");
            Need(Mathf.Approximately(GreatbladeProgressionRules.MasteryShieldCapacity(1000), 150), "Mastery shield must be 100 + 5% max HP.");
        }

        private static void CheckGates(int[] actual, params int[] expected) =>
            Need(actual.SequenceEqual(expected), "Approved skill rank-gate schedule changed.");

        private static void ValidateMageCeilings()
        {
            for (int rank = 1; rank <= 12; rank++) Need(MageProgressionRules.FrostFreezeDuration(rank) <= 2.5f, "Frost freeze exceeded its approved ceiling.");
            for (int rank = 1; rank <= 12; rank++) Need(MageProgressionRules.ArcLightningTargetCount(rank) <= 8, "Arc Lightning exceeded eight unique targets.");
            for (int rank = 1; rank <= 8; rank++)
            {
                Need(MageProgressionRules.HalfLifeDefenseReduction(rank) <= 40, "Half-Life exceeded -40 DEF per channel.");
                Need(MageProgressionRules.WeavingResistanceReduction(rank) <= 8, "Weaving aura exceeded -8 percentage points.");
            }
            for (int rank = 1; rank <= 10; rank++) Need(MageProgressionRules.QuicksandSlowPercent(rank) <= 40, "Quicksand exceeded its 40% slow ceiling.");
            Need(Mathf.Approximately(MageProgressionRules.BarrierDuration(5), 10) && Mathf.Approximately(MageProgressionRules.BarrierFraction(5), .5f), "Math Barrier approved formula changed.");
        }

        private static void ValidateMigration(string root)
        {
            var original = new EchoSave { schemaVersion = 6, revision = 4 };
            original.sections.Add(Section("class.greatblade", new[] {
                new SkillRankState("skill.greatblade.widearc", 6),
                new SkillRankState("skill.greatblade.heavyrhythm", 2),
                new SkillRankState("skill.future.extension", 4)
            }, 80, 1234, 6789));
            original.sections.Add(Section("class.mage", new[] {
                new SkillRankState("skill.mage.emberbolt", 5),
                new SkillRankState("skill.mage.mathbarrier", 1),
                new SkillRankState("skill.future.extension", 3)
            }, 92, 2345, 5432));
            original.sections.Add(Section("class.physically-blessed-novice", new[] {
                new SkillRankState("skill.physical.example", 2)
            }, 23, 3456, 4567));
            original.sections.Add(new SaveSection { id = "unknown:campaign", version = 44, json = " {\"opaque\":[2,4,6]} " });
            string path = Path.Combine(root, "primary-echo.json");
            WriteFixture(path, original);
            string exactOriginal = File.ReadAllText(path);
            var store = new LocalEchoStore(root);
            var migrated = store.Load();
            Need(File.ReadAllText(path) == exactOriginal, "Migration changed the committed source before a successful save.");
            Need(File.ReadAllText(path + ".pre-skill-rank-v1-original") == exactOriginal, "Pre-rank-reset backup does not preserve original bytes.");
            Need(migrated.schemaVersion == 7 && migrated.revision == original.revision && migrated.userId == original.userId && migrated.echoId == original.echoId, "Migration changed Echo identity or schema incorrectly.");
            CheckReset(migrated, "class.greatblade", original.sections[0]);
            CheckReset(migrated, "class.mage", original.sections[1]);
            var unchanged = ManifestationRoster.ReadManifestation(migrated, "class.physically-blessed-novice");
            var beforeUnchanged = JsonUtility.FromJson<ManifestationSave>(original.sections[2].json);
            Need(JsonUtility.ToJson(unchanged) == JsonUtility.ToJson(beforeUnchanged), "Non-target manifestation changed during rank reset.");
            Need(migrated.sections[3].version == 44 && migrated.sections[3].json == original.sections[3].json, "Unknown save section was rewritten.");
            Need(!SaveMigrations.RequiresClassSkillRankReset(migrated), "Migration marker did not make rank reset idempotent.");
            var again = SaveMigrations.Upgrade(migrated);
            Need(JsonUtility.ToJson(again) == JsonUtility.ToJson(migrated), "Second upgrade mutated an already-migrated save.");
            migrated.revision++;
            store.Commit(migrated);
            Need(File.ReadAllText(path + ".bak1") == exactOriginal, "Atomic commit did not retain the original revision in backup.");
            Need(store.Load().schemaVersion == SaveMigrations.CurrentSchema && store.MigrationMessage == null, "Current save was migrated a second time.");
        }

        private static SaveSection Section(string classId, SkillRankState[] ranks, int level, int xp, long gold)
        {
            var state = new ManifestationSave {
                classId = classId, level = level, xp = xp, anchorId = "anchor.test",
                classSkills = ranks, gold = gold
            };
            return new SaveSection { id = "manifestation:" + classId, version = 6, json = JsonUtility.ToJson(state) };
        }

        private static void CheckReset(EchoSave save, string classId, SaveSection beforeSection)
        {
            var after = ManifestationRoster.ReadManifestation(save, classId);
            var before = JsonUtility.FromJson<ManifestationSave>(beforeSection.json);
            foreach (var rank in after.classSkills)
            {
                if (rank.stableId.StartsWith(classId == "class.greatblade" ? "skill.greatblade." : "skill.mage.", StringComparison.Ordinal))
                    Need(rank.rank == 0, "Target rank was not reset: " + rank.stableId);
            }
            Need(after.classSkills.Any(value => value.stableId == "skill.future.extension" && value.rank == before.classSkills.First(rank => rank.stableId == "skill.future.extension").rank), "Unknown skill data inside target manifestation was not preserved.");
            Need(after.classSkillRankMigrationVersion == SaveMigrations.ClassSkillRankMigrationVersion, "Per-manifestation migration marker missing.");
            var expected = JsonUtility.FromJson<ManifestationSave>(beforeSection.json);
            expected.classSkillRankMigrationVersion = SaveMigrations.ClassSkillRankMigrationVersion;
            for (int i = 0; i < expected.classSkills.Length; i++)
                if (expected.classSkills[i].stableId.StartsWith(classId == "class.greatblade" ? "skill.greatblade." : "skill.mage.", StringComparison.Ordinal))
                    expected.classSkills[i].rank = 0;
            Need(JsonUtility.ToJson(after) == JsonUtility.ToJson(expected), "Non-rank manifestation data changed for " + classId + ".");
        }

        private static void WriteFixture(string path, EchoSave save)
        {
            string payload = JsonUtility.ToJson(save);
            using var sha = SHA256.Create();
            string checksum = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload)));
            File.WriteAllText(path, JsonUtility.ToJson(new Envelope { payload = payload, checksum = checksum }, true));
        }
    }
}
