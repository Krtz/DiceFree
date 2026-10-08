using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Progression;
using UnityEngine;
namespace DiceFree.Dungeons
{
    public sealed class DungeonRewardRoom : MonoBehaviour
    {
        sealed class Choice{public CombatActor actor;public ItemDefinition[] items;public SlimeDungeonRun run;public int xp,gold;public bool resolved;}
        readonly List<Choice> choices=new();
        public int Pending=>choices.FindAll(c=>!c.resolved).Count;
        public Transform variantAnchor;
        public void Offer(CombatActor actor,DungeonRewardPool pool,SlimeDungeonRun run,int xp,int gold)
        {
            var offer=pool.Roll();choices.Add(new Choice{actor=actor,items=offer,run=run,xp=xp,gold=gold});
            foreach(var item in offer)run.ObserveReward(actor,item);
            if(pool.visualVariant!=null&&variantAnchor.childCount==0)Instantiate(pool.visualVariant,variantAnchor);
        }
        public bool Choose(CombatActor actor,int option)
        {
            var c=choices.Find(v=>v.actor==actor&&!v.resolved);if(c==null||option < -1||option>=c.items.Length)return false;
            if(option==-1){actor.GetComponent<ExperienceProgression>()?.Grant(c.xp);actor.GetComponent<GoldWallet>()?.Grant(c.gold);}
            else c.run.Drop(actor,c.items[option]);
            c.resolved=true;c.run.ReturnPlayer(actor,true);if(Pending==0)c.run.Finish();return true;
        }
        void OnGUI()
        {
            int y=160;
            foreach(var c in choices)
            {
                if(c.resolved)continue;
                GUI.Box(new Rect(15,y,390,130),c.actor.name+" — choose your private reward");
                for(int i=0;i<c.items.Length;i++)if(GUI.Button(new Rect(25,y+30+i*28,370,25),c.items[i].displayName))Choose(c.actor,i);
                if(GUI.Button(new Rect(25,y+95,370,25),$"Instead: {c.xp} bonus EXP + {c.gold} gold"))Choose(c.actor,-1);
                y+=140;
            }
        }
    }
}
