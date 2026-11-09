using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Skills
{
    internal static class PsionStormVisual
    {
        public static void Play(Vector3 center, float radius, int wave)
        {
            Color glow = new(.45f, .75f, 1f);
            EarlySkillVfx.PlayPriestPulse("Lightning Storm wave", center, radius, glow);
            var material = Resources.Load<Material>("EarlySkillVfx/SoftRing");
            if (material == null) return;
            var root = new GameObject("Psion lightning wave");
            root.transform.position = center;
            for (int i=0; i<5; i++)
            {
                float angle = (i * 2.39996f) + wave * .79f;
                float distance = radius * Mathf.Sqrt((i+.4f)/5);
                var end = new Vector3(Mathf.Cos(angle)*distance,.12f,Mathf.Sin(angle)*distance);
                var origin = end + new Vector3(.9f,4.7f+i*.32f,-.6f);
                var bolt = new GameObject("Forked lightning").AddComponent<LineRenderer>();
                bolt.transform.SetParent(root.transform,false);
                bolt.sharedMaterial = material;
                bolt.useWorldSpace = false;
                bolt.startColor = glow;
                bolt.endColor = Color.white;
                bolt.startWidth = .082f;
                bolt.endWidth = .043f;
                bolt.positionCount = 8;
                for (int step=0;step<8;step++)
                {
                    float t=step/7f;
                    var point=Vector3.Lerp(origin,end,t);
                    point.x+=Mathf.Sin(angle+step*2.9f)*Mathf.Sin(t*Mathf.PI)*.39f;
                    point.z+=Mathf.Cos(angle+step*2.1f)*Mathf.Sin(t*Mathf.PI)*.35f;
                    bolt.SetPosition(step,point);
                }
            }
            Object.Destroy(root,.18f);
        }
    }
}