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
        public AttributeValues Attributes => AttributeValues.AtLevel(definition.baseAttributes, definition.growth, level);
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
            resistanceModifiers.Clear(); penetrationModifiers.Clear(); capModifiers.Clear(); Changed?.Invoke(previous);
        }
        public float MoveSpeed => definition.moveSpeed * (1 + Attributes.agility * SecondaryCoefficient(SecondaryStat.MoveSpeed));
        public float AttackSpeed => 1 + Attributes.agility * SecondaryCoefficient(SecondaryStat.AttackSpeed);
        public float HealingDone => 1 + Attributes.spirit * SecondaryCoefficient(SecondaryStat.HealingDone);
        public float HealingReceived => 1 + Attributes.spirit * SecondaryCoefficient(SecondaryStat.HealingReceived);
        // Current underlying-reference provider: class base + attributes only.
        // Gear/permanent/passive membership is OPEN; decide here later, not in DefenseMath.
        // Temporary Defense modifiers must never be folded into this reference.
        public float Defense(DamageChannel channel) => channel == DamageChannel.Physical
            ? definition.physicalDefense + Attributes.strength * SecondaryCoefficient(SecondaryStat.PhysicalDefense)
            : definition.magicalDefense + Attributes.intelligence * SecondaryCoefficient(SecondaryStat.MagicalDefense);
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
