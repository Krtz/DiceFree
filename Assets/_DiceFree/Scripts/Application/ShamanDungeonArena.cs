using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Dungeons
{
    public sealed class ShamanDungeonArena : MonoBehaviour
    {
        public Transform arrival;
        public CombatActor[] opponents;
        public GameObject[] braziers;
        public DungeonRewardPool rewards;
        public GameObject startPrompt;

        internal void Awaken(int stage)
        {
            if (stage < braziers.Length) braziers[stage].SetActive(true);
            opponents[stage].gameObject.SetActive(true);
        }

        internal void Extinguish(int stage) => braziers[stage].SetActive(false);
    }
}

