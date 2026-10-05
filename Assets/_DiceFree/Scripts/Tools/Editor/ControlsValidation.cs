using System;
using System.IO;
using System.Linq;
using DiceFree.Foundation;
using DiceFree.Persistence;
using Unity.Pipeline.Commands;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace DiceFree.EditorTools
{
    public static class ControlsValidation
    {
        [CliCommand("dicefree.controls.start", "Enter isolated Play Mode for controls validation.", Tags=new[] { "tests" })]
        public static object Start()
        {
            PersistenceTestGuard.DisableForNextPlay();
            UnityEditor.EditorApplication.isPlaying=true;
            return new { started=true };
        }
        private static void Require(bool value,string message) { if(!value) throw new InvalidOperationException(message); }
        [CliCommand("dicefree.controls.validate", "Validate exact defaults, native action overrides, composites, conflicts, resets, settings and Escape policy.", Tags=new[] { "tests" })]
        public static object Run()
        {
            if(!UnityEditor.EditorApplication.isPlaying) throw new InvalidOperationException("Run dicefree.controls.start first; native action assertions require Play Mode.");
            string directory=Path.Combine(Path.GetTempPath(),"DiceFree-Controls-"+Guid.NewGuid().ToString("N"));
            var priorMode=InputSystem.settings.updateMode;
            InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsManually;
            var keyboard=InputSystem.AddDevice<Keyboard>();
            try
            {
                ControlBindings.Initialize();
                var expected=new[] { "<Keyboard>/w","<Keyboard>/s","<Keyboard>/a","<Keyboard>/d","<Mouse>/rightButton","<Keyboard>/f6","<Keyboard>/space","<Keyboard>/i","<Mouse>/leftButton","<Keyboard>/tab","<Keyboard>/x","<Keyboard>/r","<Mouse>/scroll/y","<Keyboard>/v","<Keyboard>/upArrow","<Keyboard>/downArrow","<Keyboard>/leftArrow","<Keyboard>/rightArrow","<Mouse>/middleButton","<Keyboard>/home","<Keyboard>/f","<Keyboard>/q","<Keyboard>/e","<Keyboard>/b" };
                Require(ControlBindings.Entries.Select(e=>e.Path).SequenceEqual(expected),"Default catalog changed.");
                Debug.Log("DICEFREE_CONTROLS_DEFAULTS_OK");
                var interact=ControlBindings.Entries.Single(e=>e.Id=="interact/0");
                using(var action=ControlBindings.Create("interact"))
                {
                    action.Enable();
                    Require(ControlBindings.Set(interact,"<Keyboard>/j",out _),"Simple rebind rejected.");
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.J)); InputSystem.Update();
                    Require(action.WasPressedThisFrame(),"Native rebind did not reach existing owner: "+action.name+" / "+action.bindings[0].effectivePath+" value="+action.ReadValue<float>()+"");
                }
                var up=ControlBindings.Entries.Single(e=>e.Id=="movement/1");
                using(var movement=ControlBindings.Create("movement"))
                {
                    movement.Enable(); Require(ControlBindings.Set(up,"<Keyboard>/t",out _),"Composite rebind rejected.");
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.T)); InputSystem.Update();
                    Require(movement.ReadValue<Vector2>()==Vector2.up,"Composite native action value incorrect.");
                }
                Require(!ControlBindings.Set(interact,"<Keyboard>/t",out var conflict) && conflict.Contains("Move Forward"),"Conflict was not explicit.");
                Require(!ControlBindings.Set(interact,"<Keyboard>/escape",out _) && !ControlBindings.Set(interact,"<Mouse>/delta",out _),"Invalid control accepted.");
                Require(ControlBindings.Reset(interact,out _) && interact.Path=="<Keyboard>/i" && up.Path=="<Keyboard>/t","Single reset failed.");
                var store=new LocalControlSettingsStore(directory); store.Save(ControlBindings.Snapshot());
                ControlBindings.ResetAll(); Require(up.Path=="<Keyboard>/w","Reset all failed.");
                Require(store.Load()!=null && up.Path=="<Keyboard>/t","Serialized restart seam failed.");
                store.Save(ControlBindings.Snapshot()); File.WriteAllText(store.Path,"broken");
                ControlBindings.ResetAll(); Require(store.Load()!=null && up.Path=="<Keyboard>/t","Backup recovery failed.");
                File.WriteAllText(store.Path+".bak","broken"); ControlBindings.ResetAll();
                Require(store.Load()==null && up.Path=="<Keyboard>/w","Corruption fallback failed.");
                Require(new LocalControlSettingsStore(directory+"-missing").Load()==null,"Missing file fallback failed.");
                Debug.Log("DICEFREE_CONTROLS_REBIND_COMPOSITE_CONFLICT_RESET_RELOAD_OK");
                Require(EscapeRouting.Choose(true,true,true,true,true)==EscapeRoute.CancelCapture,"Capture priority");
                Require(EscapeRouting.Choose(false,true,true,true,true)==EscapeRoute.CloseOptions,"Options priority");
                Require(EscapeRouting.Choose(false,false,true,true,true)==EscapeRoute.CloseInventory,"Inventory priority");
                Require(EscapeRouting.Choose(false,false,false,true,true)==EscapeRoute.CloseInteraction,"Interaction priority");
                Require(EscapeRouting.Choose(false,false,false,false,true)==EscapeRoute.ClearTarget,"Target priority");
                Require(EscapeRouting.Choose(false,false,false,false,false)==EscapeRoute.OpenOptions,"Menu fallback");
                ControlBindings.ResetAll();
                foreach(var pair in new[] { ("changeMode",Key.F6),("stop",Key.Space),("interact",Key.I),("cycle",Key.Tab),("attackSelected",Key.X),("respawn",Key.R),("inventory",Key.B) })
                {
                    using var action=ControlBindings.Create(pair.Item1); action.Enable();
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState()); InputSystem.Update();
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(pair.Item2)); InputSystem.Update();
                    Require(action.WasPressedThisFrame(),"Existing input action failed: "+pair.Item1);
                }
                using(var pointer=ControlBindings.Create("pointerDelta")) Require(pointer.bindings[0].path=="<Mouse>/delta","Mouse drag delta changed.");
                Debug.Log("DICEFREE_CONTROLS_ESCAPE_EXISTING_ACTIONS_OK");
                Debug.Log("DICEFREE_CONTROLS_OK");
                return new { success=true, finalMarker="DICEFREE_CONTROLS_OK", directory };
            }
            finally { InputSystem.settings.updateMode=priorMode; InputSystem.RemoveDevice(keyboard); ControlBindings.Initialize(); }
        }
    }
}

