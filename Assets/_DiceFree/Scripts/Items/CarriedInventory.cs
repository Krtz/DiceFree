using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceFree.Items
{
    public sealed class CarriedInventory : MonoBehaviour
    {
        [SerializeField] private ItemDefinition[] definitions = Array.Empty<ItemDefinition>();
        private readonly List<ItemInstance> owned = new();
        public event Action Changed;
        public ItemInstance[] Items => owned.Select(i => i.Copy()).ToArray();
        public IReadOnlyList<ItemDefinition> Definitions => definitions;
        public void Configure(ItemDefinition[] values) => definitions = values;
        public ItemDefinition Resolve(string id) => Array.Find(definitions, d => d != null && d.stableId == id);
        public ItemInstance Find(string id) => owned.Find(i => i.instanceId == id)?.Copy();
        public ItemInstance Grant(string definitionId)
        {
            if (Resolve(definitionId) == null) throw new ArgumentException("Unresolved item cannot be granted.");
            var item = new ItemInstance { instanceId = Guid.NewGuid().ToString("D"), definitionId = definitionId };
            owned.Add(item); Changed?.Invoke(); return item.Copy();
        }
        public bool Remove(string instanceId)
        {
            int index = owned.FindIndex(i => i.instanceId == instanceId);
            if (index < 0) return false;
            owned.RemoveAt(index); Changed?.Invoke(); return true;
        }
        public static bool Valid(ItemInstance[] items) => items != null &&
            items.All(i => i != null && Guid.TryParse(i.instanceId, out _) && !string.IsNullOrWhiteSpace(i.definitionId)) &&
            items.Select(i => i.instanceId).Distinct(StringComparer.Ordinal).Count() == items.Length;
        public void Restore(ItemInstance[] items)
        {
            if (!Valid(items)) throw new ArgumentException("Invalid owned item records.");
            owned.Clear(); owned.AddRange(items.Select(i => i.Copy())); Changed?.Invoke();
        }
    }
}
