using DiceFree.Foundation;
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
        private InputAction select, cycle, attackSelected, respawn;
        private void Awake()
        {
            selection = GetComponent<TargetSelection>(); attack = GetComponent<BasicAttack>();
            select = ControlBindings.Create("select");
            cycle = ControlBindings.Create("cycle");
            attackSelected = ControlBindings.Create("attackSelected");

            respawn = ControlBindings.Create("respawn");
        }
        private void OnEnable() { select.Enable(); cycle.Enable(); attackSelected.Enable();  respawn.Enable(); }
        private void OnDisable() { select.Disable(); cycle.Disable(); attackSelected.Disable();  respawn.Disable(); }
        private void OnDestroy() { ControlBindings.Release(select); ControlBindings.Release(cycle); ControlBindings.Release(attackSelected);  ControlBindings.Release(respawn); }
        private void Update()
        {
            if (ControlBindings.BlockGameplay) return;
            if (respawn.WasPressedThisFrame()) GetComponent<RespawnAtAnchor>()?.Return();
            if (cycle.WasPressedThisFrame()) selection.Cycle(Keyboard.current != null && Keyboard.current.shiftKey.isPressed);
            if (select.WasPressedThisFrame() && Mouse.current != null && !HudPointerBlocker.Covers(Mouse.current.position.ReadValue()))
                selection.Select(Pick(Mouse.current.position.ReadValue()));
            if (attackSelected.WasPressedThisFrame()) { GetComponent<Interactor>()?.Cancel(); attack.Order(selection.Selected); }
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
