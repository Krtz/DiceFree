using System;
using System.IO;
using UnityEditor;

namespace DiceFree.EditorTools
{
    /// <summary>Passes the harness-only persistence opt-out across the editor play-mode domain reload.</summary>
    [InitializeOnLoad]
    internal static class PersistenceTestGuard
    {
        private const string Variable = "DICEFREE_DISABLE_PERSISTENCE";
        private const string SaveRootVariable = "DICEFREE_EDITOR_SAVE_ROOT";
        static PersistenceTestGuard() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

        public static void DisableForNextPlay()
        {
            Environment.SetEnvironmentVariable(SaveRootVariable, null);
            Environment.SetEnvironmentVariable(Variable, "1");
        }

        public static void UseIsolatedSaveRootForNextPlay(string absoluteRoot)
        {
            if (string.IsNullOrWhiteSpace(absoluteRoot) || !Path.IsPathFullyQualified(absoluteRoot))
                throw new ArgumentException("An isolated persistence root must be an absolute path.", nameof(absoluteRoot));

            Directory.CreateDirectory(absoluteRoot);
            Environment.SetEnvironmentVariable(Variable, null);
            Environment.SetEnvironmentVariable(SaveRootVariable, Path.GetFullPath(absoluteRoot));
        }

        public static void ClearValidationOverrides()
        {
            Environment.SetEnvironmentVariable(SaveRootVariable, null);
            Environment.SetEnvironmentVariable(Variable, null);
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
                ClearValidationOverrides();
        }
    }
}
