using System.Collections.Generic;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed partial class WorldAtlasHud
    {
        private int ashfallSelected;
        private bool InAshfall => player != null && AshfallReach.Bounds.Contains(PlayerPoint);
        private static readonly Color CinderInk = new(.89f, .51f, .27f);
        private static readonly string[] AshfallNames = { "Ironpine Gate", "Cinderwatch", "Basalt Needle", "Ember Crown", "Willowford" };
        private static readonly string[] AshfallDescriptions =
        {
            "The stone gate returns to Ironpine in World 1.",
            "A small refuge beside the valley road. Drink from the spring to recover health.",
            "Tall fractured columns rise above the northwest ridge. Ashscale spearhands and ridgebows patrol its slopes.",
            "An open volcanic crown marks the southeast of the reach. Ashscale patrols guard the trails around its base.",
            "A larger river settlement in Steamroot Vale. Follow the road west from Cinderwatch. The outfitter, spring and timber bridge serve travellers along the Cornberg river."
        };
        private static readonly Vector2[] AshfallPlaces =
        {
            AshfallReach.NorthGate, AshfallReach.Cinderwatch, AshfallReach.BasaltNeedle, AshfallReach.EmberCrown, AshfallReach.Willowford
        };

        private void DrawAshfall(Rect plot, Rect rail)
        {
            Fill(plot, new Color(.115f, .102f, .081f));
            var forestEdge=AshfallProject(plot,new Vector2(-60,0));
            Fill(new Rect(plot.x,plot.y,forestEdge.x-plot.x,plot.height),new Color(.075f,.135f,.095f));
            for (int i = 0; i < 6; i++)
            {
                float fraction = (i + 1) / 7f;
                Line(new(plot.x + plot.width * fraction, plot.y), new(plot.x + plot.width * fraction, plot.yMax),
                    1, new Color(.64f, .50f, .32f, .10f));
                Line(new(plot.x, plot.y + plot.height * fraction), new(plot.xMax, plot.y + plot.height * fraction),
                    1, new Color(.64f, .50f, .32f, .10f));
            }
            for (int i = 0; i < 35; i++)
            {
                float x = i % 2 == 0 ? -36 + Mathf.Sin(i * .8f) * 5 : 37 + Mathf.Sin(i) * 5;
                float z = 29 - i * 2.95f;
                RidgeMark(AshfallProject(plot, new Vector2(x, z)));
            }
            for (int i = 0; i < 21; i++)
            {
                float x = Mathf.Sin(i * 23.76f) * 14, z = 20 - i * 2.45f;
                if (Mathf.Abs(x) > 6) ForestMark(AshfallProject(plot, new Vector2(x, z)));
            }
            AshfallRoute(plot, AshfallReach.ArrivalTrail, Road, 2.6f);
            AshfallRoute(plot, AshfallReach.SettlementRoad, Road, 2.6f);
            AshfallRoute(plot, AshfallReach.NeedlePath, Footpath, 1.65f);
            AshfallRoute(plot, AshfallReach.CrownPath, Footpath, 1.65f);
            AshfallRoute(plot, AshfallReach.WillowfordRoad, Road, 2.6f);
            AshfallRoute(plot, AshfallReach.RiverbankTrail, Footpath, 1.65f);
            AshfallRoute(plot, AshfallReach.River, new Color(.30f,.66f,.74f), 4.0f);
            for(int i=0;i<58;i++)
            {
                float x=-221+Mathf.Repeat(i*37.7f,152),z=22-Mathf.Repeat(i*17.9f,166);
                if(Mathf.Abs(x-AshfallReach.RiverX(z))>10)
                    ForestMark(AshfallProject(plot,new Vector2(x,z)));
            }
            var forestName=AshfallProject(plot,new Vector2(-145,13));
            GUI.Label(new Rect(forestName.x-56,forestName.y-12,150,23),"Steamroot Vale",caption);

            for (int i = 0; i < AshfallPlaces.Length; i++)
            {
                var p = AshfallProject(plot, AshfallPlaces[i]);
                bool pip = i is 2 or 3;
                Color color = i == 3 ? CinderInk : Gold;
                if (pip)
                {
                    for (int ring = 0; ring < 3; ring++)
                    {
                        float radius = 15 + ring * 6;
                        for (int j = 0; j < 32; j++)
                        {
                            float a = j * Mathf.PI / 16, b = (j + 1) * Mathf.PI / 16;
                            Line(p + new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * radius,
                                p + new Vector2(Mathf.Sin(b), Mathf.Cos(b)) * radius, 2,
                                new Color(color.r, color.g, color.b, .35f - ring * .10f));
                        }
                    }
                    if (i == 2)
                    {
                        Line(p + new Vector2(-6, 7), p + new Vector2(-1, -14), 4, color);
                        Line(p + new Vector2(2, 7), p + new Vector2(6, -8), 4, color);
                    }
                    else
                    {
                        RidgeMark(p + new Vector2(-3, 3));
                        Line(p + new Vector2(-4, -5), p + new Vector2(4, -5), 3, color);
                    }
                }
                else Diamond(p, i == 1 ? 7 : 5, 2, color);
                if (i == ashfallSelected) Diamond(p, pip ? 33 : 12, 1, color);
                float textWidth = label.CalcSize(new GUIContent(AshfallNames[i])).x + 10;
                float labelX = Mathf.Clamp(p.x - textWidth / 2, plot.x + 2, plot.xMax - textWidth - 2);
                var textBox = new Rect(labelX, p.y + (pip ? 31 : 8), textWidth, 23);
                mapLabelRects.Add(textBox);
                Fill(textBox, new Color(.065f, .06f, .052f, .95f));
                if (GUI.Button(textBox, AshfallNames[i], label)) ashfallSelected = i;
                if (Event.current.type == EventType.MouseDown && (p - Event.current.mousePosition).sqrMagnitude < 18 * 18)
                {
                    ashfallSelected = i;
                    Event.current.Use();
                }
            }
            var localPlayer = PlayerPoint - new Vector2(AshfallReach.Origin.x, AshfallReach.Origin.z);
            var playerOnMap = AshfallProject(plot, localPlayer);
            mapLabelRects.Add(new Rect(playerOnMap.x - 10, playerOnMap.y - 10, 20, 20));
            Diamond(playerOnMap, 8, 2.2f, PlayerTint);
            GUI.Label(new Rect(rail.x + 15, rail.y + 12, rail.width - 30, 30), AshfallNames[ashfallSelected], heading);
            Line(new(rail.x + 14, rail.y + 49), new(rail.xMax - 14, rail.y + 49), 1, Frame);
            GUI.Label(new Rect(rail.x + 14, rail.y + 61, rail.width - 28, 125), AshfallDescriptions[ashfallSelected], body);
            float y = rail.y + 207;
            for (int i = 0; i < AshfallNames.Length; i++)
            {
                var row = new Rect(rail.x + 12, y, rail.width - 24, 30);
                Fill(row, i == ashfallSelected ? new Color(.295f, .244f, .147f) : new Color(.13f, .16f, .131f));
                if (GUI.Button(new Rect(row.x + 7, row.y, row.width - 14, row.height), AshfallNames[i], label))
                    ashfallSelected = i;
                y += 34;
            }
            if (y + 130 < rail.yMax - MapMarkerLegendHeight)
            {
                Legend(rail.x + 16, y + 20, Road, "Valley road");
                Legend(rail.x + 16, y + 42, Footpath, "Ridge trail");
                Legend(rail.x + 16, y + 64, PlayerTint, "Your location");
                GUI.Label(new Rect(rail.x + 14, y + 86, rail.width - 28, 30), "Ashscale patrols · Levels 53–55", caption);
            }
        }

        private static Vector2 AshfallProject(Rect plot, Vector2 position) => new(
            plot.x + Mathf.InverseLerp(AshfallReach.LocalBounds.xMin, AshfallReach.LocalBounds.xMax, position.x) * plot.width,
            plot.yMax - Mathf.InverseLerp(AshfallReach.LocalBounds.yMin, AshfallReach.LocalBounds.yMax, position.y) * plot.height);

        private static void AshfallRoute(Rect plot, IReadOnlyList<Vector2> points, Color color, float width)
        {
            for (int i = 1; i < points.Count; i++)
            {
                var start = AshfallProject(plot, points[i - 1]);
                var end = AshfallProject(plot, points[i]);
                Line(start, end, width + 2, MapInk);
                Line(start, end, width, color);
            }
        }
    }
}
