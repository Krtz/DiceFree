using DiceFree.Items;
using UnityEngine;

namespace DiceFree.UI
{
    [DisallowMultipleComponent]
    public sealed class DefenderGearPresentation : MonoBehaviour
    {
        public const string SwordId = "item.trimery.watch-sword";
        public const string ShieldId = "item.trimery.watch-shield";
        [SerializeField] private GameObject sword, shield;
        private Equipment equipment;

        public void Configure(GameObject blade, GameObject guard)
        {
            sword = blade;
            shield = guard;
        }

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
            var main = equipment?.ResolvedDefinition(EquipmentSlot.MainHand);
            var off = equipment?.ResolvedDefinition(EquipmentSlot.OffHand);
            sword.SetActive(main == null || main.stableId == SwordId);
            shield.SetActive(main?.handsRequired != 2 && (off == null || off.stableId == ShieldId));
        }
    }
}