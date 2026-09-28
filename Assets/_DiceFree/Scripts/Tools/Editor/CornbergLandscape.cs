using System.Collections.Generic;
using UnityEngine;
using static DiceFree.EditorTools.BlockoutShapes;

namespace DiceFree.EditorTools
{
    internal static class CornbergLandscape
    {
        internal static readonly Vector2[] Road = { new(-45,-33), new(-32,-24), new(-17,-9), new(0,0), new(25,0), new(54,6), new(72,18), new(95,34), new(117,48) };
        internal static readonly Vector2[] NorthLoop = { new(63,12), new(64,35), new(76,47), new(91,46), new(95,34) };
        internal static readonly Vector2[] SouthLoop = { new(73,19), new(83,-3), new(103,1), new(109,26), new(95,34) };
        internal static float StreamX(float z) => -18f + Mathf.Sin(z * 0.065f) * 2f;
        internal static float Height(float x, float z)
        {
            var emergence = 3.5f * Mathf.Exp(-((x + 47) * (x + 47) + (z + 34) * (z + 34)) / 220f);
            var gentle = 0.22f * Mathf.Sin(x * 0.09f) * Mathf.Sin(z * 0.08f);
            var stream = Mathf.Clamp01(1f - Mathf.Abs(x - StreamX(z)) / 3.4f) * 1.8f;
            return emergence + gentle - stream;
        }
        internal static Vector3 Ground(float x, float z) => new(x, Height(x,z), z);

        internal static void Create(Transform parent)
        {
            var terrain = Group("01 - Valley ground and stream", parent);
            const int columns = 100, rows = 68;
            var vertices = new Vector3[(columns + 1) * (rows + 1)];
            var triangles = new List<int>();
            for (var z = 0; z <= rows; z++)
            for (var x = 0; x <= columns; x++) vertices[z * (columns + 1) + x] = Ground(-66 + x * 2, -54 + z * 2);
            for (var z = 0; z < rows; z++)
            for (var x = 0; x < columns; x++)
            {
                var i = z * (columns + 1) + x;
                triangles.AddRange(new[] { i, i + columns + 1, i + 1, i + 1, i + columns + 1, i + columns + 2 });
            }
            MeshObject("Cornberg valley", SaveMesh("Cornberg valley", vertices, triangles.ToArray()), Grass, terrain, true);
            for (var z = -52; z < 82; z += 2)
                Box("Stream channel", new Vector3(StreamX(z), -0.7f, z), new Vector3(4.4f, 0.6f, 2.4f), Water, terrain);
            Bridge(-9, terrain); Bridge(26, terrain);
            var paths = Group("02 - Arrival and connected forest paths", parent);
            Trail("Mountain to east road", Road, 4.6f, paths);
            Trail("North woodland loop", NorthLoop, 3.1f, paths);
            Trail("South woodland loop", SouthLoop, 3.1f, paths);
            Trail("Dungeon approach", new[] { new Vector2(76,47), new Vector2(80,56) }, 3.5f, paths);
            var mountain = Group("03 - Southwest emergence mountain", parent);
            for (int i = 0; i < 5; i++)
                Shape("Mountain ridge", PrimitiveType.Sphere, new Vector3(-62 + i * 6, 7 + i % 2 * 4, -47 - i % 2 * 4),
                    new Vector3(22, 27 + i % 2 * 12, 23), Stone, mountain);
            Box("Emergence recess", new Vector3(-49, 5, -37), new Vector3(4, 5, 0.5f), Dark, mountain);
            Shape("Emergence left rock", PrimitiveType.Sphere, new Vector3(-53, 5, -34), new Vector3(5, 9, 6), Stone, mountain);
            Shape("Emergence right rock", PrimitiveType.Sphere, new Vector3(-45, 5, -40), new Vector3(6, 9, 5), Stone, mountain);
            Label("Cornberg mountain", new Vector3(-45, 8, -36), mountain);
        }

        private static void Bridge(float z, Transform parent)
        {
            var x = StreamX(z);
            Box("Timber footbridge", new Vector3(x, 0.02f, z), new Vector3(9, 0.35f, 5), Timber, parent, true, 8);
            for (int side = -1; side <= 1; side += 2)
                Box("Bridge rail", new Vector3(x, 0.9f, z + side * 2.45f), new Vector3(9, 0.16f, 0.2f), Timber, parent);
        }

        internal static void Trail(string name, Vector2[] points, float width, Transform parent)
        {
            var verts = new List<Vector3>(); var indices = new List<int>();
            for (int i = 1; i < points.Length; i++)
            {
                var delta = points[i] - points[i - 1];
                var normal = new Vector2(-delta.y, delta.x).normalized * width * 0.5f;
                var count = Mathf.CeilToInt(delta.magnitude);
                for (int step = 0; step < count; step++)
                {
                    var a = Vector2.Lerp(points[i - 1], points[i], step / (float)count);
                    var b = Vector2.Lerp(points[i - 1], points[i], (step + 1f) / count);
                    var index = verts.Count;
                    foreach (var p in new[] { a - normal, a + normal, b - normal, b + normal })
                        verts.Add(Ground(p.x, p.y) + Vector3.up * 0.1f);
                    indices.AddRange(new[] { index, index + 1, index + 2, index + 2, index + 1, index + 3 });
                }
            }
            MeshObject(name, SaveMesh(name, verts.ToArray(), indices.ToArray()), Path, parent);
        }

        internal static void Forest(Transform parent)
        {
            var forest = Group("06 - Forest loops and clearings", parent);
            var random = new System.Random(1506);
            for (int x = -52; x <= -28; x += 8)
            for (int z = -10; z <= 64; z += 10)
            {
                var p = new Vector2(x + (float)random.NextDouble() * 3, z + (float)random.NextDouble() * 3);
                if (DistanceTo(p, Road) > 10) Tree(Ground(p.x,p.y), 7 + (float)random.NextDouble() * 3, forest);
            }
            for (int x = 48; x < 128; x += 5)
            for (int z = -40; z < 74; z += 5)
            {
                var p = new Vector2(x + (float)random.NextDouble() * 3, z + (float)random.NextDouble() * 3);
                if (DistanceTo(p, Road) < 6 || DistanceTo(p, NorthLoop) < 5 || DistanceTo(p, SouthLoop) < 5) continue;
                if (Vector2.Distance(p, new Vector2(64,34)) < 10 || Vector2.Distance(p, new Vector2(86,-3)) < 10 ||
                    Vector2.Distance(p, new Vector2(81,56)) < 10 || (p.x < 62 && p.y > -20 && p.y < 25)) continue;
                Tree(Ground(p.x,p.y), 7 + (float)random.NextDouble() * 5, forest);
            }
            var boundary = Group("07 - Natural boundary - expand northeast from here", parent);
            for (int x = -63; x <= 132; x += 6) { BoundaryRock(x,-50,boundary); BoundaryRock(x,78,boundary); }
            for (int z = -44; z < 78; z += 6) { BoundaryRock(-63,z,boundary); BoundaryRock(132,z,boundary); }
            // A continuous visible thicket closes the road, independent of scattered tree spacing.
            for (int i = -8; i <= 5; i++) BoundaryRock(117 + i * 3, 51 - i * 3, boundary);
            Label("Dense woodland", new Vector3(118, 4, 49), boundary);
            var dungeon = Group("08 - Slime dungeon entrance - exterior only", parent);
            Box("Dark sealed entrance", new Vector3(81,2.1f,59), new Vector3(5,4.2f,1), Dark, dungeon);
            Shape("Cave left", PrimitiveType.Sphere, new Vector3(77.8f,2,59), new Vector3(4,6,6), Stone, dungeon);
            Shape("Cave right", PrimitiveType.Sphere, new Vector3(84.2f,2,59), new Vector3(4,6,6), Stone, dungeon);
            Shape("Cave lintel", PrimitiveType.Sphere, new Vector3(81,4.4f,59), new Vector3(9,3,6), Stone, dungeon);
            Label("Slime dungeon", new Vector3(81,5.8f,58), dungeon);
            var vista = Group("09 - Great Tree - distant World 1 landmark", parent);
            // Visual continuation only: the playable boundary remains the removable forest ridge.
            Box("Distant meadow backdrop", new Vector3(200,-7,220), new Vector3(1200,6,1200), Grass, vista, false);
            Shape("Distant wooded hill", PrimitiveType.Sphere, new Vector3(170,-5,160),
                new Vector3(120,30,100), LeafLight, vista, false);
            Shape("Distant wooded ridge", PrimitiveType.Sphere, new Vector3(30,-6,210),
                new Vector3(180,36,110), Leaf, vista, false);
            var origin = new Vector3(300, -3, 330);
            Shape("Ancient trunk", PrimitiveType.Cylinder, origin + Vector3.up * 65, new Vector3(19,65,19), Timber, vista, false);
            for (int i = 0; i < 7; i++)
            {
                float angle = i * Mathf.PI * 2 / 7;
                var offset = new Vector3(Mathf.Cos(angle) * 40, 117 + i % 3 * 12, Mathf.Sin(angle) * 40);
                Shape("Vast crown", PrimitiveType.Sphere, origin + offset, new Vector3(90,48,83), LeafLight, vista, false);
                var branch = Shape("Ancient bough", PrimitiveType.Cylinder, origin + offset * 0.6f,
                    new Vector3(7, 37, 7), Timber, vista, false);
                branch.transform.up = offset.normalized;
            }
        }

        private static void BoundaryRock(float x, float z, Transform parent)
        {
            var p = Ground(x,z);
            Shape("Mossy ridge", PrimitiveType.Sphere, p + Vector3.up * 2, new Vector3(9,9,9), Stone, parent);
            Tree(p + Vector3.up * 4, 11, parent);
        }
        private static float DistanceTo(Vector2 p, Vector2[] line)
        {
            float minimum = float.MaxValue;
            for (int i = 1; i < line.Length; i++)
            {
                var d = line[i] - line[i - 1];
                minimum = Mathf.Min(minimum, Vector2.Distance(p, line[i - 1] + d * Mathf.Clamp01(Vector2.Dot(p - line[i - 1], d) / d.sqrMagnitude)));
            }
            return minimum;
        }
    }
}
