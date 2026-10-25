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
        static readonly Color Gold=new(.82f,.67f,.39f),Muted=new(.61f,.66f,.72f);
        void OnDestroy(){if(options!=null){options.ShowLauncher=true;options.QuitToDesktop=null;}}
        public int SelectedTier {get;private set;}
        public MenuPortraitStage PortraitStage=>portraitStage;
        public ManifestationEntry[] EntriesForTier(int tier)=>Entries.Where(e=>e.tier==tier).ToArray();
        public void SelectTier(int tier){SelectedTier=tier;scroll=Vector2.zero;}
        public void OpenOptions(OptionsPanel.OptionsTab page=OptionsPanel.OptionsTab.Gameplay){options.Show();options.SelectTab(page);}
        static string TierName(int tier)=>tier switch{0=>"Tier 0",1=>"Tier I",2=>"Tier II",3=>"Tier III",4=>"Tier IV",5=>"Tier V",-1=>"Unavailable",_=>"Tier "+tier};
        void Styles(){
            if(title!=null)return;
            GUIStyle Text(int size,Color color,bool bold=false)=>new(GUI.skin.label){fontSize=size,wordWrap=true,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,normal={textColor=color},richText=false};
            title=Text(44,Color.white,true);heading=Text(23,Color.white,true);body=Text(15,new Color(.85f,.88f,.91f));
            small=Text(12,Muted);cardTitle=Text(18,Color.white,true);
            button=new GUIStyle(GUI.skin.button){fontSize=15,fontStyle=FontStyle.Bold,padding=new RectOffset(10,10,7,7)};
            tab=new GUIStyle(button){fontSize=13};nameField=new GUIStyle(GUI.skin.textField){fontSize=18,padding=new RectOffset(10,10,8,8)};
        }
        static void Fill(Rect r,Color c)=>HudChrome.Fill(r,c);
        static void Frame(Rect r,bool selected=false){
            Fill(r,selected?new Color(.39f,.33f,.22f):new Color(.19f,.24f,.29f));
            Fill(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),new Color(.055f,.073f,.091f));
        }
        bool Button(Rect r,string label,bool primary=false){var old=GUI.backgroundColor;GUI.backgroundColor=primary?new Color(.48f,.39f,.22f):new Color(.24f,.30f,.37f);bool hit=GUI.Button(r,label,button);GUI.backgroundColor=old;return hit;}
        void OnGUI(){
            Styles();var matrix=GUI.matrix;bool enabled=GUI.enabled;
            Fill(new Rect(0,0,Screen.width,Screen.height),new Color(.019f,.028f,.041f));
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/800f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)*.5f,(Screen.height-800*scale)*.5f,0),Quaternion.identity,Vector3.one*scale);
            GUI.enabled=options==null||!options.Open;
            try{
                Fill(new Rect(84,49,4,70),Gold);
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
            GUI.Label(new Rect(368,225,800,35),TierName(SelectedTier)+"  /  "+(SelectedTier==0?"The beginning":SelectedTier<0?"Preserved saves":"Choose your manifestation"),heading);
            if(entries.Length==0){Frame(new Rect(368,282,828,403));GUI.Label(new Rect(408,381,748,100),"No manifestations in this tier yet.\nAdvance your Echo to discover a new path.",body);return;}
            Rect viewport=new(368,278,828,407);float cardWidth=entries.Length==1?404:396;
            float contentHeight=Mathf.Ceil(entries.Length/2f)*404;
            scroll=GUI.BeginScrollView(viewport,scroll,new Rect(0,0,808,Mathf.Max(405,contentHeight)));
            for(int i=0;i<entries.Length;i++){
                var entry=entries[i];float x=(i%2)*408,y=(i/2)*404;var card=new Rect(x,y,cardWidth,394);Frame(card,entry.active);
                GUI.Label(new Rect(x+18,y+15,cardWidth-36,52),entry.displayName,cardTitle);
                var state=ManifestationRoster.ReadManifestation(profile,entry.classId);
                var texture=entry.available?portraitStage.Get(entry.classId,state,menuPresentation):null;
                if(texture!=null)GUI.DrawTexture(new Rect(x+102,y+69,192,232),texture,ScaleMode.ScaleToFit,true);
                else GUI.Label(new Rect(x+25,y+122,cardWidth-50,100),"Class preview unavailable",small);
                GUI.Label(new Rect(x+18,y+294,cardWidth-36,25),"Level "+entry.level,body);
                GUI.Label(new Rect(x+18,y+320,cardWidth-36,23),entry.active?"Last active manifestation":"Preserved manifestation",small);
                bool previousEnabled=GUI.enabled;GUI.enabled&=entry.available;
                if(Button(new Rect(x+18,y+350,cardWidth-36,34),entry.available?"Enter world":"Content unavailable",true))TryPlayManifestation(entry.classId);
                GUI.enabled=previousEnabled;
            }
            GUI.EndScrollView();
        }
    }
}
