using System;
using DiceFree.Foundation;
using DiceFree.Persistence;
using DiceFree.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace DiceFree.Core
{
    public static class ControlsComposition
    {
        private static LocalControlSettingsStore store;
        public static string Feedback { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void StartSession()
        {
            string directory=UnityEngine.Application.persistentDataPath;
#if UNITY_EDITOR
            string isolated=Environment.GetEnvironmentVariable("DICEFREE_EDITOR_SAVE_ROOT");
            if(!string.IsNullOrEmpty(isolated)) directory=isolated;
#endif
            store=new LocalControlSettingsStore(directory);
            store.Load(); Feedback=store.Feedback;
            ControlBindings.Changed+=Save;
            SceneManager.sceneLoaded-=SceneLoaded;
            SceneManager.sceneLoaded+=SceneLoaded;
        }
        private static void Save()
        {
            try { store.Save(ControlBindings.Snapshot()); Feedback=store.Feedback; }
            catch(Exception error) { Feedback="Could not save controls: " + error.Message; Debug.LogWarning(Feedback); }
            OptionsPanel.SaveFeedback=Feedback;
        }
        private static void SceneLoaded(Scene scene,LoadSceneMode mode)
        {
            foreach(var input in UnityEngine.Object.FindObjectsByType<DiceFree.Characters.TraversalInput>(FindObjectsSortMode.None))
                if(input.GetComponent<OptionsPanel>()==null) input.gameObject.AddComponent<OptionsPanel>();
            OptionsPanel.SaveFeedback=Feedback;
        }
    }
}

