using DiceFree.Combat;
using DiceFree.World;
using DiceFree.Input;
using DiceFree.Skills;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    [DisallowMultipleComponent]
    public sealed class AbilityBarSource : MonoBehaviour
    {
        public readonly struct SlotView
        {
            public readonly string name;
            public readonly string key;
            public readonly int rank;
            public readonly int maxRank;
            public readonly float cooldown;
            public readonly bool learned;
            public readonly string tooltip;
            public readonly Texture2D icon;
            public readonly string iconKey;

            public SlotView(string name, string key, int rank, int maxRank, float cooldown,
                bool learned, string tooltip, Texture2D icon, string iconKey)
            {
                this.name = name;
                this.key = key;
                this.rank = rank;
                this.maxRank = maxRank;
                this.cooldown = cooldown;
                this.learned = learned;
                this.tooltip = tooltip;
                this.icon = icon;
                this.iconKey = iconKey;
            }

            public bool Exists => !string.IsNullOrEmpty(name);
        }

        private ActorStats stats;
        private IQueuedOrders orders;
        private SkillTargetingController targeting;
        private NoviceSkillProgression novice;
        private PhysicalSkillProgression physical;
        private MagicalSkillProgression magical;
        private NoviceSkillCaster noviceCaster;
        private PhysicalSkillCaster physicalCaster;
        private MagicalSkillCaster magicalCaster;
        private InputBindings bindings;
        private InputAction[] actions;
        private ISharedCombatCommands combatInput;
        private Interactor interactor;
        private ReturnHomeAbility returnHome;
        private CombatActor actor;

        private void Awake()
        {
            stats = GetComponent<ActorStats>();
            actor = GetComponent<CombatActor>();
            foreach(var component in GetComponents<MonoBehaviour>())
                if(component is ISharedCombatCommands commands)
                {combatInput=commands;break;}
            interactor=GetComponent<Interactor>();
            returnHome=GetComponent<ReturnHomeAbility>();
            if(returnHome==null)returnHome=gameObject.AddComponent<ReturnHomeAbility>();
            foreach(var component in GetComponents<MonoBehaviour>())
                if(component is IQueuedOrders found){orders=found;break;}
            targeting = GetComponent<SkillTargetingController>();
            novice = GetComponent<NoviceSkillProgression>();
            physical = GetComponent<PhysicalSkillProgression>();
            magical = GetComponent<MagicalSkillProgression>();
            noviceCaster = GetComponent<NoviceSkillCaster>();
            physicalCaster = GetComponent<PhysicalSkillCaster>();
            magicalCaster = GetComponent<MagicalSkillCaster>();

            bindings = InputBindings.Current;
            actions = new[]
            {
                bindings.Action("Gameplay/Novice skill 1"),
                bindings.Action("Gameplay/Novice skill 2"),
                bindings.Action("Gameplay/Novice skill 3"),
                bindings.Action("Gameplay/Novice skill 4")
            };
        }

        public bool HomeCasting => returnHome!=null && returnHome.Casting;
        public float HomeCastRemaining => returnHome?.CastRemaining??0f;

        public bool WasPressedThisFrame(int slot) =>
            slot >= 0 && slot < actions.Length && actions[slot].WasPressedThisFrame();

        public SlotView View(int slot)
        {
            if (slot < 0 || slot >= 7 || stats?.Definition == null) return default;
            if(slot>=4)return SharedView(slot);
            string classId = stats.Definition.stableId;
            string key = InputBindings.Display(actions[slot]);

            if (classId == NoviceSkillProgression.ClassStableId && novice != null)
            {
                var definition = novice.ActiveAtSlot(slot);
                if (definition == null) return default;
                int rank = novice.Rank(definition.stableId);
                return new SlotView(
                    definition.displayName,
                    key,
                    rank,
                    definition.maxRank,
                    noviceCaster?.CooldownRemaining(definition) ?? 0,
                    rank > 0,
                    SkillTooltips.Describe(definition, rank, key),
                    SkillIconLibrary.Get(definition.kind),
                    SkillIconLibrary.Key(definition.kind));
            }

            if (classId == PhysicalSkillProgression.ClassStableId && physical != null)
            {
                var definition = physical.ActiveAtSlot(slot);
                if (definition == null) return default;
                int rank = physical.Rank(definition.stableId);
                return new SlotView(
                    definition.displayName,
                    key,
                    rank,
                    definition.maxRank,
                    physicalCaster?.CooldownRemaining(definition) ?? 0,
                    rank > 0,
                    SkillTooltips.Describe(definition, rank, key),
                    SkillIconLibrary.Get(definition.kind),
                    SkillIconLibrary.Key(definition.kind));
            }

            if (classId == MagicalSkillProgression.ClassStableId && magical != null)
            {
                var definition = magical.ActiveAtSlot(slot);
                if (definition == null) return default;
                int rank = magical.Rank(definition.stableId);
                return new SlotView(
                    definition.displayName,
                    key,
                    rank,
                    definition.maxRank,
                    magicalCaster?.CooldownRemaining(definition) ?? 0,
                    rank > 0,
                    SkillTooltips.Describe(definition, rank, key),
                    SkillIconLibrary.Get(definition.kind),
                    SkillIconLibrary.Key(definition.kind));
            }

            return default;
        }

        private SlotView SharedView(int slot)
        {
            string name=slot==4?"Attack / Attack Move":
                slot==5?"Interact":"Return to Revive Stone";
            string key=slot==4?InputBindings.Display(bindings.Action("Gameplay/Attack selected")):
                slot==5?InputBindings.Display(bindings.Action("Gameplay/Interact")):
                InputBindings.Display(bindings.Action("Gameplay/Return to anchor"));
            string iconId=slot==4?"attack-command":slot==5?"interact":"return-home";
            float cooldown=slot==6?returnHome?.CooldownRemaining??0f:0f;
            string description=slot==4?
                "Attack a selected enemy or choose an enemy/ground destination. Shift queues attacks and attack-move orders. Works on the minimap.":
                slot==5?
                "Interact with the nearest available NPC, chest, loot or world object.":
                "A shared Echo skill. Cast for 10 seconds to teleport to your last activated Dice revive stone. 10 minute cooldown. Interrupted by movement or damage; unavailable in combat or dungeons. No healing.";
            if(slot==6 && returnHome!=null)
            {
                description+=" Destination: "+GetComponent<RespawnAtAnchor>()?.AnchorName+".";
                if(returnHome.Casting)description+=" Casting: "+returnHome.CastRemaining.ToString("0.0")+"s.";
            }
            return new SlotView(name,key,1,1,cooldown,true,
                name+" ["+key+"]\n"+description,
                SkillIconLibrary.Get(iconId),iconId);
        }

        private static bool ShiftHeld => Keyboard.current!=null &&
            Keyboard.current.shiftKey.isPressed;
        private IQueuedOrders Orders
        {
            get
            {
                if(orders!=null)return orders;
                foreach(var component in GetComponents<MonoBehaviour>())
                    if(component is IQueuedOrders found){orders=found;break;}
                return orders;
            }
        }

        public bool Activate(int slot)
        {
            if (slot < 0 || slot >= 7 || stats?.Definition == null || targeting == null) return false;
            if(slot!=6 && returnHome!=null && returnHome.Casting)
                returnHome.Cancel();
            if(slot==4)
            {
                combatInput?.BeginAttackTargeting();
                return combatInput!=null && actor!=null && actor.CanAct;
            }
            if(slot==5)
            {
                combatInput?.CancelAttackMove();
                return interactor!=null && interactor.InteractNearest();
            }
            if(slot==6)return returnHome!=null && returnHome.TryBegin();
            string classId = stats.Definition.stableId;

            if (classId == NoviceSkillProgression.ClassStableId && novice != null && noviceCaster != null)
            {
                var definition = novice.ActiveAtSlot(slot);
                if (definition == null || !noviceCaster.CanPrepare(definition)) return false;

                bool hostile = definition.kind == NoviceSkillKind.StrengthMeleeStun ||
                               definition.kind == NoviceSkillKind.MagicSand;
                if (hostile)
                    targeting.BeginHostile(
                        definition.displayName,
                        target => noviceCaster.Cast(definition, target),
                        definition.range);
                else
                    targeting.BeginFriendly(
                        definition.displayName,
                        target => noviceCaster.Cast(definition, target),
                        definition.range);
                return true;
            }

            if (classId == PhysicalSkillProgression.ClassStableId && physical != null && physicalCaster != null)
            {
                var definition = physical.ActiveAtSlot(slot);
                if (definition == null || !physicalCaster.CanPrepare(definition)) return false;

                switch (definition.kind)
                {
                    case PhysicalSkillKind.HeavyStrike:
                        targeting.BeginHostile(
                            definition.displayName,
                            target => physicalCaster.Cast(definition, target),
                            definition.range);
                        return true;
                    case PhysicalSkillKind.ArrowRain:
                        targeting.BeginGround(
                            definition.displayName,
                            point => physicalCaster.CastArrowRain(definition, point),
                            definition.range);
                        return true;
                    case PhysicalSkillKind.Guard:
                    case PhysicalSkillKind.Quickening:
                        if(ShiftHeld && Orders!=null)
                            return Orders.SubmitAction(()=>physicalCaster.Cast(definition),true);
                        Orders?.ClearOrders();
                        targeting.Cancel();
                        return physicalCaster.Cast(definition);
                    default:
                        return false;
                }
            }

            if (classId == MagicalSkillProgression.ClassStableId && magical != null && magicalCaster != null)
            {
                var definition = magical.ActiveAtSlot(slot);
                if (definition == null || !magicalCaster.CanPrepare(definition)) return false;

                switch (definition.kind)
                {
                    case MagicalSkillKind.MagicSand:
                        targeting.BeginHostile(
                            definition.displayName,
                            target => magicalCaster.Cast(definition, target),
                            definition.range);
                        return true;
                    case MagicalSkillKind.Mend:
                    case MagicalSkillKind.FireImbuement:
                        targeting.BeginFriendly(
                            definition.displayName,
                            target => magicalCaster.Cast(definition, target),
                            definition.range);
                        return true;
                    case MagicalSkillKind.IceBurst:
                        targeting.BeginGround(
                            definition.displayName,
                            point => magicalCaster.CastIceBurst(definition, point),
                            definition.range);
                        return true;
                    default:
                        return false;
                }
            }

            return false;
        }
    }
}
