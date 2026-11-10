using DiceFree.Combat;
using DiceFree.Quests;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    [DisallowMultipleComponent]
    public sealed class WorldAtlasHud : MonoBehaviour
    {
        private static readonly Rect Extents = Rect.MinMaxRect(-54, -53, 332, 431);
        private static readonly Color Gold = new(.83f,.69f,.43f);
        private static readonly Color Ink = new(.88f,.83f,.70f);
        private static readonly Color Road = new(.66f,.53f,.35f);
        private static readonly Color PlayerTint = new(.42f,.88f,1f);
        private static readonly Landmark[] Landmarks =
        {
            new("Cornberg",new Vector2(8,6),"Your first settlement. Follow the road east toward Ironpine."),
            new("Ironpine",new Vector2(111,40),"Wolf territory between Cornberg and Trimery."),
            new("Alpha Wolf",new Vector2(167,57),"Level 20 overworld wolf. Accept The Ironpine Hunt before defeating it."),
            new("Trimery",new Vector2(248,183),"Timber town. Ilyra, Wounded Ranger, waits beside the priest. Her advancement quest opens at level 30 as a Physically Blessed Novice."),
            new("Briarback",new Vector2(265,311),"Quillboar territory. The open road to Arborium passes through the northern ridge."),
            new("Arborium",new Vector2(98,305),"World Tree capital. Enter through the north gate. The Adventurers’ Guild is just inside."),
        };
        private static readonly Vector2[] SouthernRoad =
        {
            new(8,6),new(57,24),new(111,40),new(173,75),new(215,119),new(248,183)
        };
        private static readonly Vector2[] NorthernRoad =
        {
            new(248,183),new(255,251),new(265,311),new(250,378),new(219,377),new(164,393),new(98,383),new(98,305)
        };
        private static readonly Vector2[] BrokenRoad = { new(248,183),new(218,214),new(166,255),new(98,305) };

        private readonly struct Landmark
        {
            public readonly string name, description;
            public readonly Vector2 point;
            public Landmark(string name, Vector2 point, string description)
            { this.name=name; this.point=point; this.description=description; }
        }

        private CombatActor player;
        private QuestJournal journal;
        public bool HighwayOpen => journal?.GetProgress("quest.arborium.roadwarden")?.status == QuestStatus.Completed;
        private Texture2D softDisk;
        private GUIStyle label, body, heading;
        private int selected;
        public bool Open { get; private set; }

        private void Awake()
        {
            player = GetComponent<CombatActor>();
            journal = GetComponent<QuestJournal>();
            const int size = 64;
            softDisk = new Texture2D(size,size,TextureFormat.RGBA32,false)
                { name="World atlas region shading", wrapMode=TextureWrapMode.Clamp };
            var pixels = new Color32[size*size];
            for (int y=0;y<size;y++)
                for (int x=0;x<size;x++)
                {
                    var point = new Vector2((x+.5f)*2/size-1,(y+.5f)*2/size-1);
                    pixels[y*size+x] = new Color(1,1,1,1-Mathf.SmoothStep(.26f,1,point.magnitude));
                }
            softDisk.SetPixels32(pixels);
            softDisk.Apply(false,true);
        }

        private void OnEnable() => HudPointerBlocker.OverlayCovers += Covers;
        private void OnDisable() => HudPointerBlocker.OverlayCovers -= Covers;
        private void OnDestroy() { if (softDisk != null) Destroy(softDisk); }
        private bool Covers(Vector2 _) => Open;

        public void Toggle()
        {
            if (!HudPointerBlocker.ModalOpen) Open = !Open;
        }

        private void Update()
        {
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
            label ??= new GUIStyle(GUI.skin.label) { alignment=TextAnchor.MiddleLeft, fontStyle=FontStyle.Bold,
                normal={textColor=Ink} };
            heading ??= new GUIStyle(label) { fontSize=19 };
            body ??= new GUIStyle(GUI.skin.label) { wordWrap=true, normal={textColor=Ink}, fontSize=13 };
            var theme = HudLayoutManager.Current?.Theme ?? HudThemes.Get(HudThemes.CompactStone);
            Fill(new Rect(0,0,Screen.width,Screen.height),new Color(.025f,.027f,.04f,.80f));
            float width = Mathf.Min(1110,Screen.width-24);
            float height = Mathf.Min(760,Screen.height-24);
            var panel = new Rect((Screen.width-width)*.5f,(Screen.height-height)*.5f,width,height);
            HudChrome.DrawPanel(panel,theme);
            HudChrome.DrawHeader(new Rect(panel.x+6,panel.y+6,width-12,34),theme,"WORLD 1  /  TRAVELLER’S ATLAS");
            if (GUI.Button(new Rect(panel.xMax-105,panel.y+10,92,25),"Close  [M]")) { Open=false; return; }

            float railWidth = Mathf.Clamp(width*.25f,145,250);
            var canvas = new Rect(panel.x+12,panel.y+48,width-railWidth-34,height-62);
            var rail = new Rect(canvas.xMax+10,canvas.y,railWidth,canvas.height);
            Fill(canvas,new Color(.097f,.139f,.136f));
            Fill(rail,new Color(.085f,.082f,.105f));
            HudChrome.Border(canvas,1,new Color(.41f,.42f,.31f));
            var plot = new Rect(canvas.x+37,canvas.y+24,canvas.width-86,canvas.height-52);
            DrawMap(plot);
            GUI.Label(new Rect(canvas.x+12,canvas.y+8,44,24),"↑ N",label);

            GUI.Label(new Rect(rail.x+12,rail.y+14,rail.width-24,29),Landmarks[selected].name,heading);
            GUI.Label(new Rect(rail.x+12,rail.y+53,rail.width-24,100),Landmarks[selected].description,body);
            float y = rail.y+155;
            for (int i=0;i<Landmarks.Length;i++,y+=25)
                if (GUI.Button(new Rect(rail.x+12,y,rail.width-24,22),Landmarks[i].name)) selected=i;
            if (y+68 < rail.yMax)
            {
                GUI.Label(new Rect(rail.x+12,y+11,rail.width-24,50),
                    "Gold  ·  Open roads\nRed dashes  ·  Broken highway\nBlue diamond  ·  You",body);
            }
            var current = Event.current;
            if (current.type is EventType.MouseDown or EventType.MouseUp or EventType.ScrollWheel) current.Use();
        }

        private void DrawMap(Rect plot)
        {
            for(int x=0;x<=300;x+=50)
                Line(Project(plot,new Vector2(x,-53)),Project(plot,new Vector2(x,431)),1,new Color(.42f,.50f,.42f,.20f));
            for(int z=0;z<=400;z+=50)
                Line(Project(plot,new Vector2(-54,z)),Project(plot,new Vector2(332,z)),1,new Color(.42f,.50f,.42f,.20f));
            Region(plot,new Vector2(101,52),new Vector2(200,131),new Color(.24f,.43f,.33f,.62f));
            Region(plot,new Vector2(257,302),new Vector2(114,178),new Color(.51f,.33f,.24f,.54f));
            Region(plot,new Vector2(94,307),new Vector2(169,163),new Color(.35f,.39f,.49f,.63f));
            Route(plot,SouthernRoad,Road);
            Route(plot,NorthernRoad,Road);
            if (HighwayOpen) Route(plot,BrokenRoad,Road);
            else for (int i=1;i<BrokenRoad.Length;i++)
            {
                var a=Project(plot,BrokenRoad[i-1]); var b=Project(plot,BrokenRoad[i]);
                for (int j=0;j<7;j++) Line(Vector2.Lerp(a,b,j/7f),Vector2.Lerp(a,b,(j+.57f)/7f),2,new Color(.73f,.37f,.35f));
            }
            for(int i=0;i<Landmarks.Length;i++)
            {
                var entry=Landmarks[i];
                var p=Project(plot,entry.point);
                var marker=new Rect(p.x-7,p.y-7,14,14);
                Fill(marker,i==selected?new Color(.98f,.81f,.45f):Gold);
                if(GUI.Button(new Rect(p.x+9,p.y-12,91,24),entry.name,label)) selected=i;
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
                    Line(p+Vector2.up*7,p+Vector2.right*7,3,PlayerTint);
                    Line(p+Vector2.right*7,p+Vector2.down*7,3,PlayerTint);
                    Line(p+Vector2.down*7,p+Vector2.left*7,3,PlayerTint);
                    Line(p+Vector2.left*7,p+Vector2.up*7,3,PlayerTint);
                }
            }
        }

        private static Vector2 Project(Rect plot,Vector2 world) => new(
            plot.x+Mathf.InverseLerp(Extents.xMin,Extents.xMax,world.x)*plot.width,
            plot.yMax-Mathf.InverseLerp(Extents.yMin,Extents.yMax,world.y)*plot.height);

        private void Region(Rect plot,Vector2 point,Vector2 size,Color color)
        {
            var p=Project(plot,point);
            var scale=new Vector2(size.x/Extents.width*plot.width,size.y/Extents.height*plot.height);
            var previous=GUI.color;
            GUI.color=color;
            GUI.DrawTexture(new Rect(p-scale*.5f,scale),softDisk);
            GUI.color=previous;
        }

        private static void Route(Rect plot,Vector2[] points,Color color)
        {
            for (int i=1;i<points.Length;i++)
                Line(Project(plot,points[i-1]),Project(plot,points[i]),3,color);
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
