using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Input;
using DiceFree.Skills;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.Characters
{
    [RequireComponent(typeof(TargetSelection), typeof(BasicAttack))]
    public sealed class CombatInput : MonoBehaviour, DiceFree.Foundation.IManifestationSessionState, ISharedCombatCommands
    {
        private TargetSelection selection;
        private InspectSelection inspect;
        private BasicAttack attack;
        private SkillTargetingController skillTargeting;
        private InputBindings bindings;
        private GameplayPreferences gameplayPreferences;
        private CombatActor actor;
        private IQueuedOrders orders;
        private InputAction select, selectHero, cycle, attackSelected, clear, respawn;
        private InputAction directMovement, moveDestination, stop;
        private TraversalInput traversal;
        private Interactor interactor;
        private const float NearbyAttackRadius = 6f;
        private float nextNearbyAttack;
        private const float HeroDoublePressSeconds = .32f;
        private double lastHeroPress = double.NegativeInfinity;
        public bool AttackMoving {get;private set;}
        private Vector3 attackMoveDestination;
        public void CancelAttackMove()=>AttackMoving=false;
        public void ResetForManifestationLoad()
        {
            CancelAttackMove();
            lastHeroPress = double.NegativeInfinity;
        }
        public bool OrderAttackMove(Vector3 point)
        {
            if(!actor.CanAct)return false;
            attack.Cancel();GetComponent<Interactor>()?.Cancel();
            if(!actor.Motor.MoveTo(point))return false;
            attackMoveDestination=point;AttackMoving=true;return true;
        }
        public void BeginAttackTargeting()
        {
            if(!actor.CanAct||skillTargeting==null)return;
            if (SelectedSkeleton is AcolyteSkeleton skeleton)
            {
                skillTargeting.BeginAttackMove(skeleton.OrderAttack, skeleton.OrderMove);
                return;
            }
            GetComponent<ReturnHomeAbility>()?.Cancel();
            // Starting the targeting cursor is not issuing the order.
            // In WC3 you may press X first, THEN hold Shift when choosing
            // the target, so preserve the queue until confirmation.
            // X only arms the attack-move targeting cursor. It MUST NOT stop
            // the currently executing move/attack or erase its queued successors.
            // Cancellation belongs to the destination confirmation (unshifted)
            // or to an explicit Escape/Stop command.
            skillTargeting.BeginAttackMove(target=>{selection.Select(target);return attack.Order(target);},OrderAttackMove);
        }
        private void StepAttackMove()
        {
            if(!AttackMoving||!actor.CanAct)return;
            if(attack.Target!=null)return;
            CombatActor nearest=null;float best=36;
            foreach(var enemy in CombatActor.All)
            {
                float d=(enemy.transform.position-transform.position).sqrMagnitude;
                if(d<best&&actor.IsHostileTo(enemy)&&actor.HasSightOf(enemy)){best=d;nearest=enemy;}
            }
            if(nearest!=null){selection.Select(nearest);attack.Order(nearest);return;}
            if(Vector3.ProjectOnPlane(transform.position-attackMoveDestination,Vector3.up).sqrMagnitude<.25f){CancelAttackMove();return;}
            if(!actor.Motor.Travelling&&!actor.Motor.MoveTo(attackMoveDestination))CancelAttackMove();
        }

        private void Awake()
        {
            selection = GetComponent<TargetSelection>();
            inspect=GetComponent<InspectSelection>();
            if(inspect==null)inspect=gameObject.AddComponent<InspectSelection>();
            attack = GetComponent<BasicAttack>();
            traversal = GetComponent<TraversalInput>();
            interactor = GetComponent<Interactor>();
            skillTargeting = GetComponent<SkillTargetingController>();
            actor = GetComponent<CombatActor>();
            if(GetComponent<WorldHoverFeedback>() == null) gameObject.AddComponent<WorldHoverFeedback>();
            gameplayPreferences = GameplayPreferences.Current;
            GetComponent<AcolyteSkillCaster>()?.SetAutoRetaliateSource(() => gameplayPreferences.AutoRetaliate);
            bindings = InputBindings.Current;
            foreach(var item in GetComponents<MonoBehaviour>())
                if(item is IQueuedOrders found){orders=found;break;}
            bindings.SuppressionStarted+=CancelAttackMove;
            select = bindings.Action("Gameplay/Select target");
            selectHero = bindings.Action("Gameplay/Select hero");
            directMovement = bindings.Action("Gameplay/Direct movement");
            moveDestination = bindings.Action("Gameplay/Move destination");
            stop = bindings.Action("Gameplay/Stop");
            cycle = bindings.Action("Gameplay/Cycle hostile");
            attackSelected = bindings.Action("Gameplay/Attack selected");
            clear = bindings.Action("Gameplay/Clear target");
            // R is Return Home when alive; the death HUD still owns R on death.
            respawn = bindings.Action("Gameplay/Return to anchor");
        }

        private void OnEnable()
        {
            if (actor != null && actor.Health != null) actor.Health.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            CancelAttackMove();
            lastHeroPress = double.NegativeInfinity;
            if (actor != null && actor.Health != null) actor.Health.Damaged -= OnDamaged;
        }
        private void OnDestroy(){if(bindings!=null)bindings.SuppressionStarted-=CancelAttackMove;}

        private void OnDamaged(CombatActor source, DamageResult result)
        {
            if (gameplayPreferences?.Retaliation != RetaliationMode.AttackAttacker ||
                !CanAutoAttack() || !actor.IsHostileTo(source) || !attack.Order(source)) return;
            selection.Select(source);
            interactor?.Cancel();
        }

        private bool CanAutoAttack()
        {
            if (bindings.Suppressed || actor == null || !actor.CanAct || attack == null || !attack.isActiveAndEnabled ||
                attack.Target != null || skillTargeting?.Active == true || AttackMoving || actor.Motor.Travelling ||
                orders?.HasOrders == true || moveDestination.WasPressedThisFrame() || stop.IsPressed() ||
                (traversal?.Mode == TraversalInput.ControlMode.Direct && directMovement.ReadValue<Vector2>().sqrMagnitude > .001f))
                return false;
            foreach (var ability in GetComponents<MonoBehaviour>())
                if (ability is DiceFree.Foundation.IChannelingAbility channel && channel.IsChanneling) return false;
            return true;
        }

        private void StepNearbyAttack()
        {
            if (gameplayPreferences.Retaliation != RetaliationMode.AttackNearbyEnemies) return;
            if (stop.WasPressedThisFrame() || clear.WasPressedThisFrame() || select.WasPressedThisFrame() ||
                selectHero.WasPressedThisFrame() || cycle.WasPressedThisFrame())
                nextNearbyAttack = Time.time + 1f;
            if (Time.time < nextNearbyAttack) return;
            nextNearbyAttack = Time.time + .25f;
            if (actor.Concealed || interactor?.Active != null || GetComponent<WorldAtlasHud>()?.Open == true ||
                !CanAutoAttack()) return;

            CombatActor nearest = null;
            float best = NearbyAttackRadius * NearbyAttackRadius;
            foreach (var enemy in CombatActor.All)
            {
                if (enemy == null) continue;
                float distance = (enemy.transform.position - transform.position).sqrMagnitude;
                if (distance > best || !selection.ValidHostile(enemy) ||
                    enemy.GetComponent<AggroBehaviour>()?.AutoAggroPolicy?.retaliationOnly == true ||
                    !actor.HasSightOf(enemy)) continue;
                best = distance;
                nearest = enemy;
            }
            if (nearest != null && attack.Order(nearest)) selection.Select(nearest);
        }
        private void Update()
        {
            if (bindings.Suppressed) {CancelAttackMove();lastHeroPress=double.NegativeInfinity;return;}
            if (!actor.Alive)
            {
                CancelAttackMove();
                // Keep the original R death-respawn binding; living R now
                // starts the separate shared Return Home travel ability.
                if(respawn.WasPressedThisFrame())
                    GetComponent<RespawnAtAnchor>()?.Return();
                return;
            }
            if (selectHero.WasPressedThisFrame() && GetComponent<WorldAtlasHud>()?.Open != true)
                SelectHero();
            if (skillTargeting != null && (skillTargeting.Active || skillTargeting.InputConsumedThisFrame)) return;
            if(AttackMoving&&Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){CancelAttackMove();attack.Cancel();return;}

            if (respawn.WasPressedThisFrame())
                GetComponent<ReturnHomeAbility>()?.TryBegin();
            if (cycle.WasPressedThisFrame())
                selection.Cycle(Keyboard.current != null && Keyboard.current.shiftKey.isPressed);
            if (clear.WasPressedThisFrame())
            {
                orders?.ClearOrders();
                CancelAttackMove();
                selection.Select(null);
                inspect?.Clear();
                attack.Cancel();
                GetComponent<MageSkillCaster>()?.CancelChannel();
            }
            if (select.WasPressedThisFrame() && Mouse.current != null &&
                !HudPointerBlocker.Covers(Mouse.current.position.ReadValue()))
            {
                Vector2 point=Mouse.current.position.ReadValue();
                var picked=WorldTargetPicker.Pick(actor,Camera.main,point);
                selection.Select(picked.Actor);
                inspect?.Show(picked.Actor,picked.Interaction);
            }
            if (attackSelected.WasPressedThisFrame())
            {
                if(orders==null)
                    foreach(var item in GetComponents<MonoBehaviour>())
                        if(item is IQueuedOrders found){orders=found;break;}
                BeginAttackTargeting();
            }
            StepAttackMove();
            StepNearbyAttack();
        }

        private void SelectHero()
        {
            selection.Select(actor);
            inspect.Clear();

            double now = Time.unscaledTimeAsDouble;
            if (now - lastHeroPress <= HeroDoublePressSeconds)
            {
                GetComponent<TraversalInput>()?.WorldCamera?.GetComponent<ExplorationCamera>()?.FocusOnHero();
                lastHeroPress = double.NegativeInfinity;
            }
            else lastHeroPress = now;
        }

        private CombatActor Pick(Vector2 screenPoint)
        {
            return WorldTargetPicker.Pick(actor,Camera.main,screenPoint).Actor;
        }

        public bool ContextAttack(Vector2 point)
        {
            if(bindings.Suppressed)return false;
            var target=Pick(point);
            if(!selection.Valid(target))return false;
            bool append=Keyboard.current!=null&&Keyboard.current.shiftKey.isPressed;
            if(orders==null)
                foreach(var item in GetComponents<MonoBehaviour>())
                    if(item is IQueuedOrders found){orders=found;break;}
            if(append && orders!=null)
                return orders.SubmitAttackTarget(target,t=>
                {
                    selection.Select(t);
                    return attack.Order(t);
                },true);
            orders?.ClearOrders();
            CancelAttackMove();
            selection.Select(target);
            return attack.Order(target);
        }

        private AcolyteSkeleton SelectedSkeleton
        {
            get
            {
                var target = selection?.Selected;
                if (target == null || !target.Alive) return null;
                var skeleton = target.GetComponent<AcolyteSkeleton>();
                return skeleton != null && skeleton.Owner == actor ? skeleton : null;
            }
        }

        public bool TryOrderSelectedSkeleton(Vector2 pointer, Camera camera)
        {
            var skeleton = SelectedSkeleton;
            if (skeleton == null || camera == null) return false;
            var target = Pick(pointer);
            if (target != null && skeleton.Actor.IsHostileTo(target)) skeleton.OrderAttack(target);
            else if (ArboriumInterior.RaycastGround(camera.ScreenPointToRay(pointer), out var ground))
                skeleton.OrderMove(ground.point);
            return true;
        }

        public bool TryStopSelectedSkeleton()
        {
            var skeleton = SelectedSkeleton;
            if (skeleton == null) return false;
            skeleton.Stop();
            return true;
        }
    }
}
