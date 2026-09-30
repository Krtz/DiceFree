using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;
using static DiceFree.EditorTools.CombatMathValidation;

namespace DiceFree.EditorTools
{
    public static class ResistanceCapValidation
    {
        private static void Equal(float a, float b) => Require(Mathf.Abs(a - b) < .0001f, $"Cap/inversion: {a} != {b}");
        public static void RequireDefaultCap(ActorStats stats)
        {
            var element = ScriptableObject.CreateInstance<ElementDefinition>(); element.stableId = "fixture.reset";
            try { Equal(stats.ResistanceCap(element), stats.Definition.tuning.resistanceCap); }
            finally { Object.Destroy(element); }
        }
        public static void Check(CombatActor player, CombatActor enemy)
        {
            var definition = Object.Instantiate(enemy.Stats.Definition);
            var obj = new GameObject("Resistance cap application fixture");
            var fire = ScriptableObject.CreateInstance<ElementDefinition>(); fire.stableId = "fixture.fire";
            var ice = ScriptableObject.CreateInstance<ElementDefinition>(); ice.stableId = "fixture.ice";
            var attack = Object.Instantiate(player.Stats.Definition.basicAttack);
            try
            {
                definition.baseHp = 200; definition.baseAttributes = new AttributeValues(0) { spirit = 1 };
                definition.physicalDefense = 300;
                definition.resistances = new[] { new ElementResistance { element = fire, fraction = 1.1f }, new ElementResistance { element = ice, fraction = .2f } };
                var target = obj.AddComponent<ActorStats>(); target.Configure(definition);
                var health = obj.AddComponent<Health>(); health.enabled = false; // Deterministic fixture: no regeneration tick.
                target.SetResistanceCapModifier("global", ResistanceCapModifier.Global(.05f));
                Equal(target.ResistanceCap(fire), .8f);
                target.SetResistanceCapModifier("fire", ResistanceCapModifier.ForElement(fire.stableId, .1f));
                Equal(target.ResistanceCap(fire), .9f); Equal(target.ResistanceCap(ice), .8f);
                target.RemoveResistanceCapModifier("global");
                target.SetResistanceCapModifier("fire", ResistanceCapModifier.ForElement(fire.stableId, -.2f));
                Equal(target.ResistanceCap(fire), .55f); Equal(target.Resistance(fire), .55f); Equal(target.RawResistance(fire), 1.1f);
                target.SetResistanceCapModifier("fire", ResistanceCapModifier.ForElement(fire.stableId, -1));
                Equal(target.ResistanceCap(fire), 0); Equal(target.Resistance(fire), 0);
                target.SetResistanceModifier("vulnerability", new ElementalModifier(fire.stableId, -2.6f));
                Equal(target.Resistance(fire), -1.5f); target.RemoveResistanceModifier("vulnerability");
                target.RemoveResistanceCapModifier("fire");
                Equal(new ElementalHealingResult(100, fire, target.ResolveResistance(fire, ElementalContext.Healing)).Healing, 175);
                target.SetResistanceCapModifier("fire", ResistanceCapModifier.ForElement(fire.stableId, .3f));
                Equal(target.ResistanceCap(fire), 1.05f);
                Equal(new ElementalHealingResult(100, fire, target.ResolveResistance(fire, ElementalContext.Healing)).Healing, 205);
                target.SetSecondaryModifier("receiver", new SecondaryScalingModifier { stat = SecondaryStat.HealingReceived, add = 10 });
                player.Stats.SetSecondaryModifier("fixture.healer", new SecondaryScalingModifier { stat = SecondaryStat.HealingDone, add = 10 });
                Require(target.HealingReceived > 10 && player.Stats.HealingDone > 10, "Healing scaling fixture missing.");
                attack.channel = DamageChannel.Physical; attack.element = fire; attack.baseDamage = 200; attack.coefficient = 0;
                var action = ActionResolution.Resolve(new ActionProvenance("fixture.mixed", "fixture.source"));
                DamageResult Calculate() => DamageResolver.CalculatePacket(player.Stats, target, attack, action);
                health.ApplyDamage(null, new DamageResult { mitigated = 40 });
                int damageEvents = 0, healEvents = 0, packetEvents = 0;
                health.Damaged += (_, _) => damageEvents++;
                health.Healed += _ => healEvents++;
                health.PacketResolved += (_, result) => { Require(result.action == action, "Connected packet lost action context."); packetEvents++; };
                var restored = health.ApplyPacket(player, Calculate());
                Equal(restored.afterDefense, 100); Equal(restored.RestorationPotential, 5); Equal(restored.restored, 5); Equal(restored.applied, 0);
                Require(restored.Outcome == PacketOutcome.ResistanceRestoration && restored.DamagePotential == 0 &&
                    damageEvents == 0 && healEvents == 0 && packetEvents == 1, "Restoration leaked ordinary damage/heal semantics.");
                attack.baseDamage = 120;
                var afterDefense = Calculate(); Equal(afterDefense.afterDefense, 60); Equal(afterDefense.RestorationPotential, 3);
                // Separate actual amounts survive a mixed action; never net them into one damage quantity.
                attack.element = ice; attack.baseDamage = 75;
                var damaged = health.ApplyPacket(player, Calculate()); Equal(damaged.applied, 30); Equal(damaged.restored, 0);
                Require(damaged.action == restored.action && restored.restored > 0 && damaged.applied > 0 && damageEvents == 1,
                    "Mixed packet quantities/provenance were collapsed.");
                attack.element = fire; attack.baseDamage = 800;
                health.Restore((health.Maximum - 3) / health.Maximum);
                var clamped = health.ApplyPacket(player, Calculate()); Equal(clamped.RestorationPotential, 20); Equal(clamped.restored, 3);
                // Use a resolved action-bearing lethal packet so packet telemetry also covers death.
                attack.element = null; attack.baseDamage = 10000;
                health.ApplyPacket(player, Calculate()); Require(!health.Alive, "Death fixture failed.");
                int before = packetEvents;
                attack.element = fire; attack.baseDamage = 200;
                var dead = health.ApplyPacket(player, Calculate()); Equal(dead.restored, 0); Equal(health.Current, 0);
                Require(!health.Alive && packetEvents == before && healEvents == 0, "Inversion resurrected or emitted ordinary healing.");
                target.ResetTransientModifiers(); Equal(target.ResistanceCap(fire), .75f);
                player.Stats.SetResistanceCapModifier("fixture.load", ResistanceCapModifier.Global(.5f));
                var anchor = player.GetComponent<RespawnAtAnchor>(); Require(anchor.LoadAtAnchor(anchor.AnchorId), "Load fixture failed.");
                Equal(player.Stats.ResistanceCap(fire), .75f);
                enemy.Stats.SetResistanceCapModifier("fixture.debug", ResistanceCapModifier.Global(.5f));
                Require(enemy.GetComponent<DiceFree.AI.AggroBehaviour>().ResetEncounter(), "Debug reset fixture failed.");
                Equal(enemy.Stats.ResistanceCap(fire), .75f);
            }
            finally
            {
                player.Stats.RemoveSecondaryModifier("fixture.healer"); player.Stats.RemoveResistanceCapModifier("fixture.load");
                enemy.Stats.RemoveResistanceCapModifier("fixture.debug");
                Object.Destroy(obj); Object.Destroy(definition); Object.Destroy(fire); Object.Destroy(ice); Object.Destroy(attack);
            }
            Debug.Log("RESISTANCE_CAP_OK: global/element caps, floor, >100%, defense-first inversion, separate outcomes, scaling bypass, death/clamp and resets.");
        }
    }
}
