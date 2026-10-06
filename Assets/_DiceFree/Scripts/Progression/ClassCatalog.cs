using System;
using System.Collections.Generic;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Progression
{
    [CreateAssetMenu(menuName = "DiceFree/Progression/Class catalog")]
    public sealed class ClassCatalog : ScriptableObject
    {
        [SerializeField] private ActorDefinition[] classes = Array.Empty<ActorDefinition>();

        public IReadOnlyList<ActorDefinition> Classes => classes;

        public ActorDefinition Resolve(string stableId)
        {
            if (string.IsNullOrWhiteSpace(stableId)) return null;
            foreach (var definition in classes)
                if (definition != null && definition.stableId == stableId) return definition;
            return null;
        }

        public void Configure(params ActorDefinition[] values)
        {
            classes = values == null ? Array.Empty<ActorDefinition>() : (ActorDefinition[])values.Clone();
            Validate();
        }

        public void Validate()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in classes)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.stableId))
                    throw new InvalidOperationException("Class catalog entries require stable actor definitions.");
                if (!ids.Add(definition.stableId))
                    throw new InvalidOperationException("Duplicate class catalog entry: " + definition.stableId);
            }
        }
    }
}
