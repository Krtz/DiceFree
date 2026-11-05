using DiceFree.UI;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Visuals
{
    // Names the authored city on the existing map; it creates no progression gate.
    public sealed class ArboriumCityPresentation : MonoBehaviour
    {
        private WorldFogOfWar fog;
        private MinimapHud map;
        private string previousName;
        public bool Inside { get; private set; }
        private void Update()
        {
            if (fog == null) fog = FindAnyObjectByType<WorldFogOfWar>();
            if (map == null) map = FindAnyObjectByType<MinimapHud>();
            if (fog?.Revealer == null || map == null) return;
            var p = fog.Revealer.position - transform.position;
            bool inside = new Vector2(p.x, p.z).sqrMagnitude < 83 * 83 && Mathf.Abs(p.y) < 40;
            if (inside && !Inside) { previousName = map.MapName; map.MapName = "Arborium"; }
            if (!inside && Inside && map.MapName == "Arborium") map.MapName = previousName;
            Inside = inside;
        }
        private void OnDisable()
        {
            if (Inside && map != null && map.MapName == "Arborium") map.MapName = previousName;
            Inside = false;
        }
    }
}
