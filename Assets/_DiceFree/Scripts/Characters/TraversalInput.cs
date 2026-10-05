using UnityEngine;
using UnityEngine.InputSystem;
using DiceFree.UI;
using DiceFree.Combat;
using DiceFree.World;
using DiceFree.Foundation;
using DiceFree.Input;

namespace DiceFree.Characters
{
    [RequireComponent(typeof(TraversalMotor))]
    public sealed class TraversalInput : MonoBehaviour, ITraversalUiState
    {
        public enum ControlMode { Classic, Direct }
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Transform destinationMarker;
        [SerializeField] private ControlMode mode = ControlMode.Classic;

        private TraversalMotor motor;
        private BasicAttack attack;
        private CombatInput combatInput;
        private Health health;
        private Interactor interactor;
        private InputBindings bindings;
        private InputAction movement, click, changeMode, stop, interact, closeInteraction;

        public ControlMode Mode => mode;
        public string Feedback { get; private set; } = "Follow the path into Cornberg.";
        public string ModeLabel => mode == ControlMode.Classic ? "Classic · Right-click ground to move" : "Direct · WASD to move";

        private void Awake()
        {
            motor = GetComponent<TraversalMotor>();
            attack = GetComponent<BasicAttack>();
            combatInput = GetComponent<CombatInput>();
            health = GetComponent<Health>();
            interactor = GetComponent<Interactor>();

            bindings = InputBindings.Current;
            movement = bindings.Action("Gameplay/Direct movement");
            click = bindings.Action("Gameplay/Move destination");
            changeMode = bindings.Action("Gameplay/Switch control mode");
            stop = bindings.Action("Gameplay/Stop");
            interact = bindings.Action("Gameplay/Interact");
            closeInteraction = bindings.Action("Gameplay/Close interaction");
            bindings.SuppressionStarted += SuppressCommands;
        }

        private void OnDisable() => SuppressCommands();

        private void OnDestroy()
        {
            if (bindings != null) bindings.SuppressionStarted -= SuppressCommands;
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) SuppressCommands();
        }

        private void SuppressCommands()
        {
            motor?.Stop();
            attack?.Cancel();
            interactor?.Cancel();
            if (destinationMarker != null) destinationMarker.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (health != null && !health.Alive)
            {
                destinationMarker.gameObject.SetActive(false);
                return;
            }

            if (bindings.Suppressed)
            {
                destinationMarker.gameObject.SetActive(false);
                return;
            }

            if (changeMode.WasPressedThisFrame())
            {
                mode = mode == ControlMode.Classic ? ControlMode.Direct : ControlMode.Classic;
                motor.Stop();
                attack?.Cancel();
                interactor?.Cancel();
                Feedback = mode == ControlMode.Classic ? "Right-click the ground to walk." : "WASD moves relative to the camera.";
            }

            if (stop.WasPressedThisFrame())
            {
                motor.Stop();
                attack?.Cancel();
                interactor?.Cancel();
            }

            if (closeInteraction.WasPressedThisFrame()) interactor?.Cancel();
            if (interact.WasPressedThisFrame()) interactor?.InteractNearest();

            if (mode == ControlMode.Direct)
            {
                var input = movement.ReadValue<Vector2>();
                var forward = Vector3.ProjectOnPlane(worldCamera.transform.forward, Vector3.up).normalized;
                var right = Vector3.ProjectOnPlane(worldCamera.transform.right, Vector3.up).normalized;
                if (input.sqrMagnitude > 0.001f)
                {
                    interactor?.Cancel();
                    attack?.Cancel();
                    motor.MoveDirect(forward * input.y + right * input.x, Time.deltaTime);
                }
            }

            if (click.WasPressedThisFrame() && Mouse.current != null &&
                !HudPointerBlocker.Covers(Mouse.current.position.ReadValue()))
            {
                if (interactor != null && interactor.ContextInteract(worldCamera.ScreenPointToRay(Mouse.current.position.ReadValue()))) return;
                if (combatInput != null && combatInput.ContextAttack(Mouse.current.position.ReadValue()))
                {
                    interactor?.Cancel();
                    return;
                }

                if (mode != ControlMode.Classic) return;
                var ray = worldCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out var hit, 1500f, (1 << 8) | (1 << 9), QueryTriggerInteraction.Ignore)
                    && hit.collider.gameObject.layer == 8 && motor.MoveTo(hit.point))
                {
                    interactor?.Cancel();
                    if (attack != null && attack.Target != null)
                    {
                        attack.Cancel();
                        motor.MoveTo(hit.point);
                    }
                    destinationMarker.position = hit.point + Vector3.up * 0.08f;
                    Feedback = "";
                }
                else Feedback = "That spot is not reachable. Choose open ground.";
            }

            destinationMarker.gameObject.SetActive(mode == ControlMode.Classic && motor.Travelling);
        }

        public void Configure(Camera camera, Transform marker)
        {
            worldCamera = camera;
            destinationMarker = marker;
        }
    }
}
