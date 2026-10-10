using System;
using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.Input;
using DiceFree.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace DiceFree.Characters
{
    /// <summary>
    /// Ordered player commands, WC3-style: Shift appends, ordinary commands
    /// replace. Actions execute through the existing combat and NavMesh APIs.
    /// </summary>
    [DefaultExecutionOrder(95)]
    [RequireComponent(typeof(TraversalMotor))]
    public sealed class PlayerCommandQueue : MonoBehaviour, IQueuedOrders
    {
        private enum Kind { Move, AttackMove, AttackTarget, UnitSkill, GroundSkill, Instant }
        private sealed class Order
        {
            public Kind kind;
            public Vector3 point;
            public CombatActor target;
            public float range;
            public Func<Vector3,bool> ground;
            public Func<CombatActor,bool> unit;
            public Func<bool> instant;
            public bool started;
            public float startedAt;
        }

        private const int MaxQueue = 24;
        private const float OrderTimeout = 55f;
        private readonly Queue<Order> pending = new();
        private Order current;
        private TraversalMotor motor;
        private CombatActor actor;
        private BasicAttack attack;
        private CombatInput combat;
        private InputBindings bindings;
        private NavMeshAgent agent;

        public int PendingOrders => pending.Count + (current == null ? 0 : 1);
        public bool HasOrders => PendingOrders > 0;
        public event Action Changed;

        private void Awake()
        {
            motor = GetComponent<TraversalMotor>();
            actor = GetComponent<CombatActor>();
            attack = GetComponent<BasicAttack>();
            combat = GetComponent<CombatInput>();
            agent = GetComponent<NavMeshAgent>();
            bindings = InputBindings.Current;
            bindings.SuppressionStarted += ClearOrders;
        }

        private void OnDisable() => ClearOrders();
        private void OnDestroy()
        {
            if(bindings!=null) bindings.SuppressionStarted-=ClearOrders;
        }

        public void ClearOrders()
        {
            if(!HasOrders)return;
            pending.Clear();
            current=null;
            combat?.CancelAttackMove();
            motor?.Stop();
            Changed?.Invoke();
        }

        private bool Add(Order order, bool append)
        {
            if(actor==null || !actor.Alive || !actor.CanAct ||
               bindings==null || bindings.Suppressed)return false;
            if(!append) ClearOrders();
            if(PendingOrders >= MaxQueue)return false;
            pending.Enqueue(order);
            var audio=GetComponent<PlayerAudioDirector>();
            if(audio!=null)audio.Play("UiSelect",.35f);
            Changed?.Invoke();
            return true;
        }

        public bool SubmitMove(Vector3 point, bool append)
        {
            if(!NavMesh.SamplePosition(point,out var hit,2f,NavMesh.AllAreas))
                return false;
            return Add(new Order{kind=Kind.Move,point=hit.position},append);
        }

        public bool SubmitAttackMove(Vector3 point,
            Func<Vector3,bool> attackMove,bool append)
        {
            if(attackMove==null ||
               !NavMesh.SamplePosition(point,out var hit,2f,NavMesh.AllAreas))
                return false;
            return Add(new Order{kind=Kind.AttackMove,
                point=hit.position,ground=attackMove},append);
        }

        public bool SubmitAttackTarget(CombatActor target,
            Func<CombatActor,bool> action,bool append)
        {
            if(target==null||!target.Alive||action==null)return false;
            return Add(new Order{kind=Kind.AttackTarget,target=target,unit=action},append);
        }

        public bool SubmitUnit(CombatActor target,float range,
            Func<CombatActor,bool> action,bool append)
        {
            if(target==null || action==null || !target.Alive)return false;
            return Add(new Order{kind=Kind.UnitSkill,target=target,
                range=Mathf.Max(0,range),unit=action},append);
        }

        public bool SubmitGround(Vector3 point,float range,
            Func<Vector3,bool> action,bool append)
        {
            if(action==null)return false;
            return Add(new Order{kind=Kind.GroundSkill,point=point,
                range=Mathf.Max(0,range),ground=action},append);
        }

        public bool SubmitAction(Func<bool> action,bool append)
        {
            return action!=null &&
                Add(new Order{kind=Kind.Instant,instant=action},append);
        }

        private void Update()
        {
            if(!HasOrders)return;
            if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ClearOrders();
                attack?.Cancel();
                return;
            }
            if(actor==null || !actor.Alive || bindings==null || bindings.Suppressed)
            {
                ClearOrders();
                return;
            }
            // Limit dispatch to one new order per frame. This prevents queued
            // spells from all firing in one Update and respects cooldown checks.
            if(current==null)
            {
                current=pending.Dequeue();
                Changed?.Invoke();
            }
            var order=current;
            if(Time.time-order.startedAt>OrderTimeout && order.started)
            {
                Complete();
                return;
            }
            switch(order.kind)
            {
                case Kind.Move:
                {
                    if(!order.started)
                    {
                        combat?.CancelAttackMove();
                        attack?.Cancel();
                        if(!motor.MoveTo(order.point)) { Complete();return; }
                        order.started=true;order.startedAt=Time.time;
                    }
                    if(Reached(order.point))Complete();
                    else if(!agent.pathPending && !motor.Travelling &&
                        !motor.MoveTo(order.point))Complete();
                    break;
                }
                case Kind.AttackMove:
                {
                    if(!order.started)
                    {
                        if(!order.ground(order.point)) {Complete();return;}
                        order.started=true;order.startedAt=Time.time;
                    }
                    if(!combat.AttackMoving && !motor.Travelling &&
                        (attack==null || attack.Target==null))Complete();
                    break;
                }
                case Kind.AttackTarget:
                {
                    if(order.target==null || !order.target.Alive)
                    {
                        Complete();return;
                    }
                    if(!order.started)
                    {
                        if(!order.unit(order.target)){Complete();return;}
                        order.started=true;order.startedAt=Time.time;
                    }
                    if(attack==null || attack.Target==null)Complete();
                    break;
                }
                case Kind.UnitSkill:
                {
                    if(order.target==null || !order.target.Alive)
                    {
                        Complete();return;
                    }
                    float reach=order.range+actor.Radius+order.target.Radius;
                    var point=order.target.transform.position;
                    if(FlatDistance(point)<=reach+.12f)
                    {
                        if(!order.started){order.started=true;order.startedAt=Time.time;}
                        motor.Stop();
                        if(order.unit(order.target))Complete();
                        return;
                    }
                    if(!motor.Travelling && !motor.MoveTo(point))Complete();
                    if(!order.started){order.started=true;order.startedAt=Time.time;}
                    break;
                }
                case Kind.GroundSkill:
                {
                    if(FlatDistance(order.point)<=order.range+actor.Radius+.12f)
                    {
                        if(!order.started){order.started=true;order.startedAt=Time.time;}
                        motor.Stop();
                        if(order.ground(order.point))Complete();
                        return;
                    }
                    if(!motor.Travelling && !motor.MoveTo(order.point))Complete();
                    if(!order.started){order.started=true;order.startedAt=Time.time;}
                    break;
                }
                case Kind.Instant:
                    if(order.instant())Complete();
                    else if(!order.started)
                    {
                        order.started=true;
                        order.startedAt=Time.time;
                    }
                    break;
            }
        }

        private float FlatDistance(Vector3 p)
        {
            var a=transform.position;
            return new Vector2(p.x-a.x,p.z-a.z).magnitude;
        }
        private bool Reached(Vector3 p)
        {
            if(FlatDistance(p)<.45f)return true;
            return agent!=null && !agent.pathPending && !agent.hasPath &&
                FlatDistance(p)<.85f;
        }
        private void Complete()
        {
            current=null;
            Changed?.Invoke();
        }
    }
}
