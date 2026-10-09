using DiceFree.Combat;
using DiceFree.World;

namespace DiceFree.Banking
{
    public sealed class BankInteraction : InteractionTarget
    {
        public override bool Available(CombatActor actor) => base.Available(actor) && actor.GetComponent<EchoSharedBank>() != null;
        public override void Interact(CombatActor actor) => actor.GetComponent<BankPanel>()?.Show();
    }
}
