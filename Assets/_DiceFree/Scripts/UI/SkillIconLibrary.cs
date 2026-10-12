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
