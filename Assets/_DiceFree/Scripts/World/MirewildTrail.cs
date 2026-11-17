using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.World
{
    public static class MirewildTrail
    {
        public const string SurfaceId = "travel.road.mirewild-trail";
        public static IReadOnlyList<Vector2> Points { get; } = new[]
        {
            new Vector2(-33,223), new(-35,214), new(-33,205), new(-25,198),
            new(-14,192), new(0,188), new(18,184), new(34,177), new(47,162), new(55,150), new(65,144)
        };

        public static bool ContainsForestExtension(Vector2 position) =>
            position.y >= 196 && position.y < 208 &&
            position.x >= Mathf.Lerp(-54,-64,(position.y-196)/12) &&
            position.x <= Mathf.Lerp(3,-12,(position.y-196)/12);

        public static bool ContainsMireExtension(Vector2 position)
        {
            if (position.y < 151 || position.y >= 196) return false;
            for (int i = 4; i < Points.Count; i++)
                if (DistanceToSegment(position,Points[i-1],Points[i]) <= 16) return true;
            return false;
        }

        public static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            var edge = end - start;
            return Vector2.Distance(point,start + edge * Mathf.Clamp01(Vector2.Dot(point-start,edge)/edge.sqrMagnitude));
        }
    }
}
