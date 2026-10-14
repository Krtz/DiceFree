using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.World;
using DiceFree.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    [DisallowMultipleComponent]
    public sealed class AbilityBarSource : MonoBehaviour
    {
        public const int PageSize=8;
        public const int UltimateSlot=8;
        public const int SlotCount=12;
        public readonly struct SlotView
        {
            public readonly string name,key,tooltip,iconKey;
            public readonly int rank,maxRank;
            public readonly float cooldown;
            public readonly bool learned,passive,canUpgrade;
            public readonly Texture2D icon;
            public SlotView(string name,string key,int rank,int maxRank,float cooldown,bool learned,string tooltip,Texture2D icon,string iconKey,bool passive=false,bool canUpgrade=false)
            {this.name=name;this.key=key;this.rank=rank;this.maxRank=maxRank;this.cooldown=cooldown;this.learned=learned;this.tooltip=tooltip;this.icon=icon;this.iconKey=iconKey;this.passive=passive;this.canUpgrade=canUpgrade;}
            public bool Exists=>!string.IsNullOrEmpty(name);
        }
        private ActorStats stats;
        private CombatActor actor;
        private SkillTargetingController targeting;
        private InputBindings bindings;
        private InputAction[] actions;
        private ISharedCombatCommands combatInput;
        private Interactor interactor;
        private ReturnHomeAbility returnHome;
        private IQueuedOrders orders;
        private readonly List<IClassAbilityProvider> providers=new();
        private string shownClass;
        private int page;
        private void Awake()
        {
            stats=GetComponent<ActorStats>();actor=GetComponent<CombatActor>();targeting=GetComponent<SkillTargetingController>();
            interactor=GetComponent<Interactor>();returnHome=GetComponent<ReturnHomeAbility>()??gameObject.AddComponent<ReturnHomeAbility>();
            foreach(var component in GetComponents<MonoBehaviour>())
            {if(component is ISharedCombatCommands c)combatInput=c;if(component is IQueuedOrders q)orders=q;}
            // Explicitly registered/component providers can replace a built-in class adapter.
            foreach(var provider in GetComponents<MonoBehaviour>().OfType<IClassAbilityProvider>())Register(provider);
            foreach(var provider in ClassAbilityProviders.ExistingKits(gameObject))
                if(!providers.Any(p=>p.ClassId==provider.ClassId))Register(provider);
            bindings=InputBindings.Current;
            actions=Enumerable.Range(0,9).Select(i=>bindings.Asset.FindAction(i<4?"Gameplay/Novice skill "+(i+1):
                i<8?"Gameplay/Class skill "+(i+1):"Gameplay/Class ultimate",false)).ToArray();
        }
        public void Register(IClassAbilityProvider provider)
        {
            if(provider==null||string.IsNullOrWhiteSpace(provider.ClassId))throw new ArgumentException("Ability provider needs a class ID.");
            providers.RemoveAll(p=>p.ClassId==provider.ClassId);providers.Add(provider);
        }
        public IClassAbilityProvider CurrentProvider
        {
            get
            {
                string id=stats?.Definition?.stableId;
                if(shownClass!=id){shownClass=id;page=0;}
                return providers.FirstOrDefault(p=>p.ClassId==id);
            }
        }
        public int AbilityCount=>CurrentProvider?.Count??0;
        public int PageCount
        {
            get {var p=CurrentProvider;int count=p==null?0:p.Count-(p.UltimateIndex>=0&&p.UltimateIndex<p.Count?1:0);return Mathf.Max(1,Mathf.CeilToInt(count/(float)PageSize));}
        }
        public int SkillPage {get {page=Mathf.Clamp(page,0,PageCount-1);return page;}}
        public bool SetPage(int value){if(value<0||value>=PageCount)return false;page=value;return true;}
        private int AbilityIndex(int slot)
        {
            var p=CurrentProvider;if(p==null||slot<0||slot>UltimateSlot)return -1;
            if(slot==UltimateSlot)return p.UltimateIndex>=0&&p.UltimateIndex<p.Count?p.UltimateIndex:-1;
            int index=SkillPage*PageSize+slot;
            if(p.UltimateIndex>=0&&p.UltimateIndex<=index)index++;
            return index<p.Count?index:-1;
        }
        public int UnspentPoints=>CurrentProvider?.UnspentPoints??0;
        public bool CanUpgrade(int slot)=>View(slot).canUpgrade;
        public bool Upgrade(int slot){int index=AbilityIndex(slot);return index>=0&&CanUpgrade(slot)&&CurrentProvider.Upgrade(index);}
        public string UpgradeTooltip(int slot){int index=AbilityIndex(slot);return index>=0&&CanUpgrade(slot)?CurrentProvider.UpgradeTooltip(index):"";}
        public bool HomeCasting=>returnHome!=null&&returnHome.Casting;
        public float HomeCastRemaining=>returnHome?.CastRemaining??0;
        public bool WasPressedThisFrame(int slot)=>actions!=null&&slot>=0&&slot<actions.Length&&actions[slot]!=null&&actions[slot].WasPressedThisFrame();
        public SlotView View(int slot)
        {
            if(slot<0||slot>=SlotCount||stats?.Definition==null)return default;
            if(slot>=9)return SharedView(slot);
            int index=AbilityIndex(slot);return index<0?default:CurrentProvider.View(index,InputBindings.Display(actions[slot]));
        }
        private SlotView SharedView(int slot)
        {
            string name=slot==9?"Attack / Attack Move":slot==10?"Interact":"Return to Revive Stone";
            string key=InputBindings.Display(bindings.Action(slot==9?"Gameplay/Attack selected":slot==10?"Gameplay/Interact":"Gameplay/Return to anchor"));
            string iconId=slot==9?"attack-command":slot==10?"interact":"return-home";
            float cooldown=slot==11?returnHome?.CooldownRemaining??0:0;
            string description=slot==9?"Attack a selected enemy or choose an enemy/ground destination. Shift queues attacks and attack-move orders. Works on the minimap.":
                slot==10?"Interact with the nearest available NPC, chest, loot or world object.":
                "A shared Echo skill. Cast for 10 seconds to teleport to your last activated Dice revive stone. 10 minute cooldown. Interrupted by movement or damage; unavailable in combat or dungeons. No healing.";
            if(slot==11&&returnHome!=null){description+=" Destination: "+GetComponent<RespawnAtAnchor>()?.AnchorName+".";if(returnHome.Casting)description+=" Casting: "+returnHome.CastRemaining.ToString("0.0")+"s.";}
            return new SlotView(name,key,1,1,cooldown,true,name+" ["+key+"]\n"+description,SkillIconLibrary.Get(iconId),iconId);
        }
        public bool Activate(int slot)
        {
            if(slot<0||slot>=SlotCount||stats?.Definition==null||targeting==null)return false;
            if(slot<9)
            {
                int index=AbilityIndex(slot);var view=View(slot);
                if(index<0||!view.Exists||view.passive||!view.learned)return false;
                if(returnHome!=null&&returnHome.Casting)returnHome.Cancel();
                return CurrentProvider.Activate(index,targeting,orders,Keyboard.current!=null&&Keyboard.current.shiftKey.isPressed);
            }
            if(slot!=11&&returnHome!=null&&returnHome.Casting)returnHome.Cancel();
            if(slot==9){combatInput?.BeginAttackTargeting();return combatInput!=null&&actor!=null&&actor.CanAct;}
            if(slot==10){combatInput?.CancelAttackMove();return interactor!=null&&interactor.InteractNearest();}
            return returnHome!=null&&returnHome.TryBegin();
        }
    }
}
