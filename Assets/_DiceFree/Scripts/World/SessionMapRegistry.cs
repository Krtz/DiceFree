using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceFree.World
{
    public enum SessionLocationKind
    {
        WorldFace = 0,
        Dungeon = 1
    }

    public enum DungeonLifecycleStage
    {
        Staging = 0,
        Active = 1,
        Completed = 2
    }

    public sealed class SessionParticipantLocation
    {
        public string participantId;
        public SessionLocationKind kind;
        public string mapId;
    }

    public sealed class DungeonOccupancySnapshot
    {
        public string dungeonId;
        public string leaseToken;
        public string partyId;
        public DungeonLifecycleStage stage;
        public string[] participants = Array.Empty<string>();
        public DateTime claimedUtc;
        public DateTime lastTouchedUtc;
        public DungeonRunVariantSnapshot variant;

        public DungeonOccupancySnapshot Copy() => new()
        {
            dungeonId = dungeonId,
            leaseToken = leaseToken,
            partyId = partyId,
            stage = stage,
            participants = participants?.ToArray() ?? Array.Empty<string>(),
            claimedUtc = claimedUtc,
            lastTouchedUtc = lastTouchedUtc,
            variant = variant == null ? null : new DungeonRunVariantSnapshot
            {
                dungeonId = variant.dungeonId,
                averagePartyLevel = variant.averagePartyLevel,
                matchedRuleIds = variant.matchedRuleIds?.ToArray() ?? Array.Empty<string>(),
                enabledRouteIds = variant.enabledRouteIds?.ToArray() ?? Array.Empty<string>(),
                enabledBossIds = variant.enabledBossIds?.ToArray() ?? Array.Empty<string>(),
                lootTableIds = variant.lootTableIds?.ToArray() ?? Array.Empty<string>(),
                variantTags = variant.variantTags?.ToArray() ?? Array.Empty<string>()
            }
        };
    }

    /// <summary>
    /// Host-session logical map state. World faces are never exclusive. Dungeons are:
    /// one live occupancy per stable dungeon ID, regardless of which party asks for entry.
    /// </summary>
    public sealed class SessionMapRegistry
    {
        private readonly Dictionary<string, SessionParticipantLocation> participants =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, DungeonOccupancySnapshot> dungeons =
            new(StringComparer.Ordinal);

        public SessionParticipantLocation LocationOf(string participantId)
        {
            if (string.IsNullOrWhiteSpace(participantId)) return null;
            return participants.TryGetValue(participantId, out var value)
                ? new SessionParticipantLocation
                {
                    participantId = value.participantId,
                    kind = value.kind,
                    mapId = value.mapId
                }
                : null;
        }

        public DungeonOccupancySnapshot Dungeon(string dungeonId) =>
            !string.IsNullOrWhiteSpace(dungeonId) &&
            dungeons.TryGetValue(dungeonId, out var value)
                ? value.Copy()
                : null;

        public string[] OccupiedDungeonIds =>
            dungeons.Keys.OrderBy(value => value, StringComparer.Ordinal).ToArray();

        public void PlaceOnWorldFace(string participantId, string faceId)
        {
            Require(participantId, nameof(participantId));
            Require(faceId, nameof(faceId));
            participants[participantId] = new SessionParticipantLocation
            {
                participantId = participantId,
                kind = SessionLocationKind.WorldFace,
                mapId = faceId
            };
        }

        public bool TryClaimDungeonStaging(
            string dungeonId,
            string partyId,
            string participantId,
            DateTime nowUtc,
            out DungeonOccupancySnapshot occupancy,
            out string error)
        {
            occupancy = null;
            error = null;
            Require(dungeonId, nameof(dungeonId));
            Require(partyId, nameof(partyId));
            Require(participantId, nameof(participantId));

            if (dungeons.TryGetValue(dungeonId, out var existing))
            {
                if (existing.partyId != partyId)
                    return Reject("Dungeon is currently occupied by another party.", out error);
                if (existing.stage != DungeonLifecycleStage.Staging)
                    return Reject("Dungeon run has already started.", out error);

                AddParticipant(existing, participantId);
                existing.lastTouchedUtc = nowUtc;
                participants[participantId] = DungeonLocation(participantId, dungeonId);
                occupancy = existing.Copy();
                return true;
            }

            var created = new DungeonOccupancySnapshot
            {
                dungeonId = dungeonId,
                partyId = partyId,
                leaseToken = Guid.NewGuid().ToString("N"),
                stage = DungeonLifecycleStage.Staging,
                participants = new[] { participantId },
                claimedUtc = nowUtc,
                lastTouchedUtc = nowUtc
            };
            dungeons.Add(dungeonId, created);
            participants[participantId] = DungeonLocation(participantId, dungeonId);
            occupancy = created.Copy();
            return true;
        }

        public bool TryBeginDungeon(
            string dungeonId,
            string leaseToken,
            DungeonRunVariantSnapshot variant,
            DateTime nowUtc,
            out DungeonOccupancySnapshot occupancy,
            out string error)
        {
            occupancy = null;
            error = null;
            if (!TryOwned(dungeonId, leaseToken, out var state, out error)) return false;
            if (state.stage != DungeonLifecycleStage.Staging)
                return Reject("Dungeon is not in staging.", out error);
            if (variant == null || variant.dungeonId != dungeonId)
                return Reject("Dungeon variant snapshot does not match occupancy.", out error);

            state.stage = DungeonLifecycleStage.Active;
            state.variant = variant;
            state.lastTouchedUtc = nowUtc;
            occupancy = state.Copy();
            return true;
        }

        public bool TryCompleteDungeon(
            string dungeonId,
            string leaseToken,
            DateTime nowUtc,
            out string error)
        {
            if (!TryOwned(dungeonId, leaseToken, out var state, out error)) return false;
            if (state.stage != DungeonLifecycleStage.Active)
                return Reject("Only an active dungeon can complete.", out error);

            state.stage = DungeonLifecycleStage.Completed;
            state.lastTouchedUtc = nowUtc;
            return true;
        }

        public bool TouchDungeon(
            string dungeonId,
            string leaseToken,
            DateTime nowUtc,
            out string error)
        {
            if (!TryOwned(dungeonId, leaseToken, out var state, out error)) return false;
            state.lastTouchedUtc = nowUtc;
            return true;
        }

        public bool ReleaseDungeon(
            string dungeonId,
            string leaseToken,
            string fallbackFaceId,
            out string error)
        {
            if (!TryOwned(dungeonId, leaseToken, out var state, out error)) return false;

            dungeons.Remove(dungeonId);
            foreach (string participantId in state.participants ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(participantId)) continue;
                if (!participants.TryGetValue(participantId, out var location) ||
                    location.kind != SessionLocationKind.Dungeon ||
                    location.mapId != dungeonId)
                    continue;

                if (string.IsNullOrWhiteSpace(fallbackFaceId))
                    participants.Remove(participantId);
                else
                    PlaceOnWorldFace(participantId, fallbackFaceId);
            }
            return true;
        }

        public int RecoverStaleDungeonLocks(
            DateTime nowUtc,
            TimeSpan maxHeartbeatAge,
            string fallbackFaceId)
        {
            if (maxHeartbeatAge <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(maxHeartbeatAge));

            string[] stale = dungeons.Values
                .Where(value => nowUtc - value.lastTouchedUtc > maxHeartbeatAge)
                .Select(value => value.dungeonId)
                .ToArray();

            foreach (string dungeonId in stale)
            {
                var state = dungeons[dungeonId];
                ReleaseDungeon(dungeonId, state.leaseToken, fallbackFaceId, out _);
            }
            return stale.Length;
        }

        private bool TryOwned(
            string dungeonId,
            string leaseToken,
            out DungeonOccupancySnapshot state,
            out string error)
        {
            state = null;
            error = null;
            if (string.IsNullOrWhiteSpace(dungeonId) ||
                !dungeons.TryGetValue(dungeonId, out state))
                return Reject("Dungeon is not occupied.", out error);
            if (string.IsNullOrWhiteSpace(leaseToken) || state.leaseToken != leaseToken)
                return Reject("Dungeon occupancy lease does not match.", out error);
            return true;
        }

        private static void AddParticipant(DungeonOccupancySnapshot state, string participantId)
        {
            var values = new HashSet<string>(
                state.participants ?? Array.Empty<string>(),
                StringComparer.Ordinal) { participantId };
            state.participants = values.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        }

        private static SessionParticipantLocation DungeonLocation(
            string participantId,
            string dungeonId) =>
            new()
            {
                participantId = participantId,
                kind = SessionLocationKind.Dungeon,
                mapId = dungeonId
            };

        private static bool Reject(string value, out string error)
        {
            error = value;
            return false;
        }

        private static void Require(string value, string parameter)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Stable session IDs must be non-empty.", parameter);
        }
    }
}
