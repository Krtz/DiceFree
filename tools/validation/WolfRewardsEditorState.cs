using System.Linq;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Dungeons;
using DiceFree.Foundation;
public static class WolfRewardsEditorState
{
 public static object Run(){var p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>();var portal=Object.FindAnyObjectByType<WolfDungeonPortal>();return new{playing=EditorApplication.isPlaying,scenes=Enumerable.Range(0,SceneManager.sceneCount).Select(i=>new{SceneManager.GetSceneAt(i).path,SceneManager.GetSceneAt(i).isDirty}).ToArray(),phase=WolfDungeonRun.Current?.Phase.ToString(),position=p?.transform.position.ToString(),alive=p?.Alive,lockOwner=p?.GetComponent<RunLoadoutLock>()?.Owner?.name,feedback=portal?.Feedback,canInteract=portal!=null&&p!=null&&portal.CanInteract(p)};}
}
