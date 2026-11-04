using DiceFree.Persistence;
using DiceFree.UI;
using UnityEngine;
using DiceFree.ApplicationFlow;
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
            if(returning||LoadingScreen.Busy||!TryPrepareForExit())return false;
            returning=true;
            return LoadingScreen.TryLoadScene("StartMenu","Returning to your Echo",exception=>{
                returning=false;exitPrepared=false;Debug.LogError("Could not return to menu: "+exception.Message);
            });
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
