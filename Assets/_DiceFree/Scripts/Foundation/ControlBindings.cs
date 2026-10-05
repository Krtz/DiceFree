using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.Foundation
{
    [Serializable] public sealed class BindingRecord { public string id; public string path; }
    [Serializable] public sealed class ControlSettings { public int version = 1; public List<BindingRecord> bindings = new(); }
    public sealed class ControlBinding
    {
        public string Id, Label;
        public InputAction Action;
        public int Index;
        public bool Rebindable => !Id.StartsWith("zoom/", StringComparison.Ordinal);
        public string Path => Action.bindings[Index].effectivePath;
        public string Display => Action.GetBindingDisplayString(Index);
    }
    // Reset each session; focused action owners retain gameplay and enable/disable responsibility.
    public static class ControlBindings
    {
        private static readonly Dictionary<string, InputAction> actions = new();
        private static readonly List<ControlBinding> entries = new();
        private static readonly List<WeakReference<InputAction>> owners = new();
        public static IReadOnlyList<ControlBinding> Entries => entries;
        public static bool BlockGameplay { get; set; }
        public static event Action Changed;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Initialize()
        {
            foreach(var action in actions.Values) action.Dispose();
            actions.Clear(); entries.Clear(); owners.Clear(); Changed=null; BlockGameplay=false;
            var movement = new InputAction("Direct movement", InputActionType.Value);
            movement.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            actions.Add("movement", movement);
            var click = new InputAction("Move destination", binding: "<Mouse>/rightButton");
            actions.Add("click", click);
            var changeMode = new InputAction("Switch control mode", binding: "<Keyboard>/f6");
            actions.Add("changeMode", changeMode);
            var stop = new InputAction("Stop", binding: "<Keyboard>/space");
            actions.Add("stop", stop);
            var interact = new InputAction("Interact", binding: "<Keyboard>/i");
            actions.Add("interact", interact);
            var select = new InputAction("Select target", binding: "<Mouse>/leftButton");
            actions.Add("select", select);
            var cycle = new InputAction("Cycle hostile", binding: "<Keyboard>/tab");
            actions.Add("cycle", cycle);
            var attackSelected = new InputAction("Attack selected", binding: "<Keyboard>/x");
            actions.Add("attackSelected", attackSelected);
            var respawn = new InputAction("Return to anchor", binding: "<Keyboard>/r");
            actions.Add("respawn", respawn);
            var zoom = new InputAction("Zoom", InputActionType.Value, "<Mouse>/scroll/y");
            actions.Add("zoom", zoom);
            var vista = new InputAction("Look toward World 1", binding: "<Keyboard>/v");
            actions.Add("vista", vista);
            var pan = new InputAction("Camera pan", InputActionType.Value);
            pan.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            actions.Add("pan", pan);
            var drag = new InputAction("Camera drag", binding: "<Mouse>/middleButton");
            actions.Add("drag", drag);
            var pointerDelta = new InputAction("Camera pointer delta", InputActionType.Value, "<Mouse>/delta");
            actions.Add("pointerDelta", pointerDelta);
            var recenter = new InputAction("Recenter", binding: "<Keyboard>/home");
            actions.Add("recenter", recenter);
            var followToggle = new InputAction("Toggle follow", binding: "<Keyboard>/f");
            actions.Add("followToggle", followToggle);
            var rotate = new InputAction("Rotate camera", InputActionType.Value);
            rotate.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/q").With("Positive", "<Keyboard>/e");
            actions.Add("rotate", rotate);
            actions.Add("inventory", new InputAction("Inventory", binding: "<Keyboard>/b"));
            foreach(var pair in actions)
                for(int i=0;i<pair.Value.bindings.Count;i++)
                {
                    var binding=pair.Value.bindings[i];
                    if(binding.isComposite || pair.Key=="pointerDelta") continue;
                    string label=pair.Value.name;
                    if(pair.Key=="movement") label="Move " + (binding.name=="Up" ? "Forward" : binding.name=="Down" ? "Backward" : binding.name);
                    else if(binding.isPartOfComposite) label += " / " + binding.name;
                    if(pair.Key=="respawn") label="Return to Revive Point";
                    entries.Add(new ControlBinding { Id=pair.Key+"/"+i, Label=label, Action=pair.Value, Index=i });
                }
        }
        public static string Display(string id)
        {
            if(actions.Count==0) Initialize();
            return actions[id].GetBindingDisplayString();
        }
        public static InputAction Create(string id)
        {
            if(actions.Count==0) Initialize();
            var action=actions[id].Clone(); owners.Add(new WeakReference<InputAction>(action)); return action;
        }
        public static bool Allowed(string path)
        {
            if(string.IsNullOrEmpty(path)) return false;
            if(path.StartsWith("<Keyboard>/",StringComparison.OrdinalIgnoreCase))
            {
                // Validate against the layout, including when no keyboard is connected at startup.
                string name=path.Substring("<Keyboard>/".Length);
                var layout=InputSystem.LoadLayout("Keyboard");
                foreach(var control in layout.controls)
                    if(string.Equals(control.name.ToString(),name,StringComparison.OrdinalIgnoreCase))
                        return control.layout.ToString()=="Key" && !name.Equals("escape",StringComparison.OrdinalIgnoreCase);
                return false;
            }
            return path=="<Mouse>/leftButton" || path=="<Mouse>/rightButton" || path=="<Mouse>/middleButton" || path=="<Mouse>/forwardButton" || path=="<Mouse>/backButton";
        }
        public static bool Set(ControlBinding entry,string path,out string feedback)
        {
            feedback=null;
            if(entry==null || !entries.Contains(entry) || !entry.Rebindable || !Allowed(path)) { feedback="Choose a keyboard key or mouse button. Escape is reserved."; return false; }
            foreach(var other in entries)
                if(other.Id!=entry.Id && string.Equals(other.Path,path,StringComparison.OrdinalIgnoreCase))
                { feedback="Already assigned to " + other.Label + ". Rebind that control first."; return false; }
            entry.Action.ApplyBindingOverride(entry.Index,path); Publish(); return true;
        }
        public static void Release(InputAction action) { owners.RemoveAll(reference => !reference.TryGetTarget(out var owner) || owner == action); action.Dispose(); }
        public static bool Reset(ControlBinding entry,out string feedback)
        {
            feedback=null;
            if(entry==null || !entries.Contains(entry) || !entry.Rebindable) return false;
            string path=entry.Action.bindings[entry.Index].path;
            foreach(var other in entries)
                if(other.Id!=entry.Id && string.Equals(other.Path,path,StringComparison.OrdinalIgnoreCase))
                { feedback="Default is assigned to " + other.Label + ". Reset all or rebind that control first."; return false; }
            entry.Action.RemoveBindingOverride(entry.Index); Publish(); return true;
        }
        public static void ResetAll() { foreach(var action in actions.Values) action.RemoveAllBindingOverrides(); Publish(); }
        private static void Publish(bool notify=true)
        {
            for(int i=owners.Count-1;i>=0;i--)
            {
                if(!owners[i].TryGetTarget(out var owner)) { owners.RemoveAt(i); continue; }
                foreach(var source in actions.Values)
                    if(source.name==owner.name)
                        for(int j=0;j<source.bindings.Count;j++)
                        {
                            if(source.bindings[j].overridePath==null) owner.RemoveBindingOverride(j);
                            else owner.ApplyBindingOverride(j,source.bindings[j].overridePath);
                        }
            }
            if(notify) Changed?.Invoke();
        }
        public static ControlSettings Snapshot()
        {
            var result=new ControlSettings();
            foreach(var entry in entries) if(entry.Action.bindings[entry.Index].overridePath!=null)
                result.bindings.Add(new BindingRecord { id=entry.Id,path=entry.Path });
            return result;
        }
        public static bool Load(ControlSettings settings)
        {
            if(actions.Count==0) Initialize();
            if(settings==null || settings.version!=1 || settings.bindings==null) return false;
            var paths=new Dictionary<string,string>();
            foreach(var entry in entries) paths[entry.Id]=entry.Action.bindings[entry.Index].path;
            var seen=new HashSet<string>();
            foreach(var record in settings.bindings)
            {
                var entry=entries.Find(e=>e.Id==record?.id);
                if(entry==null || !entry.Rebindable || !Allowed(record.path) || !seen.Add(record.id)) return false;
                paths[record.id]=record.path;
            }
            var unique=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach(var path in paths.Values) if(!unique.Add(path)) return false;
            foreach(var action in actions.Values) action.RemoveAllBindingOverrides();
            foreach(var record in settings.bindings) { var entry=entries.Find(e=>e.Id==record.id); entry.Action.ApplyBindingOverride(entry.Index,record.path); }
            Publish(false); return true;
        }
    }
}


