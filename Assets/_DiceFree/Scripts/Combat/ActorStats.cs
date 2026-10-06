using System;
using System.Collections.Generic;
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
        public event Action<float> Changed;
        private readonly SortedDictionary<string, VitalityModifier> vitalityModifiers = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, SecondaryScalingModifier> secondaryModifiers = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, DefenseModifier> defenseModifiers = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, ElementalModifier> resistanceModifiers = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, ElementalModifier> penetrationModifiers = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, ResistanceCapModifier> capModifiers = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, AttributeValues> attributeContributions = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, float> attackSpeedPercentModifiers = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, float> movementSpeedPercentModifiers = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, DamageTakenModifier> damageTakenModifiers = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, float> basicAttackDamagePercentContributions = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, float> physicalDefenseContributions = new(StringComparer.Ordinal);
        public void SetResistanceCapModifier(string sourceId, ResistanceCapModifier value)
        {
            if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("Stable modifier source required.");
            capModifiers[sourceId] = value;
        }
        public void RemoveResistanceCapModifier(string sourceId) => capModifiers.Remove(sourceId);
        public float ResistanceCap(ElementDefinition element)
        {
            if (element == null) return 0;
            float cap = definition.tuning.resistanceCap;
            foreach (var value in capModifiers.Values)
                if (value.elementId == null || value.elementId == element.stableId) cap += value.delta;
            return Mathf.Max(0, cap); // Cap reduction alone cannot create vulnerability; no upper ceiling.
        }
        public void SetResistanceModifier(string sourceId, ElementalModifier value)
        {
            if (string.IsNullOrWhiteSpace(sourceId) || string.IsNullOrWhiteSpace(value.elementId)) throw new ArgumentException("Stable modifier identities required.");
            if (value.contexts != (ElementalContext.Damage | ElementalContext.Healing)) throw new ArgumentException("Real resistance modifiers affect both contexts.");
            resistanceModifiers[sourceId] = value;
        }
        public void SetPenetrationModifier(string sourceId, ElementalModifier value)
        {
            if (string.IsNullOrWhiteSpace(sourceId) || string.IsNullOrWhiteSpace(value.elementId)) throw new ArgumentException("Stable modifier identities required.");
            penetrationModifiers[sourceId] = value;
        }
        public void RemoveResistanceModifier(string sourceId) => resistanceModifiers.Remove(sourceId);
        public void RemovePenetrationModifier(string sourceId) => penetrationModifiers.Remove(sourceId);
        public float Penetration(ElementDefinition element, ElementalContext context)
        {
            float result = 0;
            foreach (var value in penetrationModifiers.Values)
                if (element != null && value.elementId == element.stableId && (value.contexts & context) != 0) result += value.delta;
            return result;
        }
        public AttributeValues Attributes
        {
            get
            {
                var values = AttributeValues.AtLevel(definition.baseAttributes, definition.growth, level);
                values += EquipmentTotal.attributes;
                foreach (var contribution in attributeContributions.Values) values += contribution;
                return values;
            }
        }
        public float MaximumHp => definition.baseHp + Attributes.vitality * VitalityCoefficient(false);
        public float Regeneration => Attributes.vitality * VitalityCoefficient(true);
        private float VitalityCoefficient(bool regeneration)
        {
            float coefficient = regeneration ? definition.VitalityRegenerationCoefficient : definition.VitalityHpCoefficient;
            float percent = 0;
            foreach (var modifier in vitalityModifiers.Values)
            {
                coefficient += regeneration ? modifier.regenerationPerVitalityAdd : modifier.hpPerVitalityAdd;
                percent += regeneration ? modifier.regenerationCoefficientPercent : modifier.hpCoefficientPercent;
            }
            return Mathf.Max(0, coefficient * Mathf.Max(0, 1 + percent));
        }
        public void SetVitalityModifier(string sourceId, VitalityModifier value)
        {
            if (string.IsNullOrEmpty(sourceId)) throw new ArgumentException("Modifier requires source identity.");
            float previous = MaximumHp;
            vitalityModifiers[sourceId] = value;
            Changed?.Invoke(previous);
        }
        public void RemoveVitalityModifier(string sourceId)
        {
            float previous = MaximumHp;
            if (vitalityModifiers.Remove(sourceId)) Changed?.Invoke(previous);
        }
        public void SetAttributeContribution(string sourceId, AttributeValues value)
        {
            if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("Attribute contribution requires source identity.");
            float previous = MaximumHp;
            attributeContributions[sourceId] = value;
            Changed?.Invoke(previous);
        }
        public void RemoveAttributeContribution(string sourceId)
        {
            float previous = MaximumHp;
            if (attributeContributions.Remove(sourceId)) Changed?.Invoke(previous);
        }
        public void SetAttackSpeedPercentModifier(string sourceId, float percent)
        {
            if (string.IsNullOrWhiteSpace(sourceId) || float.IsNaN(percent) || float.IsInfinity(percent) || percent < 0)
                throw new ArgumentException("Attack-speed modifier requires a stable source and finite non-negative percent.");
            attackSpeedPercentModifiers[sourceId] = percent;
            Changed?.Invoke(MaximumHp);
        }
        public void RemoveAttackSpeedPercentModifier(string sourceId)
        {
            if (attackSpeedPercentModifiers.Remove(sourceId)) Changed?.Invoke(MaximumHp);
        }
        public void SetMovementSpeedPercentModifier(string sourceId, float percent)
        {
            if (string.IsNullOrWhiteSpace(sourceId) || float.IsNaN(percent) || float.IsInfinity(percent) || percent < 0)
                throw new ArgumentException("Movement-speed modifier requires a stable source and finite non-negative percent.");
            movementSpeedPercentModifiers[sourceId] = percent;
            Changed?.Invoke(MaximumHp);
        }
        public void RemoveMovementSpeedPercentModifier(string sourceId)
        {
            if (movementSpeedPercentModifiers.Remove(sourceId)) Changed?.Invoke(MaximumHp);
        }
        public void SetDamageTakenModifier(string sourceId, DamageTakenModifier value)
        {
            if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("Damage-taken modifier requires stable source identity.");
            damageTakenModifiers[sourceId] = value;
        }
        public void RemoveDamageTakenModifier(string sourceId) => damageTakenModifiers.Remove(sourceId);
        public float IncomingDamageMultiplier(DamageChannel channel)
        {
            float multiplier = 1f;
            foreach (var modifier in damageTakenModifiers.Values)
                if (modifier.channel == channel) multiplier *= 1f - modifier.reductionFraction;
            return Mathf.Clamp(multiplier, 0.01f, 4f);
        }
        public void SetBasicAttackDamagePercentContribution(string sourceId, float percent)
        {
            if (string.IsNullOrWhiteSpace(sourceId) || float.IsNaN(percent) || float.IsInfinity(percent) || percent < 0)
                throw new ArgumentException("Basic-attack contribution requires stable finite non-negative data.");
            basicAttackDamagePercentContributions[sourceId] = percent;
        }
        public void RemoveBasicAttackDamagePercentContribution(string sourceId) => basicAttackDamagePercentContributions.Remove(sourceId);
        public float BasicAttackDamageMultiplier
        {
            get
            {
                float percent = 0;
                foreach (float value in basicAttackDamagePercentContributions.Values) percent += value;
                return Mathf.Max(0, 1 + percent / 100f);
            }
        }
        public void SetPhysicalDefenseContribution(string sourceId, float amount)
        {
            if (string.IsNullOrWhiteSpace(sourceId) || float.IsNaN(amount) || float.IsInfinity(amount) || amount < 0)
                throw new ArgumentException("Physical-defense contribution requires stable finite non-negative data.");
            physicalDefenseContributions[sourceId] = amount;
        }
        public void RemovePhysicalDefenseContribution(string sourceId) => physicalDefenseContributions.Remove(sourceId);
        private float PhysicalDefenseContribution
        {
            get { float total = 0; foreach (float value in physicalDefenseContributions.Values) total += value; return total; }
        }
        public float SecondaryCoefficient(SecondaryStat stat)
        {
            float coefficient = definition.SecondaryCoefficient(stat), percent = 0;
            foreach (var modifier in secondaryModifiers.Values)
                if (modifier.stat == stat) { coefficient += modifier.add; percent += modifier.percent; }
            return Mathf.Max(0, coefficient * Mathf.Max(0, 1 + percent));
        }
        public void SetSecondaryModifier(string sourceId, SecondaryScalingModifier value)
        {
            if (string.IsNullOrEmpty(sourceId)) throw new ArgumentException("Modifier requires source identity.");
            secondaryModifiers[sourceId] = value; Changed?.Invoke(MaximumHp);
        }
        public void RemoveSecondaryModifier(string sourceId)
        {
            if (secondaryModifiers.Remove(sourceId)) Changed?.Invoke(MaximumHp);
        }
        public void SetDefenseModifier(string sourceId, DefenseModifier value)
        {
            if (string.IsNullOrEmpty(sourceId)) throw new ArgumentException("Modifier requires source identity.");
            defenseModifiers[sourceId] = value;
        }
        public void RemoveDefenseModifier(string sourceId) => defenseModifiers.Remove(sourceId);
        public DefenseModifier DefenseModifiers(DamageChannel channel)
        {
            var result = new DefenseModifier { channel = channel };
            foreach (var modifier in defenseModifiers.Values) if (modifier.channel == channel) result.Add(modifier);
            return result;
        }
        public void ResetTransientModifiers()
        {
            float previous = MaximumHp;
            vitalityModifiers.Clear(); secondaryModifiers.Clear(); defenseModifiers.Clear();
            resistanceModifiers.Clear(); penetrationModifiers.Clear(); capModifiers.Clear();
            attackSpeedPercentModifiers.Clear(); movementSpeedPercentModifiers.Clear(); damageTakenModifiers.Clear(); Changed?.Invoke(previous);
        }
        private readonly SortedDictionary<string, EquipmentStats> equipment = new(StringComparer.Ordinal);
        public void SetEquipmentContribution(string sourceId, EquipmentStats value)
        {
            if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("Equipment source required.");
            float previous = MaximumHp;
            equipment[sourceId] = value; Changed?.Invoke(previous);
        }
        public void RemoveEquipmentContribution(string sourceId)
        {
            float previous = MaximumHp;
            if (equipment.Remove(sourceId)) Changed?.Invoke(previous);
        }
        private EquipmentStats EquipmentTotal
        {
            get
            {
                var total = new EquipmentStats();
                foreach (var value in equipment.Values)
                {
                    total.attributes += value.attributes;
                    total.physicalDefense += value.physicalDefense; total.magicalDefense += value.magicalDefense;
                    total.attackSpeedPercent += value.attackSpeedPercent; total.movementSpeedPercent += value.movementSpeedPercent;
                }
                return total;
            }
        }
        private float TemporaryMovementSpeedPercent
        {
            get { float total = 0; foreach (float percent in movementSpeedPercentModifiers.Values) total += percent; return total; }
        }
        public float MoveSpeed => definition.moveSpeed * (1 + Attributes.agility * SecondaryCoefficient(SecondaryStat.MoveSpeed))
                                  * (1 + EquipmentTotal.movementSpeedPercent / 100f)
                                  * (1 + TemporaryMovementSpeedPercent / 100f);
        private float TemporaryAttackSpeedPercent
        {
            get
            {
                float total = 0;
                foreach (float percent in attackSpeedPercentModifiers.Values) total += percent;
                return total;
            }
        }
        public float AttackSpeed => (1 + Attributes.agility * SecondaryCoefficient(SecondaryStat.AttackSpeed))
                                    * (1 + EquipmentTotal.attackSpeedPercent / 100f)
                                    * (1 + TemporaryAttackSpeedPercent / 100f);
        public float HealingDone => 1 + Attributes.spirit * SecondaryCoefficient(SecondaryStat.HealingDone);
        public float HealingReceived => 1 + Attributes.spirit * SecondaryCoefficient(SecondaryStat.HealingReceived);
        // Underlying reference includes class, attributes and equipped gear.
        // Other permanent/passive membership remains open; keep that policy here.
        // Temporary Defense modifiers must never be folded into this reference.
        public float Defense(DamageChannel channel) => channel == DamageChannel.Physical
            ? EquipmentTotal.physicalDefense + definition.physicalDefense + PhysicalDefenseContribution + Attributes.strength * SecondaryCoefficient(SecondaryStat.PhysicalDefense)
            : EquipmentTotal.magicalDefense + definition.magicalDefense + Attributes.intelligence * SecondaryCoefficient(SecondaryStat.MagicalDefense);
        public float RawResistance(ElementDefinition element)
        {
            if (element == null) return 0;
            float raw = definition.tuning.defaultElementResistance;
            foreach (var entry in definition.resistances)
                if (entry.element != null && entry.element.stableId == element.stableId)
                { raw = entry.fraction; break; }
            foreach (var value in resistanceModifiers.Values)
                if (value.elementId == element.stableId) raw += value.delta;
            return raw;
        }
        public ElementalResistanceResolution ResolveResistance(ElementDefinition element, ElementalContext context, ActorStats source = null) =>
            new ElementalResistanceResolution(RawResistance(element), element == null || source == null ? 0 : source.Penetration(element, context),
                ResistanceCap(element), context);
        public float Resistance(ElementDefinition element) => ResolveResistance(element, ElementalContext.Damage).effective;
        public void Configure(ActorDefinition value, int actorLevel = 1) { definition = value; level = Mathf.Max(1, actorLevel); }
        public void SetLevel(int value)
        {
            var oldMaximum = MaximumHp;
            level = Mathf.Max(1, value);
            Changed?.Invoke(oldMaximum);
        }
    }
}
