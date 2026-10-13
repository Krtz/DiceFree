using System;
using System.IO;
using System.Reflection;
using DiceFree.Input;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class HealthChannelUiValidation
    {
        [CliCommand("dicefree.ui.health-channel.validate",
            "Validate independent overhead HP settings, old-settings migration and generic channel contracts.")]
        public static object Run()
        {
            void Check(bool ok,string message)
            {
                if(!ok)throw new InvalidOperationException("HEALTH_CHANNEL_UI: "+message);
            }
            Check(OverheadUnitHealthBars.ShouldShow(
                OverheadHealthVisibility.Always,100f,100f),
                "Always must include fully healed units.");
            Check(OverheadUnitHealthBars.ShouldShow(
                OverheadHealthVisibility.WhenHurt,24f,100f) &&
                !OverheadUnitHealthBars.ShouldShow(
                    OverheadHealthVisibility.WhenHurt,100f,100f),
                "Only when hurt must track current/maximum HP.");
            Check(!OverheadUnitHealthBars.ShouldShow(
                    OverheadHealthVisibility.Never,1f,100f),
                "Never must hide injured and healthy units.");
            Check(typeof(IChannelingAbility).IsAssignableFrom(
                    typeof(ReturnHomeAbility)),
                "Teleport doesn't implement reusable channel UI contract.");

            // Write v1 settings to a fresh temporary directory; never touch
            // the user's normal gameplay.json or persistent Echo files.
            string root=Path.Combine(Path.GetTempPath(),
                "DiceFree-HealthUi-Test-"+Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                string file=Path.Combine(root,"gameplay.json");
                File.WriteAllText(file,
                    "{\"version\":1,\"autoRetaliate\":false,"+
                    "\"outOfRangeSkillBehavior\":0,\"showCastRange\":false}");
                var ctor=typeof(GameplayPreferences).GetConstructor(
                    BindingFlags.Instance|BindingFlags.NonPublic,
                    null,new[]{typeof(string)},null);
                Check(ctor!=null,"Settings constructor not found.");
                GameplayPreferences Open()=>
                    (GameplayPreferences)ctor.Invoke(new object[]{root});
                var saved=Open();
                Check(saved.MinimapOrientation==MinimapOrientation.NorthUp,
                    "Legacy v1 settings must default to north-up minimap.");
                Check(!saved.AutoRetaliate && !saved.ShowCastRange &&
                    saved.OutOfRangeSkillBehavior==OutOfRangeSkillBehavior.DoNothing,
                    "Legacy options changed during v1→v2 migration.");
                Check(saved.SelfHealthBar==OverheadHealthVisibility.WhenHurt &&
                    saved.AllyHealthBar==OverheadHealthVisibility.WhenHurt &&
                    saved.EnemyHealthBar==OverheadHealthVisibility.Always,
                    "Legacy settings didn't receive the correct new defaults.");
                saved.SetOverheadHealthVisibility(
                    OverheadHealthVisibility.Always,
                    OverheadHealthVisibility.Never,
                    OverheadHealthVisibility.WhenHurt);
                saved.SetMinimapOrientation(MinimapOrientation.CameraUp);
                var reloaded=Open();
                Check(reloaded.MinimapOrientation==MinimapOrientation.CameraUp,
                    "Minimap orientation did not persist.");
                Check(reloaded.SelfHealthBar==OverheadHealthVisibility.Always &&
                    reloaded.AllyHealthBar==OverheadHealthVisibility.Never &&
                    reloaded.EnemyHealthBar==OverheadHealthVisibility.WhenHurt,
                    "Independent choices not persisted/reloaded.");
                Check(File.ReadAllText(file).Contains("\"version\": 3") ||
                      File.ReadAllText(file).Contains("\"version\":3"),
                    "Overhead settings JSON version wasn't upgraded.");
                Check(!reloaded.AutoRetaliate && !reloaded.ShowCastRange,
                    "Saving new options overwrote unrelated old preferences.");
            }
            finally{Directory.Delete(root,true);}
            Debug.Log("DICEFREE_HEALTH_CHANNEL_DATA_OK: channels, overhead HP visibility, v1 gameplay prefs migrated/persisted");
            return new{success=true,
                finalMarker="DICEFREE_HEALTH_CHANNEL_DATA_OK",
                selfAllyEnemyModes=3,channelContract=true,
                oldSettingsMigrated=true};
        }
    }
}
