using System;
using System.Linq;
using DiceFree.Persistence;
using DiceFree.UI;
using UnityEngine;
namespace DiceFree.Advancement
{
    public sealed partial class StartMenuController
    {
        [SerializeField] MenuPresentationCatalog menuPresentation;
        MenuPortraitStage portraitStage;OptionsPanel options;
        GUIStyle title,heading,small,body,cardTitle,button,tab,nameField;
        Texture2D duskBackdrop, portraitLight;
        static readonly Color Gold=new(.82f,.67f,.39f),Muted=new(.67f,.71f,.74f);
        void OnDestroy()
        {
            if (options != null) { options.ShowLauncher = true; options.QuitToDesktop = null; }
            if (duskBackdrop != null) Destroy(duskBackdrop);
            if (portraitLight != null) Destroy(portraitLight);
        }
        public int SelectedTier {get;private set;}
        public MenuPortraitStage PortraitStage=>portraitStage;
        public ManifestationEntry[] EntriesForTier(int tier)=>Entries.Where(e=>e.tier==tier).ToArray();
        public string[] LockedClassIdsForTier(int tier)
        {
            if (menuPresentation == null) return Array.Empty<string>();
            var obtained = Entries.Select(entry => entry.classId).ToHashSet(StringComparer.Ordinal);
            return menuPresentation.forms.Where(form => form != null && form.tier == tier &&
                !string.IsNullOrEmpty(form.classId) && !obtained.Contains(form.classId))
                .Select(form => form.classId).Distinct().ToArray();
        }
        public void SelectTier(int tier){SelectedTier=tier;scroll=Vector2.zero;}
        public void OpenOptions(OptionsPanel.OptionsTab page=OptionsPanel.OptionsTab.Gameplay){options.Show();options.SelectTab(page);}
        static string TierName(int tier)=>tier switch{0=>"Tier 0",1=>"Tier I",2=>"Tier II",3=>"Tier III",4=>"Tier IV",5=>"Tier V",-1=>"Unavailable",_=>"Tier "+tier};
        void Styles(){
            if(title!=null)return;
            GUIStyle Text(int size,Color color,bool bold=false)=>new(GUI.skin.label){fontSize=size,wordWrap=true,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,normal={textColor=color},richText=false};
            title=Text(48,new Color(.99f,.95f,.83f),true);heading=Text(23,new Color(.95f,.92f,.84f),true);body=Text(15,new Color(.85f,.88f,.91f));
            small=Text(12,Muted);cardTitle=Text(18,new Color(.96f,.93f,.84f),true);
            button=new GUIStyle(GUI.skin.button){fontSize=15,fontStyle=FontStyle.Bold,padding=new RectOffset(10,10,7,7)};
            tab=new GUIStyle(button){fontSize=13};nameField=new GUIStyle(GUI.skin.textField){fontSize=18,padding=new RectOffset(10,10,8,8)};
        }
        static void Fill(Rect r,Color c)=>HudChrome.Fill(r,c);
        static void Frame(Rect r,bool selected=false)
        {
            var edge = selected ? new Color(.59f,.46f,.25f) : new Color(.27f,.32f,.34f);
            Fill(new Rect(r.x+4,r.y+5,r.width,r.height),new Color(.011f,.017f,.024f,.64f));
            Fill(r,edge);
            Fill(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),new Color(.071f,.097f,.116f,.97f));
            HudChrome.Border(new Rect(r.x+5,r.y+5,r.width-10,r.height-10),1,new Color(edge.r,edge.g,edge.b,.43f));
            Fill(new Rect(r.x,r.y,28,2),selected?Gold:edge);
            Fill(new Rect(r.x,r.y,2,24),selected?Gold:edge);
            Fill(new Rect(r.xMax-28,r.yMax-2,28,2),selected?Gold:edge);
            Fill(new Rect(r.xMax-2,r.yMax-24,2,24),selected?Gold:edge);
        }

        void DrawBackdrop()
        {
            if (duskBackdrop == null)
            {
                const int width=384,height=216;
                duskBackdrop=new Texture2D(width,height,TextureFormat.RGBA32,false)
                    { name="DiceFree dusk backdrop",wrapMode=TextureWrapMode.Clamp };
                var pixels=new Color32[width*height];
                var night=new Color(.025f,.047f,.066f);
                var haze=new Color(.16f,.21f,.22f);
                for (int y=0;y<height;y++)
                    for (int x=0;x<width;x++)
                    {
                        float u=(float)x/(width-1),v=(float)y/(height-1);
                        float cloud=Mathf.PerlinNoise(u*5.2f,v*3.2f);
                        float glow=Mathf.Exp(-14*(v-.36f)*(v-.36f)-2.5f*(u-.66f)*(u-.66f));
                        var color=Color.Lerp(night,haze,glow*(.45f+.42f*cloud));
                        float far=.14f+.075f*Mathf.PerlinNoise(u*9.3f,12.4f)+.040f*Mathf.PerlinNoise(u*20,7.7f);
                        float near=.058f+.10f*Mathf.PerlinNoise(u*6.7f,22.3f)+.035f*Mathf.PerlinNoise(u*34,4.1f);
                        color=Color.Lerp(new Color(.068f,.108f,.133f),color,Mathf.SmoothStep(0,1,Mathf.InverseLerp(far-.006f,far+.012f,v)));
                        color=Color.Lerp(new Color(.028f,.055f,.062f),color,Mathf.SmoothStep(0,1,Mathf.InverseLerp(near-.004f,near+.007f,v)));
                        pixels[y*width+x]=color;
                    }
                duskBackdrop.SetPixels32(pixels);
                duskBackdrop.Apply(false,true);
            }
            GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),duskBackdrop,ScaleMode.StretchToFill,false);
        }

        void DrawPortraitLight(Rect area)
        {
            if (portraitLight == null)
            {
                const int size=128;
                portraitLight=new Texture2D(size,size,TextureFormat.RGBA32,false)
                    { name="Manifestation portrait light",wrapMode=TextureWrapMode.Clamp };
                var pixels=new Color32[size*size];
                for (int y=0;y<size;y++)
                    for (int x=0;x<size;x++)
                    {
                        var offset=new Vector2((x+.5f)*2/size-1,(y+.5f)*2/size-1);
                        float radius=offset.magnitude;
                        float glow=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.14f,1,radius));
                        float ring=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.005f,.021f,Mathf.Abs(radius-.69f)));
                        var color=Color.Lerp(new Color(.25f,.39f,.41f),Gold,ring);
                        color.a=.33f*glow+.23f*ring;
                        pixels[y*size+x]=color;
                    }
                portraitLight.SetPixels32(pixels);
                portraitLight.Apply(false,true);
            }
            GUI.DrawTexture(area,portraitLight,ScaleMode.StretchToFill,true);
        }
        bool Button(Rect r,string label,bool primary=false){var old=GUI.backgroundColor;GUI.backgroundColor=primary?new Color(.48f,.39f,.22f):new Color(.24f,.30f,.37f);bool hit=GUI.Button(r,label,button);GUI.backgroundColor=old;return hit;}
        void OnGUI(){
            if(DiceFree.ApplicationFlow.LoadingScreen.Busy)return;
            Styles();var matrix=GUI.matrix;bool enabled=GUI.enabled;
            DrawBackdrop();
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/800f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)*.5f,(Screen.height-800*scale)*.5f,0),Quaternion.identity,Vector3.one*scale);
            GUI.enabled=options==null||!options.Open;
            try{
                Fill(new Rect(84,49,4,70),Gold);
                Fill(new Rect(84,49,17,2),Gold);
                Fill(new Rect(84,117,17,2),Gold);
                GUI.Label(new Rect(108,44,700,60),"DiceFree",title);
                GUI.Label(new Rect(111,104,700,30),"An Echo beyond the dice",body);
                GUI.Label(new Rect(905,69,280,26),"YOUR ECHO  /  YOUR JOURNEY",small);
                Fill(new Rect(84,138,1112,1),new Color(.26f,.28f,.30f));
                DrawEchoSidebar();
                if(loadBlocked){GUI.Label(new Rect(390,205,700,130),"Your Echo could not be opened.\n"+status,body);if(Button(new Rect(390,360,200,40),"Retry"))Refresh();}
                else if(profile==null)DrawBeginning();
                else DrawTierCards();
                Fill(new Rect(84,713,1112,1),new Color(.17f,.21f,.26f));
                GUI.Label(new Rect(84,731,470,50),status,small);
                if(Button(new Rect(666,733,236,34),"Multiplayer development test"))DiceFree.Development.NetworkWorldLab.OpenSelection(this);
                if(Button(new Rect(916,733,280,34),"Development room"))TryOpenDevelopmentRoom();
            }finally{GUI.enabled=enabled;GUI.matrix=matrix;}
        }
        void DrawEchoSidebar(){
            Frame(new Rect(84,165,250,520));
            GUI.Label(new Rect(104,185,210,22),profile==null?"A NEW BEGINNING":"YOUR ECHO",small);
            GUI.Label(new Rect(104,219,210,70),profile==null?"Begin your\njourney":EchoIdentity.ReadName(profile),heading);
            bool canName=profile==null||EchoIdentity.CanRename(profile);
            if(canName&&!loadBlocked){
                GUI.Label(new Rect(104,304,210,22),"Echo name",small);
                echoName=GUI.TextField(new Rect(104,333,210,40),echoName,EchoIdentity.MaximumNameLength,nameField);
                if(profile!=null&&Button(new Rect(104,384,210,34),"Save name"))TryRenameEcho(echoName);
            }
            if(profile!=null){
                var active=Entries.FirstOrDefault(e=>e.active);
                GUI.Label(new Rect(104,443,210,70),Entries.Length+" manifestations\n"+(active!=null?"Last played: Level "+active.level:""),body);
                if(Button(new Rect(104,530,210,34),"Refresh saves"))Refresh();
            }
            GUI.Label(new Rect(104,582,210,38),"One Echo. Many paths.",small);
            if(Button(new Rect(104,628,210,36),"Options"))OpenOptions();
        }
        void DrawBeginning(){
            GUI.Label(new Rect(368,169,800,40),"Every Echo begins as a Novice",heading);
            Frame(new Rect(368,226,828,459));
            DrawPortraitLight(new Rect(404,259,270,338));
            var texture=portraitStage.Get("class.novice",null,menuPresentation);
            if(texture!=null)GUI.DrawTexture(new Rect(414,253,250,333),texture,ScaleMode.ScaleToFit,true);
            GUI.Label(new Rect(723,282,420,50),"The first step is yours.",heading);
            GUI.Label(new Rect(723,347,410,100),"Explore Cornberg, find your strength and discover the paths your Echo can take.",body);
            GUI.Label(new Rect(723,469,410,50),EchoIdentity.Format(echoName,classCatalog?.Resolve("class.novice")?.displayName??"Novice"),cardTitle);
            if(Button(new Rect(723,562,410,54),"Start adventure",true))TryStartNewEcho();
        }
        void DrawTierCards(){
            int max=Mathf.Max(5,Entries.Length==0?0:Entries.Max(e=>e.tier));
            var tiers=Enumerable.Range(0,max+1).Concat(Entries.Any(e=>e.tier<0)?new[]{-1}:Array.Empty<int>()).ToArray();
            float width=828f/tiers.Length;
            for(int i=0;i<tiers.Length;i++){
                var t=tiers[i];int count=EntriesForTier(t).Length;
                var old=GUI.backgroundColor;GUI.backgroundColor=t==SelectedTier?Gold:new Color(.22f,.27f,.33f);
                if(GUI.Button(new Rect(368+i*width,165,width-4,42),TierName(t)+(count>0?"  "+count:""),tab))SelectTier(t);
                GUI.backgroundColor=old;
            }
            var entries=EntriesForTier(SelectedTier);
            var locked=LockedClassIdsForTier(SelectedTier);
            int cardCount=entries.Length+locked.Length;
            GUI.Label(new Rect(368,225,800,35),TierName(SelectedTier)+"  /  "+(SelectedTier==0?"The beginning":SelectedTier<0?"Preserved saves":"Choose your manifestation"),heading);
            if(cardCount==0){Frame(new Rect(368,282,828,403));GUI.Label(new Rect(408,381,748,100),"No manifestations in this tier yet.\nAdvance your Echo to discover a new path.",body);return;}
            Rect viewport=new(368,278,828,407);float cardWidth=cardCount==1?404:396;
            float contentHeight=Mathf.Ceil(cardCount/2f)*404;
            scroll=GUI.BeginScrollView(viewport,scroll,new Rect(0,0,808,Mathf.Max(405,contentHeight)));
            for(int i=0;i<cardCount;i++){
                float x=(i%2)*408,y=(i/2)*404;
                var card=new Rect(x,y,cardWidth,394);
                if (i>=entries.Length) { DrawLockedClass(card,locked[i-entries.Length]); continue; }
                var entry=entries[i];
                Frame(card,entry.active);
                GUI.Label(new Rect(x+18,y+15,cardWidth-36,52),entry.displayName,cardTitle);
                var state=ManifestationRoster.ReadManifestation(profile,entry.classId);
                var texture=entry.available?portraitStage.Get(entry.classId,state,menuPresentation):null;
                if(texture!=null)
                {
                    DrawPortraitLight(new Rect(x+77,y+59,242,242));
                    GUI.DrawTexture(new Rect(x+102,y+69,192,232),texture,ScaleMode.ScaleToFit,true);
                }
                else GUI.Label(new Rect(x+25,y+122,cardWidth-50,100),"Class preview unavailable",small);
                GUI.Label(new Rect(x+18,y+294,cardWidth-36,25),(classCatalog?.Resolve(entry.classId)?.displayName??entry.classId)+" · Level "+entry.level,small);
                GUI.Label(new Rect(x+18,y+320,cardWidth-36,23),entry.active?"Last active manifestation":"Preserved manifestation",small);
                bool previousEnabled=GUI.enabled;GUI.enabled&=entry.available;
                if(Button(new Rect(x+18,y+350,cardWidth-36,34),entry.available?"Enter world":"Content unavailable",true))TryPlayManifestation(entry.classId);
                GUI.enabled=previousEnabled;
            }
            GUI.EndScrollView();
        }

        void DrawLockedClass(Rect card, string classId)
        {
            Frame(card);
            GUI.Label(new Rect(card.x+18,card.y+15,card.width-36,52),
                classCatalog?.Resolve(classId)?.displayName ?? "Undiscovered class",cardTitle);
            var preview = portraitStage.Get(classId,null,menuPresentation) ??
                portraitStage.Get("class.novice",null,menuPresentation);
            if (preview != null)
            {
                var rect = new Rect(card.x+102,card.y+65,192,245);
                DrawPortraitLight(new Rect(card.x+77,card.y+59,242,248));
                var original = GUI.color;
                GUI.color = new Color(.30f,.41f,.57f);
                GUI.DrawTexture(new Rect(rect.x-2,rect.y-2,rect.width+4,rect.height+4),preview,ScaleMode.ScaleToFit,true);
                GUI.color = new Color(.016f,.023f,.036f);
                GUI.DrawTexture(rect,preview,ScaleMode.ScaleToFit,true);
                GUI.color = original;
            }
            GUI.Label(new Rect(card.x+18,card.y+341,card.width-36,26),"Advance your Echo to unlock",small);
        }
    }
}
