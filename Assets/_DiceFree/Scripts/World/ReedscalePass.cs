using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.World
{
    public static class ReedscalePass
    {
        public const string SurfaceId = "travel.road.reedscale-pass";
        public static IReadOnlyList<Vector2> Points { get; } = new[]
        {
            new Vector2(31,406), new(39,404), new(49,405), new(61,410), new(77,420),
            new(96,429), new(116,432), new(136,437), new(154,437), new(174,431),
            new(190,421), new(207,418), new(231,413), new(250,407), new(265,401),
            new(267,396), new(264,391), new(257,383), new(250,378)
        };

        public static bool ContainsHighlands(Vector2 at)
        {
            if (at.x < 44 || at.x > 153 || at.y < 393 || at.y > 464) return false;
            for (int i = 2; i <= 8; i++)
                if (MirewildTrail.DistanceToSegment(at, Points[i-1], Points[i]) < Mathf.Lerp(10,29,(at.x-44)/70))
                    return true;
            return false;
        }
    }
}
