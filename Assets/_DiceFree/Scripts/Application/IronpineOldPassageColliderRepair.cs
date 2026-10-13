using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.Ironpine
{
    /// <summary>
    /// Previously authored woodland boundary trees have invisible trunk
    /// colliders suspended near head height, above navigable ground. Preserve
    /// the trees themselves but stop those orphaned playtest collider volumes
    /// from blocking passage through the southern forest transition.
    /// </summary>
    internal static class IronpineOldPassageColliderRepair
    {
        public static int LastDisabled { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded-=OnSceneLoaded;
            SceneManager.sceneLoaded+=OnSceneLoaded;
            Apply();
        }
        private static void OnSceneLoaded(Scene scene,LoadSceneMode mode) => Apply();
        private static void Apply()
        {
            int disabled=0;
            foreach(var collider in UnityEngine.Object.FindObjectsByType<CapsuleCollider>())
            {
                if(!collider.enabled || collider.name!="Playtest trunk collision")continue;
                var trunk=collider.transform;
                if(trunk.position.x<65f || trunk.position.x>150f ||
                   trunk.position.z<42f || trunk.position.z>106f ||
                   trunk.position.y<2.5f ||
                   collider.bounds.min.y<1.5f)continue;
                bool authoredWoodland=trunk.parent?.parent?.name=="Woodland tree";
                if(!authoredWoodland)continue;
                collider.enabled=false;
                disabled++;
            }
            LastDisabled=disabled;
            if(disabled>0)
                Debug.Log("IRONPINE_OLD_INVISIBLE_TRUNK_BARRIERS_DISABLED "+disabled);
        }
    }
}
