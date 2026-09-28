using UnityEngine;

namespace DiceFree.Combat
{
    // A null element reference means no element. New identities require data, never enum/switch edits.
    [CreateAssetMenu(menuName = "DiceFree/Combat/Element")]
    public sealed class ElementDefinition : ScriptableObject
    {
        public string stableId;
        public string displayName;
        public Sprite icon;
    }
}
