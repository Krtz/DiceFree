using DiceFree.AI;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class CombatValidationActors
    {
        public static CombatActor Find(string contentId)
        {
            foreach (var actor in Object.FindObjectsByType<CombatActor>())
                if (actor.GetComponent<ActorStats>().Definition.stableId == contentId) return actor;
            throw new System.InvalidOperationException("Missing actor fixture: " + contentId);
        }
        public static AggroBehaviour IsolateCropDuel()
        {
            var crop = Find("enemy.crop-slime");
            foreach (var brain in Object.FindObjectsByType<AggroBehaviour>())
                if (brain.gameObject != crop.gameObject) brain.gameObject.SetActive(false);
            return crop.GetComponent<AggroBehaviour>();
        }
    }
}
