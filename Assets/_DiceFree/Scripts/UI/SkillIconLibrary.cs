using System;
using System.Collections.Generic;
using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>
    /// A stable icon identity per skill kind, independent of slot, class load order,
    /// or HUD theme. Battle Skill Icon pack images are optional LOCAL Resources.
    /// Portable original, code-drawn fantasy sigils make clone/builds readable too.
    /// </summary>
    public static class SkillIconLibrary
    {
        private const string Folder = "AbilityIconsLicensed/";
        private static readonly Dictionary<string, Texture2D> Cache = new();

        public static string Key(NoviceSkillKind kind) => kind switch
        {
            NoviceSkillKind.StrengthMeleeStun => "novice-stun",
            NoviceSkillKind.MagicSand => "magic-sand",
            NoviceSkillKind.AgilityAttackSpeedBuff => "novice-quickness",
            NoviceSkillKind.SpiritHeal => "novice-heal",
            _ => "novice-passive"
        };
        public static string Key(PhysicalSkillKind kind) => kind switch
        {
            PhysicalSkillKind.HeavyStrike => "heavy-strike",
            PhysicalSkillKind.Guard => "guard",
            PhysicalSkillKind.Quickening => "quickening",
            PhysicalSkillKind.ArrowRain => "arrow-rain",
            _ => "physical-passive"
        };
        public static string Key(MagicalSkillKind kind) => kind switch
        {
            MagicalSkillKind.MagicSand => "magic-sand",
            MagicalSkillKind.Mend => "mend",
            MagicalSkillKind.FireImbuement => "fire-imbuement",
            MagicalSkillKind.IceBurst => "ice-burst",
            _ => "magical-passive"
        };
        public static Texture2D Get(NoviceSkillKind kind) => Get(Key(kind));
        public static Texture2D Get(PhysicalSkillKind kind) => Get(Key(kind));
        public static Texture2D Get(MagicalSkillKind kind) => Get(Key(kind));
        public static bool HasLicensedArt(string key) =>
            Resources.Load<Texture2D>(Folder + key) != null;
        public static Texture2D Get(string key)
        {
            if(string.IsNullOrEmpty(key))return null;
            if(Cache.TryGetValue(key,out var image) && image!=null)return image;
            image=Resources.Load<Texture2D>(Folder+key);
            if(image==null)image=DrawFallback(key);
            Cache[key]=image;
            return image;
        }

        private static Texture2D DrawFallback(string key)
        {
            const int size=64;
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false)
            {
                name="DiceFree original "+key+" skill icon",
                filterMode=FilterMode.Bilinear,
                wrapMode=TextureWrapMode.Clamp
            };
            Color focus=key switch
            {
                "greatblade-widearc" => new Color(.96f,.70f,.28f),
                "greatblade-spindash" => new Color(.30f,.85f,.95f),
                "greatblade-overheadbreaker" => new Color(1f,.38f,.18f),
                "greatblade-battlecry" => new Color(.94f,.52f,.28f),
                "greatblade-greatswordmastery" => new Color(.78f,.84f,.96f),
                "greatblade-heavyrhythm" => new Color(.88f,.65f,.22f),
                "attack-command" => new Color(.90f,.67f,.27f),
                "interact" => new Color(.55f,.90f,.76f),
                "return-home" => new Color(.52f,.52f,.98f),
                "novice-stun" => new Color(.93f,.70f,.22f),
                "magic-sand" => new Color(.85f,.63f,.29f),
                "novice-quickness" => new Color(.22f,.91f,.70f),
                "novice-heal" => new Color(.42f,.99f,.65f),
                "heavy-strike" => new Color(.99f,.35f,.25f),
                "guard" => new Color(.40f,.78f,1f),
                "quickening" => new Color(.89f,.89f,.25f),
                "arrow-rain" => new Color(.65f,.79f,.96f),
                "mend" => new Color(.37f,.95f,.90f),
                "fire-imbuement" => new Color(1f,.52f,.17f),
                "ice-burst" => new Color(.51f,.79f,1f),
                _ => new Color(.74f,.54f,.97f)
            };
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++)
            for(int x=0;x<size;x++)
            {
                float u=(x-31.5f)/32f,v=(y-31.5f)/32f;
                float edge=Mathf.Clamp01(1f-Mathf.Max(Mathf.Abs(u),Mathf.Abs(v)));
                float glow=.05f+.10f*edge+.15f*Mathf.Pow(Mathf.Clamp01(1f-(u*u+v*v)),3);
                pixels[y*size+x]=new Color(.03f+focus.r*glow,
                    .045f+focus.g*glow,.08f+focus.b*glow,1f);
            }
            void Dot(float x,float y,float radius,float alpha=1f)
            {
                int x0=Mathf.Max(0,Mathf.FloorToInt(x-radius));
                int y0=Mathf.Max(0,Mathf.FloorToInt(y-radius));
                int x1=Mathf.Min(size-1,Mathf.CeilToInt(x+radius));
                int y1=Mathf.Min(size-1,Mathf.CeilToInt(y+radius));
                for(int iy=y0;iy<=y1;iy++)
                for(int ix=x0;ix<=x1;ix++)
                {
                    float dist=Vector2.Distance(new Vector2(ix,iy),new Vector2(x,y));
                    float strength=Mathf.Clamp01((radius-dist)*1.5f)*alpha;
                    int n=iy*size+ix;
                    pixels[n]=Color.Lerp(pixels[n],Color.Lerp(focus,Color.white,.45f),strength);
                }
            }
            void Stroke(float ax,float ay,float bx,float by,float width=2.3f)
            {
                int steps=Mathf.CeilToInt(Vector2.Distance(new Vector2(ax,ay),new Vector2(bx,by))*2);
                for(int i=0;i<=steps;i++)
                {
                    float t=i/(float)Mathf.Max(1,steps);
                    Dot(Mathf.Lerp(ax,bx,t),Mathf.Lerp(ay,by,t),width);
                }
            }
            void Ring(float radius,int parts=96)
            {
                for(int i=0;i<parts;i++)
                {
                    float t=(i+0f)/parts*Mathf.PI*2;
                    Dot(32+radius*Mathf.Cos(t),32+radius*Mathf.Sin(t),1.4f,.62f);
                }
            }
            void Star(int rays=6)
            {
                for(int i=0;i<rays;i++)
                {
                    float a=i*Mathf.PI*2/rays;
                    Stroke(32,32,32+24*Mathf.Cos(a),32+24*Mathf.Sin(a),1.7f);
                }
                Dot(32,32,5);
            }
            switch(key)
            {
                case "greatblade-widearc":
                    Stroke(21,12,43,48,4);Stroke(14,24,36,11,3);
                    for(int i=0;i<50;i++){float a=Mathf.Lerp(-.2f,2.4f,i/49f);Dot(32+23*Mathf.Cos(a),24+23*Mathf.Sin(a),2);}
                    break;
                case "greatblade-spindash":
                    for(int i=0;i<120;i++){float a=i*.09f,r=5+i*.15f;Dot(32+r*Mathf.Cos(a),32+r*Mathf.Sin(a),2);}
                    Stroke(42,12,53,20,3);Stroke(53,20,42,23,3);break;
                case "greatblade-overheadbreaker":
                    Stroke(32,48,32,17,5);Stroke(21,39,43,39,3);
                    Stroke(32,13,16,7,2);Stroke(32,13,48,7,2);Stroke(32,13,32,4,2);break;
                case "greatblade-battlecry":
                    Stroke(15,32,45,46,5);Stroke(15,32,45,18,5);Stroke(45,18,45,46,3);
                    Stroke(21,27,24,14,3);Stroke(50,26,58,22,2);Stroke(51,33,60,33,2);Stroke(50,40,58,44,2);break;
                case "greatblade-greatswordmastery":
                    Ring(23);Stroke(32,12,32,52,4);Stroke(20,25,44,25,3);
                    Dot(32,52,5);break;
                case "greatblade-heavyrhythm":
                    Stroke(8,30,19,30,2);Stroke(19,30,24,45,2);Stroke(24,45,32,15,2);
                    Stroke(32,15,40,39,2);Stroke(40,39,46,30,2);Stroke(46,30,57,30,2);Ring(24);break;
                case "attack-command":
                    Stroke(16,15,46,45,5);Stroke(21,39,39,20,4);
                    Dot(45,45,7);break;
                case "interact":
                    Ring(22);Dot(31,34,8);
                    for(int i=0;i<4;i++)Dot(19+i*8,47,4);
                    break;
                case "return-home":
                    Ring(23);Ring(13);
                    Dot(32,32,7);break;
                case "novice-heal":
                case "mend":
                    Ring(23);
                    Stroke(32,14,32,50,5);
                    Stroke(14,32,50,32,5);
                    Dot(32,32,6);
                    break;
                case "guard":
                    Stroke(32,9,50,18,3);Stroke(50,18,47,43,3);
                    Stroke(47,43,32,55,3);Stroke(32,55,17,43,3);
                    Stroke(17,43,14,18,3);Stroke(14,18,32,9,3);
                    Stroke(32,17,32,44,2);break;
                case "fire-imbuement":
                    Stroke(22,17,28,43,7);Stroke(28,43,39,17,7);
                    Stroke(39,17,43,37,5);Dot(33,33,7);break;
                case "ice-burst":Star(8);Ring(19);break;
                case "arrow-rain":
                    for(int i=0;i<3;i++)
                    {
                        int x=18+14*i;
                        Stroke(x,50,x,16,3);Stroke(x,16,x-6,25,3);
                        Stroke(x,16,x+6,25,3);
                    }
                    break;
                case "quickening":
                case "novice-quickness":
                    for(int i=0;i<3;i++)
                    {
                        int y=17+15*i;Stroke(13,y,53,y,2);
                        Stroke(43,y-8,53,y,3);Stroke(43,y+8,53,y,3);
                    }
                    break;
                case "heavy-strike":
                case "novice-stun":
                    Stroke(13,15,44,46,7);Ring(13);Dot(40,43,5);break;
                case "magic-sand":
                    for(int i=0;i<8;i++)
                    {
                        float a=(i+0.5f)*Mathf.PI/4;
                        Dot(32+18*Mathf.Cos(a),32+18*Mathf.Sin(a),2+i%3);
                    }
                    Ring(11);Dot(32,32,7);break;
                default:Star();Ring(23);break;
            }
            texture.SetPixels(pixels);
            texture.Apply(false,true);
            return texture;
        }
    }
}
