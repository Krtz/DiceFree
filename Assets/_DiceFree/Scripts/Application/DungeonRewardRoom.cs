using System.Collections.Generic;
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
        sealed class Choice{public CombatActor actor;public ItemDefinition[] items;public SlimeDungeonRun run;public int xp,gold;public bool resolved;public readonly List<GameObject> chests=new();}
        readonly List<Choice> choices=new();
        public int Pending=>choices.FindAll(c=>!c.resolved).Count;
        public Transform variantAnchor;
        public void Offer(CombatActor actor,DungeonRewardPool pool,SlimeDungeonRun run,int xp,int gold)
        {
            var offer=pool.Roll();
            var choice = new Choice{actor=actor,items=offer,run=run,xp=xp,gold=gold};
            choices.Add(choice);
            foreach(var item in offer)run.ObserveReward(actor,item);
            if(pool.visualVariant!=null&&variantAnchor.childCount==0)Instantiate(pool.visualVariant,variantAnchor);
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
                pickup.Configure(this,actor,option,label);
                choice.chests.Add(chest);
                actor.GetComponent<InteractionRegistry>()?.Register(pickup);
            }
        }
        public bool HasChoice(CombatActor actor) =>
            choices.Exists(c=>c.actor==actor&&!c.resolved);

        public bool Choose(CombatActor actor,int option)
        {
            var c=choices.Find(v=>v.actor==actor&&!v.resolved);if(c==null||option < -1||option>=c.items.Length)return false;
            if(option==-1){actor.GetComponent<ExperienceProgression>()?.Grant(c.xp);actor.GetComponent<GoldWallet>()?.Grant(c.gold);}
            else c.run.Drop(actor,c.items[option]);
            c.resolved=true;
            foreach(var chest in c.chests)if(chest!=null)Destroy(chest);
            c.run.ReturnPlayer(actor,true);
            if(Pending==0)c.run.Finish();
            return true;
        }
        void OnGUI()
        {
            // The reward is a real world interaction, not a menu of buttons.
            if(Pending==0)return;
            GUI.Box(new Rect(15,80,445,57),
                "Treasure room: choose ONE chest to claim.\nRight-click a chest or press interact nearby; you will return to Cornberg.");
        }
    }
}
