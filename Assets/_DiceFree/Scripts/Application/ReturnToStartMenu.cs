using DiceFree.Persistence;
using DiceFree.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace DiceFree.Dungeons
{
    [DisallowMultipleComponent,RequireComponent(typeof(ManifestationPersistence))]
    public sealed class ReturnToStartMenu : MonoBehaviour
    {
        OptionsPanel options;bool returning,exitPrepared;
        void OnEnable(){Application.wantsToQuit+=OnWantsToQuit;}
        void OnDisable(){Application.wantsToQuit-=OnWantsToQuit;}
        void Start(){options=FindAnyObjectByType<OptionsPanel>();if(options!=null){options.ReturnToStartMenu=Return;options.QuitToDesktop=QuitToDesktop;}}
        public bool Return()
        {
            if(returning||!TryPrepareForExit())return false;
            returning=true;
            SceneManager.LoadScene("StartMenu",LoadSceneMode.Single);return true;
        }
        private bool QuitToDesktop()
        {
            if(!TryPrepareForExit())return false;
            Application.Quit();
            return true;
        }
        private bool OnWantsToQuit()=>exitPrepared||TryPrepareForExit();
        private bool TryPrepareForExit()
        {
            if(exitPrepared)return true;
            if(DiceFree.Development.DevelopmentRoomController.Current?.Busy==true||!GetComponent<ManifestationPersistence>().Flush())return false;
            exitPrepared=true;returning=true;SlimeDungeonRun.Current?.ReleaseForSessionExit();options?.Close();
            GetComponent<SkillTargetingController>()?.Cancel();GetComponent<QuestJournalPanel>()?.Close();
            DiceFree.Foundation.DevelopmentSession.End();
            return true;
        }
        void OnDestroy(){if(options!=null){options.ReturnToStartMenu=null;options.QuitToDesktop=null;}}
    }
}
