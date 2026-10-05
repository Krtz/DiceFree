using System;
using System.Collections;
using System.Linq;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static DiceFree.EditorTools.CombatMathValidation;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class CornbergSurgeValidation
    {
        private const string Running = "DiceFree.SurgeValidation";
        private const string Pipeline = "DiceFree.SurgeValidation.Pipeline";
        private static CombatActor player, road, elite, crop;
        private static QuestJournal journal;
        private static QuestDefinition quest;
        private static QuestGiver farmer;
        private static ManifestationPersistence persistence;
        private static IEnumerator flow;
        private static float deadline;
        private static QuestProgress State => journal.GetProgress(CornbergSurgeSetup.QuestId);
        private sealed class Roll : IRandomSource { public double value; public int calls; public double NextUnit() { calls++; return value; } }
        static CornbergSurgeValidation() => EditorApplication.playModeStateChanged += OnPlay;
        internal static void RunFromPipeline(string root)
        {
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
            CornbergValidation.ValidateNavigation();
            SessionState.SetBool(Pipeline, true);
            SessionState.SetBool(Running, true); EditorApplication.EnterPlaymode();
        }
        public static void Run()
        {
            CornbergValidation.ValidateNavigation();
            SessionState.SetBool(Running, true); EditorApplication.EnterPlaymode();
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Running, false)) return;
            deadline = Time.realtimeSinceStartup + 180; flow = Flow();
            Application.logMessageReceived += OnLog; EditorApplication.update += Tick;
        }
        private static IEnumerator Bind()
        {
            do { yield return null; persistence = UnityEngine.Object.FindAnyObjectByType<ManifestationPersistence>(); }
            while (persistence == null || !persistence.Ready);
            player = persistence.GetComponent<CombatActor>(); journal = player.GetComponent<QuestJournal>();
            quest = journal.Definitions.Single(q => q.stableId == CornbergSurgeSetup.QuestId);
            farmer = UnityEngine.Object.FindObjectsByType<QuestGiver>().Single(g => g.Quest == quest);
            road = CombatValidationActors.Find("enemy.road-slime"); crop = CombatValidationActors.Find("enemy.crop-slime");
            elite = CombatValidationActors.Find(CornbergSurgeSetup.EliteId);
            elite.GetComponent<FixedWorldDrop>().SetRandom(new Roll { value = .9 });
            foreach (var brain in UnityEngine.Object.FindObjectsByType<AggroBehaviour>()) brain.enabled = false;
        }
        private static void Kill(CombatActor target)
        {
            target.Health.Restore(); target.Health.ApplyDamage(player, new DamageResult { mitigated = 100000 });
        }
        private static void Unlock()
        {
            var records = journal.CaptureState();
            foreach (var record in records.Where(r => r.questId != quest.stableId && r.questId != CornbergDepartureSetup.QuestId))
            {
                var definition = journal.Definitions.Single(d => d.stableId == record.questId);
                record.status = QuestStatus.Completed; record.stage = definition.stages.Length - 1; record.count = definition.stages.Last().count;
            }
            journal.RestoreState(records);
        }
        private static IEnumerator Checkpoint(string label)
        {
            string before = JsonUtility.ToJson(State);
            int level = player.Stats.Level, xp = player.GetComponent<ExperienceProgression>().CurrentXp;
            string[] items = player.GetComponent<CarriedInventory>().Items.Select(i => i.instanceId).ToArray();
            long gold = player.GetComponent<GoldWallet>().Gold;
            Require(persistence.Flush(), "Q4 flush " + label);
            EditorSceneManager.LoadSceneInPlayMode(CornbergSceneBuilder.ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            for (var wait = Bind(); wait.MoveNext();) yield return null;
            Require(JsonUtility.ToJson(State) == before && player.Stats.Level == level && player.GetComponent<ExperienceProgression>().CurrentXp == xp,
                "Q4 reload progression " + label);
            Require(items.SequenceEqual(player.GetComponent<CarriedInventory>().Items.Select(i => i.instanceId)) && player.GetComponent<GoldWallet>().Gold == gold,
                "Q4 reload reward " + label);
            Require(player.Health.Current == player.Health.Maximum, "Load full HP");
            Require(UnityEngine.Object.FindObjectsByType<WorldEquipmentPickup>().Length == 0, "Transient drops survived load");
            Debug.Log("SURGE_CHECKPOINT_OK " + label);
        }
        private static IEnumerator Flow()
        {
            for (var wait = Bind(); wait.MoveNext();) yield return null;
            Require(State.status == QuestStatus.Available && !journal.CanAccept(quest), "Q4 must start locked");
            var bark = farmer.GetComponent<AmbientBark>(); var rng = new Roll { value = 0 }; bark.SetRandom(rng);
            player.Motor.Teleport(farmer.ApproachPosition); yield return null;
            Require(bark.npcId == CornbergSurgeSetup.FarmerId && bark.TryBark(player, 1000), "Ambient farmer before Q4");
            string first = bark.CurrentLine;
            Require(!bark.TryBark(player, 1001) && bark.TryBark(player, 1020) && bark.CurrentLine != first, "Bark cooldown/repeat");
            Require(!farmer.Accept(player) && State.status == QuestStatus.Available, "Bark accepted locked quest");
            Kill(road); Kill(elite); Unlock();
            Require(State.count == 0 && State.alternatives.Length == 0 && journal.CanAccept(quest), "Preaccept kill leaked");
            player.GetComponent<ExperienceProgression>().RestoreState(6, 0);
            for (var wait = Checkpoint("before-accept"); wait.MoveNext();) yield return null;
            Require(!farmer.Accept(player), "Remote accept");
            var interaction = player.GetComponent<Interactor>(); Require(interaction.Order(farmer), "Farmer approach");
            while (interaction.Active != farmer) yield return null;
            Require(farmer.Accept(player), "Valid farmer accept"); interaction.Cancel();
            for (var wait = Checkpoint("accepted-zero"); wait.MoveNext();) yield return null;
            ValidateNearby();
            var routeState = journal.CaptureState();
            var routeXp = player.GetComponent<ExperienceProgression>();
            Kill(road); Kill(road); Kill(elite);
            Require(State.status == QuestStatus.ReadyToTurnIn && ObjectiveProgress.Count(State, "surge.population") == 3, "Partial mass plus elite OR");
            string readySnapshot = JsonUtility.ToJson(State);
            for (int i = 0; i < 30; i++) Kill(road);
            Require(JsonUtility.ToJson(State) == readySnapshot, "Elite-first mass replay");
            journal.RestoreState(routeState); routeXp.RestoreState(6, 0);
            Kill(crop); Require(State.alternatives.Length == 0, "Crop counted");
            player.GetComponent<ExperienceProgression>().RestoreState(6, 0);
            for (int i = 0; i < 12; i++) Kill(road);
            Require(ObjectiveProgress.Count(State, "surge.population") == 12 && State.status == QuestStatus.Active, "Mass partial");
            for (var wait = Checkpoint("partial-12"); wait.MoveNext();) yield return null;
            for (int i = 12; i < 29; i++) Kill(road);
            for (var wait = Checkpoint("29-of-30"); wait.MoveNext();) yield return null;
            Kill(road); Require(State.status == QuestStatus.ReadyToTurnIn, "Mass route ready");
            routeXp = player.GetComponent<ExperienceProgression>();
            int massLevel = routeXp.Level, massXp = routeXp.CurrentXp;
            readySnapshot = JsonUtility.ToJson(State); Kill(elite);
            Require(JsonUtility.ToJson(State) == readySnapshot, "Mass-first elite replay"); routeXp.RestoreState(massLevel, massXp);
            for (var wait = Checkpoint("ready"); wait.MoveNext();) yield return null;
            Require(!CombatValidationActors.OriginalFarmer().TurnIn(player, quest), "Wrong farmer turn-in");
            player.Motor.Teleport(farmer.ApproachPosition); yield return null;
            var originalReward = quest.reward; var invalidReward = ScriptableObject.CreateInstance<FixedRewardDefinition>();
            invalidReward.items = new ItemDefinition[] { null }; quest.reward = invalidReward;
            Require(!farmer.TurnIn(player) && State.status == QuestStatus.ReadyToTurnIn && player.GetComponent<GoldWallet>().Gold == 0, "Reward prevalidation mutated state");
            quest.reward = originalReward; UnityEngine.Object.Destroy(invalidReward);
            bool reentered = false, partialWrite = false;
            Action observer = () => { reentered |= journal.TurnIn(quest); partialWrite |= persistence.Flush(); };
            player.GetComponent<CarriedInventory>().Changed += observer;
            Require(farmer.TurnIn(player) && !farmer.TurnIn(player), "Turn in once");
            player.GetComponent<CarriedInventory>().Changed -= observer;
            Require(!reentered && !partialWrite, "Partial save/reentrant reward");
            Require(player.GetComponent<GoldWallet>().Gold == 100 && player.GetComponent<CarriedInventory>().Items.Count(i => i.definitionId == "item.cornberg.work-gloves") == 1, "Main reward");
            Debug.Log($"SURGE_PACING_MASS level={player.Stats.Level} xp={player.GetComponent<ExperienceProgression>().CurrentXp}; 30x20 + 200 quest XP; 100 gold");
            Require(player.Stats.Level == 13 && player.GetComponent<ExperienceProgression>().CurrentXp == 30, "Mass XP exactly once");
            for (var wait = Checkpoint("completed"); wait.MoveNext();) yield return null;
            Require(!journal.TurnIn(quest), "Reload replay");
            player.GetComponent<CarriedInventory>().Restore(Array.Empty<ItemInstance>());
            player.GetComponent<GoldWallet>().Restore(0);
            var fresh = journal.CaptureState(); var q4 = Array.Find(fresh, q => q.questId == quest.stableId);
            q4.status = QuestStatus.Available; q4.count = 0; q4.alternatives = Array.Empty<ObjectiveCount>(); journal.RestoreState(fresh);
            Require(journal.Accept(quest), "Elite fixture accept"); player.GetComponent<ExperienceProgression>().RestoreState(6, 0);
            var drop = elite.GetComponent<FixedWorldDrop>(); var roll = new Roll { value = .2f }; drop.SetRandom(roll);
            Kill(elite); Require(State.status == QuestStatus.ReadyToTurnIn && roll.calls == 1, "Elite route/fail roll");
            Require(UnityEngine.Object.FindObjectsByType<WorldEquipmentPickup>().Length == 0, "Threshold failed roll created drop");
            elite.Health.ApplyDamage(player, new DamageResult { mitigated = 100000 }); Require(roll.calls == 1, "Corpse roll replay");
            Require(elite.GetComponent<OverworldRespawn>().Definition.delaySeconds == 450 && elite.Stats.Level == 8 && elite.Health.Maximum == 240, "Elite authored stats/respawn");
            player.Motor.Teleport(farmer.ApproachPosition); yield return null;
            Require(farmer.TurnIn(player), "Elite reward");
            Debug.Log($"SURGE_PACING_ELITE level={player.Stats.Level} xp={player.GetComponent<ExperienceProgression>().CurrentXp}; 80 elite + 200 quest XP");
            Require(player.Stats.Level == 9 && player.GetComponent<ExperienceProgression>().CurrentXp == 10 &&
                player.GetComponent<GoldWallet>().Gold == 100 && player.GetComponent<CarriedInventory>().Items.Length == 1, "Elite same main reward");
            var owned = player.GetComponent<CarriedInventory>().Items.Length;
            roll.value = .199; WorldEquipmentPickup pickup = null; drop.Dropped += value => pickup = value;
            Kill(elite); Require(pickup != null && roll.calls == 2 && player.GetComponent<CarriedInventory>().Items.Length == owned, "World drop not direct grant");
            var other = UnityEngine.Object.Instantiate(player.gameObject); other.GetComponent<ManifestationPersistence>().enabled = false;
            other.GetComponent<TraversalInput>().enabled = false; other.GetComponent<CarriedInventory>().Restore(Array.Empty<ItemInstance>());
            other.transform.position = pickup.transform.position;
            Require(pickup.TryPickup(other.GetComponent<CombatActor>()) && other.GetComponent<CarriedInventory>().Items.Length == 1 && !pickup.TryPickup(player), "FFA non-killer pickup");
            other.SetActive(false); UnityEngine.Object.Destroy(other);
            Kill(elite); Require(roll.calls == 3 && pickup != null && !pickup.Consumed, "Next life world drop");
            player.Motor.Teleport(pickup.transform.position); yield return null;
            Require(pickup.TryPickup(player) && !pickup.TryPickup(player), "Pickup exactly once");
            Require(player.GetComponent<CarriedInventory>().Items.Length == owned + 1, "Shoe grant");
            for (var wait = Checkpoint("shoe-pickup"); wait.MoveNext();) yield return null;
            for (var wait = EliteChallenge(); wait.MoveNext();) yield return null;
            Debug.Log("CORNBERG_SURGE_CORE_OK: both routes, gates, barks, seven reloads, rewards, threshold drop and pickup.");
        }
        private static IEnumerator EliteChallenge()
        {
            var brain = elite.GetComponent<AggroBehaviour>(); var playerAttack = player.GetComponent<BasicAttack>();
            var enemyAttack = elite.GetComponent<BasicAttack>(); var home = elite.transform.position;
            Time.timeScale = 10;
            player.GetComponent<ExperienceProgression>().RestoreState(6, 0);
            float groupPlayerHp = player.Health.Maximum, groupEliteHp = elite.Health.Maximum, groupTime = 0;
            float playerNext = 0, enemyNext = 0;
            while (groupPlayerHp > 0 && groupEliteHp > 0 && groupTime < 90)
            {
                groupTime += .01f;
                groupPlayerHp = Mathf.Min(player.Health.Maximum, groupPlayerHp + player.Stats.Regeneration * player.Stats.HealingReceived * .01f);
                if (groupTime >= playerNext) { groupEliteHp -= 2 * DamageResolver.Calculate(player.Stats, elite.Stats, playerAttack.Definition).DamagePotential; playerNext += playerAttack.Definition.interval / player.Stats.AttackSpeed; }
                if (groupEliteHp > 0 && groupTime >= enemyNext) { groupPlayerHp -= DamageResolver.Calculate(elite.Stats, player.Stats, enemyAttack.Definition).DamagePotential; enemyNext += enemyAttack.Definition.interval / elite.Stats.AttackSpeed; }
            }
            Require(groupPlayerHp > 0 && groupEliteHp <= 0, "Two level-six stationary attackers math target");
            Debug.Log($"SURGE_GROUP_MATH two-level-6: tankHP={groupPlayerHp:F2} seconds={groupTime:F2}; stationary packets, not network simulation.");
            foreach (int level in new[] { 6, 8, 9 })
            {
                playerAttack.ResetForSpawn(); enemyAttack.ResetForSpawn();
                player.GetComponent<ExperienceProgression>().RestoreState(level, 0); player.Health.Restore(); elite.Health.Restore();
                Require(elite.Motor.Teleport(home) && player.Motor.Teleport(home + Vector3.right * 2), "Elite duel positions");
                brain.enabled = true; Require(playerAttack.Order(elite), "Elite duel attack");
                float started = Time.time;
                while (player.Alive && elite.Alive && Time.time - started < 90) yield return null;
                Require(!player.Alive || !elite.Alive, "Elite duel timed out");
                Debug.Log($"SURGE_CHALLENGE solo-level-{level}: playerAlive={player.Alive} playerHP={player.Health.Current:F2} eliteHP={elite.Health.Current:F2} seconds={Time.time-started:F2}");
                Require(level == 6 ? !player.Alive : player.Alive && !elite.Alive, "Provisional elite solo target");
                brain.enabled = false; playerAttack.ResetForSpawn(); enemyAttack.ResetForSpawn();
            }
            player.Motor.Teleport(farmer.ApproachPosition);
            float due = elite.GetComponent<OverworldRespawn>().Remaining;
            Require(due > 449 && due <= 450, "Elite real respawn timer");
            Time.timeScale = 50;
            while (!elite.Alive) yield return null;
            Require(elite.Health.Current == elite.Health.Maximum && enemyAttack.Target == null && Vector3.Distance(elite.transform.position, home) < .1f,
                "Elite respawn HP/home/target");
            Time.timeScale = 1;
            Debug.Log("SURGE_RESPAWN_OK: actual 450-second timer, same actor, full HP and clean target.");
        }
        private static void ValidateNearby()
        {
            var saved = journal.CaptureState(); var xp = player.GetComponent<ExperienceProgression>(); int level = xp.Level, amount = xp.CurrentXp;
            var member = player.gameObject.AddComponent<LocalPartyMember>(); member.partyId = "party.fixture";
            var other = UnityEngine.Object.Instantiate(player.gameObject); other.name = "Local party credit fixture";
            other.GetComponent<ManifestationPersistence>().enabled = false;
            other.GetComponent<TraversalInput>().enabled = false;
            var recipient = other.GetComponent<CombatActor>(); var quests = other.GetComponent<QuestJournal>();
            var recipientXp = other.GetComponent<ExperienceProgression>();
            quests.RestoreState(saved); int otherLevel = recipientXp.Level, otherXp = recipientXp.CurrentXp;
            foreach (float distance in new[] { 49f, 50f, 50.01f })
            {
                other.transform.position = road.transform.position + Vector3.right * distance;
                int before = ObjectiveProgress.Count(quests.GetProgress(quest.stableId), "surge.population");
                Kill(road);
                Require(ObjectiveProgress.Count(quests.GetProgress(quest.stableId), "surge.population") == before + (distance <= 50 ? 1 : 0), "Nearby inclusive 50m boundary");
            }
            other.transform.position = road.transform.position;
            other.GetComponent<LocalPartyMember>().partyId = "party.other";
            Kill(road); Require(ObjectiveProgress.Count(quests.GetProgress(quest.stableId), "surge.population") == 2, "Non-party credit");
            other.GetComponent<LocalPartyMember>().partyId = member.partyId;
            var inactive = quests.CaptureState(); var q4 = Array.Find(inactive, q => q.questId == quest.stableId);
            q4.status = QuestStatus.Available; q4.alternatives = Array.Empty<ObjectiveCount>(); quests.RestoreState(inactive);
            Kill(road); Require(quests.GetProgress(quest.stableId).status == QuestStatus.Available && quests.GetProgress(quest.stableId).alternatives.Length == 0, "Inactive nearby quest");
            Require(recipientXp.Level == otherLevel && recipientXp.CurrentXp == otherXp, "Nearby quest credit leaked into XP");
            quests.RestoreState(saved); other.transform.position = elite.transform.position + Vector3.right * 51; Kill(elite);
            Require(quests.GetProgress(quest.stableId).status == QuestStatus.Active, "Out-of-radius elite credit");
            other.transform.position = elite.transform.position + Vector3.right * 50; Kill(elite);
            Require(quests.GetProgress(quest.stableId).status == QuestStatus.ReadyToTurnIn, "Nearby elite credit");
            other.SetActive(false); UnityEngine.Object.Destroy(other); UnityEngine.Object.Destroy(member);
            journal.RestoreState(saved); xp.RestoreState(level, amount);
            Debug.Log("SURGE_NEARBY_OK: owner, 49m, inclusive 50m, outside, non-party, inactive quest, elite and no shared XP.");
        }
        private static void Tick()
        {
            try { Require(Time.realtimeSinceStartup < deadline, "Q4 timeout"); if (!flow.MoveNext()) Finish(0); }
            catch (Exception error) { Debug.LogException(error); Finish(1); }
        }
        private static void OnLog(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception) return;
            if (message.StartsWith("ArgumentOutOfRangeException") && trace.Contains("UnityEditor.Search.SearchDatabase") && trace.Contains("IndexationOnStartup")) return;
            Finish(1);
        }
        private static void Finish(int code)
        {
            SessionState.SetBool(Running, false); EditorApplication.update -= Tick; Application.logMessageReceived -= OnLog; Time.timeScale = 1;
            if (SessionState.GetBool(Pipeline, false))
            {
                SessionState.SetBool(Pipeline, false); WorkGlovesRegressionValidation.Complete(code); EditorApplication.ExitPlaymode();
            }
            else EditorApplication.Exit(code);
        }
    }
}
