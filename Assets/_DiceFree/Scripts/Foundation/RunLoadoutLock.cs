using UnityEngine;
namespace DiceFree.Foundation
{
    // Session-only lock. Inventory consumables deliberately do not consult this gate.
    public sealed class RunLoadoutLock : MonoBehaviour
    {
        public bool Locked { get; set; }
        public bool InsideDungeon { get; set; }
        public Object Owner { get; set; }
        public System.Func<bool> DeathReturnHandler { get; set; }
    }
}
