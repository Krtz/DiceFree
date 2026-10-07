using System;
using System.Collections.Generic;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class ChatHud : CustomizableHudWidget
    {
        private readonly struct Message
        {
            public readonly string channel;
            public readonly string text;
            public readonly float time;

            public Message(string channel, string text, float time)
            {
                this.channel = channel;
                this.text = text;
                this.time = time;
            }
        }

        [SerializeField, Min(1)] private float visibleSeconds = 6f;
        [SerializeField, Min(0.1f)] private float fadeSeconds = 2f;
        [SerializeField] private ExperienceProgression progression;

        private readonly List<Message> messages = new();
        private Vector2 scroll;
        private float lastActivity;

        public override string LayoutId => "chat";
        public override string DisplayName => "Chat";
        public override Rect DefaultNormalizedBounds => new(0.014f, 0.525f, 0.30f, 0.16f);
        public override Vector2 MinimumPixelSize => new(280, 105);
        public override bool BlocksPointer =>
            (HudLayoutManager.Current?.EditMode ?? false) || Alpha > 0.05f;

        private float Alpha
        {
            get
            {
                if (HudLayoutManager.Current?.EditMode ?? false) return 0.78f;
                float quiet = Time.unscaledTime - lastActivity;
                if (quiet <= visibleSeconds) return 0.72f;
                return Mathf.Clamp01(1f - (quiet - visibleSeconds) / fadeSeconds) * 0.72f;
            }
        }

        public void Configure(ExperienceProgression value) => progression = value;

        private void Awake()
        {
            if (progression == null) progression = GetComponent<ExperienceProgression>();
            Post("System", "Welcome to Cornberg.");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (progression != null) progression.LeveledUp += OnLevel;
        }

        protected override void OnDisable()
        {
            if (progression != null) progression.LeveledUp -= OnLevel;
            base.OnDisable();
        }

        private void OnLevel(int level) => Post("System", "Level " + level + " reached.");

        public void Post(string channel, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            messages.Add(new Message(channel ?? "System", text.Trim(), Time.unscaledTime));
            while (messages.Count > 40) messages.RemoveAt(0);
            lastActivity = Time.unscaledTime;
            scroll.y = float.MaxValue;
        }

        private void OnGUI()
        {
            if (HudPointerBlocker.ModalOpen) return;
            float alpha = Alpha;
            if (alpha <= 0.01f) return;

            HudChrome.DrawPanel(Bounds, Theme.WithAlpha(alpha));

            Rect inner = Inner(Bounds);
            GUILayout.BeginArea(inner);
            scroll = GUILayout.BeginScrollView(scroll, false, false);
            var style = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true
            };

            var textColor = Theme.textTint;
            style.normal.textColor = new Color(
                textColor.r,
                textColor.g,
                textColor.b,
                Mathf.Clamp01(alpha + 0.2f));
            foreach (var message in messages)
                GUILayout.Label("[" + message.channel + "] " + message.text, style);

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
