using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.World
{
    public static class AshfallReach
    {
        public static readonly Vector3 Origin = new(64000, 0, 64000);
        // Matches Cornberg's authored OverworldMapBounds, including Briarback.
        public const float FaceWidth = 438, FaceHeight = 615;
        public static readonly Rect LocalBounds = new(-248, 35-FaceHeight, FaceWidth, FaceHeight);
        public static readonly Rect Bounds = new(LocalBounds.position + new Vector2(Origin.x, Origin.z), LocalBounds.size);
        public static readonly Vector2 NorthGate = new(0, 25);
        public static readonly Vector2 Cinderwatch = new(5, -47);
        public static readonly Vector2 BasaltNeedle = new(LocalBounds.xMin+FaceWidth*.25f, LocalBounds.yMin+FaceHeight*.75f);
        public static readonly Vector2 EmberCrown = new(LocalBounds.xMin+FaceWidth*.75f, LocalBounds.yMin+FaceHeight*.25f);
        public static readonly Vector2 Willowford = new(-150, -62);
        public static readonly Vector2 Anvilreach = new(92, -205);
        public static readonly Rect AnvilreachBounds = new(30,-282,140,146);
        public static readonly Vector2 Coppervein = new(105,-120), Glasswind = new(126,-520), Deepmoss = new(-111,-444);
        public static float CopperveinWeight(Vector2 p) => Mathf.SmoothStep(0,1,Mathf.InverseLerp(-15,40,p.x))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(-78,-120,p.y))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(-345,-300,p.y));
        public static float GlasswindWeight(Vector2 p) => Mathf.SmoothStep(0,1,Mathf.InverseLerp(-55,-5,p.x))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(-330,-380,p.y));
        public static float DeepmossWeight(Vector2 p) => Mathf.SmoothStep(0,1,Mathf.InverseLerp(-40,-90,p.x))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(-325,-375,p.y));
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

        public static string RegionName(Vector3 position)
        {
            var p=new Vector2(position.x-Origin.x,position.z-Origin.z);
            if(AnvilreachBounds.Contains(p))return "Anvilreach";
            if(DeepmossWeight(p)>.5f)return "Deepmoss Fen";
            if(GlasswindWeight(p)>.5f)return "Glasswind Barrens";
            if(CopperveinWeight(p)>.5f)return "Coppervein Highlands";
            return p.x < -60 ? "Steamroot Vale" : "Ashfall Reach";
        }
        public static IReadOnlyList<Vector2> AnvilreachRoad {get;}=new Vector2[]{new(32,-195),new(26,-205),new(42,-205),Anvilreach,new(92,-244),new(92,-290),new(43,-286)};
        public static IReadOnlyList<Vector2> AnvilreachNorthRoad {get;}=new Vector2[]{new(13,-115),new(40,-112),new(72,-120),new(92,-128),new(92,-165),Anvilreach};

        public static IReadOnlyList<Vector2> River { get; } = RiverPoints();
        private static Vector2[] RiverPoints()
        {
            var points = new Vector2[Mathf.CeilToInt(FaceHeight/5)+1];
            for (int i = 0; i < points.Length; i++)
            {
                float z = Mathf.Max(LocalBounds.yMin,NorthEdgeZ - i * 5);
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
            new(-61,-50), new(-78,-61), new(-95,-78), new(-112,-91), new(-112,-112),
            BasaltNeedle+new Vector2(24,4)
        };

        public static IReadOnlyList<Vector2> CrownPath { get; } = new Vector2[]
        {
            new(4,-51),new(7,-80),new(13,-115),new(22,-152),new(32,-195),
            new(39,-239),new(43,-286),new(52,-333),new(65,-374),
            EmberCrown+new Vector2(-28,48)
        };

        public static IReadOnlyList<Vector2> SouthernCircuit { get; } = new Vector2[]
        {
            BasaltNeedle+new Vector2(24,4),new(-105,-149),new(-94,-193),new(-79,-237),
            new(-59,-285),new(-37,-324),new(-11,-352),new(23,-374),
            EmberCrown+new Vector2(-28,48),new(24,-424),new(-12,-454),new(-48,-480),
            new(-85,-506),new(-124,-526),new(-152,-534),new(-154,-487),new(-147,-438),
            new(-143,-387),new(-147,-333),new(-145,-278),new(-149,-222),new(-151,-168),new(-164,-140),
            new(-170,-116),new(-158,-94),new(-139,-87),new(-115,-96),BasaltNeedle+new Vector2(24,4)
        };
        public static IReadOnlyList<Vector2> WesternTrail { get; } = new Vector2[]
        {
            new(-209,-64),new(-216,-100),new(-218,-147),new(-220,-200),new(-216,-255),
            new(-215,-300),new(-196,-318),new(-169,-318),new(-147,-333)
        };
    }
}
