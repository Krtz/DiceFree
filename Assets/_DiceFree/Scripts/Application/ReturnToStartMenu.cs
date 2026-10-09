using DiceFree.Persistence;
using DiceFree.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace DiceFree.Dungeons
{
    [DisallowMultipleComponent,RequireComponent(typeof(ManifestationPersistence))]
    public sealed class ReturnToStartMenu : MonoBehaviour
    {
        OptionsPanel options;bool returning;
        void Start(){options=FindAnyObjectByType<OptionsPanel>();if(options!=null)options.ReturnToStartMenu=Return;}
        public bool Return()
        {
            if(returning||!GetComponent<ManifestationPersistence>().Flush())return false;
            returning=true;SlimeDungeonRun.Current?.ReleaseForSessionExit();options?.Close();
            GetComponent<SkillTargetingController>()?.Cancel();GetComponent<QuestJournalPanel>()?.Close();
            SceneManager.LoadScene("StartMenu",LoadSceneMode.Single);return true;
        }
        void OnDestroy(){if(options!=null)options.ReturnToStartMenu=null;}
    }
}
