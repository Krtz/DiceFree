using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.World
{
    public static class AshfallReach
    {
        public static readonly Vector3 Origin = new(64000, 0, 64000);
        public static readonly Rect LocalBounds = Rect.MinMaxRect(-230, -155, 60, 35);
        public static readonly Rect Bounds = new(LocalBounds.position + new Vector2(Origin.x, Origin.z), LocalBounds.size);
        public static readonly Vector2 NorthGate = new(0, 25);
        public static readonly Vector2 Cinderwatch = new(5, -47);
        public static readonly Vector2 BasaltNeedle = new(-29, -4);
        public static readonly Vector2 EmberCrown = new(31, -64);
        public static readonly Vector2 Willowford = new(-150, -62);
        public const float WorldOneGateX = 163;
        public const float WorldOneRiverEdgeZ = -53;
        public const float NorthEdgeZ = 35;
        public const float RiverWaterHeight = -.386f;

        public static float RiverX(float z)
        {
            float sourceZ = WorldOneRiverEdgeZ - (NorthEdgeZ - z);
            return -18 + Mathf.Sin(sourceZ * .065f) * 2 - WorldOneGateX;
        }

        public static float RiverHalfWidth(float z) =>
            Mathf.Lerp(2.30f, 4.0f, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(35, -95, z)));

        public static float RiverlandsWeight(float x) =>
            Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-38, -70, x));

        public static string RegionName(Vector3 position) =>
            position.x - Origin.x < -60 ? "Steamroot Vale" : "Ashfall Reach";

        public static IReadOnlyList<Vector2> River { get; } = RiverPoints();
        private static Vector2[] RiverPoints()
        {
            var points = new Vector2[39];
            for (int i = 0; i < points.Length; i++)
            {
                float z = NorthEdgeZ - i * 5;
                points[i] = new Vector2(RiverX(z), z);
            }
            return points;
        }

        public static IReadOnlyList<Vector2> WillowfordRoad { get; } = new Vector2[]
        {
            Cinderwatch, new(3, -40.5f), new(-5, -40.5f), new(-12, -40.5f),
            new(-22, -40.5f), new(-36, -41), new(-48, -45), new(-61, -50),
            new(-90, -55), new(-125, -61), Willowford, new(-169, -62)
        };

        public static IReadOnlyList<Vector2> RiverbankTrail { get; } = new Vector2[]
        {
            new(-169, -62), new(-194, -62), new(-209, -64), new(-216, -47),
            new(-211, -26), new(-209, -10), new(-207, 8)
        };

        public static IReadOnlyList<Vector2> ArrivalTrail { get; } = new Vector2[]
        {
            NorthGate, new(1, 19), new(-1.5f, 11), new(-5, 5), new(-5, -3),
            new(-2, -12), new(3, -19), new(1, -25)
        };

        public static IReadOnlyList<Vector2> SettlementRoad { get; } = new Vector2[]
        {
            new(1, -25), new(2, -29), new(4, -34), new(6, -39), Cinderwatch, new(4, -51)
        };

        public static IReadOnlyList<Vector2> NeedlePath { get; } = new Vector2[]
        {
            new(-5, -3), new(-10, -5), new(-15, -5), new(-20, -3), new(-23, -3)
        };

        public static IReadOnlyList<Vector2> CrownPath { get; } = new Vector2[]
        {
            new(4, -51), new(5, -58), new(5, -65), new(12, -68), new(19, -68), new(19, -65)
        };
    }
}
