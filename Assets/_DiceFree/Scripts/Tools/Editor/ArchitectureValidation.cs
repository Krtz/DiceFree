using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DiceFree.EditorTools
{
    /// <summary>Checks the actual first-party asmdef/asmref graph and source ownership.</summary>
    public static class ArchitectureValidation
    {
        [Serializable] private sealed class Definition
        {
            public string name;
            public string[] references = Array.Empty<string>();
            public string[] includePlatforms = Array.Empty<string>();
        }
        [Serializable] private sealed class Reference { public string reference; }
        private sealed class AssemblyInfo
        {
            public string Name;
            public string Path;
            public bool EditorOnly;
            public readonly List<string> References = new();
        }

        private static readonly Dictionary<string, string[]> Allowed = new(StringComparer.Ordinal)
        {
            ["DiceFree.Foundation.Runtime"] = Array.Empty<string>(),
            ["DiceFree.Gameplay.Runtime"] = new[] { "DiceFree.Foundation.Runtime" },
            ["DiceFree.Items.Runtime"] = new[] { "DiceFree.Foundation.Runtime", "DiceFree.Gameplay.Runtime" },
            ["DiceFree.World.Runtime"] = new[] { "DiceFree.Foundation.Runtime", "DiceFree.Gameplay.Runtime" },
            ["DiceFree.Quests.Runtime"] = new[] { "DiceFree.Foundation.Runtime", "DiceFree.Gameplay.Runtime", "DiceFree.Items.Runtime", "DiceFree.World.Runtime" },
            ["DiceFree.AI.Runtime"] = new[] { "DiceFree.Foundation.Runtime", "DiceFree.Gameplay.Runtime", "DiceFree.World.Runtime" },
            ["DiceFree.Persistence.Runtime"] = new[] { "DiceFree.Foundation.Runtime", "DiceFree.Gameplay.Runtime", "DiceFree.Items.Runtime", "DiceFree.World.Runtime", "DiceFree.Quests.Runtime" },
            ["DiceFree.UI.Runtime"] = new[] { "DiceFree.Foundation.Runtime", "DiceFree.Gameplay.Runtime", "DiceFree.Items.Runtime", "DiceFree.World.Runtime", "DiceFree.Quests.Runtime", "DiceFree.AI.Runtime" },
            ["DiceFree.Application.Runtime"] = new[] { "DiceFree.Foundation.Runtime", "DiceFree.Gameplay.Runtime", "DiceFree.Items.Runtime", "DiceFree.World.Runtime", "DiceFree.Quests.Runtime", "DiceFree.AI.Runtime", "DiceFree.Persistence.Runtime", "DiceFree.UI.Runtime" }
        };

        [MenuItem("DiceFree/Validation/Architecture boundaries")]
        public static void Run()
        {
            var errors = new List<string>();
            string scriptsRoot = Path.GetFullPath("Assets/_DiceFree/Scripts");
            var definitions = LoadDefinitions(scriptsRoot, errors);
            ValidateDefinitions(definitions, errors);
            ValidateOwnership(scriptsRoot, definitions, errors);
            if (errors.Count > 0)
                throw new InvalidOperationException("DiceFree architecture validation failed:\n - " + string.Join("\n - ", errors));
            Debug.Log($"DICEFRE_ARCHITECTURE_OK: {definitions.Count} explicit first-party assemblies; all first-party C# owned; dependency graph acyclic and within policy.");
        }

        private static Dictionary<string, AssemblyInfo> LoadDefinitions(string root, List<string> errors)
        {
            var result = new Dictionary<string, AssemblyInfo>(StringComparer.Ordinal);
            var parsed = new List<(AssemblyInfo info, Definition definition)>();
            foreach (var path in Directory.GetFiles(root, "*.asmdef", SearchOption.AllDirectories))
            {
                try
                {
                    var definition = JsonUtility.FromJson<Definition>(File.ReadAllText(path));
                    if (definition == null || string.IsNullOrWhiteSpace(definition.name))
                    { errors.Add("asmdef has no assembly name: " + path); continue; }
                    var info = new AssemblyInfo { Name = definition.name, Path = path,
                        EditorOnly = definition.includePlatforms != null && definition.includePlatforms.Length == 1 && definition.includePlatforms[0] == "Editor" };
                    if (!result.TryAdd(info.Name, info)) errors.Add("duplicate first-party assembly name: " + info.Name);
                    else parsed.Add((info, definition));
                }
                catch (Exception error) { errors.Add($"cannot read asmdef {path}: {error.Message}"); }
            }

            foreach (var (info, definition) in parsed)
            foreach (var reference in definition.references ?? Array.Empty<string>())
            {
                var resolved = ResolveReference(reference, result, root);
                if (resolved != null) info.References.Add(resolved);
                else if (!ExternalAssemblyExists(reference))
                    errors.Add($"{definition.name} has unresolved assembly reference '{reference}'.");
            }

            // Resolve asmrefs only after all definition names and GUIDs are available.
            foreach (var path in Directory.GetFiles(root, "*.asmref", SearchOption.AllDirectories))
            {
                try
                {
                    var reference = JsonUtility.FromJson<Reference>(File.ReadAllText(path));
                    var target = ResolveReference(reference?.reference, result, root);
                    if (target == null) errors.Add($"asmref has invalid reference target: {path}");
                }
                catch (Exception error) { errors.Add($"cannot read asmref {path}: {error.Message}"); }
            }
            return result;
        }

        private static string ResolveReference(string reference, Dictionary<string, AssemblyInfo> known, string root)
        {
            if (string.IsNullOrWhiteSpace(reference)) return null;
            if (known.ContainsKey(reference)) return reference;
            if (!reference.StartsWith("GUID:", StringComparison.Ordinal)) return null;
            string guid = reference.Substring("GUID:".Length);
            foreach (var path in Directory.GetFiles(root, "*.asmdef", SearchOption.AllDirectories))
            {
                string meta = path + ".meta";
                if (!File.Exists(meta) || !Regex.IsMatch(File.ReadAllText(meta), @"\bguid:\s*" + Regex.Escape(guid) + @"\b")) continue;
                var definition = JsonUtility.FromJson<Definition>(File.ReadAllText(path));
                if (definition != null && known.ContainsKey(definition.name)) return definition.name;
            }
            return null;
        }

        private static bool ExternalAssemblyExists(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference)) return false;
            string packageRoot = Path.GetFullPath("Library/PackageCache");
            string packageAssets = Path.GetFullPath("Packages");
            var roots = new[] { packageRoot, packageAssets }.Where(Directory.Exists);
            bool byGuid = reference.StartsWith("GUID:", StringComparison.Ordinal);
            string guid = byGuid ? reference.Substring("GUID:".Length) : null;
            foreach (var searchRoot in roots)
            foreach (var path in Directory.GetFiles(searchRoot, "*.asmdef", SearchOption.AllDirectories))
            {
                try
                {
                    var definition = JsonUtility.FromJson<Definition>(File.ReadAllText(path));
                    if (definition == null) continue;
                    if (!byGuid && definition.name == reference) return true;
                    string meta = path + ".meta";
                    if (byGuid && File.Exists(meta) && Regex.IsMatch(File.ReadAllText(meta), @"\bguid:\s*" + Regex.Escape(guid) + @"\b")) return true;
                }
                catch { /* Ignore malformed package-owned definitions. */ }
            }
            return false;
        }

        private static void ValidateDefinitions(Dictionary<string, AssemblyInfo> definitions, List<string> errors)
        {
            foreach (var name in definitions.Keys)
                if (name != "DiceFree.Editor" && !Allowed.ContainsKey(name))
                    errors.Add("unclassified first-party assembly: " + name);
            foreach (var pair in Allowed)
            {
                if (!definitions.TryGetValue(pair.Key, out var actual))
                { errors.Add("required assembly is missing: " + pair.Key); continue; }
                foreach (var reference in actual.References)
                    if (!pair.Value.Contains(reference, StringComparer.Ordinal))
                        errors.Add($"forbidden dependency: {pair.Key} -> {reference}");
                if (pair.Key != "DiceFree.Application.Runtime" && actual.References.Any(value => value == "DiceFree.Application.Runtime"))
                    errors.Add($"domain/runtime leaf {pair.Key} references Application.");
                if (actual.References.Any(value => value == "DiceFree.Editor"))
                    errors.Add($"runtime assembly {pair.Key} references Editor.");
                if (pair.Key != "DiceFree.Persistence.Runtime" && pair.Key != "DiceFree.Application.Runtime" && pair.Key != "DiceFree.UI.Runtime" &&
                    actual.References.Any(value => value == "DiceFree.Persistence.Runtime"))
                    errors.Add($"domain assembly {pair.Key} references concrete Persistence.");
                if (pair.Key != "DiceFree.Foundation.Runtime" && actual.References.Any(value => value == "DiceFree.UI.Runtime"))
                    errors.Add($"runtime/domain assembly {pair.Key} references UI.");
                if (actual.References.Any(value => value.StartsWith("UnityEditor", StringComparison.Ordinal)))
                    errors.Add($"runtime assembly {pair.Key} references UnityEditor.");
            }
            if (!definitions.TryGetValue("DiceFree.Editor", out var editor) || !editor.EditorOnly)
                errors.Add("DiceFree.Editor must have Editor as its only included platform.");
            else
            {
                var editorAllowed = new HashSet<string>(Allowed.Keys, StringComparer.Ordinal);
                foreach (var reference in editor.References)
                    if (!editorAllowed.Contains(reference)) errors.Add("DiceFree.Editor has a forbidden first-party dependency: " + reference);
            }

            var colors = new Dictionary<string, int>(StringComparer.Ordinal);
            var stack = new List<string>();
            foreach (var name in definitions.Keys) Visit(name, definitions, colors, stack, errors);
        }

        private static void Visit(string name, Dictionary<string, AssemblyInfo> definitions,
            Dictionary<string, int> colors, List<string> stack, List<string> errors)
        {
            if (colors.TryGetValue(name, out int color))
            {
                if (color == 1) errors.Add("first-party assembly cycle: " + string.Join(" -> ", stack) + " -> " + name);
                return;
            }
            colors[name] = 1; stack.Add(name);
            foreach (var dependency in definitions[name].References)
                if (definitions.ContainsKey(dependency)) Visit(dependency, definitions, colors, stack, errors);
            stack.RemoveAt(stack.Count - 1); colors[name] = 2;
        }

        private static void ValidateOwnership(string root, Dictionary<string, AssemblyInfo> definitions, List<string> errors)
        {
            foreach (var source in Directory.GetFiles("Assets/_DiceFree", "*.cs", SearchOption.AllDirectories))
            {
                string owner = FindOwner(Path.GetDirectoryName(Path.GetFullPath(source)), root, definitions);
                string relative = source.Replace('\\', '/');
                bool editorPath = relative.Contains("/Tools/Editor/");
                if (owner == null) { errors.Add("first-party C# has no asmdef/asmref owner: " + relative); continue; }
                bool editorOwner = definitions[owner].EditorOnly;
                if (editorPath != editorOwner) errors.Add($"editor/runtime ownership mismatch: {relative} -> {owner}");
                if (!editorPath && Regex.IsMatch(File.ReadAllText(source), @"\bUnityEditor(?:\.|\s)"))
                    errors.Add("runtime source references UnityEditor: " + relative);
            }
        }

        private static string FindOwner(string directory, string root, Dictionary<string, AssemblyInfo> definitions)
        {
            while (directory != null && directory.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                var assemblyFile = Directory.GetFiles(directory, "*.asmdef", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (assemblyFile != null)
                {
                    var definition = JsonUtility.FromJson<Definition>(File.ReadAllText(assemblyFile));
                    return definition != null && definitions.ContainsKey(definition.name) ? definition.name : null;
                }
                var referenceFile = Directory.GetFiles(directory, "*.asmref", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (referenceFile != null)
                {
                    var reference = JsonUtility.FromJson<Reference>(File.ReadAllText(referenceFile));
                    if (reference == null) return null;
                    if (definitions.ContainsKey(reference.reference)) return reference.reference;
                    return ResolveReference(reference.reference, definitions, root);
                }
                directory = Directory.GetParent(directory)?.FullName;
            }
            return null;
        }
    }
}
