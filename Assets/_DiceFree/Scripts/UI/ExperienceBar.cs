using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class ExperienceBar : HudWidget
    {
        [SerializeField] private ExperienceProgression progression;
        private float notificationUntil;
        private void Start() => progression.LeveledUp += OnLevel;
        private void OnDestroy() { if (progression != null) progression.LeveledUp -= OnLevel; }
        private void OnLevel(int level) => notificationUntil = Time.time+4;
        public override Rect Bounds => new Rect(16,Screen.height-16,Screen.width-32,16);
        private void OnGUI()
        {
            var rect = Bounds; GUI.Box(rect,GUIContent.none);
            var old = GUI.color; GUI.color = new Color(0.4f,0.55f,1);
            GUI.DrawTexture(new Rect(rect.x,rect.y,rect.width*progression.CurrentXp/progression.RequiredXp,rect.height),Texture2D.whiteTexture);
            GUI.color = old;
            GUI.Label(new Rect(Screen.width/2-120,rect.y-4,240,22),$"Level {progression.Level} · XP {progression.CurrentXp}/{progression.RequiredXp}");
            if (Time.time < notificationUntil) GUI.Box(new Rect(Screen.width/2-100,120,200,30),$"Level {progression.Level} reached!");
        }
        public void Configure(ExperienceProgression value) => progression = value;
    }
}
