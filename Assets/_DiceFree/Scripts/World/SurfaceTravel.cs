using DiceFree.Characters;
using UnityEngine;

namespace DiceFree.World
{
    // Explicit opt-in actor component: Cornberg authors it only on the player.
    [DisallowMultipleComponent, RequireComponent(typeof(TraversalMotor))]
    public sealed class SurfaceTravel : MonoBehaviour
    {
        public const string SpeedSource = "travel.surface";
        private TraversalMotor motor;
        public TravelSurface Current { get; private set; }
        private void Awake() => motor = GetComponent<TraversalMotor>();
        private void OnEnable() => motor.RefreshSpeedSources += Refresh;
        private void OnDisable()
        {
            motor.RefreshSpeedSources -= Refresh; Current = null; motor.RemoveSpeedFactor(SpeedSource);
        }
        private void Refresh()
        {
            Current = null;
            if (motor.MotionAllowed)
                foreach (var surface in TravelSurface.Active)
                    if (surface.Contains(transform.position) && (Current == null ||
                        surface.Definition.priority > Current.Definition.priority ||
                        (surface.Definition.priority == Current.Definition.priority &&
                         string.CompareOrdinal(surface.StableId, Current.StableId) < 0))) Current = surface;
            if (Current == null) motor.RemoveSpeedFactor(SpeedSource);
            else motor.SetSpeedFactor(SpeedSource, 1 + Mathf.Max(0, Current.Definition.speedBonusPercent) / 100f);
        }
    }
}
