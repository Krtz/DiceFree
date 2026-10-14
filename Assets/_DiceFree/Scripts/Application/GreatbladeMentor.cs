using System.Linq;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Progression;
using DiceFree.World;
using DiceFree.UI;
using UnityEngine;
namespace DiceFree.Visuals
{
    public sealed class GreatbladeMentor:InteractionTarget
    {
        CombatActor visitor;
        public override bool Available(CombatActor actor)=>base.Available(actor)&&actor.Stats.Definition?.stableId=="class.physically-blessed-novice"&&actor.Stats.Level>=30;
        public override void Interact(CombatActor actor){if(CanInteract(actor))visitor=actor;}
        void OnGUI()
        {
            if(visitor==null||!CanInteract(visitor)||HudPointerBlocker.ModalOpen){visitor=null;return;}
            var controller=visitor.GetComponent<AdvancementController>();
            var edge=controller?.Definitions.FirstOrDefault(x=>x.targetClass.stableId==GreatbladeProgressionRules.ClassId);
            if(edge==null)return;
            var box=new Rect(Screen.width*.5f-240,Screen.height*.3f,480,160);
            GUI.Box(box,"The retired swordswoman");
            GUI.Label(new Rect(box.x+16,box.y+30,448,60),"Learn the Greatblade's Way. Begin at class level 1.\nYour Physically Blessed manifestation remains available.\nWide Arc, Spin Dash, Mastery and Heavy Rhythm start learned.");
            GUI.enabled=controller.CanAdvance(edge);
            if(GUI.Button(new Rect(box.x+16,box.y+100,280,34),"Become a Greatblade")){controller.TryAdvance(edge);visitor=null;}
            GUI.enabled=true;
            if(GUI.Button(new Rect(box.x+310,box.y+100,154,34),"Later"))visitor=null;
        }
    }
}
