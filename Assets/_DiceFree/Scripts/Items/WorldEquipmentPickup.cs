using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Items
{
    public sealed class WorldEquipmentPickup : InteractionTarget
    {
        [SerializeField] private ItemDefinition item;
        public bool Consumed { get; private set; }
        public ItemDefinition Item => item;
        public void Configure(ItemDefinition value) { item = value; ConfigureName("Pick up " + value.displayName); }
        public override bool CanInteract(CombatActor actor) => !Consumed && base.CanInteract(actor) &&
            actor.TryGetComponent<CarriedInventory>(out var inventory) && inventory.Resolve(item.stableId) != null;
        public override void Interact(CombatActor actor) => TryPickup(actor);
        private void OnGUI()
        {
            if (Consumed || item == null || Camera.main == null) return;
            var point = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
            if (point.z > 0 && point.z < 40)
                GUI.Box(new Rect(point.x - 120, Screen.height - point.y, 240, 42), item.displayName + "\nI / right-click to pick up");
        }
        public bool TryPickup(CombatActor actor)
        {
            if (!CanInteract(actor)) return false;
            Consumed = true; // Claim once before inventory events; no killer/party reservation.
            FixedItemGrant.Grant(actor.GetComponent<CarriedInventory>(), item);
            Destroy(gameObject); return true;
        }
    }
}
