using DiceFree.Combat;
using DiceFree.Input;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.Characters
{
    [RequireComponent(typeof(TargetSelection), typeof(BasicAttack))]
    public sealed class CombatInput : MonoBehaviour
    {
        private TargetSelection selection;
        private BasicAttack attack;
        private SkillTargetingController skillTargeting;
        private InputBindings bindings;
        private InputAction select, cycle, attackSelected, clear, respawn;

        private void Awake()
        {
            selection = GetComponent<TargetSelection>();
            attack = GetComponent<BasicAttack>();
            skillTargeting = GetComponent<SkillTargetingController>();
            bindings = InputBindings.Current;
            select = bindings.Action("Gameplay/Select target");
            cycle = bindings.Action("Gameplay/Cycle hostile");
            attackSelected = bindings.Action("Gameplay/Attack selected");
            clear = bindings.Action("Gameplay/Clear target");
            respawn = bindings.Action("Gameplay/Return to anchor");
        }

        private void Update()
        {
            if (bindings.Suppressed) return;
            if (skillTargeting != null && (skillTargeting.Active || skillTargeting.InputConsumedThisFrame)) return;

            if (respawn.WasPressedThisFrame()) GetComponent<RespawnAtAnchor>()?.Return();
            if (cycle.WasPressedThisFrame())
                selection.Cycle(Keyboard.current != null && Keyboard.current.shiftKey.isPressed);
            if (clear.WasPressedThisFrame())
            {
                selection.Select(null);
                attack.Cancel();
            }
            if (select.WasPressedThisFrame() && Mouse.current != null &&
                !HudPointerBlocker.Covers(Mouse.current.position.ReadValue()))
                selection.Select(Pick(Mouse.current.position.ReadValue()));
            if (attackSelected.WasPressedThisFrame())
            {
                GetComponent<Interactor>()?.Cancel();
                attack.Order(selection.Selected);
            }
        }

        private CombatActor Pick(Vector2 screenPoint)
        {
            var camera = Camera.main;
            if (camera != null && Physics.Raycast(camera.ScreenPointToRay(screenPoint), out var hit, 1500,
                (1 << 8) | (1 << 9) | (1 << 10), QueryTriggerInteraction.Ignore))
                return hit.collider.GetComponentInParent<CombatActor>();
            return null;
        }

        public bool ContextAttack(Vector2 point)
        {
            if (bindings.Suppressed) return false;
            var target = Pick(point);
            if (!selection.Valid(target)) return false;
            selection.Select(target);
            return attack.Order(target);
        }
    }
}
