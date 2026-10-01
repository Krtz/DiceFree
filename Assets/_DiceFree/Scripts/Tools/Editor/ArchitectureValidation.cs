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
            public readonly List<string> DeclaredReferences = new();
            public readonly List<string> References = new();
        }

        [MenuItem("DiceFree/Validation/Architecture boundaries")]
        public static void Run()
        {
            AssemblyDependencyPolicy.ValidateSelfTests();
            var errors = new List<string>();
            string scriptsRoot = Path.GetFullPath("Assets/_DiceFree/Scripts");
            var definitions = LoadDefinitions(scriptsRoot, errors);
            ValidateDefinitions(definitions, errors);
            ManagedReferenceValidation.Validate(errors);
            ValidateOwnership(scriptsRoot, definitions, errors);
            if (errors.Count > 0)
                throw new InvalidOperationException("DiceFree architecture validation failed:\n - " + string.Join("\n - ", errors));
            Debug.Log($"DICEFRE_ARCHITECTURE_OK: {definitions.Count} explicit first-party assemblies; all first-party C# owned; dependency graph acyclic and within policy.");
        }

        [MenuItem("DiceFree/Validation/Architecture policy self-tests")]
        public static void RunPolicySelfTests()
        {
            AssemblyDependencyPolicy.ValidateSelfTests();
            Debug.Log("DICEFRE_ARCH_POLICY_SELFTEST_OK: allowed/forbidden edges, cycles, ownership and asmref targets.");
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
                info.DeclaredReferences.Add(reference);
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
                    if (!AssemblyDependencyPolicy.IsKnownAssemblyReference(target, new HashSet<string>(result.Keys, StringComparer.Ordinal)))
                        errors.Add($"asmref has invalid reference target: {path}");
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
                if (!AssemblyDependencyPolicy.Allowed.ContainsKey(name))
                    errors.Add("unclassified first-party assembly: " + name);
            foreach (var pair in AssemblyDependencyPolicy.Allowed)
            {
                if (!definitions.TryGetValue(pair.Key, out var actual))
                { errors.Add("required assembly is missing: " + pair.Key); continue; }
                foreach (var reference in actual.References)
                    if (!AssemblyDependencyPolicy.DependencyAllowed(pair.Key, reference))
                        errors.Add($"forbidden dependency: {pair.Key} -> {reference}");
                if (!actual.EditorOnly && actual.DeclaredReferences.Any(value => value.StartsWith("UnityEditor", StringComparison.Ordinal)))
                    errors.Add($"runtime assembly {pair.Key} references UnityEditor.");
            }
            if (!definitions.TryGetValue("DiceFree.Editor", out var editor) || !editor.EditorOnly)
                errors.Add("DiceFree.Editor must have Editor as its only included platform.");

            var graph = definitions.ToDictionary(pair => pair.Key, pair => pair.Value.References, StringComparer.Ordinal);
            if (AssemblyDependencyPolicy.HasCycle(graph)) errors.Add("first-party assembly dependency graph contains a cycle.");
        }

        private static void ValidateOwnership(string root, Dictionary<string, AssemblyInfo> definitions, List<string> errors)
        {
            var knownOwners = new HashSet<string>(definitions.Keys, StringComparer.Ordinal);
            foreach (var source in Directory.GetFiles("Assets/_DiceFree", "*.cs", SearchOption.AllDirectories))
            {
                string owner = FindOwner(Path.GetDirectoryName(Path.GetFullPath(source)), root, definitions);
                string relative = source.Replace('\\', '/');
                bool editorPath = relative.Contains("/Tools/Editor/");
                if (!AssemblyDependencyPolicy.HasAssemblyOwner(owner, knownOwners)) { errors.Add("first-party C# has no asmdef/asmref owner: " + relative); continue; }
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
