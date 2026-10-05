using DiceFree.Input;
using DiceFree.UI;
using UnityEngine;

namespace DiceFree.Input
{
    /// <summary>Session composition/lifetime only; the binding service, UI and storage own their own work.</summary>
    public sealed class InputRuntimeHost : MonoBehaviour
    {
        private static InputRuntimeHost instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void StartSession()
        {
            if (instance != null) return;
            var host = new GameObject("Input & Options");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<InputRuntimeHost>();
            host.AddComponent<OptionsPanel>();
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            instance = null;
            InputBindings.ResetSession();
        }
    }
}
