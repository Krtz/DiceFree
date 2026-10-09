using DiceFree.Combat;
using DiceFree.Gameplay;
using UnityEngine;

namespace DiceFree.Dungeons
{
    [DisallowMultipleComponent,RequireComponent(typeof(KillCreditReceiver),typeof(CodexProgression))]
    public sealed class OverworldCodexCredit : MonoBehaviour
    {
        KillCreditReceiver credit;CodexProgression codex;
        void Awake(){credit=GetComponent<KillCreditReceiver>();codex=GetComponent<CodexProgression>();}
        void OnEnable()=>credit.Credited+=Record;
        void OnDisable()=>credit.Credited-=Record;
        void Record(ActorDefeated defeat)
        {
            // KillCreditReceiver deduplicates authority sequences. Dungeon run owns its own events.
            if(defeat.victim!=null&&defeat.victim.GetComponent<DungeonSlime>()==null&&!string.IsNullOrWhiteSpace(defeat.contentId))codex.RecordMonsterKill(defeat.contentId);
        }
    }
}
