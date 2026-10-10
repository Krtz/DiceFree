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
    public sealed class TraversalInput : MonoBehaviour, ITraversalUiState, IMinimapNavigator
    {
        public enum ControlMode { Classic, Direct }
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Transform destinationMarker;
        [SerializeField] private ControlMode mode = ControlMode.Classic;

        private TraversalMotor motor;
        private BasicAttack attack;
        private CombatInput combatInput;
        private IQueuedOrders orders;
        private SkillTargetingController skillTargeting;
        private Health health;
        private CombatActor actor;
        private Interactor interactor;
        private InputBindings bindings;
        private InputAction movement, click, changeMode, stop, interact, closeInteraction;

        public ControlMode Mode => mode;
        public Camera WorldCamera => worldCamera;
        public string Feedback { get; private set; } = "Follow the path into Cornberg.";
        public string ModeLabel => mode == ControlMode.Classic ? "Classic Â· Right-click ground to move" : "Direct Â· WASD to move";

        private void Awake()
        {
            motor = GetComponent<TraversalMotor>();
            attack = GetComponent<BasicAttack>();
            combatInput = GetComponent<CombatInput>();
            var queue=GetComponent<PlayerCommandQueue>();
            if(queue==null)queue=gameObject.AddComponent<PlayerCommandQueue>();
            orders=queue;
            if(GetComponent<PlayerAudioDirector>()==null)
                gameObject.AddComponent<PlayerAudioDirector>();
            if(GetComponent<DiceFree.Items.WorldDroppedItemLedger>()==null)
                gameObject.AddComponent<DiceFree.Items.WorldDroppedItemLedger>();
            if(GetComponent<DiceFree.Items.PlayerItemDropper>()==null)
                gameObject.AddComponent<DiceFree.Items.PlayerItemDropper>();
            skillTargeting = GetComponent<SkillTargetingController>();
            health = GetComponent<Health>();
            actor = GetComponent<CombatActor>();
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
            orders?.ClearOrders();
            combatInput?.CancelAttackMove();
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

            if (bindings.Suppressed || (actor != null && !actor.CanAct))
            {
                destinationMarker.gameObject.SetActive(false);
                return;
            }

            if (changeMode.WasPressedThisFrame())
            {
                orders?.ClearOrders();
                mode = mode == ControlMode.Classic ? ControlMode.Direct : ControlMode.Classic;
                motor.Stop();
                attack?.Cancel();
                interactor?.Cancel();
                Feedback = mode == ControlMode.Classic ? "Right-click the ground to walk." : "WASD moves relative to the camera.";
            }

            if (stop.WasPressedThisFrame())
            {
                orders?.ClearOrders();
                combatInput?.CancelAttackMove();
                motor.Stop();
                attack?.Cancel();
                interactor?.Cancel();
            }

            if (closeInteraction.WasPressedThisFrame() &&
                (skillTargeting == null || !skillTargeting.InputConsumedThisFrame))
                interactor?.Cancel();
            if (interact.WasPressedThisFrame()) {combatInput?.CancelAttackMove();interactor?.InteractNearest();}

            if (mode == ControlMode.Direct)
            {
                var input = movement.ReadValue<Vector2>();
                var forward = Vector3.ProjectOnPlane(worldCamera.transform.forward, Vector3.up).normalized;
                var right = Vector3.ProjectOnPlane(worldCamera.transform.right, Vector3.up).normalized;
                if (input.sqrMagnitude > 0.001f)
                {
                    orders?.ClearOrders();
                    combatInput?.CancelAttackMove();
                    interactor?.Cancel();
                    attack?.Cancel();
                    motor.MoveDirect(forward * input.y + right * input.x, Time.deltaTime);
                }
            }

            if ((skillTargeting == null || (!skillTargeting.Active && !skillTargeting.InputConsumedThisFrame)) &&
                click.WasPressedThisFrame() && Mouse.current != null &&
                !HudPointerBlocker.Covers(Mouse.current.position.ReadValue()))
            {
                bool shift=Keyboard.current!=null && Keyboard.current.shiftKey.isPressed;
                if(shift && mode==ControlMode.Classic)
                {
                    if(combatInput!=null &&
                       combatInput.ContextAttack(Mouse.current.position.ReadValue()))
                    {
                        Feedback="Shift queued attack ("+orders.PendingOrders+")";
                        return;
                    }
                    var queuedRay=worldCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                    if(Physics.Raycast(queuedRay,out var queuedHit,1500f,
                        (1<<8)|(1<<9),QueryTriggerInteraction.Ignore) &&
                       queuedHit.collider.gameObject.layer==8 &&
                       orders!=null && orders.SubmitMove(queuedHit.point,true))
                        Feedback="Shift queued move ("+orders.PendingOrders+")";
                    return;
                }
                orders?.ClearOrders();
                combatInput?.CancelAttackMove();
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

        /// <summary>
        /// Issue exactly the same NavMesh path command as a normal ground click,
        /// but with the target translated from the minimap's north-up projection.
        /// Does not teleport and works in either control mode.
        /// </summary>
        public bool TryMoveFromMinimap(Vector3 worldPoint) =>
            TryMoveFromMinimap(worldPoint,false);

        public bool TryMoveFromMinimap(Vector3 worldPoint,bool append)
        {
            if (motor == null || bindings == null || bindings.Suppressed ||
                (health != null && !health.Alive) ||
                (actor != null && !actor.CanAct) ||
                (skillTargeting != null && skillTargeting.Active) ||
                HudPointerBlocker.ModalOpen ||
                (HudLayoutManager.Current?.EditMode ?? false))
                return false;

            // Snap the proposed map point to the same walkable NavMesh as
            // ordinary movement. The actual route must be fully reachable.
            if (!UnityEngine.AI.NavMesh.SamplePosition(worldPoint, out var nearby,
                    2f, UnityEngine.AI.NavMesh.AllAreas))
            {
                Feedback = "No walkable ground at that minimap location.";
                return false;
            }
            if(append && orders!=null)
            {
                bool queued=orders.SubmitMove(nearby.position,true);
                if(queued) Feedback="Shift queued move ("+orders.PendingOrders+")";
                return queued;
            }
            orders?.ClearOrders();
            // Cancel movement-blocking orders BEFORE issuing the new NavMesh route.
            // In particular, CancelAttackMove and Interactor.Cancel may reset paths.
            combatInput?.CancelAttackMove();
            interactor?.Cancel();
            attack?.Cancel();
            if (!motor.MoveTo(nearby.position))
            {
                Feedback = "No walkable path to that minimap location.";
                return false;
            }
            if (destinationMarker != null)
            {
                destinationMarker.position=nearby.position+Vector3.up*.08f;
                destinationMarker.gameObject.SetActive(true);
            }
            Feedback="";
            return true;
        }

        public bool TryWorldPoint(Vector2 screenPoint, out Vector3 point)
        {
            point = default;
            if (worldCamera == null) return false;
            var ray = worldCamera.ScreenPointToRay(screenPoint);
            if (!Physics.Raycast(ray, out var hit, 1500f, (1 << 8) | (1 << 9), QueryTriggerInteraction.Ignore) ||
                hit.collider.gameObject.layer != 8) return false;
            point = hit.point;
            return true;
        }

        public void Configure(Camera camera, Transform marker)
        {
            worldCamera = camera;
            destinationMarker = marker;
        }
    }
}
