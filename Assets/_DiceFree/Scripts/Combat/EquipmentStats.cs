using System;
namespace DiceFree.Combat
{
    // Durable equipment contribution, reconstructed from owned definitions; never a transient effect.
    [Serializable] public struct EquipmentStats
    {
        public AttributeValues attributes;
        public float physicalDefense, magicalDefense;
        public float attackSpeedPercent, movementSpeedPercent;
        public float regeneration, basicAttackMaximumBonus;
        public float basicAttackFlatBonus;
    }
    public static class MovementUnits
    {
        public static float DisplaySpeed(float metersPerSecond) => metersPerSecond * 10;
    }
}
