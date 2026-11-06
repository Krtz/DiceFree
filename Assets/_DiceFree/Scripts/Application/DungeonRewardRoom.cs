using System.Collections.Generic;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.World;
using DiceFree.Progression;
using DiceFree.UI;
using UnityEngine;
namespace DiceFree.Dungeons
{
    public sealed class DungeonRewardRoom : MonoBehaviour
    {
        sealed class Choice{public CombatActor actor;public ItemDefinition[] items;public IDungeonRewardRun run;public int xp,gold;public bool resolved;public readonly List<GameObject> chests=new();}
        readonly List<Choice> choices=new();
        public int Pending=>choices.FindAll(c=>!c.resolved).Count;
        public Transform variantAnchor;
        public void Offer(CombatActor actor,DungeonRewardPool pool,IDungeonRewardRun run,int xp,int gold)
        {
            if(actor==null||!actor.Alive||pool==null||run==null||xp<0||gold<0||HasChoice(actor))throw new System.InvalidOperationException("A living participant can have only one valid reward offer.");
            var offer=pool.Roll();
            var choice = new Choice{actor=actor,items=offer,run=run,xp=xp,gold=gold};
            choices.Add(choice);
            foreach(var item in offer)run.ObserveReward(actor,item);
            if(pool.visualVariant!=null&&variantAnchor!=null&&variantAnchor.childCount==0)Instantiate(pool.visualVariant,variantAnchor);
            // The player sees one actual treasure chest for each offered item, plus
            // a different chest containing the experience/gold alternative.
            // Chests are private to their owner, and the first claim resolves all.
            int count=offer.Length+1;
            for(int i=0;i<count;i++)
            {
                int option=i==offer.Length?-1:i;
                string label=option==-1?"Bonus: "+xp+" EXP + "+gold+" gold":offer[i].displayName;
                Vector3 offset=new Vector3((i-(count-1)*.5f)*2.05f,0f,3.1f+(choices.Count-1)*2.25f);
                GameObject chest=WorldLootVisual.Create(actor.transform.position+offset,
                    "Reward chest - "+label,.85f);
                var pickup=chest.AddComponent<DungeonRewardChest>();
                pickup.Configure(this,actor,option,label,i+1,option<0?null:offer[option]);
                choice.chests.Add(chest);
                actor.GetComponent<InteractionRegistry>()?.Register(pickup);
            }
        }
        public bool HasChoice(CombatActor actor) =>
            choices.Exists(c=>c.actor==actor&&!c.resolved);

        public bool Choose(CombatActor actor,int option)
        {
            var c=choices.Find(v=>v.actor==actor&&!v.resolved);if(c==null||actor==null||!actor.Alive||option < -1||option>=c.items.Length)return false;
            var wallet=actor.GetComponent<GoldWallet>();
            if(option==-1&&wallet!=null&&wallet.Gold>long.MaxValue-c.gold)return false;
            // Resolve before callbacks: inventory/XP notifications must not claim twice.
            c.resolved=true;
            if(option==-1){actor.GetComponent<ExperienceProgression>()?.Grant(c.xp);actor.GetComponent<GoldWallet>()?.Grant(c.gold);}
            else c.run.Drop(actor,c.items[option]);
            foreach(var chest in c.chests)if(chest!=null)Destroy(chest);
            c.run.ReturnPlayer(actor,true);
            FinishRunIfResolved(c.run);
            return true;
        }
        public bool DiscardForSessionExit(CombatActor actor)
        {
            var c=choices.Find(v=>v.actor==actor&&!v.resolved);if(c==null)return false;
            c.resolved=true;foreach(var chest in c.chests)if(chest!=null)Destroy(chest);
            c.run.ReturnPlayer(actor,actor.Alive);FinishRunIfResolved(c.run);return true;
        }
        void FinishRunIfResolved(IDungeonRewardRun run)
        {if(!choices.Exists(c=>c.run==run&&!c.resolved))run.Finish();}
        void OnEnable()=>HudPointerBlocker.OverlayCovers+=CoversPointer;
        void OnDisable()=>HudPointerBlocker.OverlayCovers-=CoversPointer;
        Choice LocalChoice(){var actor=FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>();return choices.Find(c=>!c.resolved&&c.actor==actor);}
        static Rect Card(int index,int count)=>new Rect(Screen.width*.5f+(index-(count-1)*.5f)*240-115,Screen.height*.2f,230,106);
        bool CoversPointer(Vector2 p)
        {
            var choice=LocalChoice();if(choice==null)return false;var point=new Vector2(p.x,Screen.height-p.y);
            if(new Rect(15,80,445,57).Contains(point))return true;
            for(int i=0;i<=choice.items.Length;i++)if(Card(i,choice.items.Length+1).Contains(point))return true;return false;
        }
        static string ItemDetails(ItemDefinition item)
        {
            string Name(string id)=>string.Join(" ",id.Replace("class.","").Split('-').Select(s=>s.Length==0?s:char.ToUpperInvariant(s[0])+s.Substring(1)));
            string classes=item.allowedClassIds==null||item.allowedClassIds.Length==0?"All classes":string.Join(", ",item.allowedClassIds.Select(Name));
            string extra=item.stats.regeneration!=0?$"\n+{item.stats.regeneration:0.#} Health regeneration":"";
            if(item.stats.basicAttackMaximumBonus!=0)extra+=$"\n+{item.stats.basicAttackMaximumBonus:0.#} Maximum basic attack damage";
            return HudTooltip.Item(item)+extra+"\n\nUsable by: "+classes;
        }
        void OnGUI()
        {
            // The reward is a real world interaction, not a menu of buttons.
            if(Pending==0||HudPointerBlocker.ModalOpen)return;
            GUI.Box(new Rect(15,80,445,57),
                "Treasure room: choose ONE chest to claim.\nInspect the offers, then interact with the matching numbered chest.");
            var choice=LocalChoice();if(choice==null)return;string tooltip=null;Rect anchor=default;
            var title=new GUIStyle(GUI.skin.label){fontSize=14,fontStyle=FontStyle.Bold,wordWrap=true};title.normal.textColor=new Color(.85f,.91f,1);
            for(int i=0;i<=choice.items.Length;i++)
            {
                var r=Card(i,choice.items.Length+1);var old=GUI.color;GUI.color=new Color(.06f,.10f,.16f,.97f);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;GUI.Box(r,GUIContent.none);
                bool bonus=i==choice.items.Length;var item=bonus?null:choice.items[i];GUI.Label(new Rect(r.x+9,r.y+5,210,20),"CHEST "+(i+1)+(bonus?" · XP & GOLD":" · ITEM"));
                if(item?.icon!=null)GUI.DrawTexture(new Rect(r.x+9,r.y+30,44,44),item.icon.texture,ScaleMode.ScaleToFit,true);
                GUI.Label(new Rect(r.x+(bonus?10:61),r.y+28,bonus?210:160,49),bonus?$"{choice.xp} XP + {choice.gold} gold":item.displayName,title);
                GUI.Label(new Rect(r.x+9,r.y+79,215,20),bonus?"Choose instead of an item":"Hover to inspect stats and classes");
                if(r.Contains(HudTooltip.CurrentGuiPointerPosition())){tooltip=bonus?"Take the XP and gold instead of any offered item.":ItemDetails(item);anchor=r;}
            }
            if(tooltip!=null)HudTooltip.DrawAt(tooltip,anchor);
        }
    }
}
