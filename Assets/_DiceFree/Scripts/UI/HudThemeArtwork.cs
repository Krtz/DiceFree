using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.UI
{
    // Optional licensed local UI pack art. Themes still work on GitHub clones
    // without third-party files, using their palette and procedural chrome.
    public static class HudThemeArtwork
    {
        private static readonly Dictionary<string,Texture2D> loaded = new();
        private static readonly HashSet<string> slotThemes = new()
        {
            "pack-game-input-controller-icons-free",
            "pack-free-rpg-fantasy-spell-icons",
            "pack-battle-skill-icon",
            "pack-2d-rpg-button-7",
            "pack-simple-ui-elements",
            "pack-elemental-meters",
            "pack-spellbook-preface",
            "pack-magic-spells-40-fantasy-icons-512x512-hd",
            "pack-shields-40-fantasy-icons-512x512-hd",
            "pack-free-ui-pack"
        };
        private static Texture2D Find(string id)
        {
            if(string.IsNullOrEmpty(id) || !id.StartsWith("pack-"))return null;
            if(loaded.TryGetValue(id,out var found))return found;
            found=Resources.Load<Texture2D>("UiThemesLicensed/"+id);
            loaded[id]=found;
            return found;
        }
        public static Texture2D Panel(string id) => slotThemes.Contains(id)?null:Find(id);
        public static Texture2D Slot(string id) => slotThemes.Contains(id)?Find(id):null;
        public static bool HasArt(string id) => Find(id)!=null;
    }
}
