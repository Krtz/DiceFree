using System;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>UI-facing service for the player command queue.</summary>
    public interface IQueuedOrders
    {
        int PendingOrders { get; }
        bool HasOrders { get; }
        void ClearOrders();
        bool SubmitMove(Vector3 point, bool append);
        bool SubmitAttackMove(Vector3 point, Func<Vector3,bool> attackMove, bool append);
        bool SubmitAttackTarget(CombatActor target,
            Func<CombatActor,bool> attack, bool append);
        bool SubmitUnit(CombatActor target, float range, Func<CombatActor,bool> action, bool append);
        bool SubmitGround(Vector3 point, float range, Func<Vector3,bool> action, bool append);
        bool SubmitAction(Func<bool> action, bool append);
    }
}
