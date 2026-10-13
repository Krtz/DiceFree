using DiceFree.Combat;
using DiceFree.World;
namespace DiceFree.Development
{
    public sealed class DevelopmentPortal : InteractionTarget
    {
        public string targetScene;
        public override void Interact(CombatActor actor)
        {
            if (CanInteract(actor)) DevelopmentRoomController.Current?.Travel(targetScene);
        }
    }
}
