using System.Linq;
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
            public readonly bool passive;
            public readonly bool canUpgrade;

            public SlotView(string name, string key, int rank, int maxRank, float cooldown,
                bool learned, string tooltip, Texture2D icon, string iconKey,
                bool passive=false, bool canUpgrade=false)
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
                this.passive = passive;
                this.canUpgrade = canUpgrade;
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

        public int UnspentPoints
        {
            get
            {
                string id=stats?.Definition?.stableId;
                if(id==NoviceSkillProgression.ClassStableId)return novice?.UnspentPoints??0;
                if(id==PhysicalSkillProgression.ClassStableId)return physical?.UnspentPoints??0;
                if(id==MagicalSkillProgression.ClassStableId)return magical?.UnspentPoints??0;
                return 0;
            }
        }

        public bool CanUpgrade(int slot) => View(slot).canUpgrade;
        public bool Upgrade(int slot)
        {
            if(slot<0 || slot>4 || !CanUpgrade(slot))return false;
            string id=stats?.Definition?.stableId;
            if(id==NoviceSkillProgression.ClassStableId)
            {
                var skill=slot==4?novice.Definitions.FirstOrDefault(x=>x!=null&&!x.Active):
                    novice.ActiveAtSlot(slot);
                return skill!=null && novice.Spend(skill.stableId);
            }
            if(id==PhysicalSkillProgression.ClassStableId)
            {
                var skill=slot==4?physical.Definitions.FirstOrDefault(x=>x!=null&&!x.Active):
                    physical.ActiveAtSlot(slot);
                return skill!=null && physical.Spend(skill.stableId);
            }
            if(id==MagicalSkillProgression.ClassStableId)
            {
                var skill=slot==4?magical.Definitions.FirstOrDefault(x=>x!=null&&!x.Active):
                    magical.ActiveAtSlot(slot);
                return skill!=null && magical.Spend(skill.stableId);
            }
            return false;
        }

        public string UpgradeTooltip(int slot)
        {
            if(!CanUpgrade(slot))return "";
            string id=stats?.Definition?.stableId;
            if(id==NoviceSkillProgression.ClassStableId)
            {
                var def=slot==4?novice.Definitions.FirstOrDefault(x=>x!=null&&!x.Active):
                    novice.ActiveAtSlot(slot);
                return def==null?"":("Spend 1 skill point (next rank)\n"+
                    SkillTooltips.Describe(def,novice.Rank(def.stableId)+1));
            }
            if(id==PhysicalSkillProgression.ClassStableId)
            {
                var def=slot==4?physical.Definitions.FirstOrDefault(x=>x!=null&&!x.Active):
                    physical.ActiveAtSlot(slot);
                return def==null?"":("Spend 1 skill point (next rank)\n"+
                    SkillTooltips.Describe(def,physical.Rank(def.stableId)+1));
            }
            if(id==MagicalSkillProgression.ClassStableId)
            {
                var def=slot==4?magical.Definitions.FirstOrDefault(x=>x!=null&&!x.Active):
                    magical.ActiveAtSlot(slot);
                return def==null?"":("Spend 1 skill point (next rank)\n"+
                    SkillTooltips.Describe(def,magical.Rank(def.stableId)+1));
            }
            return "";
        }

        public bool HomeCasting => returnHome!=null && returnHome.Casting;
        public float HomeCastRemaining => returnHome?.CastRemaining??0f;

        public bool WasPressedThisFrame(int slot) =>
            slot >= 0 && slot < actions.Length && actions[slot].WasPressedThisFrame();

        public SlotView View(int slot)
        {
            if (slot < 0 || slot >= 12 || stats?.Definition == null) return default;
            if(slot>=9)return SharedView(slot);
            if(slot>=5)return default; // 6-9 reserved for future class skills.
            if(slot==4)return PassiveView();

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
                    SkillIconLibrary.Key(definition.kind),false,
                    rank < definition.maxRank && novice.UnspentPoints > 0);
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
                    SkillIconLibrary.Key(definition.kind),false,
                    rank < definition.maxRank && physical.UnspentPoints > 0);
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
                    SkillIconLibrary.Key(definition.kind),false,
                    rank < definition.maxRank && magical.UnspentPoints > 0);
            }

            return default;
        }

        private SlotView PassiveView()
        {
            string id=stats.Definition.stableId;
            if(id==NoviceSkillProgression.ClassStableId && novice!=null)
            {
                var def=novice.Definitions.FirstOrDefault(x=>x!=null&&!x.Active);
                if(def==null)return default;
                int rank=novice.Rank(def.stableId);
                return new SlotView(def.displayName,"",rank,def.maxRank,0,rank>0,
                    SkillTooltips.Describe(def,rank),
                    SkillIconLibrary.Get(def.kind),SkillIconLibrary.Key(def.kind),
                    true,rank<def.maxRank&&novice.UnspentPoints>0);
            }
            if(id==PhysicalSkillProgression.ClassStableId && physical!=null)
            {
                var def=physical.Definitions.FirstOrDefault(x=>x!=null&&!x.Active);
                if(def==null)return default;
                int rank=physical.Rank(def.stableId);
                return new SlotView(def.displayName,"",rank,def.maxRank,0,rank>0,
                    SkillTooltips.Describe(def,rank),
                    SkillIconLibrary.Get(def.kind),SkillIconLibrary.Key(def.kind),
                    true,rank<def.maxRank&&physical.UnspentPoints>0);
            }
            if(id==MagicalSkillProgression.ClassStableId && magical!=null)
            {
                var def=magical.Definitions.FirstOrDefault(x=>x!=null&&!x.Active);
                if(def==null)return default;
                int rank=magical.Rank(def.stableId);
                return new SlotView(def.displayName,"",rank,def.maxRank,0,rank>0,
                    SkillTooltips.Describe(def,rank),
                    SkillIconLibrary.Get(def.kind),SkillIconLibrary.Key(def.kind),
                    true,rank<def.maxRank&&magical.UnspentPoints>0);
            }
            return default;
        }

        private SlotView SharedView(int slot)
        {
            string name=slot==9?"Attack / Attack Move":
                slot==10?"Interact":"Return to Revive Stone";
            string key=slot==9?InputBindings.Display(bindings.Action("Gameplay/Attack selected")):
                slot==10?InputBindings.Display(bindings.Action("Gameplay/Interact")):
                InputBindings.Display(bindings.Action("Gameplay/Return to anchor"));
            string iconId=slot==9?"attack-command":slot==10?"interact":"return-home";
            float cooldown=slot==11?returnHome?.CooldownRemaining??0f:0f;
            string description=slot==9?
                "Attack a selected enemy or choose an enemy/ground destination. Shift queues attacks and attack-move orders. Works on the minimap.":
                slot==10?
                "Interact with the nearest available NPC, chest, loot or world object.":
                "A shared Echo skill. Cast for 10 seconds to teleport to your last activated Dice revive stone. 10 minute cooldown. Interrupted by movement or damage; unavailable in combat or dungeons. No healing.";
            if(slot==11 && returnHome!=null)
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
            if (slot < 0 || slot >= 12 || stats?.Definition == null || targeting == null) return false;
            if(slot>=4 && slot<=8)return false; // Passive and four reserved class-skill slots.
            if(slot!=11 && returnHome!=null && returnHome.Casting)
                returnHome.Cancel();
            if(slot==9)
            {
                combatInput?.BeginAttackTargeting();
                return combatInput!=null && actor!=null && actor.CanAct;
            }
            if(slot==10)
            {
                combatInput?.CancelAttackMove();
                return interactor!=null && interactor.InteractNearest();
            }
            if(slot==11)return returnHome!=null && returnHome.TryBegin();
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
