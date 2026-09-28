using DiceFree.Combat;
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
        private InputAction select, cycle, attackSelected, clear, respawn;
        private void Awake()
        {
            selection = GetComponent<TargetSelection>(); attack = GetComponent<BasicAttack>();
            select = new InputAction("Select target", binding: "<Mouse>/leftButton");
            cycle = new InputAction("Cycle hostile", binding: "<Keyboard>/tab");
            attackSelected = new InputAction("Attack selected", binding: "<Keyboard>/x");
            clear = new InputAction("Clear target", binding: "<Keyboard>/escape");
            respawn = new InputAction("Return to anchor", binding: "<Keyboard>/r");
        }
        private void OnEnable() { select.Enable(); cycle.Enable(); attackSelected.Enable(); clear.Enable(); respawn.Enable(); }
        private void OnDisable() { select.Disable(); cycle.Disable(); attackSelected.Disable(); clear.Disable(); respawn.Disable(); }
        private void OnDestroy() { select.Dispose(); cycle.Dispose(); attackSelected.Dispose(); clear.Dispose(); respawn.Dispose(); }
        private void Update()
        {
            if (respawn.WasPressedThisFrame()) GetComponent<RespawnAtAnchor>()?.Return();
            if (cycle.WasPressedThisFrame()) selection.Cycle(Keyboard.current != null && Keyboard.current.shiftKey.isPressed);
            if (clear.WasPressedThisFrame()) { selection.Select(null); attack.Cancel(); }
            if (select.WasPressedThisFrame() && Mouse.current != null && !HudPointerBlocker.Covers(Mouse.current.position.ReadValue()))
                selection.Select(Pick(Mouse.current.position.ReadValue()));
            if (attackSelected.WasPressedThisFrame()) attack.Order(selection.Selected);
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
            var target = Pick(point);
            if (!selection.Valid(target)) return false;
            selection.Select(target); return attack.Order(target);
        }
    }
}
