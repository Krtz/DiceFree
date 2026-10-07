using System;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Foundation;
using DiceFree.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    public enum SkillTargetingMode
    {
        None,
        HostileUnit,
        FriendlyUnit,
        Ground
    }

    [DefaultExecutionOrder(-40)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatActor))]
    public sealed class SkillTargetingController : MonoBehaviour, IManifestationSessionState
    {
        [SerializeField] private Camera worldCamera;

        private CombatActor owner;
        private InputBindings bindings;
        private Func<CombatActor, bool> unitConfirm;
        private Func<Vector3, bool> groundConfirm;
        private int consumedFrame = -1;

        public SkillTargetingMode Mode { get; private set; }
        public string SkillLabel { get; private set; } = "";
        public string Feedback { get; private set; } = "";
        public bool Active => Mode != SkillTargetingMode.None;
        public bool InputConsumedThisFrame => consumedFrame == Time.frameCount;
        public CombatActor HoveredActor { get; private set; }
        public Vector3 HoveredGroundPoint { get; private set; }
        public bool HoverValid { get; private set; }

        public void Configure(Camera camera) => worldCamera = camera;

        private void Awake()
        {
            owner = GetComponent<CombatActor>();
            bindings = InputBindings.Current;
            bindings.SuppressionStarted += Cancel;
        }

        private void OnDestroy()
        {
            if (bindings != null) bindings.SuppressionStarted -= Cancel;
            RestoreCursor();
        }

        private void OnDisable()
        {
            Cancel();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) Cancel();
        }

        public void BeginHostile(string skillLabel, Func<CombatActor, bool> confirm) =>
            BeginUnit(SkillTargetingMode.HostileUnit, skillLabel, confirm);

        public void BeginFriendly(string skillLabel, Func<CombatActor, bool> confirm) =>
            BeginUnit(SkillTargetingMode.FriendlyUnit, skillLabel, confirm);

        public void BeginGround(string skillLabel, Func<Vector3, bool> confirm)
        {
            if (confirm == null) throw new ArgumentNullException(nameof(confirm));
            Mode = SkillTargetingMode.Ground;
            SkillLabel = skillLabel ?? "Skill";
            Feedback = "Choose a ground target.";
            unitConfirm = null;
            groundConfirm = confirm;
            HoveredActor = null;
            HoverValid = false;
            Cursor.visible = false;
        }

        private void BeginUnit(
            SkillTargetingMode mode,
            string skillLabel,
            Func<CombatActor, bool> confirm)
        {
            if (mode != SkillTargetingMode.HostileUnit && mode != SkillTargetingMode.FriendlyUnit)
                throw new ArgumentOutOfRangeException(nameof(mode));
            if (confirm == null) throw new ArgumentNullException(nameof(confirm));

            Mode = mode;
            SkillLabel = skillLabel ?? "Skill";
            Feedback = mode == SkillTargetingMode.HostileUnit
                ? "Choose a hostile target."
                : "Choose a friendly target.";
            unitConfirm = confirm;
            groundConfirm = null;
            HoveredActor = null;
            HoverValid = false;
            Cursor.visible = false;
        }

        public void Cancel()
        {
            Mode = SkillTargetingMode.None;
            SkillLabel = "";
            Feedback = "";
            unitConfirm = null;
            groundConfirm = null;
            HoveredActor = null;
            HoverValid = false;
            RestoreCursor();
        }

        public void ResetForManifestationLoad() => Cancel();

        private void Update()
        {
            if (!Active) return;

            if (bindings != null && bindings.Suppressed)
            {
                Cancel();
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Consume();
                Cancel();
                return;
            }

            if (Mouse.current == null) return;

            Vector2 pointer = Mouse.current.position.ReadValue();
            UpdateHover(pointer);

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Consume();
                Cancel();
                return;
            }

            if (!Mouse.current.leftButton.wasPressedThisFrame ||
                HudPointerBlocker.Covers(pointer))
                return;

            Consume();
            if (!HoverValid)
            {
                Feedback = Mode == SkillTargetingMode.Ground
                    ? "Choose reachable ground."
                    : "That is not a valid target.";
                return;
            }

            bool cast = Mode == SkillTargetingMode.Ground
                ? groundConfirm != null && groundConfirm(HoveredGroundPoint)
                : unitConfirm != null && unitConfirm(HoveredActor);

            if (cast)
                Cancel();
            else
                Feedback = "Cast rejected. Choose another target or cancel.";
        }

        private void UpdateHover(Vector2 pointer)
        {
            HoveredActor = null;
            HoverValid = false;

            if (Mode == SkillTargetingMode.Ground)
            {
                HoverValid = TryGroundPoint(pointer, out var point);
                HoveredGroundPoint = point;
                return;
            }

            HoveredActor = PickActor(pointer);
            HoverValid = Mode switch
            {
                SkillTargetingMode.HostileUnit => owner.IsHostileTo(HoveredActor),
                SkillTargetingMode.FriendlyUnit => owner.IsFriendlyTo(HoveredActor),
                _ => false
            };
        }

        private CombatActor PickActor(Vector2 screenPoint)
        {
            var camera = worldCamera != null ? worldCamera : Camera.main;
            if (camera == null) return null;

            var hits = Physics.RaycastAll(
                camera.ScreenPointToRay(screenPoint),
                1500f,
                (1 << 8) | (1 << 9) | (1 << 10),
                QueryTriggerInteraction.Ignore);

            foreach (var hit in hits.OrderBy(value => value.distance))
            {
                var candidate = hit.collider.GetComponentInParent<CombatActor>();
                if (candidate != null && candidate.Alive) return candidate;
            }

            return null;
        }

        private bool TryGroundPoint(Vector2 screenPoint, out Vector3 point)
        {
            point = default;
            var camera = worldCamera != null ? worldCamera : Camera.main;
            if (camera == null) return false;

            var hits = Physics.RaycastAll(
                camera.ScreenPointToRay(screenPoint),
                1500f,
                (1 << 8) | (1 << 9),
                QueryTriggerInteraction.Ignore);

            foreach (var hit in hits.OrderBy(value => value.distance))
            {
                if (hit.collider.gameObject.layer != 8) continue;
                point = hit.point;
                return true;
            }

            return false;
        }

        private void Consume() => consumedFrame = Time.frameCount;

        private static void RestoreCursor()
        {
            Cursor.visible = true;
        }

        private void OnGUI()
        {
            if (!Active || Mouse.current == null || HudPointerBlocker.ModalOpen) return;

            Vector2 raw = Mouse.current.position.ReadValue();
            Vector2 pointer = new(raw.x, Screen.height - raw.y);

            const float size = 28f;
            const float arm = 8f;
            const float thickness = 2f;
            GUI.Box(new Rect(pointer.x - thickness / 2f, pointer.y - size / 2f, thickness, arm), GUIContent.none);
            GUI.Box(new Rect(pointer.x - thickness / 2f, pointer.y + size / 2f - arm, thickness, arm), GUIContent.none);
            GUI.Box(new Rect(pointer.x - size / 2f, pointer.y - thickness / 2f, arm, thickness), GUIContent.none);
            GUI.Box(new Rect(pointer.x + size / 2f - arm, pointer.y - thickness / 2f, arm, thickness), GUIContent.none);

            string target = "";
            if (Mode != SkillTargetingMode.Ground && HoveredActor != null)
                target = "\n" + (HoverValid ? "Target: " : "Invalid: ") +
                         (HoveredActor.Stats?.Definition?.displayName ?? HoveredActor.name);

            string instruction = Mode switch
            {
                SkillTargetingMode.HostileUnit => "Click hostile",
                SkillTargetingMode.FriendlyUnit => "Click ally / self",
                SkillTargetingMode.Ground => "Click ground",
                _ => ""
            };

            var label = new Rect(pointer.x + 18, pointer.y + 14, 250, 56);
            GUI.Box(label, SkillLabel + " — " + instruction + target + "\nRMB / Esc cancel");
        }
    }
}
