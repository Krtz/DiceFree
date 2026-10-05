using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceFree.EditorTools
{
    /// <summary>Pure first-party assembly policy, shared by project validation and policy self-tests.</summary>
    internal static class AssemblyDependencyPolicy
    {
        internal static readonly IReadOnlyDictionary<string, string[]> Allowed =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["DiceFree.Foundation.Runtime"] = Array.Empty<string>(),
                ["DiceFree.Input.Runtime"] = Array.Empty<string>(),
                ["DiceFree.Gameplay.Runtime"] = new[] { "DiceFree.Foundation.Runtime" },
                ["DiceFree.Items.Runtime"] = new[] { "DiceFree.Foundation.Runtime", "DiceFree.Gameplay.Runtime" },
                ["DiceFree.World.Runtime"] = new[] { "DiceFree.Foundation.Runtime", "DiceFree.Gameplay.Runtime" },
                ["DiceFree.Quests.Runtime"] = new[] { "DiceFree.Foundation.Runtime", "DiceFree.Gameplay.Runtime", "DiceFree.Items.Runtime", "DiceFree.World.Runtime" },
                ["DiceFree.AI.Runtime"] = new[] { "DiceFree.Foundation.Runtime", "DiceFree.Gameplay.Runtime", "DiceFree.World.Runtime" },
                ["DiceFree.Persistence.Runtime"] = new[] { "DiceFree.Foundation.Runtime", "DiceFree.Gameplay.Runtime", "DiceFree.Items.Runtime", "DiceFree.World.Runtime", "DiceFree.Quests.Runtime" },
                ["DiceFree.UI.Runtime"] = new[] { "DiceFree.Input.Runtime", "DiceFree.Foundation.Runtime", "DiceFree.Gameplay.Runtime", "DiceFree.Items.Runtime", "DiceFree.World.Runtime", "DiceFree.Quests.Runtime", "DiceFree.AI.Runtime" },
                ["DiceFree.Application.Runtime"] = new[] { "DiceFree.Input.Runtime", "DiceFree.Foundation.Runtime", "DiceFree.Gameplay.Runtime", "DiceFree.Items.Runtime", "DiceFree.World.Runtime", "DiceFree.Quests.Runtime", "DiceFree.AI.Runtime", "DiceFree.Persistence.Runtime", "DiceFree.UI.Runtime" },
                ["DiceFree.Editor"] = new[] { "DiceFree.Input.Runtime", "DiceFree.Foundation.Runtime", "DiceFree.Gameplay.Runtime", "DiceFree.Items.Runtime", "DiceFree.World.Runtime", "DiceFree.Quests.Runtime", "DiceFree.AI.Runtime", "DiceFree.Persistence.Runtime", "DiceFree.UI.Runtime", "DiceFree.Application.Runtime" }
            };

        internal static bool DependencyAllowed(string source, string target) =>
            Allowed.TryGetValue(source, out var references) && references.Contains(target, StringComparer.Ordinal);

        internal static bool HasAssemblyOwner(string owner, ISet<string> knownAssemblies) =>
            !string.IsNullOrWhiteSpace(owner) && knownAssemblies.Contains(owner);

        internal static bool IsKnownAssemblyReference(string target, ISet<string> assemblyNames) =>
            !string.IsNullOrWhiteSpace(target) && assemblyNames.Contains(target);

        internal static bool HasCycle(IDictionary<string, List<string>> graph)
        {
            var colors = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var name in graph.Keys)
                if (Visit(name, graph, colors)) return true;
            return false;
        }

        internal static void ValidateSelfTests()
        {
            RequireAllowed("DiceFree.Application.Runtime", "DiceFree.UI.Runtime");
            RequireAllowed("DiceFree.Application.Runtime", "DiceFree.Persistence.Runtime");
            RequireAllowed("DiceFree.Quests.Runtime", "DiceFree.Items.Runtime");
            RequireAllowed("DiceFree.Quests.Runtime", "DiceFree.World.Runtime");
            foreach (var domain in Allowed["DiceFree.Persistence.Runtime"])
                RequireAllowed("DiceFree.Persistence.Runtime", domain);
            foreach (var runtime in Allowed.Keys.Where(name => name != "DiceFree.Editor"))
                RequireAllowed("DiceFree.Editor", runtime);

            RequireAllowed("DiceFree.UI.Runtime", "DiceFree.Input.Runtime");
            RequireAllowed("DiceFree.Application.Runtime", "DiceFree.Input.Runtime");
            RequireForbidden("DiceFree.Gameplay.Runtime", "DiceFree.Input.Runtime");
            RequireForbidden("DiceFree.Input.Runtime", "DiceFree.UI.Runtime");
            RequireForbidden("DiceFree.Gameplay.Runtime", "DiceFree.UI.Runtime");
            RequireForbidden("DiceFree.Gameplay.Runtime", "DiceFree.Persistence.Runtime");
            RequireForbidden("DiceFree.Items.Runtime", "DiceFree.Persistence.Runtime");
            RequireForbidden("DiceFree.World.Runtime", "DiceFree.UI.Runtime");
            RequireForbidden("DiceFree.Quests.Runtime", "DiceFree.Application.Runtime");
            foreach (var runtime in Allowed.Keys.Where(name => name != "DiceFree.Editor"))
                RequireForbidden(runtime, "DiceFree.Editor");

            var cycle = new Dictionary<string, List<string>>(StringComparer.Ordinal)
            { ["A"] = new() { "B" }, ["B"] = new() { "A" } };
            if (!HasCycle(cycle)) throw new InvalidOperationException("Policy self-test failed to reject A -> B -> A.");
            var known = new HashSet<string>(Allowed.Keys, StringComparer.Ordinal);
            if (HasAssemblyOwner(null, known) || HasAssemblyOwner("DiceFree.Missing", known))
                throw new InvalidOperationException("Policy self-test failed to reject unowned first-party source.");
            if (!HasAssemblyOwner("DiceFree.Gameplay.Runtime", known))
                throw new InvalidOperationException("Policy self-test failed to accept a known assembly owner.");
            if (IsKnownAssemblyReference("DiceFree.Missing", known))
                throw new InvalidOperationException("Policy self-test failed to reject invalid asmref target.");
        }

        private static void RequireAllowed(string source, string target)
        {
            if (!DependencyAllowed(source, target))
                throw new InvalidOperationException($"Policy self-test expected allowed edge {source} -> {target}.");
        }

        private static void RequireForbidden(string source, string target)
        {
            if (DependencyAllowed(source, target))
                throw new InvalidOperationException($"Policy self-test expected forbidden edge {source} -> {target}.");
        }

        private static bool Visit(string name, IDictionary<string, List<string>> graph, Dictionary<string, int> colors)
        {
            if (colors.TryGetValue(name, out int color)) return color == 1;
            colors[name] = 1;
            foreach (var dependency in graph[name])
                if (graph.ContainsKey(dependency) && Visit(dependency, graph, colors)) return true;
            colors[name] = 2;
            return false;
        }
    }
}
