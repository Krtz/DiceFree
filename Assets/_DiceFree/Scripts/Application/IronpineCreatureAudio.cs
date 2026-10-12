using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.Ironpine
{
    // Local Animal SFX may be supplied from licensed Asset Store packs on the owner's
    // machine. They are intentionally gitignored; all effects have original generated
    // fallback audio so the open-source project and builds remain functional.
    [DisallowMultipleComponent]
    public sealed class IronpineCreatureAudio : MonoBehaviour
    {
        private static readonly Dictionary<string,AudioClip> Generated = new();
        private AudioSource source;
        private WildlifeKind kind;
        private AudioClip bark, growl, pack;
        private float nextAllow;
        private void Awake()
        {
            var animal=GetComponent<IronpineWildlife>();
            kind=animal==null?WildlifeKind.GreyWolf:animal.Kind;
            source=GetComponent<AudioSource>();
            if(source==null) source=gameObject.AddComponent<AudioSource>();
            source.playOnAwake=false;
            source.spatialBlend=1f;
            source.rolloffMode=AudioRolloffMode.Logarithmic;
            source.minDistance=2.2f;
            source.maxDistance=28f;
            source.volume=.62f;
            bark=Resources.Load<AudioClip>("Audio/IronpineLicensed/WolfBark");
            growl=Resources.Load<AudioClip>("Audio/IronpineLicensed/WolfGrowl");
            pack=Resources.Load<AudioClip>("Audio/IronpineLicensed/WolfPack");
        }
        public void Cue(string action)
        {
            if (!isActiveAndEnabled || source==null || Time.time<nextAllow) return;
            source.volume=.62f*DiceFree.Characters.PlayerAudioDirector.WildlifeGain;
            bool rat=kind==WildlifeKind.Quillrat;
            AudioClip clip=null;
            float volume=.40f;
            switch(action)
            {
                case "Howl":
                    // Distinct rising/falling note plus optionally a quiet natural pack layer.
                    clip=Fallback("howl"); volume=kind==WildlifeKind.AlphaWolf?.70f:.55f;
                    if(pack!=null) source.PlayOneShot(pack,.15f);
                    nextAllow=Time.time+1.9f;
                    break;
                case "Bite":
                    clip=rat?Fallback("squeak"):(UnityEngine.Random.value<.55f?bark:growl);
                    clip??=Fallback("growl");volume=rat?.18f:.30f;
                    nextAllow=Time.time+.65f;
                    break;
                case "QuillShot":
                    clip=Fallback("quill");volume=.22f; nextAllow=Time.time+.40f;
                    break;
                case "Scurry":
                case "Scratch":
                case "Forage":
                case "Sniff":
                    clip=Fallback(rat?"rustle":"sniff");volume=.12f;
                    nextAllow=Time.time+1.4f;
                    break;
                case "Hurt":
                    clip=rat?Fallback("squeak"):growl??Fallback("growl");
                    volume=rat?.2f:.25f;nextAllow=Time.time+.4f;
                    break;
                case "Death":
                    clip=Fallback(rat?"squeak":"growl");volume=.24f;
                    nextAllow=Time.time+2f;
                    break;
                case "Pounce":
                    clip=rat?Fallback("rustle"):growl??Fallback("growl");
                    volume=.32f;nextAllow=Time.time+.9f;
                    break;
                default:return;
            }
            if (clip==null) return;
            source.pitch=UnityEngine.Random.Range(rat?.91f:.87f,rat?1.17f:1.08f);
            source.PlayOneShot(clip,volume);
        }
        private static AudioClip Fallback(string id)
        {
            if(Generated.TryGetValue(id,out var c))return c;
            const int rate=22050;
            float seconds=id=="howl"?2.5f:id=="growl"?.80f:.23f;
            int n=Mathf.CeilToInt(rate*seconds);
            var data=new float[n];
            var rand=new System.Random(id.GetHashCode());
            double phase=0;
            for(int i=0;i<n;i++)
            {
                float t=i/(float)rate;
                float r=t/seconds;
                float env=Mathf.Min(1f,t/.055f)*Mathf.Clamp01((seconds-t)/.11f);
                float noise=(float)(rand.NextDouble()*2-1);
                float frequency=id switch
                {
                    "howl"=>245f+125f*Mathf.Sin(Mathf.PI*r*.9f)+26f*Mathf.Sin(r*9f),
                    "growl"=>83f+22f*Mathf.Sin(r*15f),
                    "squeak"=>390f+290f*Mathf.Sin(Mathf.PI*r),
                    "quill"=>920f-570f*r,
                    "sniff"=>125f+40f*Mathf.Sin(r*12f),
                    _=>250f
                };
                phase+=Math.PI*2*frequency/rate;
                float vowel=(float)(Math.Sin(phase)+.32*Math.Sin(phase*2.02)+
                  .15*Math.Sin(phase*3.03));
                float buzz=id is "growl" or "sniff" or "rustle"?.36f:
                    id=="howl"?.075f:.11f;
                data[i]=Mathf.Clamp((vowel*(1-buzz)+noise*buzz)*env*.45f,-.9f,.9f);
            }
            c=AudioClip.Create("Ironpine original "+id,n,1,rate,false);
            c.SetData(data,0);Generated.Add(id,c);
            return c;
        }
    }
}
