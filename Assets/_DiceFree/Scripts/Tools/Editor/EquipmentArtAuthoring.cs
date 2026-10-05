using System.Linq;
using UnityEditor;
using UnityEngine;
namespace DiceFree.EditorTools { internal static class EquipmentArtAuthoring {
        internal static GameObject Bind(Transform novice, string prefabPath, string name)
        {
            // Retain the same source-rig bind poses. Resolve the exported rig once
            // during authoring; runtime presentation never searches bone names.
            var root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), novice.gameObject.scene);
            PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            root.name = name;
            root.transform.SetParent(novice, false);
            var bones = novice.GetComponentsInChildren<Transform>(true)
                .Where(t => !t.IsChildOf(root.transform)).GroupBy(t => t.name).ToDictionary(g => g.Key, g => g.First());
            var skins = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var skin in skins)
            {
                skin.bones = skin.bones.Select(b => bones[b.name]).ToArray();
                skin.rootBone = bones[skin.rootBone.name];
                skin.transform.SetParent(root.transform, true);
            }
            foreach (var child in root.transform.Cast<Transform>().Where(t => t.GetComponent<SkinnedMeshRenderer>() == null).ToArray())
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            return root;
        }

} }

