using System.Linq;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Dungeons
{
    [DefaultExecutionOrder(80),DisallowMultipleComponent]
    public sealed class SlimeVisualMotion : MonoBehaviour
    {
        Transform visual;Vector3 scale,position;BasicAttack attack;DungeonSlime dungeon;CombatActor actor;
        string previous;float impactUntil;
        void Start()
        {
            attack=GetComponent<BasicAttack>();dungeon=GetComponent<DungeonSlime>();actor=GetComponent<CombatActor>();
            visual=transform.Find("Presentation")??transform.Cast<Transform>().FirstOrDefault(t=>t.name.StartsWith("Slimes/"))??transform.Find("Slime body");
            if(visual!=null){scale=visual.localScale;position=visual.localPosition;}
        }
        void LateUpdate()
        {
            if(visual==null||attack==null||actor!=null&&!actor.Alive||dungeon!=null&&dungeon.Busy)return;
            if(attack.State!=previous&&previous=="Wind-up"&&(attack.State=="Recovery"||attack.State=="Miss"))impactUntil=Time.time+.3f;
            previous=attack.State;
            float pulse=Mathf.Clamp01((impactUntil-Time.time)/.3f);
            bool winding=attack.State=="Wind-up";
            var factor=winding?new Vector3(1.18f,.65f,1.18f):new Vector3(1-.12f*pulse,1+.4f*pulse,1-.12f*pulse);
            visual.localScale=Vector3.Scale(scale,factor);
            visual.localPosition=position+Vector3.up*(pulse*.35f);
        }
    }
}
