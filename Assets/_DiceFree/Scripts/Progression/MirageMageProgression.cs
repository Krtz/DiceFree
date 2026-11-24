using UnityEngine;
namespace DiceFree.Skills
{
    [DisallowMultipleComponent, RequireComponent(typeof(DiceFree.Progression.ExperienceProgression))]
    public sealed class MirageMageProgression : PsionEvolutionProgression
    {
        public override string ClassId => MageId;
    }
}
