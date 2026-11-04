using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceFree.Quests
{
    public static class QuestHonorifics
    {
        static QuestDefinition[] catalog;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => catalog=null;
        public static string Resolve(IReadOnlyList<QuestProgress> progress,string fallback)
        {
            catalog ??= Resources.LoadAll<QuestDefinition>("HonorificQuests").OrderBy(q=>q.stableId,System.StringComparer.Ordinal).ToArray();
            if(progress!=null)foreach(var quest in catalog)
            {
                if(string.IsNullOrWhiteSpace(quest.rewardTitle))continue;
                foreach(var state in progress)
                    if(state!=null&&state.questId==quest.stableId&&state.definitionVersion==quest.version&&state.status==QuestStatus.Completed)return quest.rewardTitle;
            }
            return fallback;
        }
    }
}
