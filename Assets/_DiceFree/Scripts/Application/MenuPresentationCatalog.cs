using System;
using DiceFree.Items;
using UnityEngine;
namespace DiceFree.Advancement
{
    [CreateAssetMenu(menuName="DiceFree/UI/Menu presentation catalog")]
    public sealed class MenuPresentationCatalog : ScriptableObject
    {
        [Serializable] public sealed class Form { public string classId; public int tier; public GameObject preview; }
        public Form[] forms=Array.Empty<Form>();
        public ItemDefinition[] items=Array.Empty<ItemDefinition>();
        public Form Resolve(string id)=>Array.Find(forms,f=>f!=null&&f.classId==id);
        public int TierOf(string id)=>Resolve(id)?.tier??-1;
    }
}
