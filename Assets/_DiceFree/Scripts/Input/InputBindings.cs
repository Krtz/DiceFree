using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace DiceFree.Input
{
    /// <summary>Owns one mutable runtime clone of the authored actions and their binding overrides.</summary>
    public sealed class InputBindings : IDisposable
    {
        public sealed class Entry
        {
            public InputAction Action { get; }
            public int Index { get; }
            public string Map => Action.actionMap.name;
            public string Label => Action.name + (Action.bindings[Index].isPartOfComposite ? " / " + Action.bindings[Index].name : "");
            public string Display => InputBindings.Display(Action, Index);
            public string Path => Action.bindings[Index].effectivePath;
            internal Entry(InputAction action, int index) { Action = action; Index = index; }
        }

        private static InputBindings current;
        public static InputBindings Current => current ??= new InputBindings(Resources.Load<InputActionAsset>("DiceFreeControls"), SettingsDirectory);

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
                return System.IO.Path.Combine(Application.persistentDataPath, "Settings");
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetSession()
        {
            current?.Dispose();
            current = null;
        }

        public InputActionAsset Asset { get; }
        public IReadOnlyList<Entry> Entries { get; }
        public bool Suppressed { get; private set; }
        public bool Listening => operation != null;
        public string ListeningLabel { get; private set; }
        public string Status { get; private set; }
        public int LastRebindFrame { get; private set; } = -1;
        public event Action SuppressionStarted;

        private readonly BindingSettingsStore store;
        private InputActionRebindingExtensions.RebindingOperation operation;
        private bool rebindActionWasEnabled;
        private bool modal;
        private int resumeAfter;
        private bool disposed;

        public InputBindings(InputActionAsset authored, string settingsDirectory)
        {
            if (authored == null) throw new InvalidOperationException("Missing authored DiceFreeControls input asset.");
            Asset = UnityEngine.Object.Instantiate(authored);
            store = settingsDirectory == null ? null : new BindingSettingsStore(settingsDirectory);

            var entries = new List<Entry>();
            foreach (var map in Asset.actionMaps)
            foreach (var action in map.actions)
            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (binding.isComposite) continue;
                if (action.name == "Zoom" || action.name == "Camera pointer delta" || action.name == "Close options") continue;
                entries.Add(new Entry(action, i));
            }

            Entries = entries;
            Load();
            Asset.Enable();
        }

        public InputAction Action(string path) => Asset.FindAction(path, true);

        public static string Display(InputAction action, int bindingIndex = 0)
        {
            string path = action.bindings[bindingIndex].effectivePath;
            return string.IsNullOrEmpty(path)
                ? ""
                : InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice);
        }

        private void Load()
        {
            if (store == null)
            {
                Status = "Bindings ready (isolated session).";
                return;
            }

            Exception lastError = null;
            foreach (string candidatePath in store.CandidatePaths())
            {
                try
                {
                    string json = store.Read(candidatePath);
                    var candidate = UnityEngine.Object.Instantiate(Asset);
                    try
                    {
                        candidate.RemoveAllBindingOverrides();
                        candidate.LoadBindingOverridesFromJson(json);
                    }
                    finally { DestroyAsset(candidate); }

                    Asset.RemoveAllBindingOverrides();
                    Asset.LoadBindingOverridesFromJson(json);
                    Status = candidatePath == store.Path ? "Bindings loaded." : "Bindings recovered from backup.";
                    return;
                }
                catch (Exception error) when (error is IOException || error is UnauthorizedAccessException ||
                                              error is ArgumentException || error is InvalidOperationException ||
                                              error is FormatException)
                {
                    lastError = error;
                    Asset.RemoveAllBindingOverrides();
                }
            }

            Status = lastError == null
                ? "Bindings ready."
                : "Settings unreadable; defaults active. Existing files preserved: " + lastError.Message;
        }

        public void Save()
        {
            try
            {
                store?.Write(Asset.SaveBindingOverridesAsJson());
                Status = store == null ? "Bindings changed (isolated session)." : "Bindings saved.";
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                Status = "Bindings active, but could not save: " + error.Message;
            }
        }

        public void ResetDefaults()
        {
            CancelRebind();
            Asset.RemoveAllBindingOverrides();
            Save();
        }

        public void ApplyOverride(Entry entry, string path)
        {
            if (!Entries.Contains(entry)) throw new ArgumentException("Entry belongs to another input asset.");
            bool wasEnabled = entry.Action.enabled;
            if (wasEnabled) entry.Action.Disable();
            entry.Action.ApplyBindingOverride(entry.Index, path);
            if (wasEnabled) entry.Action.Enable();
        }

        public string Conflict(Entry entry)
        {
            if (string.IsNullOrEmpty(entry.Path)) return "";
            var others = Entries
                .Where(other => other != entry && string.Equals(other.Path, entry.Path, StringComparison.OrdinalIgnoreCase))
                .Select(other => other.Label)
                .ToArray();
            return others.Length == 0 ? "" : "Shared binding: " + string.Join(", ", others);
        }

        public void SetModal(bool open)
        {
            modal = open;
            if (open)
            {
                if (Suppressed) return;
                Suppressed = true;
                Asset.FindActionMap("Gameplay", true).Disable();
                Asset.FindActionMap("Camera", true).Disable();
                SuppressionStarted?.Invoke();
                return;
            }

            resumeAfter = Time.frameCount + 1;
            if (!Application.isPlaying) Resume();
        }

        public void Tick()
        {
            if (!Suppressed || modal || Listening || Time.frameCount <= resumeAfter) return;
            if (InputSystem.devices.Any(device =>
                    (device is Keyboard || device is Mouse) &&
                    device.allControls.OfType<ButtonControl>().Any(button => button.isPressed)))
                return;
            Resume();
        }

        private void Resume()
        {
            Suppressed = false;
            Asset.FindActionMap("Gameplay", true).Enable();
            Asset.FindActionMap("Camera", true).Enable();
        }

        public void BeginRebind(Entry entry)
        {
            if (!Entries.Contains(entry)) throw new ArgumentException("Entry belongs to another input asset.");
            CancelRebind();
            SetModal(true);
            rebindActionWasEnabled = entry.Action.enabled;
            if (rebindActionWasEnabled) entry.Action.Disable();
            ListeningLabel = entry.Label;

            try
            {
                operation = entry.Action.PerformInteractiveRebinding(entry.Index)
                    .WithExpectedControlType("Button")
                    .WithControlsExcluding("<Pointer>/position")
                    .WithControlsExcluding("<Pointer>/delta")
                    .WithControlsExcluding("<Mouse>/scroll")
                    .WithControlsExcluding("<Gamepad>")
                    .WithControlsExcluding("<Joystick>")
                    .WithControlsExcluding("<Touchscreen>")
                    .WithControlsExcluding("<Pen>")
                    .WithCancelingThrough("<Keyboard>/backspace")
                    .OnMatchWaitForAnother(0.1f)
                    .OnComplete(_ => FinishRebind(entry, true))
                    .OnCancel(_ => FinishRebind(entry, false));
                operation.Start();
            }
            catch
            {
                operation?.Dispose();
                operation = null;
                ListeningLabel = null;
                if (rebindActionWasEnabled) entry.Action.Enable();
                rebindActionWasEnabled = false;
                throw;
            }
        }

        private void FinishRebind(Entry entry, bool completed)
        {
            var finished = operation;
            operation = null;
            finished?.Dispose();
            LastRebindFrame = Time.frameCount;
            ListeningLabel = null;

            if (rebindActionWasEnabled) entry.Action.Enable();
            rebindActionWasEnabled = false;
            if (completed) Save();
            else Status = "Rebind canceled; binding unchanged.";
        }

        public void CancelRebind() => operation?.Cancel();

        private static void DestroyAsset(UnityEngine.Object asset)
        {
            if (asset == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(asset);
            else UnityEngine.Object.DestroyImmediate(asset);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            CancelRebind();
            Asset.Disable();
            DestroyAsset(Asset);
            SuppressionStarted = null;
        }
    }
}
