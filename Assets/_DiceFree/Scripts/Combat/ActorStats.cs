using UnityEngine;

namespace DiceFree.Combat
{
    [DefaultExecutionOrder(-120)]
    public sealed class ActorStats : MonoBehaviour
    {
        [SerializeField] private ActorDefinition definition;
        [SerializeField, Min(1)] private int level = 1;
        public ActorDefinition Definition => definition;
        public int Level => level;
        public AttributeValues Attributes => AttributeValues.AtLevel(definition.baseAttributes, definition.growth, level);
        public float MaximumHp => definition.baseHp + Attributes.vitality * definition.tuning.hpPerVitality;
        public float Regeneration => Attributes.vitality * definition.tuning.regenerationPerVitality;
        public float MoveSpeed => definition.moveSpeed * (1 + Attributes.agility * definition.tuning.moveSpeedPerAgility);
        public float AttackSpeed => 1 + Attributes.agility * definition.tuning.attackSpeedPerAgility;
        public float HealingReceived => 1 + Attributes.spirit * definition.tuning.healingReceivedPerSpirit;
        public float Defense(DamageChannel channel) => channel == DamageChannel.Physical
            ? definition.physicalDefense + Attributes.strength * definition.tuning.defensePerAttribute
            : definition.magicalDefense + Attributes.intelligence * definition.tuning.defensePerAttribute;
        public float Resistance(ElementDefinition element)
        {
            if (element == null) return 0;
            foreach (var entry in definition.resistances)
                if (entry.element != null && entry.element.stableId == element.stableId)
                    return Mathf.Min(entry.fraction, definition.tuning.resistanceCap);
            return definition.tuning.defaultElementResistance;
        }
        public void Configure(ActorDefinition value, int actorLevel = 1) { definition = value; level = Mathf.Max(1, actorLevel); }
    }
}
