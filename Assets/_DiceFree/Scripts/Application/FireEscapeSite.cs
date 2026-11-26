using System;
using UnityEngine;
namespace DiceFree.Advancement
{
    public sealed class FireEscapeSite:MonoBehaviour
    {
        public const string SceneName="EmberCrownTrial",TrialId="trial.fire-mage.ember-walk";
        public Vector3 entrance,finish;
        public Rect mapBounds;
        public Rect[] safeGround;
        [Serializable] public sealed class Flame
        {
            public Rect bounds;
            public float offset;
            public Renderer visual;
        }
        public Flame[] flames;
        public bool IsLava(Vector3 point)=>!Array.Exists(safeGround,r=>r.Contains(new Vector2(point.x,point.z)));
        // 2s warning, 2s active, 2s safe; deterministic staggered cycles.
        public static int FlamePhase(float elapsed,float offset){float t=Mathf.Repeat(elapsed+offset,6);return t<2?0:t<4?1:2;}
    }
}
