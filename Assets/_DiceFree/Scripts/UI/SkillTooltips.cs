using System.Collections.Generic;
using System.Text;
using DiceFree.Combat;
using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>
    /// Runtime skill descriptions shared by action-bar buttons and the skill
    /// allocation windows. Values come from the active skill's real rank curves.
    /// </summary>
    public static class SkillTooltips
    {
        private static string Format(float n) => n.ToString("0.##");

        public static string Range(Vector2 amount) => Mathf.Approximately(amount.x,amount.y)
            ? Format(amount.y) : $"{amount.x:0.##}–{amount.y:0.##}";

        public static string Scaling(AttackDefinition attack, float? coefficient = null, float flat = 0)
        {
            if (attack == null) return "";
            var weights = attack.weights;
            float scale = coefficient ?? attack.coefficient;
            float basis = attack.baseDamage + flat;
            if (scale <= 0) return Format(basis);
            if (attack.scaling == AttributeScaling.Highest) return $"{basis:0.##} + {scale:0.###} × highest attribute";
            var terms = new List<string>();
            bool highest = attack.scaling == AttributeScaling.HighestSelected;
            void Attribute(string name,float weight)
            {
                if (weight <= 0) return;
                float factor = highest ? weight : scale * weight;
                terms.Add(highest && Mathf.Approximately(factor,1) ? name : $"{factor:0.###} × {name}");
            }
            Attribute("STR",weights.strength);
            Attribute("AGI",weights.agility);
            Attribute("INT",weights.intelligence);
            Attribute("SPI",weights.spirit);
            Attribute("VIT",weights.vitality);
            if (highest) return $"{basis:0.##} + {scale:0.###} × highest of " +
                (terms.Count == 0 ? "all attributes" : string.Join(", ",terms));
            return terms.Count == 0 ? Format(basis) : Format(basis) + " + " + string.Join(" + ",terms);
        }

        private static void Damage(StringBuilder text, AttackDefinition attack, ActorStats stats, float? coefficient, float flat, int hits = 1)
        {
            if (attack == null) return;
            text.AppendLine("Scaling: " + Scaling(attack,coefficient,flat));
            if (stats == null) return;
            var amount = attack.RawDamageRange(stats.Attributes,coefficient ?? attack.coefficient) + Vector2.one * flat;
            text.AppendLine($"Current: {Range(amount)} raw {attack.channel} damage" + (hits > 1 ? " per hit." : "."));
            if (hits > 1) text.AppendLine($"Total: {Range(amount * hits)} raw if all {hits} hits land.");
        }

        public static string Describe(NoviceSkillDefinition skill, int rank, string key = null, ActorStats stats = null)
        {
            if (skill == null) return "";
            int level = Mathf.Clamp(rank > 0 ? rank : 1, 1, skill.maxRank);
            var s = Start(skill.displayName, rank, skill.maxRank, key);
            switch(skill.kind)
            {
                case NoviceSkillKind.StrengthMeleeStun:
                    s.AppendLine("Strike a nearby enemy and stun it.");
                    Damage(s,skill.attack,stats,null,skill.FlatDamage(level));
                    s.AppendLine("Bonus raw damage: +"+Format(skill.FlatDamage(level)));
                    s.AppendLine("Stun: "+Format(skill.StunDuration(level))+" seconds");
                    break;
                case NoviceSkillKind.MagicSand:
                    s.AppendLine("Launch damaging magic sand and reduce enemy accuracy.");
                    Damage(s,skill.attack,stats,null,skill.FlatDamage(level));
                    s.AppendLine("Bonus raw damage: +"+Format(skill.FlatDamage(level)));
                    s.AppendLine("Enemy miss chance: +"+
                        Format(skill.MagicSandMissChance(level)*100f)+"%");
                    break;
                case NoviceSkillKind.AgilityAttackSpeedBuff:
                    s.AppendLine("Increase an ally's attack speed (can target yourself).");
                    s.AppendLine("Bonus: 10% + "+Format(level*.2f)+"% per AGI");
                    break;
                case NoviceSkillKind.SpiritHeal:
                    s.AppendLine("Heal an ally or yourself.");
                    s.AppendLine("Healing: 10 x Spirit + "+Format(skill.flatHealPerRank*level));
                    if(stats!=null)s.AppendLine($"Current: {skill.HealAmount(stats.Attributes,level):0.##} healing before modifiers.");
                    break;
                case NoviceSkillKind.AllStatPassive:
                    s.AppendLine("Passive: +"+level+" to each core attribute.");
                    s.AppendLine("Strength, Agility, Intelligence, Spirit, Vitality.");
                    break;
            }
            if(skill.Active) Extras(s,skill.range,skill.cooldownSeconds,skill.durationSeconds,
                skill.kind==NoviceSkillKind.MagicSand ||
                skill.kind==NoviceSkillKind.AgilityAttackSpeedBuff);
            Next(s,rank,skill.maxRank,"Spend skill points using the + button on the action bar.");
            return s.ToString().TrimEnd();
        }

        public static string Describe(MagicalSkillDefinition skill, int rank, string key = null, ActorStats stats = null)
        {
            if (skill == null) return "";
            int level=Mathf.Clamp(rank>0?rank:1,1,skill.maxRank);
            var s=Start(skill.displayName,rank,skill.maxRank,key);
            switch(skill.kind)
            {
                case MagicalSkillKind.MagicSand:
                    s.AppendLine("Throw sand magic at one enemy and impair accuracy.");
                    Damage(s,skill.attack,stats,skill.DamageCoefficient(level),skill.FlatDamage(level));
                    s.AppendLine("Damage coefficient: x"+Format(skill.DamageCoefficient(level)));
                    s.AppendLine("Bonus raw damage: +"+Format(skill.FlatDamage(level)));
                    s.AppendLine("Enemy miss chance: +"+
                        Format(skill.MagicSandMissChance(level)*100f)+"%");
                    break;
                case MagicalSkillKind.Mend:
                    s.AppendLine("Heal yourself or a friendly target.");
                    s.AppendLine("Healing: Spirit x "+
                        Format(skill.healCoefficientRank1+
                        (level-1)*skill.healCoefficientPerRank)+
                        " + "+Format(skill.flatHealPerRank*level));
                    if(stats!=null)s.AppendLine($"Current: {skill.HealAmount(stats.Attributes,level):0.##} healing before modifiers.");
                    break;
                case MagicalSkillKind.FireImbuement:
                    s.AppendLine("Imbue an ally's basic attacks with additional fire damage.");
                    Damage(s,skill.secondaryAttack,stats,skill.ImbuementCoefficient(level),skill.FlatDamage(level));
                    s.AppendLine("The damage preview assumes you imbue yourself. Allies use their own attributes.");
                    s.AppendLine("Extra-hit coefficient: x"+
                        Format(skill.ImbuementCoefficient(level)));
                    s.AppendLine("Flat fire damage per basic hit: +"+Format(skill.FlatDamage(level)));
                    break;
                case MagicalSkillKind.IceBurst:
                    s.AppendLine("Choose ground to unleash a delayed area ice burst.");
                    Damage(s,skill.attack,stats,skill.DamageCoefficient(level),skill.FlatDamage(level));
                    s.AppendLine("Radius: "+Format(skill.iceRadius)+" m"+
                        "    Delay: "+Format(skill.iceDelaySeconds)+" s");
                    s.AppendLine("Slow: "+Format(skill.IceSlowPercent(level))+"% for "+
                        Format(skill.slowDurationSeconds)+" s");
                    s.AppendLine("Bonus raw damage: +"+Format(skill.FlatDamage(level)));
                    s.AppendLine("Damage coefficient: x"+Format(skill.DamageCoefficient(level)));
                    break;
                case MagicalSkillKind.ManaAttunement:
                    s.AppendLine("Passive: increases mana and mana regeneration.");
                    s.AppendLine("Max Mana: +"+Format(skill.MaximumManaBonus(level)));
                    s.AppendLine("Personal regen: +"+Format(skill.PersonalRegenBonus(level))+"/s");
                    s.AppendLine("Aura regen: +"+Format(skill.AuraRegenBonus(level))+"/s");
                    s.AppendLine("Aura radius: "+Format(skill.auraRadius)+" m");
                    break;
            }
            if(skill.Active)
            {
                s.AppendLine("Mana cost: "+Format(skill.ManaCost(level)));
                Extras(s,skill.range,skill.cooldownSeconds,skill.durationSeconds,
                    skill.kind==MagicalSkillKind.MagicSand ||
                    skill.kind==MagicalSkillKind.FireImbuement);
            }
            Next(s,rank,skill.maxRank,null);
            return s.ToString().TrimEnd();
        }

        public static string Describe(PhysicalSkillDefinition skill, int rank, string key = null, ActorStats stats = null)
        {
            if (skill == null) return "";
            int level=Mathf.Clamp(rank>0?rank:1,1,skill.maxRank);
            var s=Start(skill.displayName,rank,skill.maxRank,key);
            switch(skill.kind)
            {
                case PhysicalSkillKind.HeavyStrike:
                    s.AppendLine("Hit a nearby enemy with a heavy, stunning attack.");
                    Damage(s,skill.attack,stats,skill.DamageCoefficient(level),skill.FlatDamage(level));
                    s.AppendLine("Damage coefficient: x"+Format(skill.DamageCoefficient(level)));
                    s.AppendLine("Stun: "+Format(skill.HeavyStunSeconds(level))+" s");
                    s.AppendLine("Bonus raw damage: +"+Format(skill.FlatDamage(level)));
                    break;
                case PhysicalSkillKind.Guard:
                    s.AppendLine("Brace yourself to reduce incoming damage.");
                    s.AppendLine("Damage reduction: "+Format(skill.GuardReduction(level)*100f)+"%");
                    break;
                case PhysicalSkillKind.Quickening:
                    s.AppendLine("Increase your attack and movement speed.");
                    s.AppendLine("Attack speed: +"+
                        Format(skill.QuickeningAttackSpeedPercent(level))+"%");
                    s.AppendLine("Movement speed: +"+
                        Format(skill.QuickeningMoveSpeedPercent(level))+"%");
                    break;
                case PhysicalSkillKind.ArrowRain:
                    s.AppendLine("Target the ground to rain arrows over nearby enemies.");
                    Damage(s,skill.attack,stats,skill.DamageCoefficient(level),skill.FlatDamage(level),skill.arrowHitCount);
                    s.AppendLine("Damage coefficient per hit: x"+
                        Format(skill.DamageCoefficient(level)));
                    s.AppendLine("Bonus raw damage per hit: +"+Format(skill.FlatDamage(level)));
                    s.AppendLine("Radius: "+Format(skill.arrowRadius)+" m; waves: "+
                        skill.arrowHitCount);
                    break;
                case PhysicalSkillKind.MartialAptitude:
                    s.AppendLine("Passive: improved basic attacks and physical defence.");
                    s.AppendLine("Basic attack damage: +"+
                        Format(skill.MartialBasicAttackPercent(level))+"%");
                    s.AppendLine("Physical defense: +"+
                        Format(skill.MartialPhysicalDefense(level)));
                    break;
            }
            if(skill.Active)
            {
                s.AppendLine("Mana cost: "+Format(skill.ManaCost(level)));
                Extras(s,skill.range,skill.cooldownSeconds,skill.durationSeconds,false);
            }
            Next(s,rank,skill.maxRank,null);
            return s.ToString().TrimEnd();
        }

        private static StringBuilder Start(string name,int rank,int max,string key)
        {
            var s=new StringBuilder(name).Append("  |  Rank ")
                .Append(rank).Append('/').Append(max).AppendLine();
            if(!string.IsNullOrWhiteSpace(key))s.Append("Hotkey: ").Append(key).AppendLine();
            if(rank==0)s.AppendLine("Not learned yet - previewing Rank 1.");
            return s;
        }
        private static void Extras(StringBuilder s,float range,float cooldown,float duration,bool debuff)
        {
            if(range>0)s.AppendLine("Range: "+Format(range)+" m");
            if(cooldown>0)s.AppendLine("Cooldown: "+Format(cooldown)+" s");
            if(duration>0)s.AppendLine((debuff?"Effect duration: ":"Duration: ")+Format(duration)+" s");
        }
        private static void Next(StringBuilder s,int rank,int max,string fallback)
        {
            if(rank>=max)s.AppendLine("Maximum rank reached.");
            else if(rank>0)s.AppendLine("Next rank: "+(rank+1)+"/"+max);
            else if(!string.IsNullOrEmpty(fallback))s.AppendLine(fallback);
        }
    }
}
