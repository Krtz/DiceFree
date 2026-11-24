using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using UnityEngine;
namespace DiceFree.Skills
{
    internal static class PsionSpeedPresence
    {
        private static readonly List<PsionEvolutionCaster> sources=new();
        private static readonly Dictionary<CombatActor,float> applied=new();
        private static float next;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset(){sources.Clear();applied.Clear();next=0;}
        public static void Register(PsionEvolutionCaster c){if(!sources.Contains(c))sources.Add(c);next=0;}
        public static void Unregister(PsionEvolutionCaster c){sources.Remove(c);next=0;Tick();}
        public static void Invalidate(CombatActor actor)
        {
            applied.Remove(actor);next=0;
            actor.Stats.RemoveAttackSpeedPercentModifier("psion3.speed-presence");
            actor.Stats.RemoveMovementSpeedPercentModifier("psion3.speed-presence");
        }
        public static void Tick()
        {
            if(Time.time<next)return;next=Time.time+.15f;
            foreach(var recipient in CombatActor.All.ToArray())
            {
                float value=0;
                foreach(var source in sources)if(source!=null&&source.Active&&source.Actor.Alive&&source.Actor.IsFriendlyTo(recipient)&&(source.transform.position-recipient.transform.position).sqrMagnitude<=144)
                    value=Mathf.Max(value,source.Progression.Rank(4)*.1f);
                if(applied.TryGetValue(recipient,out float old)&&Mathf.Approximately(value,old))continue;
                if(value>0){recipient.Stats.SetAttackSpeedPercentModifier("psion3.speed-presence",value);recipient.Stats.SetMovementSpeedPercentModifier("psion3.speed-presence",value);applied[recipient]=value;}
                else {recipient.Stats.RemoveAttackSpeedPercentModifier("psion3.speed-presence");recipient.Stats.RemoveMovementSpeedPercentModifier("psion3.speed-presence");applied.Remove(recipient);}
            }
            foreach(var gone in applied.Keys.Where(a=>a==null||!a.isActiveAndEnabled).ToArray())
            {if(gone!=null){gone.Stats.RemoveAttackSpeedPercentModifier("psion3.speed-presence");gone.Stats.RemoveMovementSpeedPercentModifier("psion3.speed-presence");}applied.Remove(gone);}
        }
    }
}
