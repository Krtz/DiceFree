using DiceFree.Items;
using UnityEngine;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent]
    public sealed class RangerBowPresentation : MonoBehaviour
    {
        public const string ShortbowId = "item.trimery.ironpine-shortbow";
        public const string LongbowId = "item.trimery.ironpine-longbow";

        [SerializeField] private GameObject shortbow, longbow;
        private Equipment equipment;

        private void Awake() => equipment = GetComponentInParent<Equipment>();
        private void OnEnable()
        {
            if (equipment != null) equipment.Changed += Refresh;
            Refresh();
        }
        private void OnDisable()
        {
            if (equipment != null) equipment.Changed -= Refresh;
        }
        private void Refresh()
        {
            bool usesLongbow = equipment?.ResolvedDefinition(EquipmentSlot.MainHand)?.stableId == LongbowId;
            shortbow.SetActive(!usesLongbow);
            longbow.SetActive(usesLongbow);
        }

        public void Configure(GameObject shortVisual, GameObject longVisual)
        {
            shortbow = shortVisual;
            longbow = longVisual;
            shortbow.SetActive(true);
            longbow.SetActive(false);
        }
    }
}
