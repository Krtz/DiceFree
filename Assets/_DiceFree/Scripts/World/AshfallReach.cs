using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.World
{
    public static class AshfallReach
    {
        public static readonly Vector3 Origin = new(64000, 0, 64000);
        public static readonly Rect LocalBounds = Rect.MinMaxRect(-43, -81, 48, 35);
        public static readonly Rect Bounds = new(LocalBounds.position + new Vector2(Origin.x, Origin.z), LocalBounds.size);
        public static readonly Vector2 NorthGate = new(0, 25);
        public static readonly Vector2 Cinderwatch = new(5, -47);
        public static readonly Vector2 BasaltNeedle = new(-29, -4);
        public static readonly Vector2 EmberCrown = new(31, -64);

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
            new(4, -51), new(9, -55), new(15, -58), new(19, -62), new(22, -65)
        };
    }
}
