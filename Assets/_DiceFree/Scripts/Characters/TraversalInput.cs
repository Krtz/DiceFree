using UnityEngine;
using UnityEngine.InputSystem;
using DiceFree.UI;
using DiceFree.Combat;

namespace DiceFree.Characters
{
    [RequireComponent(typeof(TraversalMotor))]
    public sealed class TraversalInput : MonoBehaviour
    {
        public enum ControlMode { Classic, Direct }
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Transform destinationMarker;
        [SerializeField] private ControlMode mode = ControlMode.Classic;
        private TraversalMotor motor;
        private BasicAttack attack;
        private CombatInput combatInput;
        private Health health;
        private InputAction movement, click, changeMode, stop;
        public ControlMode Mode => mode;
        public string Feedback { get; private set; } = "Follow the path into Cornberg.";

        private void Awake()
        {
            motor = GetComponent<TraversalMotor>();
            attack = GetComponent<BasicAttack>(); combatInput = GetComponent<CombatInput>(); health = GetComponent<Health>();
            movement = new InputAction("Direct movement", InputActionType.Value);
            movement.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            click = new InputAction("Move destination", binding: "<Mouse>/rightButton");
            changeMode = new InputAction("Switch control mode", binding: "<Keyboard>/f6");
            stop = new InputAction("Stop", binding: "<Keyboard>/space");
        }

        private void OnEnable() { movement.Enable(); click.Enable(); changeMode.Enable(); stop.Enable(); }
        private void OnDisable()
        {
            movement.Disable(); click.Disable(); changeMode.Disable(); stop.Disable();
            if (motor != null) motor.Stop();
        }
        private void OnDestroy() { movement.Dispose(); click.Dispose(); changeMode.Dispose(); stop.Dispose(); }
        private void OnApplicationFocus(bool focused) { if (!focused) { motor.Stop(); attack?.Cancel(); } }

        private void Update()
        {
            if (health != null && !health.Alive) { destinationMarker.gameObject.SetActive(false); return; }
            if (changeMode.WasPressedThisFrame())
            {
                mode = mode == ControlMode.Classic ? ControlMode.Direct : ControlMode.Classic;
                motor.Stop();
                attack?.Cancel();
                Feedback = mode == ControlMode.Classic ? "Right-click the ground to walk." : "WASD moves relative to the camera.";
            }
            if (stop.WasPressedThisFrame()) { motor.Stop(); attack?.Cancel(); }
            if (mode == ControlMode.Direct)
            {
                var input = movement.ReadValue<Vector2>();
                var forward = Vector3.ProjectOnPlane(worldCamera.transform.forward, Vector3.up).normalized;
                var right = Vector3.ProjectOnPlane(worldCamera.transform.right, Vector3.up).normalized;
                if (input.sqrMagnitude > 0.001f) { attack?.Cancel(); motor.MoveDirect(forward * input.y + right * input.x, Time.deltaTime); }
            }
            if (click.WasPressedThisFrame() && Mouse.current != null &&
                     !HudPointerBlocker.Covers(Mouse.current.position.ReadValue()))
            {
                if (combatInput != null && combatInput.ContextAttack(Mouse.current.position.ReadValue())) return;
                if (mode != ControlMode.Classic) return;
                var ray = worldCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out var hit, 1500f, (1 << 8) | (1 << 9), QueryTriggerInteraction.Ignore)
                    && hit.collider.gameObject.layer == 8 && motor.MoveTo(hit.point))
                {
                    // MoveTo already installed the valid route; cancel only the attack command.
                    if (attack != null && attack.Target != null) { attack.Cancel(); motor.MoveTo(hit.point); }
                    destinationMarker.position = hit.point + Vector3.up * 0.08f;
                    Feedback = "";
                }
                else Feedback = "That spot is not reachable. Choose open ground.";
            }
            destinationMarker.gameObject.SetActive(mode == ControlMode.Classic && motor.Travelling);
        }

        public void Configure(Camera camera, Transform marker) { worldCamera = camera; destinationMarker = marker; }
    }
}
