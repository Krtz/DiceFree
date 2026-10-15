using System;
using System.Linq;
using UnityEngine;
namespace DiceFree.UI
{
    public static class CreditsView
    {
        static CreditBook book;
        static CreditBook.Entry[] visible;
        static string search="";
        public static CreditBook Book=>book??=Resources.Load<CreditBook>("Credits/CreditBook");
        public static CreditBook.Entry[] Entries=>visible??=Book?.entries.Where(e=>e.resourceProbes.Length==0||
            e.resourceProbes.Any(p=>Resources.Load<UnityEngine.Object>(p)!=null)).ToArray()??Array.Empty<CreditBook.Entry>();
        public static void Draw(){
            GUILayout.Label("DiceFree",new GUIStyle(GUI.skin.label){fontSize=26,fontStyle=FontStyle.Bold});
            GUILayout.Label("Design & development — Axel Kratz");
            GUILayout.Space(10);GUILayout.Box(GUIContent.none,GUILayout.ExpandWidth(true),GUILayout.Height(1));GUILayout.Space(10);
            GUILayout.Label("Asset creators & licenses",new GUIStyle(GUI.skin.label){fontSize=18,fontStyle=FontStyle.Bold});
            GUILayout.Label("Search credits");search=GUILayout.TextField(search);GUILayout.Space(6);
            var text=new GUIStyle(GUI.skin.label){wordWrap=true,richText=false};
            foreach(var entry in Entries.Where(e=>string.IsNullOrWhiteSpace(search)||
                (e.title+" "+e.creator+" "+e.category).IndexOf(search,StringComparison.OrdinalIgnoreCase)>=0)){
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(entry.title+" — "+entry.creator,new GUIStyle(text){fontStyle=FontStyle.Bold});
                if(!string.IsNullOrEmpty(entry.category))GUILayout.Label(entry.category,text);
                GUILayout.Label(entry.licenseName,text);
                if(!string.IsNullOrWhiteSpace(entry.notice))GUILayout.Label(entry.notice,text);
                GUILayout.BeginHorizontal();
                if(SafeLink(entry.sourceUrl)&&GUILayout.Button("Creator / source",GUILayout.Width(160)))Application.OpenURL(entry.sourceUrl);
                if(SafeLink(entry.licenseUrl)&&GUILayout.Button("License",GUILayout.Width(130)))Application.OpenURL(entry.licenseUrl);
                GUILayout.EndHorizontal();GUILayout.EndVertical();GUILayout.Space(5);
            }
            if(Book!=null)foreach(var notice in Book.notices.Where(n=>n!=null)){
                GUILayout.Space(10);GUILayout.Label(notice.name,new GUIStyle(text){fontStyle=FontStyle.Bold});GUILayout.Label(notice.text,text);
            }
        }
        static bool SafeLink(string value)=>Uri.TryCreate(value,UriKind.Absolute,out var url)&&url.Scheme==Uri.UriSchemeHttps;
    }
}
