using System;
using System.IO;
using UnityEngine;

namespace DiceFree.Persistence
{
    public static class EchoSaveLocation
    {
        public const string EditorSaveRootVariable = "DICEFREE_EDITOR_SAVE_ROOT";

        public static string ResolveRoot()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-diceFreeSaveRoot");
            if (index >= 0)
            {
                if (index + 1 >= args.Length || !Path.IsPathRooted(args[index + 1]))
                    throw new InvalidDataException("-diceFreeSaveRoot requires an absolute isolated directory.");
                return args[index + 1];
            }

            if (Application.isEditor)
            {
                string editorRoot = Environment.GetEnvironmentVariable(EditorSaveRootVariable);
                if (!string.IsNullOrEmpty(editorRoot))
                {
                    if (!Path.IsPathFullyQualified(editorRoot))
                        throw new InvalidDataException(EditorSaveRootVariable + " must be an absolute isolated directory.");
                    return editorRoot;
                }
            }

            if (Application.isBatchMode ||
                Environment.GetEnvironmentVariable("DICEFREE_DISABLE_PERSISTENCE") == "1")
                return null;

            return Path.Combine(
                Application.persistentDataPath,
                Application.isEditor ? "EditorProfiles" : "Profiles");
        }
    }
}
