using System.Collections.Generic;
using DiceFree.Quests;
using DiceFree.Input;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed partial class WorldAtlasHud
    {
        private sealed class AtlasMarker
        {
            public Component source;
            public MapIconCategory kind;
            public Vector3 position;
            public string name;
            public bool ready, activeAnchor;
            public readonly List<QuestDefinition> quests = new();
            public readonly List<string> details = new();
        }

        private const float MapMarkerLegendHeight = 154;
        private const float MarkerSize = 22;
        private readonly List<AtlasMarker> mapMarkers = new();
        private readonly List<Rect> mapMarkerRects = new();
        private readonly List<Rect> mapLabelRects = new();
        private readonly List<Rect> minimapMarkerRects = new();
        private readonly List<Rect> minimapIconExclusions = new();
        private float nextMarkerRefresh;
        private GUIStyle mapGlyphStyle;
        private AtlasMarker hoveredMapMarker;
        private Rect hoveredMapMarkerRect;

        private void RefreshMapMarkers()
        {
            nextMarkerRefresh = Time.unscaledTime + .35f;
            mapMarkers.Clear();
            if (player == null) return;
            var scene = InAshfall ? "WorldTwoArrival" : player.gameObject.scene.name;
            foreach (var target in FindObjectsByType<InteractionTarget>())
            {
                if (!target.isActiveAndEnabled || !target.WorldTarget.isActiveAndEnabled ||
                    target.gameObject.scene.name != scene) continue;
                var root = target.WorldTarget;
                if (target is IMapMerchant)
                    MarkerFor(root, MapIconCategory.Merchant, root.transform.position, TargetName(root));
                if (target is TimedInteractionTarget work && work.Available(player))
                    MarkerFor(root, MapIconCategory.QuestObjective, work.ApproachPosition, work.DisplayName);
                if (journal == null) continue;
                if (target is QuestGiver giver)
                {
                    foreach (var quest in giver.Offers)
                    {
                        if (quest == null) continue;
                        var progress = journal.GetProgress(quest.stableId);
                        bool ready = progress?.status == QuestStatus.ReadyToTurnIn &&
                            progress.definitionVersion == quest.version && !quest.completeOnObjectives;
                        if (ready || journal.CanAccept(quest))
                            AddMapQuest(root, quest, ready ? "Ready to turn in" : "Available", ready);
                    }
                }
                if (target is ConversationTarget conversation && conversation.Available(player))
                    foreach (var quest in journal.QuestsNeedingConversation(conversation.NpcId, conversation.ConversationId))
                        AddMapQuest(root, quest, "Speak to this character", false);
            }
            var respawn = player.GetComponent<RespawnAtAnchor>();
            foreach (var anchor in FindObjectsByType<ResurrectionAnchor>())
            {
                if (!anchor.isActiveAndEnabled || anchor.gameObject.scene.name != scene ||
                    string.IsNullOrEmpty(anchor.StableId)) continue;
                var marker = MarkerFor(anchor, MapIconCategory.ResurrectionDice, anchor.ActivationPosition, anchor.name);
                if (marker != null) marker.activeAnchor = respawn != null && respawn.AnchorId == anchor.StableId;
            }
            mapMarkers.Sort((a, b) =>
            {
                int kind = a.kind.CompareTo(b.kind);
                if (kind != 0) return kind;
                int x = a.position.x.CompareTo(b.position.x);
                return x != 0 ? x : a.position.z.CompareTo(b.position.z);
            });
        }

        private static string TargetName(InteractionTarget target) =>
            string.IsNullOrWhiteSpace(target.DisplayName) ? target.name : target.DisplayName;

        private AtlasMarker MarkerFor(Component source, MapIconCategory kind, Vector3 position, string name)
        {
            var bounds = InAshfall ? AshfallReach.Bounds : Extents;
            if (!bounds.Contains(new Vector2(position.x, position.z))) return null;
            foreach (var marker in mapMarkers)
                if (marker.source == source && marker.kind == kind) return marker;
            var created = new AtlasMarker { source = source, kind = kind, position = position, name = name };
            mapMarkers.Add(created);
            return created;
        }

        private void AddMapQuest(InteractionTarget target, QuestDefinition quest, string status, bool ready)
        {
            var kind = quest.repeatable ? MapIconCategory.RepeatableQuest :
                quest.category == QuestCategory.Main ? MapIconCategory.MainQuest : MapIconCategory.SideQuest;
            var marker = MarkerFor(target, kind, target.transform.position, TargetName(target));
            if (marker == null || marker.quests.Contains(quest)) return;
            marker.quests.Add(quest);
            marker.details.Add(quest.title + " · " + status);
            marker.ready |= ready;
        }

        private void DrawMapMarkers(Rect plot)
        {
            if (Time.unscaledTime >= nextMarkerRefresh) RefreshMapMarkers();
            mapMarkerRects.Clear();
            hoveredMapMarker = null;
            foreach (var marker in mapMarkers)
            {
                if (marker.source == null || !marker.source.gameObject.activeInHierarchy ||
                    !GameplayPreferences.Current.IsMapIconVisible(marker.kind)) continue;
                var point = new Vector2(marker.position.x, marker.position.z);
                var projected = InAshfall
                    ? AshfallProject(plot, point - new Vector2(AshfallReach.Origin.x, AshfallReach.Origin.z))
                    : Project(plot, point);
                var rect = PlaceMapMarker(plot, projected, mapMarkerRects, mapLabelRects);
                mapMarkerRects.Add(rect);
                bool over = rect.Contains(Event.current.mousePosition);
                Color tint = MarkerColor(marker.kind);
                if (Vector2.Distance(projected, rect.center) > 6)
                    Line(projected, rect.center, 1, new Color(tint.r, tint.g, tint.b, .65f));
                DrawMapIcon(rect.center, marker.kind, marker.ready);
                if (marker.activeAnchor) Diamond(rect.center, 15, 1.3f, new Color(.39f, .95f, .69f));
                if (over)
                {
                    HudChrome.Border(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4), 1, Ink);
                    hoveredMapMarker = marker;
                    hoveredMapMarkerRect = rect;
                }
            }
        }

        internal void DrawMinimapMarkers(MinimapHud minimap, Rect plot, Rect compass, Rect zoomIn, Rect zoomOut)
        {
            if (Time.unscaledTime >= nextMarkerRefresh) RefreshMapMarkers();
            minimapMarkerRects.Clear();
            minimapIconExclusions.Clear();
            minimapIconExclusions.Add(compass);
            minimapIconExclusions.Add(zoomIn);
            minimapIconExclusions.Add(zoomOut);
            minimapIconExclusions.Add(new Rect(plot.center.x - 12, plot.center.y - 12, 24, 24));
            hoveredMapMarker = null;
            foreach (var marker in mapMarkers)
            {
                if (marker.kind == MapIconCategory.QuestObjective || marker.source == null ||
                    !marker.source.gameObject.activeInHierarchy ||
                    !GameplayPreferences.Current.IsMapIconVisible(marker.kind) ||
                    !minimap.TryProjectServiceMarker(marker.position, plot, out var projected)) continue;
                var rect = PlaceMapMarker(plot, projected, minimapMarkerRects, minimapIconExclusions);
                minimapMarkerRects.Add(rect);
                Color tint = MarkerColor(marker.kind);
                if (Vector2.Distance(projected, rect.center) > 6)
                    Line(projected, rect.center, 1, new Color(tint.r, tint.g, tint.b, .65f));
                DrawMapIcon(rect.center, marker.kind, marker.ready);
                if (marker.activeAnchor) Diamond(rect.center, 14, 1, new Color(.39f, .95f, .69f));
                if (rect.Contains(Event.current.mousePosition))
                {
                    hoveredMapMarker = marker;
                    hoveredMapMarkerRect = rect;
                }
            }
        }

        internal void DrawMinimapTooltip() => DrawMapMarkerTooltip();

        private void DrawMapMarkerTooltip()
        {
            if (hoveredMapMarker == null) return;
            string type = MapIconName(hoveredMapMarker.kind);
            if (hoveredMapMarker.activeAnchor) type += " · Active";
            var tooltip = hoveredMapMarker.name + "\n" + type;
            if (hoveredMapMarker.details.Count > 0)
                tooltip += "\n\n" + string.Join("\n", hoveredMapMarker.details);
            HudTooltip.DrawAt(tooltip, hoveredMapMarkerRect);
        }

        private Rect PlaceMapMarker(Rect plot, Vector2 position, List<Rect> occupied, List<Rect> labels)
        {
            Rect candidate = default;
            for (int attempt = 0; attempt < 120; attempt++)
            {
                float angle = attempt * 2.399963f;
                float radius = attempt == 0 ? 0 : 13 * Mathf.Sqrt(attempt);
                var center = position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                center.x = Mathf.Clamp(center.x, plot.x + 14, plot.xMax - 14);
                center.y = Mathf.Clamp(center.y, plot.y + 14, plot.yMax - 14);
                candidate = new Rect(center.x - MarkerSize / 2, center.y - MarkerSize / 2, MarkerSize, MarkerSize);
                bool blocked = false;
                foreach (var existing in occupied)
                {
                    var padded = new Rect(existing.x - 3, existing.y - 3, existing.width + 6, existing.height + 6);
                    if (padded.Overlaps(candidate)) { blocked = true; break; }
                }
                if (blocked) continue;
                foreach (var text in labels)
                    if (text.Overlaps(candidate)) { blocked = true; break; }
                if (!blocked) return candidate;
            }
            return candidate;
        }

        private void DrawMapMarkerLegend(Rect rail)
        {
            float y = rail.yMax - MapMarkerLegendHeight + 8;
            Line(new Vector2(rail.x + 14, y - 6), new Vector2(rail.xMax - 14, y - 6), 1, Frame);
            for (int i = 0; i < 6; i++)
            {
                var kind = (MapIconCategory)i;
                var center = new Vector2(rail.x + 26, y + 10);
                DrawMapIcon(center, kind, false);
                var preferences = GameplayPreferences.Current;
                bool current = preferences.IsMapIconVisible(kind);
                bool visible = GUI.Toggle(new Rect(rail.x + 44, y - 2, rail.width - 54, 24), current, MapIconName(kind));
                if (visible != current) preferences.SetMapIconVisible(kind, visible);
                y += 23;
            }
        }

        internal static string MapIconName(MapIconCategory kind) => kind switch
        {
            MapIconCategory.Merchant => "Merchant",
            MapIconCategory.ResurrectionDice => "Resurrection dice",
            MapIconCategory.MainQuest => "Main quest",
            MapIconCategory.RepeatableQuest => "Repeatable quest",
            MapIconCategory.QuestObjective => "Quest objectives",
            _ => "Side quest"
        };

        private static Color MarkerColor(MapIconCategory kind) => kind switch
        {
            MapIconCategory.Merchant => new Color(.98f, .72f, .28f),
            MapIconCategory.ResurrectionDice => new Color(.55f, .88f, .98f),
            MapIconCategory.MainQuest => new Color(1, .81f, .22f),
            MapIconCategory.RepeatableQuest => new Color(.36f, .68f, 1),
            MapIconCategory.QuestObjective => new Color(.20f, .80f, 1),
            _ => new Color(.92f, .88f, .72f)
        };

        private void DrawMapIcon(Vector2 center, MapIconCategory kind, bool ready)
        {
            Color tint = MarkerColor(kind);
            Fill(new Rect(center.x - 11, center.y - 11, 22, 22), new Color(.033f, .047f, .042f, .96f));
            var x = Vector2.right;
            var y = Vector2.up;
            switch (kind)
            {
                case MapIconCategory.QuestObjective:
                    Diamond(center, 7, 3, tint);
                    break;
                case MapIconCategory.Merchant:
                    Fill(new Rect(center.x - 6, center.y - 3, 12, 10), new Color(.42f, .25f, .09f));
                    Line(center + new Vector2(-6, -2), center + new Vector2(-6, 7), 1.7f, tint);
                    Line(center + new Vector2(6, -2), center + new Vector2(6, 7), 1.7f, tint);
                    Line(center + new Vector2(-5, 8), center + new Vector2(5, 8), 1.7f, tint);
                    Line(center - x * 5 - y * 2, center - x * 3 - y * 7, 1.7f, tint);
                    Line(center + x * 5 - y * 2, center + x * 3 - y * 7, 1.7f, tint);
                    Line(center - x * 4 - y * 7, center + x * 4 - y * 7, 1.7f, tint);
                    Line(center - x * 6 - y, center + x * 6 - y, 2, tint);
                    Diamond(center + y * 3, 2.7f, 1.5f, tint);
                    break;
                case MapIconCategory.ResurrectionDice:
                    var top = center - y * 9;
                    var left = center - x * 8 - y * 4;
                    var right = center + x * 8 - y * 4;
                    var bottom = center + y * 9;
                    Line(top, left, 1.7f, tint);
                    Line(top, right, 1.7f, tint);
                    Line(left, center, 1.7f, tint);
                    Line(right, center, 1.7f, tint);
                    Line(center, bottom, 1.7f, tint);
                    Line(left, left + y * 8, 1.7f, tint);
                    Line(right, right + y * 8, 1.7f, tint);
                    Line(left + y * 8, bottom, 1.7f, tint);
                    Line(right + y * 8, bottom, 1.7f, tint);
                    Fill(new Rect(center.x - 1, center.y - 6, 2.5f, 2.5f), Ink);
                    Fill(new Rect(center.x - 5, center.y, 2.5f, 2.5f), Ink);
                    Fill(new Rect(center.x + 3, center.y + 2, 2.5f, 2.5f), Ink);
                    break;
                case MapIconCategory.MainQuest:
                    Diamond(center, 10, 2, tint);
                    DrawQuestPunctuation(center, tint, ready);
                    break;
                case MapIconCategory.RepeatableQuest:
                    for (int i = 0; i < 12; i++)
                    {
                        float a = i * Mathf.PI / 8;
                        var first = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 9;
                        var second = center + new Vector2(Mathf.Cos(a + .27f), Mathf.Sin(a + .27f)) * 9;
                        Line(first, second, 1.8f, tint);
                    }
                    Line(center + new Vector2(8, -7), center + new Vector2(9, 1), 1.8f, tint);
                    Line(center + new Vector2(9, 1), center + new Vector2(3, -2), 1.8f, tint);
                    Line(center + new Vector2(-8, 7), center + new Vector2(-9, -1), 1.8f, tint);
                    Line(center + new Vector2(-9, -1), center + new Vector2(-3, 2), 1.8f, tint);
                    DrawQuestPunctuation(center, tint, ready);
                    break;
                case MapIconCategory.SideQuest:
                    Line(center + new Vector2(-7, -8), center + new Vector2(5, -8), 1.7f, tint);
                    Line(center + new Vector2(-7, -8), center + new Vector2(-7, 7), 1.7f, tint);
                    Line(center + new Vector2(7, -6), center + new Vector2(7, 8), 1.7f, tint);
                    Line(center + new Vector2(-7, 8), center + new Vector2(7, 8), 1.7f, tint);
                    Line(center + new Vector2(3, -8), center + new Vector2(7, -4), 2, tint);
                    DrawQuestPunctuation(center, tint, ready);
                    break;
            }
        }

        private void DrawQuestPunctuation(Vector2 center, Color tint, bool ready)
        {
            mapGlyphStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold, fontSize = 16, alignment = TextAnchor.MiddleCenter
            };
            mapGlyphStyle.normal.textColor = ready ? new Color(.42f, 1, .60f) : tint;
            GUI.Label(new Rect(center.x - 8, center.y - 11, 16, 22), ready ? "?" : "!", mapGlyphStyle);
        }
    }
}
