using System;
using System.IO;
using UnityEngine;
using DiceFree.Foundation;

namespace DiceFree.Persistence
{
    public static class EchoSaveLocation
    {
        public const string EditorSaveRootVariable = "DICEFREE_EDITOR_SAVE_ROOT";

        // Explicit temporary-session override, used by the isolated multiplayer lab.
        public static string TemporaryRootOverride { get; private set; }
        public static void SetTemporaryRoot(string root)
        {
            if (root != null && !Path.IsPathFullyQualified(root))
                throw new InvalidDataException("Temporary save root must be absolute.");
            TemporaryRootOverride = root;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetTemporaryRoot() => TemporaryRootOverride = null;

        public static string ResolveRoot()
        {
            if (TemporaryRootOverride != null) return SessionRoot(TemporaryRootOverride);
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-diceFreeSaveRoot");
            if (index >= 0)
            {
                if (index + 1 >= args.Length || !Path.IsPathRooted(args[index + 1]))
                    throw new InvalidDataException("-diceFreeSaveRoot requires an absolute isolated directory.");
                return SessionRoot(args[index + 1]);
            }

            if (Application.isEditor)
            {
                string editorRoot = Environment.GetEnvironmentVariable(EditorSaveRootVariable);
                if (!string.IsNullOrEmpty(editorRoot))
                {
                    if (!Path.IsPathFullyQualified(editorRoot))
                        throw new InvalidDataException(EditorSaveRootVariable + " must be an absolute isolated directory.");
                    return SessionRoot(editorRoot);
                }
            }

            if (Application.isBatchMode ||
                Environment.GetEnvironmentVariable("DICEFREE_DISABLE_PERSISTENCE") == "1")
                return null;

            return SessionRoot(Path.Combine(
                Application.persistentDataPath,
                Application.isEditor ? "EditorProfiles" : "Profiles"));
        }

        private static string SessionRoot(string root) =>
            DevelopmentSession.Active ? Path.Combine(root, "DevelopmentRoom") : root;

    }
}
