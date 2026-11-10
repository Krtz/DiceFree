using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Advancement
{
    [DisallowMultipleComponent]
    public sealed class PsionMirrorSite : MonoBehaviour
    {
        public const string SceneName = "PsionMirrorTrial";
        public const string PuzzleId = "puzzle.arborium.mirror-trial";
        public CombatActor[] reflections;
        public Vector3 playerSpawn;
        public Rect mapBounds;
    }
}
