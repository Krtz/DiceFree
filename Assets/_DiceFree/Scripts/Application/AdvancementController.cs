using System;
using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.Advancement
{
    [DisallowMultipleComponent, RequireComponent(typeof(ActorStats), typeof(ManifestationPersistence))]
    public sealed class AdvancementController : MonoBehaviour
    {
        [SerializeField] private ClassCatalog classCatalog;
        [SerializeField] private AdvancementDefinition[] advancements = Array.Empty<AdvancementDefinition>();

        private ActorStats stats;
        private ManifestationPersistence persistence;

        public string Feedback { get; private set; } = "";
        public string CurrentClassId => persistence?.CurrentClassId ?? stats?.Definition?.stableId;
        public ActorDefinition CurrentClass => stats?.Definition;
        public IReadOnlyList<AdvancementDefinition> Definitions => advancements;
        public IReadOnlyList<ActorDefinition> Classes => classCatalog?.Classes ?? Array.Empty<ActorDefinition>();

        private void Awake()
        {
            stats = GetComponent<ActorStats>();
            persistence = GetComponent<ManifestationPersistence>();
            ValidateConfiguration();
        }

        public void Configure(ClassCatalog catalog, AdvancementDefinition[] definitions)
        {
            classCatalog = catalog;
            advancements = definitions == null ? Array.Empty<AdvancementDefinition>() : (AdvancementDefinition[])definitions.Clone();
            if (UnityEngine.Application.isPlaying) ValidateConfiguration();
        }

        public bool HasManifestation(string classId) => persistence != null && persistence.HasManifestation(classId);

        public bool CanAdvance(AdvancementDefinition definition) =>
            definition != null &&
            definition.Eligible(stats) &&
            definition.targetClass != null &&
            persistence != null &&
            persistence.Ready &&
            !persistence.HasManifestation(definition.targetClass.stableId);

        public bool TryAdvance(AdvancementDefinition definition)
        {
            if (!CanAdvance(definition))
                return SetFeedback("That advancement is not currently available.", false);

            if (!persistence.TryForkAndActivate(definition.targetClass, out string error))
                return SetFeedback(error, false);

            Feedback = "Advanced into " + definition.targetClass.displayName + ". Parent manifestation preserved.";
            return true;
        }

        public bool TrySwitch(string classId)
        {
            if (string.IsNullOrWhiteSpace(classId))
                return SetFeedback("Manifestation class ID is missing.", false);
            if (classId == CurrentClassId)
                return true;
            if (!persistence.TryActivateExisting(classId, out string error))
                return SetFeedback(error, false);

            Feedback = "Loaded " + stats.Definition.displayName + ".";
            return true;
        }

        private bool SetFeedback(string value, bool result)
        {
            Feedback = value ?? "";
            return result;
        }

        private void ValidateConfiguration()
        {
            classCatalog?.Validate();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in advancements)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.stableId) ||
                    definition.sourceClass == null || definition.targetClass == null ||
                    definition.requiredLevel < 1)
                    throw new InvalidOperationException("Advancement definitions require stable source/target data.");
                if (!ids.Add(definition.stableId))
                    throw new InvalidOperationException("Duplicate advancement definition: " + definition.stableId);
                if (classCatalog != null &&
                    (classCatalog.Resolve(definition.sourceClass.stableId) == null ||
                     classCatalog.Resolve(definition.targetClass.stableId) == null))
                    throw new InvalidOperationException("Advancement class is missing from the class catalog: " + definition.stableId);
            }
        }
    }
}
