using UnityEngine;
namespace DiceFree.Skills
{
    [DisallowMultipleComponent, RequireComponent(typeof(DiceFree.Progression.ExperienceProgression))]
    public sealed class PsionicKnightProgression : PsionEvolutionProgression
    {
        public override string ClassId => KnightId;
    }
}
