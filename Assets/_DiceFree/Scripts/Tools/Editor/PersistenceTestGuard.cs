using System;
using UnityEditor;

namespace DiceFree.EditorTools
{
    /// <summary>Passes the harness-only persistence opt-out across the editor play-mode domain reload.</summary>
    [InitializeOnLoad]
    internal static class PersistenceTestGuard
    {
        private const string Variable = "DICEFREE_DISABLE_PERSISTENCE";
        static PersistenceTestGuard() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

        public static void DisableForNextPlay() => Environment.SetEnvironmentVariable(Variable, "1");

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
                Environment.SetEnvironmentVariable(Variable, null);
        }
    }
}
