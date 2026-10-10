using DiceFree.Combat;
using DiceFree.Input;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.Characters
{
    [RequireComponent(typeof(TargetSelection), typeof(BasicAttack))]
    public sealed class CombatInput : MonoBehaviour, DiceFree.Foundation.IManifestationSessionState
    {
        private TargetSelection selection;
        private BasicAttack attack;
        private SkillTargetingController skillTargeting;
        private InputBindings bindings;
        private GameplayPreferences gameplayPreferences;
        private CombatActor actor;
        private IQueuedOrders orders;
        private InputAction select, cycle, attackSelected, clear, respawn;
        public bool AttackMoving {get;private set;}
        private Vector3 attackMoveDestination;
        public void CancelAttackMove()=>AttackMoving=false;
        public void ResetForManifestationLoad()=>CancelAttackMove();
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
            bool shift=Keyboard.current!=null && Keyboard.current.shiftKey.isPressed;
            if(!shift)
            {
                orders?.ClearOrders();
                CancelAttackMove();attack.Cancel();GetComponent<Interactor>()?.Cancel();
            }
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
            attack = GetComponent<BasicAttack>();
            skillTargeting = GetComponent<SkillTargetingController>();
            actor = GetComponent<CombatActor>();
            gameplayPreferences = GameplayPreferences.Current;
            bindings = InputBindings.Current;
            foreach(var item in GetComponents<MonoBehaviour>())
                if(item is IQueuedOrders found){orders=found;break;}
            bindings.SuppressionStarted+=CancelAttackMove;
            select = bindings.Action("Gameplay/Select target");
            cycle = bindings.Action("Gameplay/Cycle hostile");
            attackSelected = bindings.Action("Gameplay/Attack selected");
            clear = bindings.Action("Gameplay/Clear target");
            respawn = bindings.Action("Gameplay/Return to anchor");
        }

        private void OnEnable()
        {
            if (actor != null && actor.Health != null) actor.Health.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            CancelAttackMove();
            if (actor != null && actor.Health != null) actor.Health.Damaged -= OnDamaged;
        }
        private void OnDestroy(){if(bindings!=null)bindings.SuppressionStarted-=CancelAttackMove;}

        private void OnDamaged(CombatActor source, DamageResult result)
        {
            if (gameplayPreferences == null || !gameplayPreferences.AutoRetaliate) return;
            if (source == null || actor == null || !actor.IsHostileTo(source)) return;
            if (attack == null || attack.Target != null) return;
            if (skillTargeting != null && skillTargeting.Active) return;

            selection.Select(source);
            GetComponent<Interactor>()?.Cancel();
            attack.Order(source);
        }

        private void Update()
        {
            if (bindings.Suppressed || !actor.Alive) {CancelAttackMove();return;}
            if (skillTargeting != null && (skillTargeting.Active || skillTargeting.InputConsumedThisFrame)) return;
            if(AttackMoving&&Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){CancelAttackMove();attack.Cancel();return;}

            if (respawn.WasPressedThisFrame()) GetComponent<RespawnAtAnchor>()?.Return();
            if (cycle.WasPressedThisFrame())
                selection.Cycle(Keyboard.current != null && Keyboard.current.shiftKey.isPressed);
            if (clear.WasPressedThisFrame())
            {
                orders?.ClearOrders();
                CancelAttackMove();
                selection.Select(null);
                attack.Cancel();
            }
            if (select.WasPressedThisFrame() && Mouse.current != null &&
                !HudPointerBlocker.Covers(Mouse.current.position.ReadValue()))
                selection.Select(Pick(Mouse.current.position.ReadValue()));
            if (attackSelected.WasPressedThisFrame())
            {
                if(orders==null)
                    foreach(var item in GetComponents<MonoBehaviour>())
                        if(item is IQueuedOrders found){orders=found;break;}
                BeginAttackTargeting();
            }
            StepAttackMove();
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
    }
}
