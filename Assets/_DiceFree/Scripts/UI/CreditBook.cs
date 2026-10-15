using System;
using UnityEngine;
namespace DiceFree.UI
{
    [CreateAssetMenu(menuName="DiceFree/UI/Credits")]
    public sealed class CreditBook : ScriptableObject
    {
        [Serializable] public sealed class Entry {
            public string title,creator,category,sourceUrl,licenseName,licenseUrl,notice;
            public string[] resourceProbes=Array.Empty<string>();
        }
        public Entry[] entries=Array.Empty<Entry>();
        public TextAsset[] notices=Array.Empty<TextAsset>();
    }
}
