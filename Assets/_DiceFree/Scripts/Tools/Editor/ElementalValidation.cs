using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;
using static DiceFree.EditorTools.CombatMathValidation;

namespace DiceFree.EditorTools
{
    public static class ElementalValidation
    {
        private sealed class Roll : ICriticalRollSource { public float NextUnit() => 0; }
        private static void Equal(float a, float b) => Require(Mathf.Abs(a - b) < 0.0001f, $"Elemental mismatch: {a} != {b}");
        public static void Check(CombatActor player, CombatActor enemy)
        {
            var definition = Object.Instantiate(enemy.Stats.Definition);
            var obj = new GameObject("Elemental fixture");
            var fire = ScriptableObject.CreateInstance<ElementDefinition>(); fire.stableId = "fixture.fire";
            var packet = Object.Instantiate(player.Stats.Definition.basicAttack);
            var source = player.Stats;
            try
            {
                var target = obj.AddComponent<ActorStats>(); target.Configure(definition);
                void Raw(float value) => definition.resistances = new[] { new ElementResistance { element = fire, fraction = value } };
                ElementalResistanceResolution Resolve(ElementalContext context) => target.ResolveResistance(fire, context, source);
                Raw(1.1f); Equal(target.RawResistance(fire), 1.1f); Equal(Resolve(ElementalContext.Damage).effective, .75f);
                target.SetResistanceModifier("debuff", new ElementalModifier(fire.stableId, -.2f));
                Equal(target.RawResistance(fire), .9f); Equal(Resolve(ElementalContext.Damage).effective, .75f);
                target.ResetTransientModifiers(); Raw(1);
                source.SetPenetrationModifier("fixture.pen", new ElementalModifier(fire.stableId, .2f, ElementalContext.Damage));
                Equal(Resolve(ElementalContext.Damage).preCap, .8f); Equal(Resolve(ElementalContext.Damage).effective, .75f);
                source.SetPenetrationModifier("fixture.pen", new ElementalModifier(fire.stableId, .3f, ElementalContext.Damage));
                Equal(Resolve(ElementalContext.Damage).effective, .7f);
                Raw(0); target.SetResistanceModifier("a", new ElementalModifier(fire.stableId, .3f));
                target.SetResistanceModifier("b", new ElementalModifier(fire.stableId, .2f));
                target.SetResistanceModifier("c", new ElementalModifier(fire.stableId, -.15f));
                Equal(target.RawResistance(fire), .35f); target.ResetTransientModifiers();
                Raw(.5f); source.SetPenetrationModifier("fixture.pen", new ElementalModifier(fire.stableId, .1f, ElementalContext.Damage));
                Equal(Resolve(ElementalContext.Damage).effective, .4f); Equal(Resolve(ElementalContext.Healing).effective, .5f);
                source.SetPenetrationModifier("fixture.pen", new ElementalModifier(fire.stableId, .1f, ElementalContext.Healing));
                Equal(Resolve(ElementalContext.Damage).effective, .5f); Equal(Resolve(ElementalContext.Healing).effective, .6f);
                source.SetPenetrationModifier("fixture.pen", new ElementalModifier(fire.stableId, .1f));
                Equal(Resolve(ElementalContext.Damage).effective, .4f); Equal(Resolve(ElementalContext.Healing).effective, .6f);
                source.RemovePenetrationModifier("fixture.pen");
                target.SetResistanceModifier("debuff", new ElementalModifier(fire.stableId, -.3f));
                Equal(Resolve(ElementalContext.Damage).effective, .2f); Equal(Resolve(ElementalContext.Healing).effective, .2f);
                target.ResetTransientModifiers();
                foreach (float value in new[] { .5f, -.5f, -1f, -1.5f, -10f })
                {
                    Raw(value); Equal(Resolve(ElementalContext.Damage).effective, value);
                    var heal = new ElementalHealingResult(100, fire, Resolve(ElementalContext.Healing));
                    Equal(heal.signedAmount, 100 * (1 + value));
                    Equal(heal.InversionDamage, Mathf.Max(0, -100 * (1 + value)));
                }
                Raw(-.8f); source.SetPenetrationModifier("fixture.pen", new ElementalModifier(fire.stableId, .2f, ElementalContext.Damage));
                Equal(Resolve(ElementalContext.Damage).effective, -1);
                Equal(target.ResolveResistance(null, ElementalContext.Damage, source).effective, 0);
                Equal(new ElementalHealingResult(100, null, default).Healing, 100);
                packet.element = fire; packet.baseDamage = 100; packet.coefficient = 0;
                var origin = new ActionProvenance("action.fireball", "caster.fixture");
                var first = CriticalResolver.EvaluateAction(new[] { new CriticalRule("hat", "double", true, 1, 2) }, new Roll(), origin);
                var second = CriticalResolver.EvaluateAction(new[] { new CriticalRule("other", "triple", true, 1, 3) }, new Roll(), origin);
                Require(first.Origin == second.Origin && first.Origin.SourceId != first.Winner.sourceId && first.Origin.ActionId == "action.fireball", "Action and grant provenance conflated.");
                var hit = DamageResolver.Calculate(source, target, packet, first);
                var another = DamageResolver.Calculate(source, target, packet, first);
                Equal(hit.elementalResistance.raw, -.8f); Equal(hit.elementalResistance.preCap, -1);
                Require(hit.critical.Origin == another.critical.Origin, "Packet provenance lost.");
                source.SetResistanceModifier("fixture.res", new ElementalModifier(fire.stableId, .4f));
                Require(player.GetComponent<RespawnAtAnchor>().LoadAtAnchor(player.GetComponent<RespawnAtAnchor>().AnchorId), "Load reset failed.");
                Equal(source.Penetration(fire, ElementalContext.Damage), 0);
                Equal(source.RawResistance(fire), source.Definition.tuning.defaultElementResistance);
                target.SetResistanceModifier("reset", new ElementalModifier(fire.stableId, .8f));
                target.SetPenetrationModifier("reset", new ElementalModifier(fire.stableId, .8f)); target.ResetTransientModifiers();
                Equal(target.RawResistance(fire), -.8f); Equal(target.Penetration(fire, ElementalContext.Damage), 0);
            }
            finally { source.RemovePenetrationModifier("fixture.pen"); source.RemoveResistanceModifier("fixture.res"); Object.Destroy(obj); Object.Destroy(definition); Object.Destroy(fire); Object.Destroy(packet); }
            Debug.Log("ELEMENTAL_OK: raw/overcap, additive modifiers, contextual penetration, inversion, reset and action provenance.");
        }
    }
}
