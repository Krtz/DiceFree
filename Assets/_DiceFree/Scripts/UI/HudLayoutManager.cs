using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiceFree.Input;
using UnityEngine;

namespace DiceFree.UI
{
    [DefaultExecutionOrder(-9000)]
    [DisallowMultipleComponent]
    public sealed class HudLayoutManager : MonoBehaviour
    {
        [Serializable]
        private sealed class LayoutDocument
        {
            public int version = 1;
            public string themeId = HudThemes.CompactStone;
            public bool customTint;
            public float tintR = 1f;
            public float tintG = 1f;
            public float tintB = 1f;
            public List<LayoutEntry> widgets = new();
        }

        [Serializable]
        private sealed class LayoutEntry
        {
            public string id;
            public float x;
            public float y;
            public float width;
            public float height;

            public Rect Rect => new(x, y, width, height);

            public void Set(Rect value)
            {
                x = value.x;
                y = value.y;
                width = value.width;
                height = value.height;
            }
        }

        private static HudLayoutManager current;
        private readonly List<CustomizableHudWidget> widgets = new();

        private LayoutDocument document = new();
        private InputBindings bindings;
        private string activeDrag;
        private string activeResize;
        private Vector2 dragOffset;
        private bool dirty;

        public static HudLayoutManager Current
        {
            get
            {
                if (current == null && Application.isPlaying)
                    current = FindAnyObjectByType<HudLayoutManager>();
                return current;
            }
        }

        public bool EditMode { get; private set; }
        public string ThemeId => string.IsNullOrWhiteSpace(document.themeId)
            ? HudThemes.CompactStone
            : document.themeId;
        public bool CustomTintEnabled => document.customTint;
        public Color HudTint => new(
            Mathf.Clamp01(document.tintR),
            Mathf.Clamp01(document.tintG),
            Mathf.Clamp01(document.tintB),
            1f);
        public HudThemeMetrics Theme
        {
            get
            {
                HudThemeMetrics theme = HudThemes.Get(ThemeId);
                return document.customTint ? theme.WithUserTint(HudTint) : theme;
            }
        }

        private static string SettingsDirectory
        {
            get
            {
#if UNITY_EDITOR
                string isolated = Environment.GetEnvironmentVariable("DICEFREE_EDITOR_SETTINGS_ROOT");
                if (!string.IsNullOrEmpty(isolated)) return isolated;
                if (Environment.GetEnvironmentVariable("DICEFREE_DISABLE_PERSISTENCE") == "1" ||
                    !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DICEFREE_EDITOR_SAVE_ROOT")))
                    return null;
#endif
                return Path.Combine(Application.persistentDataPath, "Settings");
            }
        }

        private string LayoutPath =>
            SettingsDirectory == null ? null : Path.Combine(SettingsDirectory, "hud-layout.json");

        private void Awake()
        {
            current = this;
            bindings = InputBindings.Current;
            Load();

            // These session-level UI behaviors must never depend on an optional/debug panel being present.
            if (!TryGetComponent<DeathRespawnHud>(out _))
                gameObject.AddComponent<DeathRespawnHud>();
            if (!TryGetComponent<EscapeMenuController>(out _))
                gameObject.AddComponent<EscapeMenuController>();
        }

        private void OnDestroy()
        {
            if (current == this) current = null;
            if (EditMode) SetEditMode(false);
        }

        private void Update()
        {
            // HUD Edit Mode owns its modal input transition; do not rely on another UI
            // component being present just to resume Gameplay/Camera maps.
            if (bindings != null && bindings.Suppressed)
                bindings.Tick();
        }

        public void Register(CustomizableHudWidget widget)
        {
            if (widget != null && !widgets.Contains(widget)) widgets.Add(widget);
        }

        public void Unregister(CustomizableHudWidget widget) => widgets.Remove(widget);

        public Rect Resolve(CustomizableHudWidget widget)
        {
            if (widget == null) return default;
            Rect normalized = Entry(widget.LayoutId)?.Rect ?? widget.DefaultNormalizedBounds;
            normalized = SanitizeNormalized(normalized, widget.MinimumPixelSize, widget.LockedAspectRatio);
            return ToPixels(normalized);
        }

        public void SetEditMode(bool value)
        {
            if (EditMode == value) return;
            EditMode = value;
            activeDrag = activeResize = null;
            if (bindings == null) bindings = InputBindings.Current;

            if (value)
                bindings.SetModal(true);
            else
            {
                if (dirty) Save();
                bindings.SetModal(false);
            }
        }

        public void ToggleEditMode() => SetEditMode(!EditMode);

        public void ResetLayout()
        {
            document.widgets.Clear();
            dirty = true;
            Save();
        }

        public void CycleTheme() => SetTheme(HudThemes.Next(ThemeId));

        public void SetTheme(string id)
        {
            HudThemeMetrics resolved = HudThemes.Get(id);
            if (resolved.id == ThemeId) return;
            document.themeId = resolved.id;
            dirty = true;
            Save();
        }

        public void SetHudTint(Color tint)
        {
            document.customTint = true;
            document.tintR = Mathf.Clamp01(tint.r);
            document.tintG = Mathf.Clamp01(tint.g);
            document.tintB = Mathf.Clamp01(tint.b);
            dirty = true;
            Save();
        }

        public void ResetHudTint()
        {
            if (!document.customTint) return;
            document.customTint = false;
            dirty = true;
            Save();
        }

        public Rect NormalizedBounds(CustomizableHudWidget widget) =>
            widget == null ? default : ToNormalized(Resolve(widget));

        public void SetNormalizedBounds(CustomizableHudWidget widget, Rect normalized, bool persist = true)
        {
            if (widget == null) throw new ArgumentNullException(nameof(widget));
            normalized = SanitizeNormalized(normalized, widget.MinimumPixelSize, widget.LockedAspectRatio);
            EnsureEntry(widget).Set(normalized);
            dirty = true;
            if (persist) Save();
        }

        private LayoutEntry Entry(string id) =>
            document.widgets.FirstOrDefault(value => value != null && value.id == id);

        private LayoutEntry EnsureEntry(CustomizableHudWidget widget)
        {
            var entry = Entry(widget.LayoutId);
            if (entry != null) return entry;

            entry = new LayoutEntry { id = widget.LayoutId };
            entry.Set(widget.DefaultNormalizedBounds);
            document.widgets.Add(entry);
            return entry;
        }

        private Rect SanitizeNormalized(Rect value, Vector2 minimumPixels, float aspect)
        {
            float minWidth = Mathf.Clamp01(minimumPixels.x / Mathf.Max(1, Screen.width));
            float minHeight = Mathf.Clamp01(minimumPixels.y / Mathf.Max(1, Screen.height));
            value.width = Mathf.Clamp(value.width, minWidth, 1f);
            value.height = Mathf.Clamp(value.height, minHeight, 1f);

            if (aspect > 0.01f)
            {
                float pixelWidth = value.width * Screen.width;
                float pixelHeight = value.height * Screen.height;
                if (pixelHeight <= 0.01f) pixelHeight = minimumPixels.y;
                if (pixelWidth / pixelHeight > aspect)
                    pixelWidth = pixelHeight * aspect;
                else
                    pixelHeight = pixelWidth / aspect;
                value.width = Mathf.Max(minWidth, pixelWidth / Screen.width);
                value.height = Mathf.Max(minHeight, pixelHeight / Screen.height);
            }

            value.x = Mathf.Clamp(value.x, 0f, Mathf.Max(0, 1f - value.width));
            value.y = Mathf.Clamp(value.y, 0f, Mathf.Max(0, 1f - value.height));
            return value;
        }

        private static Rect ToPixels(Rect normalized) =>
            new(
                normalized.x * Screen.width,
                normalized.y * Screen.height,
                normalized.width * Screen.width,
                normalized.height * Screen.height);

        private static Rect ToNormalized(Rect pixels) =>
            new(
                pixels.x / Mathf.Max(1, Screen.width),
                pixels.y / Mathf.Max(1, Screen.height),
                pixels.width / Mathf.Max(1, Screen.width),
                pixels.height / Mathf.Max(1, Screen.height));

        private void OnGUI()
        {
            if (!EditMode) return;

            GUI.depth = -80;
            var theme = Theme;
            var oldColor = GUI.color;
            GUI.color = new Color(theme.panelTint.r, theme.panelTint.g, theme.panelTint.b, 0.98f);
            var toolbar = new Rect(Mathf.Max(8, Screen.width * 0.5f - 250), 8, 500, 38);
            GUI.Box(toolbar, GUIContent.none);
            GUI.color = oldColor;

            GUI.Label(new Rect(toolbar.x + 8, toolbar.y + 9, 190, 22),
                "HUD EDIT — drag panels / corner to resize");
            if (GUI.Button(new Rect(toolbar.x + 205, toolbar.y + 6, 95, 26), "Reset layout"))
                ResetLayout();
            if (GUI.Button(new Rect(toolbar.x + 304, toolbar.y + 6, 105, 26), Theme.displayName))
                CycleTheme();
            if (GUI.Button(new Rect(toolbar.x + 413, toolbar.y + 6, 78, 26), "Done"))
            {
                SetEditMode(false);
                return;
            }

            var evt = Event.current;
            Vector2 mouse = evt.mousePosition;

            foreach (var widget in widgets.ToArray())
            {
                if (widget == null || !widget.isActiveAndEnabled) continue;
                Rect rect = Resolve(widget);
                DrawOutline(rect, widget.DisplayName, theme);

                var grip = new Rect(rect.xMax - 18, rect.yMax - 18, 18, 18);
                GUI.Box(grip, "↘");

                if (evt.type == EventType.MouseDown && evt.button == 0)
                {
                    if (grip.Contains(mouse))
                    {
                        activeResize = widget.LayoutId;
                        activeDrag = null;
                        evt.Use();
                    }
                    else if (rect.Contains(mouse))
                    {
                        activeDrag = widget.LayoutId;
                        activeResize = null;
                        dragOffset = mouse - rect.position;
                        evt.Use();
                    }
                }

                bool dragging = activeDrag == widget.LayoutId;
                bool resizing = activeResize == widget.LayoutId;
                if (evt.type == EventType.MouseDrag && evt.button == 0 && (dragging || resizing))
                {
                    if (dragging)
                        rect.position = mouse - dragOffset;
                    else
                        rect.size = mouse - rect.position;

                    Rect normalized = SanitizeNormalized(
                        ToNormalized(rect),
                        widget.MinimumPixelSize,
                        widget.LockedAspectRatio);
                    EnsureEntry(widget).Set(normalized);
                    dirty = true;
                    evt.Use();
                }
            }

            if (evt.type == EventType.MouseUp && evt.button == 0 &&
                (activeDrag != null || activeResize != null))
            {
                activeDrag = activeResize = null;
                if (dirty) Save();
                evt.Use();
            }
        }

        private static void DrawOutline(Rect rect, string label, HudThemeMetrics theme)
        {
            var previous = GUI.color;
            GUI.color = new Color(theme.accentTint.r, theme.accentTint.g, theme.accentTint.b, 0.28f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = previous;
            GUI.Box(new Rect(rect.x + 3, rect.y + 3, Mathf.Min(rect.width - 24, 180), 22), label);
        }

        private void Load()
        {
            document = new LayoutDocument();
            string path = LayoutPath;
            if (string.IsNullOrEmpty(path)) return;

            foreach (string candidate in new[] { path, path + ".bak" })
            {
                if (!File.Exists(candidate)) continue;
                try
                {
                    var loaded = JsonUtility.FromJson<LayoutDocument>(File.ReadAllText(candidate));
                    if (loaded == null || loaded.version != 1 || loaded.widgets == null) continue;
                    document = loaded;
                    if (string.IsNullOrWhiteSpace(document.themeId))
                        document.themeId = HudThemes.CompactStone;
                    if (document.tintR <= 0f && document.tintG <= 0f && document.tintB <= 0f)
                    {
                        document.tintR = 1f;
                        document.tintG = 1f;
                        document.tintB = 1f;
                    }
                    return;
                }
                catch (Exception error) when (
                    error is IOException ||
                    error is UnauthorizedAccessException ||
                    error is ArgumentException)
                {
                    // Preserve unreadable files and fall back to defaults/backup.
                }
            }
        }

        private void Save()
        {
            string path = LayoutPath;
            if (string.IsNullOrEmpty(path))
            {
                dirty = false;
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string pending = path + ".pending";
            string backup = path + ".bak";
            string json = JsonUtility.ToJson(document, true);

            using (var writeLock = new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                try
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    using (var stream = new FileStream(pending, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Flush(true);
                    }

                    if (File.Exists(path)) File.Replace(pending, path, backup);
                    else File.Move(pending, path);
                }
                finally
                {
                    if (File.Exists(pending)) File.Delete(pending);
                }
            }

            dirty = false;
        }
    }
}
