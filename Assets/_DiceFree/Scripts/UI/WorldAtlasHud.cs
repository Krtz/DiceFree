using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.Quests;
using DiceFree.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    [DisallowMultipleComponent]
    public sealed partial class WorldAtlasHud : MonoBehaviour
    {
        private static readonly Rect Extents = Rect.MinMaxRect(-83, -53, 332, 475);
        private static readonly Rect VerdantBounds = Rect.MinMaxRect(-77, 264, 4, 353);
        private static readonly Rect FernhollowBounds = Rect.MinMaxRect(-77, 202, -7, 264);
        private static readonly Rect EastwatchBounds = Rect.MinMaxRect(-13, 373, 49, 426);
        public const string VerdantName = "Verdant Wilds";
        public const string LookoutName = "Reedscale Lookout";
        public const string CrimsonName = "Crimson Mire";
        private static readonly Color Gold = new(.83f,.69f,.43f);
        private static readonly Color Ink = new(.93f,.88f,.75f);
        private static readonly Color Road = new(.91f,.76f,.51f);
        private static readonly Color Footpath = new(.73f,.58f,.37f);
        private static readonly Color Closure = new(.91f,.48f,.40f);
        private static readonly Color PlayerTint = new(.42f,.88f,1f);
        private static readonly Color MapInk = new(.047f,.089f,.086f);
        private static readonly Color Frame = new(.51f,.43f,.29f);
        private const string SouthPassName = "Ironpine South Pass";
        private static readonly Landmark[] Landmarks =
        {
            new("Cornberg",new Vector2(8,6),"Your first settlement. Follow the road east toward Ironpine."),
            new("Ironpine",new Vector2(111,40),"Wolf territory between Cornberg and Trimery."),
            new("Alpha Wolf",new Vector2(167,57),"Level 20 overworld wolf. Accept The Ironpine Hunt before defeating it."),
            new("Trimery",new Vector2(248,183),"Timber town. Ilyra, Wounded Ranger, waits beside the priest. Her advancement quest opens at level 30 as a Physically Blessed Novice."),
            new("Briarback",new Vector2(265,311),"Quillboar territory. Follow the northern ridge road west to Arborium, or take the highland trail to Reedscale."),
            new("Arborium",new Vector2(98,305),"World Tree capital. Enter through the north gate. The Adventurers’ Guild is just inside."),
            new(VerdantName,new Vector2(-33,320),"Level 30–40 forest west of Arborium. Warden Lethen waits at Lanternmere. Follow the southern trail through Fernhollow to Crimson Mire."),
            new(LookoutName,new Vector2(-42,400),"Level 39–41 lizardmen. Cross the footbridge north of Lanternmere, or follow the highland trail west from Briarback through Eastwatch."),
            new(CrimsonName,new Vector2(65,144),"Level 50 red slimes gather around the rust-colored pools. The Crimson Crucible's red-crystal arch stands just east of the central trail: a six-boss gauntlet for level 55 adventurers. Trails connect Cornberg, Ironpine and Fernhollow."),
            new(SouthPassName,new Vector2(163,13),"Dense pines block this trail south of Ironpine. Complete The Ancient Road Dragon to open the way."),
        };
        private static readonly Vector2[] SouthPassTrail =
        {
            new(166,47),new(165,38),new(163,27),new(163,13)
        };
        private static readonly Vector2[] SouthernRoad =
        {
            new(8,6),new(54,6),new(95,34),new(114,43),new(175,47),new(180,80),
            new(183,98),new(208,130),new(223,149),new(248,153),new(248,183)
        };
        private static readonly Vector2[] NorthernRoad =
        {
            new(248,183),new(255,251),new(265,311),new(250,378),new(219,377),new(164,393),new(98,383),new(98,305)
        };
        private static readonly Vector2[] BrokenRoad = { new(248,183),new(218,214),new(166,255),new(98,305) };
        private static readonly Vector2[] VerdantTrail = { new(98,305),new(20,305),new(-17,305),new(-30,309) };
        private static readonly Vector2[] LookoutTrail = { new(-30,309),new(-20,326),new(-34,342),new(-36,369),new(-42,400) };
        private static readonly Vector2[] FernhollowTrail =
        {
            new(-30,309),new(-36,296),new(-43,264),new(-50,243),new(-46,221),
            new(-33,223),new(-27,234),new(-29,247),new(-43,264)
        };
        private static readonly Vector2[] EastwatchTrail =
        {
            new(-40,386),new(-24,378),new(-11,388),new(11,394),new(31,406),
            new(25,414),new(13,417),new(-5,406),new(11,394)
        };
        private static readonly Vector2[] ReedscaleCreek = { new(-74,371),new(-58,371),new(-43,369),new(-28,371),new(-14,370) };
        private static readonly Vector2[] MireTrail = { new(28,6),new(29,52),new(32,77),new(47,106),new(65,144),new(102,145),new(131,135),new(165,130),new(208,130) };
        private static readonly Vector2[] MirePools = { new(25,143),new(64,169),new(95,119),new(113,158) };
        private static readonly Vector2[] ForestSymbols =
        {
            new(25,45),new(40,65),new(63,51),new(85,82),new(114,93),new(153,99),
            new(96,3),new(138,14),new(160,29),new(198,47),new(211,70),new(260,138),
            new(-53,288),new(-35,279),new(-60,315),new(-62,344),new(-48,353),new(-19,345),new(-4,335),
            new(-59,393),new(-59,415),new(-26,416),
            new(-56,257),new(-42,241),new(-25,228),new(-56,218),new(-65,230),
            new(3,389),new(11,403),new(22,420),new(36,400),new(14,132),new(24,174),new(119,179),
            new(66,421),new(81,439),new(111,452),new(134,451)
        };
        private static readonly Vector2[] RidgeSymbols =
        {
            new(144,415),new(163,430),new(190,414),new(218,422),new(245,410),new(274,398),
            new(293,356),new(286,370),new(300,325),new(219,326),new(210,306),new(192,289),
            new(144,276),new(126,257),new(114,232)
        };

        private readonly struct Landmark
        {
            public readonly string name, description;
            public readonly Vector2 point;
            public Landmark(string name, Vector2 point, string description)
            { this.name=name; this.point=point; this.description=description; }
        }

        private static readonly (Vector2 point, Vector2 size, Color color)[] Regions =
        {
            (new(101,52),new(197,128),new(.20f,.42f,.31f,.72f)),
            (new(257,302),new(118,182),new(.47f,.28f,.18f,.73f)),
            (new(94,307),new(174,165),new(.27f,.42f,.52f,.73f)),
            (new(-35,317),new(85,114),new(.23f,.53f,.41f,.79f)),
            (new(-44,397),new(67,81),new(.48f,.51f,.30f,.68f)),
            (new(-43,244),new(70,69),new(.22f,.51f,.39f,.69f)),
            (new(15,399),new(68,48),new(.52f,.51f,.31f,.66f)),
            (new(89,428),new(131,70),new(.48f,.52f,.31f,.68f)),
            (new(-33,320),new(27,21),new(.24f,.62f,.65f,.82f)),
            (new(-25,204),new(64,46),new(.27f,.44f,.28f,.69f)),
            (new(18,184),new(91,40),new(.47f,.28f,.20f,.72f)),
            (new(65,144),new(149,99),new(.57f,.25f,.20f,.85f))
        };

        private CombatActor player;
        private QuestJournal journal;
        public bool HighwayOpen
        {
            get
            {
                var progress = journal?.GetProgress("quest.arborium.roadwarden");
                return progress != null && (progress.status == QuestStatus.Completed || progress.stage >= 2);
            }
        }
        private bool SouthPassUnlocked =>
            journal?.GetProgress("quest.arborium.ancient-road-dragon")?.status == QuestStatus.Completed;

        private Texture2D relief, mapGrain;
        private GUIStyle label, body, heading, caption;
        private int selected;
        public bool Open { get; private set; }

        private void Awake()
        {
            player = GetComponent<CombatActor>();
            journal = GetComponent<QuestJournal>();
        }

        private void CreateArtwork()
        {
            const int width = 480, height = 432;
            relief = new Texture2D(width,height,TextureFormat.RGBA32,false)
                { name="Traveller's Atlas relief", wrapMode=TextureWrapMode.Clamp };
            var pixels = new Color32[width*height];
            for (int y=0;y<height;y++)
            for (int x=0;x<width;x++)
            {
                var point = new Vector2(Extents.xMin+Extents.width*x/(width-1),Extents.yMin+Extents.height*y/(height-1));
                float folds = Mathf.PerlinNoise(36+point.x*.017f,19+point.y*.015f);
                float grainDetail = Mathf.PerlinNoise(83+point.x*.092f,67+point.y*.093f);
                float heightMark = folds*.70f + grainDetail*.18f + Mathf.PerlinNoise(point.x*.038f+91,point.y*.039f+4)*.12f;
                float contour = 1-Mathf.SmoothStep(.023f,.108f,Mathf.Abs(Mathf.Repeat(heightMark*11,1)-.5f));
                var ink = Color.Lerp(MapInk,new Color(.12f,.19f,.155f),Mathf.InverseLerp(.22f,.89f,heightMark)*.71f);
                foreach (var region in Regions)
                {
                    float dx=(point.x-region.point.x)/region.size.x, dz=(point.y-region.point.y)/region.size.y;
                    float distance=Mathf.Sqrt(dx*dx+dz*dz)*2 + (folds-.5f)*.26f;
                    float wash=(1-Mathf.SmoothStep(.20f,1.12f,distance))*region.color.a*.73f;
                    ink=Color.Lerp(ink,region.color,wash);
                }
                ink=Color.Lerp(ink,new Color(.61f,.55f,.35f),contour*.085f+grainDetail*.018f);
                pixels[y*width+x]=ink;
            }
            relief.SetPixels32(pixels);
            relief.Apply(false,true);
            const int grainSize = 128;
            mapGrain = new Texture2D(grainSize, grainSize, TextureFormat.RGBA32, false)
                { name = "World atlas paper grain", wrapMode = TextureWrapMode.Repeat };
            var grain = new Color32[grainSize * grainSize];
            for (int y = 0; y < grainSize; y++)
                for (int x = 0; x < grainSize; x++)
                {
                    float cloud = Mathf.PerlinNoise(x * .071f, y * .071f);
                    int speckle = ((x * 73 ^ y * 151 ^ (x * y * 17)) & 31);
                    byte value = (byte)(153 + cloud * 69 + speckle);
                    grain[y * grainSize + x] = new Color32(value, value, value, 255);
                }
            mapGrain.SetPixels32(grain);
            mapGrain.Apply(false, true);
        }

        private void OnEnable() => HudPointerBlocker.OverlayCovers += Covers;
        private void OnDisable() => HudPointerBlocker.OverlayCovers -= Covers;
        private void OnDestroy()
        {
            if (relief != null) Destroy(relief);
            if (mapGrain != null) Destroy(mapGrain);
        }
        private bool Covers(Vector2 _) => Open;
        internal bool InVerdantWilds => player != null &&
            (ContainsPlayer(VerdantBounds) || ContainsPlayer(FernhollowBounds) ||
                MirewildTrail.ContainsForestExtension(PlayerPoint));
        internal bool InLookout => player != null && player.transform.position.y < 24 &&
            (ContainsPlayer(Rect.MinMaxRect(-77,353,-13,427)) || ContainsPlayer(EastwatchBounds) ||
                ReedscalePass.ContainsHighlands(PlayerPoint));
        internal bool InCrimsonMire
        {
            get
            {
                if (player == null) return false;
                var point = PlayerPoint;
                var offset = new Vector2((point.x-65)/72, (point.y-144)/47);
                return offset.sqrMagnitude <= 1 || MirewildTrail.ContainsMireExtension(point);
            }
        }
        private Vector2 PlayerPoint => new(player.transform.position.x,player.transform.position.z);
        private bool ContainsPlayer(Rect bounds) => bounds.Contains(PlayerPoint);

        public void Toggle()
        {
            if (!HudPointerBlocker.ModalOpen)
            {
                Open = !Open;
                if (Open) RefreshMapMarkers();
            }
        }

        private void Update()
        {
            if (DiceFree.Input.InputBindings.Current.Suppressed) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (Open && keyboard.escapeKey.wasPressedThisFrame) Open = false;
            else if (keyboard.mKey.wasPressedThisFrame) Toggle();
        }

        private void OnGUI()
        {
            if (!Open || HudPointerBlocker.ModalOpen) return;
            int previousDepth = GUI.depth;
            GUI.depth = -90;
            try { DrawAtlas(); }
            finally { GUI.depth = previousDepth; }
        }

        private void DrawAtlas()
        {
            mapLabelRects.Clear();
            if (relief == null) CreateArtwork();
            label ??= new GUIStyle(GUI.skin.label) { alignment=TextAnchor.MiddleLeft, fontStyle=FontStyle.Bold, wordWrap=false,
                normal={textColor=Ink} };
            heading ??= new GUIStyle(label) { fontSize=19 };
            body ??= new GUIStyle(GUI.skin.label) { wordWrap=true, normal={textColor=Ink}, fontSize=13 };
            caption ??= new GUIStyle(label) { fontSize=11, normal={textColor=Gold} };
            var theme = HudLayoutManager.Current?.Theme ?? HudThemes.Get(HudThemes.CompactStone);
            Fill(new Rect(0,0,Screen.width,Screen.height),new Color(.014f,.028f,.033f,.85f));
            float width = Mathf.Min(1110,Screen.width-24);
            float height = Mathf.Min(760,Screen.height-24);
            var panel = new Rect((Screen.width-width)*.5f,(Screen.height-height)*.5f,width,height);
            HudChrome.DrawPanel(panel,theme);
            HudChrome.DrawHeader(new Rect(panel.x+6,panel.y+6,width-12,34),theme,
                InAshfall ? "WORLD 2  /  TRAVELLER’S ATLAS" : "WORLD 1  /  TRAVELLER’S ATLAS");
            if (GUI.Button(new Rect(panel.xMax-105,panel.y+10,92,25),"Close  [M]")) { Open=false; return; }

            float railWidth = Mathf.Clamp(width*.25f,145,250);
            var canvas = new Rect(panel.x+12,panel.y+48,width-railWidth-34,height-62);
            var rail = new Rect(canvas.xMax+10,canvas.y,railWidth,canvas.height);
            Fill(canvas, MapInk);
            Fill(rail,new Color(.088f,.109f,.095f));
            var plot = new Rect(canvas.x+29,canvas.y+27,canvas.width-103,canvas.height-59);
            if (!InAshfall) GUI.DrawTexture(plot,relief);
            var previousColor = GUI.color;
            GUI.color = new Color(.29f,.29f,.17f,.15f);
            GUI.DrawTextureWithTexCoords(plot, mapGrain, new Rect(0, 0, plot.width / 256, plot.height / 256));
            GUI.color = previousColor;
            HudChrome.Border(canvas,1,Frame);
            HudChrome.Border(new Rect(canvas.x+5,canvas.y+5,canvas.width-10,canvas.height-10),1,new Color(.37f,.34f,.23f));
            HudChrome.Border(rail,1,Frame);
            HudChrome.Border(plot,1,new Color(.56f,.48f,.30f,.31f));
            DrawCorners(canvas);
            if (InAshfall)
            {
                DrawAshfall(plot, rail);
                DrawMapMarkers(plot);
                DrawMapMarkerLegend(rail);
                DrawCompass(new Vector2(canvas.xMax-40, canvas.y+46));
                DrawMapMarkerTooltip();
                var input = Event.current;
                if (input.type is EventType.MouseDown or EventType.MouseUp or EventType.ScrollWheel) input.Use();
                return;
            }
            DrawMap(plot);
            DrawMapMarkers(plot);
            DrawMapMarkerLegend(rail);
            DrawCompass(new Vector2(canvas.xMax-40, canvas.y+46));

            Fill(new Rect(rail.x+1,rail.y+1,rail.width-2,48),new Color(.20f,.19f,.125f));
            DrawLandmark(new Vector2(rail.x+22,rail.y+24),selected,true);
            GUI.Label(new Rect(rail.x+36,rail.y+9,rail.width-45,30),Landmarks[selected].name,heading);
            Line(new Vector2(rail.x+14,rail.y+44),new Vector2(rail.xMax-14,rail.y+44),1,Frame);
            var landmark = Landmarks[selected];
            string description = landmark.name == SouthPassName && SouthPassUnlocked
                ? "Follow the trail south from the Ironpine road to the old stone gate into World 2."
                : landmark.description;
            GUI.Label(new Rect(rail.x+14,rail.y+54,rail.width-28,100),description,body);
            float y = rail.y+Mathf.Clamp(rail.height-166,106,157);
            float spacing = Mathf.Clamp((rail.yMax-y-12-MapMarkerLegendHeight)/Landmarks.Length, 16, 29);
            for (int i=0;i<Landmarks.Length;i++,y+=spacing)
            {
                var row = new Rect(rail.x+10,y,rail.width-20,spacing-2);
                bool hovered=row.Contains(Event.current.mousePosition);
                Fill(row,i==selected ? new Color(.295f,.244f,.147f) : hovered ? new Color(.20f,.219f,.16f) : new Color(.13f,.16f,.131f));
                if (i==selected) Fill(new Rect(row.x,row.y,2,row.height),Gold);
                DrawLandmark(new Vector2(row.x+12,row.center.y),i,i==selected);
                if (GUI.Button(new Rect(row.x+23,row.y,row.width-23,row.height),Landmarks[i].name,label)) selected=i;
                if (Event.current.type==EventType.MouseDown && row.Contains(Event.current.mousePosition) && Event.current.button==0)
                {
                    selected=i;
                    Event.current.Use();
                }
            }
            if (y+93 < rail.yMax-MapMarkerLegendHeight)
            {
                Line(new Vector2(rail.x+14,y+9),new Vector2(rail.xMax-14,y+9),1,Frame);
                Legend(rail.x+16,y+24,Road,"Open road");
                Legend(rail.x+16,y+43,Footpath,"Footpath",true);
                Legend(rail.x+16,y+62,Closure,"Broken highway",true);
                Legend(rail.x+16,y+81,PlayerTint,"Your location");
            }
            DrawMapMarkerTooltip();
            var current = Event.current;
            if (current.type is EventType.MouseDown or EventType.MouseUp or EventType.ScrollWheel) current.Use();
        }

        private void DrawMap(Rect plot)
        {
            for(int x=-50;x<=300;x+=50)
                Line(Project(plot,new Vector2(x,Extents.yMin)),Project(plot,new Vector2(x,Extents.yMax)),1,new Color(.54f,.55f,.36f,.13f));
            for(int z=0;z<=400;z+=50)
                Line(Project(plot,new Vector2(Extents.xMin,z)),Project(plot,new Vector2(Extents.xMax,z)),1,new Color(.54f,.55f,.36f,.13f));
            foreach (var at in ForestSymbols) ForestMark(Project(plot,at));
            foreach (var at in RidgeSymbols) RidgeMark(Project(plot,at));
            Route(plot,ReedscaleCreek,new Color(.39f,.70f,.71f),2.3f);
            Route(plot,SouthernRoad,Road);
            Route(plot,NorthernRoad,Road);
            Route(plot,VerdantTrail,Footpath,1.9f,true);
            Route(plot,LookoutTrail,Footpath,1.9f,true);
            Route(plot,FernhollowTrail,Footpath,1.9f,true);
            Route(plot,EastwatchTrail,Footpath,1.9f,true);
            Route(plot,ReedscalePass.Points,Footpath,1.9f,true);
            Route(plot,MireTrail,Footpath,1.9f,true);
            Route(plot,MirewildTrail.Points,Footpath,1.9f,true);
            Route(plot,SouthPassTrail,SouthPassUnlocked ? Footpath : Closure,1.9f,true);
            if (HighwayOpen) Route(plot,BrokenRoad,Road);
            else Route(plot,BrokenRoad,Closure,1.9f,true);
            foreach(var pool in MirePools)
            {
                var p=Project(plot,pool);
                Line(p+new Vector2(-6,0),p+new Vector2(7,0),1.2f,new Color(.75f,.52f,.37f,.73f));
                Line(p+new Vector2(-3,3),p+new Vector2(5,3),1.2f,new Color(.75f,.52f,.37f,.52f));
            }
            for(int i=0;i<Landmarks.Length;i++)
            {
                var entry=Landmarks[i];
                var p=Project(plot,entry.point);
                var marker=new Rect(p.x-8,p.y-8,16,16);
                DrawLandmark(p,i,i==selected);
                var text = new GUIContent(entry.name);
                float labelWidth = label.CalcSize(text).x + 8;
                var labelRect = new Rect(p.x+12,p.y-12,labelWidth,24);
                mapLabelRects.Add(labelRect);
                Fill(new Rect(labelRect.x-2,p.y-8,labelRect.width,16),new Color(.036f,.063f,.057f,i==selected?.95f:.82f));
                if(GUI.Button(labelRect,text,label)) selected=i;
                if(Event.current.type==EventType.MouseDown&&marker.Contains(Event.current.mousePosition))
                { selected=i; Event.current.Use(); }
            }
            if (player != null)
            {
                var position = player.transform.position;
                var point = new Vector2(position.x,position.z);
                if (Extents.Contains(point))
                {
                    var p=Project(plot,point);
                    mapLabelRects.Add(new Rect(p.x-10,p.y-10,20,20));
                    Diamond(p,12,1.2f,new Color(PlayerTint.r,PlayerTint.g,PlayerTint.b,.55f));
                    Diamond(p,8,4,MapInk);
                    Diamond(p,7,2,PlayerTint);
                    Fill(new Rect(p.x-1,p.y-1,2,2),PlayerTint);
                }
            }
        }

        private void DrawCompass(Vector2 p)
        {
            var previous=p+Vector2.right*23;
            for(int i=1;i<=32;i++)
            {
                float angle=i*Mathf.PI/16;
                var next=p+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*23;
                Line(previous,next,1,Frame);
                previous=next;
            }
            Diamond(p,16,1,Frame);
            for(int i=0;i<4;i++)
            {
                float a=i*Mathf.PI/2-Mathf.PI/2;
                var direction=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                var side=new Vector2(-direction.y,direction.x)*4;
                var tip=p+direction*(i==0?22:17);
                var color=i==0?new Color(1,.85f,.52f):Gold;
                Line(p+side,tip,1.2f,color);
                Line(tip,p-side,1.2f,color);
                Line(p,tip,1.6f,color);
            }
            Diamond(p,4,1.6f,Gold);
            GUI.Label(new Rect(p.x-4,p.y-41,16,16),"N",caption);
        }

        private static void DrawCorners(Rect frame)
        {
            for(int y=0;y<2;y++)
            for(int x=0;x<2;x++)
            {
                var corner=new Vector2(x==0?frame.x+10:frame.xMax-10,y==0?frame.y+10:frame.yMax-10);
                var horizontal=Vector2.right*(x==0?1:-1);
                var vertical=Vector2.down*(y==0?1:-1);
                Diamond(corner,4,1.25f,Gold);
                Line(corner+horizontal*6,corner+horizontal*29,1,Frame);
                Line(corner+vertical*6,corner+vertical*29,1,Frame);
            }
        }

        private static void DrawLandmark(Vector2 p,int index,bool selected)
        {
            var tint=selected?new Color(1,.86f,.52f):Gold;
            Diamond(p,selected?9:7,1.8f,MapInk);
            Diamond(p,selected?8:6,1.25f,tint);
            if(index==0||index==3||index==5)
            {
                Fill(new Rect(p.x-2.3f,p.y-1.5f,4.6f,3.7f),tint);
                Line(p+new Vector2(-3,-1),p+new Vector2(0,-3.6f),1.1f,tint);
                Line(p+new Vector2(0,-3.6f),p+new Vector2(3,-1),1.1f,tint);
            }
            else
                Diamond(p,index==2?3.1f:2.2f,1.4f,index==2||index==8?Closure:tint);
        }

        private void Legend(float x,float y,Color color,string text,bool broken=false)
        {
            if (color==PlayerTint) Diamond(new Vector2(x+8,y),5,2,color);
            else if (broken)
            {
                Line(new Vector2(x,y),new Vector2(x+6,y),2,color);
                Line(new Vector2(x+10,y),new Vector2(x+16,y),2,color);
            }
            else Line(new Vector2(x,y),new Vector2(x+16,y),2,color);
            GUI.Label(new Rect(x+23,y-9,117,18),text,caption);
        }

        private static void ForestMark(Vector2 p)
        {
            var tint = new Color(.47f,.64f,.43f,.74f);
            Line(p+new Vector2(-5,3),p+new Vector2(0,-6),1.2f,tint);
            Line(p+new Vector2(0,-6),p+new Vector2(5,3),1.2f,tint);
            Line(p+new Vector2(-3,2),p+new Vector2(3,2),1,tint);
            Line(p+new Vector2(0,1),p+new Vector2(0,5),1,tint);
        }

        private static void RidgeMark(Vector2 p)
        {
            var tint = new Color(.73f,.60f,.41f,.59f);
            Line(p+new Vector2(-10,5),p+new Vector2(-2,-7),1.25f,tint);
            Line(p+new Vector2(-2,-7),p+new Vector2(4,2),1.25f,tint);
            Line(p+new Vector2(3,0),p+new Vector2(7,-5),1.1f,tint);
            Line(p+new Vector2(7,-5),p+new Vector2(13,5),1.1f,tint);
        }

        private static void Diamond(Vector2 p,float radius,float width,Color color)
        {
            Line(p+Vector2.up*radius,p+Vector2.right*radius,width,color);
            Line(p+Vector2.right*radius,p+Vector2.down*radius,width,color);
            Line(p+Vector2.down*radius,p+Vector2.left*radius,width,color);
            Line(p+Vector2.left*radius,p+Vector2.up*radius,width,color);
        }

        private static Vector2 Project(Rect plot,Vector2 world) => new(
            plot.x+Mathf.InverseLerp(Extents.xMin,Extents.xMax,world.x)*plot.width,
            plot.yMax-Mathf.InverseLerp(Extents.yMin,Extents.yMax,world.y)*plot.height);

        private static void Route(Rect plot,IReadOnlyList<Vector2> points,Color color,float width=2.65f,bool dashed=false)
        {
            for (int i=1;i<points.Count;i++)
            {
                var from=Project(plot,points[i-1]);
                var to=Project(plot,points[i]);
                if (!dashed)
                {
                    Line(from,to,width+1.8f,MapInk);
                    Line(from,to,width,color);
                    continue;
                }
                float length=Vector2.Distance(from,to);
                for(float at=0;at<length;at+=8)
                    Line(Vector2.Lerp(from,to,at/length),Vector2.Lerp(from,to,(at+5)/length),width,color);
            }
        }

        private static void Line(Vector2 start,Vector2 end,float thickness,Color color)
        {
            var direction=end-start;
            var previous=GUI.matrix;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg,start);
            Fill(new Rect(start.x,start.y-thickness*.5f,direction.magnitude,thickness),color);
            GUI.matrix=previous;
        }

        private static void Fill(Rect rect,Color color)
        {
            var previous=GUI.color;
            GUI.color=color;
            GUI.DrawTexture(rect,Texture2D.whiteTexture);
            GUI.color=previous;
        }
    }
}
