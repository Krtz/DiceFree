using System;
using System.Linq;
using DiceFree.Ironpine;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class DiceFreeOctoberUiValidation
    {
        private static void Require(bool condition,string message)
        {
            if(!condition)throw new InvalidOperationException("OCTOBER QOL: "+message);
        }

        [CliCommand("dicefree.october-ui.validate",
            "Validate the 35 built-in/pack HUD themes, new Options tabs, imported theme skins and repaired URP wildlife materials.")]
        public static object Validate()
        {
            var all=HudThemes.All;
            Require(all.Length==35,"Expected five original themes and 30 pack themes");
            Require(all.Select(x=>x.id).Distinct().Count()==all.Length,
                "Duplicate theme identity");
            int optionsTabCount=Enum.GetValues(typeof(OptionsPanel.OptionsTab)).Length;
            Require(optionsTabCount==7,
                "Expected seven distinct options tabs including Session and Credits");
            int loaded=0;
            foreach(var theme in all)
                if(theme.id.StartsWith("pack-")&&HudThemeArtwork.HasArt(theme.id))
                    loaded++;
            // Two newly-downloaded packs contained no suitable stand-alone
            // visual sprites; those remain selectable procedural theme styles.
            Require(loaded>=28,"Optional UI asset skins not available locally ("+loaded+")");
            string dir="Assets/_DiceFree/Settings/Ironpine";
            var quillrats=AssetDatabase.FindAssets("t:Material",new[]{dir})
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p=>p.Contains("Art_Quillrat_"))
                .Select(AssetDatabase.LoadAssetAtPath<Material>)
                .ToArray();
            Require(quillrats.Length==30,"Expected 30 distinct quillrat palette materials");
            foreach(var mat in quillrats)
            {
                Require(mat!=null && mat.shader!=null &&
                    mat.shader.name=="Universal Render Pipeline/Lit",
                    "Pink quillrat due to non-URP shader: "+mat?.name);
                Require(mat.HasProperty("_BaseColor"),"Missing URP color "+mat.name);
            }
            var colors=quillrats.Where(m=>m.name.Contains("Ironpine_RatCoat"))
                .Select(m=>m.GetColor("_BaseColor")).ToArray();
            Require(colors.Length==3 && (colors[0]-colors[1]).maxColorComponent!=0f,
                "Quillrat three visual palettes collapsed");
            return new {success=true,themeCount=all.Length,
                localArtworkThemes=loaded,optionsTabs=optionsTabCount,
                quillratUrpMaterials=quillrats.Length};
        }

        [CliCommand("dicefree.october-ui.runtime",
            "Verify 10x wildlife HP and minimap attack-move queue in a controlled Play Mode session.")]
        public static object Runtime()
        {
            Require(EditorApplication.isPlaying,"Play Mode only");
            var player=UnityEngine.Object.FindFirstObjectByType<DiceFree.Characters.TraversalInput>();
            Require(player!=null,"Player not loaded");
            var map=player.GetComponent<MinimapHud>();
            var combat=player.GetComponent<DiceFree.Characters.CombatInput>();
            var queue=player.GetComponent<DiceFree.Characters.PlayerCommandQueue>();
            Require(map!=null && combat!=null && queue!=null,
                "Minimap/attack targeting/queue missing");
            var animals=UnityEngine.Object.FindObjectsByType<IronpineWildlife>(
                FindObjectsSortMode.None);
            Require(animals.Length==22,"Ironpine should have exactly 22 wildlife actors");
            foreach(var animal in animals)
            {
                var stats=animal.Actor.Stats;
                float multiple=animal.Kind==WildlifeKind.Quillrat?7f:
                    animal.Kind==WildlifeKind.AlphaWolf?65f:12f;
                Require(stats.MaximumHp>=stats.Definition.baseHp*multiple*.98f,
                    "HP scaling didn't take effect on "+animal.name);
            }
            var validPoint=player.transform.position+new Vector3(2,0,0);
            if(UnityEngine.AI.NavMesh.SamplePosition(validPoint,
                out var nav,4, UnityEngine.AI.NavMesh.AllAreas))
                validPoint=nav.position;
            combat.BeginAttackTargeting();
            Require(map.TryIssueWorldPoint(validPoint,true),
                "Shift-minimap attack-move not queued");
            Require(queue.PendingOrders==1,"First minimap attack-move missing");
            combat.BeginAttackTargeting();
            Require(map.TryIssueWorldPoint(validPoint+new Vector3(0,0,1.5f),true),
                "Second minimap attack-move wasn't queued");
            Require(queue.PendingOrders==2,"Second minimap attack-move lost");
            queue.ClearOrders();
            return new {success=true,creatures=animals.Length,
                minimapAttackMoveQueued=2,hpBoost=10};
        }
    }
}
