using System;

namespace DiceFree.Skills
{
    public interface IClassSkillState
    {
        string ClassId { get; }
        event Action Changed;
        bool CanRestore(SkillRankState[] state, int level);
        void RestoreState(SkillRankState[] state);
        SkillRankState[] CaptureState();
        void SetClassActive(bool active);
    }
}
