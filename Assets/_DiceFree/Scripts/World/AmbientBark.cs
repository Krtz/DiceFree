using System;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    public sealed class AmbientBark : MonoBehaviour
    {
        public string npcId;
        public string[] lines = Array.Empty<string>();
        public CombatActor listener;
        [Min(0)] public float radius = 7, cooldown = 18, duration = 4;
        private IRandomSource random;
        private int previous = -1;
        private float next, visibleUntil;
        public string CurrentLine { get; private set; }
        public event Action<string> Spoken;
        public void SetRandom(IRandomSource source) => random = source ?? throw new ArgumentNullException(nameof(source));
        public bool TryBark(CombatActor actor, float now)
        {
            if (!isActiveAndEnabled || actor == null || !actor.Alive || lines.Length == 0 || now < next ||
                (actor.transform.position - transform.position).sqrMagnitude > radius * radius) return false;
            random ??= new SeededRandomSource(Guid.NewGuid().GetHashCode());
            int choices = lines.Length - (previous >= 0 && lines.Length > 1 ? 1 : 0);
            int selected = Math.Min(choices - 1, (int)(random.NextUnit() * choices));
            if (lines.Length > 1 && previous >= 0 && selected >= previous) selected++;
            previous = selected; CurrentLine = lines[selected]; next = now + cooldown; visibleUntil = now + duration;
            Spoken?.Invoke(CurrentLine); return true;
        }
        private void Update() => TryBark(listener, Time.time);
        private void OnGUI()
        {
            if (Time.time >= visibleUntil || string.IsNullOrEmpty(CurrentLine) || Camera.main == null) return;
            var point = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.7f);
            if (point.z > 0) GUI.Box(new Rect(point.x - 160, Screen.height - point.y - 44, 320, 44), CurrentLine);
        }
    }
}
