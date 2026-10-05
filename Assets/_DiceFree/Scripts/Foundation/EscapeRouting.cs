namespace DiceFree.Foundation
{
    public enum EscapeRoute { CancelCapture, CloseOptions, CloseInventory, CloseInteraction, ClearTarget, OpenOptions }
    public static class EscapeRouting
    {
        public static EscapeRoute Choose(bool capture,bool options,bool inventory,bool interaction,bool target) =>
            capture ? EscapeRoute.CancelCapture : options ? EscapeRoute.CloseOptions : inventory ? EscapeRoute.CloseInventory :
            interaction ? EscapeRoute.CloseInteraction : target ? EscapeRoute.ClearTarget : EscapeRoute.OpenOptions;
    }
}
