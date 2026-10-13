using UnityEngine;
namespace DiceFree.Foundation
{
    public static class DevelopmentSession
    {
        public static bool Active { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void End() => Active = false;
        public static void Begin() => Active = true;
    }
}
