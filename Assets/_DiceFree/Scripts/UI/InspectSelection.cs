using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>
    /// Independent examination target, separate from hostile attack selection.
    /// NPCs can be inspected by left click without being attackable.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InspectSelection : MonoBehaviour
    {
        public CombatActor Actor { get; private set; }
        public InteractionTarget Npc { get; private set; }
        public Transform SelectedTransform =>
            Actor!=null?Actor.transform:Npc!=null?Npc.transform:null;
        public void Show(CombatActor actor,InteractionTarget npc)
        {
            Actor=actor!=null && actor.Alive?actor:null;
            Npc=Actor==null?npc:null;
        }
        public void Clear()=>Show(null,null);
    }
}
