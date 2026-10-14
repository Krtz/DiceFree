using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace DiceFree.Input
{
    public enum OutOfRangeSkillBehavior
    {
        DoNothing = 0,
        WalkIntoRange = 1
    }

    public enum OverheadHealthVisibility { Always = 0, WhenHurt = 1, Never = 2 }
    public enum MinimapOrientation { NorthUp = 0, CameraUp = 1 }
    public enum MinimapCompassCorner { Off = 0, BottomLeft = 1, BottomRight = 2, TopLeft = 3, TopRight = 4 }

    [Serializable]
    internal sealed class GameplayPreferencesData
    {
        public int version = 4;
        public bool autoRetaliate = true;
        public OutOfRangeSkillBehavior outOfRangeSkillBehavior = OutOfRangeSkillBehavior.WalkIntoRange;
        public bool showCastRange = true;
        public OverheadHealthVisibility selfHealthBar = OverheadHealthVisibility.WhenHurt;
        public OverheadHealthVisibility allyHealthBar = OverheadHealthVisibility.WhenHurt;
        public OverheadHealthVisibility enemyHealthBar = OverheadHealthVisibility.Always;
        public MinimapOrientation minimapOrientation = MinimapOrientation.NorthUp;
        public MinimapCompassCorner minimapCompassCorner = MinimapCompassCorner.BottomLeft;
    }

    public sealed class GameplayPreferences
    {
        private static GameplayPreferences current;
        public static GameplayPreferences Current => current ??= new GameplayPreferences(SettingsDirectory);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() => current = null;

        private static string SettingsDirectory
        {
            get
            {
#if UNITY_EDITOR
                string isolated = Environment.GetEnvironmentVariable("DICEFREE_EDITOR_SETTINGS_ROOT");
                if (!string.IsNullOrEmpty(isolated)) return isolated;
                if (Environment.GetEnvironmentVariable("DICEFREE_DISABLE_PERSISTENCE") == "1" ||
                    !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DICEFREE_EDITOR_SAVE_ROOT")))
                    return null;
#endif
                return Path.Combine(Application.persistentDataPath, "Settings");
            }
        }

        private readonly string path;
        private GameplayPreferencesData data = new();

        public bool AutoRetaliate => data.autoRetaliate;
        public OutOfRangeSkillBehavior OutOfRangeSkillBehavior => data.outOfRangeSkillBehavior;
        public bool ShowCastRange => data.showCastRange;
        public OverheadHealthVisibility SelfHealthBar => data.selfHealthBar;
        public OverheadHealthVisibility AllyHealthBar => data.allyHealthBar;
        public OverheadHealthVisibility EnemyHealthBar => data.enemyHealthBar;
        public MinimapOrientation MinimapOrientation => data.minimapOrientation;
        public MinimapCompassCorner MinimapCompassCorner => data.minimapCompassCorner;

        private GameplayPreferences(string directory)
        {
            path = directory == null ? null : Path.Combine(directory, "gameplay.json");
            Load();
        }

        public void SetAutoRetaliate(bool value)
        {
            if (data.autoRetaliate == value) return;
            data.autoRetaliate = value;
            Save();
        }

        public void SetOutOfRangeSkillBehavior(OutOfRangeSkillBehavior value)
        {
            if (data.outOfRangeSkillBehavior == value) return;
            data.outOfRangeSkillBehavior = value;
            Save();
        }

        public void SetShowCastRange(bool value)
        {
            if (data.showCastRange == value) return;
            data.showCastRange = value;
            Save();
        }

        public void SetMinimapOrientation(MinimapOrientation value)
        {
            if(data.minimapOrientation==value)return;
            data.minimapOrientation=value;
            Save();
        }

        public void SetMinimapCompassCorner(MinimapCompassCorner value)
        {
            if(!Enum.IsDefined(typeof(MinimapCompassCorner),value))throw new ArgumentOutOfRangeException(nameof(value));
            if(data.minimapCompassCorner==value)return;
            data.minimapCompassCorner=value;
            Save();
        }

        public void SetOverheadHealthVisibility(
            OverheadHealthVisibility self, OverheadHealthVisibility ally,
            OverheadHealthVisibility enemy)
        {
            if(data.selfHealthBar==self && data.allyHealthBar==ally &&
                data.enemyHealthBar==enemy)return;
            data.selfHealthBar=self;
            data.allyHealthBar=ally;
            data.enemyHealthBar=enemy;
            Save();
        }

        private void Load()
        {
            data = new GameplayPreferencesData();
            if (path == null || !File.Exists(path)) return;

            try
            {
                var loaded = JsonUtility.FromJson<GameplayPreferencesData>(File.ReadAllText(path));
                if(loaded!=null && (loaded.version>=1 && loaded.version<=4))
                {
                    // Unity's JsonUtility does not guarantee field initializers
                    // for missing fields: preserve the existing v1 settings,
                    // but explicitly initialize the newly introduced choices.
                    if(loaded.version==1)
                    {
                        loaded.selfHealthBar=OverheadHealthVisibility.WhenHurt;
                        loaded.allyHealthBar=OverheadHealthVisibility.WhenHurt;
                        loaded.enemyHealthBar=OverheadHealthVisibility.Always;
                    }
                    if(loaded.version<3)
                        loaded.minimapOrientation=MinimapOrientation.NorthUp;
                    if(loaded.version<4)
                        loaded.minimapCompassCorner=MinimapCompassCorner.BottomLeft;
                    if(!Enum.IsDefined(typeof(MinimapCompassCorner),loaded.minimapCompassCorner))
                        loaded.minimapCompassCorner=MinimapCompassCorner.BottomLeft;
                    loaded.version=4;
                    data=loaded;
                }
            }
            catch (Exception error) when (
                error is IOException ||
                error is UnauthorizedAccessException ||
                error is ArgumentException)
            {
                Debug.LogWarning("Gameplay settings unreadable; defaults active. Existing file preserved: " + error.Message);
            }
        }

        private void Save()
        {
            if (path == null) return;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string pending = path + ".pending";
                string backup = path + ".bak";
                byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true));
                using (var stream = new FileStream(pending, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                if (File.Exists(path)) File.Replace(pending, path, backup);
                else File.Move(pending, path);
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                Debug.LogWarning("Gameplay settings changed for this session but could not be saved: " + error.Message);
            }
        }
    }
}
