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
        Ground,
        AttackMove
    }

    [DefaultExecutionOrder(-40)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatActor))]
    public sealed class SkillTargetingController : MonoBehaviour, IManifestationSessionState
    {
        [SerializeField] private Camera worldCamera;

        private CombatActor owner;
        private InputBindings bindings;
        private GameplayPreferences preferences;
        private Func<CombatActor, bool> unitConfirm;
        private Func<Vector3, bool> groundConfirm;
        private int consumedFrame = -1;
        private float castRange;

        private CombatActor pendingUnit;
        private Vector3 pendingGround;
        private bool pendingGroundActive;

        private LineRenderer rangeLine;
        private Material rangeMaterial;

        public SkillTargetingMode Mode { get; private set; }
        public string SkillLabel { get; private set; } = "";
        public string Feedback { get; private set; } = "";
        public bool Active => Mode != SkillTargetingMode.None;
        public bool InputConsumedThisFrame => consumedFrame == Time.frameCount;
        public CombatActor HoveredActor { get; private set; }
        public Vector3 HoveredGroundPoint { get; private set; }
        public bool HoverValid { get; private set; }
        public float CastRange => castRange;

        public void Configure(Camera camera) => worldCamera = camera;
        public void BeginAttackMove(Func<CombatActor,bool> unit, Func<Vector3,bool> ground)
        {
            BeginGround("Attack",ground);
            Mode=SkillTargetingMode.AttackMove;unitConfirm=unit;Feedback="Click an enemy or ground to attack-move.";
        }

        private void Awake()
        {
            owner = GetComponent<CombatActor>();
            bindings = InputBindings.Current;
            preferences = GameplayPreferences.Current;
            bindings.SuppressionStarted += Cancel;
            CreateRangeLine();
        }

        private void OnDestroy()
        {
            if (bindings != null) bindings.SuppressionStarted -= Cancel;
            if (rangeMaterial != null) Destroy(rangeMaterial);
            RestoreCursor();
        }

        private void OnDisable() => Cancel();

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) Cancel();
        }

        public void BeginHostile(
            string skillLabel,
            Func<CombatActor, bool> confirm,
            float range = 0f) =>
            BeginUnit(SkillTargetingMode.HostileUnit, skillLabel, confirm, range);

        public void BeginFriendly(
            string skillLabel,
            Func<CombatActor, bool> confirm,
            float range = 0f) =>
            BeginUnit(SkillTargetingMode.FriendlyUnit, skillLabel, confirm, range);

        public void BeginGround(
            string skillLabel,
            Func<Vector3, bool> confirm,
            float range = 0f)
        {
            if (confirm == null) throw new ArgumentNullException(nameof(confirm));
            ClearPending(false);
            Mode = SkillTargetingMode.Ground;
            SkillLabel = skillLabel ?? "Skill";
            Feedback = "Choose a ground target.";
            castRange = Mathf.Max(0f, range);
            unitConfirm = null;
            groundConfirm = confirm;
            HoveredActor = null;
            HoverValid = false;
            Cursor.visible = false;
            RefreshRangeLine();
        }

        private void BeginUnit(
            SkillTargetingMode mode,
            string skillLabel,
            Func<CombatActor, bool> confirm,
            float range)
        {
            if (mode != SkillTargetingMode.HostileUnit &&
                mode != SkillTargetingMode.FriendlyUnit)
                throw new ArgumentOutOfRangeException(nameof(mode));
            if (confirm == null) throw new ArgumentNullException(nameof(confirm));

            ClearPending(false);
            Mode = mode;
            SkillLabel = skillLabel ?? "Skill";
            Feedback = mode == SkillTargetingMode.HostileUnit
                ? "Choose a hostile target."
                : "Choose a friendly target.";
            castRange = Mathf.Max(0f, range);
            unitConfirm = confirm;
            groundConfirm = null;
            HoveredActor = null;
            HoverValid = false;
            Cursor.visible = false;
            RefreshRangeLine();
        }

        public void Cancel()
        {
            ClearPending(true);
            Mode = SkillTargetingMode.None;
            SkillLabel = "";
            Feedback = "";
            castRange = 0f;
            unitConfirm = null;
            groundConfirm = null;
            HoveredActor = null;
            HoverValid = false;
            if (rangeLine != null) rangeLine.enabled = false;
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

            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                if(Mode!=SkillTargetingMode.AttackMove)Consume();
                Cancel();
                return;
            }

            if (UpdatePendingCast()) return;
            if (Mouse.current == null) return;

            Vector2 pointer = Mouse.current.position.ReadValue();
            UpdateHover(pointer);

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

            if(Mode==SkillTargetingMode.AttackMove)
            {
                bool ordered=HoveredActor!=null ? unitConfirm!=null&&unitConfirm(HoveredActor) : groundConfirm!=null&&groundConfirm(HoveredGroundPoint);
                if(ordered)Cancel();else Feedback="Cannot reach that target.";
                return;
            }

            if (Mode == SkillTargetingMode.Ground)
            {
                if (!GroundInRange(HoveredGroundPoint))
                {
                    HandleOutOfRangeGround(HoveredGroundPoint);
                    return;
                }

                bool cast = groundConfirm != null && groundConfirm(HoveredGroundPoint);
                if (cast) Cancel();
                else Feedback = "Cast rejected. Choose another target or cancel.";
                return;
            }

            if (!UnitInRange(HoveredActor))
            {
                HandleOutOfRangeUnit(HoveredActor);
                return;
            }

            bool unitCast = unitConfirm != null && unitConfirm(HoveredActor);
            if (unitCast) Cancel();
            else Feedback = "Cast rejected. Choose another target or cancel.";
        }

        private bool UpdatePendingCast()
        {
            if (pendingUnit != null)
            {
                if (!pendingUnit.Alive || !ValidUnitForMode(pendingUnit))
                {
                    ClearPending(true);
                    Feedback = "Target is no longer valid.";
                    return false;
                }

                if (UnitInRange(pendingUnit))
                {
                    owner.Motor.Stop();
                    var target = pendingUnit;
                    ClearPending(false);
                    if (unitConfirm != null && unitConfirm(target))
                    {
                        Cancel();
                        return true;
                    }

                    Feedback = "Cast rejected after moving into range.";
                    return false;
                }

                if (!owner.Motor.Travelling &&
                    !owner.Motor.MoveTo(pendingUnit.transform.position))
                {
                    ClearPending(false);
                    Feedback = "Cannot reach cast range.";
                    return false;
                }

                return true;
            }

            if (pendingGroundActive)
            {
                if (GroundInRange(pendingGround))
                {
                    owner.Motor.Stop();
                    Vector3 point = pendingGround;
                    ClearPending(false);
                    if (groundConfirm != null && groundConfirm(point))
                    {
                        Cancel();
                        return true;
                    }

                    Feedback = "Cast rejected after moving into range.";
                    return false;
                }

                if (!owner.Motor.Travelling && !owner.Motor.MoveTo(pendingGround))
                {
                    ClearPending(false);
                    Feedback = "Cannot reach cast range.";
                    return false;
                }

                return true;
            }

            return false;
        }

        private void HandleOutOfRangeUnit(CombatActor target)
        {
            if (preferences.OutOfRangeSkillBehavior == OutOfRangeSkillBehavior.DoNothing)
            {
                Feedback = "Target is out of range.";
                return;
            }

            if (!owner.Motor.MoveTo(target.transform.position))
            {
                Feedback = "Cannot reach cast range.";
                return;
            }

            pendingUnit = target;
            pendingGroundActive = false;
            Feedback = "Moving into cast range...";
        }

        private void HandleOutOfRangeGround(Vector3 point)
        {
            if (preferences.OutOfRangeSkillBehavior == OutOfRangeSkillBehavior.DoNothing)
            {
                Feedback = "Ground target is out of range.";
                return;
            }

            if (!owner.Motor.MoveTo(point))
            {
                Feedback = "Cannot reach cast range.";
                return;
            }

            pendingUnit = null;
            pendingGround = point;
            pendingGroundActive = true;
            Feedback = "Moving into cast range...";
        }

        private bool UnitInRange(CombatActor target)
        {
            if (target == null || target == owner) return target == owner;
            float reach = owner.Radius + target.Radius + castRange;
            return Vector3.Distance(owner.transform.position, target.transform.position) <= reach;
        }

        private bool GroundInRange(Vector3 point)
        {
            float reach = castRange;
            var a = new Vector2(owner.transform.position.x, owner.transform.position.z);
            var b = new Vector2(point.x, point.z);
            return Vector2.Distance(a, b) <= reach;
        }

        private bool ValidUnitForMode(CombatActor candidate) => Mode switch
        {
            SkillTargetingMode.HostileUnit or SkillTargetingMode.AttackMove => owner.IsHostileTo(candidate),
            SkillTargetingMode.FriendlyUnit => owner.IsFriendlyTo(candidate),
            _ => false
        };

        private void ClearPending(bool stopMovement)
        {
            bool hadPending = pendingUnit != null || pendingGroundActive;
            pendingUnit = null;
            pendingGroundActive = false;
            if (stopMovement && hadPending) owner?.Motor?.Stop();
        }

        private void UpdateHover(Vector2 pointer)
        {
            HoveredActor = null;
            HoverValid = false;

            if(Mode==SkillTargetingMode.AttackMove)
            {
                HoveredActor=PickActor(pointer);
                if(HoveredActor!=null){HoverValid=owner.IsHostileTo(HoveredActor);return;}
                HoverValid=TryGroundPoint(pointer,out var ground);HoveredGroundPoint=ground;return;
            }

            if (Mode == SkillTargetingMode.Ground)
            {
                HoverValid = TryGroundPoint(pointer, out var point);
                HoveredGroundPoint = point;
                return;
            }

            HoveredActor = PickActor(pointer);
            HoverValid = ValidUnitForMode(HoveredActor);
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

        private static void RestoreCursor() => Cursor.visible = true;

        private void CreateRangeLine()
        {
            var child = new GameObject("Skill Cast Range");
            child.transform.SetParent(transform, false);
            rangeLine = child.AddComponent<LineRenderer>();
            rangeLine.useWorldSpace = true;
            rangeLine.loop = true;
            rangeLine.positionCount = 65;
            rangeLine.widthMultiplier = 0.055f;
            rangeLine.numCornerVertices = 2;
            rangeLine.numCapVertices = 2;
            rangeLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rangeLine.receiveShadows = false;

            Shader shader =
                Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Unlit/Color");
            rangeMaterial = new Material(shader) { name = "Runtime Skill Range" };
            rangeMaterial.color = new Color(0.35f, 0.78f, 1f, 0.72f);
            rangeLine.sharedMaterial = rangeMaterial;
            rangeLine.enabled = false;
        }

        private void LateUpdate() => RefreshRangeLine();

        private void RefreshRangeLine()
        {
            if (rangeLine == null) return;
            bool visible = Active && preferences != null &&
                           preferences.ShowCastRange && castRange > 0.01f;
            rangeLine.enabled = visible;
            if (!visible) return;

            float radius = castRange;
            Vector3 center = owner.transform.position + Vector3.up * 0.08f;
            for (int i = 0; i < rangeLine.positionCount; i++)
            {
                float angle = i / (float)(rangeLine.positionCount - 1) * Mathf.PI * 2f;
                rangeLine.SetPosition(
                    i,
                    center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
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
            {
                string rangeState = UnitInRange(HoveredActor) ? "" : " · out of range";
                target = "\n" + (HoverValid ? "Target: " : "Invalid: ") +
                         (HoveredActor.Stats?.Definition?.displayName ?? HoveredActor.name) +
                         rangeState;
            }

            string instruction = Mode switch
            {
                SkillTargetingMode.HostileUnit => "Click hostile",
                SkillTargetingMode.FriendlyUnit => "Click ally / self",
                SkillTargetingMode.Ground => "Click ground",
                SkillTargetingMode.AttackMove => "Click enemy / attack-move ground",
                _ => ""
            };

            var label = new Rect(pointer.x + 18, pointer.y + 14, 280, 74);
            GUI.Box(
                label,
                SkillLabel + " — " + instruction + target +
                "\n" + Feedback + "\nRMB / Esc cancel");
        }
    }
}
