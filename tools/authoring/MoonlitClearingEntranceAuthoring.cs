using System;
using System.IO;
using System.Linq;
using DiceFree.EditorTools;
using DiceFree.Quests;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public static class MoonlitClearingEntranceAuthoring
{
    public static void Batch()
    {
        var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
        var journal = Object.FindObjectsByType<QuestJournal>().Single();
        var mira = Object.FindObjectsByType<QuestGiver>().Single(giver =>
            giver.Offers.Any(quest => quest != null && quest.stableId == CornbergSlimeRepeatAuthoring.QuestId));
        CornbergSlimeRepeatAuthoring.InstallRedSlimeHunt(journal, mira);
        var entrance = GameObject.Find("Wolf Survival entrance").transform;
        var tree = Object.FindObjectsByType<Transform>().Single(item => item.name == "Trees/Tree_Spruce - 3D" &&
            item.parent != null && item.parent.name == "Ironpine conifer 047" &&
            Vector3.Distance(item.position, entrance.position) < 3);
        var target = new Vector3(167.4f, 40, 91.6f);
        Physics.SyncTransforms();
        if (!Physics.Raycast(target, Vector3.down, out var ground, 80, 1 << 8))
            throw new InvalidOperationException("The clearing spruce needs a valid ground position.");
        tree.position = ground.point;
        PrefabUtility.RecordPrefabInstancePropertyModifications(tree);
        Physics.SyncTransforms();

        var roadwarden = Object.FindObjectsByType<MonoBehaviour>().Single(item => item.GetType().Name == "OgreRoadwardenRoute");
        using var serialized = new SerializedObject(roadwarden);
        var wreckage = serialized.FindProperty("wreckage");
        var closed = Enumerable.Range(0, wreckage.arraySize)
            .Select(i => (GameObject)wreckage.GetArrayElementAtIndex(i).objectReferenceValue)
            .Where(piece => piece.activeSelf).ToArray();
        try
        {
            foreach (var piece in closed) piece.SetActive(false);
            CornbergSceneBuilder.Rebake();
        }
        finally
        {
            foreach (var piece in closed) piece.SetActive(true);
        }
        var nav = NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            var path = new NavMeshPath();
            if (!NavMesh.SamplePosition(entrance.position + new Vector3(5, 0, -9), out var road, 2, 1) ||
                !NavMesh.SamplePosition(entrance.position, out var gate, 1, 1) ||
                !NavMesh.CalculatePath(road.position, gate.position, 1, path) || path.status != NavMeshPathStatus.PathComplete)
                throw new InvalidOperationException("Moonlit Clearing approach must remain walkable.");
        }
        finally { nav.Remove(); }
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save Cornberg.");
        AssetDatabase.SaveAssets();
        File.WriteAllLines("Logs/QuestBalanceSceneIds.txt", new Object[] { journal, mira, tree }
            .Select(item =>
            {
                var id = GlobalObjectId.GetGlobalObjectIdSlow(item);
                return (item == tree && id.targetPrefabId != 0 ? id.targetPrefabId : id.targetObjectId).ToString();
            }));
        Debug.Log("DICEFREE_QUEST_BALANCE_AUTHORING_OK");
    }
}
