using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>Read-only bridge from an encounter owner to the common HUD.</summary>
    public interface IEncounterHudSource
    {
        CombatActor EncounterActor { get; }
        bool EngagedWith(CombatActor viewer);
        int LivingGuards { get; }
        string CastLabel { get; }
        float CastRemaining { get; }
        float CastDuration { get; }
    }

    [DisallowMultipleComponent]
    public sealed class EncounterHud : CustomizableHudWidget
    {
        private static readonly List<IEncounterHudSource> sources = new();
        private CombatActor player;
        private WorldFogOfWar fog;
        private IEncounterHudSource current;
        private float lastEngaged;
        public IEncounterHudSource Current => current;
        public bool Visible => current != null && current.EncounterActor != null;
        public override string LayoutId => "encounter";
        public override string DisplayName => "Boss Encounter";
        public override Rect DefaultNormalizedBounds => new(.32f, .022f, .36f, .105f);
        public override Vector2 MinimumPixelSize => new(330, 92);
        public override bool BlocksPointer => Visible || (HudLayoutManager.Current?.EditMode ?? false);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearSources() => sources.Clear();
        public static void Register(IEncounterHudSource source) { if (source != null && !sources.Contains(source)) sources.Add(source); }
        public static void Unregister(IEncounterHudSource source) => sources.Remove(source);
        private static bool Live(IEncounterHudSource source) => source is MonoBehaviour owner && owner != null && owner.isActiveAndEnabled &&
            source.EncounterActor != null && source.EncounterActor.Alive;
        private bool VisibleToPlayer(IEncounterHudSource source) => Live(source) && player != null && player.Alive &&
            Vector3.Distance(player.transform.position, source.EncounterActor.transform.position) <= 50 &&
            (fog == null || fog.IsVisible(source.EncounterActor.transform.position));
        private void Update()
        {
            if (player == null) player = FindAnyObjectByType<ReturnHomeAbility>()?.GetComponent<CombatActor>();
            if (fog == null) fog = FindAnyObjectByType<WorldFogOfWar>();
            if (current != null && (!VisibleToPlayer(current) || !sources.Contains(current))) current = null;
            float nearest = float.PositiveInfinity;
            IEncounterHudSource selected = null;
            foreach (var source in sources)
            {
                if (!VisibleToPlayer(source) || !source.EngagedWith(player)) continue;
                float distance = (source.EncounterActor.transform.position - player.transform.position).sqrMagnitude;
                if (distance < nearest) { nearest = distance; selected = source; }
            }
            if (selected != null) { current = selected; lastEngaged = Time.time; }
            // A smash temporarily cancels BasicAttack. Keep its readout steady across recovery.
            else if (Time.time - lastEngaged > 1.2f) current = null;
        }
        private void OnGUI()
        {
            if (HudPointerBlocker.ModalOpen) return;
            bool editing = HudLayoutManager.Current?.EditMode ?? false;
            if (!Visible && !editing) return;
            DrawPanel(Bounds);
            var inner = Inner(Bounds);
            var boss = current?.EncounterActor;
            string title = boss != null ? boss.DisplayName + "  ·  Lv " + boss.Stats.Level : "Boss encounter";
            HudChrome.DrawHeader(new Rect(inner.x, inner.y, inner.width, 23), Theme, title);
            var hp = new Rect(inner.x, inner.y + 26, inner.width, 19);
            float fraction = boss == null ? .72f : Mathf.Clamp01(boss.Health.Current / boss.Health.Maximum);
            DrawBar(hp, fraction, new Color(.7f, .15f, .12f), boss != null ?
                $"{boss.Health.Current:0} / {boss.Health.Maximum:0}   ({fraction:P0})" : "Boss health");
            float remaining = current?.CastRemaining ?? 0;
            var cue = new Rect(inner.x, hp.yMax + 4, inner.width, 19);
            if (remaining > 0)
            {
                float completion = 1 - Mathf.Clamp01(remaining / Mathf.Max(.01f, current.CastDuration));
                DrawBar(cue, completion, new Color(.83f, .31f, .11f), current.CastLabel + $"  {remaining:0.0}s");
            }
            else GUI.Label(cue, boss != null ? $"World boss  ·  {current.LivingGuards} guards standing" : "Attack warning / surviving guards",
                new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 11, normal = { textColor = Theme.textTint } });
        }
    }
}
